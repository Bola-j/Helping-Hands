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
| Live API requests on Android and iOS | **Not run** | No running API was available, so no native cookie/TLS/API behavior is claimed. |
| Eight-hour expiry on a running API | **Not run** | The API has an eight-hour test-clock integration test in its source, but no live API/test database was available for this spike. |

The adapter tests use a local simulated transport; they are mobile unit tests, not evidence that the remote server or native platform accepted the contract.

## Current blocker and next dependency

In this workspace, `.env` and `.local/https/dev.crt` are absent, Docker is not installed, and `https://localhost:5087/health` and `https://localhost:8443/health` both refused connections. Thus the API could not be started or reached for native verification. No contract incompatibility has been observed, and no backend code change is currently indicated. Before dependent sign-in, home, or request flows are treated as integrated, provide an isolated fictional-data API environment reachable over HTTPS from both simulator targets, its platform-trusted development certificate, and a beneficiary test account. Then repeat the live sequence on Android and iOS, including role denial, a real expiry/rejection case, and API outage.

The repository API test `tests/HelpingHands.Api.Tests/SessionCookieTests.Expiry.cs` advances a test `TimeProvider` and verifies the eight-hour boundary. It is evidence of intended server behavior only; it was not executed in this mobile spike.
