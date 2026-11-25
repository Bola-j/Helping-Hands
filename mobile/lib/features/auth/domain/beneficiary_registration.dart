class BeneficiaryRegistration {
  const BeneficiaryRegistration({
    required this.accountId,
    required this.beneficiaryId,
    required this.reference,
    required this.displayName,
    required this.email,
  });

  final String accountId;
  final String beneficiaryId;
  final String reference;
  final String displayName;
  final String email;

  factory BeneficiaryRegistration.fromJson(Map<String, dynamic> json) =>
      BeneficiaryRegistration(
        accountId: json['accountId'] as String,
        beneficiaryId: json['beneficiaryId'] as String,
        reference: json['reference'] as String,
        displayName: json['displayName'] as String,
        email: json['email'] as String,
      );
}

class BeneficiaryRegistrationRequest {
  const BeneficiaryRegistrationRequest({
    required this.displayName,
    required this.email,
    required this.password,
    required this.isFamily,
    this.contactDetails,
  });

  final String displayName;
  final String email;
  final String password;
  final String? contactDetails;
  final bool isFamily;

  Map<String, Object?> toJson() => {
    'displayName': displayName,
    'email': email,
    'password': password,
    'contactDetails': contactDetails,
    'isFamily': isFamily,
  };
}
