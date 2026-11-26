import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/domain/session_repository.dart';
import 'package:mobile/features/auth/domain/session_user.dart';
import 'package:mobile/features/auth/domain/beneficiary_profile_hint.dart';
import 'package:mobile/features/requests/data/single_need_remote_data_source.dart';
import 'package:mobile/features/requests/data/single_need_submission_repository_impl.dart';
import 'package:mobile/features/requests/domain/single_need_submission.dart';

void main() {
  late Dio dio;
  late ApiAdapter adapter;
  late FakeSession session;
  late SingleNeedSubmissionRepositoryImpl repository;

  setUp(() {
    adapter = ApiAdapter();
    dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = adapter;
    session = FakeSession();
    repository = SingleNeedSubmissionRepositoryImpl(
      SingleNeedRemoteDataSource(dio),
      session,
    );
  });

  tearDown(() => dio.close(force: true));

  test(
    'uses the linked beneficiary projection, CSRF token, and exact DTO',
    () async {
      adapter.respond = (request) => switch (request.path) {
        '/api/v1/beneficiaries' => jsonResponse(200, [
          {'id': 'fictional-beneficiary', 'displayName': 'Demo Beneficiary'},
        ]),
        '/api/v1/auth/antiforgery' => jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        }),
        '/api/v1/requests/single-need' => jsonResponse(201, requestView),
        _ => jsonResponse(404, {}),
      };

      final request = await repository.submit(validSubmission);

      expect(request.reference, 'REQ-2026-0042');
      expect(request.status, 'Submitted');
      final post = adapter.requests.last;
      expect(post.path, '/api/v1/requests/single-need');
      expect(post.headers['X-CSRF-TOKEN'], 'fictional-token');
      expect(post.body, {
        'beneficiaryId': 'fictional-beneficiary',
        'description': 'Fictional food support',
        'authorityConfirmed': true,
        'sharingAgreed': true,
      });
      expect(adapter.requests.map((request) => request.path), [
        '/api/v1/beneficiaries',
        '/api/v1/auth/antiforgery',
        '/api/v1/requests/single-need',
      ]);
    },
  );

  test(
    'uses signup beneficiary ID only for the matching signed-in account',
    () async {
      adapter.respond = (request) => switch (request.path) {
        '/api/v1/auth/antiforgery' => jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        }),
        '/api/v1/requests/single-need' => jsonResponse(201, requestView),
        _ => jsonResponse(404, {}),
      };

      await repository.submit(
        validSubmission,
        profileHint: const BeneficiaryProfileHint(
          accountId: 'fictional-user',
          beneficiaryId: 'registered-beneficiary-id',
        ),
      );

      expect(adapter.requests.map((request) => request.path), [
        '/api/v1/auth/antiforgery',
        '/api/v1/requests/single-need',
      ]);
      expect(
        adapter.requests.last.body['beneficiaryId'],
        'registered-beneficiary-id',
      );
    },
  );

  test(
    'does not use a registration hint after a different account signs in',
    () async {
      adapter.respond = (request) => switch (request.path) {
        '/api/v1/beneficiaries' => jsonResponse(200, [
          {'id': 'current-account-profile', 'displayName': 'Demo Beneficiary'},
        ]),
        '/api/v1/auth/antiforgery' => jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        }),
        '/api/v1/requests/single-need' => jsonResponse(201, requestView),
        _ => jsonResponse(404, {}),
      };

      await repository.submit(
        validSubmission,
        profileHint: const BeneficiaryProfileHint(
          accountId: 'another-account',
          beneficiaryId: 'stale-registration-profile',
        ),
      );

      expect(
        adapter.requests.last.body['beneficiaryId'],
        'current-account-profile',
      );
    },
  );

  test('does not submit if beneficiary projection is ambiguous', () async {
    adapter.respond = (request) => request.path == '/api/v1/beneficiaries'
        ? jsonResponse(200, [
            {'id': 'one', 'displayName': 'Demo Beneficiary'},
            {'id': 'two', 'displayName': 'Demo Beneficiary'},
          ])
        : jsonResponse(200, {'id': 'fictional-user'});

    await expectSubmissionFailure(
      repository,
      SingleNeedFailureType.profileUnavailable,
    );
    expect(
      adapter.requests.any((request) => request.path.endsWith('single-need')),
      isFalse,
    );
  });

  test('rejects missing confirmations before making API calls', () async {
    await expectSubmissionFailure(
      repository,
      SingleNeedFailureType.invalidInput,
      submission: const SingleNeedSubmission(
        description: 'Fictional need',
        authorityConfirmed: true,
        sharingAgreed: false,
      ),
    );
    expect(adapter.requests, isEmpty);
  });

  test('maps denied and conflict responses without success', () async {
    for (final scenario in [
      (403, SingleNeedFailureType.denied),
      (409, SingleNeedFailureType.conflict),
    ]) {
      adapter.respond = (request) => switch (request.path) {
        '/api/v1/beneficiaries' => jsonResponse(200, [
          {'id': 'fictional-beneficiary', 'displayName': 'Demo Beneficiary'},
        ]),
        '/api/v1/auth/antiforgery' => jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        }),
        '/api/v1/requests/single-need' => jsonResponse(scenario.$1, {}),
        _ => jsonResponse(200, {'id': 'fictional-user'}),
      };
      await expectSubmissionFailure(repository, scenario.$2);
    }
  });

  test(
    'maps expired sessions and validation rejection without success',
    () async {
      adapter.respond = (request) => request.path == '/api/v1/beneficiaries'
          ? jsonResponse(401, {})
          : jsonResponse(200, {'id': 'fictional-user'});
      await expectSubmissionFailure(
        repository,
        SingleNeedFailureType.sessionExpired,
      );
      expect(
        adapter.requests.any((request) => request.path.endsWith('single-need')),
        isFalse,
      );

      adapter.requests.clear();
      adapter.respond = (request) => switch (request.path) {
        '/api/v1/beneficiaries' => jsonResponse(200, [
          {'id': 'fictional-beneficiary', 'displayName': 'Demo Beneficiary'},
        ]),
        '/api/v1/auth/antiforgery' => jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        }),
        '/api/v1/requests/single-need' => jsonResponse(400, {}),
        _ => jsonResponse(200, {'id': 'fictional-user'}),
      };
      await expectSubmissionFailure(
        repository,
        SingleNeedFailureType.invalidInput,
      );
    },
  );

  test('treats a write timeout as unconfirmed and never retries it', () async {
    adapter.respond = (request) {
      if (request.path == '/api/v1/beneficiaries') {
        return jsonResponse(200, [
          {'id': 'fictional-beneficiary', 'displayName': 'Demo Beneficiary'},
        ]);
      }
      if (request.path == '/api/v1/auth/antiforgery') {
        return jsonResponse(200, {
          'requestToken': 'fictional-token',
          'headerName': 'X-CSRF-TOKEN',
        });
      }
      throw DioException.receiveTimeout(
        requestOptions: RequestOptions(path: request.path),
        timeout: const Duration(seconds: 1),
      );
    };

    await expectSubmissionFailure(
      repository,
      SingleNeedFailureType.outcomeUnconfirmed,
    );
    expect(
      adapter.requests.where((request) => request.path.endsWith('single-need')),
      hasLength(1),
    );
  });

  test('preflight outage is unavailable and does not send the write', () async {
    adapter.respond = (request) {
      if (request.path == '/api/v1/beneficiaries') {
        throw DioException.connectionError(
          requestOptions: RequestOptions(path: request.path),
          reason: 'fictional offline network',
        );
      }
      return jsonResponse(200, {'id': 'fictional-user'});
    };

    await expectSubmissionFailure(
      repository,
      SingleNeedFailureType.unavailable,
    );
    expect(
      adapter.requests.any((request) => request.path.endsWith('single-need')),
      isFalse,
    );
  });

  test('treats server failure after write as unconfirmed', () async {
    adapter.respond = (request) => switch (request.path) {
      '/api/v1/beneficiaries' => jsonResponse(200, [
        {'id': 'fictional-beneficiary', 'displayName': 'Demo Beneficiary'},
      ]),
      '/api/v1/auth/antiforgery' => jsonResponse(200, {
        'requestToken': 'fictional-token',
        'headerName': 'X-CSRF-TOKEN',
      }),
      '/api/v1/requests/single-need' => jsonResponse(503, {}),
      _ => jsonResponse(200, {'id': 'fictional-user'}),
    };

    await expectSubmissionFailure(
      repository,
      SingleNeedFailureType.outcomeUnconfirmed,
    );
    expect(
      adapter.requests.where((request) => request.path.endsWith('single-need')),
      hasLength(1),
    );
  });
}

Future<void> expectSubmissionFailure(
  SingleNeedSubmissionRepositoryImpl repository,
  SingleNeedFailureType type, {
  SingleNeedSubmission submission = validSubmission,
}) async {
  try {
    await repository.submit(submission);
    fail('Expected $type');
  } on SingleNeedFailure catch (failure) {
    expect(failure.type, type);
  }
}

const validSubmission = SingleNeedSubmission(
  description: 'Fictional food support',
  authorityConfirmed: true,
  sharingAgreed: true,
);

const requestView = {
  'id': 'fictional-request-id',
  'reference': 'REQ-2026-0042',
  'isMultiNeed': false,
  'description': 'Fictional food support',
  'status': 'Submitted',
  'createdAt': '2026-11-25T10:00:00Z',
};

class FakeSession implements SessionRepository {
  @override
  Future<SessionUser> currentUser() async => const SessionUser(
    id: 'fictional-user',
    displayName: 'Demo Beneficiary',
    email: 'beneficiary@example.test',
    roles: ['Beneficiary'],
  );

  @override
  Future<void> registerBeneficiary(Map<String, Object?> input) async {}

  @override
  Future<SessionUser> signIn(Map<String, Object?> credentials) => currentUser();

  @override
  Future<void> signOut() async {}
}

class RecordedRequest {
  const RecordedRequest(this.path, this.body, this.headers);

  final String path;
  final dynamic body;
  final Map<String, dynamic> headers;
}

class ApiAdapter implements HttpClientAdapter {
  final requests = <RecordedRequest>[];
  ResponseBody Function(RequestOptions)? respond;

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    final body = options.data;
    requests.add(
      RecordedRequest(
        options.path,
        body is String ? jsonDecode(body) : body,
        Map.of(options.headers),
      ),
    );
    return respond?.call(options) ??
        jsonResponse(200, {'id': 'fictional-user'});
  }

  @override
  void close({bool force = false}) {}
}

ResponseBody jsonResponse(int status, Object data) => ResponseBody.fromString(
  jsonEncode(data),
  status,
  headers: {
    Headers.contentTypeHeader: [Headers.jsonContentType],
  },
);
