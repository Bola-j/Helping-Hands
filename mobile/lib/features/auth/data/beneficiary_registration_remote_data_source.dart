import 'package:dio/dio.dart';

import '../domain/beneficiary_registration.dart';

class BeneficiaryRegistrationRemoteDataSource {
  BeneficiaryRegistrationRemoteDataSource(this._dio);

  final Dio _dio;
  String? _requestToken;
  String? _requestHeaderName;

  Future<BeneficiaryRegistration> register(
    BeneficiaryRegistrationRequest request,
  ) async {
    final token = await _ensureRequestToken();
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/auth/register/beneficiary',
      data: request.toJson(),
      options: Options(headers: {_requestHeaderName!: token}),
    );
    return BeneficiaryRegistration.fromJson(response.data!);
  }

  Future<String> _ensureRequestToken() async {
    if (_requestToken case final token?) return token;
    return _refreshRequestToken();
  }

  Future<String> _refreshRequestToken() async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/auth/antiforgery',
    );
    _requestToken = response.data!['requestToken'] as String;
    _requestHeaderName = response.data!['headerName'] as String;
    return _requestToken!;
  }
}
