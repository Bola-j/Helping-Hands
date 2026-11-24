// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get switchLanguage => 'English';

  @override
  String get phaseBadge => 'Phase 1 & 2 · Beneficiary';

  @override
  String get welcomeTitle => 'Welcome to Helping Hands';

  @override
  String get welcomeSubtitle =>
      'A calm, private place to submit and follow your assistance requests,\nso approved charities can coordinate support for you.';

  @override
  String get chooseLanguage => 'Choose app language';

  @override
  String get english => 'English';

  @override
  String get arabic => 'العربية';

  @override
  String get scopeNotice =>
      'The app records your request and its updates only. It does not collect money or\nguarantee approval. Support is coordinated outside the app by approved charities.';

  @override
  String get createBeneficiaryAccount => 'Create beneficiary account';

  @override
  String get signInExistingAccount => 'Sign in to an existing account';

  @override
  String get createAccount => 'Create account';

  @override
  String get signIn => 'Sign in';

  @override
  String get comingSoon => 'This screen will be implemented in a later issue.';
}
