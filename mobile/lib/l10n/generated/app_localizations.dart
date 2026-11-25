import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_ar.dart';
import 'app_localizations_en.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'generated/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('ar'),
    Locale('en'),
  ];

  /// No description provided for @switchLanguage.
  ///
  /// In en, this message translates to:
  /// **'English'**
  String get switchLanguage;

  /// No description provided for @phaseBadge.
  ///
  /// In en, this message translates to:
  /// **'Phase 1 & 2 · Beneficiary'**
  String get phaseBadge;

  /// No description provided for @welcomeTitle.
  ///
  /// In en, this message translates to:
  /// **'Welcome to Helping Hands'**
  String get welcomeTitle;

  /// No description provided for @welcomeSubtitle.
  ///
  /// In en, this message translates to:
  /// **'A calm, private place to submit and follow your assistance requests,\nso approved charities can coordinate support for you.'**
  String get welcomeSubtitle;

  /// No description provided for @chooseLanguage.
  ///
  /// In en, this message translates to:
  /// **'Choose app language'**
  String get chooseLanguage;

  /// No description provided for @english.
  ///
  /// In en, this message translates to:
  /// **'English'**
  String get english;

  /// No description provided for @arabic.
  ///
  /// In en, this message translates to:
  /// **'العربية'**
  String get arabic;

  /// No description provided for @scopeNotice.
  ///
  /// In en, this message translates to:
  /// **'The app records your request and its updates only. It does not collect money or\nguarantee approval. Support is coordinated outside the app by approved charities.'**
  String get scopeNotice;

  /// No description provided for @createBeneficiaryAccount.
  ///
  /// In en, this message translates to:
  /// **'Create beneficiary account'**
  String get createBeneficiaryAccount;

  /// No description provided for @signInExistingAccount.
  ///
  /// In en, this message translates to:
  /// **'Sign in to an existing account'**
  String get signInExistingAccount;

  /// No description provided for @createAccount.
  ///
  /// In en, this message translates to:
  /// **'Create account'**
  String get createAccount;

  /// No description provided for @signIn.
  ///
  /// In en, this message translates to:
  /// **'Sign in'**
  String get signIn;

  /// No description provided for @back.
  ///
  /// In en, this message translates to:
  /// **'Back'**
  String get back;

  /// No description provided for @registrationIntro.
  ///
  /// In en, this message translates to:
  /// **'Create your beneficiary profile. You will sign in separately after registration.'**
  String get registrationIntro;

  /// No description provided for @fullName.
  ///
  /// In en, this message translates to:
  /// **'Full name'**
  String get fullName;

  /// No description provided for @email.
  ///
  /// In en, this message translates to:
  /// **'Email'**
  String get email;

  /// No description provided for @password.
  ///
  /// In en, this message translates to:
  /// **'Password'**
  String get password;

  /// No description provided for @contactDetailsOptional.
  ///
  /// In en, this message translates to:
  /// **'Contact details (optional)'**
  String get contactDetailsOptional;

  /// No description provided for @contactDetailsHint.
  ///
  /// In en, this message translates to:
  /// **'Optional contact information for your profile'**
  String get contactDetailsHint;

  /// No description provided for @profileFor.
  ///
  /// In en, this message translates to:
  /// **'Who will this beneficiary profile represent?'**
  String get profileFor;

  /// No description provided for @forMe.
  ///
  /// In en, this message translates to:
  /// **'Me'**
  String get forMe;

  /// No description provided for @forFamily.
  ///
  /// In en, this message translates to:
  /// **'My family'**
  String get forFamily;

  /// No description provided for @profileForRequired.
  ///
  /// In en, this message translates to:
  /// **'Choose who the profile represents.'**
  String get profileForRequired;

  /// No description provided for @nameRequired.
  ///
  /// In en, this message translates to:
  /// **'Enter your name.'**
  String get nameRequired;

  /// No description provided for @nameTooLong.
  ///
  /// In en, this message translates to:
  /// **'Name must be 200 characters or fewer.'**
  String get nameTooLong;

  /// No description provided for @emailRequired.
  ///
  /// In en, this message translates to:
  /// **'Enter your email address.'**
  String get emailRequired;

  /// No description provided for @emailInvalid.
  ///
  /// In en, this message translates to:
  /// **'Enter a valid email address.'**
  String get emailInvalid;

  /// No description provided for @passwordRequirements.
  ///
  /// In en, this message translates to:
  /// **'Use at least 12 characters with uppercase, lowercase, a number, and a symbol.'**
  String get passwordRequirements;

  /// No description provided for @contactDetailsTooLong.
  ///
  /// In en, this message translates to:
  /// **'Contact details must be 500 characters or fewer.'**
  String get contactDetailsTooLong;

  /// No description provided for @showPassword.
  ///
  /// In en, this message translates to:
  /// **'Show password'**
  String get showPassword;

  /// No description provided for @hidePassword.
  ///
  /// In en, this message translates to:
  /// **'Hide password'**
  String get hidePassword;

  /// No description provided for @alreadyHaveAccount.
  ///
  /// In en, this message translates to:
  /// **'Already have an account? Sign in'**
  String get alreadyHaveAccount;

  /// No description provided for @continueToSignIn.
  ///
  /// In en, this message translates to:
  /// **'Continue to sign in'**
  String get continueToSignIn;

  /// No description provided for @registrationSuccess.
  ///
  /// In en, this message translates to:
  /// **'Your account has been created'**
  String get registrationSuccess;

  /// No description provided for @registrationNextStep.
  ///
  /// In en, this message translates to:
  /// **'Your account is ready. Continue to sign in with your email and password.'**
  String get registrationNextStep;

  /// No description provided for @beneficiaryReference.
  ///
  /// In en, this message translates to:
  /// **'Beneficiary reference: {reference}'**
  String beneficiaryReference(Object reference);

  /// No description provided for @registrationDuplicateEmail.
  ///
  /// In en, this message translates to:
  /// **'This email is already registered. Try signing in instead.'**
  String get registrationDuplicateEmail;

  /// No description provided for @registrationInvalidRequest.
  ///
  /// In en, this message translates to:
  /// **'We could not accept these details. Check the form and try again.'**
  String get registrationInvalidRequest;

  /// No description provided for @registrationApiUnavailable.
  ///
  /// In en, this message translates to:
  /// **'Registration is temporarily unavailable. Check your connection and try again.'**
  String get registrationApiUnavailable;

  /// No description provided for @registrationUnexpected.
  ///
  /// In en, this message translates to:
  /// **'We could not complete registration. Please try again.'**
  String get registrationUnexpected;

  /// No description provided for @comingSoon.
  ///
  /// In en, this message translates to:
  /// **'This screen will be implemented in a later issue.'**
  String get comingSoon;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['ar', 'en'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'ar':
      return AppLocalizationsAr();
    case 'en':
      return AppLocalizationsEn();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
