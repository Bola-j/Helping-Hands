enum SessionFailureType {
  unauthenticated,
  invalidCredentials,
  sessionExpired,
  denied,
  apiUnavailable,
  invalidRequest,
  unexpected,
}

class SessionFailure implements Exception {
  const SessionFailure(this.type);

  final SessionFailureType type;
}
