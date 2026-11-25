import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/session_failure.dart';
import '../domain/session_repository.dart';
import '../domain/session_user.dart';

sealed class SessionEvent {}

final class SignInSubmitted extends SessionEvent {
  SignInSubmitted({required this.email, required this.password});

  final String email;
  final String password;
}

final class SignOutRequested extends SessionEvent {}

sealed class SessionState {}

final class SessionSignedOut extends SessionState {
  SessionSignedOut({this.failure});

  final SessionFailureType? failure;
}

final class SessionSubmitting extends SessionState {}

final class SessionSignedIn extends SessionState {
  SessionSignedIn(this.user);

  final SessionUser user;
}

final class SessionSigningOut extends SessionState {}

class SessionBloc extends Bloc<SessionEvent, SessionState> {
  SessionBloc(this._repository, {SessionUser? initialUser})
    : super(
        initialUser == null ? SessionSignedOut() : SessionSignedIn(initialUser),
      ) {
    on<SignInSubmitted>(_signIn);
    on<SignOutRequested>(_signOut);
  }

  final SessionRepository _repository;

  Future<void> _signIn(
    SignInSubmitted event,
    Emitter<SessionState> emit,
  ) async {
    emit(SessionSubmitting());
    try {
      final user = await _repository.signIn({
        'email': event.email.trim(),
        'password': event.password,
      });
      emit(SessionSignedIn(user));
    } on SessionFailure catch (failure) {
      emit(SessionSignedOut(failure: failure.type));
    } catch (_) {
      emit(SessionSignedOut(failure: SessionFailureType.unexpected));
    }
  }

  Future<void> _signOut(
    SignOutRequested event,
    Emitter<SessionState> emit,
  ) async {
    final previous = state;
    final user = switch (previous) {
      SessionSignedIn(:final user) => user,
      _ => null,
    };
    if (user == null) return;
    emit(SessionSigningOut());
    try {
      await _repository.signOut();
      emit(SessionSignedOut());
    } on SessionFailure catch (failure) {
      if (failure.type == SessionFailureType.unauthenticated ||
          failure.type == SessionFailureType.sessionExpired) {
        emit(SessionSignedOut());
      } else {
        emit(SessionSignedOut(failure: failure.type));
      }
    } catch (_) {
      emit(SessionSignedOut(failure: SessionFailureType.unexpected));
    }
  }
}
