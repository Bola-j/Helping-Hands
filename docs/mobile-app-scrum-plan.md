# Helping Hands Mobile App — Scrum Plan, Product Requirements, and GitHub Issue Samples

**Status:** Proposed mobile-sector plan for discussion with the project leader and team. It does not create or assign GitHub issues or represent approved assignments.

## 1. Product summary

### Product vision

Give beneficiaries a clear, private mobile way to request assistance and follow the status of their own requests.

### Mobile app boundary

The Flutter app is for **beneficiaries only** and targets **both Android and iOS**. Charity representatives, coordinators, administrators, donors, and contributors are outside the mobile app's role scope. Their workflows remain in the wider Helping Hands product and existing web/API work.

The mobile app is an additional client of the shared Helping Hands API. It does not replace the current Expo/React Native Web client, change API ownership, or add a second implementation of backend business rules.

Helping Hands records and coordinates assistance; charities or contributors arrange and provide assistance outside the platform. The app must not imply that Helping Hands receives, holds, purchases, stores, or delivers aid. An outcome report and a coordinator's acceptance are records and review, not independent proof of delivery.

### Primary user and value

| User | Need | Mobile value |
|---|---|---|
| Beneficiary | Request assistance and understand what is happening with their own request | Submit a request, keep its reference, see status/progress, and receive clear explanations of required actions or errors |

### Product goals

1. Make beneficiary self-service understandable on a phone.
2. Require the existing authority and information-sharing confirmations when submitting a request.
3. Show only the signed-in beneficiary's authorized information.
4. Reuse the shared API and established project terminology, requirements, and privacy rules.
5. Provide a tested, demonstrable cross-platform app using fictional data for course evaluation.

### Out of scope

- Charity, coordinator, administrator, donor, or contributor screens.
- Third-party/on-behalf-of intake, public browsing of requests, payment, aid custody/delivery, or charity inventory/accounting.
- Offline writes or a claim that a request succeeded when the API response is unknown.
- Backend/database changes inside a Flutter feature. Raise API gaps as separate dependencies for the API owner and team to plan.

## 2. Product requirements

All requirements below are proposed and should be traced into the team's product backlog and mobile section of the SRS after review.

| ID | Requirement | Phase | Acceptance summary |
|---|---|---:|---|
| MPR-01 | Beneficiary registration | 1 | A person can create a beneficiary account using the existing API contract. The app explains that registration and sign-in are separate steps. |
| MPR-02 | Beneficiary session | 1 | A beneficiary can sign in and sign out. Session loading, expiry, invalid credentials, and API-unavailable states are distinguishable. Credentials are not persisted by the app. |
| MPR-03 | Single-need request | 1 | A signed-in beneficiary can submit one need with both required confirmations. The app validates input and shows the returned reference/status only after confirmed success. |
| MPR-04 | Own request list | 1 | A beneficiary can view their own submitted requests and an empty state. The app never exposes another beneficiary's records. |
| MPR-05 | Multi-need request | 2 | A beneficiary can select current catalog standards, add distinct needs and quantities, review the request, and submit it with the required confirmations. Invalid/stale catalog choices are explained. |
| MPR-06 | Request progress and detail | 2 | A beneficiary can open one of their requests and see its authorized status, needs, assigned portions, remaining work, and relevant next steps. |
| MPR-07 | Request history | 2 | A beneficiary can view history permitted by the API, including clarification or decision information relevant to them. Staff-only information and private evidence for unrelated records stay hidden. |
| MPR-08 | Safe failure handling | Both | Validation, unauthorized access, expired sessions, conflicts, timeouts, and unavailable API responses are shown clearly. An uncertain write is not automatically resubmitted as if it had never happened. |
| MPR-09 | Accessible mobile interaction | Both | Core flows support screen readers, clear labels, sufficient contrast, text scaling, keyboard navigation where applicable, and touch-sized controls. |
| MPR-10 | Cross-platform behavior | Both | The agreed core flows work on Android and iOS. Platform-specific limitations are documented and tested on available devices/simulators. |

Use fictional people, accounts, requests, and attachments in demonstrations and automated tests. Do not include sensitive beneficiary data in logs, analytics, screenshots, crash reports, or local caches.

## 3. Project specifications

### 3.1 Architecture and integration

- **Mobile client:** Flutter and Dart; Android and iOS targets.
- **Backend:** existing ASP.NET Core API, PostgreSQL persistence, and private evidence storage. The mobile app calls the API; it does not connect directly to the database or object storage.
- **API contract:** confirm field names, status codes, authorization projections, pagination, and error formats against the Development OpenAPI document and API tests before implementing each feature.
- **Authentication:** current API uses HTTPS secure, HttpOnly, SameSite=Strict authentication and antiforgery cookies. The mobile client must retain the cookies, obtain an antiforgery token, send it on state-changing calls, and handle expiry. Validate this native-client behavior early for both platforms. The existing same-origin web proxy does not by itself prove that native mobile integration works.
- **Authorization:** trust the API as the authorization boundary; show only data returned to the current beneficiary. A hidden UI control is not an access-control mechanism.
- **Connectivity:** online request submission only. If the API is unavailable, explain the failure and preserve form data safely for the current screen; do not claim a write succeeded. Avoid automatic retries of non-idempotent writes unless the API provides a verified idempotency contract.
- **API changes:** if native cookie handling, CORS/deployment configuration, or a mobile flow needs backend support, create a separately reviewed dependency in the team's backlog. Do not quietly modify teammate-owned backend code.

### 3.2 Flutter source organization and coding style

Use a feature-oriented structure, small reusable widgets, typed request/response models, and a centralized API/session layer. Keep screens focused on presentation and user interaction; keep transport, validation, and response mapping out of widget build methods. Prefer null-safe Dart, explicit async error handling, descriptive names, and focused files.

Proposed starting structure (to refine with the team):

```text
lib/
  app/
  core/
    network/
    session/
    theme/
  features/
    auth/
    requests/
    progress/
  shared/
    widgets/
```

This is a proposed structure, not an implemented project. Avoid adding layers or dependencies without a concrete need. Keep API DTOs separate from presentation state when their responsibilities differ.

### 3.3 Visual design and tone

Carry the existing web app's calm, clear visual language into native layouts; adapt it for small screens rather than copying desktop page layouts.

| Token | Existing web value | Proposed mobile use |
|---|---|---|
| Deep navy | `#15334A` | Headings, primary text, navigation |
| Teal | `#126B62` | Primary actions and links |
| Teal green | `#16675F` | Small emphasis and status accents |
| Light background | `#F6F9FA` | App canvas |
| Warm gold | `#F0C77A` | Limited accent for journey/attention cues |
| White | `#FFFFFF` | Cards and form surfaces |
| Blue-gray text | `#405966` | Supporting copy |
| Pale borders | `#D9E4E6` | Dividers and card/input boundaries |

Use plain, respectful language, clear page titles, visible status and error messages, simple cards, and uncluttered request steps. Avoid language that stigmatizes beneficiaries or suggests that platform review proves aid delivery. Confirm Arabic and right-to-left requirements with the team; they are not assumed by this draft.

### 3.4 Quality, privacy, and verification

- Unit tests: input validation, API error mapping, request/session state transitions.
- Widget tests: sign-in and request flows, loading/empty/error/confirmation states, accessibility labels.
- Integration tests: session plus write-token behavior, single-need submission and own-request visibility against safe fakes or isolated fictional services.
- Phase 2: broader integration coverage for multi-need submission, progress/history, denied access, stale versions, and unavailable dependencies.
- Never use a shared or production database/storage target for tests.
- Test Android and iOS behavior on available emulators/simulators and record actual platform/device coverage; don't claim an unrun platform was verified.

### 3.5 GitHub workflow and contribution evidence

- Add the mobile workstream to the existing Helping Hands GitHub Project/backlog, with issue links, priorities, dependencies, estimates, owners, dates, and Todo/In Progress/Done status.
- Use feature branches and link each implementation PR to its issue. Phase 2 requires a separate dedicated branch for each feature, merged into main only after the feature is finished and reviewed.
- Each student must make genuine code contributions through their own Git identity. Issues, docs, and assignments do not substitute for code contribution evidence.
- Keep Scrum meeting notes, milestones, sprint review outcomes, actual test results, and blockers in the repository after the team accepts the plan.
- User approval is required before any future commit, push, or PR from this work. This document does not create GitHub issues or send a PR.

## 4. Scrum plan

### Phase 1 — Implementation baseline and contribution

**Course schedule provided:** no labs during 21–26 November 2026; use that period to develop Phase 1. Evaluation discussions are scheduled for 28 November–3 December 2026. This plan does not claim any deliverable has already been completed.

**Sprint goal:** demonstrate a working Flutter beneficiary path on **both Android and iOS**. A successful build alone is not enough: registration/sign-in, single-need submission, and own-request viewing must work on each platform. Prioritize this complete path over extra screens.

**Phase 1 demo acceptance:**

1. Build and launch the app on an agreed Android device/emulator **and** iOS simulator/device.
2. On each platform, register a fictional beneficiary, then sign in separately.
3. On each platform, submit one single-need request with both required confirmations.
4. On each platform, show the returned request reference/status and the beneficiary's own request list.
5. Show at least validation, loading, empty, session-expired, denied, and API-unavailable states.
6. Demonstrate through the shared API using isolated fictional data; explain the cookie/antiforgery integration and known limitations.

If the complete beneficiary flow works on only one platform, Phase 1's cross-platform demo acceptance is **not met**. Record a platform gap as a blocker and keep the related issue open; do not count the gap record as successful acceptance.

**Phase 1 course evidence:**

- Active GitHub Issues and Project tracking for mobile tasks, estimates, priorities, owners, due targets, and progress.
- Feature branch/PR workflow and visible individual code contribution under the student's own Git identity.
- Basic GitHub Actions workflow for Flutter formatting/analyze/test and an Android build; add iOS build where an available macOS runner permits it.
- Reproducible setup/demo guide and a sprint review note with actual results, limitations, and follow-up actions.
- Do not present the mobile beneficiary demo as the team's complete charity-to-referral workflow or full-app rehearsal.

**Phase 1 backlog candidates:** see MOB-P1-01 through MOB-P1-06 below. Refine estimates and assignees with the team; this document does not assign work or create issues.

### Phase 2 — Comprehensive engineering, Agile, and DevOps

**Sprint goal:** deliver at least two complete beneficiary features through feature-first branches, comprehensive tests, system models/specification, automated CI/CD, and live distribution/deployment evidence for Android and iOS.

**Required feature-first rule:** one dedicated branch per feature; complete and verify the feature; open a linked PR for team review; merge into main only after required review and checks. The student must personally implement at least two features. The two proposed mobile features are MOB-P2-F1 and MOB-P2-F2.

**Phase 2 backlog candidates:**

- Feature 1: catalog-based multi-need request composer.
- Feature 2: beneficiary request details, progress, and history.
- Supporting issues: full test suite, requirements/UML models, Android+iOS CI builds, release/distribution, and final rehearsal.

**Relative iterations (dates set with the project team):**

| Iteration | Goal | Planned evidence |
|---|---|---|
| P2-A — Stabilize | Confirm API/session and requirements; establish Flutter analysis, test, and build baseline | Reviewed mobile requirements/model, session spike result, green baseline workflow |
| P2-B — Feature 1 | Implement multi-need intake on its own branch | Valid and invalid submissions, tests, linked PR/review/CI |
| P2-C — Feature 2 | Implement progress/history on its own branch | Own-record and denied-access scenarios, tests, linked PR/review/CI |
| P2-D — Release | Regress, model, deploy/distribute, and prepare final lab | Android/iOS release evidence, live API connection, final demo, review and retrospective |

Exact dates, estimates, team capacity, and the final-lab date are not supplied here; set them during joint backlog refinement rather than treating these iterations as calendar commitments.

### Definition of Done

A mobile issue is Done when its acceptance criteria are demonstrated; the implementation is on the right feature branch; relevant tests pass; authorization, privacy, and meaningful failure states are checked; docs/models are updated when needed; required CI is green; and the result can be reproduced with fictional data. A merged partial implementation is not automatically complete.

## 5. Sample technical GitHub issues

The items below are **issue drafts** with proposed keys for discussion. They have no GitHub issue numbers, assignees, estimates, or due dates yet. Link them to the existing requirements and shared project backlog only after the team accepts the plan.

### Phase 1 issue drafts

#### MOB-P1-01 — Bootstrap Flutter app for Android and iOS

**Type:** Foundation · **Priority:** High · **Estimate/owner/due date:** team to set · **Dependencies:** agree Flutter SDK and available devices/runners.

**Description:** Create the mobile app shell and a small beneficiary navigation structure without changing the existing web app or API.

**Acceptance criteria:**
- App builds and starts on an agreed Android emulator/device and iOS simulator/device.
- Project has a documented Flutter SDK/tooling requirement and repeatable local run steps.
- Shared theme and feature-oriented directory foundation exist.
- No credentials or environment-specific secrets are added to source control.

#### MOB-P1-02 — Prove native API session and antiforgery behavior

**Type:** Security/integration spike · **Priority:** Blocker for API writes · **Estimate/owner/due date:** team to set · **Dependencies:** existing API auth routes and isolated HTTPS stack.

**Description:** Verify that native Android and iOS clients can register, retain the required cookies, obtain and send the antiforgery token, sign in separately, and handle expiry using the current API contract.

**Acceptance criteria:**
- Registration, separate sign-in, an authenticated read, and an antiforgery-protected write succeed on both Android and iOS against isolated fictional test data.
- State-changing calls send the required token and cookie; no credential is saved in app preferences/logs.
- Unauthenticated, expired, denied, and unavailable outcomes are distinguishable.
- Any necessary API/CORS/origin/deployment change is recorded as a separate backend dependency for team approval; this issue does not modify API code. If either platform is blocked, keep this issue open and do not mark the Phase 1 cross-platform demo complete.

#### MOB-P1-03 — Implement beneficiary registration and sign-in

**Type:** Feature · **Priority:** High · **Estimate/owner/due date:** team to set · **Dependencies:** MOB-P1-01, MOB-P1-02.

**Description:** Provide a clear beneficiary account creation, sign-in, current-session, and sign-out flow.

**Acceptance criteria:**
- Registration uses the beneficiary-specific endpoint and reports the returned reference.
- Registration does not imply automatic sign-in; sign-in is a separate step.
- Validation, duplicate/invalid account response, busy state, and network failure are handled.
- Passwords are not stored by the client; sign-out clears client session state.

#### MOB-P1-04 — Submit a single-need request and show own requests

**Type:** Feature · **Priority:** High · **Estimate/owner/due date:** team to set · **Dependencies:** MOB-P1-02, MOB-P1-03, API single-need route.

**Description:** Let the signed-in beneficiary submit one need and see the resulting reference/status and their own request list.

**Acceptance criteria:**
- Submit is unavailable until required description/validation and both authority/sharing confirmations are satisfied.
- API success displays the returned reference/status and refreshes the own-request list.
- Validation, permission, session, conflict, and API-unavailable responses do not display false success.
- The view does not expose another beneficiary's requests or unrelated private evidence.

#### MOB-P1-05 — Add Phase 1 Flutter checks and GitHub Actions baseline

**Type:** Quality/CI · **Priority:** High · **Estimate/owner/due date:** team to set · **Dependencies:** MOB-P1-01.

**Description:** Add meaningful checks for mobile foundation and Phase 1 beneficiary behavior.

**Acceptance criteria:**
- Workflow runs formatting, static analysis, relevant unit/widget tests, and Android build on pushes and PRs.
- iOS build is checked on macOS runner where configured; limitations are documented.
- Tests cover user-visible behavior/failures and use only fictional, isolated data.
- CI results are linked from the issue/PR; no skipped check is reported as passing.

#### MOB-P1-06 — Prepare Phase 1 mobile demo and contribution evidence

**Type:** Demo/course evidence · **Priority:** High · **Estimate/owner/due date:** team to set · **Dependencies:** MOB-P1-01 through MOB-P1-05 as refined.

**Description:** Make the app demo reproducible and prepare the mobile contribution evidence for the Phase 1 lab discussion.

**Acceptance criteria:**
- Setup, run, fictional account, and demo steps are documented.
- GitHub Project shows linked tasks, actual owner/status/priority/date/estimate fields.
- The student's own code contribution is visible in commits and linked feature branch/PR evidence.
- Review note states what was demonstrated, actual checks, platform/device coverage, limitations, and follow-up work.

### Phase 2 feature issue drafts

#### MOB-P2-F1 — Build catalog-based multi-need request composer

**Type:** Feature · **Priority:** High · **Estimate/owner/due date:** team to set · **Dedicated branch:** `phase2/mobile-multi-need-intake` · **Dependencies:** MOB-P1-04, verified catalog and multi-need API contracts.

**Description:** Let a beneficiary select current catalog standards, add distinct needs/quantities, review the complete request, and submit it.

**Acceptance criteria:**
- Current catalog choices and component/unit information are presented clearly.
- User can add/remove needs and quantities within API limits; duplicate/invalid entries are prevented or explained.
- Submission includes required confirmations and the exact selected standard versions.
- Stale selection, validation, session expiry, API failure, and successful generated reference have clear outcomes.
- Unit/widget/API-contract tests cover the normal and failure paths; only the beneficiary's own request is shown.
- Feature is complete and reviewed on its dedicated branch before merging.

#### MOB-P2-F2 — Show beneficiary request progress and history

**Type:** Feature · **Priority:** High · **Estimate/owner/due date:** team to set · **Dedicated branch:** `phase2/mobile-request-progress` · **Dependencies:** MOB-P1-04, verified progress/history projections.

**Description:** Let a beneficiary open one of their requests and understand its status, separate needs, assigned portions, remaining work, and allowed history.

**Acceptance criteria:**
- List and detail screens load only requests authorized for the signed-in beneficiary.
- Progress and history use the API's beneficiary-safe response projection.
- Loading, empty, not-found/concealed, expired session, denied, and unavailable cases are understandable.
- No coordinator-only notes or unrelated participant/evidence data are displayed.
- Unit/widget/integration tests cover successful and denied/unavailable access.
- Feature is complete and reviewed on its dedicated branch before merging.

### Phase 2 supporting issue drafts

#### MOB-P2-03 — Complete mobile test suite for core beneficiary workflows

**Type:** Quality · **Dependencies:** MOB-P2-F1 and MOB-P2-F2.

Acceptance: unit, widget, and safe integration tests cover authentication/session, single and multi-need requests, progress/history, validation, ownership, failure states, and accessibility-relevant semantics. Test layers and any unverified cases are documented.

#### MOB-P2-04 — Add mobile requirements and UML/system models

**Type:** Documentation/design · **Dependencies:** agreed feature behavior.

Acceptance: mobile stories and functional/non-functional requirements trace to the shared PRD/SRS; deliver/use current use-case, request sequence, component/deployment, and relevant class/domain models; diagrams match implemented behavior and retain broader product roles outside Flutter.

#### MOB-P2-05 — Build cross-platform CI pipeline

**Type:** CI/CD · **Dependencies:** MOB-P1-05.

Acceptance: GitHub Actions runs analysis, tests, and Android build; iOS build runs on macOS where available; release signing secrets are not committed; build artifacts and failure results are retained/linked according to team policy.

#### MOB-P2-06 — Release Flutter app for Android and iOS

**Type:** Deployment/release · **Dependencies:** MOB-P2-03, MOB-P2-05, live API deployment and approved signing/distribution access.

Acceptance: release artifacts are produced for Android and iOS with version information; app connects to the approved live API using production-safe configuration; a real distribution path is demonstrated and documented (for example, Play internal testing and TestFlight if the team has required accounts). Confirm with the instructor whether beta distribution satisfies the course's mandatory live-deployment criterion. Do not claim app-store publication without an approved release and successful store process.

#### MOB-P2-07 — Rehearse final mobile demo and record sprint outcomes

**Type:** Release readiness · **Dependencies:** features, test suite, models, CI, and distribution issues.

Acceptance: run the agreed Android/iOS demo path with fictional accounts; show Project/Issues, feature branches/PR reviews, CI, test evidence, models/specifications, and distribution; record actual results, feedback, and unresolved gaps in repository meeting notes after team approval.

## 6. Dependencies and open decisions

| Decision/dependency | Proposed handling |
|---|---|
| Flutter SDK and package choices | Agree versions/dependencies during refinement; prefer the smallest set needed and document why each nonstandard dependency is used. |
| Native cookie + antiforgery support | Treat MOB-P1-02 as an early blocking spike for both platforms. Escalate API-origin/config changes to backend owner. |
| Arabic and right-to-left support | Confirm whether required for course evaluation; if required, include localization and RTL acceptance early, not as a final cosmetic task. |
| Android/iOS test availability | Identify real devices/simulators and CI runner limits. Record tested platform versions; do not imply parity without evidence. |
| Live deployment definition | Confirm API hosting plus mobile beta distribution vs app-store release. Obtain signing/accounts through the team; never put secrets in the repository. |
| Estimates, assignees, milestone dates | Set jointly on the existing GitHub Project. Do not treat this draft as an assignment or commitment. |
| Existing React Native Web scope | Flutter is an additional mobile client. Coordinate shared user flow/API behavior with web owners to avoid incompatible terminology/contracts. |

## 7. Project references

- [Business Requirements Document](https://github.com/Bola-j/Helping-Hands/blob/main/docs/BRD.md)
- [Product Requirements Document](https://github.com/Bola-j/Helping-Hands/blob/main/docs/PRD.md)
- [Software Requirements Specification](https://github.com/Bola-j/Helping-Hands/blob/main/docs/SRS.md)
- [Phase 1 Scrum roadmap](https://github.com/Bola-j/Helping-Hands/blob/main/docs/api-demo-scrum-roadmap.md)
- [Full-app Scrum roadmap](https://github.com/Bola-j/Helping-Hands/blob/main/docs/scrum-roadmap.md)
- [API v1 plan](https://github.com/Bola-j/Helping-Hands/blob/main/docs/api-v1-plan.md)
- [API demo walkthrough](https://github.com/Bola-j/Helping-Hands/blob/main/docs/api-demo-walkthrough.md)
- [Current web theme tokens](https://github.com/Bola-j/Helping-Hands/blob/main/web/src/components/styles.ts)
- [Phase 1 GitHub Project](https://github.com/users/Bola-j/projects/4)

## 8. Handoff

This draft is for the mobile contributor and project leader to brainstorm and refine. Once accepted by the team, merge its workstream into the master Scrum backlog, add approved issues and estimates to GitHub Projects, confirm requirements/API dependencies, and record the agreed milestone/sprint schedule. No GitHub issue or assignment has been created from this plan. This file remains an uncommitted local change; no commit, push, PR, or deployment was performed.
