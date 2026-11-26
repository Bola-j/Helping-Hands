import 'package:dio/dio.dart';

import '../../auth/domain/session_failure.dart';
import '../../auth/domain/session_repository.dart';
import '../domain/requests_failure.dart';
import '../domain/requests_repository.dart';
import 'requests_remote_data_source.dart';

class RequestsRepositoryImpl implements RequestsRepository {
  RequestsRepositoryImpl(this._remote, this._session);

  final RequestsRemoteDataSource _remote;
  final SessionRepository _session;

  @override
  Future<RequestPage> load({String? cursor}) async {
    try {
      return await _remote.load(cursor: cursor);
    } on DioException catch (error) {
      throw await _mapFailure(error);
    }
  }

  Future<RequestsFailure> _mapFailure(DioException error) async {
    if (error.response?.statusCode == 401) {
      await _clearExpiredSession();
      return const RequestsFailure(RequestsFailureType.sessionExpired);
    }
    if (error.response?.statusCode == 403) {
      return const RequestsFailure(RequestsFailureType.denied);
    }
    if (_isUnavailable(error)) {
      return const RequestsFailure(RequestsFailureType.unavailable);
    }
    return const RequestsFailure(RequestsFailureType.unexpected);
  }

  Future<void> _clearExpiredSession() async {
    try {
      await _session.currentUser();
    } on SessionFailure catch (failure) {
      if (failure.type == SessionFailureType.sessionExpired ||
          failure.type == SessionFailureType.unauthenticated) {
        return;
      }
    } catch (_) {
      // Preserve the expiry state if session cleanup cannot reach the API.
    }
  }

  bool _isUnavailable(DioException error) {
    final status = error.response?.statusCode;
    return (status != null && status >= 500) ||
        error.type == DioExceptionType.connectionError ||
        error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout;
  }
}
