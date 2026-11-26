class BeneficiaryRequest {
  const BeneficiaryRequest({
    required this.id,
    required this.reference,
    required this.isMultiNeed,
    required this.description,
    required this.status,
    required this.createdAt,
    this.category,
    this.serviceSubtype,
    this.requiredQuantity,
    this.unit,
    this.needs = const [],
  });

  final String id;
  final String reference;
  final bool isMultiNeed;
  final String description;
  final String status;
  final DateTime? createdAt;
  final String? category;
  final String? serviceSubtype;
  final num? requiredQuantity;
  final String? unit;
  final List<RecordedNeedSummary> needs;

  factory BeneficiaryRequest.fromJson(Map<String, dynamic> json) {
    final needs = (json['needs'] as List<dynamic>? ?? const [])
        .whereType<Map<String, dynamic>>()
        .map(RecordedNeedSummary.fromJson)
        .toList(growable: false);
    return BeneficiaryRequest(
      id: json['id'] as String,
      reference: json['reference'] as String,
      isMultiNeed: json['isMultiNeed'] as bool? ?? false,
      description: json['description'] as String? ?? '',
      status: json['status'] as String? ?? 'Submitted',
      createdAt: DateTime.tryParse(json['createdAt'] as String? ?? ''),
      category: json['category'] as String?,
      serviceSubtype: json['serviceSubtype'] as String?,
      requiredQuantity: json['requiredQuantity'] as num?,
      unit: json['unit'] as String?,
      needs: needs,
    );
  }
}

class RecordedNeedSummary {
  const RecordedNeedSummary({
    required this.category,
    required this.serviceSubtype,
    required this.quantity,
    required this.status,
  });

  final String category;
  final String? serviceSubtype;
  final num quantity;
  final String status;

  factory RecordedNeedSummary.fromJson(Map<String, dynamic> json) =>
      RecordedNeedSummary(
        category: json['category'] as String? ?? '',
        serviceSubtype: json['serviceSubtype'] as String?,
        quantity: json['quantity'] as num? ?? 0,
        status: json['status'] as String? ?? 'Submitted',
      );
}
