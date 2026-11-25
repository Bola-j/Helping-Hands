class SessionUser {
  const SessionUser({
    required this.id,
    required this.displayName,
    required this.email,
    required this.roles,
  });

  final String id;
  final String displayName;
  final String email;
  final List<String> roles;

  factory SessionUser.fromJson(Map<String, dynamic> json) => SessionUser(
    id: json['id'] as String,
    displayName: json['displayName'] as String,
    email: json['email'] as String,
    roles: (json['roles'] as List<dynamic>).cast<String>(),
  );

  bool get isBeneficiary => roles.contains('Beneficiary');
}
