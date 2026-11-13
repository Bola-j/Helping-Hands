#!/usr/bin/env bash
set -euo pipefail
umask 077
p1_access=$(openssl rand -hex 12)
p1_secret=$(openssl rand -hex 24)
echo "::add-mask::$p1_access"
echo "::add-mask::$p1_secret"
printf '{"identities":[{"name":"ci","credentials":[{"accessKey":"%s","secretKey":"%s"}],"actions":["Admin","Read","Write","List","Tagging"]}]}' "$p1_access" "$p1_secret" > "$RUNNER_TEMP/p1-s3.json"
docker run -d --name p1-ci-storage -p 127.0.0.1:8333:8333 -v "$RUNNER_TEMP/p1-s3.json:/s3.json:ro" -v /data -w /data chrislusf/seaweedfs:3.90 server -dir=/data -s3 -s3.config=/s3.json -volume.max=30 >/dev/null
{
  echo 'P1_TEST_S3_URL=http://127.0.0.1:8333'
  echo 'P1_TEST_S3_BUCKET=helping-hands-test-ci-evidence'
  printf 'AWS_ACCESS_KEY_ID=%s\n' "$p1_access"
  printf 'AWS_SECRET_ACCESS_KEY=%s\n' "$p1_secret"
  echo 'AWS_DEFAULT_REGION=us-east-1'
  printf 'P1_TEST_S3_ACCESS_KEY=%s\n' "$p1_access"
  printf 'P1_TEST_S3_SECRET_KEY=%s\n' "$p1_secret"
} >> "$GITHUB_ENV"
