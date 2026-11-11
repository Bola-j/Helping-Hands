# Helping Hands

Helping Hands is a proposed platform that helps people request assistance and lets platform coordinators refer requests to approved charities or individual donors and contributors. It records who handles each referral, its progress, and evidence of reported delivery and receipt.

The platform does not collect or hold money or goods, operate a warehouse, manage inventory or charity accounting, or take a share of contributions. The charity or contributor delivers assistance outside the platform.

## How it works

1. A charity self-registers. A platform administrator approves, rejects, or leaves the application pending; approval activates the charity account.
2. A beneficiary or third party submits a request, or a coordinator records it on their behalf. Requests can be for goods, money, or an organizational referral subtype such as tutoring or transport.
3. A coordinator reviews the request and may split mixed needs into linked requests. Any request type can be referred to an approved charity or individual contributor, and a request can be divided among several recipients.
4. Each recipient handles its assigned portion and supplies evidence that the assistance was delivered and received, including for money. The platform tracks the reported outcome and any unresolved need.

Individual contributors register and need coordinator approval before referral. Recipients provide an outcome description and attachment(s) addressing delivery and receipt; a coordinator reviews them before marking a referral complete. Access is limited by role and involvement. Evidence and review are not a guarantee of independently verified delivery.

## First version

The full first-version scope covers charity and contributor approvals, request submission and review, splitting and referrals, evidence, progress tracking, and role-appropriate access using fictional people and sample data. The course delivery sequence demonstrates the core charity-to-referral workflow first, then adds contributor approval, partial fulfillment, and mixed-request splitting; sequencing does not remove them from scope. Real donations, payment processing, subscription billing, detailed service delivery management, and live operations are outside the demonstration.

The potential service model may be supported by subscriptions, grants, or sponsorships; adoption and funding require validation. A real launch also requires partner input and appropriate local review.

## Project team

- **Bola Ghaly** — backend development.
- **Ziad Walid** — database and architecture design.
- **hussien okely** and **ali fathy** — web application UI.

These are areas of responsibility; the team collaborates on planning, integration, reviews, and testing. Scrum roles have not been assigned.

## Project documents

- [Business Requirements Document (version 1.3)](docs/BRD.md)
- [Product Requirements Document](docs/PRD.md)
- [Software Requirements Specification](docs/SRS.md)
- [System models](docs/models.md), [Phase 1 database design proposal](docs/database-design.md), and [architecture and compatibility](docs/architecture.md)
- [API v1 plan](docs/api-v1-plan.md) and [full-app Scrum roadmap](docs/scrum-roadmap.md)
- [Full idea and business model — original reference](docs/references/helping-hands-full-idea.md). The current BRD supersedes its inventory, financial management, and direct service-coordination model.

## API foundation

`src/HelpingHands.Api` is the only ASP.NET Core .NET 10 project in `HelpingHands.sln`. It uses a minimal API for its sole process-liveness endpoint. No business workflows, authentication, frontend, database connection, or EF-generated models are implemented yet.

With the .NET 10 SDK installed, copy the example once and edit the local settings:

```powershell
Copy-Item .env.example .env
./scripts/run-api.ps1
```

The PowerShell launcher reads root `.env` as plain, unquoted `KEY=VALUE` lines, skips blank lines and `#` comments, and passes them as process environment variables to the API. Values can contain `=`. It restores any previous process values on exit. The default URL in the example is `http://127.0.0.1:5087`; visit `/health` to see `{"status":"ok"}`. The endpoint checks only the process, not PostgreSQL. To stop the application, press Ctrl+C. `dotnet run` by itself does not load `.env`; use the launcher or supply operating-system environment variables yourself. Build separately with `dotnet build HelpingHands.sln`.

For Docker, edit `.env` to use `ASPNETCORE_URLS=http://+:8080` inside the container, then run:

```powershell
docker build -t helping-hands-api .
docker run --rm --env-file .env -p 8080:8080 helping-hands-api
```

Visit `http://localhost:8080/health`. The scaffold does not require PostgreSQL; the commented connection string in `.env.example` is a future placeholder, not an active database integration. `.env` and its variants are Git-ignored and excluded from Docker build context. Keep real credentials out of `.env.example` and source control; production secrets should come from the deployment environment or secret manager. Docker and the launcher read `.env` separately; avoid quoted values, variable expansion, and inline comments for portability. See the [database design](docs/database-design.md) for planned migrations.

## Repository status

The BRD is the business baseline. The PRD and SRS are finalized working requirements baselines, subject to normal change control; the database structure is confirmed; supporting models and the PostgreSQL mapping still require implementation and verification. The API foundation is runnable, but its planned business capabilities are not implemented.

The Git commit dates in the initial project sequence were reconstructed at the project owner's request; they do not establish when work or validation actually occurred. Recorded stakeholder and owner validation dates in the documents retain their actual meaning.
