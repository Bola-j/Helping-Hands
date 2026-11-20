import shutil
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def run(*args, timeout=120):
    return subprocess.run(args, cwd=ROOT, check=True, capture_output=True, text=True, timeout=timeout).stdout.strip()


def create_certificates(path):
    for directory in ("trusted", "untrusted"):
        target = path / directory
        target.mkdir()
        run("openssl", "req", "-x509", "-newkey", "rsa:2048", "-nodes", "-days", "1",
            "-subj", "/CN=localhost", "-addext", "subjectAltName=DNS:localhost",
            "-keyout", str(target / "dev.key"), "-out", str(target / "dev.crt"))
    shutil.copytree(path / "trusted", path / "api-certificate")


def start_containers(path, image, network, api, web):
    run("docker", "build", "-f", "web/Dockerfile", "-t", image, ".", timeout=600)
    run("docker", "network", "create", network)
    run("docker", "run", "-d", "--name", api, "--network", network, "--network-alias", "api",
        "-v", f"{path / 'api-certificate'}:/https:ro", "-v", f"{path / 'api.conf'}:/etc/nginx/conf.d/default.conf:ro", "nginx:alpine")
    run("docker", "run", "-d", "--name", web, "--network", network,
        "-p", "127.0.0.1::8443", "-v", f"{path / 'trusted'}:/https:ro", image)
