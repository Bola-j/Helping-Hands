# Phase 1 beneficiary mobile demo guide

This guide prepares a repeatable, fictional-data demonstration of the Flutter beneficiary app. It covers the committed Phase 1 mobile journey only: account creation, separate sign-in, one single-need request, and the signed-in beneficiary's own request list. The app uses the shared API; it must never connect directly to PostgreSQL or evidence storage. Assistance is arranged outside the platform. A submitted request or status is not proof that assistance was delivered.

## 1. What this guide does and does not claim

The app's normal Dio client uses the target platform's HTTPS trust store and has no certificate bypass. The native integration test can add a test certificate as an explicit trust anchor; that test-only setup does not configure the regular app client. Each simulator/device must trust the certificate for the API hostname, or the demo API must use an HTTPS certificate already trusted by that platform. Do not disable TLS validation.

As of 27 November 2026, `mobile/docs/issue-156-verification.md` records a passing native API request journey on an Android API 36 emulator and an iOS 18.6 iPhone 16 Pro Max simulator. Those were integration-test runs through the native HTTP client, not a recorded, manual walkthrough of every visible UI screen. Record a separate manual demo result for each platform when rehearsed; do not describe a build or integration test as a manual UI demonstration.

## 2. Prerequisites

- Flutter 3.44.3 / Dart 3.12.2 (the versions used by the mobile CI workflow).
- Android Studio, Android SDK, and an Android emulator or device for the Android demonstration.
- macOS with Xcode and an iOS Simulator or device for the iOS demonstration.
- .NET 10 SDK, a compatible `dotnet-ef` tool, Docker with its engine running, and PowerShell (`pwsh`) for the repository's API migration launcher.
- A trusted HTTPS certificate for the API hostname on each target. The local API certificate trusted on the host is not automatically trusted by a device. Android may require the certificate in the emulator's system trust store; a user-installed CA alone may not be trusted by an app. On iOS, add/trust the certificate in the simulator/device trust settings. If that cannot be done, use a development API host with a platform-trusted certificate. Keep the hostname in `API_BASE_URL` covered by the certificate.

The repository root [README](../../README.md) is the source for Docker and API setup details. The commands below use a separate Compose project and environment file so this rehearsal gets its own database and storage volumes. They do not overwrite the repository's `.env` or reuse the default Compose volumes.

## 3. Prepare an isolated local API

Run these commands from the repository root. On PowerShell, use `Copy-Item` instead of `cp` where needed.

```sh
cp .env.example .env.mobile-demo
```

Edit `.env.mobile-demo` locally. It is ignored by Git. Replace all development placeholders with unique fictional values. Set the local database password, matching host connection string, S3-compatible access/secret keys (ASCII letters and digits only), SMTP catcher credentials, and development certificate password. Use these isolated host ports and certificate directory, adjusting them only if occupied:

```dotenv
DEV_HTTPS_DIR=./.local/mobile-demo-https
API_HOST_PORT=18443
POSTGRES_HOST_PORT=65432
S3_API_HOST_PORT=19000
SMTP_HOST_PORT=11025
SMTP_UI_HOST_PORT=18025
ConnectionStrings__HelpingHands=Host=localhost;Port=65432;Database=helping_hands;Username=helping_hands;Password=<your-local-db-password>
EvidenceStorage__ServiceUrl=http://localhost:19000
```

Keep `POSTGRES_USER`, `POSTGRES_PASSWORD`, `S3_ACCESS_KEY`, `S3_SECRET_KEY`, `EvidenceStorage__AccessKey`, `EvidenceStorage__SecretKey`, `SMTP_USER`, `SMTP_PASSWORD`, and `DEV_CERT_PASSWORD` consistent with the values referenced in `.env.mobile-demo`. Do not paste passwords, tokens, cookies, real beneficiary details, or this environment file into screenshots, terminal recordings, issues, or commits.

Create the HTTPS certificate expected by the Compose API, using the same local-only password as `DEV_CERT_PASSWORD`:

```sh
mkdir -p .local/mobile-demo-https
# Substitute the local DEV_CERT_PASSWORD as described in the root README; do not commit the resulting files.
dotnet dev-certs https -ep .local/mobile-demo-https/dev.pfx -p '<DEV_CERT_PASSWORD>'
dotnet dev-certs https -ep .local/mobile-demo-https/dev.crt --format PEM --no-password
dotnet dev-certs https --trust
```

Start the isolated dependencies, apply migrations to this stack, and start the API. No seed account is required; create the beneficiary in the app during the demo.

```sh
docker compose --env-file .env.mobile-demo -p hh-mobile-demo up -d --wait postgres
docker compose --env-file .env.mobile-demo -p hh-mobile-demo up -d storage storage-init
# Wait for storage-init to exit successfully.
docker compose --env-file .env.mobile-demo -p hh-mobile-demo ps -a
pwsh ./scripts/run-api.ps1 -EnvironmentFile .env.mobile-demo -Migrate
docker compose --env-file .env.mobile-demo -p hh-mobile-demo up -d --build api
```

Confirm the API reports healthy before opening the app:

```sh
curl https://localhost:18443/health
curl https://localhost:18443/ready
```

Expected results are HTTP 200 with `status: ok` and `status: ready`. If `curl` reports a certificate/hostname error, resolve trust first; do not add `--insecure` or another verification bypass. `/ready` confirms the API can connect to PostgreSQL; it is an infrastructure readiness check, not an application test.

To stop this demo stack after the session, stop only its Compose project and preserve the isolated data unless the team explicitly agrees to reset it:

```sh
docker compose --env-file .env.mobile-demo -p hh-mobile-demo down
```

Do not add `--volumes`/`-v`; that deletes the demo database and storage data.

## 4. Run the app on each platform

First confirm that the API certificate is trusted by the simulator/device and its name matches `localhost` in the URL. For Android, forward the mapped host port to the emulator and use the same localhost URL:

```sh
flutter emulators
flutter emulators --launch <android-emulator-id>
adb devices
adb -s <emulator-serial> reverse tcp:18443 tcp:18443
cd mobile
flutter pub get --enforce-lockfile
flutter devices
flutter run -d <android-device-id> --dart-define=API_BASE_URL=https://localhost:18443
```

For iOS, boot an iOS Simulator on macOS, trust the API certificate in that simulator, and run:

```sh
xcrun simctl list devices available
open -a Simulator
cd mobile
flutter pub get --enforce-lockfile
flutter devices
flutter run -d <ios-device-id> --dart-define=API_BASE_URL=https://localhost:18443
```

The integration-test `API_CERTIFICATE_BASE64` define is not a replacement for simulator trust when launching the app normally. Do not use a direct database connection, a production/shared API, or a TLS override. Use separate unique fictional email addresses for Android and iOS because the API enforces unique accounts.

## 5. Fictional demo data and happy-path walkthrough

Use a new `example.test` email per platform, for example `beneficiary-demo-android-<short-random-suffix>@example.test` and `beneficiary-demo-ios-<short-random-suffix>@example.test`. Use a fictional display name and contact note, choose the family option truthfully for the fictional scenario, and create a unique local password that meets the API's ASP.NET Identity policy. Do not reuse a personal or shared password. Keep the credentials in a private local note only if needed, and remove that note after the rehearsal.

For each platform, show this sequence:

1. **Welcome & Language:** launch the app, select English or Arabic, and show the direction/layout switch if useful.
2. **Create Account:** enter the fictional display name, email, password, optional contact details, and family selection. Submit once and show the beneficiary reference from the successful API response. Registration does not sign the user in; do not describe the confirmation as an email-verification step.
3. **Sign In:** sign in separately with the same fictional email/password. The app retains the API cookie in memory during the running session; it does not persist the password or restore the session after an app restart.
4. **Home / My Requests:** open the beneficiary's request list before creating a request and show the successful empty state. An API/session error is not an empty list.
5. **Single-Need Request:** open the single-need flow, enter a short fictional need description, proceed to confirmations, and check both authority and information-sharing confirmations. Submit once.
6. **Submission result:** wait for the API response and show its returned request reference and status. A success view is shown only after the server confirms creation. The status means the request was recorded; it does not mean aid has been delivered.
7. **My Requests:** return to or refresh the list and show that same request under the signed-in beneficiary. Do not show another account's records.
8. **Profile / Sign Out (optional):** show the selected language and sign out through the app. Do not show cookies or antiforgery tokens.

The app's API interactions for this flow are the existing routes: `GET /api/v1/auth/antiforgery`, `POST /api/v1/auth/register/beneficiary`, `POST /api/v1/auth/login`, authenticated `GET /api/v1/auth/me`, authenticated `GET /api/v1/beneficiaries` when the beneficiary profile ID must be resolved, `POST /api/v1/requests/single-need`, and authenticated `GET /api/v1/requests`. The single-need write requires `beneficiaryId`, `description`, `authorityConfirmed`, and `sharingAgreed`; both confirmations must be true. Cookie/antiforgery handling is performed by the app. Do not manually copy an ID from PostgreSQL or invent a client-side result.

## 6. Failure states and safe evidence

Use validation and state tests as evidence where a live UI failure cannot be produced safely and repeatably. Do not force unauthorized access, edit shared data, or claim an error state was manually demonstrated when only a test covers it.

- **Validation:** submit an empty Create Account or request description; show the field-level error, then correct it.
- **Loading:** show the visible loading indicator during an ordinary API operation if timing permits; do not add artificial delays to the shared API.
- **Empty list:** use a newly registered fictional account before its first request.
- **Session expired, denied, unavailable, and uncertain write:** use relevant Flutter widget/BLoC/API tests for these cases. The current beneficiary UI does not provide a safe control to manufacture an expired cookie or privileged role. If an isolated local stack is used for an outage demonstration, clearly identify the API as intentionally unavailable and show recovery; never interrupt a shared service. After a timeout during a non-idempotent request, do not resubmit until the outcome is checked in the beneficiary's list.

The checks currently recorded for the mobile baseline are in [`issue-156-verification.md`](issue-156-verification.md). They include formatting, analysis, widget/unit tests, Android and iOS builds, plus native API request journeys on Android API 36 and iOS 18.6. CI evidence is available from [PR #195](https://github.com/Bola-j/Helping-Hands/pull/195) and the [Flutter checks workflow](https://github.com/Bola-j/Helping-Hands/actions/workflows/flutter-ci.yml). These checks are not a substitute for a manual UI rehearsal on both target platforms.

## 7. Course and contribution evidence to present

Open the live [Phase 1 GitHub Project](https://github.com/users/Bola-j/projects/4) and show the actual issue cards, assignees, statuses, priority, dates/estimates if populated, and milestones/backlog. Show [issue #157](https://github.com/Bola-j/Helping-Hands/issues/157) and its linked mobile work. Point out missing or stale board fields as gaps instead of inventing values.

Show actual feature branches, commits, and pull requests that contain each student's implementation work. Link each example to its issue and review. This demo guide is documentation work; do not present it as a mobile code contribution. Show [PR #195](https://github.com/Bola-j/Helping-Hands/pull/195) and its successful Flutter Actions checks for formatting, analysis, tests, Android build, and macOS iOS build. Open the run/job summary rather than exposing build secrets or environment values.

For the live app, capture screenshots or a short screen recording on **each** platform showing the device/model or simulator, OS version, run date, the beneficiary flow, the returned request reference/status, and the own-request list. Use only fictional accounts. Crop or redact email/password fields, notification banners, logs, tokens, cookies, and unrelated personal information. Keep evidence in the team's approved presentation/evidence location; this guide does not add screenshots or claim that a manual rehearsal has already happened.

## 8. Rehearsal record — 27 November 2026

This record is a partial manual UI attempt, not a completed end-to-end demo. `Not run` means that step was not observed in the visible app during this rehearsal. The screenshots below show only the screens reached; they do not prove registration or request submission.

| Platform/device and OS | Date | Account/request references (fictional) | Registration + sign-in UI | Submit + own-list UI | Issues/limitations | Evidence |
|---|---|---|---|---|---|---|
| Android API 36, `HelpingHands_API36` emulator | 2026-11-27 | None; no account was created | Partial: app launched, language switched to English, Create Account opened, required-field validation displayed. Registration/sign-in not completed. | Not run | Android System UI became unresponsive during the attempt. After restarting, the emulator could not boot again because the host ran short of disk space. | [Welcome](evidence/issue-157-android-welcome-en.png), [Create Account validation](evidence/issue-157-android-registration-validation.png) |
| iPhone 16 Pro Max simulator, iOS 18.6 | 2026-11-27 | None; no account was created | App built and launched to Welcome in Arabic. No form interaction. | Not run | macOS continued to deny simulator UI automation: `osascript is not allowed assistive access`. System Settings was opened to Privacy & Security → Accessibility, but access had not taken effect during this run. | [Welcome](evidence/issue-157-ios-welcome-ar.png) |

### Review note — actual results and follow-up

- **What worked:** Flutter dependencies resolved with `flutter pub get --enforce-lockfile`; the Android debug app built and opened; the Android Welcome language switch and Create Account required-field validation were exercised; the iOS app built and opened to Welcome. The isolated local API returned HTTP 200 from `/health` and `/ready`. PR #195's already-recorded Flutter CI checks passed. The prior native API integration runs remain documented in `issue-156-verification.md` and are integration evidence, not this manual UI result.
- **What was not demonstrated:** Neither platform completed account registration, separate sign-in, single-need submission, or the beneficiary's own request list in the visible UI. No account or request references were generated during this attempt.
- **Project / issue / CI evidence:** Issue #157 is open. PR #195 and its successful Flutter workflow can be shown. Project #4 was opened in Codex, but its fields could not be verified: the available GitHub credential lacks the `read:project` scope. No owner/status/priority/estimate values are asserted here.
- **Limitations:** The Android emulator became unstable and its restart reported insufficient disk space. The iOS simulator is running the app, but macOS still denies the shell's assistive access needed to tap through it. The live Project fields remain unverified.
- **Follow-up:** Free enough disk space to restart the Android emulator; grant Accessibility to the app controlling this terminal (Terminal/Codex, as applicable), then repeat the complete visible flow on both simulators and capture the server-returned references/list. Grant the GitHub connection Project read access or verify the live fields in the browser and record their actual values. Keep #157 open until the required cross-platform UI and board evidence is reviewed.

The screenshots are partial evidence only. Full Phase 1 manual acceptance is not met by this rehearsal.
