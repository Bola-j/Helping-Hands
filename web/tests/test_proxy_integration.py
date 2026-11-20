import json
import shutil
import subprocess
import time

from proxy_fixture import ProxyCase, run
from proxy_recovery import replace_api


class ProxyIntegrationTests(ProxyCase):
    def test_frontend_and_api_routing(self):
        root_status, _, root = self.request("/")
        route_status, _, route = self.request("/how-it-works")
        self.assertEqual((root_status, route_status), (200, 200))
        self.assertIn(b"<html", root.lower())
        self.assertEqual(root, route)
        self.assertEqual(self.request("/_expo/missing-test-asset.js")[0], 404)
        status, headers, body = self.request("/api/v1/echo?value=hello%20world", b"{}", {
            "Cookie": "client=test", "X-CSRF-TOKEN": "fictional-token", "Content-Type": "application/json"})
        result = json.loads(body)
        self.assertEqual(status, 201)
        self.assertEqual(result, {"method": "POST", "uri": "/api/v1/echo?value=hello%20world",
                                  "cookie": "client=test", "csrf": "fictional-token"})
        self.assertIn("fixture=present", headers["Set-Cookie"])
        self.assertIn("Secure", headers["Set-Cookie"])
        status, _, body = self.request("/api/v1")
        self.assertEqual(status, 201)
        self.assertEqual(json.loads(body)["uri"], "/api/v1")

    def test_proxy_recovers_after_api_address_changes(self):
        old_ip = run("docker", "inspect", "--format", "{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}", self.api)
        self.assertTrue(old_ip)
        run("docker", "stop", self.api)
        started = time.monotonic()
        self.assertIn(self.request("/api/v1/echo")[0], (502, 504))
        self.assertLess(time.monotonic() - started, 10)
        old_ip, new_ip = replace_api(self, old_ip)
        self.assertNotEqual(old_ip, new_ip)
        deadline = time.monotonic() + 20
        while time.monotonic() < deadline:
            status, _, _ = self.request("/api/v1/echo")
            if status == 201:
                break
            time.sleep(0.5)
        self.assertEqual(status, 201)

    def test_upload_limit_and_upstream_certificate_verification(self):
        headers = {"Content-Type": "application/octet-stream"}
        self.assertEqual(self.request("/api/v1/echo", b"x" * (2 * 1024 * 1024), headers)[0], 201)
        self.assertEqual(self.request("/api/v1/echo", b"x" * (11 * 1024 * 1024), headers)[0], 413)
        for filename in ("dev.crt", "dev.key"):
            shutil.copyfile(self.path / "untrusted" / filename, self.path / "api-certificate" / filename)
        run("docker", "exec", self.api, "nginx", "-s", "reload")
        for attempt in range(20):
            status, _, _ = self.request("/api/v1/echo")
            if status == 502:
                break
            time.sleep(0.25)
        self.assertEqual(status, 502)
        logs = subprocess.run(["docker", "logs", self.web], capture_output=True, text=True, check=True).stderr
        self.assertIn("upstream SSL certificate verify error", logs)
