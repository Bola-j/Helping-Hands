import 'package:dio/dio.dart';

import '../../auth/domain/session_user.dart';
import '../../auth/domain/beneficiary_profile_hint.dart';
import '../domain/beneficiary_request.dart';
import '../domain/single_need_submission.dart';

class SingleNeedRemoteDataSource {
  SingleNeedRemoteDataSource(this._dio);

  final Dio _dio;

  Future<BeneficiaryRequest> submit(
    SessionUser user,
    SingleNeedSubmission submission,
    BeneficiaryProfileHint? profileHint,
  ) async {
    late final String beneficiaryId;
    late final _RequestToken token;
    try {
      beneficiaryId = await _resolveBeneficiaryId(user, profileHint);
      token = await _loadRequestToken();
    } on DioException catch (error) {
      throw SingleNeedTransportFailure(error, writeAttempted: false);
    }
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/v1/requests/single-need',
        data: submission.toJson(beneficiaryId),
        options: Options(headers: {token.headerName: token.value}),
      );
      return BeneficiaryRequest.fromJson(response.data!);
    } on DioException catch (error) {
      throw SingleNeedTransportFailure(error, writeAttempted: true);
    }
  }

  Future<String> _resolveBeneficiaryId(
    SessionUser user,
    BeneficiaryProfileHint? profileHint,
  ) async {
    if (profileHint?.belongsTo(user) == true) return profileHint!.beneficiaryId;
    final response = await _dio.get<List<dynamic>>('/api/v1/beneficiaries');
    final matches = (response.data ?? const [])
        .whereType<Map<String, dynamic>>()
        .where((item) => item['displayName'] == user.displayName)
        .toList(growable: false);
    if (matches.length != 1 || matches.single['id'] is! String) {
      throw const SingleNeedFailure(SingleNeedFailureType.profileUnavailable);
    }
    return matches.single['id'] as String;
  }

  Future<_RequestToken> _loadRequestToken() async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/auth/antiforgery',
    );
    final data = response.data!;
    return _RequestToken(
      data['headerName'] as String,
      data['requestToken'] as String,
    );
  }
}

class SingleNeedTransportFailure implements Exception {
  const SingleNeedTransportFailure(this.error, {required this.writeAttempted});

  final DioException error;
  final bool writeAttempted;
}

class _RequestToken {
  const _RequestToken(this.headerName, this.value);

  final String headerName;
  final String value;
}
