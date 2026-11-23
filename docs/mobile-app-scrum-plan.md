# React JS Web and Flutter Mobile Scrum Plan

Status: **approved direction and current ticket map**. The JavaScript Vite build/container foundation is implemented locally under #148; complete screens and requester flows remain tracked in their issues. This plan adapts the mobile draft in `D:/Downloads/mobile-app-scrum-plan.md` to the project's current requirements and user's decisions. It covers a React browser website built with JavaScript and a distinct Flutter native mobile application. The website is not a PWA.

## Product split

- The React JavaScript application serves browser workflows. Its Phase 1 replacement rebuilds the current Expo browser prototype and preserves the planned browser journeys. Phase 2 adds responsive layouts for phone and tablet browsers.
- Flutter is the native requester and charity mobile app. Phase 1 covers beneficiary registration/sign-in, a single-need request, and the beneficiary's own request list. Phase 2 adds multi-need intake and progress/history, third-party requester submissions and tracking, and charity capacity and referral/outcome flows.
- Both clients use the same ASP.NET Core API. Role and record-involvement checks remain enforced by the API. Any API change discovered by native session or third-party flows gets a separate issue with its own contract, authorization criteria and tests.
- Use fictional people, records and evidence. Neither app verifies assistance independently or performs aid transfers.

## Phase 1 tickets — Project #4

| Issue | Work | Dependency |
| --- | --- | --- |
| [P1-WEB-01 #138](https://github.com/Bola-j/Helping-Hands/issues/138) | Rebuild browser routes and beneficiary workflows in React JavaScript, preserving API session/proxy behavior. | #1, #53, #148 |
| [P1-WEB-02 #148](https://github.com/Bola-j/Helping-Hands/issues/148) | Establish the Vite JavaScript build, browser/session checks and container foundation. | #53 |
| [P1-MOB-01 #139](https://github.com/Bola-j/Helping-Hands/issues/139) | Establish Flutter and verify native session, antiforgery and failure behavior on Android and iOS. | #8–#10 |
| [P1-MOB-02 #140](https://github.com/Bola-j/Helping-Hands/issues/140) | Deliver Flutter beneficiary self-service: signup/sign-in, single-need submission and own-request listing. | P1-MOB-01 |

Closed issue #7 records the original Expo scaffold and stays closed as historical work. Issues #138 and #148 are GitHub blockers for all downstream web and Flutter UI feature tickets. #148 is assigned to Bola for the TypeScript/Expo checks and container conversion; other web UI work remains assigned to Ali and mobile UI work to Hussein. Existing web workflow issues remain responsible for their individual pages.

## Phase 2 tickets — Project #5

| Issue | Work | Dependency |
| --- | --- | --- |
| [P2-WEB-01 #141](https://github.com/Bola-j/Helping-Hands/issues/141) | Make the React JavaScript website work across agreed phone, tablet and desktop viewports. | #138, #148 and relevant web pages |
| [P2-MOB-01 #142](https://github.com/Bola-j/Helping-Hands/issues/142) | Add third-party requester submission and own-submission tracking with authority/sharing consent and ownership checks. | Phase 2 requester/API requirements |
| [P2-MOB-02 #143](https://github.com/Bola-j/Helping-Hands/issues/143) | Show charity services and update available-from-now capacity with Cairo-day freshness and authorization states. | Verified capacity API |
| [P2-MOB-03 #144](https://github.com/Bola-j/Helping-Hands/issues/144) | Show assigned referrals and submit progress/outcome reports with protected evidence and clear review states. | Verified charity/referral API projections |
| [P2-MOB-04 #145](https://github.com/Bola-j/Helping-Hands/issues/145) | Add Flutter CI regression and reviewable Android/iOS demo builds. | P1-MOB-01 and mobile features |
| [P2-MOB-05 #146](https://github.com/Bola-j/Helping-Hands/issues/146) | Add catalog-based multi-need request composition with selected standard versions. | #140, #27, #28 |
| [P2-MOB-06 #147](https://github.com/Bola-j/Helping-Hands/issues/147) | Show beneficiary-safe request details, per-need progress and history. | #140, #37 |

The existing [P2-DEP-06 release verification issue #63](https://github.com/Bola-j/Helping-Hands/issues/63) remains the live web/API release check. Flutter build and demo verification is tracked separately in #145; app-store publication is not required by this plan.

## Acceptance and delivery

- New client issues remain in Backlog until selected during planning. Estimates and sprint dates are left for refinement. Web issues are assigned to Ali (`alifathyy`) and Flutter issues to Hussein (`HussienOkely`).
- Feature acceptance includes observable success, validation, session expiry, permission/ownership denial and unavailable-service behavior where relevant. No false success is shown after an unconfirmed write.
- Web verification records actual browser versions and viewports. Flutter verification records Android/iOS device or simulator and build coverage. A CI build does not substitute for an observed device workflow.
- Feature tests use fictional, isolated data; app credentials, signing secrets and sensitive participant data never enter source control, logs or public storage.
- A ticket closes only after its acceptance evidence, relevant automated checks, documentation and linked implementation review are recorded. Skipped platforms or external release steps remain explicit limitations.

No manifest, install prompt, service worker, PWA installation, offline business workflow or background synchronization is planned for the React JS website.
