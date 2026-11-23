# Ali — React browser website walkthrough and deliverables

**Prepared:** 23 November 2026  
**Audience:** Ali Fathy (`alifathyy`)  
**Purpose:** Walk the team through the current app and backend-supported workflows, identify the browser-client gaps, and agree the web tickets and evidence needed for delivery.

## Current state to explain

The target browser client is a standalone React website written in JavaScript. The current checkout now has a Vite/React shell with the home and “How it works” routes. The older Expo/TypeScript screens and session source remain in the repository as unmounted prototype code pending #138; `/demo` is not part of the current React shell. The browser foundation is merged in #148, but the beneficiary flow and role-specific Phase 1 screens are still unimplemented.

The API is ahead of the client. Its documented workflows include beneficiary and charity registration, coordinator charity decisions, service/standard management, charity capacity, request review and matching, split referrals, private evidence, outcome review, progress/history and explicit closure. The API-client rehearsal is recorded in [P1-58 backend rehearsal](p1-58-backend-rehearsal.md). It used fictional data and does not demonstrate the browser journey. The detailed API route walkthrough is [here](api-demo-walkthrough.md).

## Suggested walkthrough

1. **Orient the audience:** show the current home and navigation, then state the target is a JavaScript React browser website. The closed Expo scaffold ticket #7 is historical; it is not completion of replacement ticket #138.
2. **Show the current React shell:** demonstrate the home and “How it works” navigation and narrow-screen layout. Do not open `/demo` as a working route. Explain that beneficiary registration, sign-in/out, single-need submission and own-request listing are expected in #138 and still need implementation and isolated live verification.
3. **Walk the API-supported workflow:** use [the API demo walkthrough](api-demo-walkthrough.md) to explain charity application and coordinator decision; catalog and versioned standards; charity service/capacity update and Cairo-day freshness; beneficiary needs and coordinator review; matching/split referral; charity report with private evidence; coordinator review; and closure only when accepted quantities cover all needs. Use the dated [P1-58 evidence](p1-58-backend-rehearsal.md) to label which API behaviors were actually rehearsed.
4. **Trace role screens to tickets:** use the list below to explain which existing browser screens still need implementation or evidence. Role access is enforced by the API; client hiding alone is not authorization.
5. **Close with delivery evidence:** identify which workflow was observed live, which is documented/API-tested only, what remains unbuilt, and the tickets and owners for the next increment.

Use fictional people, accounts, requests and evidence only. A coordinator's acceptance is a review of a report, not independent proof that aid was delivered. The app coordinates assistance; it does not transfer aid or money.

## Ali's tickets and expected deliverables

### Assigned React client tickets

| Ticket | Expected deliverable |
| --- | --- |
| [P1-WEB-01 #138](https://github.com/Bola-j/Helping-Hands/issues/138) | Rebuild browser routes and beneficiary workflows in React JavaScript; preserve the same-origin API proxy and session/antiforgery behavior; document local start/build commands. This remains open. |
| [P1-WEB-02 #148](https://github.com/Bola-j/Helping-Hands/issues/148) | Establish JavaScript Vite build, browser/session checks and container pipeline. Implemented and merged in PR #149; CI passed build, session, navigation, narrow-screen and isolated proxy integration checks. |
| [P2-WEB-01 #141](https://github.com/Bola-j/Helping-Hands/issues/141) | Reflow core workflows at agreed phone/tablet/desktop sizes, with no ordinary-content horizontal overflow and touch/keyboard-usable forms, navigation, dialogs and evidence actions; record tested browsers/viewports and gaps. This is Phase 2. |

### Browser workflow tickets in the roadmap

The role workflows are separate from the React foundation. The roadmap lists Ali as the proposed responsible person for [charity application and coordinator review #17](https://github.com/Bola-j/Helping-Hands/issues/17), [coordinator services and standards #15](https://github.com/Bola-j/Helping-Hands/issues/15), [daily capacity task #23](https://github.com/Bola-j/Helping-Hands/issues/23), [coordinator request review #31](https://github.com/Bola-j/Helping-Hands/issues/31), [matching and referrals #36](https://github.com/Bola-j/Helping-Hands/issues/36), [charity referral and evidence #41](https://github.com/Bola-j/Helping-Hands/issues/41), and [coordinator follow-up and closure #43](https://github.com/Bola-j/Helping-Hands/issues/43). These are workflow acceptance tickets, not evidence that their screens are already implemented. Confirm the live issue/project assignment and status before sprint planning; the project plan says the new client issues remain in Backlog until selected.

Beneficiary browser demo [#116](https://github.com/Bola-j/Helping-Hands/issues/116) is the current narrow browser slice: single-need registration/submission and own-request confirmation. Keep multi-need and charity-to-outcome claims out of this slice unless their respective screens and acceptance evidence are complete.

## Definition of a reviewable handoff

- Link Ali's actual branch/PR and commits to the issues; do not use assignment as contribution evidence.
- Show the success path and meaningful validation, session-expiry, permission, conflict and unavailable-API states for the changed workflow.
- Record actual browser, viewport and isolated API/proxy results; clearly label unrun platforms and workflows.
- Keep web client behavior aligned with API contracts, including safe per-role projections and private evidence handling.
- Include relevant automated checks and passing CI before closing each ticket; do not substitute a build-only result for browser workflow evidence.
- Do not add PWA installation, service worker, offline business workflows or change API contracts under the React foundation tickets.

## Evidence boundary

This report was prepared from the current repository and the assigned-ticket search on 23 November 2026. The local worktree already has user changes in several planning documents; this report is a new standalone file and does not reconcile or overwrite those changes. No browser workflow was executed while preparing this report. Consult live GitHub issue/project state before treating statuses or assignments as current.
