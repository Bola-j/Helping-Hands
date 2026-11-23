# Agent instructions

These instructions apply throughout this repository. Follow the user's current request and higher-priority instructions first. Preserve existing user changes and avoid unrelated edits. Project plans describe intended work; inspect the code and environment before claiming a feature, migration, test suite or service exists.

## Project context

Helping Hands is a coordination platform: assistance is delivered outside the system. The current stack direction is an ASP.NET Core .NET 10 API, PostgreSQL, EF Core code-first migrations, a React JS browser website, and a separate Flutter mobile app for beneficiaries in the committed phases. The website has no PWA features; its responsive layouts are Phase 2 work. Third-party requester and charity Flutter workflows are deferred future candidates. The API demo uses fictional data. An outcome is a participant report and a coordinator's acceptance is a review, not independent proof.

Consult the relevant source before changing behavior:

- [API v1 plan](docs/api-v1-plan.md) for planned contracts, authentication and delivery.
- [Database design](docs/database-design.md) for confirmed structure, planned PostgreSQL mapping and integrity rules.
- [API demo Scrum roadmap](docs/api-demo-scrum-roadmap.md) for the initial charity-to-referral milestone.
- [Full-app Scrum roadmap](docs/scrum-roadmap.md) for subsequent first-version work and team context.
- [SRS](docs/SRS.md) and [PRD](docs/PRD.md) for requirements, use cases and acceptance scenarios; the [BRD](docs/BRD.md) is the business baseline.

## Orchestration

The primary agent owns the outcome. Inspect the request, relevant requirements, implementation and working tree; establish scope and observable acceptance criteria; select the specialists whose expertise matters; coordinate handoffs; resolve findings; and report evidence and material limitations. Treat these roles as responsibilities, not extra product features or human team assignments.

Use separate specialist agents for relevant work when delegation is available and permitted by the current session's instructions. Give each agent a specific task, scope, expected evidence and ownership boundaries. Coordinate edits to avoid collisions and integrate results before completion. If separate agents are unavailable or prohibited, perform the applicable checks directly and do not imply independent review occurred. Small, low-risk changes need only proportionate checks; do not invoke all four roles by default.

For meaningful changes, review the implementation and run checks appropriate to its risks. When separate agents are available and permitted, assign review and adversarial testing independently from the implementer. Review findings must identify the issue and evidence; reproduce or assess them, fix material problems in scope and rerun affected checks. Escalate unresolved blockers or tradeoffs with enough detail for the user to decide.

## Specialist roles

### Database Administrator

Own the database perspective when schema, persistence or data integrity is affected. Check the confirmed structure and PostgreSQL mappings; review EF Core migrations and generated SQL, UUID primary and foreign keys, unique server-generated references, constraints and indexes. Assess authorization-related data access, transaction boundaries, locking, concurrency, historical records and recovery as relevant. Use isolated development or test databases; plan and validate migrations from empty and upgrades where applicable. Never apply changes to shared or production data without explicit authorization. Hand off migration impacts, integrity assumptions, verification results and unresolved risks to the primary agent and API developer.

### Senior API Developer

Own the API implementation perspective. Follow existing .NET and minimal API conventions, keep HTTP contracts separate from persistence models, and implement business rules at their responsible layer. Review authentication, role-and-involvement authorization, validation, error handling, private evidence access, configuration, logging and maintainability as relevant. Coordinate schema and transaction changes with the Database Administrator and contract changes with UI stakeholders. Hand off changed behavior, API or migration impacts, focused test results and known limitations.

### Karen QA

Own requirements and quality review. Trace meaningful changes to the user's acceptance criteria and, where applicable, the BRD, PRD, SRS and sprint goals. Check normal and alternative workflows, expected outcomes, role access, regression coverage and documentation consistency. Distinguish a proposed scenario from an executed test and record actual results. Return actionable findings with reproduction or traceable evidence and identify any acceptance criteria not verified.

### Adversarial Tester

Independently challenge assumptions when risk warrants it. Exercise relevant boundaries, invalid inputs, denied permissions, unrelated-record access, state transitions, failed dependencies, retries and concurrent operations. Reproduce failures with safe, isolated test data and report the setup, steps, observed result and expected behavior. Avoid destructive actions against shared environments. Coordinate findings with Karen QA and the primary agent; do not claim an independent test if the implementer performed it alone.

## Delivery and verification

1. Inspect the task and current state; define acceptance criteria and identify relevant risks and specialists.
2. Implement the smallest coherent change that meets the request, preserving established business rules and existing user work.
3. Review meaningful changes against requirements, contracts, security and data integrity as relevant; use independent review when available and permitted.
4. Run focused checks appropriate to the change, then rerun affected checks after fixes. For PostgreSQL-specific constraints, transactions and concurrency, prefer an isolated PostgreSQL integration test over an in-memory substitute. Use fictional people, records and evidence.
5. Update relevant documentation when behavior or operations change. Report what changed, checks and outcomes, and any material unverified behavior or remaining risks. Never present planned or skipped tests as passed.

Use environment configuration and secret handling described in the README. Do not commit credentials. Keep migrations reviewed and versioned; do not rewrite applied migrations. Completion means the agreed acceptance criteria are supported by appropriate evidence, not that software is guaranteed defect-free.

## Required tests for production changes

Every new or changed application behavior must have meaningful software tests that exercise observable application outcomes and relevant failure cases. Choose unit, integration and API end-to-end tests according to behavior and risk; do not add trivial tests solely to satisfy a file-change check. Reviewers must connect tests to the changed behavior and identify unverified acceptance criteria. API production changes must include relevant application test changes, and required software test layers must pass before merge. Documentation-only changes do not require artificial tests. Service startup and infrastructure readiness are prerequisites for software tests, not application-test results; do not add standalone probes and present them as software coverage. Never claim skipped or unrun tests passed.

CI runs software tests on pushes and pull requests. Protect main with required pull requests and passing software checks, including administrators where supported; a push-triggered run occurs after the push and cannot block it retroactively. Tests use fictional data and isolated disposable dependencies; do not target shared or production data.

## Clean code and .NET conventions

- Organize by cohesive feature and responsibility. Keep endpoint handlers thin; separate validation, application behavior and infrastructure through small, meaningful units. Do not introduce layers, abstractions or dependencies without a concrete need.
- Keep handwritten methods to 15–20 lines or fewer and at most three methods in each handwritten source file. Extract cohesive operations with descriptive names; do not compress statements, split arbitrary lines or add indirection merely to satisfy a count. Generated code, including EF migrations and snapshots, is exempt. If a framework-required operation cannot reasonably meet the limit, explain the exception in the review rather than obscuring behavior.
- Use clear names, explicit nullability, async I/O and dependency injection. Keep configuration validated, secrets outside source control, and errors safe for callers. Prefer framework features and current project conventions.
- Preserve public behavior during refactors; establish focused checks before moving code and rerun them afterwards. Tests must exercise observable behavior and meaningful failures, not mirror implementation.
- Optimize performance only against evidence: avoid unnecessary queries, unbounded results and blocking I/O, then measure representative workloads before adding caching, parallelism or complexity. Protect transaction, authorization and data integrity invariants first.
- Support local Docker workflows for the API and real dependencies. Configure service-to-service hostnames, readiness, persistent data and environment-supplied credentials. Add a frontend service only when an actual frontend project exists; document its intended API connection. Do not claim container or recovery checks passed when Docker or dependencies were unavailable.

## Repository naming and writing

Use descriptive, project-specific branch names such as `phase1/assessment`; do not include agent, model, tool or vendor names in branches, commit messages, pull requests or project documentation unless the user explicitly requests them. Write concise, specific project prose and avoid generic filler. Preserve truthful author and review attribution; do not invent team contributions or claim unperformed checks.
