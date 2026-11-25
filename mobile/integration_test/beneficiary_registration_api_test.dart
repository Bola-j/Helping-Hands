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
import 'package:mobile/features/auth/domain/beneficiary_registration.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration_failure.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('registers a fictional beneficiary through the native API client', (
    tester,
  ) async {
    const apiBaseUrl = String.fromEnvironment('API_BASE_URL');
    expect(apiBaseUrl, isNotEmpty);
    final cookieJar = CookieJar();
    final dio = DioFactory.create(cookieJar: cookieJar);
    _trustTestApiCertificate(dio);
    final trace = <String>[];
    dio.interceptors.add(
      InterceptorsWrapper(
        onResponse: (response, handler) {
          trace.add(
            'HTTP ${response.statusCode} ${response.requestOptions.path}',
          );
          handler.next(response);
        },
        onError: (error, handler) {
          trace.add(
            'Dio ${error.type.name} ${error.error.runtimeType} HTTP ${error.response?.statusCode ?? '-'} ${error.requestOptions.path}',
          );
          handler.next(error);
        },
      ),
    );
    addTearDown(() => dio.close(force: true));

    final repository = BeneficiaryRegistrationRepositoryImpl(
      BeneficiaryRegistrationRemoteDataSource(dio),
    );
    late final BeneficiaryRegistration registration;
    try {
      registration = await repository.register(
        BeneficiaryRegistrationRequest(
          displayName: 'Fictional mobile beneficiary',
          email: 'mobile-${DateTime.now().microsecondsSinceEpoch}@example.test',
          password: 'FictionalPass#123',
          contactDetails: 'Fictional contact',
          isFamily: false,
        ),
      );
    } on BeneficiaryRegistrationFailure catch (failure) {
      fail('Registration failed with ${failure.type.name}: $trace');
    }

    expect(registration.accountId, isNotEmpty);
    expect(registration.beneficiaryId, isNotEmpty);
    expect(registration.reference, startsWith('BEN-'));
    final cookies = await cookieJar.loadForRequest(Uri.parse(apiBaseUrl));
    expect(
      cookies.map((cookie) => cookie.name),
      isNot(contains('.AspNetCore.Identity.Application')),
    );
  });
}

void _trustTestApiCertificate(Dio dio) {
  const encodedCertificate = String.fromEnvironment('API_CERTIFICATE_BASE64');
  if (encodedCertificate.isEmpty) {
    throw StateError(
      'API_CERTIFICATE_BASE64 is required for local TLS testing.',
    );
  }
  final context = SecurityContext(withTrustedRoots: true)
    ..setTrustedCertificatesBytes(base64Decode(encodedCertificate));
  dio.httpClientAdapter = IOHttpClientAdapter(
    createHttpClient: () => HttpClient(context: context),
  );
}
