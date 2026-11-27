# Issue #156 verification

Date: 2026-11-27 (Africa/Cairo)

## Local Flutter checks

Run from `mobile/` with Flutter 3.44.3 / Dart 3.12.2:

- `flutter pub get --enforce-lockfile` — passed.
- `dart format --output=none --set-exit-if-changed lib test integration_test` — passed.
- `flutter analyze --fatal-infos` — passed with no issues.
- `flutter test --reporter expanded` — passed (69 tests, including the profile sign-out outage test).
- `flutter build apk --debug --no-pub` — passed.
- `flutter build ios --no-codesign --no-pub` — passed on macOS/Xcode 16.4.

## Native API journey

Both runs used a disposable local PostgreSQL container, a local API instance, a test-only TLS certificate, and fictional beneficiary/request data. No shared or production data was used.

- Android API 36 emulator: `flutter test integration_test/beneficiary_requests_api_test.dart --device-id emulator-5554 --dart-define-from-file=/tmp/hh156-native-verification/defines.json` — passed. The app registered and signed in a fictional beneficiary, read the beneficiary's empty request list, submitted a single-need request, then read the created request from the API.
- iOS 18.6 iPhone 16 Pro Max simulator: `flutter test integration_test/beneficiary_requests_api_test.dart --device-id B2C3E338-0521-486F-A077-F2A82A4452A1 --dart-define-from-file=/tmp/hh156-native-verification/defines.json` — passed with the same request journey.

The `/tmp/hh156-native-verification` configuration and local services were test-only. GitHub Actions results are tracked on the pull request and are not represented here as already passed.
