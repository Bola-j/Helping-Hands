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
}
