import 'dart:convert';
import 'dart:io';

import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:mobile/core/network/dio_factory.dart';
import 'package:mobile/features/auth/data/beneficiary_registration_remote_data_source.dart';
import 'package:mobile/features/auth/data/beneficiary_registration_repository_impl.dart';
import 'package:mobile/features/auth/data/session_remote_data_source.dart';
import 'package:mobile/features/auth/data/session_repository_impl.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration.dart';
import 'package:mobile/features/requests/data/requests_remote_data_source.dart';
import 'package:mobile/features/requests/data/requests_repository_impl.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('beneficiary sees only own requests from the API', (_) async {
    const baseUrl = String.fromEnvironment('API_BASE_URL');
    expect(baseUrl, isNotEmpty);
    final cookies = CookieJar();
    final dio = DioFactory.create(cookieJar: cookies);
    _trustTestCertificate(dio);
    addTearDown(() => dio.close(force: true));

    final session = SessionRepositoryImpl(
      SessionRemoteDataSource(dio, cookies),
    );
    final registration = await _register(dio);
    final user = await session.signIn({
      'email': registration.email,
      'password': _password,
    });
    expect(user.roles, contains('Beneficiary'));

    final requests = RequestsRepositoryImpl(
      RequestsRemoteDataSource(dio),
      session,
    );
    expect((await requests.load()).items, isEmpty);
    final token = await _freshAntiforgeryToken(dio);
    await _submitSingleNeed(dio, registration.beneficiaryId, token);
    final page = await requests.load();

    expect(page.items, hasLength(1));
    expect(page.items.single.description, _description);
    expect(page.items.single.status, 'Submitted');
    expect(page.items.single.reference, startsWith('REQ-'));
  });
}

const _password = 'GoodPass#12345';
const _description = 'Fictional request-list integration check';

Future<BeneficiaryRegistration> _register(Dio dio) =>
    BeneficiaryRegistrationRepositoryImpl(
      BeneficiaryRegistrationRemoteDataSource(dio),
    ).register(
      BeneficiaryRegistrationRequest(
        displayName: 'Fictional request-list beneficiary',
        email: 'requests-${DateTime.now().microsecondsSinceEpoch}@example.test',
        password: _password,
        contactDetails: 'Fictional integration test account',
        isFamily: false,
      ),
    );

Future<String> _freshAntiforgeryToken(Dio dio) async {
  final response = await dio.get<Map<String, dynamic>>(
    '/api/v1/auth/antiforgery',
  );
  return response.data!['requestToken'] as String;
}

Future<void> _submitSingleNeed(
  Dio dio,
  String beneficiaryId,
  String token,
) async {
  final response = await dio.post<void>(
    '/api/v1/requests/single-need',
    data: {
      'beneficiaryId': beneficiaryId,
      'description': _description,
      'authorityConfirmed': true,
      'sharingAgreed': true,
    },
    options: Options(headers: {'X-CSRF-TOKEN': token}),
  );
  expect(response.statusCode, 201);
}

void _trustTestCertificate(Dio dio) {
  const encoded = String.fromEnvironment('API_CERTIFICATE_BASE64');
  if (encoded.isEmpty) {
    throw StateError('API_CERTIFICATE_BASE64 is required for local TLS tests.');
  }
  final context = SecurityContext(withTrustedRoots: true)
    ..setTrustedCertificatesBytes(base64Decode(encoded));
  dio.httpClientAdapter = IOHttpClientAdapter(
    createHttpClient: () => HttpClient(context: context),
  );
}
