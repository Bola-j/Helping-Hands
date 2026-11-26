import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/beneficiary_request.dart';
import '../domain/single_need_submission.dart';
import '../../auth/domain/beneficiary_profile_hint.dart';

sealed class SingleNeedSubmissionEvent {
  const SingleNeedSubmissionEvent();
}

final class SingleNeedSubmitRequested extends SingleNeedSubmissionEvent {
  const SingleNeedSubmitRequested(this.submission, {this.profileHint});

  final SingleNeedSubmission submission;
  final BeneficiaryProfileHint? profileHint;
}

sealed class SingleNeedSubmissionState {
  const SingleNeedSubmissionState();
}

final class SingleNeedSubmissionInitial extends SingleNeedSubmissionState {
  const SingleNeedSubmissionInitial();
}

final class SingleNeedSubmissionSubmitting extends SingleNeedSubmissionState {
  const SingleNeedSubmissionSubmitting();
}

final class SingleNeedSubmissionSucceeded extends SingleNeedSubmissionState {
  const SingleNeedSubmissionSucceeded(this.request);

  final BeneficiaryRequest request;
}

final class SingleNeedSubmissionFailed extends SingleNeedSubmissionState {
  const SingleNeedSubmissionFailed(this.failure);

  final SingleNeedFailureType failure;
}

class SingleNeedSubmissionBloc
    extends Bloc<SingleNeedSubmissionEvent, SingleNeedSubmissionState> {
  SingleNeedSubmissionBloc(this._repository)
    : super(const SingleNeedSubmissionInitial()) {
    on<SingleNeedSubmitRequested>(_submit);
  }

  final SingleNeedSubmissionRepository _repository;
  bool _busy = false;

  Future<void> _submit(
    SingleNeedSubmitRequested event,
    Emitter<SingleNeedSubmissionState> emit,
  ) async {
    if (_busy) return;
    _busy = true;
    emit(const SingleNeedSubmissionSubmitting());
    try {
      emit(
        SingleNeedSubmissionSucceeded(
          await _repository.submit(
            event.submission,
            profileHint: event.profileHint,
          ),
        ),
      );
    } on SingleNeedFailure catch (failure) {
      emit(SingleNeedSubmissionFailed(failure.type));
    } catch (_) {
      emit(const SingleNeedSubmissionFailed(SingleNeedFailureType.unexpected));
    } finally {
      _busy = false;
    }
  }
}
