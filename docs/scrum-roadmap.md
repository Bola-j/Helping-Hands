# Full-app Scrum roadmap

Status: forecast for the complete documented first version, not a delivery promise or evidence of completed functionality. Each sprint is one week; dates, team capacity and velocity are unknown. Reorder and resize items during backlog refinement based on dependencies and actual capacity. The course demonstration uses fictional people, records and evidence; it is a milestone within the full app.

| Sprint | Goal and candidate backlog | Demonstrable acceptance outcome |
| --- | --- | --- |
| 1 — Foundation | Local PostgreSQL, EF Core initial migration, API conventions, CI and development instructions. | An empty isolated database migrates successfully; API liveness and readiness distinguish process and database status. |
| 2 — Identity and access | Identity, secure browser sessions, roles, role/involvement authorization and initial administrator provisioning. | Fictional users sign in and denied users cannot access protected operations. |
| 3 — Charity onboarding | Charity application, private application attachments, administrator decisions and responsive application/review screens. | A fictional charity applies; only an administrator approves it; pending/rejected charities are ineligible. |
| 4 — Request intake | Beneficiaries, requester and coordinator intake, authority/sharing confirmation and client forms. | Direct and third-party fictional requests are recorded; missing confirmations block submission. |
| 5 — Review and referral | Coordinator classification and decisions, eligible recipient list, first assignment and corresponding screens. | Coordinator reviews a request and assigns available need to an approved charity; ineligible assignment fails. |
| 6 — Outcome and evidence | Private evidence storage, outcome submission, review decisions and participant screens. | Assigned representative supplies evidence and coordinator accepts or requests clarification; unauthorized downloads fail. |
| 7 — Core demonstration | Integrate charity-to-referral flow, role-specific progress/history, audit and end-to-end checks. | Demonstrate the entire fictional charity workflow with authorization and decision history, without claiming independent proof. |
| 8 — Contributors | Contributor registration, coordinator approval and participant screens. | A contributor cannot receive a referral before approval and can afterwards. |
| 9 — Partial fulfillment | Multiple portions, balance calculations, explicit release, reassignment, concurrency tests and screens. | In the ten-box scenario, accepting three and releasing three preserves seven unresolved and makes exactly three available for reassignment. |
| 10 — Mixed needs | Split request creation, linked child tracking, non-allocatable parent and parent completion rules. | Children track separate needs without double counting; parent completes only when all are fulfilled and accepted. |
| 11 — Follow-up and disputes | Follow-up, reasoned unresolved closure, optional confirmation; validate and, if accepted, implement proposed later-dispute freezes and corrections. | Unmet need can close only with reason; an accepted dispute scenario is demonstrated only if its proposal has been validated. |
| 12 — Full-app readiness | Responsive and accessibility checks, end-to-end regression, deployment, backup/restore exercise, documentation and stakeholder review. | Full fictional-data workflows pass agreed checks; recovery is exercised and unresolved launch decisions are recorded. |

## Scrum practice and completion

Maintain one ordered product backlog, refine stories and dependencies throughout, and select work at each sprint planning based on actual capacity. Agree a sprint goal; coordinate daily; demonstrate completed work and record feedback at review; choose an improvement at retrospective. A story is done when its acceptance criteria and relevant automated checks pass, authorization and failure paths are reviewed, documentation is updated, and the behavior can be demonstrated with fictional data. Incomplete items return to the backlog; no sprint is automatically considered complete.

A real operational launch requires validated participant procedures, hosting, identity and access administration, file safety and retention, backup and recovery, stakeholder input and appropriate local review. Payment handling, inventory, delivery logistics and charity accounting remain outside the documented first version. Future funding ideas are not validated product commitments.
