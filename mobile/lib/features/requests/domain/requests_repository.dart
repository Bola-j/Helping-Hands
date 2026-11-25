import 'beneficiary_request.dart';

class RequestPage {
  const RequestPage({required this.items, this.nextCursor});

  final List<BeneficiaryRequest> items;
  final String? nextCursor;
}

abstract interface class RequestsRepository {
  Future<RequestPage> load({String? cursor});
}
