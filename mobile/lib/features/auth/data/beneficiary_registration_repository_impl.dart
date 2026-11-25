import 'package:dio/dio.dart';

import '../domain/beneficiary_registration.dart';
import '../domain/beneficiary_registration_failure.dart';
import '../domain/beneficiary_registration_repository.dart';
import 'beneficiary_registration_remote_data_source.dart';

class BeneficiaryRegistrationRepositoryImpl
    implements BeneficiaryRegistrationRepository {
  BeneficiaryRegistrationRepositoryImpl(this._remote);

  final BeneficiaryRegistrationRemoteDataSource _remote;

  @override
  Future<BeneficiaryRegistration> register(
    BeneficiaryRegistrationRequest request,
  ) async {
    try {
      return await _remote.register(request);
    } on DioException catch (error) {
      throw BeneficiaryRegistrationFailure(_failureType(error));
    } on TypeError {
      throw const BeneficiaryRegistrationFailure(
        BeneficiaryRegistrationFailureType.unexpected,
      );
    }
  }

  BeneficiaryRegistrationFailureType _failureType(DioException error) {
    final status = error.response?.statusCode;
    if (status == 400) {
      return _hasDuplicateEmail(error.response?.data)
          ? BeneficiaryRegistrationFailureType.duplicateEmail
          : BeneficiaryRegistrationFailureType.invalidRequest;
    }
    if (status != null && status >= 500 || _isConnectionFailure(error)) {
      return BeneficiaryRegistrationFailureType.apiUnavailable;
    }
    return BeneficiaryRegistrationFailureType.unexpected;
  }

  bool _hasDuplicateEmail(Object? data) {
    if (data is! Map<String, dynamic>) return false;
    final errors = data['errors'];
    return errors is Map && errors.containsKey('DuplicateEmail');
  }

  bool _isConnectionFailure(DioException error) => switch (error.type) {
    DioExceptionType.connectionError ||
    DioExceptionType.connectionTimeout ||
    DioExceptionType.receiveTimeout ||
    DioExceptionType.sendTimeout => true,
    _ => false,
  };
}
