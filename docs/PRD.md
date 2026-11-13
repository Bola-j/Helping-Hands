# Helping Hands — Product Requirements Document

## 1. Document control

| Field | Value |
| --- | --- |
| Version | 1.1 |
| Status | Working product baseline; Phase 1 review accepted by the Scrum Master; diagram reconciliation and external validation remain open |
| Authors | Bola Ghaly, Ziad Walid, hussien okely and ali fathy |
| Business source | [Business Requirements Document v1.4](BRD.md), working baseline with owner delegation; Scrum Master review accepted |
| Related specification | [Software Requirements Specification](SRS.md); detailed system requirements and models are maintained there |

| Version | Change |
| --- | --- |
| 0.1 | Initial traceable product requirements and proposed Agile plan. |
| 1.0 | Finalized the working product baseline from BRD 1.3; retained open decisions and proposed validation and planning details. |
| 1.1 | Reconciled current Phase 1 scope and coordinator charity approval; Scrum Master review accepted on 11 November 2026. |

This working baseline applies the project owner’s request to finalize the requirements and delegation of remaining decisions. The owner validated an earlier BRD; this does not mean each product rule was individually approved. No instructor, charity, or other stakeholder approval, field validation, implementation, or test result is implied by this document.

Course reference locations below are PDF page numbers, not necessarily printed slide numbers: `D:/Downloads/swe/Lecture 1.pdf`, `Lecture 2.pdf`, `Lecture 3.pdf`, and `Lecture 5_new (Part1 and Part2).pdf` (also supplied as `(1)`); the latter copies have the same page count and extracted content. These are local source materials, not deliverables. Lecture 4 and a separate instructor submission rubric were not supplied; no missing content is inferred.

## 2. Product overview

**Vision:** Give authorized participants a clear, accountable record of assistance requests and referrals while the parties arrange and provide assistance outside the platform.

**Problem:** Requests, offers and updates may be spread across calls, visits and messages, making responsibility and unresolved needs difficult to track. The particular current process and scale at a real charity have not been verified.

**Product:** A responsive web application backed by a single API records participant approvals, requests, referral portions, decisions, reported outcomes, supporting evidence and follow-up. The beneficiary receives the assistance; a referral recipient is the approved charity or individual contributor assigned to provide it. The platform neither handles nor takes a share of aid. The course demonstration uses fictional people and evidence.

**Value and objectives:** Authorized participants can see relevant progress (BO-01); coordinators connect needs to approved recipients (BO-02); decisions and reported evidence support accountability without claiming independent proof (BO-03); access is limited by role and involvement (BO-04); and the team can seek feedback on usefulness and sustainability (BO-05). Proposed future funding through grants, sponsorship or subscriptions is unvalidated and billing is not a course feature.

## 3. Stakeholders and roles

| Role | Interest and responsibilities |
| --- | --- |
| Beneficiary | Receives assistance, may submit their own request and needs appropriate privacy and progress visibility. |
| Third-party requester | Submits for another person, confirms authority and necessary sharing, and sees only what is needed to follow that request. |
| Platform administrator | Manages platform access and inherits Coordinator permissions for coordinator actions. |
| Platform coordinator | Reviews charity applications, reviews and classifies requests, approves contributors, assigns and follows portions, reviews outcomes, and closes requests. |
| Charity | Applies with organization, contact and registration information or documentation; once approved and activated, handles assigned referrals and supplies evidence. |
| Individual contributor | Registers with contact information; once approved by a coordinator, may handle assigned referrals and supply evidence. |
| Project team and instructor | Build and assess the course project; no instructor approval is recorded. |
| Potential sponsor or funding partner | Possible future service supporter; participation and model are unconfirmed. |

Role access is constrained by involvement and minimum necessary information, not merely by possessing an account. The coordinator or requester is not necessarily the beneficiary.

## 4. Scope and priorities

The full first version includes the feature groups below and BRD IS-01–IS-08. Priorities are delivery order, not removal from scope. The Phase 1 demonstration includes separate beneficiary and charity signup flows: beneficiary signup creates an account and linked profile, while charity signup creates an account and pending, inactive application for coordinator review. It also includes charity approval, beneficiary self-submission of multiple needs, review, split referrals to approved charities, versioned standards, daily charity capacity and reminders, private evidence and review, authorized progress and explicit closure. Contributor approval, third-party intake and release/reassignment are later scope. A split allocation in Phase 1 does not imply release or reassignment is implemented. The selected signup contract is implemented on `main` by merged PR #82; charity decision policy remains tracked separately in P1-16. The team and instructor should validate the feasible demonstration. The team is targeting 12 November 2026 for the Phase 1 backend, PostgreSQL, local containers and API CI, as a forecast rather than a delivery commitment; capacity and dependency availability remain unconfirmed.

Excluded: custody, purchasing, processing, storage or distribution of money or goods; warehouse and inventory management; delivery logistics; charity accounting; taking a percentage of assistance; subscription billing and payment integration in the course version; detailed service scheduling, staffing or supervision; real operations or real donations in the course demonstration; and public listings or dashboards of requests. Existing implementation and verification must be assessed against the capability matrix; this scope is not a claim that the full workflow is implemented.

### Backend delivery milestones

The first proposed milestone is a reproducible local API foundation: reviewed PostgreSQL migrations, protected S3-compatible storage, development configuration and an API CI baseline. It is a prerequisite for complete workflow and integration verification, not proof of a finished demonstration. Later milestones cover identity and approvals; standards, capacity and requests; allocation, evidence, reminders and closure; and final verification. See the [backend delivery breakdown](phase1-backend-delivery-breakdown.md) for ticket dependencies, checks, the 12 November forecast and the explicitly partial fallback. The existing [demo roadmap](api-demo-scrum-roadmap.md) retains the web and overall Phase 1 scope.

## 5. Product capabilities and acceptance criteria

Priorities: **Core** = first demonstration slice; **Next** = later first-version increment. Rows covering both use the Phase 1-specific requirements below to distinguish scope. Each item is required for the full first version. Acceptance criteria describe intended behavior, not completed work. Role access, history and fictional data apply throughout.

| ID / priority | Capability, users and value | Dependencies | Acceptance criteria | BRD trace |
| --- | --- | --- | --- | --- |
| F-00 Core | Beneficiary and charity account signup: gives each participant a distinct entry flow. Users: beneficiary, charity representative. | P1-08 session and antiforgery contract. | Separate signup routes create UUID-backed accounts with globally unique email. Beneficiary signup assigns `Beneficiary` and creates a linked profile; charity signup assigns `RecipientRepresentative` and creates a pending, inactive charity application and membership. Signup does not authenticate the participant or verify email. | IS-01, IS-02; BR-01, BR-03 |
| F-01 Core | Charity application review: establishes eligibility and accountability. Users: charity, coordinator. | F-00; coordinator access. | The charity application created at signup includes organization name, contact details and optional registration information or later supporting documentation; coordinator may leave pending, reject or approve and activate; pending/rejected charity cannot receive referrals; decisions are recorded. | IS-01; BR-01, BR-02; BRule-01, BRule-10; BO-02, BO-03 |
| F-02 Next | Individual contributor registration and coordinator approval: enables another referral recipient. Users: contributor, coordinator. | Identity/access. | Contributor registers with contact information; referral eligibility begins only after coordinator approval; pending contributor cannot be assigned. | IS-08; BR-10; BRule-11; BO-02, BO-03 |
| F-03 Core | Request submission by beneficiary, third party or coordinator on behalf: captures needs and authority. Users: requester, beneficiary, coordinator. | F-00 identity; necessary information and sharing confirmation. | Phase 1 beneficiary submits their own request using the linked profile. Third-party and coordinator-on-behalf intake remain later full-app scope. The submitter confirms authority and necessary sharing; request is available to authorized review and requester can follow it. | IS-02; BR-03; BRule-07, BRule-12; BO-01, BO-04 |
| F-04 Core | Request review and classification: supports a reasoned referral decision. Users: coordinator, requester. | F-03. | Coordinator may request clarification, accept for referral or reject with reason; classifies need as goods, money or organizational referral, whose subtypes include services such as tutoring, repairs and transport; decision and actor/time are retained. | IS-03; BR-04; BRule-02, BRule-09; BO-01, BO-03 |
| F-05 Core | Separate mixed needs for referral: makes distinct assistance types independently trackable. Users: coordinator. | F-04. | Coordinator can separately review and refer mixed needs by support type; related needs preserve original context without double-counting fulfillment. | IS-04; BR-04; BRule-04; BO-01 |
| F-06 Core | Refer accepted requests or portions: establishes responsibility for providing help outside the platform. Users: coordinator, approved charity or contributor. | F-01 or F-02, F-04; F-08 for later release and reassignment. | Any need category may be referred to an approved recipient; multiple recipients can each have a separately tracked portion; the same assigned portion cannot be simultaneously assigned twice; only information needed by the recipient is shared. | IS-04, IS-05; BR-05; BRule-02–04, BRule-07, BRule-12, BRule-14; BO-01, BO-02 |
| F-07 Core | Record and review outcome and evidence: supports accountable reporting. Users: recipient, coordinator. | F-06; secure attachment access. | Responsible recipient supplies an outcome description and attachment(s) addressing delivery and receipt; monetary outcomes include amount and evidence of transfer and receipt; coordinator can request clarification or accept the report. Acceptance is a review, not independent proof. Independent beneficiary confirmation is not required for the course demo; optional confirmation and disagreement are recorded separately; disputed or insufficient outcomes remain unresolved. | IS-06; BR-06, BR-10; BRule-05, BRule-08, BRule-13; BO-03, BO-04 |
| F-08 Next | Track partial fulfillment and release/reassignment: prevents double assignment and preserves history. Users: coordinator, recipient. | F-06, F-07. | Remaining need includes assigned and unassigned unresolved need; only unassigned or explicitly released need is available for assignment. An accepted partial outcome records fulfillment without completing the whole referral; undelivered portion stays assigned until released, and release retains referral and outcome history. Non-quantified services use separately trackable scopes. | IS-05; BR-05, BR-07; BRule-14; BO-01, BO-02, BO-03 |
| F-09 Core / partial follow-up Next | Progress, follow-up and closure: lets participants understand what happens next. Users: authorized requester, coordinator and recipient. | F-03, F-06, F-07. | Relevant progress and unresolved/declined portions remain visible for follow-up. A coordinator explicitly closes a request as complete only when all required need is fulfilled and accepted; it remains open while further action is expected, or can close unresolved with a reason when work stops. | IS-07; BR-07; BRule-09, BRule-14; BO-01, BO-03 |
| F-10 Core | Involvement-based access and significant-action history: protects sensitive information and preserves decisions. Users: all authorized roles. | Identity/access and records. | Coordinators and administrators (who inherit coordinator permissions) access information needed for their work; representatives see only records connected to active memberships and assigned referrals; Phase 1 beneficiaries see their own requests; third-party requesters are later scope. Significant decisions and updates record actor and time; unrelated users cannot access restricted data. | IS-07; BR-07, BR-08; BRule-07–09, BRule-12; BO-03, BO-04 |

### Phase 1-specific product requirements

- A beneficiary submits their own request with one or more separately tracked needs; third-party and coordinator-on-behalf intake remain full-product capabilities after Phase 1. Requests retain authority and necessary-sharing confirmations.
- Coordinators create and version service categories and unit standards with structured component names, quantities and measurement units. New requests use the current version; historical records retain their original version. For an outstanding need on older versions, [P1-33](https://github.com/Bola-j/Helping-Hands/issues/33) requires an exact-version match until the policy is reviewed; leave the need open when none is eligible.
- Charities select supported services and provide nonnegative platform-unit availability from now; an update replaces available balance without changing earlier commitments. A service/version without an update for the current Africa/Cairo date is frozen for new referrals; a current zero is distinct from stale.
- At 08:00 Cairo, create one daily reminder intent per charity with stale services; expose an in-app task, record outcomes and handle retries or ambiguous SMTP sends without claiming mailbox delivery or exactly-once external delivery.
- Coordinators match approved active charities by compatible service/version and available capacity, assign needs in portions without double assignment or oversubscription, and retain progress and decision history. Assigned recipients submit a report and private attachment; coordinators review and explicitly close only when all required needs are fulfilled and accepted.

These are Phase 1 targets and acceptance criteria, not evidence of implemented behavior or stakeholder validation. See the [demo backlog](api-demo-scrum-roadmap.md) for dependencies and the [SRS](SRS.md) for testable scenarios.

## 6. User journeys and workflow

1. **Registration:** A charity submits its organization and registration information. An authorized coordinator leaves it pending, approves and activates it, or rejects it. Separately, a contributor submits contact information; a coordinator approves eligibility. Neither pending participant can receive a referral.
2. **Submission:** A beneficiary or authorized third party submits a need, or a coordinator enters it on their behalf. The requester confirms authority and necessary sharing; the system retains the request for review.
3. **Review:** A coordinator examines the request, asks for clarification when necessary, accepts it for referral or rejects it with a reason, and classifies the need. Mixed categories can be tracked separately without losing their relationship; their physical representation is subject to design review.
4. **Referral:** The coordinator chooses an approved charity or contributor, assigns the whole available need or a separately tracked portion, and shares only necessary information. Aid is arranged and delivered outside the platform.
5. **Outcome review:** The assigned recipient reports what was provided and attaches evidence addressing delivery and receipt. The coordinator accepts the report, asks for clarification or records it unresolved. A report or acceptance is not independent proof. Any separate confirmation or disagreement is recorded; disputes remain unresolved for follow-up. The SRS describes proposed handling of a disagreement received after acceptance; its allocation freeze and correction details require design review.
6. **Partial and follow-up:** Accepted fulfillment reduces unresolved need. The remainder of that referral stays assigned until explicitly released. The coordinator follows up on declined, insufficient, disputed or incomplete outcomes and may reassign only unassigned or released need. For 10 boxes, assigning 6 and 4, then accepting 3 from the first recipient leaves 7 unresolved, all assigned; releasing the first recipient's undelivered 3 makes only those 3 available for reassignment. History stays intact.
7. **Completion or closure:** Complete the request only after all required need is fulfilled and accepted. Keep it open if further action is expected, or close it unresolved with a recorded reason if work stops with unmet need.

## 7. Representative user stories

- As a charity applicant, I want to see whether my application is pending, approved or rejected so I understand my eligibility.
- As a third-party requester, I want to submit a need with appropriate authority and follow its progress without seeing unrelated private evidence.
- As a coordinator, I want to refer a defined available portion to an approved recipient so responsibility is clear and no portion is assigned twice.
- As a recipient, I want to report a partial delivery and attach supporting evidence so the coordinator can review what I provided.
- As a coordinator, I want to release an undelivered portion while retaining its history so another recipient can be assigned safely.
- As a beneficiary, I want my information shared only where needed so my privacy is respected.

## 8. Success and validation

| Criterion | Proposed check, not a completed result | BRD |
| --- | --- | --- |
| Eligibility | Demonstrate pending, rejected and approved/activated charities and pending versus approved contributors with sample accounts; attempt prohibited assignments. | SC-01, SC-07 |
| End-to-end referral | Demonstrate a direct and a third-party fictional request through review and referral to eligible recipient types. | SC-02 |
| Mixed and partial needs | Demonstrate related needs, separately assigned portions and accepted partial outcome; inspect history and remaining versus available need. Release and reassignment are later scope. | SC-03 |
| Evidence and review | Demonstrate fictional goods, money and service outcomes, attachments, clarification and acceptance; check that acceptance is not represented as independent proof. | SC-04 |
| Privacy | Exercise allowed and denied access for roles and involvement, including unrelated evidence. | SC-05 |
| Practical usefulness | Seek feedback on usability, operating arrangements and sustainability from prospective participants where available; record what was learned and unresolved issues. This is a validation goal, not a prerequisite for course demonstration. | SC-06 |

Potential measures include requests awaiting review, review time, referrals awaiting outcome, portions reported fulfilled and evidence awaiting review. Definitions, baselines, targets and collection methods are not agreed; no improvement is assumed. Stakeholder feedback, evidence review standards, participant willingness and real operating arrangements are unvalidated.

## 9. Planning, assumptions and risks

**Assumptions:** Participants can arrange assistance externally and recipients can provide meaningful evidence (BRD A-01–A-02). These require practical validation. Service scopes can be defined independently when numerical quantities are inappropriate (BRule-14).

**Constraints:** The course project uses fictional data, aligns with applicable university requirements, uses .NET 10, PostgreSQL, code-first development with EF Core migrations, Docker and one monolithic API, and intends a React Native-based web client with separately configured PWA capabilities. No real operational pilot is included. Client platform compatibility is a proposed support decision, not a universal promise; see SRS for technical evidence, support matrix and limitations.

**Dependencies:** Team and instructor clarification of course deliverables; identity and access design; a usable evidence storage approach; eventual review by prospective participants and local legal/operational experts before real operation (BRD D-01–D-03). No legal compliance or partner commitment is asserted.

| Risk | Proposed response |
| --- | --- |
| Evidence may be misleading or insufficient; approval does not guarantee future conduct. | Record the reporting and review distinction, define review criteria and escalation with partners. |
| Sensitive beneficiary data or attachments may be exposed. | Minimize data and restrict access; agree consent, retention, incident and privacy practices before real use. |
| A recipient may not respond or fulfill its portion. | Keep unresolved work visible, follow up, release and reassign when appropriate. |
| Full scope may exceed available course capacity. | Sequence a thin end-to-end core first; review scope and demonstration with team and instructor without deleting first-version requirements. |
| Frontend PWA and device behavior may differ. | Prototype and verify supported browser/device combinations; decide capabilities and fallbacks before implementation. |

**Open decisions:** exact identity and access onboarding; evidence formats, limits, storage, retention, review criteria and consent language; service-scope representation; supported browsers and PWA installation behavior; notification and accessibility acceptance details; recovery and measurable performance targets; operational partner, funding and local legal arrangements. These must be confirmed without weakening BRD rules. No dates or individual approvals are inferred.

## 10. Product backlog and Scrum-inspired approach

The following is an illustrative ordered backlog, not a commitment or estimate. Items should be refined into small testable slices by the team; a review can reorder them without changing the agreed scope.

| Epic / sequence | Example backlog stories or deliverables | Related features |
| --- | --- | --- |
| E1 Foundation and privacy | Establish local runnable API and fictional-data workflow; define identity, roles, authorization and history. | F-10; supports all |
| E2 Charity participation | Charity applies; coordinator reviews pending, rejects or approves and activates. | F-01 |
| E3 Request intake and review | Requester submits with authority; coordinator classifies and requests clarification, accepts or rejects. | F-03, F-04 |
| E4 Core referral and evidence | Coordinator refers to approved charity; recipient reports and attaches evidence; coordinator reviews; authorized users follow progress. | F-06, F-07, F-09, F-10 |
| E5 Contributor participation | Contributor registers and coordinator approves before assignment. | F-02, F-06 |
| E6 Portions and follow-up | Track separate assignments, partial outcomes, unresolved versus available need, release and reassignment. | F-06, F-08, F-09 |
| E7 Mixed needs | Separately track mixed categories and their progress while preserving their relationship to the original request. | F-05, F-09 |
| E8 Validation and readiness | Review fictional demonstrations, representative access tests, usability and stakeholder feedback; identify remaining launch decisions. | F-01–F-10, BO-05 |

Illustrative cycle: maintain a single ordered product backlog; at sprint planning choose a small coherent increment based on the team's actual capacity and dependencies, agree a sprint goal and acceptance checks, and break stories into tasks. During a sprint hold brief coordination meetings and update progress and blockers. At review, demonstrate working behavior against acceptance criteria with available stakeholders and record feedback without inventing it. At retrospective, select practical improvements to team process. Refine and re-prioritize the backlog between cycles. One-week sprint goals and candidates are forecast in the [full-app Scrum roadmap](scrum-roadmap.md); individual specialties are recorded in the roadmap; Scrum roles, capacity and dates are not set. A proposed definition of done for a business story is reviewed requirements, implemented behavior, relevant passing checks including access and error paths, updated documentation and a demonstrable fictional-data scenario; adapt it with the team.

**Illustrative first sprint:** A possible goal is to establish a runnable foundation and agree the first thin charity-to-referral slice. Candidate items are the local API health endpoint and development instructions, refinement of charity application and request-intake stories, and an initial role and access review using fictional examples. At planning, select only items the team judges feasible and define their acceptance checks; during the sprint, discuss progress and blockers and refine the slice. At review, demonstrate whichever foundation items are actually complete and walk through the proposed fictional workflow, recording feedback and gaps. The retrospective identifies a process improvement for the next cycle. This example is not a delivery commitment and does not imply the business workflow has been implemented.

## 11. Product-to-business traceability

| BRD requirement | Product features | Objectives and governing rules |
| --- | --- | --- |
| BR-01 charity application and review | F-01 | BO-02, BO-03; BRule-01, BRule-10 |
| BR-02 approved/activated charity only | F-01, F-06 | BO-02, BO-04; BRule-01 |
| BR-03 request submission/tracking | F-03, F-09 | BO-01; BRule-07, BRule-09, BRule-12 |
| BR-04 review, categories, related needs | F-04, F-05 | BO-01, BO-03; BRule-02, BRule-04 |
| BR-05 referral and portions | F-06, F-08 | BO-01, BO-02; BRule-03, BRule-04, BRule-14 |
| BR-06 outcome and evidence | F-07 | BO-03; BRule-05, BRule-08, BRule-13 |
| BR-07 relevant progress and history | F-08, F-09, F-10 | BO-01, BO-03, BO-04; BRule-07, BRule-09, BRule-14 |
| BR-08 restricted access | F-10 | BO-04; BRule-07, BRule-12 |
| BR-09 stakeholder feedback | E8 and section 8; validation activity, not a system feature | BO-05; D-02, D-03 |
| BR-10 contributor approval and outcome review | F-02, F-07 | BO-02, BO-03; BRule-05, BRule-11 |

The SRS maintains the detailed BRD → feature → user/system requirement → use case → verification mapping. BO-05 cannot be satisfied by a software function alone; feedback and operational validation remain open activities.

## 12. Course alignment

| Source (PDF page) | Concept | Application here / related work |
| --- | --- | --- |
| Lecture 1 pp. 6, 17 | Project assessment mentions specifications, requirements, team management, Scrum, test cases and UML; software activities include specification, development and validation. | PRD and SRS define scope, traceability and validation; backlog describes team process; SRS supplies models and scenarios. The lecture does not establish a specific project rubric beyond its statement. |
| Lecture 2 pp. 3–4, 10–12, 19–21 | Process activities, incremental Agile planning and prototyping/feedback. | Iterative feature delivery, review and product validation; a prototype is not represented as a fully validated production system. |
| Lecture 2 pp. 30–35 | Scrum, backlog, sprint cycle, team coordination. | Section 10 proposes a backlog, sprint planning, coordination, review and retrospective without asserting team capacity or dates. |
| Lecture 3 pp. 6–13, 35, 40–45 | User and system requirements; functional and non-functional requirements, precision, structured specifications, elicitation and scenarios/use cases. | Product features and acceptance criteria lead to individually identified requirements and scenarios in SRS; outstanding stakeholder discovery is explicit. |
| Lecture 5 pp. 5–8, 11–12, 16–18, 27, 39–47 | Context, interaction, structural and behavioral views; use cases, sequences, classes, activities and state machines. | SRS proposes corresponding models and describes system boundary and behavior; models are proposals, not evidence of implemented workflows. |

Page references identify relevant course content; they do not imply instructor endorsement of project-specific design decisions.

