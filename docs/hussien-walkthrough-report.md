# Hussien Okely — Flutter mobile app walkthrough and deliverables

**Prepared:** 23 November 2026  
**Audience:** Hussien Okely (`HussienOkely`)  
**Responsibility:** Cross-Platform Mobile App Dev
**Purpose:** Walk the team through the current app and backend-supported workflows, define the Flutter mobile slice, and make its tickets and evidence expectations clear.

## Current state to explain

The intended mobile client is a distinct Flutter app using the shared ASP.NET Core API. The repository currently has no Flutter project. Therefore there is no mobile workflow to demonstrate yet: the Phase 1 Flutter tickets are foundations and planned deliverables, not completed app features.

The current `web/` application is the browser prototype and is scheduled for replacement by a JavaScript React website. Do not present it as the Flutter app. The backend does support beneficiary registration/sign-in, a public single-need request path and an own-request listing projection, with API behavior and broader charity/referral/outcome flows described in the [API demo walkthrough](api-demo-walkthrough.md). The [P1-58 rehearsal record](p1-58-backend-rehearsal.md) documents an API-client run against isolated services, not an Android/iOS device walkthrough.

Phase 1 mobile scope is deliberately small: beneficiary registration/sign-in, one single-need request, and listing the signed-in beneficiary's own requests. Phase 2 extends Flutter to third-party requests, multi-need intake, per-need progress/history, and charity capacity/referral/outcome workflows. Both phases use fictional data; the app coordinates assistance and does not transfer aid or independently verify delivery.

## Suggested walkthrough

1. **Orient the audience:** show the repository's current client split and explain that Flutter is planned but not present. Identify the browser prototype as a separate client and do not imply its screens are native mobile screens.
2. **Walk the API contract:** in [the API demo walkthrough](api-demo-walkthrough.md), show the beneficiary registration and separate login contract, antiforgery-protected single-need submission, generated reference/status, and own-request read. Explain the relevant session, ownership, validation and failure states the mobile client must preserve.
3. **Explain the first Flutter increment:** start with [shared UI #151](https://github.com/Bola-j/Helping-Hands/issues/151) and [native sessions #139](https://github.com/Bola-j/Helping-Hands/issues/139), then follow the beneficiary screen, test and demo tickets [#152–#157](mobile-app-scrum-plan.md#phase-1--working-beneficiary-demo-project-4). [#140](https://github.com/Bola-j/Helping-Hands/issues/140) is the umbrella acceptance. The app must not report success until the API confirms the write or expose other beneficiaries' requests.
4. **Preview the later mobile workflows:** use Phase 2 tickets #142–#147 below to show how the app expands after requirements/API dependencies are ready. Charity reports remain participant-submitted reports; coordinator acceptance is a separate review, not proof of delivery.
5. **Close with platform evidence:** record the actual Android and iOS device/simulator, session and workflow outcomes, automated checks, build artifacts, and anything unavailable or unverified. A Flutter build or API test alone is not a device workflow.

## Hussien's tickets and expected deliverables

### Phase 1 — Project #4

| Ticket | Expected deliverable |
| --- | --- |
| [P1-MOB-01 #139](https://github.com/Bola-j/Helping-Hands/issues/139) | Create Flutter project and document local Android/iOS run setup; prove native cookie and antiforgery session behavior for registration or login, authenticated read, protected write, logout, expiry, denied access and unavailable API on Android and iOS. Keep credentials out of preferences/logs and record device/simulator plus limits. |
| [P1-MOB-02 #140](https://github.com/Bola-j/Helping-Hands/issues/140) | Deliver beneficiary signup, separate sign-in/out, required authority/sharing confirmations, single-need request submission, server-confirmed reference/status and own-request listing. Cover validation, duplicate signup, expired/unauthenticated session, denied access, conflict and unavailable API; add focused fictional-data widget/service tests and Android/iOS execution evidence. |

### Phase 2 — Project #5

| Ticket | Expected deliverable |
| --- | --- |
| [P2-MOB-01 #142](https://github.com/Bola-j/Helping-Hands/issues/142) | Third-party request submission and tracking under an approved API contract; authority/sharing consent, requester ownership, and clear denial/conflict/failure behavior. Track any needed backend capability in a separate API issue. |
| [P2-MOB-02 #143](https://github.com/Bola-j/Helping-Hands/issues/143) | Charity representative's supported services and available-from-now capacity; show server-confirmed updates, Cairo-day freshness/stale status, and validation/session/authorization/unavailable states. |
| [P2-MOB-03 #144](https://github.com/Bola-j/Helping-Hands/issues/144) | Assigned referral and outcome/progress workflow with protected evidence upload; show clarification, unresolved, accepted, partial and completed states; prevent unrelated referral/evidence access and handle upload/API failures. |
| [P2-MOB-04 #145](https://github.com/Bola-j/Helping-Hands/issues/145) | Flutter format/analyze/test checks and Android CI build; iOS CI where a macOS runner is available; documented reviewable demo builds and actual tested device/simulator coverage. Keep signing secrets out of source. |
| [P2-MOB-05 #146](https://github.com/Bola-j/Helping-Hands/issues/146) | Catalog-backed multi-need composer with distinct services, selected standard versions and quantities; authority/sharing confirmations, stale/invalid selection handling, server-confirmed submission, and fictional-data success/failure tests. |
| [P2-MOB-06 #147](https://github.com/Bola-j/Helping-Hands/issues/147) | Beneficiary-safe request list/detail, separate needs, assigned portions, remaining work and allowed history; loading/empty/concealed/expired/denied/unavailable states; own-record success and unrelated-record denial tests. |

## Definition of a reviewable handoff

The expanded [mobile Scrum plan](mobile-app-scrum-plan.md) now includes Phase 1 bootstrap, account, sign-in, list, single-need, CI and demo tickets #151–#157, and Phase 2 accessibility and requirements/model tickets #158–#159. The earlier charity and third-party tickets remain separate planned work.

- Link actual contributor branches, commits and PRs to the relevant issue. An assignee is not proof of authorship.
- Demonstrate the observable success path plus meaningful invalid-input, expired-session, authorization/ownership and dependency-failure paths.
- Exercise both Android and iOS where the ticket requires it; name tested simulators/devices and explicitly state platform coverage that could not be run.
- Use API-authorized projections and server authorization; never rely on hidden UI alone to protect records or evidence.
- Use fictional isolated accounts and data. Do not place real participant details, credentials, signing material or private evidence in source, logs or public app storage.
- Keep Phase 2 features behind their dependencies and refined sprint scope. Do not describe them as shipped because their API or ticket exists.

## Evidence boundary

This report was prepared from the current repository and the assigned-ticket search on 23 November 2026. The local worktree already has user changes in several planning documents; this report is a new standalone file and does not reconcile or overwrite those changes. No Flutter or device workflow was executed while preparing this report. Consult live GitHub issue/project state before treating statuses or assignments as current.
