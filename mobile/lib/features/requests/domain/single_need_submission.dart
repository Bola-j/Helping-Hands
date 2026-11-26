import 'beneficiary_request.dart';
import '../../auth/domain/beneficiary_profile_hint.dart';

class SingleNeedSubmission {
  const SingleNeedSubmission({
    required this.description,
    required this.authorityConfirmed,
    required this.sharingAgreed,
  });

  final String description;
  final bool authorityConfirmed;
  final bool sharingAgreed;

  Map<String, Object?> toJson(String beneficiaryId) => {
    'beneficiaryId': beneficiaryId,
    'description': description.trim(),
    'authorityConfirmed': authorityConfirmed,
    'sharingAgreed': sharingAgreed,
  };
}

abstract interface class SingleNeedSubmissionRepository {
  Future<BeneficiaryRequest> submit(
    SingleNeedSubmission submission, {
    BeneficiaryProfileHint? profileHint,
  });
}

enum SingleNeedFailureType {
  invalidInput,
  sessionExpired,
  denied,
  profileUnavailable,
  conflict,
  outcomeUnconfirmed,
  unavailable,
  unexpected,
}

class SingleNeedFailure implements Exception {
  const SingleNeedFailure(this.type);

  final SingleNeedFailureType type;
}
