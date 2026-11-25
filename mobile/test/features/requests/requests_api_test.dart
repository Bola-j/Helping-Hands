import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/domain/session_failure.dart';
import 'package:mobile/features/auth/domain/session_repository.dart';
import 'package:mobile/features/auth/domain/session_user.dart';
import 'package:mobile/features/requests/data/requests_remote_data_source.dart';
import 'package:mobile/features/requests/data/requests_repository_impl.dart';
import 'package:mobile/features/requests/domain/requests_failure.dart';

void main() {
  late Dio dio;
  late Adapter adapter;
  late FakeSession session;
  late RequestsRepositoryImpl repository;
  setUp(() {
    adapter = Adapter();
    dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = adapter;
    session = FakeSession();
    repository = RequestsRepositoryImpl(RequestsRemoteDataSource(dio), session);
  });
  tearDown(() => dio.close(force: true));

  Future<void> expectFailure(RequestsFailureType expected) async {
    try {
      await repository.load();
      fail('Expected $expected');
    } on RequestsFailure catch (failure) {
      expect(failure.type, expected);
    }
  }

  test('parses verified array response and X-Next-Cursor', () async {
    adapter.respond = (_) => jsonResponse(
      200,
      [
        {
          'id': 'fictional-id',
          'reference': 'REQ-2026-0042',
          'isMultiNeed': false,
          'category': 'Goods',
          'serviceSubtype': null,
          'description': 'Fictional support',
          'requiredQuantity': 2,
          'unit': 'items',
          'status': 'Submitted',
          'createdAt': '2026-11-18T12:00:00Z',
          'version': 1,
          'needs': [],
        },
      ],
      headers: {
        'X-Next-Cursor': ['opaque-cursor'],
      },
    );
    final page = await repository.load(cursor: 'prior-cursor');
    expect(page.items.single.reference, 'REQ-2026-0042');
    expect(page.items.single.requiredQuantity, 2);
    expect(page.nextCursor, 'opaque-cursor');
    expect(adapter.requests.single.path, '/api/v1/requests');
    expect(adapter.requests.single.query, {
      'limit': 25,
      'cursor': 'prior-cursor',
    });
  });

  test(
    'classifies unauthorized request as expiry and invokes session cleanup',
    () async {
      adapter.respond = (_) => jsonResponse(401, {});
      session.failure = const SessionFailure(SessionFailureType.sessionExpired);
      await expectFailure(RequestsFailureType.sessionExpired);
      expect(session.currentCalls, 1);
    },
  );

  test('keeps forbidden distinct from expiry', () async {
    adapter.respond = (_) => jsonResponse(403, {});
    await expectFailure(RequestsFailureType.denied);
    expect(session.currentCalls, 0);
  });

  test('maps server and connection failures to unavailable', () async {
    adapter.respond = (_) => jsonResponse(503, {});
    await expectFailure(RequestsFailureType.unavailable);
    adapter.respond = (request) => throw DioException.connectionError(
      requestOptions: request,
      reason: 'fictional outage',
    );
    await expectFailure(RequestsFailureType.unavailable);
  });
}

class FakeSession implements SessionRepository {
  SessionFailure? failure;
  int currentCalls = 0;
  @override
  Future<SessionUser> currentUser() async {
    currentCalls++;
    if (failure case final error?) throw error;
    return const SessionUser(
      id: 'fictional',
      displayName: 'Demo Beneficiary',
      email: 'beneficiary@example.test',
      roles: ['Beneficiary'],
    );
  }

  @override
  Future<void> registerBeneficiary(Map<String, Object?> input) async {}
  @override
  Future<SessionUser> signIn(Map<String, Object?> credentials) => currentUser();
  @override
  Future<void> signOut() async {}
}

class RecordedRequest {
  const RecordedRequest(this.path, this.query);
  final String path;
  final Map<String, dynamic> query;
}

class Adapter implements HttpClientAdapter {
  final requests = <RecordedRequest>[];
  ResponseBody Function(RequestOptions)? respond;
  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    requests.add(
      RecordedRequest(options.path, Map.of(options.queryParameters)),
    );
    return respond?.call(options) ?? jsonResponse(200, []);
  }

  @override
  void close({bool force = false}) {}
}

ResponseBody jsonResponse(
  int status,
  Object data, {
  Map<String, List<String>> headers = const {},
}) => ResponseBody.fromString(
  jsonEncode(data),
  status,
  headers: {
    Headers.contentTypeHeader: [Headers.jsonContentType],
    ...headers,
  },
);
