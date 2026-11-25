import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/application/session_bloc.dart';
import 'package:mobile/features/auth/domain/session_failure.dart';
import 'package:mobile/features/auth/domain/session_repository.dart';
import 'package:mobile/features/auth/domain/session_user.dart';

void main() {
  late FakeSessionRepository repository;
  late SessionBloc bloc;

  setUp(() {
    repository = FakeSessionRepository();
    bloc = SessionBloc(repository);
  });

  tearDown(() => bloc.close());

  test('emits submitting then beneficiary signed-in state', () async {
    final states = <SessionState>[];
    final subscription = bloc.stream.listen(states.add);
    bloc.add(
      SignInSubmitted(
        email: ' beneficiary@example.test ',
        password: 'fictional-password',
      ),
    );
    await bloc.stream.firstWhere((state) => state is SessionSignedIn);

    expect(states, contains(isA<SessionSubmitting>()));
    expect(states.last, isA<SessionSignedIn>());
    expect(repository.credentials, {
      'email': 'beneficiary@example.test',
      'password': 'fictional-password',
    });
    await subscription.cancel();
  });

  for (final type in [
    SessionFailureType.invalidCredentials,
    SessionFailureType.denied,
    SessionFailureType.sessionExpired,
    SessionFailureType.apiUnavailable,
  ]) {
    test('exposes recoverable sign-in state for $type', () async {
      repository.signInFailure = SessionFailure(type);
      bloc.add(
        SignInSubmitted(email: 'fictional@example.test', password: 'test'),
      );
      final state =
          await bloc.stream.firstWhere(
                (state) => state is SessionSignedOut && state.failure != null,
              )
              as SessionSignedOut;
      expect(state.failure, type);
    });
  }

  test('signs out and returns to signed-out state', () async {
    await bloc.close();
    bloc = SessionBloc(repository, initialUser: fakeUser);
    bloc.add(SignOutRequested());
    expect(
      await bloc.stream.firstWhere((state) => state is SessionSignedOut),
      isA<SessionSignedOut>(),
    );
    expect(repository.signOutCalls, 1);
  });

  test('clears local signed-in state when remote sign-out fails', () async {
    await bloc.close();
    repository.signOutFailure = const SessionFailure(
      SessionFailureType.apiUnavailable,
    );
    bloc = SessionBloc(repository, initialUser: fakeUser);
    bloc.add(SignOutRequested());
    final state =
        await bloc.stream.firstWhere(
              (state) => state is SessionSignedOut && state.failure != null,
            )
            as SessionSignedOut;
    expect(state.failure, SessionFailureType.apiUnavailable);
  });
}

const fakeUser = SessionUser(
  id: 'fictional-user',
  displayName: 'Demo Beneficiary',
  email: 'beneficiary@example.test',
  roles: ['Beneficiary'],
);

class FakeSessionRepository implements SessionRepository {
  Map<String, Object?>? credentials;
  SessionFailure? signInFailure;
  SessionFailure? signOutFailure;
  int signOutCalls = 0;

  @override
  Future<SessionUser> signIn(Map<String, Object?> input) async {
    credentials = input;
    if (signInFailure case final failure?) throw failure;
    return fakeUser;
  }

  @override
  Future<void> signOut() async {
    signOutCalls++;
    if (signOutFailure case final failure?) throw failure;
  }

  @override
  Future<SessionUser> currentUser() async => fakeUser;

  @override
  Future<void> registerBeneficiary(Map<String, Object?> input) async {}
}
