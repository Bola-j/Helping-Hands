# React JS Web and Flutter Mobile Scrum Plan

Status: **approved beneficiary-only Flutter scope for Phases 1 and 2**. The React JS foundation is merged; the Flutter app and device evidence remain to be implemented. The web app is a browser site without PWA features.

## Team responsibility

**Hussien Okely** (`HussienOkely`) — **Cross-Platform Mobile App Dev**. He owns Flutter tickets. Ali owns the React JS web UI; Bola owns completed JavaScript tooling issue #148. Assignment is not code contribution evidence.

## Phase 1 — working beneficiary demo (Project #4)

The first demo follows one path: create an account, sign in, submit one single-need request, see its confirmation, and view the beneficiary's own requests. English, Arabic, and RTL are part of the planned UI.

| Ticket | Work | Dependency |
| --- | --- | --- |
| [P1-MOB-03 #151](https://github.com/Bola-j/Helping-Hands/issues/151) | Bootstrap Flutter for Android/iOS, shared theme, navigation, UI states, and English/Arabic RTL. | API/platform setup |
| [P1-MOB-01 #139](https://github.com/Bola-j/Helping-Hands/issues/139) | Verify native cookie session, antiforgery, expiry, denied and unavailable states. | #151; API session contract |
| [P1-MOB-04 #152](https://github.com/Bola-j/Helping-Hands/issues/152) | Account creation with name, Egyptian mobile, governorate, password and bilingual validation. | #139, #151 |
| [P1-MOB-05 #153](https://github.com/Bola-j/Helping-Hands/issues/153) | Sign-in, sign-out and session-expired flows. | #139, #151 |
| [P1-MOB-06 #154](https://github.com/Bola-j/Helping-Hands/issues/154) | Beneficiary home and own-request list, including loading, empty and error states. | #153; own-request API |
| [P1-MOB-07 #155](https://github.com/Bola-j/Helping-Hands/issues/155) | Single-need submission, both confirmations, progress/recovery and server-confirmed result. | #139, #153; request API |
| [P1-MOB-08 #156](https://github.com/Bola-j/Helping-Hands/issues/156) | Flutter formatting, analysis, unit/widget checks and Android CI build; record iOS coverage. | Initial beneficiary flow |
| [P1-MOB-09 #157](https://github.com/Bola-j/Helping-Hands/issues/157) | Reproducible fictional-data demo and actual platform/contribution evidence. | #151–#156 |
| [P1-MOB-02 #140](https://github.com/Bola-j/Helping-Hands/issues/140) | Umbrella acceptance for the full beneficiary journey. | #139, #151–#157 |

Closed Expo issue #7 is historical web work. #138 and completed #148 remain GitHub prerequisites for downstream UI tickets. Android/iOS execution, app CI, and live API behavior are unverified until recorded on their tickets.

## Phase 2 — additional features and engineering evidence (Project #5)

| Ticket | Work | Dependency |
| --- | --- | --- |
| [P2-MOB-05 #146](https://github.com/Bola-j/Helping-Hands/issues/146) | Catalog-based multi-need wizard with current standards, review, confirmations and failure recovery. | Phase 1 flow; catalog/API |
| [P2-MOB-06 #147](https://github.com/Bola-j/Helping-Hands/issues/147) | Beneficiary request details, progress and history with own-record access. | Phase 1 flow; progress API |
| [P2-MOB-07 #158](https://github.com/Bola-j/Helping-Hands/issues/158) | Accessibility, text scaling, English/Arabic layout and RTL review. | Completed screens |
| [P2-MOB-08 #159](https://github.com/Bola-j/Helping-Hands/issues/159) | Mobile requirements trace and current use case, sequence, component and deployment models. | Implemented behavior |
| [P2-MOB-04 #145](https://github.com/Bola-j/Helping-Hands/issues/145) | Regression, Android/iOS build and release evidence. | Phase 1 and Phase 2 flows |

The approved Flutter plan serves beneficiaries only in both phases. Earlier [third-party requester #142](https://github.com/Bola-j/Helping-Hands/issues/142), [charity capacity #143](https://github.com/Bola-j/Helping-Hands/issues/143), and [charity referral/outcome #144](https://github.com/Bola-j/Helping-Hands/issues/144) issues are unscheduled future product candidates. They remain open for later refinement, carry the `future-mobile-scope` label, and are outside Project #5. The wider web/API product still includes those roles. Any required mobile API changes receive separate issues with authorization and tests. App-store publication is not assumed.

## Planning and acceptance

- Tickets start in Backlog. The team sets priority, estimates, sprint dates and device targets during refinement.
- Each ticket needs actual work, relevant fictional-data success and failure checks, and reviewable evidence before Done. A build alone does not prove a device workflow.
- Request history records participant and coordinator reports; it does not independently prove aid delivery. The shared API enforces role and record access.
