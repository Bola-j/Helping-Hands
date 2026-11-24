import 'package:dio/dio.dart';

abstract final class DioFactory {
  static Dio create() {
    const apiBaseUrl = String.fromEnvironment('API_BASE_URL');
    return Dio(
      BaseOptions(
        baseUrl: apiBaseUrl,
        connectTimeout: const Duration(seconds: 15),
        receiveTimeout: const Duration(seconds: 15),
        responseType: ResponseType.json,
        headers: const {'Accept': 'application/json'},
      ),
    );
  }
}
