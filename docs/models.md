# Helping Hands system models

Status: supporting models for the finalized [SRS](SRS.md), not implemented. BRD 1.3 remains the business authority. State labels below are proposed analysis states, not a final API contract. Mermaid diagrams are editable text; the EER specialization constraints and physical mappings are explained in [database design](database-design.md).

Editable draw.io files: [conceptual EER](diagrams/conceptual-eer.drawio) and [relational schema](diagrams/relational-schema.drawio). Their XML and connectors were checked, but their layout has not been visually verified in draw.io.

## System context

```mermaid
flowchart LR
 B[Beneficiary] -->|request and updates| H[Helping Hands coordination platform]
 T[Third-party requester] -->|request and updates| H
 A[Platform administrator] -->|registration decisions| H
 C[Platform coordinator] -->|review and referrals| H
 H <-->|assigned referrals and evidence| P[Approved charity or individual contributor]
 P -.->|goods money or services outside platform| B
```

## Use case overview

```mermaid
flowchart LR
 requester[Beneficiary / third-party requester] --> submit([UC-003 Submit and review request])
 coordinator[Coordinator] --> submit
 charity[Charity applicant] --> registration([UC-001 Review charity registration])
 admin[Administrator] --> registration([UC-001 Review charity registration])
 individual[Individual contributor applicant] --> contributor([UC-002 Review contributor registration])
 coordinator --> contributor
 coordinator --> review([UC-004 Split mixed request])
 coordinator --> assign([UC-005 Refer and assign portions])
 provider[Approved charity / contributor] --> outcome([UC-006 Submit outcome and evidence])
 coordinator --> outcome
 coordinator --> follow([UC-008 Follow up and close])
 requester --> progress([UC-007 View authorized progress])
 provider --> progress
```

## Activity: registration and approval

```mermaid
flowchart TD
 A[Charity or contributor registers] --> B[Application pending]
 B --> C{Authorized reviewer decision}
 C -->|Approve| D[Activate eligible participant]
 C -->|Reject| E[Record rejection]
 C -->|Leave pending| B
```

Administrator reviews charities; coordinator reviews individual contributors. Administration of accounts is not evidence of beneficiary verification.

## Activity: review, referral and follow-up

```mermaid
flowchart TD
 A[Submit request] --> B[Coordinator reviews]
 B -->|Clarification| A
 B -->|Decline with reason| Z[Record decision]
 B -->|Accept| C{Mixed needs?}
 C -->|Yes| D[Create linked categorized requests]
 C -->|No| E[Select categorized request]
 D --> E
 E --> F[Assign available unassigned portion]
 F --> G[Recipient provides support outside platform]
 G --> H[Submit outcome and evidence]
 H --> I{Coordinator review}
 I -->|Clarification or dispute| H
 I -->|Accept partial| J[Record accepted portion; keep rest unresolved]
 I -->|Accept full| K[Complete referral]
 J --> L{Release undelivered portion?}
 L -->|Yes| M[Release; retain referral history]
 M --> P{Unassigned need available?}
 P -->|Yes| F
 P -->|No| Q[Wait or follow up on assigned portions]
 Q --> G
 L -->|No| G
 K --> N{All needs fulfilled?}
 N -->|Yes| O[Complete request]
 N -->|No| P
```

If work stops with unmet need, the coordinator may close the request unresolved with a reason. A partial outcome cannot itself complete an entire referral. A released undelivered portion is available for another referral; the historical referral does not become fulfilled merely because it was released.

The proposed late-dispute handling also applies after an outcome has been accepted: a recorded disagreement is linked to the affected outcome, or to the request if the specific outcome is unclear; the affected request is flagged and its allocation is frozen pending coordinator resolution. A disputed child prevents its split ancestors from being displayed as completed. An audited correction preserves previous decisions and recomputes balances and ancestor outcomes. See the SRS and database design for the proposed rules and their test scenarios.

## Sequence: concurrent assignment (proposed)

```mermaid
sequenceDiagram
 actor C as Coordinator
 participant API as API
 participant DB as PostgreSQL
 C->>API: Assign quantity to approved recipient
 API->>DB: Begin transaction; lock request allocation
 API->>DB: Read need, accepted fulfillment and active assignments
 DB-->>API: Current totals
 alt Quantity is available
 API->>DB: Record referral and decision; commit
 API-->>C: Assignment recorded
 else Exceeds unassigned need or stale state
 API->>DB: Roll back
 API-->>C: Conflict; refresh current totals
 end
```

## Sequence: evidence review and partial delivery

```mermaid
sequenceDiagram
 actor P as Referral recipient
 actor C as Coordinator
 participant API as API
 participant Files as Protected evidence storage
 participant DB as PostgreSQL
 P->>API: Submit reported portion and evidence
 API->>Files: Validate and store protected file
 API->>DB: Record outcome and evidence metadata
 C->>API: Review outcome
 alt Clarification or dispute
 API->>DB: Record unresolved review and reason
 else Accept partial
 API->>DB: Record accepted quantity and partial status
 API-->>C: Referral incomplete; rest remains assigned until released
 else Accept report completing the assigned portion cumulatively
 API->>DB: Record accepted outcome and complete referral
 end
```

File/database failures must not falsely mark an outcome complete; the design must handle cleanup and recovery of a file whose metadata operation fails. Evidence review records a judgment, not independent proof.

## Conceptual domain / EER overview

```mermaid
classDiagram
 class Participant {
  identity and contact
 }
 class ReferralRecipient
 class Charity {
  registration information
 }
 class Contributor
 class Beneficiary
 class Request {
  category and need
 }
 class Referral {
  assigned portion
 }
 class Outcome {
  reported and accepted result
 }
 class Evidence {
  protected file metadata
 }
 ReferralRecipient <|-- Charity : subtype
 ReferralRecipient <|-- Contributor : subtype
 Participant "0..*" --> "0..*" ReferralRecipient : represents
 Participant "1" --> "0..*" Request : submits
 Beneficiary "1" --> "0..*" Request : concerns
 Request "1" --> "0..*" Referral : has
 ReferralRecipient "1" --> "0..*" Referral : assigned to
 Referral "1" --> "0..*" Outcome : reports
 Outcome "1" --> "0..*" Evidence : supports
 Request "0..1" --> "0..*" Request : split into linked requests
```

The proposed referral-recipient specialization comprises charity and individual contributor; it is disjoint for a particular recipient profile and total for a referral target. A person can separately participate in other roles. Other participants in the conceptual diagram are not required to be referral recipients. Physical integrity and the mapping of this conceptual distinction are in [database design](database-design.md). Evidence may be collected in multiple attachments. Submitted evidence alone does not establish completion.

## Request state model

```mermaid
stateDiagram-v2
 [*] --> Submitted
 Submitted --> UnderReview
 UnderReview --> ClarificationNeeded
 ClarificationNeeded --> UnderReview
 UnderReview --> Declined: reason recorded
 UnderReview --> Accepted
 UnderReview --> Split: mixed needs separated
 Split --> Completed: all linked child needs fulfilled and accepted
 Split --> ClosedUnresolved: work stops with unmet child need and reason
 Accepted --> InProgress: referral made
 InProgress --> Completed: all required need fulfilled and accepted
 Accepted --> ClosedUnresolved: work stops with reason
 InProgress --> ClosedUnresolved: work stops with unmet need and reason
```

Linked requests retain their relationship to the original. Under the proposed database design, a split parent is not allocatable: its children carry the distinct needs, and its displayed outcome is derived from their outcomes without counting the parent need again. Its status is completed only if all required child needs are fulfilled and accepted; it can be closed unresolved with a reason if work stops with unmet child need. These are proposed model details, not extra BRD decisions.

## Referral state model

This is a combined conceptual lifecycle: `Reported` and `ClarificationNeeded` describe outcome review, while the separate database proposal keeps those states on Outcome rather than Referral. Declined assignments retain their unresolved amount until explicitly released. The physical statuses and release history are specified in the database design.

```mermaid
stateDiagram-v2
 [*] --> Assigned
 Assigned --> Reported: outcome and evidence submitted
 Assigned --> Declined: recipient declines
 Reported --> ClarificationNeeded: insufficient or disputed
 ClarificationNeeded --> Reported: revised evidence
 Reported --> PartiallyFulfilled: coordinator accepts partial outcome
 PartiallyFulfilled --> Reported: further outcome submitted
 PartiallyFulfilled --> PartiallyFulfilled: coordinator releases undelivered portion
 Assigned --> Released: coordinator releases undelivered portion
 Declined --> Released: coordinator releases undelivered portion
 Reported --> Completed: assigned portion fulfilled and accepted cumulatively
```

Released and declined referrals remain historical records; release does not mean fulfillment. After partial fulfillment, the referral remains historically partially fulfilled even when its entire undelivered balance is released; its released balance is no longer assigned, and the history records the release. Partial acceptance does not complete the referral. The aggregate request can remain open while its unresolved need is assigned or unassigned.

## Monolithic component design

```mermaid
flowchart LR
 Web[React Native Web + PWA client - future] -->|HTTPS JSON| API
 subgraph API[One ASP.NET Core API application]
 Auth[Authentication and authorization]
 Cases[Requests and referrals]
 Registry[Participant approval]
 Evidence[Evidence and outcome review]
 Audit[Decision history and logging]
 end
 API --> SQL[(PostgreSQL - future)]
 API --> Files[Protected file storage - future]
```

The current scaffold has only `/health`; none of these business components or external dependencies is implemented. See [architecture and compatibility](architecture.md).
