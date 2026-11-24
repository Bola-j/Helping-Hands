# Issue #139: native session integration spike

## Result

The mobile client now has a Dio session data source and Clean Architecture repository boundary for the existing beneficiary authentication contract. The code retains antiforgery and authentication cookies in a shared in-memory cookie jar, sends `X-CSRF-TOKEN` with protected auth writes, keeps beneficiary registration separate from sign-in, and does not persist passwords or session cookies across app restarts. TLS verification is unchanged; no certificate bypass was added.

## Contract traced in the API source

- `GET /api/v1/auth/antiforgery` is anonymous and returns `requestToken` and `headerName`; the server also issues a strict, secure, HttpOnly antiforgery cookie.
- Registration and login are separate unsafe requests and require the CSRF header and cookie. Registration does not sign the user in.
- `GET /api/v1/auth/me` requires authentication. `POST /api/v1/auth/logout` requires authentication and antiforgery validation.
- The authentication cookie is secure, HttpOnly, SameSite Strict, non-sliding and expires after eight hours. Auth challenges use 401; role/authorization denial uses 403; invalid antiforgery returns 400.
- API source references: `src/HelpingHands.Api/Auth/AuthSetup.RoutesAndRegistration.cs`, `AuthSetup.Options.cs`, `AuthSetup.Session.cs`, `AntiforgeryFilter.cs`, and `src/HelpingHands.Api/Features/Catalog/CatalogEndpoints.cs`.

## Verification evidence

| Check | Result | What this establishes |
|---|---|---|
| `flutter analyze` | Passed | Mobile source is statically clean. |
| `flutter test` | Passed, 12 tests | Contract-shaped adapter tests cover CSRF token/cookie pairing, cookie retention on `/auth/me`, protected logout and cookie clearing, registration without login, invalid credentials, 401 after an authenticated session, 403, invalid antiforgery, and connection failure classification. Existing foundation widget tests also pass. |
| `flutter build apk --debug` | Passed | Android debug target compiles. |
| `flutter build ios --simulator --debug` | Passed | iOS Simulator target compiles. |
| Live API integration test on iPhone 16 Simulator | **Passed** | Against the isolated local .NET API and PostgreSQL database: antiforgery token and cookie retrieval, beneficiary registration without login, separate sign-in, retained authentication cookie, authenticated `/me` read, 403 for a beneficiary-only role at a staff endpoint, logout and cookie clearing, unauthenticated read, invalid credentials, missing authenticated cookie classified as an expired session (401), and API-unavailable classification. |
| Live API integration test on Android emulator | **Passed** | The native Android flow passed against the isolated API and PostgreSQL database: antiforgery cookie/token, beneficiary registration, separate sign-in, authenticated `/me`, denied staff endpoint, logout, unauthenticated and invalid-credential outcomes, expired-session classification after the authenticated cookie was removed, and API-unavailable classification. The local HTTPS certificate was supplied as an explicit trust anchor to the test client; certificate verification remained enabled. |
| API eight-hour expiry test | **Passed, 1 test** | `Session_ExpiresAfterEightHoursWithoutSlidingRenewal` advanced the API test clock against an isolated PostgreSQL test database and observed the session expire with 401. The iOS client flow separately verified that an authenticated client receiving 401 after its auth cookie is removed classifies the result as `sessionExpired`; this does not wait for the eight-hour expiry on-device. |

The adapter tests use a local simulated transport; they are unit tests, not evidence that the remote server or native platform accepted the contract. The native integration test is `integration_test/native_session_api_test.dart` and requires a reachable HTTPS API, a beneficiary test account flow, and a platform-trusted development certificate. It creates a fictional beneficiary in the isolated API database.

## Remaining verification

The live session flow now has passing iOS Simulator and Android emulator evidence. For a self-signed local API certificate, the integration test accepts an optional `API_CERTIFICATE_BASE64` define and adds that certificate to a `SecurityContext` with system roots retained. This is test-only trust configuration; TLS verification remains enabled, and production builds continue using the platform trust store. The API test-clock expiry and client classification of a 401 after the authenticated cookie is removed were verified separately.

The repository API test `tests/HelpingHands.Api.Tests/SessionCookieTests.Expiry.cs` advances a test `TimeProvider` and verifies the eight-hour boundary against an isolated PostgreSQL test database. The native client flow exercises the corresponding 401 classification by deleting the local auth cookie after successful sign-in; it does not wait eight hours on-device.
