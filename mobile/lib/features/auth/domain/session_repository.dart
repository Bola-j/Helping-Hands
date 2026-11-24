import 'session_user.dart';

abstract interface class SessionRepository {
  Future<void> registerBeneficiary(Map<String, Object?> input);
  Future<SessionUser> signIn(Map<String, Object?> credentials);
  Future<SessionUser> currentUser();
  Future<void> signOut();
}
