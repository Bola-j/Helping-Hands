import 'dart:convert';
import 'dart:typed_data';

import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/data/beneficiary_registration_remote_data_source.dart';
import 'package:mobile/features/auth/data/beneficiary_registration_repository_impl.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration_failure.dart';

void main() {
  late CookieJar cookieJar;
  late RecordingAdapter adapter;
  late Dio dio;

  setUp(() {
    cookieJar = CookieJar();
    adapter = RecordingAdapter();
    dio =
        Dio(
            BaseOptions(
              baseUrl: 'https://api.example.test',
              responseType: ResponseType.json,
            ),
          )
          ..httpClientAdapter = adapter
          ..interceptors.add(CookieManager(cookieJar));
  });

  tearDown(() => dio.close(force: true));

  test(
    'posts the verified DTO with antiforgery cookie and returns account',
    () async {
      final dataSource = BeneficiaryRegistrationRemoteDataSource(dio);
      final result = await dataSource.register(
        const BeneficiaryRegistrationRequest(
          displayName: 'Fictional beneficiary',
          email: 'beneficiary@example.test',
          password: 'FictionalPass#123',
          contactDetails: 'Fictional contact',
          isFamily: true,
        ),
      );

      expect(result.accountId, 'account-1');
      expect(result.beneficiaryId, 'beneficiary-1');
      expect(result.reference, 'BEN-00000001');
      expect(adapter.requests.map((request) => request.path), [
        '/api/v1/auth/antiforgery',
        '/api/v1/auth/register/beneficiary',
      ]);
      final request = adapter.requests[1];
      expect(request.headers['X-CSRF-TOKEN'], 'fictional-token');
      expect(request.headers['cookie'], contains('HH-Anti=fictional-cookie'));
      expect(request.body, {
        'displayName': 'Fictional beneficiary',
        'email': 'beneficiary@example.test',
        'password': 'FictionalPass#123',
        'contactDetails': 'Fictional contact',
        'isFamily': true,
      });
      final cookies = await cookieJar.loadForRequest(
        Uri.parse('https://api.example.test/api/v1/auth/me'),
      );
      expect(cookies.map((cookie) => cookie.name), contains('HH-Anti'));
      expect(
        cookies.map((cookie) => cookie.name),
        isNot(contains('.AspNetCore.Identity.Application')),
      );
    },
  );

  test('maps the API duplicate-email validation response', () async {
    adapter.respond = (request) {
      if (request.path.endsWith('/antiforgery')) {
        return jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        });
      }
      return jsonResponse(400, {
        'errors': {
          'DuplicateEmail': ['Email is already registered.'],
        },
      });
    };
    final repository = BeneficiaryRegistrationRepositoryImpl(
      BeneficiaryRegistrationRemoteDataSource(dio),
    );

    await expectLater(
      repository.register(
        const BeneficiaryRegistrationRequest(
          displayName: 'Fictional beneficiary',
          email: 'duplicate@example.test',
          password: 'FictionalPass#123',
          isFamily: false,
        ),
      ),
      throwsA(
        isA<BeneficiaryRegistrationFailure>().having(
          (failure) => failure.type,
          'type',
          BeneficiaryRegistrationFailureType.duplicateEmail,
        ),
      ),
    );
  });
}

ResponseBody jsonResponse(int status, Object body, {List<String>? cookies}) =>
    ResponseBody.fromString(
      jsonEncode(body),
      status,
      headers: {
        Headers.contentTypeHeader: [Headers.jsonContentType],
        ...?cookieHeader(cookies),
      },
    );

Map<String, List<String>>? cookieHeader(List<String>? cookies) =>
    cookies == null ? null : {'set-cookie': cookies};

class RecordedRequest {
  const RecordedRequest(this.path, this.headers, this.body);

  final String path;
  final Map<String, dynamic> headers;
  final Map<String, dynamic>? body;
}

class RecordingAdapter implements HttpClientAdapter {
  final requests = <RecordedRequest>[];
  ResponseBody Function(RequestOptions request)? respond;

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    final content = requestStream == null
        ? null
        : await requestStream.fold<List<int>>(
            <int>[],
            (all, next) => all..addAll(next),
          );
    final body = content == null
        ? null
        : jsonDecode(utf8.decode(content)) as Map<String, dynamic>;
    requests.add(RecordedRequest(options.path, Map.of(options.headers), body));
    if (respond case final response?) return response(options);
    if (options.path.endsWith('/antiforgery')) {
      return jsonResponse(
        200,
        {'requestToken': 'fictional-token', 'headerName': 'X-CSRF-TOKEN'},
        cookies: [
          'HH-Anti=fictional-cookie; Path=/; Secure; HttpOnly; SameSite=Strict',
        ],
      );
    }
    if (options.path.endsWith('/register/beneficiary')) {
      return jsonResponse(201, {
        'accountId': 'account-1',
        'beneficiaryId': 'beneficiary-1',
        'reference': 'BEN-00000001',
        'displayName': 'Fictional beneficiary',
        'email': 'beneficiary@example.test',
      });
    }
    return jsonResponse(404, {});
  }

  @override
  void close({bool force = false}) {}
}
