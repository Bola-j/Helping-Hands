# Full-app Scrum roadmap

Status: forecast for the complete documented first version, not a delivery promise or evidence of completed functionality. **Phase 1 planning is superseded by the newer [Phase 1 Scrum backlog](api-demo-scrum-roadmap.md); the earlier sequence below remains historical context pending P1-02 reconciliation.** Each sprint is one week; dates, team capacity and velocity are unknown. Reorder and resize items during backlog refinement based on dependencies and actual capacity. The course demonstration uses fictional people, records and evidence; it is a milestone within the full app. The Phase 1 backlog covers the full web demo and does not create a second parallel sprint calendar.

Identity signup is currently under implementation in [P1-09](https://github.com/Bola-j/Helping-Hands/issues/9) and PR #82; its candidate replaces the legacy shared signup contract. Keep the sprint outcome forecasted until required CI passes and the change is merged.

| Sprint | Goal and candidate backlog | Demonstrable acceptance outcome |
| --- | --- | --- |
| 1 — Foundation | Local PostgreSQL, EF Core initial migration, API conventions, CI and development instructions. | An empty isolated database migrates successfully; API liveness and readiness distinguish process and database status. |
| 2 — Identity and access | Separate beneficiary and charity signup contracts, secure browser sessions, seeded roles, role/involvement authorization and initial administrator provisioning. | Beneficiary signup creates a linked profile; charity signup creates a pending application; fictional users sign in and denied users cannot access protected operations. |
| 3 — Charity onboarding | Private application attachments, coordinator decisions and responsive application/review screens for the application created during charity signup. | A fictional charity application moves through pending, rejected and approved states; only approved, active charities are eligible. |
| 4 — Request intake | Beneficiaries, requester and coordinator intake, authority/sharing confirmation and client forms. | Direct and third-party fictional requests are recorded; missing confirmations block submission. |
| 5 — Review and referral | Coordinator classification and decisions, eligible recipient list, first assignment and corresponding screens. | Coordinator reviews a request and assigns available need to an approved charity; ineligible assignment fails. |
| 6 — Outcome and evidence | Private evidence storage, outcome submission, review decisions and participant screens. | Assigned representative supplies evidence and coordinator accepts or requests clarification; unauthorized downloads fail. |
| 7 — Core demonstration | Integrate charity-to-referral flow, role-specific progress/history, audit and end-to-end checks. | Demonstrate the entire fictional charity workflow with authorization and decision history, without claiming independent proof. |
| 8 — Contributors | Contributor registration, coordinator approval and participant screens. | A contributor cannot receive a referral before approval and can afterwards. |
| 9 — Partial fulfillment | Multiple portions, balance calculations, explicit release, reassignment, concurrency tests and screens. | In the ten-box scenario, accepting three and releasing three preserves seven unresolved and makes exactly three available for reassignment. |
| 10 — Mixed needs | Split request creation, linked child tracking, non-allocatable parent and parent completion rules. | Children track separate needs without double counting; parent completes only when all are fulfilled and accepted. |
| 11 — Follow-up and disputes | Follow-up, reasoned unresolved closure, optional confirmation; validate and, if accepted, implement proposed later-dispute freezes and corrections. | Unmet need can close only with reason; an accepted dispute scenario is demonstrated only if its proposal has been validated. |
| 12 — Full-app readiness | Responsive and accessibility checks, end-to-end regression, deployment, backup/restore exercise, documentation and stakeholder review. | Full fictional-data workflows pass agreed checks; recovery is exercised and unresolved launch decisions are recorded. |

API CI is pulled forward into Phase 1; live deployment and frontend CI remain in the [Phase 2 deployment project](https://github.com/users/Bola-j/projects/5). Phase 1 workstream views remain in the [Phase 1 Scrum backlog](api-demo-scrum-roadmap.md). The [backend delivery breakdown](phase1-backend-delivery-breakdown.md) and [foundation milestone](phase1-foundation-milestone.md) describe the 12 November forecast; capacity and dependencies may require replanning.

## Team and collaboration

| Member | Primary contribution | Coordination |
| --- | --- | --- |
| Bola Ghaly | Backend API, application logic, integration and backend tests. | Agree contracts with UI and database owners; review authorization and failure behavior with the team. |
| Ziad Walid | Database design, migrations, constraints and system architecture. | Review model and transaction changes with backend; maintain structural and architectural documentation. |
| hussien okely | Web app UI and responsive user workflows. | Agree API states and integration with backend; review usability with ali fathy. |
| ali fathy | Web app UI and responsive user workflows. | Agree API states and integration with backend; review usability with hussien okely. |

All four participate in backlog refinement, planning, daily coordination, integration, testing, review and retrospective. The specialties do not assign Scrum Master or Product Owner roles; the team must agree who facilitates and who represents product priorities. Stakeholder and instructor participation depend on availability and are not implied to have happened.

## Sprint backlog and course evidence

The items below refine the forecast above into candidate sprint backlog slices. Each sprint produces a potentially reviewable increment and associated evidence, subject to actual capacity; none is a claim of completion. F and TC identifiers refer to the [PRD](PRD.md) and [SRS](SRS.md).

| Sprint | Candidate work by specialty | Review evidence and trace |
| --- | --- | --- |
| 1 | Bola: API setup and CI; Ziad: PostgreSQL model, first migration and constraints; UI team: navigation and page inventory. Agree development configuration and versioned contracts together. | Run the scaffold, migrate an isolated empty database and explain component/data models. E1, F-10; database integration checks. |
| 2 | Bola: authentication, session and authorization; Ziad: UUID identity/role mappings; UI team: sign-in and access-denied states. Define fictional role examples. | Show allowed and denied actions for distinct roles and involvement; record test cases and results. F-10, TC-007. |
| 3 | Bola: recipient application and decision endpoints; Ziad: recipient and decision history mappings; UI team: charity application and coordinator review. | Show pending, rejected and approved examples, validation and recorded decision. F-01, TC-001; registration use case. |
| 4 | Bola: beneficiary and request intake; Ziad: referential and authority constraints; UI team: beneficiary and third-party request forms. | Demonstrate normal intake, missing confirmation and permitted request progress. F-03, TC-003; UC-003. |
| 5 | Bola: review and referral operations; Ziad: transaction/eligibility and allocation design; UI team: coordinator review and assignment screens. | Demonstrate acceptance, reasoned rejection and one approved-charity referral; reject ineligible recipient. F-04/F-06, TC-004/TC-005. |
| 6 | Bola: protected uploads, outcome and review operations; Ziad: evidence/review persistence; UI team: upload and review screens. | Demonstrate delivery/receipt evidence, clarification and acceptance; reject unrelated access. F-07/F-10, TC-006/TC-007. |
| 7 | Integrate the charity workflow and role-specific history; all members repair cross-feature issues and prepare sample data, test evidence and a walkthrough. | Review fictional end-to-end increment against core requirements; capture actual feedback, known gaps and actions. F-01/F-03/F-04/F-06/F-07/F-09/F-10. |
| 8 | Bola: contributor application and approval; Ziad: recipient subtype integrity; UI team: contributor registration and coordinator review. | Show pending contributor denial and approval before assignment. F-02, TC-002. |
| 9 | Bola: partial outcomes, release and reassignment; Ziad: locking and balance verification; UI team: separate assigned, remaining and available amounts. | Run ten-box example and competing assignments on real PostgreSQL; inspect preserved history. F-08, TC-005. |
| 10 | Bola: split and linked progress; Ziad: link uniqueness and cycle prevention; UI team: split and child status views. | Show mixed needs split without duplicate counting; prevent parent allocation and premature completion. F-05, TC-004. |
| 11 | Bola: follow-up, closure and confirmation; Ziad: history and constraints; UI team: unresolved work and closure states. Validate later-dispute proposal before implementation. | Show reasoned unresolved closure and optional confirmation; record the dispute decision and test any accepted extension separately. F-09, TC-008; proposed TC-009. |
| 12 | All members integrate, execute regression and role/access tests, inspect responsive accessibility, rehearse deployment and restoration, and update user/developer documentation. | Demonstrate full agreed first-version workflows with fictional data; present test evidence, known issues, recovery exercise and remaining operational decisions. E8; TC-001–TC-008. |

## Applying the course material

The slides in `D:/Downloads/swe` describe an initial outline planning and architecture phase, iterative sprint cycles, and project closure (Lecture 2, PDF pp. 30–35). The project already has requirements and models; Sprint 1 refreshes those artifacts before implementation. Sprints 1–12 are proposed one-week timeboxes selected by the team, shorter than the lecture's typical two-to-four-week example; no course mandate for one week is inferred. Sprint 12 includes closure activities and lessons learned, while review and retrospective happen every sprint.

Lecture 3 distinguishes user and system requirements, functional and non-functional requirements, and scenarios with starting conditions, normal and exceptional paths, concurrency and end state (PDF pp. 6–13, 35, 40–45). Refine each selected story into testable acceptance criteria and trace it to PRD features, SRS requirements/use cases and planned test cases. Record actual execution separately from proposed scenarios; update affected requirements and models when decisions change.

Lecture 5 distinguishes context, interaction, structural and behavioral views (PDF pp. 5–8, 11–12, 16–18, 27, 39–47). Use the existing [models](models.md) and [database diagrams](database-design.md) during review, updating them when implementation changes their meaning. Lecture 1 p. 6 mentions specifications, requirements, team management, Scrum, test cases and UML in project assessment; the slides do not supply a separate detailed grading rubric or guarantee that these artifacts meet instructor expectations.

### Example sprint planning and review

For Sprint 5, the candidate story is: *As a coordinator, I want to assign an accepted request to an approved charity so that responsibility is clear.* Preconditions are an authorized coordinator, accepted allocatable need and an active approved charity. The normal path selects an available amount, records the referral and history, and displays the new assignment. Attempting an ineligible charity or exceeding the available amount must fail without a partial record; concurrent attempts must not over-allocate. At review, show the successful and rejected cases with fictional data and trace results to F-06 and TC-005. The team decides the actual sprint selection and estimate during planning.

## Scrum practice and completion

Maintain one ordered product backlog with priority, story, acceptance criteria, dependencies and estimates chosen by the team. At sprint planning, select a feasible subset, agree a sprint goal and break stories into tasks. In short daily coordination each member shares progress, next work and blockers; track decisions and adjust tasks without pretending unfinished work is done. At sprint review demonstrate the increment to available stakeholders, record actual feedback and adapt the backlog. At retrospective select an owner and practical improvement for the next sprint. A story is done when its acceptance criteria and relevant automated checks pass, authorization and failure paths are reviewed, documentation is updated, and the behavior can be demonstrated with fictional data. Incomplete items return to the backlog; no sprint is automatically considered complete.

A real operational launch requires validated participant procedures, hosting, identity and access administration, file safety and retention, backup and recovery, stakeholder input and appropriate local review. Payment handling, inventory, delivery logistics and charity accounting remain outside the documented first version. Future funding ideas are not validated product commitments.
