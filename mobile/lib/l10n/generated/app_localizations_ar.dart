// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Arabic (`ar`).
class AppLocalizationsAr extends AppLocalizations {
  AppLocalizationsAr([String locale = 'ar']) : super(locale);

  @override
  String get switchLanguage => 'English';

  @override
  String get phaseBadge => 'المرحلة ١ و ٢ · المستفيد';

  @override
  String get welcomeTitle => 'أهلاً بك في أيادي الخير';

  @override
  String get welcomeSubtitle =>
      'منصة لتسجيل ومتابعة طلبات الدعم الشخصي بهدوء وخصوصية،\nلنساعد الجمعيات المعتمدة على تنسيق المساعدة المناسبة لك.';

  @override
  String get chooseLanguage => 'اختر لغة التطبيق';

  @override
  String get english => 'English';

  @override
  String get arabic => 'العربية';

  @override
  String get scopeNotice =>
      'يسجّل التطبيق طلبك وتحديثاته فقط. لا يجمع أموالاً ولا\nيضمن قبول الطلب، ويتم تنسيق الدعم خارج التطبيق عبر\nجمعيات معتمدة.';

  @override
  String get createBeneficiaryAccount => 'إنشاء حساب مستفيد';

  @override
  String get signInExistingAccount => 'تسجيل الدخول إلى حساب حالي';

  @override
  String get createAccount => 'إنشاء حساب';

  @override
  String get signIn => 'تسجيل الدخول';

  @override
  String get back => 'رجوع';

  @override
  String get registrationIntro =>
      'أنشئ ملف المستفيد. ستسجل الدخول في خطوة منفصلة بعد إنشاء الحساب.';

  @override
  String get fullName => 'الاسم الكامل';

  @override
  String get email => 'البريد الإلكتروني';

  @override
  String get password => 'كلمة المرور';

  @override
  String get contactDetailsOptional => 'بيانات التواصل (اختياري)';

  @override
  String get contactDetailsHint => 'بيانات تواصل اختيارية لملفك';

  @override
  String get profileFor => 'لمن سيكون ملف المستفيد؟';

  @override
  String get forMe => 'لي';

  @override
  String get forFamily => 'لأسرتي';

  @override
  String get profileForRequired => 'اختر لمن سيكون الملف.';

  @override
  String get nameRequired => 'أدخل اسمك.';

  @override
  String get nameTooLong => 'يجب ألا يتجاوز الاسم ٢٠٠ حرف.';

  @override
  String get emailRequired => 'أدخل بريدك الإلكتروني.';

  @override
  String get emailInvalid => 'أدخل بريداً إلكترونياً صالحاً.';

  @override
  String get passwordRequirements =>
      'استخدم ١٢ حرفاً على الأقل، تشمل حرفاً كبيراً وصغيراً ورقماً ورمزاً.';

  @override
  String get contactDetailsTooLong => 'يجب ألا تتجاوز بيانات التواصل ٥٠٠ حرف.';

  @override
  String get showPassword => 'إظهار كلمة المرور';

  @override
  String get hidePassword => 'إخفاء كلمة المرور';

  @override
  String get alreadyHaveAccount => 'لديك حساب بالفعل؟ سجل الدخول';

  @override
  String get continueToSignIn => 'المتابعة إلى تسجيل الدخول';

  @override
  String get registrationSuccess => 'تم إنشاء حسابك';

  @override
  String get registrationNextStep =>
      'حسابك جاهز. تابع لتسجيل الدخول ببريدك الإلكتروني وكلمة المرور.';

  @override
  String beneficiaryReference(Object reference) {
    return 'رقم ملف المستفيد: $reference';
  }

  @override
  String get registrationDuplicateEmail =>
      'هذا البريد مسجل بالفعل. يمكنك تسجيل الدخول بدلاً من ذلك.';

  @override
  String get registrationInvalidRequest =>
      'تعذر قبول هذه البيانات. راجع الحقول وحاول مرة أخرى.';

  @override
  String get registrationApiUnavailable =>
      'التسجيل غير متاح مؤقتاً. تحقق من الاتصال وحاول مرة أخرى.';

  @override
  String get registrationUnexpected => 'تعذر إكمال التسجيل. حاول مرة أخرى.';

  @override
  String get comingSoon => 'ستتم إضافة هذه الشاشة في مهمة لاحقة.';

  @override
  String get invalidEmail => 'أدخل بريداً إلكترونياً صحيحاً.';

  @override
  String get signInInvalidCredentials =>
      'البريد الإلكتروني أو كلمة المرور غير صحيحة. تحقق من بياناتك وحاول مجدداً.';

  @override
  String get signInExpired =>
      'انتهت صلاحية جلستك. سجّل الدخول مجدداً للمتابعة.';

  @override
  String get signInDenied => 'لا يمكن لهذا الحساب استخدام تطبيق المستفيد.';

  @override
  String get signInUnavailable =>
      'تعذر الاتصال بخدمة أيادي الخير. تحقق من اتصالك وحاول مجدداً.';

  @override
  String get signInInvalidRequest => 'تعذر إتمام تسجيل الدخول. حاول مجدداً.';

  @override
  String get signInUnexpected => 'تعذر تسجيل الدخول. حاول مجدداً.';

  @override
  String get signOut => 'تسجيل الخروج';

  @override
  String get signOutFailed => 'تعذر تأكيد تسجيل الخروج من الخدمة. حاول مجدداً.';

  @override
  String get signedOut => 'تم تسجيل الخروج.';

  @override
  String get backToSignIn => 'الانتقال إلى تسجيل الدخول';

  @override
  String get passwordRequired => 'أدخل كلمة المرور.';

  @override
  String get signInTitle => 'تسجيل الدخول';

  @override
  String get signInSubtitle =>
      'أدخل البريد الإلكتروني وكلمة المرور اللذين استخدمتهما لإنشاء حسابك.';

  @override
  String get phaseSignIn => 'المرحلة ١ · تسجيل الدخول';

  @override
  String get phaseSessionExpired => 'انتهت الجلسة · المرحلة ١ و٢';

  @override
  String get noBeneficiaryAccount => 'ليس لديك حساب مستفيد؟';

  @override
  String get sessionExpiredTitle => 'تم تسجيل خروجك لحماية حسابك';

  @override
  String get sessionExpiredBody =>
      'انتهت الجلسة بسبب عدم النشاط أو تم تسجيل خروجك. تظل طلباتك المسجلة محفوظة. سجّل الدخول مجدداً للمتابعة.';

  @override
  String get sessionExpiredPrivacy =>
      'لن تظهر بياناتك الشخصية أو تفاصيل طلباتك حتى تسجل الدخول مجدداً.';

  @override
  String get signInAgain => 'إعادة تسجيل الدخول';

  @override
  String get returnWelcome => 'العودة لشاشة اللغة والترحيب';

  @override
  String get profileTitle => 'الحساب والإعدادات';

  @override
  String get profileSubtitle => 'المرحلة ١ و٢ · اللغة وتسجيل الخروج';

  @override
  String get englishLtr => 'English (LTR)';

  @override
  String get arabicRtl => 'العربية (RTL)';

  @override
  String get homeNavigation => 'الرئيسية';

  @override
  String get requestsNavigation => 'طلباتي';

  @override
  String get accountNavigation => 'الحساب';

  @override
  String get beneficiaryGreeting => 'مرحباً،';

  @override
  String get homeNewRequestLabel => 'تقديم طلب جديد';

  @override
  String get homeQuestion => 'هل تحتاج إلى تسجيل طلب دعم؟';

  @override
  String get homeQuestionBody =>
      'اختر نوع الدعم الذي يناسب احتياجك الحالي. يمكنك مراجعة التفاصيل قبل الإرسال.';

  @override
  String get singleNeedRequest => 'طلب احتياج واحد (المرحلة ١)';

  @override
  String get multipleNeedsRequest => 'طلب احتياجات متعددة (المرحلة ٢)';

  @override
  String get requestsTitle => 'طلباتي';

  @override
  String get requestsSubtitle => 'طلباتك المسجلة فقط';

  @override
  String get viewAllRequests => 'عرض الكل';

  @override
  String requestMetadata(Object count, Object date, Object phase) {
    return 'المرحلة $phase · $count احتياجات · $date';
  }

  @override
  String get requestStatusSubmitted => 'تم الإرسال';

  @override
  String get requestStatusOpen => 'مفتوح';

  @override
  String get requestStatusInReview => 'قيد المراجعة';

  @override
  String get requestStatusClarification => 'مطلوب توضيح';

  @override
  String get requestStatusAccepted => 'مقبول';

  @override
  String get requestStatusRejected => 'مرفوض';

  @override
  String get requestStatusFullyAccepted => 'تم قبول كل الاحتياجات';

  @override
  String get requestStatusPartiallyAccepted => 'مقبول جزئياً';

  @override
  String get requestStatusFullyRejected => 'تم رفض كل الاحتياجات';

  @override
  String get requestStatusCompleted => 'مغلق';

  @override
  String requestStatusUnknown(Object status) {
    return 'الحالة: $status';
  }

  @override
  String get requestEmptyTitle => 'لا توجد طلبات بعد';

  @override
  String get requestEmptyBody => 'ستظهر هنا الطلبات التي تسجلها.';

  @override
  String get requestEmptyAction => 'العودة للرئيسية لبدء طلب';

  @override
  String get requestLoading => 'جارٍ تحميل طلباتك…';

  @override
  String get requestUnavailableTitle => 'الطلبات غير متاحة مؤقتاً';

  @override
  String get requestUnavailableBody =>
      'تعذر الاتصال بخدمة أيادي الخير. لم تُحذف طلباتك. حاول مجدداً.';

  @override
  String get requestDeniedTitle => 'الطلبات غير متاحة لهذا الحساب';

  @override
  String get requestDeniedBody =>
      'هذا الحساب غير مصرح له بعرض طلبات المستفيدين.';

  @override
  String get requestExpiredTitle => 'انتهت صلاحية جلستك';

  @override
  String get requestExpiredBody => 'سجّل الدخول مجدداً لعرض طلباتك.';

  @override
  String get requestUnexpectedTitle => 'تعذر تحميل طلباتك';

  @override
  String get requestUnexpectedBody => 'يرجى المحاولة مجدداً.';

  @override
  String get tryAgain => 'حاول مجدداً';

  @override
  String get loadMore => 'تحميل المزيد';

  @override
  String get requestPrivacyNote =>
      'طلباتك خاصة ولا يراها إلا أنت والمشاركون المصرح لهم في المشروع. يتم تنسيق الدعم خارج التطبيق.';

  @override
  String get supportSingleNeedUnavailable =>
      'تسجيل الطلب غير متاح في هذا الإصدار بعد.';

  @override
  String get phaseSingleNeed => 'المرحلة ١ · احتياج واحد';

  @override
  String get phaseConfirmRequest => 'المرحلة ١ · التأكيدات';

  @override
  String get phaseSubmittingRequest => 'المرحلة ١ · إرسال الطلب';

  @override
  String get phaseReceipt => 'المرحلة ١ · تم الاستلام';

  @override
  String get singleNeedTitle => 'تسجيل طلب احتياج واحد';

  @override
  String get singleNeedSubtitle =>
      'اذكر المعلومات اللازمة لفهم طلبك فقط. لا يضمن النظام قبول الطلب أو وصول الدعم.';

  @override
  String get singleNeedDescriptionLabel => 'وصف الطلب';

  @override
  String get singleNeedDescriptionHint => 'صِف المساعدة التي تحتاج إليها';

  @override
  String get singleNeedDescriptionRequired =>
      'صِف المساعدة التي تحتاج إليها للمتابعة.';

  @override
  String get singleNeedDescriptionTooLong => 'يجب ألا يتجاوز الوصف ٤٠٠٠ حرف.';

  @override
  String get singleNeedPrivacyNote => 'تجنب إضافة معلومات حساسة غير ضرورية.';

  @override
  String get continueToConfirm => 'المتابعة للمراجعة';

  @override
  String get cancelRequest => 'إلغاء';

  @override
  String get confirmRequestTitle => 'تأكيد الصلاحية ومشاركة البيانات';

  @override
  String get confirmRequestSubtitle =>
      'قبل الإرسال، راجع التأكيدين لحماية خصوصيتك.';

  @override
  String get authorityConfirmationTitle => 'أؤكد أن لدي صلاحية تقديم هذا الطلب';

  @override
  String get authorityConfirmationBody =>
      'أقدم هذا الطلب لنفسي أو لأسرتي بمعلومات صحيحة حسب معرفتي.';

  @override
  String get sharingConfirmationTitle =>
      'أوافق على مشاركة المعلومات اللازمة مع الجمعيات المشاركة';

  @override
  String get sharingConfirmationBody =>
      'قد تُشارك المعلومات اللازمة للتنسيق فقط مع المشاركين المصرح لهم.';

  @override
  String get confirmationRequired => 'وافق على كلا التأكيدين لإرسال الطلب.';

  @override
  String get submitRequestNow => 'إرسال الطلب الآن';

  @override
  String get submissionInProgress => 'جارٍ تسجيل طلبك…';

  @override
  String get submissionWaitNote => 'يرجى الانتظار حتى تسجل الخدمة طلبك.';

  @override
  String get submissionCheckSuccess => 'التحقق من الاتصال';

  @override
  String get submissionUnknownTitle => 'تعذر تأكيد إرسال الطلب';

  @override
  String get submissionUnknownBody =>
      'قد تكون الخدمة قد استلمت طلبك. تحقق من طلباتي قبل المحاولة مجدداً لتجنب التكرار.';

  @override
  String get submissionFailedTitle => 'لم يتم تأكيد الطلب';

  @override
  String get submissionInvalidBody =>
      'تعذر قبول تفاصيل الطلب. راجعها وحاول مجدداً.';

  @override
  String get submissionDeniedBody => 'هذا الحساب غير مصرح له بإرسال هذا الطلب.';

  @override
  String get submissionProfileBody =>
      'تعذر تحديد ملف مستفيد واحد لهذا الحساب. لم يتم تأكيد أي طلب.';

  @override
  String get submissionConflictBody =>
      'أبلغت الخدمة عن تعارض. تحقق من طلباتي قبل إعادة الإرسال.';

  @override
  String get submissionUnavailableBody =>
      'الخدمة غير متاحة. لا يمكننا تأكيد ما إذا تم تسجيل الطلب.';

  @override
  String get submissionUnexpectedBody =>
      'تعذر تأكيد الطلب. تحقق من طلباتي قبل المحاولة مجدداً.';

  @override
  String get submissionSuccessTitle => 'تم تسجيل طلبك';

  @override
  String get submissionSuccessSubtitle =>
      'احتفظ بهذا الرقم لمتابعة حالة الطلب.';

  @override
  String get requestReferenceLabel => 'رقم المرجع';

  @override
  String get requestCurrentStatusLabel => 'الحالة الحالية';

  @override
  String get submissionReviewNote =>
      'تراجع الجمعيات المشاركة الطلبات وفق الأولوية والقدرة. المراجعة لا تثبت وصول المساعدة.';

  @override
  String get viewMyRequests => 'الانتقال إلى طلباتي';

  @override
  String get backToHome => 'العودة للرئيسية';

  @override
  String get submissionTryAgain => 'مراجعة الطلب وإعادة الإرسال';

  @override
  String get statusSubmitted => 'تم الإرسال';

  @override
  String get statusOpen => 'مفتوح';

  @override
  String get statusInReview => 'قيد المراجعة';

  @override
  String statusUnknown(Object status) {
    return 'الحالة: $status';
  }

  @override
  String get noProfileForAccount => 'لم يتم العثور على ملف مستفيد واحد مطابق.';
}
