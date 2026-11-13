# Software Requirements Specification — Helping Hands

**Version:** 1.3 | **Status:** Working requirements baseline; Scrum Master review accepted; signup and identity role decisions reconciled; other diagram reconciliation remains open | **Authors:** Project team

| Version | Change |
| --- | --- |
| 1.3 | Defined P1-10 identity access: Administrator inherits Coordinator permissions; Phase 1 self-service uses Beneficiary; third-party requester access is deferred. |
| 1.2 | Specified distinct beneficiary and charity signup flows and linked-profile/application outcomes for P1-09; documented the planned-vs-current implementation boundary. |
| 1.1 | Reconciled Phase 1 scope and coordinator approval; Scrum Master review accepted on 11 November 2026. |
| 1.0 | Finalized the working requirements baseline under the project owner’s delegation, retaining distinctions between business rules, proposed technical elaborations, and open targets. |
| 0.1 | Initial specification derived from BRD 1.3 and project instructions. |

## 1. Introduction

This specification describes the intended first-version behavior and constraints for Helping Hands, for the project team, instructor, and prospective stakeholders. BRD 1.4 is the working business baseline; this document is the project team’s working specification, finalized under the project owner’s delegation of remaining decisions. It elaborates the BRD without claiming separate approval of every requirement or any instructor or external stakeholder approval. See [BRD](BRD.md), [PRD](PRD.md), [system models](models.md), and [database design](database-design.md). The attached course materials inform the requirements and modeling approach; applicable course requirements should be checked against the final submission.

Helping Hands coordinates assistance and records requests, referrals, outcomes and review. Participants arrange and provide aid outside the platform. The platform does not take custody of money or goods, process payments, operate inventory or delivery logistics, or perform charity accounting. Public request listings, real operational pilots, and actual donations are outside the first version. The full BRD scope remains in scope even where sequenced after the core demonstration. Demonstrations use fictional people, data and evidence.

**Terms.** A *beneficiary* receives assistance and has a distinct Phase 1 account role; a *requester* submits the need, possibly for someone else, and third-party requester signup is not in the Phase 1 demo. A self-registering beneficiary account is linked to its beneficiary profile. A *referral recipient* is the approved charity or contributor assigned to provide assistance. A *portion* is an independently tracked quantity or clearly defined service scope. *Remaining need* is all unresolved need, including assigned portions; *unassigned need* is the unresolved part available to assign, including explicitly released undelivered portions. An *accepted outcome* is a coordinator's review of a participant's report, not independent proof. *Completed* means all required need is fulfilled and accepted; *closed unresolved* means work has stopped with unmet need and a reason.

## 2. Overall description

The intended React Native-based web client communicates with a single ASP.NET Core .NET 10 API backed by PostgreSQL. The API and database can use separate containers without making the application distributed. The frontend is not part of the present implementation. Participants deliver aid through arrangements outside the system; no payment or independent charity verification integration is assumed. The demo plans an SMTP-compatible local email catcher and private S3-compatible storage.

Actors are beneficiaries, third-party requesters, administrators, coordinators, charity representatives and individual contributors. Phase 1 beneficiaries submit their own requests; third-party requester access is deferred. Role and actual involvement both constrain access. An Administrator inherits Coordinator permissions while remaining a distinct role; a legacy `Requester` role does not grant Phase 1 access. Charity and contributor representatives handle only records connected to their active memberships and assigned referrals. Third-party requesters, when introduced, will receive only the information needed to follow requests they submitted.

The intended environment is a responsive browser application on phones, tablets and computers, with installable PWA capabilities on supported mobile browsers. React Native web support and PWA functionality require separate configuration and compatibility verification. PostgreSQL, .NET 10, Docker and the code-first migration approach are project constraints. Dependencies include browser capabilities, evidence storage and operational policies yet to be selected, and stakeholder and local legal review before real operation. Participants' willingness to provide meaningful evidence and arrange assistance externally is assumed, not validated.

**Requirement conventions:** priorities are Core (course demonstration), Next (later first-version delivery), or Cross-cutting; these are sequencing suggestions, not exclusions. Confirmed means adopted from the BRD or explicit project direction for this baseline, not separately approved wording. Proposed means a working technical or behavioral elaboration that still requires design or stakeholder validation where applicable; it is not a completed implementation. Open identifies an unresolved policy, capability or target. The Scrum Master accepted the Phase 1 review on 11 November 2026; the baseline remains subject to unresolved design work, and adoption does not resolve open items or turn proposals into verified facts. Verification describes a future check, not a completed result. Actor abbreviations: B beneficiary, T third-party requester, A administrator, C coordinator, R referral recipient, U authorized user, Team project team.

### Phase 1 backend delivery planning

The current forecast targets 12 November 2026 for the Phase 1 API, PostgreSQL migrations, local containers and API CI. This is a planning target, not a claim of implementation, a verified test result or a commitment; capacity, Docker availability and P1-33 implementation and testing remain constraints. The proposed first milestone establishes the local API, database, private S3-compatible storage and a build/test CI baseline before the workflow integrations are treated as complete. The [backend delivery breakdown](phase1-backend-delivery-breakdown.md) maps existing backlog tickets to dependencies and evidence; the [demo roadmap](api-demo-scrum-roadmap.md) retains the web-client and complete demonstration scope. The fallback described in that plan is explicitly partial and does not waive acceptance criteria.

## 3. User requirements

| ID | User requirement | Priority | Actor | Source | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- |
| UR-001 | A charity needs to apply and receive an application decision. | Core | C, R | BR-01, BR-02 | TC-001 | Confirmed |
| UR-002 | A contributor needs to register and be approved before referral. | Next | C, R | BR-10, BRule-11 | TC-002 | Confirmed |
| UR-003 | A requester needs to submit and follow a need, including on another person's behalf. | Core | B, T, C | BR-03, BR-07 | TC-003 | Confirmed |
| UR-004 | A coordinator needs to review, classify and, when appropriate, split mixed needs. | Core / Next | C | BR-04 | TC-004 | Confirmed |
| UR-005 | A coordinator needs to refer and track distinct portions with approved recipients. | Core / Next | C, R | BR-05, BRule-14 | TC-005 | Confirmed |
| UR-006 | A recipient needs to report outcomes and provide evidence for review. | Core | R, C | BR-06, BR-10 | TC-006 | Confirmed |
| UR-007 | Authorized users need relevant progress and decision history while private information remains restricted. | Core | U | BR-07, BR-08 | TC-007 | Confirmed |
| UR-008 | A coordinator needs to follow unresolved need, release it when appropriate and close work with a reason when unmet. | Next | C | BRule-09, BRule-14 | TC-005, TC-008 | Confirmed |
| UR-009 | Beneficiaries and charity representatives need distinct account signup flows that create the appropriate profile or pending application. | Core | B, R | BR-01, BR-03 | TC-010 | Confirmed |

## 4. Functional requirements

| ID | Mandatory system requirement | Priority | Actor | Source | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- |
| FR-001 | The system shall accept charity applications containing organization name, contact details and registration information or supporting documentation. | Core | R | BRule-10 | TC-001 | Confirmed |
| FR-002 | The system shall record an application as pending until an authorized coordinator makes a decision. | Core | C | BRule-01 | TC-001 | Confirmed |
| FR-003 | The system shall allow an authorized coordinator to approve or reject a charity application and record the decision. | Core | C | BR-01 | TC-001 | Confirmed |
| FR-004 | The system shall activate a charity account on approval and prevent pending or rejected charities from receiving referrals. | Core | C | BRule-01 | TC-001 | Confirmed |
| FR-005 | The system shall accept contributor registrations with contact information. | Next | R | BRule-11 | TC-002 | Confirmed |
| FR-006 | The system shall allow a coordinator to approve a contributor and prevent referral to a contributor before approval. | Next | C | BRule-11 | TC-002 | Confirmed |
| FR-007 | The system shall accept a request from a beneficiary; later full-product scope includes third-party and coordinator-on-behalf requests. | Core / Next | B, T, C | BR-03 | TC-003 | Confirmed |
| FR-008 | The system shall record the requester, beneficiary, need and request submission time. | Core | B, T, C | BR-03, BRule-09 | TC-003 | Proposed |
| FR-009 | Before submission, the system shall require the requester to confirm authority to submit the information and agreement to necessary sharing for referral. | Core | B, T, C | BRule-12 | TC-003 | Confirmed |
| FR-010 | The system shall allow a coordinator to request clarification on a submitted request. | Core | C | BR-04 | TC-004 | Confirmed |
| FR-011 | The system shall allow a coordinator to accept a request for referral or reject it with a recorded reason. | Core | C | BR-04 | TC-004 | Confirmed |
| FR-012 | The system shall allow a coordinator to classify a need as goods, money or organizational referral. | Core | C | IS-03 | TC-004 | Confirmed |
| FR-013 | The system shall permit an organizational-referral need to identify a service subtype. | Core | C | IS-03 | TC-004 | Confirmed |
| FR-014 | The system shall preserve separately tracked mixed needs and their source relationship when a coordinator separates them for referral. | Core | C | BRule-04 | TC-004 | Confirmed |
| FR-015 | The system shall allow a coordinator to refer any need category to an eligible charity or contributor. | Core / Next | C | BRule-03 | TC-005 | Confirmed |
| FR-016 | The system shall allow a coordinator to define separately tracked portions and assign each to one eligible referral recipient. | Core | C | BR-05, BRule-14 | TC-005 | Confirmed |
| FR-017 | The system shall prevent assigning a portion simultaneously to another recipient while it remains assigned. | Core | C | BRule-14 | TC-005 | Confirmed |
| FR-018 | The system shall show remaining unresolved need separately from unassigned need available for referral. | Core | C | BRule-14 | TC-005 | Confirmed |
| FR-019 | For a service without meaningful quantity, the system shall record a coordinator-defined, separately trackable scope for each portion. | Next | C | BRule-14 | TC-005 | Confirmed |
| FR-020 | The system shall allow a recipient to report an outcome against its assigned referral. | Core | R | BR-06 | TC-006 | Confirmed |
| FR-021 | The system shall require an outcome description and supporting attachment(s) addressing delivery and receipt for outcome review. | Core | R | BRule-05 | TC-006 | Confirmed |
| FR-022 | The system shall record the amount and evidence of transfer and receipt for monetary outcome reports. | Core | R | BRD §8 | TC-006 | Confirmed |
| FR-023 | The system shall record the assistance and evidence of provision and receipt for goods or service outcome reports. | Core | R | BRD §8 | TC-006 | Confirmed |
| FR-024 | The system shall allow an authorized coordinator to accept a reported outcome, request clarification, or record it unresolved. | Core | C | BRD §8, BRule-08 | TC-006 | Confirmed |
| FR-025 | The system shall prevent a referral from being completed before coordinator acceptance. | Core | C | BR-10 | TC-006 | Confirmed |
| FR-026 | The system shall record accepted partial fulfillment without completing the whole referral and retain its undelivered portion as unresolved. | Core | C | BRule-14 | TC-005 | Confirmed |
| FR-027 | The system shall allow a coordinator to release an undelivered assigned portion for reassignment while preserving the original referral and outcome history. | Next | C | BRule-14 | TC-005 | Confirmed |
| FR-028 | The system shall permit reassignment only of unassigned or explicitly released unresolved need. | Next | C | BRule-14 | TC-005 | Confirmed |
| FR-029 | The system shall record optional beneficiary or authorized third-party confirmation and disagreements separately from the recipient's report. | Next | B, T, C | BRule-13 | TC-008 | Confirmed |
| FR-030 | The system shall leave a disputed or insufficient outcome unresolved for coordinator follow-up. | Core | C | BRule-13 | TC-008 | Confirmed |
| FR-031 | The system shall allow authorized users to view relevant request and referral progress. | Core | U | BR-07 | TC-007 | Confirmed |
| FR-032 | The system shall record significant decisions and referral updates with actor and time. | Core | U | BRule-09 | TC-007 | Confirmed |
| FR-033 | The system shall retain unresolved or declined portions for follow-up. | Next | C | BRule-09 | TC-008 | Confirmed |
| FR-034 | The system shall permit explicit coordinator completion only when all required need is fulfilled and accepted. | Core | C | BRule-09 | TC-008 | Confirmed |
| FR-035 | The system shall allow a coordinator to close a request with unmet need as closed unresolved only with a recorded reason. | Next | C | BRule-09 | TC-008 | Confirmed |
| FR-036 | The system shall restrict beneficiary details and evidence according to role and involvement, including recipient assignment and requester relationship. | Core | U | BRule-07, BRule-12 | TC-007 | Confirmed |
| FR-037 | The system shall reject unauthorized read or mutation attempts without disclosing restricted information. | Core | U | BR-08 | TC-007 | Proposed |
| FR-038 | The system shall allow a coordinator to record a follow-up action for an unresolved or declined referral. | Core | C | BRD §8 | TC-008 | Confirmed |
| FR-042 | The system shall provide separate beneficiary and charity signup routes. Beneficiary signup creates an account with the `Beneficiary` role and a linked beneficiary profile; charity signup creates an account with the `RecipientRepresentative` role, membership and a pending inactive charity application. Both use globally unique email, antiforgery protection, UUID identities and server-generated profile/application references; registration does not sign the user in or verify email. | Core | B, R | UR-009; IS-01–02 | TC-010 | Confirmed |

### Phase 1 functional elaboration (planned)

The current Phase 1 baseline narrows FR-007 to beneficiary self-submission; third-party and coordinator-on-behalf submission remain later full-product scope. Each submission may contain multiple distinct needs. Separately tracked needs may be implemented as linked records or another reviewed representation, provided their source and history are preserved without double-counting. Full split allocation among approved charities is in the demo; explicit release and reassignment after a partial report remain later scope.

| ID | Requirement | Verification scenario | Backlog |
| --- | --- | --- | --- |
| P1-FR-01 | Coordinators manage service categories and versioned platform-unit standards with structured component names, quantities and units; new requests use current versions while history retains its version. | Create a new version and inspect earlier need, capacity and referral history. | P1-12–P1-15, P1-51 |
| P1-FR-02 | A charity can select supported services and set nonnegative available-from-now quantities, replacing current availability without altering previous commitments. | Update availability after a referral and check balance and history. | P1-18–P1-20, P1-22 |
| P1-FR-03 | A service/version without an update on the current Africa/Cairo calendar day is ineligible for new referral; an updated zero is current but unavailable. | Exercise Cairo midnight, zero and multiple services with different freshness. | P1-21–P1-23, P1-32 |
| P1-FR-04 | Create one 08:00 Cairo daily reminder intent per charity with stale services, with an in-app task, delivery attempt history and retry handling; do not assert mailbox delivery from SMTP acceptance. | Exercise multiple stale services, restart, failures and ambiguous sends with local test email. | P1-23–P1-26, P1-52 |
| P1-FR-05 | Match an approved active charity by supported, compatible version and current availability; atomically allocate portions without oversubscribing need or capacity. | Race assignments and capacity updates in isolated PostgreSQL; inspect balances and denied access. | P1-32–P1-36, P1-53–P1-54 |
| P1-FR-06 | Require an assigned recipient's report and private supporting evidence, coordinator acceptance or clarification, and explicit closure only after all required needs are fulfilled and accepted. | Check missing evidence, unrelated access, partial acceptance and premature closure. | P1-39–P1-43, P1-55 |

For existing needs when a standard changes, the Scrum Master selected exact-version matches only until the policy is reviewed in [P1-33](https://github.com/Bola-j/Helping-Hands/issues/33). If no eligible exact-version capacity exists, the need remains open; no incompatible version may be substituted. These scenarios specify expected behavior and are not test results.

## 5. Non-functional requirements

| ID | Requirement | Priority | Actor | Source | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- |
| NFR-001 | The system shall authenticate protected operations and enforce authorization on the server. | Cross-cutting | U | BR-08 | Authorization tests | Proposed |
| NFR-002 | The system shall transmit credentials and sensitive data over encrypted transport outside an isolated local development environment. | Cross-cutting | U | BO-04 | Configuration review | Proposed |
| NFR-003 | The system shall minimize disclosed beneficiary details and limit evidence access to authorized involved users. | Cross-cutting | U | BRule-07, BRule-12 | TC-007 | Confirmed |
| NFR-004 | The interface shall support keyboard access, meaningful labels and readable content; an accessibility standard and conformance target require agreement. | Cross-cutting | U | Project quality | Accessibility review | Proposed |
| NFR-005 | The interface shall adapt its layout and controls to phone, tablet, laptop and desktop viewport sizes without requiring horizontal scrolling for ordinary content. | Cross-cutting | U | Explicit technology direction | Responsive device tests | Proposed |
| NFR-006 | The web build shall provide an installable PWA experience where browser and platform capabilities support it; installation is not assumed to work identically everywhere. | Cross-cutting | U | Explicit direction | Matrix-based install tests | Confirmed |
| NFR-007 | Offline business operations and automatic background synchronization are outside the course-version scope; push notifications are not required. Adding any of these capabilities requires a separate decision and conflict-handling design. | Cross-cutting | Team | Explicit direction | Scope review | Confirmed |
| NFR-008 | The system shall preserve accepted decisions and history across normal restarts and recover from failures according to a documented backup and restore procedure. | Cross-cutting | Team | BO-03 | Restore exercise | Proposed |
| NFR-009 | Availability and recovery objectives shall be agreed before operational deployment; no numeric service commitment is established here. | Cross-cutting | Team | BRD D-03 | Review agreed objectives | Open |
| NFR-010 | Response-time and scale targets shall be agreed and measured with representative workloads before operational deployment. | Cross-cutting | Team | Project instructions | Review benchmark and targets | Open |
| NFR-011 | The API shall remain one understandable .NET 10 monolith with PostgreSQL and configuration external to code; secrets shall not be committed. | Cross-cutting | Team | Explicit direction | Architecture and configuration review | Confirmed |
| NFR-012 | The system shall record operational errors without exposing credentials or unnecessary sensitive evidence in logs. | Cross-cutting | Team | BO-04 | Log inspection | Proposed |
| NFR-013 | Assignment and accepted fulfillment updates shall preserve data integrity under concurrent actions. | Cross-cutting | C | BRule-14 | Concurrent assignment test | Proposed |
| NFR-014 | The application shall support local Docker-based development using configuration supplied for the environment. | Cross-cutting | Team | Explicit direction | Local startup check | Confirmed |
| NFR-015 | The interface shall provide touch-operable controls for primary actions and forms without requiring hover. | Cross-cutting | U | Explicit direction | Exercise primary journeys on representative touch devices | Proposed |
| NFR-016 | The interface shall identify form errors in text and associate each error with its field so that keyboard and assistive-technology users can locate and correct it. | Cross-cutting | U | Explicit direction | Submit invalid forms and inspect keyboard and assistive-technology behavior | Proposed |
| NFR-017 | The interface shall provide visible keyboard focus and navigation that can be operated in a logical order without a pointer. | Cross-cutting | U | Explicit direction | Navigate representative journeys using only a keyboard | Proposed |
| NFR-018 | The interface shall use text and control colors that meet the contrast criteria of an agreed accessibility standard; the standard and conformance target remain open. | Cross-cutting | U | Explicit direction | Check representative screens against the agreed standard once selected | Proposed |
| NFR-019 | When connectivity is lost during a business operation, the interface shall inform the user that the operation could not be confirmed and shall not present it as successful until the server confirms it. | Cross-cutting | U | Explicit direction | Disconnect during a submission and verify the message and subsequent server-confirmed state | Proposed |

The course-version client requires a connection for business operations; offline business writes and automatic background synchronization are not specified. If a response is lost after the server may have processed an operation, the client must let the user check the current server state after reconnection before retrying. Offline caching of sensitive information is not assumed; any offline capability requires a separate decision.

**Proposed support matrix for validation:** current stable Chrome on Android, Safari on iOS/iPadOS, and current stable Chrome, Edge and Firefox on desktop; test responsive layouts on representative phone, tablet and desktop sizes. Browser versions, OS minimums, PWA installation and update behavior remain open pending actual device tests. iOS installation and browser behavior differ from Android; offline operation and push are not implied by installability. Consult primary documentation and record results before committing to a support policy; see the PRD or architecture references for current source-backed compatibility analysis.

## 6. Domain rules and interfaces

BRD BRule-01 through BRule-14 remain authoritative. Particularly, eligibility precedes referral; only unassigned or explicitly released need can be assigned; remaining need includes assigned unresolved portions; accepted partial delivery does not finish the referral; and independent beneficiary confirmation is not required for the course demonstration. A coordinator's acceptance is review of a report, never a guarantee of independent verification. A closed unresolved request retains its unmet need and reason.

| ID | Interface requirement | Priority | Actor | Source | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- |
| IR-001 | The interface shall present role-appropriate actions and progress, including clear pending, partial and unresolved states. | Core | U | BR-07 | UI walkthrough | Proposed |
| IR-002 | The API shall validate input, enforce authorization and return an actionable error for invalid or unauthorized operations without exposing restricted data. | Cross-cutting | U | BR-08 | API negative tests | Proposed |
| IR-003 | The evidence interface shall associate uploaded attachment metadata and reported outcomes with the relevant referral and restrict access. | Core | R, C | BR-06, BR-08 | TC-006, TC-007 | Proposed |
| IR-004 | Permitted file types, size limits, storage, malware handling and retention shall be decided before operational use. | Cross-cutting | Team | BRD D-03 | Policy review | Open |

## 7. Data requirements

| ID | Requirement | Priority | Actor | Source | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- |
| DR-001 | The system shall distinguish users, beneficiaries, charity applications, contributors, requests, linked requests, portions, referrals, reports, evidence and review decisions. | Cross-cutting | Team | BR-01–08, BR-10 | Model review | Proposed |
| DR-002 | Each referral shall identify its request, defined portion and responsible eligible recipient. | Core / Next | C | BR-05 | Referential integrity test | Proposed |
| DR-003 | The system shall retain actor, timestamp, decision and relevant reason for significant decisions and updates. | Core | U | BRule-09 | History review | Confirmed |
| DR-004 | Evidence metadata shall identify its referral or report, submitting actor, time and storage reference, with access restricted. | Core | R, C | BR-06, BR-08 | Metadata/access test | Proposed |
| DR-005 | Quantity-based assignments and accepted fulfillment shall not exceed the relevant available or assigned amount. | Core | C | BRule-14 | Boundary and concurrent tests | Proposed |
| DR-006 | Personal data and evidence retention, deletion, access recovery and consent policies shall be resolved with appropriate local review before real use. | Cross-cutting | Team | BRule-12, D-03 | Policy review | Open |

The Phase 1 database design proposal, conceptual model, candidate PostgreSQL mapping, integrity considerations and code-first migration approach are in [database-design.md](database-design.md). The deployed database state and enforcement require verification; the presence of code or migrations does not establish a working database. Each table has a UUID primary key, and user-facing business records receive server-generated references as described in the [API plan](api-v1-plan.md).

## 8. Use cases and models

The context, use-case overview, activity, sequence, domain and state models are in [models.md](models.md); the database models are in [database-design.md](database-design.md). Models illustrate intended behavior and do not imply implementation.

| ID | Actor / precondition | Main flow | Exception / result | Requirements |
| --- | --- | --- | --- | --- |
| UC-001 | Charity applicant; coordinator authorized | Apply → coordinator reviews → approve and activate | Pending or rejected cannot receive referrals | FR-001–004 |
| UC-002 | Contributor; coordinator authorized | Register → coordinator approves → eligible for referral | Unapproved contributor cannot be assigned | FR-005–006 |
| UC-003 | B, T or C; authority and sharing confirmed | Submit → coordinator reviews and classifies → accepts | Clarification or reasoned rejection | FR-007–013 |
| UC-004 | C; mixed request reviewed | Track and refer each distinct need | Preserve source relationship without double counting | FR-014 |
| UC-005 | C; eligible recipient and available need | Define portion → assign → track progress | Assigned portion cannot be duplicated; release undelivered portion before reassignment | FR-015–019, FR-026–028 |
| UC-006 | R assigned; C authorized | Report delivery and receipt with attachment → coordinator reviews → accept | Clarification, dispute or insufficient evidence remains unresolved; partial acceptance does not complete referral | FR-020–030 |
| UC-007 | U with role and involvement | View permitted progress and history | Unauthorized access denied | FR-031–032, FR-036–037 |
| UC-008 | C; request has progress | Record follow-up → complete if all fulfilled and accepted, or close unresolved with reason | Unmet need remains visible in history | FR-033–035, FR-038 |
| UC-009 | Beneficiary or charity representative | Use type-specific signup → receive account and linked profile/application | Charity remains pending; login is separate | FR-042 |

## 9. System design and verification

The planned React Native Web client calls one ASP.NET Core Minimal API. Existing endpoint and workflow behavior must be verified against the Phase 1 assessment; planned routes do not prove complete workflows. Endpoint handlers handle HTTP input and authorization; focused services implement workflows; database access persists to PostgreSQL. See [architecture.md](architecture.md) for the current foundation and proposed evolution. The planned demo uses private S3-compatible storage, with metadata and access checks in the API; the selected operational storage policies remain open. Existing behavior must be verified independently. Configuration and secrets belong outside committed source. No external aid transfer is performed by the application.

Review requirements with users and the instructor where available; validate usefulness and operating arrangements with prospective participants before real deployment. Verify functionality with fictional-data demonstrations and automated or manual role, boundary, concurrency and recovery checks as implementation becomes available. These scenarios are representative specifications, not test results.

| ID | Scenario and expected result | Covers |
| --- | --- | --- |
| TC-001 | Submit charity application; verify pending and rejected are ineligible, approval activates and enables referral. | FR-001–004 |
| TC-002 | Register contributor; referral fails before coordinator approval and succeeds after it. | FR-005–006 |
| TC-003 | Submit direct, third-party and coordinator-entered requests; missing authority/sharing confirmation blocks submission. | FR-007–009 |
| TC-004 | Review and classify each category; request clarification, accept or reject with reason; split a mixed need and inspect links. | FR-010–014 |
| TC-005 | For 10 boxes assign 6 to A and 4 to B. Accept 3 from A: 7 unresolved, 0 unassigned, A incomplete. Release A's 3: 3 become assignable, B's 4 remain assigned; reassign 3 and retain history. Concurrent duplicate assignments must fail. | FR-015–019, FR-026–028; DR-005 |
| TC-006 | For fictional money, goods and service referrals, submit description and supporting delivery/receipt evidence; request clarification and then accept. No completion before acceptance. | FR-020–025 |
| TC-007 | Exercise requester, third-party, unrelated recipient, coordinator and administrator access to request, evidence and history; unauthorized reads and writes fail. | FR-031–032, FR-036–037 |
| TC-008 | Record disagreement separately; insufficient or disputed report stays unresolved. Record follow-up, complete only when all need is accepted, or close unresolved with reason. | FR-029–035, FR-038 |
| TC-010 | Exercise both signup routes, profile/application links, seeded roles, references, validation, global and concurrent email uniqueness, antiforgery, transaction rollback, separate login and credential-safe responses. | FR-042 |

## 10. Traceability

| BRD objective / requirement | Product feature | User requirement | System requirements | Use case | Verification |
| --- | --- | --- | --- | --- | --- |
| BO-02/03; BR-01/02 | F-01 Charity | UR-001 | FR-001–004 | UC-001 | TC-001 |
| BO-02/03; BR-10 | F-02 Contributor | UR-002 | FR-005–006 | UC-002 | TC-002 |
| BO-01/03; BR-03/04 | F-03 Submission/authority; F-04 Review/classification | UR-003/004 | FR-007–013 | UC-003 | TC-003/004 |
| BO-01; BR-04 | F-05 Mixed request splitting | UR-004 | FR-014 | UC-004 | TC-004 |
| BO-01/02; BR-05, BRule-14 | F-06 Referral/assigned portions; F-08 Partial fulfillment/reassignment | UR-005/008 | FR-015–019, 026–028; DR-005 | UC-005 | TC-005 |
| BO-03; BR-06/10 | F-07 Outcome/evidence/review | UR-006 | FR-020–025, 029–030; IR-003 | UC-006 | TC-006/008 |
| BO-01/03/04; BR-07/08 | F-10 Authorization/history | UR-007 | FR-031–032, 036–037; NFR-001–003; DR-003 | UC-007 | TC-007 |
| BO-01/03; BRule-09 | F-09 Progress/follow-up/closure | UR-008 | FR-033–035, 038 | UC-008 | TC-008 |
| BO-02/03; BR-01/03 | F-00 Signup; F-01 Charity review | UR-009 | FR-042, FR-001–004 | UC-009, UC-001 | TC-010, TC-001 |
| BO-05; BR-09 | Stakeholder validation | — | No software requirement implied | — | Review feedback if available; gap remains |

## 11. Open decisions, risks and evolution

Evidence quality and charity approval do not prove future conduct. Privacy, consent, retention, verification, file handling and local legal obligations need stakeholder and appropriate local review before real operation. Browser/device minimums, accessibility target, PWA offline/push scope, availability, recovery and performance targets remain open; no targets or operational validation are invented. Quantity units, service-scope overlap and detailed lifecycle transitions require design review to avoid double counting. Real partners' processes, availability and willingness remain assumptions. The simple monolith can grow through feature-focused code and controlled database changes as validated needs arise; no scaling infrastructure or external integrations are assumed now.


### Detailed use case descriptions

**UC-002 — Contributor registration and approval**  
Purpose: establish contributor eligibility for referrals. Actors: contributor applicant and authorized coordinator. Preconditions: the applicant can access registration and the coordinator is authorized. Trigger: the contributor starts registration.

1. The contributor submits contact information.
2. The system records the registration for coordinator review.
3. The coordinator reviews and approves the contributor.
4. The system makes the approved contributor eligible for referral.

Alternatives: An incomplete registration is returned for correction; an unapproved contributor remains ineligible for referral. Postconditions: the registration and any approval decision remain available to authorized users. Requirements: FR-005–006, FR-032.

**UC-007 — View authorized progress**  
Purpose: let involved participants follow relevant work while protecting restricted information. Actors: requester, referral recipient and authorized staff. Preconditions: the user is authenticated and has a role and involvement permitting access. Trigger: the user opens a request or referral.

1. The system checks the user's role and relationship to the record.
2. The system presents the progress and history permitted for that user.
3. The user follows the available request or referral updates.

Alternatives: An unrelated or unauthorized user is denied access without disclosure of restricted details; a third-party requester receives only information needed to follow the submitted request. Postconditions: no business record is changed by viewing it. Requirements: FR-031–032, FR-036–037, NFR-001–003.

**UC-001 — Charity application and review**  
Purpose: establish eligibility for charity referrals. Actors: charity representative and coordinator. Preconditions: applicant can access signup; coordinator is authorized. Trigger: applicant submits the charity signup route.

1. Applicant enters representative account details, organization name, contact details and optional registration information and submits the charity signup route.
2. The system creates the account, `RecipientRepresentative` role, membership and application together; the application remains pending and inactive.
3. The coordinator reviews the application and supplied information.
4. The coordinator approves it; the system records the decision and activates the charity.

Alternatives: Invalid registration or duplicate email creates no partial account/application; the coordinator may leave the application pending or reject it. Postconditions: the decision and actor/time are retained; pending or rejected charities remain ineligible. Requirements: FR-001–004, FR-032, FR-042.

**UC-009 — Beneficiary and charity account signup**
Purpose: create the correct account and business profile at signup. Actors: beneficiary and charity representative. Preconditions: applicant can access the public signup route and has an antiforgery token. Trigger: applicant selects the appropriate signup flow.

1. A beneficiary submits display name, email, password, optional contact details and family flag to `/api/v1/auth/register/beneficiary`; the system creates the `Beneficiary` account and linked profile in one transaction and returns both UUIDs and the `BEN` reference.
2. A charity representative submits representative name, email, password, organization name, contact details and optional registration information to `/api/v1/auth/register/charity`; the system creates the `RecipientRepresentative` account, membership and pending inactive charity application in one transaction and returns the account/application UUIDs and `REC` reference.
3. The applicant signs in separately. Charity review and activation follow UC-001; signup itself does not grant referral eligibility.

Alternatives: Invalid fields, weak password, duplicate/case-variant email, concurrent duplicate signup, invalid antiforgery token or failed related-record creation returns a safe error without credentials or partial business records. Email verification and third-party requester signup are outside this increment. Requirements: FR-042, NFR-001–003.

**UC-003 — Submit and review a request**  
Purpose: record and assess a need. Actors: beneficiary, third-party requester or coordinator; coordinator reviewer. Preconditions: the submitter has the authority required to provide information. Trigger: a requester begins recording a need.

1. The requester provides the beneficiary and need details and confirms authority and necessary sharing.
2. The system records the request and makes it available for authorized review.
3. A coordinator reviews the need, classifies it as goods, money or organizational referral, and records the decision.
4. The coordinator accepts the request for referral.

Alternatives: Missing confirmation prevents submission; the coordinator requests clarification or rejects the request with a reason. Mixed support may proceed to UC-004. Postconditions: request and decision history are available only to authorized participants. Requirements: FR-007–013, FR-031–032, FR-036.

**UC-004 — Split mixed needs**  
Purpose: track different kinds of support separately. Actor: coordinator. Preconditions: a request has mixed needs and is under authorized review. Trigger: the coordinator reviews and refers each distinct need.

1. The coordinator identifies and reviews distinct kinds of support in the submitted request.
2. The system tracks each need separately and preserves its source and context.
3. The coordinator reviews the resulting needs for referral.

Alternatives: If no split is needed, the original continues through review. Postconditions: needs are separately trackable and parent and child representations are not double-counted. Requirements: FR-012–014; the physical representation requires design review.

**UC-005 — Assign and follow a portion**  
Purpose: give an eligible recipient responsibility for a defined available need. Actors: coordinator and approved charity or contributor. Preconditions: the need is accepted for referral, the recipient is eligible, and the portion is unassigned or explicitly released. Trigger: the coordinator initiates an assignment.

1. The coordinator inspects remaining and unassigned need and selects an eligible recipient.
2. The coordinator defines the quantity or separately trackable service scope and assigns it.
3. The system records the referral and shows its progress separately.
4. If a portion remains undelivered, the coordinator follows it up and may explicitly release it for reassignment.

Alternatives: The system rejects ineligible recipients, unavailable or simultaneously assigned portions, and assignments exceeding available need. A concurrent change requires re-evaluation against current balances. Postconditions: a released amount becomes available, while the original referral and its outcomes remain in history. Requirements: FR-015–019, FR-026–028, FR-038, NFR-013.

**UC-006 — Report and review an outcome**  
Purpose: review a recipient's report of assistance and supporting evidence. Actors: assigned recipient and authorized coordinator. Preconditions: an assigned referral exists. Trigger: the recipient reports an outcome.

1. The recipient describes the assistance and attaches evidence addressing delivery and receipt; monetary reports include amount and evidence of transfer and receipt.
2. The system associates the report and attachments with the referral for authorized review.
3. The coordinator reviews the report and accepts it, requests clarification or records it unresolved.
4. The system records the decision and updates fulfillment only for an accepted outcome.

Alternatives: A missing description or attachment blocks submission for review. Insufficient or disputed evidence remains unresolved. An accepted partial report leaves the undelivered balance unresolved and assigned until released; it does not complete the referral. Separate beneficiary or authorized third-party confirmation is optional for the course demonstration. Postconditions: the report, evidence and decision remain in history; acceptance is a review, not independent proof. Requirements: FR-020–030, FR-032, IR-003.

**UC-008 — Follow up and close**  
Purpose: keep unresolved work visible and record its final disposition. Actor: coordinator. Preconditions: a request exists and the coordinator is authorized. Trigger: follow-up or closure is needed.

1. The coordinator reviews relevant referral progress and remaining need.
2. The coordinator records a follow-up action for any unresolved or declined portion.
3. If all required need is fulfilled and accepted, the coordinator completes the request.
4. If work stops with unmet need, the coordinator instead closes it unresolved and records a reason.

Alternatives: The request remains open while further action is expected; completion or unresolved closure without the required conditions is rejected. Postconditions: progress, unmet need, reason and history remain available to authorized participants. Requirements: FR-031–035, FR-038.

### Proposed handling of a later dispute

A disagreement can arrive after an outcome was accepted. Until the coordinator resolves it, the system should flag the outcome and request for follow-up and prevent completion based on a disputed acceptance. An already completed request should be visibly marked disputed or reopened; affected split ancestors should likewise not appear completed. The proposed database design freezes allocation, acceptance, release and completion transitions for the affected request while its current assignment and historical review remain visible. A coordinator may close the request unresolved with a reason when work stops with unmet need, even while the dispute remains open; this does not treat the disputed outcome as accepted or resolve the dispute. A coordinator may resolve the disagreement with a recorded rationale or initiate an explicit audited correction; a correction must recompute accepted fulfillment, remaining and unassigned need atomically without deleting prior decisions or automatically making an assigned amount available. This behavior is **proposed** and requires stakeholder and design review before implementation. It does not imply independent proof or require beneficiary confirmation.

| ID | Requirement | Priority | Actor | Source | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- |
| FR-039 | On a later recorded disagreement concerning an accepted outcome, the system shall flag the disputed outcome and request, prevent completion based on that acceptance, and visibly mark an already completed request and affected split ancestors disputed or reopen them until coordinator resolution. Closure unresolved with a reason remains available when work stops with unmet need. | Next | C | BRule-13; proposed elaboration | TC-009 | Proposed |
| FR-040 | The system shall prevent allocation, acceptance, release and completion transitions for a request with an open dispute until the coordinator resolves it or makes an audited correction. | Next | C | BRule-13–14; proposed elaboration | TC-009 | Proposed |
| FR-041 | The system shall preserve the original report and decision and atomically recalculate affected balances when an authorized coordinator records a correction. | Next | C | BRule-09, BRule-14; proposed elaboration | TC-009 | Proposed |
| TC-009 | Accept a fictional partial outcome, record a later disagreement, attempt completion and affected reassignment, then resolve or correct it; check the freeze, recalculated balances and preserved audit history. | Next | C | FR-039–041 | Future test, not a result | Proposed |

# Appendix: Phase 1 requirements trace and review

Status: **draft for team review**, 9 November 2026. The current Phase 1 roadmap is the source of the newer demo scope. This record reconciles it with the older full-product documents; it does not assert that planned features are implemented. See the implementation baseline appendix below for historical implementation evidence and the [roadmap](api-demo-scrum-roadmap.md) for acceptance and dependencies.

## Decisions and traceability

| Capability or decision | Phase 1 target and boundary | Work and verification tickets |
| --- | --- | --- |
| Charity eligibility | A coordinator reviews an application, with actor, time and decision retained; Administrator inherits the coordinator permission. Only approved, active charities receive referrals. P1-10 owns the access boundary; P1-16 owns decision workflow and eligibility behavior. | [P1-02](https://github.com/Bola-j/Helping-Hands/issues/2), [P1-10](https://github.com/Bola-j/Helping-Hands/issues/10), [P1-16](https://github.com/Bola-j/Helping-Hands/issues/16), [P1-17](https://github.com/Bola-j/Helping-Hands/issues/17), [P1-44](https://github.com/Bola-j/Helping-Hands/issues/44) |
| Intake and role access | The demo beneficiary submits only their own request. Third-party requester and contributor flows remain full-app scope for later. Role and involvement checks protect each request and attachment. | [P1-09](https://github.com/Bola-j/Helping-Hands/issues/9)–[P1-11](https://github.com/Bola-j/Helping-Hands/issues/11), [P1-28](https://github.com/Bola-j/Helping-Hands/issues/28)–[P1-31](https://github.com/Bola-j/Helping-Hands/issues/31), [P1-44](https://github.com/Bola-j/Helping-Hands/issues/44) |
| Standards | A coordinator manages service categories and versioned units with structured components; historical records retain their original versions. New requests use current versions. The compatible physical mapping requires a reviewed future migration. | [P1-12](https://github.com/Bola-j/Helping-Hands/issues/12)–[P1-15](https://github.com/Bola-j/Helping-Hands/issues/15), [P1-51](https://github.com/Bola-j/Helping-Hands/issues/54) |
| Capacity and reminders | Each charity reports direct available-from-now capacity in platform units, per service/version and Cairo day. Zero is current but unavailable; stale services freeze only their own new assignments. The 08:00 Cairo reminder records one intent per charity/day; retries cannot claim guaranteed mailbox delivery. | [P1-18](https://github.com/Bola-j/Helping-Hands/issues/18)–[P1-26](https://github.com/Bola-j/Helping-Hands/issues/26), [P1-52](https://github.com/Bola-j/Helping-Hands/issues/55), [P1-53](https://github.com/Bola-j/Helping-Hands/issues/56) |
| Needs and referrals | A request can contain distinct needs, each tracked by service, standard version and quantity; coordinators can split allocations among eligible charities. Individual need and assignment balances are counted once. The earlier linked-child model is a candidate mapping, not an assertion that Phase 1 mapping is decided or implemented. | [P1-27](https://github.com/Bola-j/Helping-Hands/issues/27)–[P1-36](https://github.com/Bola-j/Helping-Hands/issues/36), [P1-53](https://github.com/Bola-j/Helping-Hands/issues/56), [P1-54](https://github.com/Bola-j/Helping-Hands/issues/58) |
| Evidence, progress and closure | Assigned representatives report outcomes and upload private evidence; coordinators accept or request clarification. Accepted amounts count once. An explicit coordinator closure is allowed only after all required needs are fulfilled and accepted. Acceptance is a review, not independent proof of delivery. | [P1-37](https://github.com/Bola-j/Helping-Hands/issues/37)–[P1-43](https://github.com/Bola-j/Helping-Hands/issues/43), [P1-55](https://github.com/Bola-j/Helping-Hands/issues/57) |
| Demo and later delivery | Fictional web-to-API, PostgreSQL, private storage and email rehearsal is Phase 1. Production deployment and the remaining first-version capabilities are tracked separately. | [P1-45](https://github.com/Bola-j/Helping-Hands/issues/45)–[P1-48](https://github.com/Bola-j/Helping-Hands/issues/48), [P1-56](https://github.com/Bola-j/Helping-Hands/issues/59), [P1-57](https://github.com/Bola-j/Helping-Hands/issues/60), [Phase 2 deployment](https://github.com/users/Bola-j/projects/5) |

## Phase 1 authorization summary

| Actor | Intended demo access |
| --- | --- |
| Beneficiary | Register, authenticate, submit their own needs and see their authorized progress. |
| Charity representative | Submit the charity application, manage that charity’s supported services and capacity, and report only assigned referrals with private evidence. |
| Coordinator | Review charity applications and requests; manage standards, match and allocate, review outcomes and explicitly close fulfilled requests. |
| Administrator | Manage platform access and inherit Coordinator permissions for coordinator actions. |
| Unrelated user | No access to another participant’s private records or evidence. |

## Scenarios to verify during implementation

- A pending or rejected charity cannot be allocated; an authorized coordinator can record a decision, while an unrelated user is denied.
- Intake confirmation and role/involvement checks prevent unauthorized requests and evidence access; a beneficiary can view only their own progress.
- A current update of zero differs from a stale service at Cairo midnight, and an update replaces availability without subtracting past commitments again.
- Multiple stale services create one daily reminder intent per charity; a failed or ambiguous SMTP attempt remains visible and does not prove delivery.
- Concurrent allocations and capacity updates preserve consistent balances. Split allocations cannot exceed need or capacity; only compatible standard versions match.
- An accepted partial outcome leaves unmet need open; repeated acceptance does not double count; incomplete needs prevent explicit closure.

These are acceptance scenarios, not executed test results.

## Open decisions and review

- [P1-33](https://github.com/Bola-j/Helping-Hands/issues/33): implement and test the interim exact-version rule for an outstanding old-standard need; further conversion policy awaits review.
- Physical mapping of needs, versioned standards and capacity remains to be designed and validated through the linked database tickets; the draw.io models depict the proposed Phase 1 design and identify unresolved mappings.
- Scrum Master review: **accepted orally in conversation on 11 November 2026**. This does not establish a separate team review or resolve outstanding diagram and design work.

# Appendix: Phase 1 implementation baseline and verification

Assessment date: 9 November 2026. This is a historical assessment of the local snapshot on that date, not a review of the complete demonstration. On 13 November 2026, the registration row was refreshed against the PR #82 candidate and its local software-test results; other entries retain their original assessment evidence. The assessed application changes are not part of the P1-01 documentation pull request. Phase 1 scope and ticket IDs are defined by the [Phase 1 project](https://github.com/users/Bola-j/projects/4). Source presence is not proof of runtime correctness. No shared databases were changed for these assessments.

## Environment and evidence

- The working tree already contained modified configuration, API and documentation, a deleted `.sln` replaced by an untracked `.slnx`, and untracked Compose, feature, migration and test files. These are not evidence of authorship or completion.
- SDK `10.0.204`; `dotnet build HelpingHands.slnx --nologo`: **passed**, zero warnings and zero errors.
- `dotnet test HelpingHands.slnx --no-build --nologo`: **passed**, 9 tests, zero failures and zero skips. Tests include HTTP pipeline and authorization/validation checks and stubbed S3 storage cleanup; they do not establish a complete database-backed workflow.
- `dotnet ef migrations has-pending-model-changes --project src/HelpingHands.Api --no-build`: **passed**, no pending model changes. This checks model/snapshot alignment, not migration application or an upgrade.
- Docker engine `29.8.2` is available. The configured `quay.io/minio/minio:RELEASE.2025-04-22T22-12-26Z` pull returned `401 Unauthorized`; an alternate same-tag Docker Hub pull also failed. Configured live S3 integration is therefore not verified. PostgreSQL 17 was pulled and started as a uniquely named ephemeral container with a loopback-only random port and fictional credentials. `dotnet ef database update --project src/HelpingHands.Api --no-build` succeeded against the empty isolated database; one migration-history row and 16 public tables were verified. The isolated container was removed; existing containers and volumes were untouched. This does not verify an upgrade, data recovery or full API workflow.

## Capability, gaps and follow-up

| Phase 1 capability / ticket | Observed implementation and evidence | Gap or unverified behavior / next work |
| --- | --- | --- |
| Baseline and local stack (P1-01, P1-05, P1-06, P1-46, P1-49) | `HelpingHands.slnx`, .NET 10 API, one `InitialDemo` migration and model snapshot, Dockerfile and Compose definitions for API, PostgreSQL 17, MinIO and bucket initialization. Build and snapshot check passed. | The empty PostgreSQL migration passed; application startup, migration upgrade, persistent-volume restart and recovery still need isolated verification. Configured MinIO image pull failed. No frontend or SMTP service in Compose; track P1-06, P1-25, P1-46 and P1-49. |
| Registration and identity (P1-09–P1-11, P1-50) | `main` exposes separate beneficiary and charity signup routes through merged PR #82. P1-09 is complete. The P1-10 candidate verifies login/logout and enforces Beneficiary self-service, active representative membership and Coordinator permissions inherited by Administrator; legacy `Requester` has no Phase 1 permission. Its full local suite passed 69/69 on 13 November against isolated PostgreSQL and private S3-compatible storage. | The P1-10 candidate is not yet merged; PR and post-merge CI evidence remain pending. P1-16 retains charity decision workflow and eligibility behavior. No web app is in the tracked main baseline. |
| Charity approval (P1-16, P1-17) | Recipient application and decision endpoints, records and history exist; review code requires `Admin`. A test checks that a requester cannot approve. | Phase 1 requires coordinator approval; reconcile and implement the agreed role policy. Pending/rejected eligibility and real authorization workflows remain unverified. |
| Services and standard versions (P1-12–P1-15, P1-51) | No service catalog or structured versioned standards were found among source routes or mapped entities. | Model, migrate, expose and display catalog, immutable versions and contents; test historical references and new-request rules. |
| Charity capacity and daily freshness (P1-18–P1-23, P1-53) | Existing referral code checks recipient approval and total assigned request quantity; it does not establish the Phase 1 charity service/version daily capacity model. | Add supported services, current-day quantities, Cairo-day freeze, locking and web screens; validate real concurrent operations. |
| Daily reminders (P1-24–P1-26, P1-52) | No reminder scheduler, SMTP adapter or local email catcher found in source and Compose inspection. | Add persistent reminder state, local catcher, SMTP adapter and scheduling with retry/ambiguous-result handling; verify Cairo time boundaries. |
| Multiple needs and review (P1-27–P1-31) | Beneficiary and request endpoints, authority/sharing checks, decision history and role/involvement filters are present; validation test rejects missing authority. | Request input represents an existing single request, not the documented multi-need catalog workflow. Verify self-only Phase 1 and implement missing need/version model and screens. |
| Matching and split referrals (P1-32–P1-36, P1-54) | Referral code checks approved active charity and quantity against the request; transactional PostgreSQL row-lock code exists. | No Phase 1 catalog/version/daily-capacity matching established; at assessment time the old-version policy P1-33 was open; the interim exact-version rule was selected subsequently. Prove lock behavior and competing operations against real PostgreSQL; no browser flow shown. |
| Progress and private evidence (P1-37–P1-41) | Request/referral/outcome and attachment routes, involved-user access checks, staged S3 storage and cleanup paths exist. Three storage tests use a stub client. | The configured MinIO image was unavailable; real upload, download and failure recovery have not passed. UI, cross-role access and privacy end-to-end need verification. |
| Outcome review and closure (P1-42, P1-43, P1-55) | Outcome submission requires an evidence attachment, and coordinator review handles accepted/clarification/unresolved decisions. Source sets request `Completed` automatically when accepted total equals requested quantity. | Phase 1 requires explicit coordinator closure of all fulfilled needs, so source and target differ. Test concurrency, no double-counting, review authorization and closure after requirements are reconciled. |
| Team, UI and full demo (P1-03, P1-04, P1-07, P1-44–P1-48, P1-57) | GitHub issues and project exist; Ziad access is recorded. No `package.json` or web project was found by repository file search. | Confirm remaining team access; implement screens and browser/API integration, broader access and workflow tests, a fictional-data rehearsal, and genuine individual contributions. |

## Open verification and handoff

- Do not mark a capability done based on this inventory. Recheck S3 availability and run isolated upgrade, recovery, representative API and storage verification; record actual results.
- The P1-10 role policy is now explicit: Administrator inherits Coordinator permissions; Phase 1 self-requesting uses `Beneficiary`; legacy `Requester` grants no Phase 1 access; third-party requesters remain later scope. Current automatic request completion remains a separate behavior to reconcile under P1-02/P1-44.
- At assessment time P1-33 was a product decision; an interim exact-version rule was selected subsequently. The Scrum Master accepted the assessment review; remaining blockers are tracked separately.
