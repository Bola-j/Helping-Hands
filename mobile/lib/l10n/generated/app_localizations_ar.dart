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
  String get comingSoon => 'ستتم إضافة هذه الشاشة في مهمة لاحقة.';
}
