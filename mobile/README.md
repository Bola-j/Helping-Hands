# Helping Hands mobile app

Beneficiary-only Flutter app for Android and iOS. The app communicates with the existing API through Dio; it must not connect directly to PostgreSQL.

## Run locally

From this directory:

```sh
flutter pub get
flutter run
```

Configure the API base URL for the target environment:

```sh
flutter run --dart-define=API_BASE_URL=https://your-api-host
```

The session data layer follows the API's cookie and antiforgery contract. It uses an in-memory cookie jar shared by Dio requests; cookies are cleared by the API's logout response and are not persisted across app restarts. Passwords are passed to the sign-in request only and are not stored. The API base URL defaults to `https://localhost:5087` for local host-run API development and can be overridden with `--dart-define`. For an Android emulator reaching a host-run API, set up `adb reverse tcp:5087 tcp:5087`; iOS Simulator can use the host's `localhost`. Native TLS verification remains enabled; trust the local development certificate on the target platform or use a correctly certified HTTPS API host. Do not add certificate bypasses.

The session repository supports beneficiary registration, sign-in, current-user lookup, and sign-out. Registration and sign-in are separate. The repository distinguishes invalid sign-in credentials (401 on login), an expired session (401 after this process has authenticated), no current session (401 before authentication), denied (403), invalid request (400), and API unavailable (connection failures and 5xx). The session can only be retained during the running process; it is not restored across app restarts.

See [`docs/native-session-spike.md`](docs/native-session-spike.md) for implemented coverage and live API limitations before starting dependent auth or request screens.

## Beneficiary account creation

The Create Account screen submits the backend contract fields to
`POST /api/v1/auth/register/beneficiary`: `displayName`, `email`, `password`,
optional `contactDetails`, and `isFamily`. It retrieves and sends the API's
antiforgery token and cookie through Dio. Registration is separate from sign-in
and displays the account's beneficiary reference after a successful response.
The app never connects directly to PostgreSQL.

The native registration integration test is
`integration_test/beneficiary_registration_api_test.dart`. It writes a fictional
beneficiary to the configured API, so run it only against an isolated disposable
database and API with a test certificate trusted explicitly by the test client.
TLS verification remains enabled.

## Verify

```sh
flutter analyze
flutter test
```

## Structure

- `lib/app/`: app setup, route table, and app-wide dependency providers.
- `lib/core/`: shared theme, Dio configuration, and button, text-field, and card components. Buttons expose loading/disabled states, fields expose error/disabled states, and cards expose selection.
- `lib/features/welcome/`: welcome feature; `application/` contains its locale Cubit and `presentation/` its UI.
- `lib/features/auth/`: beneficiary registration DTOs, API data source, repository, BLoC and Create Account screen, alongside the session contract and API repository.
- `lib/l10n/`: English and Arabic strings; Arabic uses RTL through Flutter localization.
- `assets/icons/` and `assets/fonts/`: Figma-exported SVG assets and the Figma typography families, with their licenses.

The Welcome & Language, beneficiary Create Account, Sign In, Session Expired, and Profile/Sign Out flows are implemented. Sign-in uses the API cookie session and verifies the returned current user has the Beneficiary role before opening the profile. The API must be reachable over HTTPS from the selected Android/iOS device, and its certificate must be trusted by that device. Never disable TLS verification or put credentials in source.
