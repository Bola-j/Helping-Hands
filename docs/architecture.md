# Proposed monolithic architecture and technology compatibility

Status: supporting design for the Phase 1 [SRS](SRS.md). The repository contains API workflow code, PostgreSQL/EF migrations and local container configuration; see the [capability assessment](SRS.md#appendix-phase-1-implementation-baseline-and-verification) for evidence and gaps. The Expo web client and complete end-to-end workflow remain planned.

## System design

One ASP.NET Core Web API serves an eventual React Native Web client over HTTPS/JSON. A small monolith can organize code by features as needed: participant approvals; requests and referrals; evidence and review; authentication/authorization and audit. Introduce folders and services only with actual behavior. PostgreSQL is the future persistent system of record; protected file storage holds evidence bytes and PostgreSQL holds metadata. See [models](models.md) and [database design](database-design.md).

The API v1 plan selects ASP.NET Core Identity and secure same-origin cookies. Server-side authorization checks role and involvement on requests, referrals and evidence. Phase 1 beneficiaries submit their own requests; representatives need active charity membership; Coordinator actions are also available to Administrator. The P1-10 candidate passes its full local software suite; PR and post-merge verification remain pending. Use DTO validation and consistent problem responses; do not expose sensitive details in errors or operational logs. The API uses cookie and antiforgery mechanisms in source; browser integration and deployment behavior require verification. Decisions and their actors belong in business history; request diagnostics belong in operational logs without beneficiary data.

Transactions serialize assignment, accepted outcomes, releases and their history per request, preserving remaining versus unassigned need; see the explicit lock protocol in the data design. Store evidence outside the public web root and mediate downloads through authorized API endpoints. Because file storage and SQL do not share a transaction, use staged files and reconcile or remove orphans after a failed metadata operation. File size/type policies, malware checks, retention and recovery must be decided before real use. Back up and restore-test PostgreSQL and protected files together; configure persistent volumes when containerized. The API liveness endpoint checks the process; PostgreSQL readiness has a separate endpoint and must be verified against a running service.

Provide connection configuration and secrets through environment-specific secure configuration; never commit secrets. The repository configures EF Core with Npgsql and contains a migration; review and apply version-controlled migrations using isolated databases; apply them as a controlled deployment step, not automatically on startup. See the [API v1 plan](api-v1-plan.md) for local setup and migration workflow.

Deploy one API container and a separate PostgreSQL instance or container only when required. Use a secured reverse proxy with HTTPS, environment-based configuration, basic structured logs and health monitoring, and tested backups. Database locks, evidence upload size, storage, and manual reviews may limit throughput; measure before scaling. No message broker, distributed cache, microservices, or cloud service is required for this proposal. An outage should yield a clear error and preserve existing committed state; do not claim offline completion or success after a failed write.

## React Native Web and PWA feasibility

[Expo's web documentation](https://docs.expo.dev/workflow/web/) describes React Native for Web and web-specific dependencies and export; Expo is the selected Phase 1 direction; the web application is not yet implemented. [Expo's PWA guide](https://docs.expo.dev/guides/progressive-web-apps/) documents separate web manifest and service-worker considerations. Neither React Native nor the scaffold automatically provides installability, offline business operations, or push notifications. The eventual web app needs a manifest, appropriate icons, HTTPS or a development secure context, and platform-specific installation testing. A service worker, if chosen, may cache non-sensitive static assets only; do not cache beneficiary data or evidence. Offline access to business records and offline writes are out of scope; show a connection error and do not claim an unconfirmed action succeeded. Push is not required.

### Proposed browser/device test matrix — not a support promise

| Environment | Responsive interaction | Install behavior to test |
| --- | --- | --- |
| Current stable Chrome, Android phone and tablet | Touch, forms, upload, accessibility | Browser installation UI depends on manifest, platform and browser criteria; test a real device. |
| Current stable Safari, iPhone and iPad | Touch, forms, upload, accessibility | Add to Home Screen uses platform UI; behavior varies with iOS/iPadOS version. |
| Current stable Chrome and Edge, Windows desktop | Keyboard, forms, zoom, upload | Browser-provided app installation where supported. |
| Current stable Firefox, desktop | Keyboard, forms, zoom, upload | Usable website; do not promise native PWA install. |

Exact OS/browser versions, minimums, and supported-device coverage are decisions for the team after device testing. [Chrome's installability guidance](https://developer.chrome.com/docs/lighthouse/pwa/installable-manifest) explains Chrome-specific criteria and differing browser behavior. [WebKit's iOS Home Screen article](https://webkit.org/blog/13878/) and [Safari 26 notes](https://webkit.org/blog/17333/webkit-features-in-safari-26-0/) show that add-to-home-screen behavior changes across versions. Installation and prompt availability must be checked rather than assumed; compatibility in this document is not a test result.

## Open decisions

Participant identity and review procedures; exact target browser/OS versions; evidence formats, limits, private storage, retention and correction; hosting and restore procedures; PWA service-worker strategy; notification needs; and PostgreSQL version are not fixed. Proposed schema lengths and status codes require review against user workflows. Stakeholder and applicable local operational review remain prerequisites to a real launch.
