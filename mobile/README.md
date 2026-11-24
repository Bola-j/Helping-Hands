# Helping Hands mobile app

Beneficiary-only Flutter app for Android and iOS. The app communicates with the existing API through Dio; it must not connect directly to PostgreSQL.

## Run locally

From this directory:

```sh
flutter pub get
flutter run
```

To configure the API base URL when a backend contract is ready:

```sh
flutter run --dart-define=API_BASE_URL=https://your-api-host
```

The current foundation deliberately makes no API requests. Authentication and request workflows belong to later issues.

## Verify

```sh
flutter analyze
flutter test
```

## Structure

- `lib/app/`: app setup, route table, and app-wide dependency providers.
- `lib/core/`: shared theme, Dio configuration, and button, text-field, and card components. Buttons expose loading/disabled states, fields expose error/disabled states, and cards expose selection.
- `lib/features/welcome/`: welcome feature; `application/` contains its locale Cubit and `presentation/` its UI.
- `lib/l10n/`: English and Arabic strings; Arabic uses RTL through Flutter localization.
- `assets/icons/` and `assets/fonts/`: Figma-exported SVG assets and the Figma typography families, with their licenses.

The Welcome & Language screen is the first Phase 1 screen. Account and sign-in routes currently show foundation placeholders; no auth behavior is implemented here.
