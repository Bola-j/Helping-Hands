import 'beneficiary_registration.dart';

abstract interface class BeneficiaryRegistrationRepository {
  Future<BeneficiaryRegistration> register(
    BeneficiaryRegistrationRequest request,
  );
}
