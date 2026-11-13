# Phase 1 Scrum roadmap and ticket backlog

Status: **planned, pending team refinement and verification**. This roadmap implements the latest Phase 1 decision and replaces the earlier API-only demo sequence. A ticket here is a forecast, not evidence of completion. The assessed API and local infrastructure baseline has been merged; later work remains subject to each ticket's acceptance checks. The [Phase 1 GitHub Project](https://github.com/users/Bola-j/projects/4) contains the 57 linked Issues; its [Scrum board](https://github.com/users/Bola-j/projects/4/views/2) and Sprint 1–3 views make the forecast visible. Issue links identify tracking items rather than completed work.

**P1-09 progress (13 November 2026):** documentation was aligned in merged PR #83. The separate beneficiary and charity signup implementation and PostgreSQL tests are now in PR #82; its required checks and post-merge run remain outstanding. P1-09 remains open until those checks pass on the final candidate and merged `main`. The current checkout's local full suite passed 57/57 software tests against isolated fictional PostgreSQL and evidence-storage dependencies; this is pre-merge evidence, not the GitHub merge gate.

## Phase 1 goal

Using fictional data, demonstrate a React Native Web application with beneficiary and charity registration/login; coordinator charity approval; coordinator-managed services and versioned standard units with structured contents; charity daily capacity, an in-app task and email reminder; beneficiary mixed needs; coordinator matching and split referrals; and charity evidence followed by coordinator review, progress and closure. The backend is ASP.NET Core/.NET 10, PostgreSQL via reviewed EF migrations, with private S3-compatible evidence storage. Expo web, API, database, storage and a local email catcher must work together.

Charity capacity is **available from now** in direct platform-standard units. An update replaces availability; existing commitments remain in history, and later assignments reduce the balance. A service/standard version without an update on the current Africa/Cairo calendar day is frozen for new referrals; other updated services are not frozen. The 08:00 Cairo reminder is one per charity with stale services, with recorded outcomes and safe retries. SMTP acceptance is not proof of mailbox delivery; after an ambiguous send, avoid claiming exactly-once external delivery.

The coordinator creates and versions service categories and unit standards, each standard having structured component names, quantities and units; historical records retain the category and standard versions they used. New requests use current standards; historical records retain their version. The Scrum Master selected an interim P1-33 rule: an outstanding old-version need may match only the exact recorded versions until the policy is reviewed; no silent conversion. If no eligible exact-version capacity exists, the need stays open. A reported outcome and attachment require coordinator review; acceptance is not independent proof of delivery. Complete a request only when all required needs are fulfilled and accepted.

**Phase 2:** third-party requests, email address verification, and calculating standard-unit capacity from ingredient/component inventory. Real transfers, operational deployment and production SMTP are outside this fictional-data demo; SMTP is configurable for later hosting.

## Scrum cadence and ownership

Three one-week forecast increments follow. The team selects a feasible Sprint Backlog at planning and may replan work; dates and content are not a completion commitment. Forecast: Sprint 1 **8–14 Nov** (foundation, accounts and standards); Sprint 2 **15–21 Nov** (capacity, reminders and requests); Sprint 3 **22–27 Nov** (matching/referrals, evidence, integration and rehearsal). The announced no-lab period is **21–26 Nov**; Phase 1 discussions are **28 Nov–3 Dec**. No lab attendance or stakeholder review is assumed.

| Specialty | Proposed work owner | Supporting work |
| --- | --- | --- |
| Backend/API | Bola Ghaly | API contracts, auth, workflow, SMTP and tests |
| Database/architecture | Ziad Walid | schema, migrations, concurrency, integrity and recovery |
| Web UI | hussien okely | accounts, charity capacity, beneficiary intake/progress and web tests |
| Web UI | ali fathy | coordinator workflows, charity reporting and web tests |

Owners are proposed by specialty; GitHub assignees require confirmed usernames and access. All four must contribute actual code under their own GitHub identities, use feature branches and link PRs/commits to tickets. Do not infer contribution from an assignment or fabricate authorship. The team agrees who facilitates Scrum and who represents product priorities. For Phase 2, each student must implement at least two features, each on its own branch and merged only when finished; Phase 1 contribution evidence does not automatically satisfy that later requirement.

At planning, select tickets and agree a sprint goal; track a single ordered backlog using **Backlog → Ready → In progress → In review → Done**, plus blocked status/reason. Daily coordination records progress, next work and blockers. Reviews demonstrate finished increments and capture actual feedback; retrospectives record one owner and improvement. Keep meeting notes and milestones in the repository. Each ticket is done only with implemented acceptance criteria, relevant automated checks including access/failure cases, reviewed migrations when applicable, documentation, a linked authentic code change, and observable evidence. Exceptions and unrun checks stay visible.

The [backend, database, containers and CI delivery breakdown](phase1-backend-delivery-breakdown.md) groups the existing tickets by dependency for the 12 November target. It is a forecast and records the separate full-scope and fallback outcomes.

## Workstreams

Each issue has one primary workstream and one proposed responsible person. Workstream views identify ownership; prerequisites connect the work across views. Collaborators can review and contribute without sharing accountability. Sprint and status remain planning and progress fields.

| Workstream | GitHub view | Tickets |
| --- | --- | --- |
| API and backend | [API view](https://github.com/users/Bola-j/projects/4/views/6) | P1-08, P1-09, P1-10, P1-14, P1-16, P1-19, P1-20, P1-21, P1-26, P1-28, P1-30, P1-32, P1-34, P1-37, P1-39, P1-40, P1-42, P1-44, P1-51, P1-52, P1-54, P1-55 |
| Infrastructure, database and local integrations | [Infrastructure view](https://github.com/users/Bola-j/projects/4/views/7) | P1-05, P1-06, P1-12, P1-13, P1-18, P1-24, P1-25, P1-27, P1-35, P1-46, P1-49, P1-53, P1-56 |
| Web frontend | [Web view](https://github.com/users/Bola-j/projects/4/views/8) | P1-07, P1-11, P1-15, P1-17, P1-22, P1-23, P1-29, P1-31, P1-36, P1-38, P1-41, P1-43, P1-45, P1-50, P1-57 |
| Shared planning and integration | [Shared view](https://github.com/users/Bola-j/projects/4/views/9) | P1-01, P1-02, P1-03, P1-04, P1-33, P1-47, P1-48 |

Phase 1 infrastructure covers local Docker, PostgreSQL, S3 and the email catcher. API CI is pulled forward into Phase 1 and remains a prerequisite for the separate [Phase 2 deployment project](https://github.com/users/Bola-j/projects/5): [hosting and secrets](https://github.com/Bola-j/Helping-Hands/issues/49), [API CI](https://github.com/Bola-j/Helping-Hands/issues/50), [web CI](https://github.com/Bola-j/Helping-Hands/issues/61), [API deployment](https://github.com/Bola-j/Helping-Hands/issues/51), [web deployment](https://github.com/Bola-j/Helping-Hands/issues/62), and [live release verification](https://github.com/Bola-j/Helping-Hands/issues/63). Web deployment depends on hosting, web CI and API deployment; release verification depends on both deployments. These are plans, not completed CI or deployment.

The [P1-01 assessment and P1-02 reconciliation plans](phase1-assessment-and-reconciliation-plan.md) contain the checklists. The [capability assessment](SRS.md#appendix-phase-1-implementation-baseline-and-verification) records actual checks and gaps; the [requirements trace](SRS.md#appendix-phase-1-requirements-trace-and-review) records working decisions and review status. P1-02 discovery can start in parallel, but its final reconciliation depends on P1-01.

## Backend milestones

The 12 November target is a high-risk forecast. GitHub milestones group the existing backend issues without changing their completion state; API CI is included now and remains a dependency for later deployment.

1. [Foundation](https://github.com/Bola-j/Helping-Hands/milestone/1): requirements prerequisite, isolated PostgreSQL, local API/private storage and API CI. See the [first milestone plan](phase1-foundation-milestone.md).
2. [Identity and charity approval](https://github.com/Bola-j/Helping-Hands/milestone/2): sessions, roles and coordinator decisions.
3. [Catalog, needs and capacity schema](https://github.com/Bola-j/Helping-Hands/milestone/3): reviewed migrations, standards and locking.
4. [Intake, referrals and outcomes](https://github.com/Bola-j/Helping-Hands/milestone/4): capacity, matching, split allocations, evidence and closure.
5. [Reminders, verification and rehearsal](https://github.com/Bola-j/Helping-Hands/milestone/5): email, integration tests, recovery and demonstration evidence.

## Ticket format and dependencies

Each row is a separate issue with one proposed responsible person and one workstream. `Depends on` lists prerequisite ticket IDs; `—` means no ticket prerequisite. Dependencies are also linked in GitHub. Owners are proposals, not GitHub assignments; others may collaborate or review. Checked rows indicate issues closed on GitHub; P1-03 was closed at the Scrum Master's direction with remaining access handled separately. Open rows remain unchecked. Estimates are intentionally omitted until the team refines them.

The existing API source already configures secure cookies, antiforgery and identity routes. These tickets assess and complete the relevant behavior; the full expiry and authorization lifecycle still needs verification.

### Sprint 1 tickets

| Ticket | Proposed responsible | Depends on | Acceptance / observable evidence |
| --- | --- | --- | --- |
| [x] **[P1-01](https://github.com/Bola-j/Helping-Hands/issues/1)** — Assess the existing implementation and Phase 1 gaps | Bola | — | Inventory current API, migrations, tests, Docker and uncommitted work; document verified reuse, gaps and blockers without marking planned work complete. |
| [x] **[P1-02](https://github.com/Bola-j/Helping-Hands/issues/2)** — Align the requirements and role decisions | Bola | P1-01 | Update requirements and design for coordinator charity approval, self-only beneficiary requests, mixed needs, capacity and versioned standards; identify Phase 2 exclusions. |
| [x] **[P1-03](https://github.com/Bola-j/Helping-Hands/issues/3)** — Set up Phase 1 GitHub Project and team access | Bola | — | Create the Project, backlog views and status workflow; obtain Project permissions and invite teammates only after confirming their real GitHub usernames. |
| [ ] **[P1-04](https://github.com/Bola-j/Helping-Hands/issues/4)** — Establish feature branches and contribution tracking | Bola | P1-03 | Each teammate has an independently reviewable code task, uses their own account for commits, and links feature branches and PRs to Issues; do not fabricate authorship. |
| [x] **[P1-05](https://github.com/Bola-j/Helping-Hands/issues/5)** — Validate PostgreSQL baseline and migration process | Ziad | P1-01, P1-02 | Run the existing migration on an empty isolated PostgreSQL database; document upgrade, reviewed SQL, backup and recovery approach; preserve applied migrations. |
| [x] **[P1-06](https://github.com/Bola-j/Helping-Hands/issues/6)** — Make local dependencies reproducible | Ziad | P1-01 | Configure Docker API, PostgreSQL and private S3-compatible storage with persistent volumes, environment-provided secrets, service names and distinct liveness/readiness. |
| [ ] **[P1-07](https://github.com/Bola-j/Helping-Hands/issues/7)** — Create the Expo React Native Web application | hussien | P1-01 | Create a runnable Expo web frontend with navigation and a documented local entry point. Container and proxy setup is tracked separately in P1-49. |
| [ ] **[P1-08](https://github.com/Bola-j/Helping-Hands/issues/8)** — Provide secure API session and antiforgery contracts | Bola | P1-05, P1-06 | Assess and verify the existing cookie and antiforgery contract, session expiry and safe API authentication errors; login and logout behavior belongs to P1-10. |
| [ ] **[P1-09](https://github.com/Bola-j/Helping-Hands/issues/9)** — Complete beneficiary and charity account registration | Bola | P1-05, P1-08 | Replace shared signup with separate beneficiary and charity routes. Create a `Beneficiary` account and linked `BEN` profile, or a `RecipientRepresentative` account and pending inactive charity application with `REC` reference; enforce global email uniqueness, validation, antiforgery and transaction rollback. Do not add email verification or third-party requester signup. |
| [ ] **[P1-10](https://github.com/Bola-j/Helping-Hands/issues/10)** — Complete login, logout and role-based access | Bola | P1-08, P1-09 | Complete login/logout and role-and-involvement authorization for the seeded `Beneficiary`, `RecipientRepresentative` and coordinator roles; reject unauthorized and unrelated access. Charity approval remains P1-16. |
| [ ] **[P1-11](https://github.com/Bola-j/Helping-Hands/issues/11)** — Build registration and login screens | hussien | P1-07, P1-50, P1-09, P1-10 | Build distinct beneficiary and charity signup forms against the P1-09 contracts, including validation and pending charity confirmation; add login/logout and session error states against P1-10, using fictional accounts. |
| [ ] **[P1-12](https://github.com/Bola-j/Helping-Hands/issues/12)** — Model service catalog and versioned unit standards | Ziad | P1-02, P1-05 | Add reviewed EF migrations for coordinator-managed, versioned service categories, standard units and structured component names, quantities and measurement units; preserve UUID keys and history. |
| [ ] **[P1-13](https://github.com/Bola-j/Helping-Hands/issues/13)** — Preserve standard versions in the database | Ziad | P1-12 | Use reviewed migrations and integrity rules to retain immutable category and standard versions referenced by historical records. API version behavior is P1-51. |
| [ ] **[P1-14](https://github.com/Bola-j/Helping-Hands/issues/14)** — Create coordinator service and standard APIs | Bola | P1-10, P1-12, P1-13, P1-51 | Provide authorized create, view and edit/version endpoints for services, standards and components; validate quantities and return stable identifiers. |
| [ ] **[P1-15](https://github.com/Bola-j/Helping-Hands/issues/15)** — Build coordinator service and standards screens | ali | P1-07, P1-50, P1-14 | Connect create, edit/version and view screens to the API; show component quantities, units, validation and version history. |
| [ ] **[P1-16](https://github.com/Bola-j/Helping-Hands/issues/16)** — Implement charity application and coordinator decisions | Bola | P1-02, P1-05, P1-09, P1-10 | Record organization details or supporting document, decision actor/time and reason; coordinator approves or rejects; pending or rejected charities cannot receive referrals. |
| [ ] **[P1-17](https://github.com/Bola-j/Helping-Hands/issues/17)** — Build charity application and coordinator review screens | ali | P1-07, P1-50, P1-16 | Submit charity details or document, show pending/approved/rejected state, and enable coordinator review with clear validation and denial states. |
| [ ] **[P1-49](https://github.com/Bola-j/Helping-Hands/issues/53)** — Containerize the web frontend and configure the local proxy | Ziad | P1-06, P1-07 | Build the Expo frontend container and configure a documented same-origin HTTPS proxy to the API. Verify service-to-service routing and environment-based settings. |
| [ ] **[P1-50](https://github.com/Bola-j/Helping-Hands/issues/52)** — Implement the web session and antiforgery client | hussien | P1-07, P1-08, P1-49 | Connect the web application to API sessions: cookie persistence, antiforgery tokens, errors, logout and session expiry; verify it through the local proxy. |
| [ ] **[P1-51](https://github.com/Bola-j/Helping-Hands/issues/54)** — Implement current and historical standard-version behavior | Bola | P1-12, P1-13 | Use current category and standard versions for new requests and preserve recorded historical versions without silently substituting incompatible versions. |

### Sprint 2 tickets

| Ticket | Proposed responsible | Depends on | Acceptance / observable evidence |
| --- | --- | --- | --- |
| [ ] **[P1-18](https://github.com/Bola-j/Helping-Hands/issues/18)** — Model charity services and daily capacity | Ziad | P1-12, P1-13 | Add EF migrations for supported service-category and standard-unit versions, capacity and Cairo-date updates, with UUID keys, unique relationships and audit history. |
| [ ] **[P1-19](https://github.com/Bola-j/Helping-Hands/issues/19)** — Implement charity supported-service selection | Bola | P1-10, P1-14, P1-18 | Allow an involved charity representative to select and view supported services and applicable platform-unit versions; prevent unrelated access. |
| [ ] **[P1-20](https://github.com/Bola-j/Helping-Hands/issues/20)** — Implement direct available-from-now capacity updates | Bola | P1-13, P1-18, P1-19, P1-53 | Save nonnegative platform-unit quantities per charity/service/version for the Cairo day, replacing the available balance without double-subtracting earlier commitments. |
| [ ] **[P1-21](https://github.com/Bola-j/Helping-Hands/issues/21)** — Enforce daily capacity freshness and freeze | Bola | P1-18, P1-20 | Block new assignments only for the service/version lacking today’s update; show stale versus zero distinctly, preserve existing referrals, and test Cairo midnight. |
| [ ] **[P1-22](https://github.com/Bola-j/Helping-Hands/issues/22)** — Build charity services and capacity screens | hussien | P1-50, P1-19, P1-20, P1-21 | Select services, show standards and component contents, update direct unit capacity, and display last update, remaining units, zero and stale states. |
| [ ] **[P1-23](https://github.com/Bola-j/Helping-Hands/issues/23)** — Add the charity daily capacity task | ali | P1-50, P1-21, P1-22 | Show each service/version needing today’s update in the dashboard; refresh the task after an update and expose failure states. |
| [ ] **[P1-24](https://github.com/Bola-j/Helping-Hands/issues/24)** — Persist reminder scheduling and delivery state | Ziad | P1-18, P1-21 | Record one reminder intent per charity/Cairo day and attempt outcomes; support restart and retry without claiming delivery or silently duplicating recorded success. |
| [ ] **[P1-25](https://github.com/Bola-j/Helping-Hands/issues/25)** — Configure the local SMTP email catcher | Ziad | P1-06, P1-24 | Configure an isolated local email catcher in Docker with environment-supplied settings; no real email or credentials in Git. The API sender is P1-52. |
| [ ] **[P1-26](https://github.com/Bola-j/Helping-Hands/issues/26)** — Schedule the 08:00 Cairo capacity reminder | Bola | P1-21, P1-24, P1-25, P1-52 | Identify charities with one or more stale services and send one daily reminder per charity; handle Cairo time/DST, failures, retry and ambiguous sends explicitly. |
| [ ] **[P1-27](https://github.com/Bola-j/Helping-Hands/issues/27)** — Model multi-need beneficiary requests | Ziad | P1-02, P1-12, P1-13 | Add reviewed migrations for a request with separately tracked needs, each linked to a service, unit-standard version and quantity; retain existing records and history. |
| [ ] **[P1-28](https://github.com/Bola-j/Helping-Hands/issues/28)** — Implement beneficiary multi-need request submission | Bola | P1-09, P1-10, P1-27, P1-51 | Allow an authenticated beneficiary to submit their own request with one or more validated needs and authority/sharing confirmation; return UUIDs and human-readable references. |
| [ ] **[P1-29](https://github.com/Bola-j/Helping-Hands/issues/29)** — Build beneficiary request submission screens | hussien | P1-07, P1-50, P1-14, P1-28 | Let a beneficiary choose services and unit standards, add multiple needs and quantities, submit, and view validation, confirmation and generated reference. |
| [ ] **[P1-30](https://github.com/Bola-j/Helping-Hands/issues/30)** — Implement coordinator request and need review | Bola | P1-10, P1-27, P1-28 | Expose authorized review, classification, clarification and reasoned rejection/acceptance with actor/time history and current state. |
| [ ] **[P1-31](https://github.com/Bola-j/Helping-Hands/issues/31)** — Build coordinator request review screens | ali | P1-50, P1-28, P1-30 | Display mixed needs and standard contents; support review, clarification, accept/reject with reasons, authorization failures and progress. |
| [ ] **[P1-32](https://github.com/Bola-j/Helping-Hands/issues/32)** — Implement eligible-charity matching | Bola | P1-13, P1-19, P1-20, P1-21, P1-27, P1-30, P1-51 | Return only approved and active charities with matching selected service-category and compatible recorded standard-unit versions; show current capacity and daily freshness without leaking unrelated records. |
| [ ] **[P1-33](https://github.com/Bola-j/Helping-Hands/issues/33)** — Resolve existing requests when standards change | Bola | P1-13, P1-20, P1-27, P1-32 | Implement and test the interim exact-version policy for outstanding needs; reject incompatible substitutions, leave unmatched needs open, and preserve their history. |
| [ ] **[P1-52](https://github.com/Bola-j/Helping-Hands/issues/55)** — Implement the configurable SMTP sending adapter | Bola | P1-25 | Implement an environment-configured SMTP adapter for the API, targeting the local email catcher; handle failures and avoid claiming mailbox delivery. |
| [ ] **[P1-53](https://github.com/Bola-j/Helping-Hands/issues/56)** — Define and implement PostgreSQL allocation locking | Ziad | P1-18, P1-27 | Implement and document the shared ordered PostgreSQL row-lock and transaction protocol for capacity updates and referral allocations, preserving history and consistent balances. |

### Sprint 3 tickets

| Ticket | Proposed responsible | Depends on | Acceptance / observable evidence |
| --- | --- | --- | --- |
| [ ] **[P1-34](https://github.com/Bola-j/Helping-Hands/issues/34)** — Implement split referral allocation API | Bola | P1-18, P1-20, P1-21, P1-27, P1-32, P1-33, P1-53 | Allocate all or part of each need to eligible charities using the shared PostgreSQL locking protocol and P1-33 version decision; preserve commitments and prevent over-allocation. |
| [ ] **[P1-35](https://github.com/Bola-j/Helping-Hands/issues/35)** — Test PostgreSQL allocation and capacity concurrency | Ziad | P1-20, P1-21, P1-34 | Against isolated PostgreSQL, race assignments for the last unit and an update against an assignment; assert consistent balances and history. |
| [ ] **[P1-36](https://github.com/Bola-j/Helping-Hands/issues/36)** — Build coordinator matching and referral screens | ali | P1-50, P1-30, P1-32, P1-33, P1-34 | Show eligible charities, matching unit version, current availability and freeze reason; assign partial or full amounts to multiple charities under the P1-33 version policy and handle conflicts. |
| [ ] **[P1-37](https://github.com/Bola-j/Helping-Hands/issues/37)** — Expose role-appropriate progress and history | Bola | P1-10, P1-27, P1-34 | Show beneficiaries their own request/need progress, charity representatives only assigned details, and coordinators relevant decisions and follow-up; conceal unrelated records. |
| [ ] **[P1-38](https://github.com/Bola-j/Helping-Hands/issues/38)** — Build beneficiary request progress screens | hussien | P1-50, P1-28, P1-34, P1-37 | Show request reference, individual needs, assigned portions and progress with loading, empty, denial and error states; never expose unrelated evidence. |
| [ ] **[P1-39](https://github.com/Bola-j/Helping-Hands/issues/39)** — Implement charity outcome report and private attachment | Bola | P1-06, P1-10, P1-34 | Allow only assigned representatives to describe delivery and receipt, submit reported quantity and upload supported fictional evidence; validate size/type and mediate downloads. |
| [ ] **[P1-40](https://github.com/Bola-j/Helping-Hands/issues/40)** — Handle API evidence upload failures and reconciliation | Bola | P1-06, P1-39 | Coordinate staged S3 uploads and PostgreSQL metadata; clean up or reconcile orphans after failure and test interrupted uploads and denied downloads. |
| [ ] **[P1-41](https://github.com/Bola-j/Helping-Hands/issues/41)** — Build charity referral and evidence screens | ali | P1-50, P1-34, P1-37, P1-39 | Show only assigned referrals; enter delivered quantity and report, upload fictional evidence, submit and show unresolved/clarification/error states. |
| [ ] **[P1-42](https://github.com/Bola-j/Helping-Hands/issues/42)** — Implement coordinator outcome review | Bola | P1-27, P1-34, P1-39, P1-40 | Record coordinator review actor/time and accept or request clarification; count each accepted amount once. Explicit request closure is tracked in P1-55. |
| [ ] **[P1-43](https://github.com/Bola-j/Helping-Hands/issues/43)** — Build coordinator follow-up and closure screens | ali | P1-50, P1-37, P1-39, P1-42, P1-55 | Display each need, assignment and reviewed evidence, unresolved quantities and history; support review and explicit coordinator closure, and show why incomplete work cannot close. |
| [ ] **[P1-44](https://github.com/Bola-j/Helping-Hands/issues/44)** — Automate API and authorization regression scenarios | Bola | P1-09, P1-10, P1-16, P1-21, P1-26, P1-28, P1-32, P1-34, P1-39, P1-42, P1-55 | Add focused tests for registration, approvals, roles, capacity freeze, category and unit versions, matching, split allocations, explicit coordinator closure, evidence access and failure paths. |
| [ ] **[P1-45](https://github.com/Bola-j/Helping-Hands/issues/45)** — Automate web and integration scenarios | hussien | P1-50, P1-11, P1-15, P1-17, P1-22, P1-23, P1-29, P1-31, P1-36, P1-38, P1-41, P1-43 | Test role-specific navigation, authentication/CSRF integration, form validation and key browser paths, including unavailable dependency and permission states. |
| [ ] **[P1-46](https://github.com/Bola-j/Helping-Hands/issues/46)** — Verify database migration and persistence recovery | Ziad | P1-05, P1-06, P1-12, P1-18, P1-24, P1-25, P1-27, P1-40 | Apply migrations to an empty isolated PostgreSQL database, test an upgrade and restore/restart persistence; document results and unverified recovery behavior. |
| [ ] **[P1-47](https://github.com/Bola-j/Helping-Hands/issues/47)** — Run the complete fictional-data rehearsal | Bola | P1-26, P1-35, P1-42, P1-44, P1-45, P1-46, P1-54, P1-55, P1-56, P1-57 | Demonstrate web-to-API-to-PostgreSQL/S3/email flow, mixed needs and split referrals; show freeze and denied-access cases, record test evidence and defects. |
| [ ] **[P1-48](https://github.com/Bola-j/Helping-Hands/issues/48)** — Prepare Phase 1 evaluation evidence | Bola | P1-03, P1-04, P1-47 | Maintain current Issues/Project status and meeting notes; prepare reproducible setup, demo steps, team members’ genuine individual code commits, known gaps and discussion materials. |
| [ ] **[P1-54](https://github.com/Bola-j/Helping-Hands/issues/58)** — Test API responses for competing capacity operations | Bola | P1-20, P1-21, P1-34, P1-35, P1-53 | Exercise API-level conflict responses and transaction rollback during competing referrals and updates using isolated PostgreSQL; retain denied and failure-path evidence. |
| [ ] **[P1-55](https://github.com/Bola-j/Helping-Hands/issues/57)** — Implement explicit coordinator request closure | Bola | P1-27, P1-34, P1-42 | Add an authorized explicit coordinator closure action only after every required need is fulfilled and accepted; preserve actor/time and reject premature closure. |
| [ ] **[P1-56](https://github.com/Bola-j/Helping-Hands/issues/59)** — Verify local service startup and restart recovery | Ziad | P1-06, P1-25, P1-40, P1-46 | Start and restart the isolated API, database, S3 and email services; verify readiness, persisted data and unavailable-dependency behavior, recording limits. |
| [ ] **[P1-57](https://github.com/Bola-j/Helping-Hands/issues/60)** — Verify browser-to-API proxy integration | hussien | P1-11, P1-45, P1-49, P1-50, P1-56 | Exercise the containerized frontend through the same-origin proxy with fictional accounts, cookie/CSRF handling, errors and service interruption. |

## Scope-to-ticket coverage

| Agreed Phase 1 capability | Tickets |
| --- | --- |
| Existing implementation, requirements and local stack | P1-01, P1-02, P1-05–P1-08, P1-46, P1-49, P1-50, P1-56, P1-57 |
| Team access, GitHub tracking and individual contributions | P1-03, P1-04, P1-48 |
| Beneficiary/charity accounts and coordinator approval | P1-09–P1-11, P1-16, P1-17 |
| Coordinator-managed services, structured standards and versions | P1-12–P1-15, P1-33, P1-51 |
| Charity service selection and daily capacity freeze | P1-18–P1-23, P1-35, P1-53, P1-54 |
| In-app task, 08:00 Cairo email and SMTP integration | P1-23–P1-26, P1-52 |
| Multi-need requests, coordinator review and eligible matching | P1-27–P1-33 |
| Full/partial split referrals and concurrent capacity | P1-34–P1-38, P1-53, P1-54 |
| Charity evidence, follow-up, review and closure | P1-39–P1-43, P1-55 |
| Automated API/UI tests, recovery and fictional-data rehearsal | P1-44–P1-48, P1-56, P1-57 |

## Integration and acceptance checkpoints

- **Accounts → charity review:** signup/login uses real secure cookies and antiforgery; a coordinator can review a charity and only an approved active charity is eligible. The working requirements now specify coordinator approval; the earlier administrator-only API must be aligned under P1-16.
- **Standards → capacity → matching:** the selected service and standard version must match; test structured content and immutable version history. At Cairo midnight, an unrefreshed service/version freezes even if another service is current. Zero is a current update with zero availability, not a missing update.
- **Capacity → allocation:** a later direct update replaces available-from-now units without altering committed referrals or double-subtracting earlier assignments. Race assignments and updates against a real isolated PostgreSQL database; never exceed available need or capacity.
- **Needs → reports → closure:** keep distinct needs and split portions individually tracked without double counting; accepted partial reports leave unmet work open. Only authorized recipients upload private evidence and coordinators review; all needs must be fulfilled and accepted to complete.
- **Reminder integration:** exercise the 08:00 Cairo boundary, multiple stale services for one charity, retries, restart and idempotency with the local email catcher; document ambiguous SMTP outcomes rather than promising exactly-once delivery.
- **End-to-end:** demonstrate Expo web, cookie/CSRF authentication, API, PostgreSQL, S3 and email catcher together with fictional data. Verify unavailable dependencies, privacy and role restrictions, restart persistence, reviewed migrations and actual individual contributions.

## Existing API test baseline

The API test suite covers the implemented session foundation, intake/review validation, isolated PostgreSQL eligibility and reference/concurrency checks, and a fictional charity-to-outcome flow with private S3. These checks support the existing demo; they do not complete the later Phase 1 feature and integration tickets. Full recovery remains P1-46, and browser or deployment verification remains separate. See the README for quick and real-dependency test setup.

## Reconciliation and tracking notes

The previous [full-app roadmap](scrum-roadmap.md), [API v1 plan](api-v1-plan.md), [SRS](SRS.md), [PRD](PRD.md) and [database design](database-design.md) have been updated as working drafts for P1-02; the Scrum Master accepted the review and the diagrams were reconciled; physical mapping and cardinality decisions remain open: the latest Phase 1 decisions put coordinator charity approval, mixed needs, split referrals and capacity in the demo. Do not describe superseded requirements as already implemented. Update schema only with new reviewed migrations; do not rewrite applied migrations.

Project permissions were granted for setup; Ziad Walid (ZiadWaleed5) has accepted repository access; GitHub reports write permission. The other members’ usernames and access remain to be confirmed. Issues remain unassigned until each teammate’s real username and repository access are confirmed. All 57 Phase 1 issues are linked above with planned-sprint labels; the Project records their actual status. P1-03 was closed at the Scrum Master's direction; remaining access will be arranged separately. P1-04 tracks individual contribution evidence.

Contribution steps, proposed individual tasks and the evidence required for P1-04 are described in the [Phase 1 contribution workflow](phase1-contribution-workflow.md).
