# Helping Hands — Business Requirements Document

## 1. Document information

| Field | Value |
| --- | --- |
| Version | 1.7 |
| Updated | 2026-11-22 |
| Status | Working business baseline; request review outcome decision recorded |
| Prepared by | Helping Hands project team |

| Version | Change |
| --- | --- |
| 0.1 | Initial business requirements draft. |
| 0.2 | Introduced organizational referrals and referrals for all request types. |
| 0.3 | Added charity registration and evidence; removed platform custody, inventory, and financial management. |
| 1.0 | Finalized first-version business rules and recorded project-owner confirmation. |
| 1.1 | Clarified terminology, completion, scope priorities, and proposed partial-fulfillment rules following BRD feedback. |
| 1.2 | Adopted practical first-version rules and recorded delegation of remaining decisions. |
| 1.3 | Distinguished unresolved from unassigned needs and partial outcomes from referral completion. |
| 1.4 | Reconciled the latest Phase 1 scope and coordinator charity approval; Scrum Master review accepted on 11 November 2026. |
| 1.5 | Recorded the project decision that Administrator inherits Coordinator permissions while remaining a distinct role. |
| 1.6 | Recorded per-need review and fully accepted, partially accepted, or fully rejected request outcomes. |
| 1.7 | Separated Phase 1 backend readiness evidence from the complete web application rehearsal and evaluation. |

The [original idea and business model](references/helping-hands-full-idea.md) is background. This document supersedes its direct inventory, finance, and organizational-help management model. The project owner validated the earlier BRD, requested finalization, and delegated the remaining decisions to practical business judgment. Version 1.3 adopted those decisions as the earlier baseline; version 1.4 records the later Phase 1 project direction; version 1.5 records the explicit identity-access decision; version 1.6 records the project direction for mixed-need review outcomes; version 1.7 separates backend readiness evidence from full-app evaluation. No instructor, charity, or other stakeholder approval is implied.

## 2. Executive summary

Helping Hands is a coordination platform that helps people request assistance and helps a platform coordinator refer those requests to approved charities or individual donors and contributors. The platform records decisions, referrals, progress, and evidence of reported delivery and receipt. Money, goods, and services are handled outside the platform by the parties providing and receiving help.

The course project aims to demonstrate a small, functioning service with fictional data. The longer-term idea is a coordination service that could be supported by subscriptions, grants, or sponsorship; adoption and funding have not been validated.

## 3. Business background and problem

People and organizations may exchange requests and offers through calls, visits, referrals, and messages. When information is scattered, it can be difficult to know who is handling a request, what remains outstanding, and whether help was delivered and received. The proposed platform provides a shared record and a way to review participating charities and evidence. The current practices and scale of these problems at a real partner have not yet been confirmed.

The platform does not collect or hold donations, operate a warehouse, manage a charity's inventory or finances, or take a share of a contribution. A charity or donor is responsible for providing its assigned help; the charity's own internal operations are outside the platform.

## 4. Business objectives

| ID | Objective |
| --- | --- |
| BO-01 | Make requests and their current responsibility and progress visible to authorized participants. |
| BO-02 | Connect requests to approved charities or individual contributors without the platform taking custody of aid. |
| BO-03 | Improve accountability through recorded decisions, evidence, and review of reported outcomes. |
| BO-04 | Protect beneficiary information and limit access to what is needed for a participant's role. |
| BO-05 | Test whether a simple coordination service is useful and sustainable with real stakeholders. |

## 5. Stakeholders

| Stakeholder | Role and interest |
| --- | --- |
| Beneficiary | Self-registers for the Phase 1 demonstration, requests or receives assistance, and needs appropriate privacy and a way to understand progress. |
| Third-party requester | Submits a request for another person, such as a relative, neighbor, or teacher. |
| Platform administrator | Manages platform access and inherits all coordinator permissions while remaining a distinct role. |
| Platform coordinator | Reviews charity applications and requests, divides needs when appropriate, makes referrals, and follows outcomes. |
| Charity | A representative self-registers an account and pending charity application; after coordinator approval, the charity handles referrals and supplies evidence of assistance. |
| Individual donor or contributor | Registers an account, may handle a referred portion after coordinator approval, and supplies evidence of assistance. |
| Project team and instructor | Build and assess the course project. |
| Potential sponsor or funding partner | May support ongoing service costs; participation remains unconfirmed. |

## 6. Scope

### In scope for the first version

| ID | Capability |
| --- | --- |
| IS-01 | A charity representative self-registers an account and pending application; an authorized coordinator leaves it pending, approves it to activate the charity, or rejects it. |
| IS-02 | Beneficiaries self-register for the Phase 1 demo and submit their own requests. Third-party requesters and coordinators can submit on behalf in the full first version. |
| IS-03 | Coordinators review each need and classify it as goods, money, or organizational referral. Other needs, such as tutoring, repairs, and transport, are subtypes of organizational referral; they may be fulfilled by an organization or an individual contributor. |
| IS-04 | A coordinator can separately track the different kinds of support within a request and refer any request type to approved charities or individual contributors. |
| IS-05 | A coordinator can divide a request among multiple referral recipients, track each assigned portion separately, and see any remaining need. |
| IS-06 | The responsible charity or donor provides evidence that its assigned help was delivered and received; the platform keeps the evidence and reported outcome with the relevant referral. |
| IS-07 | Authorized users can follow request and referral progress and see a history of significant decisions. |
| IS-08 | Individual contributors register and become eligible for referrals after coordinator approval. |

### Course delivery priorities

The complete scope above remains the first-version goal. The Phase 1 course demo uses fictional data and includes charity registration and coordinator approval, beneficiary self-submission of multiple needs, coordinator review and matching, split allocations to eligible charities, private outcome evidence, coordinator review, progress and explicit closure. Coordinators manage versioned service categories and structured unit standards; charities provide daily available-from-now capacity and receive an in-app task and 08:00 Africa/Cairo reminder for stale services. These are planned capabilities, not implementation evidence.

The team is planning an expedited backend, PostgreSQL, local-container and API CI increment with **12 November 2026** as a forecast target, not a delivery commitment. The first proposed milestone establishes a reproducible local API, database, private evidence storage and a CI baseline; later milestones deliver the Phase 1 business workflow and its integration checks. The complete [backend delivery breakdown](phase1-backend-delivery-breakdown.md) identifies dependencies and a fallback if the forecast is not achievable. This scheduling decision does not remove the web client or any full-product capability from scope.

The backend has a separate readiness gate: rehearse the fictional workflow through an API client against isolated PostgreSQL, private evidence storage and email, and record actual results, denied access and limitations in [P1-58](https://github.com/Bola-j/Helping-Hands/issues/132). This evidence can support a backend demonstration. The complete browser-to-API rehearsal [P1-47](https://github.com/Bola-j/Helping-Hands/issues/47) and final Phase 1 evaluation evidence [P1-48](https://github.com/Bola-j/Helping-Hands/issues/48) remain separate gates for the full application.

Third-party or coordinator-on-behalf intake, contributor onboarding and live operational deployment are later scope. Explicit release/reassignment after an accepted partial outcome remains a later first-version increment; partial assignment among charities and separately tracked need balances are included in Phase 1. For outstanding requests after a standard changes, the Scrum Master selected exact-version matches only until review in [P1-33](https://github.com/Bola-j/Helping-Hands/issues/33); no incompatible version may be substituted, and unmatched needs remain open.

### Out of scope

- The platform collecting, holding, processing, purchasing, storing, or distributing money or goods; warehouse, inventory, delivery logistics, and charity accounting systems.
- Taking a percentage of donations or assistance; subscription billing and payment integration in the course version.
- Managing the detailed scheduling, staffing, or supervision of services delivered by a recipient.
- A real operational pilot in the course demonstration; use fictional people and sample data.
- A public listing or dashboard of requests in the first version.

## 7. Current business process

Requests and offers may be communicated through several channels, with organizations independently reviewing needs and arranging help. No specific partner's actual process, tools, or pain points have been verified. Interviews or observation are needed before describing a confirmed current-state process.

## 8. Proposed business process

1. A beneficiary creates an account and linked beneficiary profile through beneficiary signup. A charity representative creates an account and pending charity application through the separate charity signup flow; the coordinator can keep it pending, approve and activate it, or reject it.
2. A beneficiary submits a request. Third-party and coordinator-on-behalf intake are full first-version scope, sequenced later than the Phase 1 demo.
3. A coordinator reviews each need, records its classification, and accepts, rejects with a reason, or requests clarification. The beneficiary may supply an explanation without changing the selected standard or quantity. The request remains under review until each need has a final decision; it is fully accepted when all needs are accepted, partially accepted when decisions are mixed, and fully rejected when all needs are rejected.
4. A coordinator refers a request or a defined portion to one or more approved charities or individual contributors. Each portion has its own responsible referral recipient and progress record.
5. The referral recipient handles the assistance outside the platform and supplies evidence that it was delivered and received, including for monetary assistance.
6. The outcome and evidence are recorded against the referral. The coordinator follows up on partial, declined, or unsuccessful referrals and records the next action. The original request remains visible while a need is unresolved.

A coordinator reviews submitted evidence and either accepts the reported outcome, requests clarification, or records the referral as unresolved. A referral is completed only after acceptance. The coordinator explicitly closes a completed request only after all required needs are fulfilled and accepted. Acceptance records a review decision; it is not a guarantee of independent verification.

For the first version, the responsible referral recipient submits an outcome description and supporting attachment(s) that together address both delivery and receipt. For money this includes the amount and evidence of transfer and receipt; for goods or services it identifies the assistance and evidence of its provision and receipt. The coordinator judges whether the evidence supports the assigned portion and may request more information. Demonstrations use fictional evidence.

## 9. High-level business requirements

| ID | Requirement | Objective |
| --- | --- | --- |
| BR-01 | The platform needs to accept charity registrations and allow an authorized coordinator to approve, reject, or leave them pending. | BO-02, BO-03 |
| BR-02 | Only approved and activated charities are eligible to handle referrals made to them. | BO-02, BO-04 |
| BR-03 | The platform needs to receive and track requests submitted by beneficiaries, third parties, or coordinators. | BO-01 |
| BR-04 | Coordinators need to review requests, record decisions, categorize needs, and separately track mixed needs and preserve their relationship to the original request. | BO-01, BO-03 |
| BR-05 | Coordinators need to refer any type of request to approved charities or individual contributors, including splitting it into separately tracked portions. | BO-01, BO-02 |
| BR-06 | Referral recipients need to document each assigned portion's outcome and provide evidence of delivery and receipt, including for money. | BO-03 |
| BR-07 | Authorized participants need to see relevant progress, unresolved needs, evidence, and a history of decisions and updates. | BO-01, BO-03, BO-04 |
| BR-08 | The platform needs to restrict access to beneficiary details and evidence according to role and involvement. | BO-04 |
| BR-09 | The project team needs feedback from potential participants on usefulness, operating arrangements, and sustainability. | BO-05 |
| BR-10 | Coordinators need to approve individual contributors before referral and review evidence before marking a referral complete. | BO-02, BO-03 |

Detailed product behavior and account access are developed in the [PRD](PRD.md) and [SRS](SRS.md).

## 10. Business rules

These rules form the first-version business baseline. The owner decided the main direction and delegated remaining choices to practical business judgment. Operating policies remain subject to validation before launch.

| ID | Rule | Status |
| --- | --- | --- |
| BRule-01 | Charity registration does not activate an account until an authorized coordinator approves it; rejected or pending charities cannot handle referrals. | Project decision |
| BRule-02 | Authorized platform coordinators review requests and determine referrals. | Baseline |
| BRule-03 | Goods, money, and organizational referral requests can each be referred to a charity or individual contributor. | Project decision |
| BRule-04 | Different kinds of support within a request may be separately tracked while retaining their relationship to the original; one request may also be divided among recipients. | Project decision |
| BRule-05 | Each referral recipient is responsible for its assigned assistance and provides an outcome description and supporting attachment(s) addressing delivery and receipt. A coordinator reviews the evidence before marking the referral complete. | Baseline |
| BRule-06 | Helping Hands does not take custody of aid or a share of a contribution. | Project decision |
| BRule-07 | Administrators and coordinators access information needed for their work; a referral recipient accesses only its assigned referral and necessary beneficiary details and evidence. Requesters can follow their own requests. A third-party requester sees only the information needed to follow the request they submitted. | Baseline |
| BRule-08 | A recorded outcome is a participant's report; coordinator review does not imply independent verification. Administrator inherits Coordinator permission to review outcomes. | Baseline |
| BRule-09 | Decisions and referral updates record who acted and when; unresolved or declined portions remain visible for follow-up. A request is **completed** when all its required need is fulfilled and accepted; it remains **open** while further action is expected; it may be **closed unresolved** when work stops with unmet need and a recorded reason. | Baseline |
| BRule-10 | Charity applicants provide their organization name, contact details, and registration information or supporting documentation. An authorized coordinator reviews these before approval, rejection, or leaving the application pending; approval is not proof of future conduct. | Baseline |
| BRule-11 | Individual contributors register with contact information; a coordinator approves them before making referrals. | Baseline |
| BRule-12 | The requester must confirm authority to submit information and agree to necessary sharing for referral. Coordinators share only information needed by the selected recipient. Sensitive evidence is restricted, and participants should omit unnecessary personal information. | Baseline; local operational review required before launch |
| BRule-13 | Independent beneficiary confirmation is not required for the course demonstration. The referral recipient provides the evidence; any beneficiary or authorized third-party confirmation and any disagreement are recorded separately. A disputed or insufficient outcome remains unresolved for coordinator follow-up. | Baseline |
| BRule-14 | Remaining need includes both assigned and unassigned unresolved portions. Only an unassigned or explicitly released portion is available for assignment or reassignment; an assigned portion cannot be assigned to another referral recipient at the same time. An accepted partial delivery records the fulfilled amount but does not complete the whole referral: the undelivered portion remains unresolved. The coordinator may release that portion for reassignment while retaining the original referral and outcome history. For service needs without a meaningful quantity, the coordinator defines a clear, separately trackable scope for each portion. | Baseline |

For example, for 10 food boxes, a coordinator assigns 6 to charity A and 4 to charity B. If A's accepted outcome covers only 3, A's referral is partially fulfilled, not completed. Seven boxes remain unresolved: 3 still assigned to A and 4 assigned to B. None is available for reassignment while assigned. If the coordinator releases A's undelivered 3, those 3 become unassigned and available for referral again; B's 4 remain assigned. The original referral and partial outcome stay in the history.

## 11. Assumptions, constraints, and dependencies

| ID | Type | Item | Status |
| --- | --- | --- | --- |
| A-01 | Assumption | Charities and contributors will be willing and able to provide meaningful evidence. | Working assumption |
| A-02 | Assumption | Participants can arrange the actual assistance outside the platform. | Working assumption |
| C-01 | Constraint | The project must fit the university's schedule and submission requirements. | Applicable course requirements govern |
| C-02 | Constraint | The demonstration uses fictional data and no real donations or payments. | Project scope |
| D-01 | Dependency | The team must align deliverables with the applicable course requirements. | Applies to course submission |
| D-02 | Dependency | Charity, donor, and beneficiary feedback informs practical adoption. | Applies before real operation |
| D-03 | Dependency | Appropriate local and legal review, including handling of donations, privacy, consent, and evidence retention. | Required before real operation |

## 12. Success criteria

| ID | Criterion | How to check | Objective |
| --- | --- | --- | --- |
| SC-01 | A charity registration can remain pending, be rejected, or be approved and activated; only approved charities can handle referrals. | Demonstrate each decision with sample accounts. | BO-02, BO-03 |
| SC-02 | A direct or third-party request can be reviewed and referred to an approved charity or an approved individual contributor. | Demonstrate with fictional data. | BO-01, BO-02 |
| SC-03 | A mixed request can be tracked as distinct related needs, and each need can be divided among referral recipients with each portion tracked separately. An accepted partial outcome leaves the referral incomplete and its undelivered portion unresolved; only unassigned or released need can be reassigned. | Demonstrate the resulting need relationships, responsibilities, partial outcome, and (in later full-app scope) release and reassignment of a portion. | BO-01, BO-02 |
| SC-04 | A recipient can provide a description and attachment(s) for delivery and receipt; a coordinator can request clarification or accept the outcome before completion. | Demonstrate fictional goods, money, and service evidence and review. | BO-03, BO-04 |
| SC-05 | A user cannot access another participant's restricted beneficiary information or evidence without the appropriate role and involvement. | Exercise representative access scenarios. | BO-04 |
| SC-06 | The team seeks stakeholder feedback on usefulness and practical operation; feedback is a validation goal rather than a prerequisite for demonstrating the course prototype. | Review feedback notes where participants are available. | BO-05 |
| SC-07 | An individual contributor cannot receive a referral before coordinator approval. | Demonstrate pending and approved sample accounts. | BO-02, BO-03 |

Potential measures include requests awaiting review, time to review, referrals awaiting an outcome, portions reported fulfilled, and evidence awaiting review. Targets and measurement methods have not been agreed; no improvement is assumed without a baseline.

## 13. Risks and operating considerations

| ID | Risk | Response |
| --- | --- | --- |
| R-01 | Uploaded evidence may be misleading or insufficient, and approval of a charity does not prove future conduct. | Agree review criteria, limits of verification, and a process for concerns with stakeholders. |
| R-02 | Evidence may expose sensitive beneficiary information. | Agree minimum information, access, consent, and retention with partners before real use. |
| R-03 | Individual contributors or charities may not respond or complete a referral. | Track the unresolved portion and allow coordinator follow-up or another referral. |
| R-04 | Self-registration, multiple participants, evidence, and splitting may exceed the course schedule. | Validate the minimum end-to-end demonstration with the team and instructor. |

The category **organizational referral** includes referrals to individuals as well as organizations. Real-world charity verification, evidence quality, consent, retention, funding, and local requirements require operational validation before launch.

## 14. Glossary

| Term | Meaning |
| --- | --- |
| Beneficiary | The person or family receiving assistance. |
| Requester | A beneficiary, third party, or coordinator who submits a request. |
| Coordinator | An authorized platform user who reviews requests and makes referrals. |
| Referral recipient | The approved charity or individual contributor to whom a referral is sent; distinct from the beneficiary receiving assistance. |
| Request | A recorded need for goods, money, or a subtype of organizational referral. |
| Organizational referral | A request category for non-goods, non-money help, such as tutoring or transport, which may be referred to an organization or an individual. |
| Referral | A request or defined portion directed to a charity or individual contributor for assistance. Any request type can be referred. |
| Assigned portion | The part of a need assigned to one referral recipient. |
| Unresolved need | The part of a need not yet fulfilled and accepted, whether assigned or unassigned. |
| Unassigned need | The unresolved part not currently assigned to a referral recipient, including an undelivered portion explicitly released from an assignment; it is available for assignment. |
| Accepted partial outcome | A reviewed and accepted report that fulfills only part of an assigned referral; the referral is not completed and the undelivered portion remains unresolved. |
| Completed / closed unresolved | Completed means all required need was fulfilled and accepted; closed unresolved means work stopped while need remained unmet and a reason was recorded. |
| Linked requests | Separate requests created from an original request that contains different kinds of support. |
| Evidence | Material provided by the responsible charity or contributor to support a reported delivery and receipt; it is not automatically independent proof. |

## 15. Review and agreement

| Reviewer | Status | Focus |
| --- | --- | --- |
| Project owner | Validation and delegation recorded on 2026-11-04 | Validated the earlier BRD, requested finalization, and delegated remaining choices to practical judgment; individual defaults were not separately signed off. |
| Instructor | Not recorded | Course requirements apply; no instructor approval is represented here. |
| Prospective charity and users | Not recorded | No external operational validation is represented here. |

Version 1.4 is the working business baseline for subsequent requirements and project work; the Phase 1 review was accepted by the Scrum Master; diagram reconciliation remains open. External feedback and launch readiness remain separate from the project owner's validation and delegation.
