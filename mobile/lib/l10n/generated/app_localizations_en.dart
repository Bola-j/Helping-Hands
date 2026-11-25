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

  @override
  String get invalidEmail => 'Enter a valid email address.';

  @override
  String get signInInvalidCredentials =>
      'Email or password is incorrect. Check your details and try again.';

  @override
  String get signInExpired =>
      'Your session has expired. Sign in again to continue.';

  @override
  String get signInDenied => 'This account cannot access the beneficiary app.';

  @override
  String get signInUnavailable =>
      'We could not reach Helping Hands. Check your connection and try again.';

  @override
  String get signInInvalidRequest =>
      'We could not process this sign-in. Please try again.';

  @override
  String get signInUnexpected => 'We could not sign you in. Please try again.';

  @override
  String get signOut => 'Sign out';

  @override
  String get signOutFailed =>
      'We could not confirm sign out with the service. Please try again.';

  @override
  String get signedOut => 'You are signed out.';

  @override
  String get backToSignIn => 'Go to sign in';

  @override
  String get passwordRequired => 'Enter your password.';

  @override
  String get signInTitle => 'Sign in';

  @override
  String get signInSubtitle =>
      'Enter the email and password you used to create your account.';

  @override
  String get phaseSignIn => 'Phase 1 · Sign in';

  @override
  String get phaseSessionExpired => 'Session expired · Phases 1 & 2';

  @override
  String get noBeneficiaryAccount => 'Don\'t have a beneficiary account?';

  @override
  String get sessionExpiredTitle =>
      'You’ve been signed out to protect your account';

  @override
  String get sessionExpiredBody =>
      'Your session ended due to inactivity or you signed out. Your submitted requests remain saved. Sign in again to continue.';

  @override
  String get sessionExpiredPrivacy =>
      'Your personal information and request details stay hidden until you sign in again.';

  @override
  String get signInAgain => 'Sign in again';

  @override
  String get returnWelcome => 'Return to welcome and language';

  @override
  String get profileTitle => 'Account & settings';

  @override
  String get profileSubtitle => 'Phase 1 & 2 · Language and sign out';

  @override
  String get englishLtr => 'English (LTR)';

  @override
  String get arabicRtl => 'العربية (RTL)';

  @override
  String get homeNavigation => 'Home';

  @override
  String get requestsNavigation => 'My requests';

  @override
  String get accountNavigation => 'Account';
}
