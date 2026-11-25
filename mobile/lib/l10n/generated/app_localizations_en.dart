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
  String get back => 'Back';

  @override
  String get registrationIntro =>
      'Create your beneficiary profile. You will sign in separately after registration.';

  @override
  String get fullName => 'Full name';

  @override
  String get email => 'Email';

  @override
  String get password => 'Password';

  @override
  String get contactDetailsOptional => 'Contact details (optional)';

  @override
  String get contactDetailsHint =>
      'Optional contact information for your profile';

  @override
  String get profileFor => 'Who will this beneficiary profile represent?';

  @override
  String get forMe => 'Me';

  @override
  String get forFamily => 'My family';

  @override
  String get profileForRequired => 'Choose who the profile represents.';

  @override
  String get nameRequired => 'Enter your name.';

  @override
  String get nameTooLong => 'Name must be 200 characters or fewer.';

  @override
  String get emailRequired => 'Enter your email address.';

  @override
  String get emailInvalid => 'Enter a valid email address.';

  @override
  String get passwordRequirements =>
      'Use at least 12 characters with uppercase, lowercase, a number, and a symbol.';

  @override
  String get contactDetailsTooLong =>
      'Contact details must be 500 characters or fewer.';

  @override
  String get showPassword => 'Show password';

  @override
  String get hidePassword => 'Hide password';

  @override
  String get alreadyHaveAccount => 'Already have an account? Sign in';

  @override
  String get continueToSignIn => 'Continue to sign in';

  @override
  String get registrationSuccess => 'Your account has been created';

  @override
  String get registrationNextStep =>
      'Your account is ready. Continue to sign in with your email and password.';

  @override
  String beneficiaryReference(Object reference) {
    return 'Beneficiary reference: $reference';
  }

  @override
  String get registrationDuplicateEmail =>
      'This email is already registered. Try signing in instead.';

  @override
  String get registrationInvalidRequest =>
      'We could not accept these details. Check the form and try again.';

  @override
  String get registrationApiUnavailable =>
      'Registration is temporarily unavailable. Check your connection and try again.';

  @override
  String get registrationUnexpected =>
      'We could not complete registration. Please try again.';

  @override
  String get comingSoon => 'This screen will be implemented in a later issue.';
}
