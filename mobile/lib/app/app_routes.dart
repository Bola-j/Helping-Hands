import '../features/auth/domain/beneficiary_profile_hint.dart';
import '../features/auth/domain/session_user.dart';

abstract final class AppRoutes {
  static const welcome = '/';
  static const createAccount = '/create-account';
  static const signIn = '/sign-in';
  static const sessionExpired = '/session-expired';
  static const profile = '/profile';
  static const home = '/home';
  static const requests = '/requests';
  static const singleNeed = '/requests/single-need';
}

class SignInRouteArguments {
  const SignInRouteArguments({this.email, this.profileHint});

  final String? email;
  final BeneficiaryProfileHint? profileHint;
}

class BeneficiaryHomeRouteArguments {
  const BeneficiaryHomeRouteArguments(this.user, {this.profileHint});

  final SessionUser user;
  final BeneficiaryProfileHint? profileHint;
}
