import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/widgets/app_card.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../application/requests_bloc.dart';
import '../domain/beneficiary_request.dart';
import '../domain/requests_failure.dart';

class RequestCard extends StatelessWidget {
  const RequestCard({required this.request, super.key});

  final BeneficiaryRequest request;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final locale = Localizations.localeOf(context).toLanguageTag();
    return AppCard(
      padding: const EdgeInsets.all(16),
      borderRadius: 18,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  request.reference,
                  textAlign: TextAlign.end,
                  style: const TextStyle(
                    color: AppColors.navy,
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
              const SizedBox(width: 12),
              _StatusBadge(status: request.status),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            _description(request),
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.navy,
              fontSize: 15,
              height: 1.4,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            strings.requestMetadata(
              request.isMultiNeed ? 2 : 1,
              request.isMultiNeed ? request.needs.length : 1,
              request.createdAt == null
                  ? '—'
                  : DateFormat.yMMMMd(locale).format(request.createdAt!),
            ),
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.supportingText,
              fontSize: 13,
            ),
          ),
          if (request.isMultiNeed && request.needs.isNotEmpty) ...[
            const SizedBox(height: 12),
            Wrap(
              alignment: WrapAlignment.end,
              spacing: 6,
              runSpacing: 6,
              children: request.needs
                  .take(3)
                  .map(
                    (need) => Chip(
                      visualDensity: VisualDensity.compact,
                      label: Text(
                        need.category,
                        style: const TextStyle(fontSize: 11),
                      ),
                      backgroundColor: AppColors.canvas,
                      side: const BorderSide(color: AppColors.border),
                      padding: EdgeInsets.zero,
                    ),
                  )
                  .toList(),
            ),
          ],
        ],
      ),
    );
  }

  String _description(BeneficiaryRequest request) {
    if (request.description.trim().isNotEmpty) {
      return request.description.trim();
    }
    return [
      request.category,
      request.serviceSubtype,
    ].whereType<String>().where((value) => value.trim().isNotEmpty).join(' · ');
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final (label, background, foreground, icon) = switch (status) {
      'Submitted' => (
        strings.requestStatusSubmitted,
        AppColors.phaseBadge,
        AppColors.phaseText,
        Icons.hourglass_bottom,
      ),
      'Open' => (
        strings.requestStatusOpen,
        AppColors.selectedSurface,
        AppColors.selectedText,
        Icons.circle_outlined,
      ),
      'InReview' => (
        strings.requestStatusInReview,
        AppColors.scopeSurface,
        AppColors.navy,
        Icons.schedule,
      ),
      'Clarification' => (
        strings.requestStatusClarification,
        AppColors.warningBackground,
        AppColors.warningText,
        Icons.help_outline,
      ),
      'Accepted' => (
        strings.requestStatusAccepted,
        AppColors.selectedSurface,
        AppColors.selectedText,
        Icons.check_circle_outline,
      ),
      'FullyAccepted' => (
        strings.requestStatusFullyAccepted,
        AppColors.selectedSurface,
        AppColors.selectedText,
        Icons.check_circle_outline,
      ),
      'Rejected' => (
        strings.requestStatusRejected,
        AppColors.warningBackground,
        AppColors.warningText,
        Icons.cancel_outlined,
      ),
      'FullyRejected' => (
        strings.requestStatusFullyRejected,
        AppColors.warningBackground,
        AppColors.warningText,
        Icons.cancel_outlined,
      ),
      'PartiallyAccepted' => (
        strings.requestStatusPartiallyAccepted,
        AppColors.phaseBadge,
        AppColors.phaseText,
        Icons.timelapse,
      ),
      'Completed' => (
        strings.requestStatusCompleted,
        AppColors.selectedSurface,
        AppColors.selectedText,
        Icons.task_alt,
      ),
      _ => (
        strings.requestStatusUnknown(status),
        AppColors.canvas,
        AppColors.supportingText,
        Icons.info_outline,
      ),
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 13, color: foreground),
          const SizedBox(width: 4),
          Text(label, style: TextStyle(color: foreground, fontSize: 11)),
        ],
      ),
    );
  }
}

class RequestsContent extends StatelessWidget {
  const RequestsContent({
    required this.state,
    required this.onRetry,
    this.onLoadMore,
    this.onEmptyAction,
    super.key,
  });

  final RequestsState state;
  final VoidCallback onRetry;
  final VoidCallback? onLoadMore;
  final VoidCallback? onEmptyAction;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return switch (state) {
      RequestsInitial() || RequestsLoading() => _scrollable(
        context,
        _message(strings.requestLoading),
      ),
      RequestsFailed(:final failure) => _scrollable(
        context,
        _failure(context, failure),
      ),
      RequestsLoaded(:final items) when items.isEmpty => _scrollable(
        context,
        _empty(strings, onEmptyAction),
      ),
      RequestsLoaded(:final items, :final nextCursor, :final isLoadingMore) =>
        _items(strings, items, nextCursor != null, isLoadingMore),
    };
  }

  Widget _scrollable(BuildContext context, Widget child) => LayoutBuilder(
    builder: (context, constraints) => ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      children: [SizedBox(height: constraints.maxHeight, child: child)],
    ),
  );

  Widget _message(String message) => Center(
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const CircularProgressIndicator(color: AppColors.teal),
        const SizedBox(height: 14),
        Text(message),
      ],
    ),
  );

  Widget _empty(AppLocalizations strings, VoidCallback? onEmptyAction) =>
      Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.inbox_outlined, color: AppColors.teal, size: 42),
              const SizedBox(height: 12),
              Text(
                strings.requestEmptyTitle,
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.w700,
                  color: AppColors.navy,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                strings.requestEmptyBody,
                textAlign: TextAlign.center,
                style: const TextStyle(color: AppColors.supportingText),
              ),
              const SizedBox(height: 18),
              AppButton(
                label: strings.requestEmptyAction,
                onPressed: onEmptyAction,
                variant: AppButtonVariant.secondary,
              ),
            ],
          ),
        ),
      );

  Widget _failure(BuildContext context, RequestsFailureType failure) {
    final strings = AppLocalizations.of(context);
    final (title, body) = switch (failure) {
      RequestsFailureType.sessionExpired => (
        strings.requestExpiredTitle,
        strings.requestExpiredBody,
      ),
      RequestsFailureType.denied => (
        strings.requestDeniedTitle,
        strings.requestDeniedBody,
      ),
      RequestsFailureType.unavailable => (
        strings.requestUnavailableTitle,
        strings.requestUnavailableBody,
      ),
      RequestsFailureType.unexpected => (
        strings.requestUnexpectedTitle,
        strings.requestUnexpectedBody,
      ),
    };
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              failure == RequestsFailureType.sessionExpired
                  ? Icons.lock_clock_outlined
                  : Icons.cloud_off_outlined,
              color: AppColors.supportingText,
              size: 40,
            ),
            const SizedBox(height: 12),
            Text(
              title,
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: AppColors.navy,
                fontSize: 18,
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 6),
            Text(
              body,
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: AppColors.supportingText,
                height: 1.4,
              ),
            ),
            if (failure == RequestsFailureType.unavailable ||
                failure == RequestsFailureType.unexpected) ...[
              const SizedBox(height: 18),
              AppButton(
                label: strings.tryAgain,
                onPressed: onRetry,
                variant: AppButtonVariant.secondary,
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _items(
    AppLocalizations strings,
    List<BeneficiaryRequest> items,
    bool hasMore,
    bool loadingMore,
  ) => ListView.separated(
    padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
    itemCount: items.length + (hasMore ? 1 : 0),
    separatorBuilder: (_, _) => const SizedBox(height: 12),
    itemBuilder: (context, index) => index < items.length
        ? RequestCard(request: items[index])
        : AppButton(
            label: strings.loadMore,
            isLoading: loadingMore,
            onPressed: loadingMore ? null : onLoadMore,
            variant: AppButtonVariant.secondary,
          ),
  );
}
