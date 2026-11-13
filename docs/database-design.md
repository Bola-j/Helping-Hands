# Phase 1 database design — proposal for review

**Status: Phase 1 design proposal, with implemented catalog and need schemas.** This document describes the intended fictional-data Phase 1 demo. The existing EF model and migration cover only part of this proposal and do not establish the state of any deployed database. The [roadmap](api-demo-scrum-roadmap.md), [SRS](SRS.md), and [BRD](BRD.md) describe the requirements. The [earlier full-product proposal and prior diagrams](archive/database-proposal/) are preserved as historical working material.

## Models and notation

- [Conceptual model](diagrams/conceptual-eer.drawio): one canvas with the Phase 1 concepts and their principal relationships.
- [Relational model](diagrams/relational-schema.drawio): one canvas with candidate tables, fields and foreign-key relationships. Colored boxes group identity, requests, catalog, capacity/reminders, and referrals/outcomes. Cross-group lines are intentional.

Both diagrams use crow’s-foot notation: a bar means one, a circle means zero, and a crow’s foot means many. The relational diagram uses dashed lines without cardinality markers for Phase 1 extension links whose nullability or multiplicity has not been settled; the conceptual diagram shows candidate business cardinalities. Conditional rules and unresolved mappings require review. PK means primary key, FK foreign key, and UQ unique. Baseline fields retain their earlier proposed types and `!`/`?` nullability; Phase 1 extension fields have unspecified type and nullability unless explicitly stated. The diagrams are design aids, not executable DDL. See the [open decisions](#open-decisions) before treating any proposed table or relationship as final.

## Phase 1 scope and business rules

- A beneficiary submits their own request, confirms authority and sharing, and records one or more separately tracked needs. A need records its service and standard version and quantity. Third-party and coordinator-on-behalf intake are later scope.
- A coordinator reviews charity applications. Only approved, active charities can receive new referrals; members' access also depends on involvement. An application needs registration details or supporting documentation.
- Coordinators manage versioned service categories and platform standards with structured components. New requests use current versions; historical needs and assignments retain their recorded versions. Until reviewed, outstanding older needs match only the exact recorded versions; unmatched needs remain open (P1-33).
- Charity capacity is nonnegative available-from-now platform units. An update replaces availability without erasing prior commitments; new assignments reduce it. A service/version with no update on the current Africa/Cairo day is frozen for new referrals; current zero is distinct from stale.
- A charity with stale services receives one daily reminder intent, with attempt outcomes and restart/retry handling. The intended schedule is 08:00 Cairo. SMTP acceptance is not proof of delivery and ambiguous sends cannot establish exactly-once delivery.
- Coordinators can allocate each need in portions to eligible charities. Allocation cannot exceed remaining need or available capacity. A charity submits a report and private evidence; coordinator acceptance is a review, not independent proof. Count each accepted quantity once, and close a request explicitly only when all needs are fulfilled and accepted.

## Candidate relational mapping

UUID primary and foreign keys and server-generated unique references for business records are the intended direction. Baseline names and types below are inherited proposals, not a verified description of the running model. `!` means required and `?` nullable. FK deletion behavior, indexes, codes, identity rules and constraints need review before a migration. The extension fields below are deliberately incomplete where the source proposal does not settle them.

### Retained core candidates

| Table | Candidate fields | Candidate rules and notes |
| --- | --- | --- |
| UserAccount | UserId uuid PK; DisplayName varchar(200)!; Email varchar(320)?; Phone varchar(40)?; AccountStatus varchar(20)!; CreatedAt timestamptz!; Version uuid! | Status Pending/Active/Disabled; partial UQ normalized email if used for login, after identity rules chosen. Contact is not proof of identity. |
| Role | RoleId uuid PK; Code varchar(40)! | UQ Code; seeded codes include Administrator, Coordinator, Beneficiary, legacy Requester and RecipientRepresentative. `Beneficiary` is role seed data, not a new table or migration. P1-10 grants Phase 1 self-service to Beneficiary; legacy Requester has no Phase 1 permissions, and third-party requester access is later scope. Administrator inherits Coordinator permissions while roles remain distinct. A user's functional roles may overlap. |
| UserRole | UserRoleId uuid PK; UserId uuid! FK UserAccount; RoleId uuid! FK Role; GrantedAt timestamptz! | UQ (UserId, RoleId). Role alone never grants unrelated request/evidence access. |
| Beneficiary | BeneficiaryId uuid PK; Reference varchar(32)! UQ; CreatedByUserId uuid! FK UserAccount; LinkedUserId uuid? FK UserAccount; DisplayName varchar(200)!; ContactDetails varchar(500)?; IsFamily boolean!; CreatedAt timestamptz! | Planned self-signup links the new account through LinkedUserId and sets CreatedByUserId to that account; independent profiles remain possible for later authorized intake. Partial UQ LinkedUserId when not null remains a proposed one-to-one account link. Sharing and family representation need validation. |
| ReferralRecipient | RecipientId uuid PK; Reference varchar(32)! UQ; RecipientType varchar(20)!; DisplayName varchar(200)!; ContactDetails varchar(500)!; ApprovalStatus varchar(20)!; Active boolean!; CreatedAt timestamptz!; Version uuid! | Type Charity/Contributor; status Pending/Approved/Rejected; CHECK Active = false unless Approved. Index (ApprovalStatus, Active). Only Approved and Active may receive a new referral. |
| Charity | RecipientId uuid PK/FK ReferralRecipient; RegistrationInformation varchar(1000)? | Must supply registration information or supporting application document before review; enforce transactionally. |
| RecipientMember | RecipientMemberId uuid PK; RecipientId uuid! FK ReferralRecipient; UserId uuid! FK UserAccount; IsActive boolean! | Planned charity signup creates a representative membership in the same transaction as the account and pending application. UQ (RecipientId, UserId); index (UserId, IsActive). Membership alone does not convey access to unrelated referrals. |
| RecipientDecision | RecipientDecisionId uuid PK; RecipientId uuid! FK ReferralRecipient; ActorUserId uuid! FK UserAccount; Decision varchar(20)!; Reason varchar(2000)?; DecidedAt timestamptz! | Decision Pending/Approved/Rejected; in Phase 1 coordinator reviews charities; contributor approval remains later scope. Append only; index (RecipientId, DecidedAt). |
| ApplicationAttachment | ApplicationAttachmentId uuid PK; RecipientId uuid! FK ReferralRecipient; StorageKey varchar(500)!; OriginalName varchar(255)!; MediaType varchar(150)!; SizeBytes bigint!; UploadedByUserId uuid! FK UserAccount; UploadedAt timestamptz! | SizeBytes > 0; UQ StorageKey; restrict access. Registration information or at least one attachment is required for a charity application. |
| AssistanceRequest | RequestId uuid PK; Reference varchar(32)! UQ; IsMultiNeed boolean!; BeneficiaryId uuid! FK Beneficiary; SubmittedByUserId uuid! FK UserAccount; Category varchar(30)?; ServiceSubtype varchar(100)?; Description varchar(4000)!; RequiredQuantity decimal(18,3)?; Unit varchar(50)?; Status varchar(30)!; AuthorityConfirmed boolean!; SharingAgreed boolean!; CreatedAt timestamptz!; Version uuid! | IsMultiNeed distinguishes legacy request-level demand from separately tracked needs. Self-submission and authority/sharing confirmation apply in Phase 1. |
| DecisionHistory | DecisionId uuid PK; RequestId uuid? FK AssistanceRequest; ReferralId uuid? (legacy target; mapping open); ActorUserId uuid! FK UserAccount; Action varchar(50)!; Reason varchar(2000)?; OccurredAt timestamptz! | Request decision history is required. The subject mapping for a Phase 1 referral portion and the exactly-one-subject rule need review. |

Existing requests remain legacy (`IsMultiNeed = false`) with their original fields, referrals, outcomes and decisions; they are not backfilled into needs. New multi-need requests use `IsMultiNeed = true` and cannot carry request-level classification or quantity. The mode cannot be changed after insertion. Legacy review, clarification and referral endpoints reject multi-need requests. Multi-need intake and per-need review have separate behavior; later referral work owns allocation of accepted needs. A DecisionHistory reference to the earlier Referral is intentionally not shown as a Phase 1 foreign key until the referral-portion mapping is resolved.

### Implemented catalog schema (P1-12)

The EF model and additive `ServiceCatalog` migration define `ServiceCategory` (UUID and unique code), `ServiceVersion` (category, positive version number and name), `UnitStandard` (service version and name), `StandardVersion` (standard, positive version number and effective timestamp), and `StandardComponent` (standard version, name, positive decimal quantity and measurement unit). Version numbers are unique within their parent; foreign keys restrict deletion of referenced parents. One service version can have multiple independently versioned standards. Needs reference standard versions; capacities and referrals do not yet use the catalog.

P1-13 adds draft and published states to service and standard versions. Existing catalog versions are preserved and marked published by the additive migration, including incomplete versions. New versions start as drafts; publishing a standard requires its service version to be published. PostgreSQL triggers make publication irreversible and protect published versions and their components, the category code, and the standard name and ownership. Publication and component or parent changes are serialized with row locks. Changes to published content require a new version. P1-14 owns the catalog API; the current-version rules and need model are described below. Future capacity and referral migrations must reference the appropriate versions. The P1-13 `Down` migration removes these protections and publication states; review and authorize rollback before using it with catalog data. The earlier `ServiceCatalog` rollback removes catalog tables and their contents and requires a reviewed backup and explicit authorization if data exists.

### Phase 1 extension candidates

| Table | Candidate fields; types and nullability open |
| --- | --- |
| CharityService | CharityServiceId PK; CharityId FK Charity; ServiceVersionId FK ServiceVersion; StandardVersionId FK StandardVersion; Enabled  |
| DailyCapacity | CapacityId PK; CharityServiceId FK CharityService; CairoDate ; Available ; UpdatedAt  |
| CapacityUpdate | CapacityUpdateId PK; CapacityId FK DailyCapacity; ActorId FK UserAccount; Available ; UpdatedAt  |
| ReminderIntent | ReminderId PK; CharityId FK Charity; CairoDate ; Status  |
| ReminderService | ReminderServiceId PK; ReminderId FK ReminderIntent; CharityServiceId FK CharityService |
| ReminderAttempt | AttemptId PK; ReminderId FK ReminderIntent; Result ; AttemptedAt  |
| RequestNeed (implemented) | Id uuid PK; RequestId FK AssistanceRequest; StandardVersionId FK StandardVersion; Quantity decimal(18,3); Status. Unique request/version pair; positive quantity; recorded version and request cannot be reassigned. Service version is derived through UnitStandard. |
| ReferralPortion | PortionId PK; NeedId FK RequestNeed; CharityServiceId FK CharityService; Reference UQ; AssignedQuantity  |
| PortionOutcome | OutcomeId PK; PortionId FK ReferralPortion; Reference UQ; Quantity ; Status  |
| PortionEvidence | EvidenceId PK; OutcomeId FK PortionOutcome; StorageKey UQ |
| PortionReview | ReviewId PK; OutcomeId FK PortionOutcome; ActorId FK UserAccount; Decision ; OccurredAt  |
| RequestClosure | ClosureId PK; RequestId FK AssistanceRequest; ActorId FK UserAccount; ClosedAt  |

The extension list is a starting point, not an exhaustive storage specification. In particular, report narrative, reporting actor, timestamps and evidence metadata must be resolved before this mapping can support the required outcome workflow. The drawing shows all fields listed here, not a claim that the list is migration-ready.

Further integrity rules include immutable published versions and components; unique charity/service/standard combinations; unique daily capacity per charity service and Cairo date; append-only capacity updates; and one reminder intent per charity and Cairo date with recorded attempts. Exact version compatibility must be enforced across CharityService and its standard, rather than inferred merely from separate foreign keys. UUID and reference generation, status domains, required fields, indexes and checks need reviewed EF mappings and migrations.

## Allocation and concurrency invariant

Matching and allocation need a consistent transactional decision over charity approval, supported service, exact version, daily freshness, remaining need and available capacity. Cross-row sums, evidence counts, state transitions and role/involvement authorization cannot be guaranteed by simple column checks alone. Re-read relevant balances and eligibility after locking, and commit allocation, capacity changes and history together.

The earlier candidate lock order was hierarchy advisory lock, affected requests by UUID, then capacity rows by UUID; capacity-only updates skip request locks. This is an **unvalidated proposal**, not an existing guarantee. P1-53 must settle every writer's lock order and validate races between allocations and capacity updates against isolated PostgreSQL. No new migration or database change follows from this document.

## Open decisions

- P1-27 introduces separate needs without converting legacy demand. Later referral work must map portions to needs without double counting. The older RequestLink approach is not an additional source of demand.
- Review the proposed one-to-one linked beneficiary account constraint and charity representative membership cardinalities before implementation. `Beneficiary` is planned role seed data, not a schema entity; no migration is implied by the signup decision. Remaining field types, nullability, status domains, reference generation, retention, indexes and cardinalities still require review.
- Decide how decision history associates with a Phase 1 portion, and how reports retain actor, narrative, time, attachment metadata and reviewer history. The source candidate does not fully specify these fields.
- Define version consistency constraints between service, standard, supported service, need and allocation; review P1-33's interim matching rule before any change.
- Design authorization, private evidence access and storage reconciliation; define and test the P1-53 concurrency protocol and migration/upgrade path. Later dispute, correction, contributor onboarding and third-party intake designs remain in the archive rather than being represented as Phase 1 commitments.

## Proposed verification scenarios

These are requirements for future tests, not completed results: self-only submission and unrelated-user denial; coordinator charity approval; mixed needs and split allocations without double counting; immutable historical versions and exact-version matching; Cairo-day stale versus current-zero capacity; reminder retry and ambiguous sends; competing allocations and capacity updates on isolated PostgreSQL; private evidence access and failed uploads; and rejection of premature closure.

## Code-first evolution

Review generated SQL and create new versioned EF migrations for approved changes. Preserve applied migrations and historical records. Test empty-database creation and applicable upgrades, backup and recovery using isolated environments. The current diagrams and this document do not authorize applying changes to a shared or production database.

### Current versions and need creation

The current service version is the highest-numbered published version in its category. For a unit standard belonging to that service, the current published standard version has the latest effective timestamp at or before the decision time; the highest version number breaks ties. Drafts and future-effective standards are excluded. A new service version owns its own standards. Historical needs retain their recorded published version and components; no substitution or conversion is performed.

`NeedVersionService` provides a non-reserving current-version preview and historical reads. Its creation method requires a caller-owned PostgreSQL read-committed transaction and a persisted multi-need request; it locks the request, then category and unit-standard rows in deterministic order, checks all selected versions, and saves all needs before the caller commits. These locks serialize selection with publication. A stale selection fails without creating needs. The multi-need submission candidate creates the request and decision history in the same transaction, rolls back on an unsuccessful need-creation result, rejects stale selections, and enforces linked-beneficiary authorization.

The additive migration keeps all legacy rows and adds the need table. Its rollback drops all need data and the request-mode marker; do not roll it back where multi-need data exists without an authorized recovery plan. Deploy the migration and updated application together before enabling multi-need writes: older application binaries do not recognize the new demand mode.

### Per-need review outcomes

Each need retains its classification, optional subtype and current review status. The need review history records the actor, time and decision or clarification response; existing legacy request and referral histories remain unchanged. Coordinators can request clarification, accept, or reject an individual need, with a reason required for rejection. A beneficiary response adds an explanation without changing the selected standard or quantity. The request remains under review until every need has a final decision; final review outcomes are fully accepted, partially accepted, and fully rejected. Rejected needs are not referral demand.

The review migration adds need classification and history and extends request statuses. Its rollback drops review history and classification and cannot restore the old request-status constraint while new statuses remain; use a reviewed preservation and forward-fix plan. Existing need statuses were not constrained before this migration and are retained; unknown states are not treated as final review decisions.
