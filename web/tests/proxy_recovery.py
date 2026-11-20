from proxy_setup import run


def replace_api(fixture, old_ip):
    run("docker", "rm", "-f", fixture.api)
    run("docker", "run", "-d", "--name", fixture.holder, "--network", fixture.network, "nginx:alpine", "sleep", "60")
    run("docker", "run", "-d", "--name", fixture.replacement, "--network", fixture.network, "--network-alias", "api",
        "-v", f"{fixture.path / 'api-certificate'}:/https:ro",
        "-v", f"{fixture.path / 'api.conf'}:/etc/nginx/conf.d/default.conf:ro", "nginx:alpine")
    new_ip = run("docker", "inspect", "--format", "{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}", fixture.replacement)
    type(fixture).api = fixture.replacement
    return old_ip, new_ip
