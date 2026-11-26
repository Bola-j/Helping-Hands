import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/requests/application/requests_bloc.dart';
import 'package:mobile/features/requests/domain/beneficiary_request.dart';
import 'package:mobile/features/requests/domain/requests_failure.dart';
import 'package:mobile/features/requests/domain/requests_repository.dart';

void main() {
  late FakeRepository repository;
  late RequestsBloc bloc;
  setUp(() {
    repository = FakeRepository();
    bloc = RequestsBloc(repository);
  });
  tearDown(() => bloc.close());

  test('loads request projections and cursor', () async {
    repository.page = RequestPage(items: [request], nextCursor: 'opaque');
    bloc.add(const RequestsLoadRequested());
    final state =
        await bloc.stream.firstWhere((value) => value is RequestsLoaded)
            as RequestsLoaded;
    expect(state.items.single.reference, request.reference);
    expect(state.nextCursor, 'opaque');
  });

  test('represents a successful empty response distinctly', () async {
    repository.page = const RequestPage(items: []);
    bloc.add(const RequestsLoadRequested());
    final state =
        await bloc.stream.firstWhere((value) => value is RequestsLoaded)
            as RequestsLoaded;
    expect(state.items, isEmpty);
  });

  for (final failure in RequestsFailureType.values) {
    test('preserves $failure as failure, not empty', () async {
      repository.failure = RequestsFailure(failure);
      bloc.add(const RequestsLoadRequested());
      final state =
          await bloc.stream.firstWhere((value) => value is RequestsFailed)
              as RequestsFailed;
      expect(state.failure, failure);
    });
  }

  test('appends cursor page', () async {
    repository.page = RequestPage(items: [request], nextCursor: 'opaque');
    bloc.add(const RequestsLoadRequested());
    await bloc.stream.firstWhere((value) => value is RequestsLoaded);
    repository.page = const RequestPage(items: [secondRequest]);
    bloc.add(const RequestsLoadMoreRequested());
    final state =
        await bloc.stream.firstWhere(
              (value) => value is RequestsLoaded && value.items.length == 2,
            )
            as RequestsLoaded;
    expect(state.items.last.reference, secondRequest.reference);
    expect(repository.cursors, [null, 'opaque']);
  });
}

const request = BeneficiaryRequest(
  id: 'fictional-42',
  reference: 'REQ-2026-0042',
  isMultiNeed: false,
  description: 'Fictional food support',
  status: 'Submitted',
  createdAt: null,
);
const secondRequest = BeneficiaryRequest(
  id: 'fictional-43',
  reference: 'REQ-2026-0043',
  isMultiNeed: false,
  description: 'Fictional medicine support',
  status: 'InReview',
  createdAt: null,
);

class FakeRepository implements RequestsRepository {
  RequestPage page = const RequestPage(items: [request]);
  RequestsFailure? failure;
  final cursors = <String?>[];
  @override
  Future<RequestPage> load({String? cursor}) async {
    cursors.add(cursor);
    if (failure case final error?) throw error;
    return page;
  }
}
