import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/beneficiary_request.dart';
import '../domain/requests_failure.dart';
import '../domain/requests_repository.dart';

sealed class RequestsEvent {
  const RequestsEvent();
}

final class RequestsLoadRequested extends RequestsEvent {
  const RequestsLoadRequested();
}

final class RequestsLoadMoreRequested extends RequestsEvent {
  const RequestsLoadMoreRequested();
}

final class RequestsRefreshRequested extends RequestsEvent {
  const RequestsRefreshRequested();
}

sealed class RequestsState {
  const RequestsState();
}

final class RequestsInitial extends RequestsState {
  const RequestsInitial();
}

final class RequestsLoading extends RequestsState {
  const RequestsLoading();
}

final class RequestsLoaded extends RequestsState {
  const RequestsLoaded(
    this.items, {
    this.nextCursor,
    this.isLoadingMore = false,
  });

  final List<BeneficiaryRequest> items;
  final String? nextCursor;
  final bool isLoadingMore;
}

final class RequestsFailed extends RequestsState {
  const RequestsFailed(this.failure);

  final RequestsFailureType failure;
}

class RequestsBloc extends Bloc<RequestsEvent, RequestsState> {
  RequestsBloc(this._repository) : super(const RequestsInitial()) {
    on<RequestsLoadRequested>(_load);
    on<RequestsRefreshRequested>(_refresh);
    on<RequestsLoadMoreRequested>(_loadMore);
  }

  final RequestsRepository _repository;

  Future<void> _load(RequestsEvent event, Emitter<RequestsState> emit) async {
    emit(const RequestsLoading());
    try {
      final page = await _repository.load();
      emit(RequestsLoaded(page.items, nextCursor: page.nextCursor));
    } on RequestsFailure catch (failure) {
      emit(RequestsFailed(failure.type));
    } catch (_) {
      emit(const RequestsFailed(RequestsFailureType.unexpected));
    }
  }

  Future<void> _refresh(RequestsEvent event, Emitter<RequestsState> emit) =>
      _load(event, emit);

  Future<void> _loadMore(
    RequestsEvent event,
    Emitter<RequestsState> emit,
  ) async {
    final current = state;
    if (current is! RequestsLoaded || current.nextCursor == null) return;
    emit(
      RequestsLoaded(
        current.items,
        nextCursor: current.nextCursor,
        isLoadingMore: true,
      ),
    );
    try {
      final page = await _repository.load(cursor: current.nextCursor);
      emit(
        RequestsLoaded([
          ...current.items,
          ...page.items,
        ], nextCursor: page.nextCursor),
      );
    } on RequestsFailure catch (failure) {
      emit(RequestsFailed(failure.type));
    } catch (_) {
      emit(const RequestsFailed(RequestsFailureType.unexpected));
    }
  }
}
