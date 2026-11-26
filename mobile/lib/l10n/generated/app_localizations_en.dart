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

  @override
  String get beneficiaryGreeting => 'Hello';

  @override
  String get homeNewRequestLabel => 'New request';

  @override
  String get homeQuestion => 'Do you need to submit a request today?';

  @override
  String get homeQuestionBody =>
      'Choose the type of support that best matches your current need. You can review the details before submitting.';

  @override
  String get singleNeedRequest => 'Request one need (Phase 1)';

  @override
  String get multipleNeedsRequest => 'Request multiple needs (Phase 2)';

  @override
  String get requestsTitle => 'My requests';

  @override
  String get requestsSubtitle => 'Your submitted requests only';

  @override
  String get viewAllRequests => 'View all';

  @override
  String requestMetadata(Object count, Object date, Object phase) {
    return 'Phase $phase · $count need(s) · $date';
  }

  @override
  String get requestStatusSubmitted => 'Submitted';

  @override
  String get requestStatusOpen => 'Open';

  @override
  String get requestStatusInReview => 'Under review';

  @override
  String get requestStatusClarification => 'Clarification requested';

  @override
  String get requestStatusAccepted => 'Accepted';

  @override
  String get requestStatusRejected => 'Rejected';

  @override
  String get requestStatusFullyAccepted => 'All needs accepted';

  @override
  String get requestStatusPartiallyAccepted => 'Partially accepted';

  @override
  String get requestStatusFullyRejected => 'All needs rejected';

  @override
  String get requestStatusCompleted => 'Closed';

  @override
  String requestStatusUnknown(Object status) {
    return 'Status: $status';
  }

  @override
  String get requestEmptyTitle => 'No requests yet';

  @override
  String get requestEmptyBody => 'Requests you submit will appear here.';

  @override
  String get requestEmptyAction => 'Go to Home to start a request';

  @override
  String get requestLoading => 'Loading your requests…';

  @override
  String get requestUnavailableTitle => 'Requests are temporarily unavailable';

  @override
  String get requestUnavailableBody =>
      'We couldn\'t reach Helping Hands. Your requests have not been removed. Try again.';

  @override
  String get requestDeniedTitle => 'Requests are unavailable for this account';

  @override
  String get requestDeniedBody =>
      'This account is not authorized to view beneficiary requests.';

  @override
  String get requestExpiredTitle => 'Your session has expired';

  @override
  String get requestExpiredBody => 'Sign in again to view your requests.';

  @override
  String get requestUnexpectedTitle => 'We couldn\'t load your requests';

  @override
  String get requestUnexpectedBody => 'Please try again.';

  @override
  String get tryAgain => 'Try again';

  @override
  String get loadMore => 'Load more';

  @override
  String get requestPrivacyNote =>
      'Your requests are private and visible only to you and authorized project participants. Support is coordinated outside the app.';

  @override
  String get supportSingleNeedUnavailable =>
      'Request submission is not available in this build yet.';

  @override
  String get phaseSingleNeed => 'Phase 1 · One need';

  @override
  String get phaseConfirmRequest => 'Phase 1 · Confirmations';

  @override
  String get phaseSubmittingRequest => 'Phase 1 · Submission';

  @override
  String get phaseReceipt => 'Phase 1 · Received';

  @override
  String get singleNeedTitle => 'Describe the support you need';

  @override
  String get singleNeedSubtitle =>
      'Share only the information needed to understand your request. The service does not guarantee approval or delivery.';

  @override
  String get singleNeedDescriptionLabel => 'Request description';

  @override
  String get singleNeedDescriptionHint => 'Describe the assistance you need';

  @override
  String get singleNeedDescriptionRequired =>
      'Describe the assistance you need before continuing.';

  @override
  String get singleNeedDescriptionTooLong =>
      'Keep the description within 4,000 characters.';

  @override
  String get singleNeedPrivacyNote =>
      'Avoid including unnecessary sensitive information.';

  @override
  String get continueToConfirm => 'Continue to review';

  @override
  String get cancelRequest => 'Cancel';

  @override
  String get confirmRequestTitle => 'Confirm authority and information sharing';

  @override
  String get confirmRequestSubtitle =>
      'Review these confirmations before submitting your request.';

  @override
  String get authorityConfirmationTitle =>
      'I confirm I am authorized to submit this request';

  @override
  String get authorityConfirmationBody =>
      'I am providing this request for myself or a person I am authorized to represent.';

  @override
  String get sharingConfirmationTitle =>
      'I agree to necessary sharing with participating charities';

  @override
  String get sharingConfirmationBody =>
      'Only the information needed to coordinate a response may be shared with authorized participants.';

  @override
  String get confirmationRequired => 'Confirm both statements to submit.';

  @override
  String get submitRequestNow => 'Submit request';

  @override
  String get submissionInProgress => 'Submitting your request…';

  @override
  String get submissionWaitNote =>
      'Please wait while the service records your request.';

  @override
  String get submissionCheckSuccess => 'Check connection';

  @override
  String get submissionUnknownTitle => 'We couldn\'t confirm the submission';

  @override
  String get submissionUnknownBody =>
      'The service may have received your request. Check My requests before trying again to avoid creating a duplicate.';

  @override
  String get submissionFailedTitle => 'Your request wasn\'t confirmed';

  @override
  String get submissionInvalidBody =>
      'The service could not accept the request details. Review them and try again.';

  @override
  String get submissionDeniedBody =>
      'This account is not allowed to submit this request.';

  @override
  String get submissionProfileBody =>
      'We couldn\'t identify one beneficiary profile for this account. No request was confirmed.';

  @override
  String get submissionConflictBody =>
      'The service reported a conflict. Check My requests before submitting again.';

  @override
  String get submissionUnavailableBody =>
      'The service is unavailable. We cannot confirm whether the request was recorded.';

  @override
  String get submissionUnexpectedBody =>
      'We could not confirm the request. Check My requests before trying again.';

  @override
  String get submissionSuccessTitle => 'Your request was submitted';

  @override
  String get submissionSuccessSubtitle =>
      'Keep this reference to follow the request status.';

  @override
  String get requestReferenceLabel => 'Reference number';

  @override
  String get requestCurrentStatusLabel => 'Current status';

  @override
  String get submissionReviewNote =>
      'Participating charities review requests according to their priorities and capacity. A review is not proof of aid delivery.';

  @override
  String get viewMyRequests => 'Go to My requests';

  @override
  String get backToHome => 'Return to Home';

  @override
  String get submissionTryAgain => 'Review and submit again';

  @override
  String get statusSubmitted => 'Submitted';

  @override
  String get statusOpen => 'Open';

  @override
  String get statusInReview => 'Under review';

  @override
  String statusUnknown(Object status) {
    return 'Status: $status';
  }

  @override
  String get noProfileForAccount =>
      'No single matching beneficiary profile was found.';
}
