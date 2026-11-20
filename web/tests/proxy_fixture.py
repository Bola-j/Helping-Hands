"""Exercise the built web image against an isolated HTTPS API fixture.

Requires Docker and OpenSSL. Run from the repository root with
`python -m unittest discover -s web/tests -p test_proxy_integration.py`.
"""

import ssl
import subprocess
import tempfile
import time
import unittest
import urllib.error
import urllib.request
import uuid
from pathlib import Path

from proxy_setup import create_certificates, run, start_containers


class ProxyCase(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.suffix = uuid.uuid4().hex[:12]
        cls.network = f"hh-proxy-test-{cls.suffix}"
        cls.image = f"hh-proxy-test:{cls.suffix}"
        cls.api = f"hh-proxy-api-{cls.suffix}"
        cls.web = f"hh-proxy-web-{cls.suffix}"
        cls.replacement = f"hh-proxy-replacement-{cls.suffix}"
        cls.holder = f"hh-proxy-holder-{cls.suffix}"
        cls.temp = tempfile.TemporaryDirectory(prefix="hh-proxy-test-")
        cls.addClassCleanup(cls.cleanup)
        cls.path = Path(cls.temp.name)
        create_certificates(cls.path)
        (cls.path / "api.conf").write_text('''server {
    listen 8443 ssl;
    ssl_certificate /https/dev.crt;
    ssl_certificate_key /https/dev.key;
    client_max_body_size 12m;
    location / {
        default_type application/json;
        add_header Set-Cookie "fixture=present; Path=/; Secure; HttpOnly; SameSite=Strict" always;
        return 201 '{"method":"$request_method","uri":"$request_uri","cookie":"$http_cookie","csrf":"$http_x_csrf_token"}';
    }
}
''', encoding="utf-8")
        start_containers(cls.path, cls.image, cls.network, cls.api, cls.web)
        mapped = run("docker", "port", cls.web, "8443/tcp")
        cls.base = f"https://localhost:{mapped.rsplit(':', 1)[1]}"
        cls.context = ssl.create_default_context(cafile=str(cls.path / "trusted" / "dev.crt"))
        for attempt in range(40):
            try:
                if cls.request("/")[0] == 200 and cls.request("/api/v1/ready")[0] == 201:
                    break
                time.sleep(0.25)
            except (OSError, urllib.error.URLError):
                if attempt == 39:
                    raise
                time.sleep(0.25)
        else:
            raise RuntimeError("The isolated HTTPS proxy fixture did not become ready")

    @classmethod
    def cleanup(cls):
        for name in (cls.web, cls.api, cls.replacement, cls.holder):
            subprocess.run(["docker", "rm", "-f", name], capture_output=True, timeout=30)
        subprocess.run(["docker", "network", "rm", cls.network], capture_output=True, timeout=30)
        subprocess.run(["docker", "image", "rm", cls.image], capture_output=True, timeout=30)
        cls.temp.cleanup()

    @classmethod
    def request(cls, path, data=None, headers=None):
        request = urllib.request.Request(cls.base + path, data=data, headers=headers or {})
        try:
            with urllib.request.urlopen(request, context=cls.context, timeout=10) as response:
                return response.status, response.headers, response.read()
        except urllib.error.HTTPError as error:
            return error.code, error.headers, error.read()

