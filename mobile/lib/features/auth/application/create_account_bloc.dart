import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/beneficiary_registration.dart';
import '../domain/beneficiary_registration_failure.dart';
import '../domain/beneficiary_registration_repository.dart';

sealed class CreateAccountEvent {}

final class CreateAccountSubmitted extends CreateAccountEvent {
  CreateAccountSubmitted(this.request);

  final BeneficiaryRegistrationRequest request;
}

sealed class CreateAccountState {}

final class CreateAccountInitial extends CreateAccountState {}

final class CreateAccountSubmitting extends CreateAccountState {}

final class CreateAccountFailureState extends CreateAccountState {
  CreateAccountFailureState(this.type);

  final BeneficiaryRegistrationFailureType type;
}

final class CreateAccountSuccess extends CreateAccountState {
  CreateAccountSuccess(this.registration);

  final BeneficiaryRegistration registration;
}

class CreateAccountBloc extends Bloc<CreateAccountEvent, CreateAccountState> {
  CreateAccountBloc(this._repository) : super(CreateAccountInitial()) {
    on<CreateAccountSubmitted>(_submit);
  }

  final BeneficiaryRegistrationRepository _repository;
  bool _isSubmitting = false;

  Future<void> _submit(
    CreateAccountSubmitted event,
    Emitter<CreateAccountState> emit,
  ) async {
    if (_isSubmitting) return;
    _isSubmitting = true;
    emit(CreateAccountSubmitting());
    try {
      final registration = await _repository.register(event.request);
      emit(CreateAccountSuccess(registration));
    } on BeneficiaryRegistrationFailure catch (failure) {
      emit(CreateAccountFailureState(failure.type));
    } catch (_) {
      emit(
        CreateAccountFailureState(
          BeneficiaryRegistrationFailureType.unexpected,
        ),
      );
    } finally {
      _isSubmitting = false;
    }
  }
}
