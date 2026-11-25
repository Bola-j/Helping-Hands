import 'package:dio/dio.dart';

import '../domain/beneficiary_request.dart';
import '../domain/requests_repository.dart';

class RequestsRemoteDataSource {
  RequestsRemoteDataSource(this._dio);

  final Dio _dio;

  Future<RequestPage> load({String? cursor}) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/requests',
      queryParameters: {'limit': 25, ...?_cursorParameters(cursor)},
    );
    final items = (response.data ?? const [])
        .whereType<Map<String, dynamic>>()
        .map(BeneficiaryRequest.fromJson)
        .toList(growable: false);
    return RequestPage(items: items, nextCursor: _nextCursor(response));
  }

  Map<String, String>? _cursorParameters(String? cursor) =>
      cursor == null ? null : {'cursor': cursor};

  String? _nextCursor(Response<List<dynamic>> response) =>
      response.headers.value('X-Next-Cursor');
}
