# P1-01 and P1-02: assessment and requirements reconciliation plans

Status: **assessment reviewed and completed; reconciliation and diagrams in progress; Scrum Master review accepted**. These plans refine [P1-01](https://github.com/Bola-j/Helping-Hands/issues/1) and [P1-02](https://github.com/Bola-j/Helping-Hands/issues/2). Findings below are preliminary inspection, not completion evidence. P1-02 discovery can begin now; its final reconciliation depends on P1-01. See the [Phase 1 roadmap](api-demo-scrum-roadmap.md) for ticket dependencies and responsibilities.

## P1-01 — assess the current implementation

**Responsible:** Bola; request database/architecture input from Ziad and web input from hussien and ali. Preserve all existing changes, including untracked files. Do not stage, reset, remove, or apply migrations to shared data as part of the assessment.

### Work plan

- [ ] Record the current Git status, solution and project layout, SDK and configuration requirements. Inventory the existing uncommitted work without assuming its authorship or discarding it.
- [ ] Map existing endpoints, identities and access rules, entities, migrations, private storage, Docker services and tests against every Phase 1 capability in the roadmap. Record the source and implementation status for each capability.
- [ ] Run the existing solution build and relevant test suite; record commands, results and failures. Inspect whether tests use real dependencies or substitutes. Report a route or passing build as evidence only of what it actually proves.
- [ ] Inspect the existing migration, current model and Compose configuration. If the Docker engine is available, use isolated fictional-data services to check migration from empty, startup/readiness and representative API, PostgreSQL and private-storage interactions. Do not overwrite or reset existing volumes. Record unavailable checks as blocked, and propose a safe recovery/upgrade test for the later infrastructure ticket.
- [ ] Produce a capability matrix with columns: requirement or ticket, observed code, check and result, reuse or gap, dependency, and next ticket/owner. Include unavailable or failed checks and open questions; link the matrix and actual evidence from P1-01.
- [ ] Review the matrix with the team and use it as input to P1-02 and sprint planning. P1-01 is closed after the Scrum Master accepted the review; runtime blockers remain visible.

**Preliminary starting points:** the repository contains a .NET 10 API, an EF migration, tests and Compose definitions for PostgreSQL and S3-compatible storage. Source inspection found no frontend project or SMTP container. The Docker CLI could not reach the engine during the planning inspection; live service behavior is therefore unverified. Recheck these observations during execution.

## P1-02 — reconcile Phase 1 requirements and design

**Responsible:** Bola; Ziad reviews database and architecture consequences, and hussien and ali review client implications. Discovery can run in parallel with P1-01; use its verified matrix before finalizing changes.

### Work plan

- [ ] Trace the latest Phase 1 goal and each user journey through BRD, PRD, SRS, API plan, database design, relevant diagrams and the demo roadmap. Build a list of conflicts and affected tickets; distinguish confirmed decisions, proposed details and unresolved questions.
- [ ] Reconcile charity approval explicitly: the current Phase 1 roadmap calls for a coordinator, while the earlier BRD, SRS, API plan and database design described an administrator. The working documents now record the coordinator decision and distinct roles; review those changes and the authorization implications with the team.
- [ ] Specify self-only beneficiary intake as the Phase 1 subset while preserving third-party intake as later scope. Define the roles, involvement checks and authorized views for each demo journey.
- [ ] Reconcile multiple needs and partial/split referrals with the existing child-request model, history and completion rules. Specify quantities, capacity and acceptance invariants without claiming a new schema exists. Apply the interim P1-33 exact-version rule for outstanding requests after standards change; any future conversion policy requires review.
- [ ] Add testable requirements for service categories and structured versioned standards; per-service Cairo-day capacity, stale versus zero and freeze; one daily reminder per charity, retries and ambiguous SMTP results; evidence access, coordinator review and explicit closure. Trace each to API, database, web and verification tickets.
- [ ] Update the affected source documents and diagrams after the decisions are recorded. Keep full-product capabilities and Phase 2 exclusions clearly scoped rather than deleting future requirements. Describe planned and verified functionality separately using P1-01 evidence.
- [ ] Review the role matrix, workflow examples and cross-document links with the team; record approvers, date, unresolved decisions and affected backlog items. The Scrum Master accepted the review; P1-02 remains open for diagram reconciliation.

### Acceptance examples to refine

- A coordinator can approve or reject a charity under the agreed role model; pending or rejected charities cannot receive referrals, and unrelated users are denied.
- A beneficiary submits their own request with multiple needs; quantities, versions and split portions retain their meaning and cannot be counted twice.
- A zero capacity update is current but offers no units; a service without an update on the current Cairo day is stale and frozen for new assignments.
- One charity with multiple stale services receives one daily reminder intent; retries and ambiguous SMTP outcomes are recorded without claiming confirmed delivery.
- Accepted reports contribute once to fulfillment; explicit coordinator closure is available only when every required need is fulfilled and accepted.

These are proposed acceptance scenarios to refine and test, not claims of implementation or passed tests.
