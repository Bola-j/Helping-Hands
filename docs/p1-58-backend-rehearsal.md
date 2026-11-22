# P1-58 — Phase 1 backend API rehearsal

**Observed:** 22 November 2026 (Africa/Cairo)

**Result:** Backend API rehearsal completed with fictional data. This is API-client and isolated-service evidence; it is not browser or full-app evaluation evidence.

## Isolated setup

The rehearsal used a dedicated Docker Compose project, `helping-hands-p158`, with its own PostgreSQL 17 and SeaweedFS volumes, a private S3-compatible bucket, and Mailpit. The API was rebuilt from `main` at `5657484`. Host ports were API `18443`, PostgreSQL `15432`, S3 `19000`, SMTP `11025`, and Mailpit UI `18025`. The local environment file and HTTPS certificate were under ignored `.local/p158/`; generated credentials and evidence were not committed. No shared or production data was used.

The reproducible startup and migration commands are:

```powershell
docker compose --env-file .local/p158/.env -p helping-hands-p158 up -d --build api postgres storage storage-init mailpit
./scripts/run-api.ps1 -EnvironmentFile .local/p158/.env -Migrate
./scripts/run-api.ps1 -EnvironmentFile .local/p158/.env -SeedDemo
docker compose --env-file .local/p158/.env -p helping-hands-p158 up -d api
```

Set unique loopback ports and credentials in the ignored environment file, export the local HTTPS development certificate to its configured path, and keep the S3 bucket exclusive to this test database. The launcher restores process environment after migration and seeding. Obtain an antiforgery cookie and request token per actor session, preserve cookies across login, and fetch a fresh token after authentication. The API-client requests used `/openapi/v1.json` and the routes in the [API walkthrough](api-demo-walkthrough.md).

## Fictional workflow exercised

1. Liveness and readiness returned `200`; the empty PostgreSQL volume accepted the full EF Core migration set. The seeded Administrator account was used for Coordinator-authorized steps.
2. Created and published a service version with two distinct standards and components. Registered two fictional charities, approved both applications, mapped both standards to each charity, and recorded current-day capacity.
3. Registered a fictional beneficiary and submitted a request with two distinct needs, quantity `2` each. The Coordinator accepted both needs.
4. Allocated the food need as two referrals of quantity `1`, split between the charities. Allocated the hygiene need as quantity `2` to the first charity. PostgreSQL progress showed each need's assigned and accepted totals separately.
5. Each of the three referrals received a fictional reported outcome, a private PDF attachment in S3-compatible storage, submission, and Coordinator acceptance. Progress showed the correct per-referral and per-need quantities. Explicit closure returned `204`; the request became `Completed`, and request history contained one `RequestClosed` entry.
6. A second small request used a Coordinator-role session to verify a stale-capacity rejection, refresh capacity, then allocate, submit evidence, accept the outcome, and close the request.

## Observed checks

| Scenario | Observed result |
| --- | --- |
| Charity and beneficiary registration; Coordinator approval; published service/standards; current capacity | `201` registrations/catalog creates; `200` approval, publish, service and capacity updates; request creation `201`; both need decisions `200` |
| Split allocation | Two food referral legs and one hygiene leg persisted; each allocation returned `201` |
| Stale request version | Allocation returned `409` with `StaleVersion`; no state change |
| Zero capacity on one leg of a split | Allocation returned `409` with `InsufficientDailyCapacity`; the transaction created no partial referral, and the unchanged request version could be retried after capacity recovery |
| Stale/missing Cairo-day capacity | Capacity read returned `isStale: true`; allocation returned `409` with `StaleCapacity`. Updating today's capacity returned `200`, after which allocation returned `201` |
| Invalid transition | Closure before accepted outcomes returned `409`; closure after accepted outcomes for every required need returned `204` |
| Role and private evidence | Representative review attempt returned `403`; anonymous evidence read returned `401`; a different charity's evidence read returned `404` |
| PostgreSQL unavailable and restart | `/ready` returned `503` while only the isolated PostgreSQL service was stopped. After it restarted healthy, the completed request and both needs remained queryable |
| Private evidence persistence | Restarted the isolated SeaweedFS service; all three evidence downloads succeeded and matched the source PDF SHA-256. A duplicate identical PDF was also attached to the first food outcome during a client retry; it did not affect review or closure and remains confined to the disposable rehearsal volume |
| Email catcher | Mailpit captured one fictional daily-capacity reminder; the application recorded the intent as `Delivered` with a successful final attempt. A local runner invocation used `forceRunNow` to avoid waiting for 08:00 Cairo. Three earlier attempts failed because the temporary harness lacked or mismatched Mailpit credentials; after aligning the isolated Mailpit/API credentials, the retry succeeded |

The initial PowerShell multipart upload used a generic MIME type and returned `400`; resending the same fictional PDF as `application/pdf` returned `201`. The first request creation attempt also used the same standard version twice, which the API correctly rejected; the final fixture used two distinct standard versions. These were rehearsal-client/setup corrections, not reported product defects.

## Existing automated coverage and limits

P1-44's automated regression remains in [issue #44](https://github.com/Bola-j/Helping-Hands/issues/44) and [PR #134](https://github.com/Bola-j/Helping-Hands/pull/134): registration, approval, role behavior, Cairo capacity freeze, catalog versions, matching, split allocation, closure, rollback, evidence access and revoked-role concealment. PR #134's recorded corrected-head CI passed 68 quick, 107 PostgreSQL integration and 4 private-evidence workflow tests, plus web checks and GitGuardian. This rehearsal did not duplicate those test cases.

No browser session or web-client behavior was tested. This does not complete [P1-47](https://github.com/Bola-j/Helping-Hands/issues/47) or [P1-48](https://github.com/Bola-j/Helping-Hands/issues/48). The dependency check exercised PostgreSQL unavailability and service restarts, not database backup restoration or a SeaweedFS outage. Mailpit acceptance proves local SMTP relay acceptance only, not delivery by an external mail provider. The reminder's force-run used the application runner locally; it was not exposed as a public API route. No new application test suite was added or run for this documentation/evidence ticket.
