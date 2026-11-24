import 'package:dio/dio.dart';
import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';

abstract final class DioFactory {
  static Dio create({CookieJar? cookieJar}) {
    const configuredBaseUrl = String.fromEnvironment('API_BASE_URL');
    final apiBaseUrl = configuredBaseUrl.isEmpty
        ? 'https://localhost:5087'
        : configuredBaseUrl;
    final dio = Dio(
      BaseOptions(
        baseUrl: apiBaseUrl,
        connectTimeout: const Duration(seconds: 15),
        receiveTimeout: const Duration(seconds: 15),
        responseType: ResponseType.json,
        headers: const {'Accept': 'application/json'},
      ),
    );
    dio.interceptors.add(CookieManager(cookieJar ?? CookieJar()));
    return dio;
  }
}
