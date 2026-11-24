import 'package:dio/dio.dart';

import '../domain/session_failure.dart';
import '../domain/session_repository.dart';
import '../domain/session_user.dart';
import 'session_remote_data_source.dart';

class SessionRepositoryImpl implements SessionRepository {
  SessionRepositoryImpl(this._remote);

  final SessionRemoteDataSource _remote;
  bool _hasAuthenticatedSession = false;

  @override
  Future<void> registerBeneficiary(Map<String, Object?> input) =>
      _guard(() => _remote.registerBeneficiary(input));

  @override
  Future<SessionUser> signIn(Map<String, Object?> credentials) =>
      _guard(() async {
        await _remote.signIn(credentials);
        _hasAuthenticatedSession = true;
        try {
          return SessionUser.fromJson(await _remote.currentUser());
        } on SessionFailure {
          _hasAuthenticatedSession = false;
          rethrow;
        }
      }, unauthorized: SessionFailureType.invalidCredentials);

  @override
  Future<SessionUser> currentUser() =>
      _guard(() async => SessionUser.fromJson(await _remote.currentUser()));

  @override
  Future<void> signOut() async {
    await _guard(_remote.signOut);
    _hasAuthenticatedSession = false;
  }

  Future<T> _guard<T>(
    Future<T> Function() action, {
    SessionFailureType unauthorized = SessionFailureType.unauthenticated,
  }) async {
    try {
      return await action();
    } on DioException catch (error) {
      final failure = _failureType(error, unauthorized);
      if (error.response?.statusCode == 401) _hasAuthenticatedSession = false;
      throw SessionFailure(failure);
    }
  }

  SessionFailureType _failureType(
    DioException error,
    SessionFailureType unauthorized,
  ) {
    return switch (error.response?.statusCode) {
      401 =>
        _hasAuthenticatedSession
            ? SessionFailureType.sessionExpired
            : unauthorized,
      403 => SessionFailureType.denied,
      400 => SessionFailureType.invalidRequest,
      final status? when status >= 500 => SessionFailureType.apiUnavailable,
      _ when _isConnectionFailure(error) => SessionFailureType.apiUnavailable,
      _ => SessionFailureType.unexpected,
    };
  }

  bool _isConnectionFailure(DioException error) => switch (error.type) {
    DioExceptionType.connectionError ||
    DioExceptionType.connectionTimeout ||
    DioExceptionType.receiveTimeout ||
    DioExceptionType.sendTimeout => true,
    _ => false,
  };
}
