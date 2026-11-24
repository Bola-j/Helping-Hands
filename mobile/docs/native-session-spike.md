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
| Live API integration test on iPhone 16 Simulator | **Passed** | Against the isolated local .NET API and PostgreSQL database: antiforgery token and cookie retrieval, beneficiary registration without login, separate sign-in, retained authentication cookie, authenticated `/me` read, 403 for a beneficiary-only role at a staff endpoint, logout and cookie clearing, unauthenticated read, invalid credentials, and API-unavailable classification. |
| Live API integration test on Android emulator | **Not completed** | The emulator booted, but two `assembleDebug` attempts remained blocked while Gradle downloaded artifacts (about seven minutes, then about two and a half minutes). Both were stopped; the native Android test did not start and no Android API result is claimed. |
| API eight-hour expiry test | **Passed, 1 test** | `Session_ExpiresAfterEightHoursWithoutSlidingRenewal` advanced the API test clock against an isolated PostgreSQL test database and observed the session expire with 401. This verifies server expiry behavior; it does not exercise the native client after that expiry. |

The adapter tests use a local simulated transport; they are unit tests, not evidence that the remote server or native platform accepted the contract. The native integration test is `integration_test/native_session_api_test.dart` and requires a reachable HTTPS API, a beneficiary test account flow, and a platform-trusted development certificate. It creates a fictional beneficiary in the isolated API database.

## Remaining verification

The live flow currently has iOS simulator evidence only. Run the integration test on Android after Gradle dependencies resolve and configure the emulator to trust the local API certificate without disabling TLS verification. Repeat the native flow on Android and record its result. The API's test-clock expiry is verified, but the native client has not yet been exercised against the expired cookie response.

The repository API test `tests/HelpingHands.Api.Tests/SessionCookieTests.Expiry.cs` advances a test `TimeProvider` and verifies the eight-hour boundary. It is evidence of intended server behavior only; it was not executed in this mobile spike.
