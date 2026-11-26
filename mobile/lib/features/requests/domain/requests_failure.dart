enum RequestsFailureType { sessionExpired, denied, unavailable, unexpected }

class RequestsFailure implements Exception {
  const RequestsFailure(this.type);

  final RequestsFailureType type;
}
