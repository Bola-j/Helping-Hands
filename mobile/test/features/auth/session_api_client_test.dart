import 'dart:convert';
import 'dart:typed_data';

import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/data/session_remote_data_source.dart';
import 'package:mobile/features/auth/data/session_repository_impl.dart';
import 'package:mobile/features/auth/domain/session_failure.dart';

void main() {
  late CookieJar jar;
  late RecordingAdapter adapter;
  late Dio dio;
  late SessionRepositoryImpl repository;

  setUp(() {
    jar = CookieJar();
    adapter = RecordingAdapter();
    dio =
        Dio(
            BaseOptions(
              baseUrl: 'https://api.example.test',
              responseType: ResponseType.json,
            ),
          )
          ..httpClientAdapter = adapter
          ..interceptors.add(CookieManager(jar));
    repository = SessionRepositoryImpl(SessionRemoteDataSource(dio, jar));
  });

  tearDown(() => dio.close(force: true));

  test(
    'retains antiforgery and auth cookies across login, me and logout',
    () async {
      final user = await repository.signIn({
        'email': 'beneficiary@example.test',
        'password': 'fictional-password',
      });

      expect(user.id, 'user-1');
      expect(adapter.requests.map((request) => request.path), [
        '/api/v1/auth/antiforgery',
        '/api/v1/auth/login',
        '/api/v1/auth/antiforgery',
        '/api/v1/auth/me',
      ]);
      final login = adapter.requests[1];
      expect(login.headers['X-CSRF-TOKEN'], 'request-token-1');
      expect(login.headers['cookie'], contains('HH-Anti=anti-cookie'));
      expect(
        adapter.requests[3].headers['cookie'],
        contains('HH-Anti=anti-cookie'),
      );
      expect(
        adapter.requests[3].headers['cookie'],
        contains('.AspNetCore.Identity.Application=auth-cookie'),
      );

      await repository.signOut();
      final logout = adapter.requests[4];
      expect(logout.path, '/api/v1/auth/logout');
      expect(logout.headers['X-CSRF-TOKEN'], 'request-token-2');
      expect(
        logout.headers['cookie'],
        contains('.AspNetCore.Identity.Application=auth-cookie'),
      );
      expect(adapter.requests.last.path, '/api/v1/auth/antiforgery');
      final cookies = await jar.loadForRequest(
        Uri.parse('https://api.example.test/api/v1/auth/me'),
      );
      expect(cookies.map((cookie) => cookie.name), contains('HH-Anti'));
      expect(
        cookies.map((cookie) => cookie.name),
        isNot(contains('.AspNetCore.Identity.Application')),
      );
    },
  );

  test(
    'maps unauthorized, denied and invalid antiforgery responses distinctly',
    () async {
      adapter.respond = (request) {
        if (request.path.endsWith('/antiforgery')) {
          return jsonResponse(200, {'requestToken': 'token'});
        }
        if (request.path.endsWith('/me')) {
          return jsonResponse(401, {});
        }
        if (request.path.endsWith('/login')) {
          return jsonResponse(400, {'title': 'Invalid antiforgery token'});
        }
        return jsonResponse(403, {});
      };

      await expectFailure(
        repository.currentUser(),
        SessionFailureType.unauthenticated,
      );
      await expectFailure(
        repository.signIn({
          'email': 'a@example.test',
          'password': 'never-logged',
        }),
        SessionFailureType.invalidRequest,
      );
    },
  );

  test(
    'registration is a separate antiforgery-protected action and stays signed out',
    () async {
      await repository.registerBeneficiary({
        'displayName': 'Demo Beneficiary',
        'email': 'beneficiary@example.test',
        'password': 'fictional-password',
      });

      expect(adapter.requests.map((request) => request.path), [
        '/api/v1/auth/antiforgery',
        '/api/v1/auth/register/beneficiary',
        '/api/v1/auth/antiforgery',
      ]);
      expect(adapter.requests[1].headers['X-CSRF-TOKEN'], 'request-token-1');
      expect(
        adapter.requests[1].headers['cookie'],
        contains('HH-Anti=anti-cookie'),
      );
      expect(
        await jar.loadForRequest(
          Uri.parse('https://api.example.test/api/v1/auth/me'),
        ),
        isNot(
          contains(
            predicate<Cookie>(
              (cookie) => cookie.name == '.AspNetCore.Identity.Application',
            ),
          ),
        ),
      );
    },
  );

  test('maps forbidden and unavailable API responses', () async {
    adapter.respond = (request) => jsonResponse(403, {});
    await expectFailure(repository.currentUser(), SessionFailureType.denied);

    adapter.respond = (request) => throw DioException.connectionError(
      requestOptions: request,
      reason: 'simulated unavailable API',
    );
    await expectFailure(
      repository.currentUser(),
      SessionFailureType.apiUnavailable,
    );
  });

  test(
    'maps sign-in rejection and a previously authenticated 401 separately',
    () async {
      adapter.respond = (request) {
        if (request.path.endsWith('/antiforgery')) {
          return jsonResponse(200, {'requestToken': 'request-token'});
        }
        if (request.path.endsWith('/login')) return jsonResponse(401, {});
        return jsonResponse(403, {});
      };
      await expectFailure(
        repository.signIn({
          'email': 'bad@example.test',
          'password': 'transient',
        }),
        SessionFailureType.invalidCredentials,
      );

      adapter.respond = null;
      await repository.signIn({
        'email': 'beneficiary@example.test',
        'password': 'transient',
      });
      adapter.respond = (request) => jsonResponse(401, {});
      await expectFailure(
        repository.currentUser(),
        SessionFailureType.sessionExpired,
      );
      expect(
        await jar.loadForRequest(
          Uri.parse('https://api.example.test/api/v1/auth/me'),
        ),
        isNot(
          contains(
            predicate<Cookie>(
              (cookie) => cookie.name == '.AspNetCore.Identity.Application',
            ),
          ),
        ),
      );
    },
  );

  test('rejects a signed-in non-beneficiary and clears that session', () async {
    adapter.respond = (request) {
      if (request.path.endsWith('/me')) {
        return jsonResponse(200, {
          'id': 'staff-user',
          'displayName': 'Fictional Staff',
          'email': 'staff@example.test',
          'roles': ['Coordinator'],
        });
      }
      return nullResponse(request);
    };

    await expectFailure(
      repository.signIn({
        'email': 'staff@example.test',
        'password': 'fictional-password',
      }),
      SessionFailureType.denied,
    );
    expect(
      adapter.requests.map((request) => request.path),
      contains('/api/v1/auth/logout'),
    );
    expect(
      await jar.loadForRequest(
        Uri.parse('https://api.example.test/api/v1/auth/me'),
      ),
      isNot(
        contains(
          predicate<Cookie>(
            (cookie) => cookie.name == '.AspNetCore.Identity.Application',
          ),
        ),
      ),
    );
  });

  test('clears local cookies when remote sign-out is unavailable', () async {
    await repository.signIn({
      'email': 'beneficiary@example.test',
      'password': 'fictional-password',
    });
    adapter.respond = (request) {
      if (request.path.endsWith('/logout')) return jsonResponse(503, {});
      if (request.path.endsWith('/me')) return jsonResponse(401, {});
      return nullResponse(request);
    };

    await expectFailure(
      repository.signOut(),
      SessionFailureType.apiUnavailable,
    );
    expect(
      await jar.loadForRequest(
        Uri.parse('https://api.example.test/api/v1/auth/me'),
      ),
      isEmpty,
    );
    await expectFailure(
      repository.currentUser(),
      SessionFailureType.unauthenticated,
    );
  });
}

Future<void> expectFailure(
  Future<Object?> action,
  SessionFailureType type,
) async {
  await expectLater(
    action,
    throwsA(isA<SessionFailure>().having((error) => error.type, 'type', type)),
  );
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
  const RecordedRequest(this.path, this.headers);

  final String path;
  final Map<String, dynamic> headers;
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
    requests.add(RecordedRequest(options.path, Map.of(options.headers)));
    if (respond case final response?) return response(options);
    if (options.path.endsWith('/antiforgery')) {
      return jsonResponse(
        200,
        {
          'requestToken':
              'request-token-${requests.where((request) => request.path.endsWith('/antiforgery')).length}',
          'headerName': 'X-CSRF-TOKEN',
        },
        cookies: [
          'HH-Anti=anti-cookie; Path=/; Secure; HttpOnly; SameSite=Strict',
        ],
      );
    }
    if (options.path.endsWith('/login')) {
      return ResponseBody.fromString(
        '',
        204,
        headers: {
          'set-cookie': [
            '.AspNetCore.Identity.Application=auth-cookie; Path=/; Secure; HttpOnly; SameSite=Strict',
          ],
        },
      );
    }
    if (options.path.endsWith('/register/beneficiary')) {
      return jsonResponse(201, {'userId': 'user-1'});
    }
    if (options.path.endsWith('/me')) {
      return jsonResponse(200, {
        'id': 'user-1',
        'displayName': 'Demo Beneficiary',
        'email': 'beneficiary@example.test',
        'roles': ['Beneficiary'],
      });
    }
    if (options.path.endsWith('/logout')) {
      return ResponseBody.fromString(
        '',
        204,
        headers: {
          'set-cookie': [
            '.AspNetCore.Identity.Application=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; Secure; HttpOnly; SameSite=Strict',
          ],
        },
      );
    }
    return jsonResponse(404, {});
  }

  @override
  void close({bool force = false}) {}
}

ResponseBody nullResponse(RequestOptions request) {
  if (request.path.endsWith('/antiforgery')) {
    return jsonResponse(200, {'requestToken': 'token'});
  }
  if (request.path.endsWith('/login') || request.path.endsWith('/logout')) {
    return ResponseBody.fromString('', 204);
  }
  if (request.path.endsWith('/me')) {
    return jsonResponse(200, {
      'id': 'user-1',
      'displayName': 'Demo Beneficiary',
      'email': 'beneficiary@example.test',
      'roles': ['Beneficiary'],
    });
  }
  return jsonResponse(404, {});
}
