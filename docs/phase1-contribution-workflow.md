# Phase 1 contribution workflow

This workflow supports [P1-04](https://github.com/Bola-j/Helping-Hands/issues/4) and the course requirement for visible individual code contributions. The Scrum Master coordinates the board and accepts reviews; that review does not replace a teammate's own work or commit history.

## From issue to completed work

1. Select a suitably sized issue from the [Phase 1 project](https://github.com/users/Bola-j/projects/4), confirm its dependencies and acceptance criteria, and record the intended owner in the issue. Do not assign or invite an account until its owner confirms the username.
2. Start from an updated `main` in a clean checkout or an isolated worktree. Create a descriptive branch for one feature or independently reviewable task, for example `phase1/session-contract`. Preserve other local changes.
3. Implement the task and run focused checks. Each contributor uses their own GitHub account and Git author identity; configure the email associated with that account locally. Never fabricate a contribution or use another teammate's identity.
4. Open a pull request describing the change and actual verification. Link the issue using `Closes #<number>` only when the PR is intended to complete it; otherwise use `Related to #<number>`. Keep credentials, real participant data and generated local files out of commits.
5. Have the Scrum Master or a delegated reviewer check the requirements, tests, failures and limitations. Merge the finished feature into `main` after the relevant checks pass. Keep the issue and board status aligned with the evidence; a merged partial PR does not by itself complete the issue.
6. At sprint review, record the issue, branch, PR, commit author, validation and outcome. Capture decisions and follow-up work in repository meeting notes. Phase 2 requires each student to implement at least two features, each on a dedicated branch; track those separately from a single Phase 1 contribution.

## Candidate ownership and dependencies

The following are proposed starting points, not assignments or evidence of completed contributions. Confirm them with each teammate before work begins. Later web tickets are listed only to show potential team ownership; this workflow does not implement them.

| Team member | Independently reviewable candidate | Dependency or handoff |
| --- | --- | --- |
| Bola Ghaly | [P1-08: API session and antiforgery contracts](https://github.com/Bola-j/Helping-Hands/issues/8) | Depends on P1-06 and P1-10; reconcile existing behavior and run focused HTTP access checks. |
| Ziad Walid | [P1-12: service catalog and versioned standards schema](https://github.com/Bola-j/Helping-Hands/issues/12) | Depends on P1-02 and P1-05; use a reviewed migration and isolated PostgreSQL checks. |
| hussien okely | [P1-11: registration and login screens](https://github.com/Bola-j/Helping-Hands/issues/11) | Future UI work; depends on the web foundation, session client and API identity tickets. |
| ali fathy | [P1-15: coordinator service and standards screens](https://github.com/Bola-j/Helping-Hands/issues/15) | Future UI work; depends on the web foundation, session client and service API. |

As of this review, the repository collaborator list includes Bola and Ziad. The GitHub usernames and access for hussien and ali remain unconfirmed. The repository history reviewed so far does not establish individual code commits by those three teammates. These are gaps to resolve, not reasons to manufacture activity. Ziad's access is accepted as previously recorded.

## P1-04 completion evidence

Keep P1-04 open until each teammate has an independently reviewable code task, contributes under their own account and links the associated feature branch and PR to an issue. Link the actual PRs and checks in P1-04 and record any access or contribution still missing. Future implementation tasks retain their own acceptance criteria and are not completed by this document.
