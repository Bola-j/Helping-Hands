import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:mobile/core/network/dio_factory.dart';
import 'package:mobile/features/auth/data/session_remote_data_source.dart';
import 'package:mobile/features/auth/data/session_repository_impl.dart';
import 'package:mobile/features/auth/domain/session_failure.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('native client follows beneficiary session contract', (_) async {
    const baseUrl = String.fromEnvironment('API_BASE_URL');
    expect(baseUrl, isNotEmpty);
    final cookieJar = CookieJar();
    final dio = DioFactory.create(cookieJar: cookieJar);
    final responseStatuses = <int>[];
    dio.interceptors.add(
      InterceptorsWrapper(
        onError: (error, handler) {
          if (error.response?.statusCode case final status?) {
            responseStatuses.add(status);
          }
          handler.next(error);
        },
      ),
    );
    final repository = SessionRepositoryImpl(SessionRemoteDataSource(dio));
    final email =
        'native-${DateTime.now().microsecondsSinceEpoch}@example.test';
    const password = 'GoodPass#12345';
    final apiUri = Uri.parse(baseUrl);

    final tokenResponse = await dio.get<Map<String, dynamic>>(
      '/api/v1/auth/antiforgery',
    );
    expect(tokenResponse.data?['requestToken'], isNotEmpty);
    expect(await cookieJar.loadForRequest(apiUri), isNotEmpty);

    try {
      await repository.registerBeneficiary({
        'displayName': 'Native API Test',
        'email': email,
        'password': password,
        'contactDetails': null,
        'isFamily': false,
      });
    } on SessionFailure catch (error) {
      fail(
        'Registration failed: ${error.type}, HTTP ${responseStatuses.lastOrNull}.',
      );
    }
    expect(await cookieJar.loadForRequest(apiUri), hasLength(1));

    final user = await repository.signIn({
      'email': email,
      'password': password,
    });
    expect(user.email, email);
    expect(user.roles, contains('Beneficiary'));
    expect(await cookieJar.loadForRequest(apiUri), hasLength(2));
    expect((await repository.currentUser()).id, user.id);
    await _expectDenied(dio);

    await repository.signOut();
    expect(await cookieJar.loadForRequest(apiUri), hasLength(1));
    await _expectFailure(
      repository.currentUser,
      SessionFailureType.unauthenticated,
    );
    await _expectFailure(
      () => repository.signIn({'email': email, 'password': 'WrongPass#12345'}),
      SessionFailureType.invalidCredentials,
    );
    await _expectUnavailable(baseUrl);
  });
}

Future<void> _expectDenied(Dio dio) async {
  try {
    await dio.get<void>('/api/v1/catalog/categories');
    fail('Beneficiary access to staff catalog unexpectedly succeeded.');
  } on DioException catch (error) {
    expect(error.response?.statusCode, 403);
  }
}

Future<void> _expectFailure(
  Future<Object?> Function() action,
  SessionFailureType expected,
) async {
  try {
    await action();
    fail('Expected session failure $expected.');
  } on SessionFailure catch (error) {
    expect(error.type, expected);
  }
}

Future<void> _expectUnavailable(String baseUrl) async {
  final unavailableDio = DioFactory.create(cookieJar: CookieJar())
    ..options.baseUrl = Uri.parse(baseUrl).replace(port: 59999).toString();
  final repository = SessionRepositoryImpl(
    SessionRemoteDataSource(unavailableDio),
  );
  await _expectFailure(
    repository.currentUser,
    SessionFailureType.apiUnavailable,
  );
}
