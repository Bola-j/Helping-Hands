import 'package:dio/dio.dart';

import '../../auth/domain/session_failure.dart';
import '../../auth/domain/session_repository.dart';
import '../../auth/domain/beneficiary_profile_hint.dart';
import '../domain/beneficiary_request.dart';
import '../domain/single_need_submission.dart';
import 'single_need_remote_data_source.dart';

class SingleNeedSubmissionRepositoryImpl
    implements SingleNeedSubmissionRepository {
  SingleNeedSubmissionRepositoryImpl(this._remote, this._session);

  final SingleNeedRemoteDataSource _remote;
  final SessionRepository _session;

  @override
  Future<BeneficiaryRequest> submit(
    SingleNeedSubmission submission, {
    BeneficiaryProfileHint? profileHint,
  }) async {
    _validate(submission);
    try {
      final user = await _session.currentUser();
      return await _remote.submit(user, submission, profileHint);
    } on SingleNeedFailure {
      rethrow;
    } on SingleNeedTransportFailure catch (failure) {
      throw await _mapFailure(failure.error, failure.writeAttempted);
    } on SessionFailure catch (failure) {
      throw SingleNeedFailure(switch (failure.type) {
        SessionFailureType.sessionExpired ||
        SessionFailureType.unauthenticated =>
          SingleNeedFailureType.sessionExpired,
        SessionFailureType.denied => SingleNeedFailureType.denied,
        SessionFailureType.apiUnavailable => SingleNeedFailureType.unavailable,
        _ => SingleNeedFailureType.unexpected,
      });
    }
  }

  void _validate(SingleNeedSubmission submission) {
    final text = submission.description.trim();
    if (text.isEmpty ||
        text.length > 4000 ||
        !submission.authorityConfirmed ||
        !submission.sharingAgreed) {
      throw const SingleNeedFailure(SingleNeedFailureType.invalidInput);
    }
  }

  Future<SingleNeedFailure> _mapFailure(
    DioException error,
    bool writeAttempted,
  ) async {
    final status = error.response?.statusCode;
    if (status == 401) {
      await _clearExpiredSession();
      return const SingleNeedFailure(SingleNeedFailureType.sessionExpired);
    }
    if (status == 403) {
      return const SingleNeedFailure(SingleNeedFailureType.denied);
    }
    if (status == 404) {
      return const SingleNeedFailure(SingleNeedFailureType.profileUnavailable);
    }
    if (status == 409) {
      return const SingleNeedFailure(SingleNeedFailureType.conflict);
    }
    if (status == 400) {
      return const SingleNeedFailure(SingleNeedFailureType.invalidInput);
    }
    if (writeAttempted && _couldHaveReachedServer(error)) {
      return const SingleNeedFailure(SingleNeedFailureType.outcomeUnconfirmed);
    }
    return const SingleNeedFailure(SingleNeedFailureType.unavailable);
  }

  Future<void> _clearExpiredSession() async {
    try {
      await _session.currentUser();
    } on SessionFailure {
      // SessionRepository clears rejected cookies for expiry and denial.
    } catch (_) {
      // Preserve the submission result if the cleanup request also fails.
    }
  }

  bool _couldHaveReachedServer(DioException error) {
    final status = error.response?.statusCode;
    return (status != null && status >= 500) ||
        error.type == DioExceptionType.connectionError ||
        error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout;
  }
}
