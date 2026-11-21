# Local service restart and recovery verification

**Recorded 21 November 2026 — P1-56 local evidence.** This is an isolated fictional-data exercise, not production certification. The disposable Compose project bound service ports to loopback, used a unique PostgreSQL volume and a unique private SeaweedFS volume, and ran a Development API container. Credentials and certificates were kept in the ignored `.local/p156` directory and are not included here.

## Exercise and observed results

| Scenario | Observed result |
| --- | --- |
| Start PostgreSQL, SeaweedFS, Mailpit and API; query API health | `/health` returned `ok`; `/ready` returned `ready`. PostgreSQL and Mailpit Compose health checks were healthy. |
| Exercise public fictional charity-to-outcome API flow against isolated PostgreSQL and SeaweedFS | `FictionalCharityToOutcome_UsesPrivateEvidenceAndRetainsHistory` passed 1/1 on the P1-55 worktree. The resulting request and private attachment were then read through the running P1-56 API. |
| Verify private evidence identity before restart | Retrieved attachment SHA-256: `F309ED73692404FAD110332AF75FA5250C4E92E1C0094208927621C1970E9CD7`. |
| Stop and restart API, PostgreSQL, SeaweedFS and Mailpit; wait for dependencies and rerun storage initialization | Compose health checks returned healthy. Authenticated request and attachment reads succeeded; evidence content hash remained `F309ED73692404FAD110332AF75FA5250C4E92E1C0094208927621C1970E9CD7`; `/health` returned `ok`, `/ready` returned `ready`. |
| Stop PostgreSQL while API remained running | `/health` remained `ok`; `/ready` returned HTTP 503. A request read failed with a generic HTTP 500. After PostgreSQL restart, readiness returned `ready` and the request became readable again. |
| Stop SeaweedFS while API remained running | A private evidence download did not return an HTTP status within the 45-second client timeout (`TaskCanceledException`); no response file was produced, so no false download success was reported. API liveness/readiness remained `ok`/`ready`. After storage recovery and initialization, download returned the same evidence hash. |
| Stop Mailpit while API remained running | SMTP connection attempt failed; API liveness/readiness remained `ok`/`ready`. After Mailpit recovery, the reminder worker retried and Mailpit accepted the email. This proves local relay acceptance only, not delivery to a real mailbox. |
| Reminder retry | The first fictional reminder attempt failed because the seeded charity contact was deliberately non-email text. After setting that isolated record to a `.test` email and restarting the worker, persisted state moved from `Failed` (attempt 1) to `Delivered` (attempt 2), with two messages visible in Mailpit: a relay probe and the reminder. |

## Limits

- This verifies restart persistence using the same named Docker volumes. It does not test restoring PostgreSQL from a backup, restoring evidence from an independent backup, point-in-time recovery, loss/corruption of a volume, or cross-volume consistency after partial restore.
- Mailpit has no persistent volume in this setup; its message history was empty after the full stack restart. Persisted reminder state and a retry were observed, not mailbox-history recovery or exactly-once SMTP delivery.
- When SeaweedFS was stopped, the client timed out before receiving an HTTP response. The API did not report a successful download, but bounded failure latency/status mapping was not established by this exercise.
- PostgreSQL unavailability produced a generic HTTP 500 on the request read; a purpose-built dependency error response remains unverified.
- The original configured MinIO image was unavailable in the earlier baseline assessment. This exercise used SeaweedFS, the repository's S3-compatible local service. It does not certify compatibility with a production object store.
- The exercise used fictional records and local self-signed HTTPS only. No real people, evidence, external SMTP, production credentials or shared data were used.

P1-56 remains open until its issue acceptance, branch/PR CI and required review are reconciled. P1-46's paired backup/restore acceptance is not satisfied by a restart-only exercise.
