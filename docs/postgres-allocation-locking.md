# PostgreSQL Allocation & Capacity Locking Protocol (`[P1-53]`)

## Purpose
Define a single, deterministic row-lock and transaction protocol in PostgreSQL for:
- Daily charity capacity updates (`DailyCapacity` and `CapacityUpdate`).
- Referral allocations against assistance requests (`AssistanceRequest`, `Referral`, and `DecisionHistory`).

## Global Lock Ordering (Deadlock Prevention)
All write transactions that acquire row locks (`SELECT ... FOR UPDATE`) must acquire locks strictly in this ascending tier order, and in ascending primary-key (`Guid`) order within each tier:

1. **Tier 1 — `AssistanceRequest`** (`"AssistanceRequest"."Id"` ordered ascending)
2. **Tier 2 — `ReferralRecipient` then `Charity`** (`"ReferralRecipient"."Id"` ordered ascending, then `"Charity"."RecipientId"` ordered ascending)
3. **Tier 3 — `CharityService`** (`"CharityService"."CharityServiceId"` ordered ascending)
4. **Tier 4 — `DailyCapacity`** (`"DailyCapacity"."CapacityId"` ordered ascending)

Transactions must never acquire a higher-numbered tier lock while holding a lower-numbered tier lock out of sequence.

## Transaction & Consistency Guarantees
- **Atomic Transactions:** Each capacity update or referral allocation executes inside an explicit database transaction (`BeginTransactionAsync`) and commits only after validation, row locking, state mutation, and audit history insertion succeed.
- **Preserved History:** Historical audit entries (`CapacityUpdate` and `DecisionHistory`) are append-only and never overwritten or deleted; each mutation records the acting user, timestamp, and action.
- **Consistent Balances:** Available daily capacity and allocated referral quantities are validated and updated under row locks so concurrent operations cannot over-allocate or produce negative balances.

## Implemented writers

- `UpdateCharityDailyCapacity` locks the charity, selected `CharityService` rows, and existing current-day `DailyCapacity` rows before replacing availability or creating a new daily row.
- `SplitReferralAllocationService` locks the request, recipients, charities, matched `CharityService` rows, and current-day capacities before deducting capacity and creating referrals.
- Both writers use the same transaction-scoped order above; the service-row lock also serializes same-day capacity-row creation for a service.
- The legacy `POST /requests/{id}/referrals` path checks capacity freshness but does not deduct `DailyCapacity`; it acquires the compatible request/recipient/charity prefix. The balance-mutating allocation writer covered here is `SplitReferralAllocationService`.

The focused isolated PostgreSQL test holds the CharityService row lock until both independent writers appear in its PostgreSQL lock-wait chain, then releases them and verifies referral totals, nonnegative capacity, separate request/referral history, and capacity-update history. A second regression allocates one accepted need on a two-need request with a rejected sibling, verifies both review decisions are preserved and the request remains Open, and confirms the outcome review API succeeds. These tests do not establish failure-path rollback behavior.

## Work order and closure blockers — 21 November 2026 (historical snapshot)

1. **Implementation:** complete. Capacity updates and split allocations now use the shared lock order. Allocation leaves the request `Open` and preserves the accepted need status, so later outcome review remains eligible.
2. **Local verification on the P1-53 worktree based on current `main`:** release build succeeded with one existing EF1002 warning in `WorkflowDbTests.CharityCapacity.cs`; the focused PostgreSQL regressions passed (2/2), the workflow PostgreSQL suite passed (91/91) on a fresh isolated database, the non-PostgreSQL suite passed (66/66), and `git diff --check` passed. Full API CI remains required.
3. **Delivery gate:** follow-up PR [#123](https://github.com/Bola-j/Helping-Hands/pull/123) is open from `phase1/p1-53-postgres-allocation-locking`. Required API full verification and Web checks passed on commit `2220c9e`; one approval remains before merge.
4. **Close the ticket:** after the PR checks and approval pass, attach the exact commands and results to issue #56, move its Project card to Done, and close the issue. Do not close it from local test results alone.

## Delivery status after PR #129 — 22 November 2026 (historical snapshot)

PR [#123](https://github.com/Bola-j/Helping-Hands/pull/123) merged to `main` at `889ffccec4abd12c8b45aebf374ac9dd86bf0ec3`; its reviewed head was `a94634a`. Required API full verification, Web typecheck/export/session checks and GitGuardian checks passed. The implementation and documentation reconciliation had merged. Issue #56 was closed after PR #129; this status was superseded the same day when follow-up review found that the real PostgreSQL failure-path rollback had not been exercised.

## Failure-path rollback verification follow-up — 22 November 2026 (pre-merge snapshot)

The P1-53 reviewers identified that the older API test named for rollback simulates an outcome and does not inspect persistent PostgreSQL state. Issue #56 was reopened for real failure-path coverage. PR #130 adds test-only PostgreSQL triggers that fail a capacity audit insert and a split-allocation history insert; both tests verify from fresh contexts that balances, referrals, and history remain unchanged (and that request/need state is preserved for allocation). The Release solution build passed with one existing EF1002 warning, and all four focused P1-53 PostgreSQL tests passed on a disposable PostgreSQL 17 database.

## P1-53 completion after PR #130 — 22 November 2026

PR [#130](https://github.com/Bola-j/Helping-Hands/pull/130) merged to `main` at `89565b068c0fe2f772fb7607fc3db456b159a6b5`; its reviewed head was `16ae27d`. Both API full-verification runs ([44960603340](https://github.com/Bola-j/Helping-Hands/actions/runs/44960603340), [44960637075](https://github.com/Bola-j/Helping-Hands/actions/runs/44960637075)) and both Web checks ([44960603364](https://github.com/Bola-j/Helping-Hands/actions/runs/44960603364), [44960637052](https://github.com/Bola-j/Helping-Hands/actions/runs/44960637052)) passed, along with GitGuardian. The four focused P1-53 PostgreSQL tests passed locally on a disposable PostgreSQL 17 database. Issue #56 is closed and the Project card is Done. The dedicated P108/P1 end-to-end fixtures remain outside the locally verified P1-53 evidence.

The current Compose API/web certificate failure is tracked separately from P1-53: PostgreSQL, storage and Mailpit started, but the API and web containers need the local `.local/https` certificate exports for standalone health checks. The full integration category also includes unrelated fixtures requiring `P108_TEST_POSTGRES` and `P1_E2E_POSTGRES`; those are not P1-53 closure evidence. Failure-path rollback is covered by PR #130's isolated PostgreSQL tests and passing required CI.
