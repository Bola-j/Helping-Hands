# Issue #152 verification

Date: 25 November 2026

## API contract tests

Ran the beneficiary registration, separate sign-in, invalid input, and
antiforgery tests from `HelpingHands.Api.Tests` against a newly created,
disposable PostgreSQL database named `helping_hands_workflow`.

Result: **5 passed, 0 failed**. The tests confirmed a beneficiary account and
profile are created, registration does not issue an authentication cookie,
sign-in is separate, requests without antiforgery are rejected, and invalid
registration input is rejected. The disposable database container was removed
after the run. The existing `helping_hands` database was not used.

## Flutter native API check

The integration test used the real Flutter Dio registration client and the
actual API over HTTPS with fictional data. A disposable API instance, database,
and short-lived localhost certificate were used. The certificate was supplied
as an explicit test trust anchor; TLS verification was left enabled. The API
returned account/profile IDs and a `BEN-` reference, and the client retained no
identity cookie after registration.

| Platform | Result | Evidence |
| --- | --- | --- |
| Android emulator, Android 16 API 36 | **Passed** | `integration_test/beneficiary_registration_api_test.dart` completed against the disposable API. |
| iOS Simulator, iPhone 16 Pro Max / iOS 18.6 | **Passed** | The native integration completed against the disposable API and returned the account/profile IDs and `BEN-` reference without an identity cookie. The first attempts exposed a TLS handshake failure with a newly generated test certificate; after disk space was freed, the test passed using the existing localhost development certificate as an explicit trust anchor. |

The temporary API, database container, generated certificate, and Android port
forward were cleaned up after the checks. The existing `helping_hands` database
was not used.

## Coverage limit

Both simulator integrations passed; a physical iPhone run has not been performed.
This report does not close or update the GitHub issue.
