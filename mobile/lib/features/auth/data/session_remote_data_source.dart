import 'package:dio/dio.dart';

class SessionRemoteDataSource {
  SessionRemoteDataSource(this._dio);

  final Dio _dio;
  String? _requestToken;

  Future<void> registerBeneficiary(Map<String, Object?> input) async {
    await _post('/api/v1/auth/register/beneficiary', input);
    await _refreshRequestToken();
  }

  Future<void> signIn(Map<String, Object?> credentials) async {
    await _post('/api/v1/auth/login', credentials);
    await _refreshRequestToken();
  }

  Future<Map<String, dynamic>> currentUser() async {
    final response = await _dio.get<Map<String, dynamic>>('/api/v1/auth/me');
    return response.data!;
  }

  Future<void> signOut() async {
    await _post('/api/v1/auth/logout', const {});
    _requestToken = null;
    await _refreshRequestToken();
  }

  Future<void> _post(String path, Map<String, Object?> body) async {
    final token = await _ensureRequestToken();
    await _dio.post<void>(
      path,
      data: body,
      options: Options(headers: {'X-CSRF-TOKEN': token}),
    );
  }

  Future<String> _ensureRequestToken() async {
    if (_requestToken case final token?) return token;
    return _refreshRequestToken();
  }

  Future<String> _refreshRequestToken() async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/v1/auth/antiforgery',
    );
    final data = response.data!;
    _requestToken = data['requestToken'] as String;
    return _requestToken!;
  }
}
