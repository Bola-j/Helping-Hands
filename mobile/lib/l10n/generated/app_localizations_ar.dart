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
}
