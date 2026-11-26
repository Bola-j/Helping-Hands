import 'beneficiary_registration.dart';
import 'session_user.dart';

class BeneficiaryProfileHint {
  const BeneficiaryProfileHint({
    required this.accountId,
    required this.beneficiaryId,
  });

  final String accountId;
  final String beneficiaryId;

  factory BeneficiaryProfileHint.fromRegistration(
    BeneficiaryRegistration registration,
  ) => BeneficiaryProfileHint(
    accountId: registration.accountId,
    beneficiaryId: registration.beneficiaryId,
  );

  bool belongsTo(SessionUser user) => user.id == accountId;
}
