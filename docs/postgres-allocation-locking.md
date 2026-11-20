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