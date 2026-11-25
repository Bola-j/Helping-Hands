enum BeneficiaryRegistrationFailureType {
  duplicateEmail,
  invalidRequest,
  apiUnavailable,
  unexpected,
}

class BeneficiaryRegistrationFailure implements Exception {
  const BeneficiaryRegistrationFailure(this.type);

  final BeneficiaryRegistrationFailureType type;
}
