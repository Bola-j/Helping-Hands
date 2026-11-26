import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/widgets/app_card.dart';
import '../../../core/widgets/app_text_field.dart';
import '../../../features/auth/domain/session_user.dart';
import '../../../features/auth/domain/beneficiary_profile_hint.dart';
import '../../../features/welcome/application/locale_cubit.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../application/single_need_submission_bloc.dart';
import '../domain/beneficiary_request.dart';
import '../domain/single_need_submission.dart';

class SingleNeedRequestPage extends StatefulWidget {
  const SingleNeedRequestPage({
    required this.user,
    this.profileHint,
    super.key,
  });

  final SessionUser user;
  final BeneficiaryProfileHint? profileHint;

  @override
  State<SingleNeedRequestPage> createState() => _SingleNeedRequestPageState();
}

class _SingleNeedRequestPageState extends State<SingleNeedRequestPage> {
  final _descriptionController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _confirmations = false;
  bool _authorityConfirmed = false;
  bool _sharingAgreed = false;
  bool _attemptedSubmit = false;

  @override
  void dispose() {
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final state = context.watch<SingleNeedSubmissionBloc>().state;
    final phase = switch (state) {
      SingleNeedSubmissionSucceeded() => strings.phaseReceipt,
      SingleNeedSubmissionSubmitting() ||
      SingleNeedSubmissionFailed() => strings.phaseSubmittingRequest,
      _ =>
        _confirmations ? strings.phaseConfirmRequest : strings.phaseSingleNeed,
    };
    return Scaffold(
      backgroundColor: AppColors.canvas,
      body: SafeArea(
        bottom: false,
        child: Column(
          children: [
            _RequestFlowHeader(phase: phase),
            Expanded(
              child:
                  BlocListener<
                    SingleNeedSubmissionBloc,
                    SingleNeedSubmissionState
                  >(
                    listenWhen: (_, state) =>
                        state is SingleNeedSubmissionFailed &&
                        state.failure == SingleNeedFailureType.sessionExpired,
                    listener: (context, _) => Navigator.pushNamedAndRemoveUntil(
                      context,
                      AppRoutes.sessionExpired,
                      (route) => route.isFirst,
                    ),
                    child:
                        BlocBuilder<
                          SingleNeedSubmissionBloc,
                          SingleNeedSubmissionState
                        >(
                          builder: (context, state) => switch (state) {
                            SingleNeedSubmissionSubmitting() => _progress(
                              strings,
                            ),
                            SingleNeedSubmissionSucceeded(:final request) =>
                              _success(strings, request),
                            SingleNeedSubmissionFailed(:final failure) =>
                              _recovery(strings, failure),
                            _ =>
                              _confirmations
                                  ? _confirmationContent(strings)
                                  : _requestContent(strings),
                          },
                        ),
                  ),
            ),
            _footer(context, strings),
          ],
        ),
      ),
    );
  }

  Widget _requestContent(AppLocalizations strings) => SingleChildScrollView(
    key: const ValueKey('single-need-form-scroll'),
    padding: const EdgeInsets.fromLTRB(16, 22, 16, 24),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(strings.singleNeedTitle, textAlign: TextAlign.end, style: _title),
        const SizedBox(height: 6),
        Text(
          strings.singleNeedSubtitle,
          textAlign: TextAlign.end,
          style: _subtitle,
        ),
        const SizedBox(height: 82),
        _FieldLabel(text: strings.singleNeedDescriptionLabel),
        const SizedBox(height: 8),
        Form(
          key: _formKey,
          child: AppTextField(
            key: const ValueKey('single-need-description'),
            controller: _descriptionController,
            label: strings.singleNeedDescriptionLabel,
            labelAbove: true,
            hintText: strings.singleNeedDescriptionHint,
            minLines: 3,
            maxLines: 4,
            maxLength: 4000,
            keyboardType: TextInputType.multiline,
            textInputAction: TextInputAction.newline,
            validator: (value) {
              final text = value?.trim() ?? '';
              if (text.isEmpty) return strings.singleNeedDescriptionRequired;
              if (text.length > 4000) {
                return strings.singleNeedDescriptionTooLong;
              }
              return null;
            },
          ),
        ),
        const SizedBox(height: 12),
        _InfoNote(text: strings.singleNeedPrivacyNote),
      ],
    ),
  );

  Widget _confirmationContent(AppLocalizations strings) =>
      SingleChildScrollView(
        key: const ValueKey('single-need-confirmations-scroll'),
        padding: const EdgeInsets.fromLTRB(16, 22, 16, 24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              strings.confirmRequestTitle,
              textAlign: TextAlign.end,
              style: _title,
            ),
            const SizedBox(height: 6),
            Text(
              strings.confirmRequestSubtitle,
              textAlign: TextAlign.end,
              style: _subtitle,
            ),
            const SizedBox(height: 24),
            _ConsentCard(
              checked: _authorityConfirmed,
              title: strings.authorityConfirmationTitle,
              body: strings.authorityConfirmationBody,
              onChanged: (value) => setState(() => _authorityConfirmed = value),
            ),
            const SizedBox(height: 12),
            _ConsentCard(
              checked: _sharingAgreed,
              title: strings.sharingConfirmationTitle,
              body: strings.sharingConfirmationBody,
              onChanged: (value) => setState(() => _sharingAgreed = value),
            ),
            const SizedBox(height: 20),
            _InfoNote(text: strings.submissionReviewNote),
            if (_attemptedSubmit &&
                (!_authorityConfirmed || !_sharingAgreed)) ...[
              const SizedBox(height: 12),
              Text(
                strings.confirmationRequired,
                key: const ValueKey('single-need-confirmation-error'),
                textAlign: TextAlign.end,
                style: const TextStyle(color: AppColors.error, fontSize: 13),
              ),
            ],
          ],
        ),
      );

  Widget _progress(AppLocalizations strings) => Center(
    child: SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Transform.translate(
        offset: const Offset(0, 14),
        child: AppCard(
          padding: const EdgeInsets.fromLTRB(24, 46, 24, 32),
          borderRadius: 18,
          child: Column(
            children: [
              Container(
                width: 56,
                height: 56,
                decoration: const BoxDecoration(
                  color: AppColors.selectedSurface,
                  shape: BoxShape.circle,
                ),
                child: const Center(
                  child: SizedBox.square(
                    dimension: 24,
                    child: CircularProgressIndicator(
                      strokeWidth: 2.5,
                      color: AppColors.teal,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 18),
              Text(
                strings.submissionInProgress,
                textAlign: TextAlign.center,
                style: _title,
              ),
              const SizedBox(height: 10),
              Text(
                strings.submissionWaitNote,
                textAlign: TextAlign.center,
                style: _subtitle,
              ),
              const SizedBox(height: 80),
            ],
          ),
        ),
      ),
    ),
  );

  Widget _success(AppLocalizations strings, BeneficiaryRequest request) =>
      SingleChildScrollView(
        key: const ValueKey('single-need-success'),
        padding: const EdgeInsets.fromLTRB(16, 24, 16, 24),
        child: Column(
          children: [
            const SizedBox(height: 95),
            const _SuccessMark(),
            const SizedBox(height: 18),
            Text(
              strings.submissionSuccessTitle,
              textAlign: TextAlign.center,
              style: _title,
            ),
            const SizedBox(height: 8),
            Text(
              strings.submissionSuccessSubtitle,
              textAlign: TextAlign.center,
              style: _subtitle,
            ),
            const SizedBox(height: 18),
            AppCard(
              padding: const EdgeInsets.all(16),
              borderRadius: 16,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _RequestValueRow(
                    label: strings.requestReferenceLabel,
                    value: request.reference,
                    emphasize: true,
                  ),
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 12),
                    child: Divider(height: 1, color: AppColors.border),
                  ),
                  _RequestValueRow(
                    label: strings.requestCurrentStatusLabel,
                    value: _statusLabel(strings, request.status),
                    status: true,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 18),
            _InfoNote(text: strings.submissionReviewNote),
          ],
        ),
      );

  Widget _recovery(AppLocalizations strings, SingleNeedFailureType failure) =>
      Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              const _InfoMark(size: 36, color: AppColors.navy),
              const SizedBox(height: 18),
              Text(
                failure == SingleNeedFailureType.outcomeUnconfirmed
                    ? strings.submissionUnknownTitle
                    : strings.submissionFailedTitle,
                textAlign: TextAlign.center,
                style: _title,
              ),
              const SizedBox(height: 12),
              Text(
                _failureText(strings, failure),
                textAlign: TextAlign.center,
                style: _subtitle,
              ),
              const SizedBox(height: 20),
              if (failure == SingleNeedFailureType.outcomeUnconfirmed ||
                  failure == SingleNeedFailureType.unavailable ||
                  failure == SingleNeedFailureType.unexpected)
                AppButton(
                  label: strings.viewMyRequests,
                  onPressed: () => _openRequests(context),
                )
              else
                AppButton(
                  label: strings.submissionTryAgain,
                  onPressed: () => setState(() {
                    _confirmations = true;
                    _attemptedSubmit = false;
                  }),
                ),
            ],
          ),
        ),
      );

  Widget _footer(BuildContext context, AppLocalizations strings) {
    final state = context.watch<SingleNeedSubmissionBloc>().state;
    if (state is SingleNeedSubmissionSucceeded) {
      return _Footer(
        primary: strings.viewMyRequests,
        onPrimary: () => _openRequests(context),
        secondary: strings.backToHome,
        onSecondary: () => Navigator.pushNamedAndRemoveUntil(
          context,
          AppRoutes.home,
          (route) => route.isFirst,
          arguments: BeneficiaryHomeRouteArguments(
            widget.user,
            profileHint: widget.profileHint,
          ),
        ),
      );
    }
    if (state is SingleNeedSubmissionSubmitting) {
      return const SizedBox(height: 24);
    }
    if (state is SingleNeedSubmissionFailed) {
      return const SizedBox(height: 24);
    }
    if (_confirmations) {
      return _Footer(
        primary: strings.submitRequestNow,
        onPrimary: _submit,
        secondary: strings.cancelRequest,
        onSecondary: () => setState(() {
          _confirmations = false;
          _attemptedSubmit = false;
        }),
      );
    }
    return _Footer(
      primary: strings.continueToConfirm,
      onPrimary: _continue,
      secondary: strings.cancelRequest,
      onSecondary: () => Navigator.maybePop(context),
    );
  }

  void _continue() {
    if (!(_formKey.currentState?.validate() ?? false)) return;
    setState(() => _confirmations = true);
  }

  void _submit() {
    setState(() => _attemptedSubmit = true);
    if (!_authorityConfirmed || !_sharingAgreed) return;
    context.read<SingleNeedSubmissionBloc>().add(
      SingleNeedSubmitRequested(
        SingleNeedSubmission(
          description: _descriptionController.text,
          authorityConfirmed: _authorityConfirmed,
          sharingAgreed: _sharingAgreed,
        ),
        profileHint: widget.profileHint,
      ),
    );
  }

  void _openRequests(BuildContext context) => Navigator.pushNamedAndRemoveUntil(
    context,
    AppRoutes.requests,
    (route) => route.isFirst,
    arguments: widget.user,
  );

  String _failureText(
    AppLocalizations strings,
    SingleNeedFailureType failure,
  ) => switch (failure) {
    SingleNeedFailureType.invalidInput => strings.submissionInvalidBody,
    SingleNeedFailureType.sessionExpired => strings.signInExpired,
    SingleNeedFailureType.denied => strings.submissionDeniedBody,
    SingleNeedFailureType.profileUnavailable => strings.submissionProfileBody,
    SingleNeedFailureType.conflict => strings.submissionConflictBody,
    SingleNeedFailureType.outcomeUnconfirmed => strings.submissionUnknownBody,
    SingleNeedFailureType.unavailable => strings.submissionUnavailableBody,
    SingleNeedFailureType.unexpected => strings.submissionUnexpectedBody,
  };

  String _statusLabel(AppLocalizations strings, String status) =>
      switch (status) {
        'Submitted' => strings.statusSubmitted,
        'Open' => strings.statusOpen,
        'InReview' || 'UnderReview' => strings.statusInReview,
        _ => strings.statusUnknown(status),
      };

  TextStyle get _title => const TextStyle(
    color: AppColors.navy,
    fontSize: 24,
    height: 1.3,
    fontWeight: FontWeight.w700,
  );

  TextStyle get _subtitle => const TextStyle(
    color: AppColors.supportingText,
    fontSize: 13,
    height: 1.55,
  );
}

class _RequestFlowHeader extends StatelessWidget {
  const _RequestFlowHeader({required this.phase});
  final String phase;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final language = Localizations.localeOf(context).languageCode;
    return Container(
      height: 88,
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: AppColors.border)),
      ),
      child: Stack(
        alignment: Alignment.center,
        children: [
          Positioned.fill(
            child: Center(
              child: Container(
                constraints: const BoxConstraints(maxWidth: 190),
                padding: const EdgeInsets.symmetric(
                  horizontal: 12,
                  vertical: 5,
                ),
                decoration: const BoxDecoration(
                  color: AppColors.selectedSurface,
                  borderRadius: BorderRadius.all(Radius.circular(999)),
                ),
                child: Text(
                  phase,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: AppColors.selectedText,
                    fontSize: 11,
                  ),
                ),
              ),
            ),
          ),
          Row(
            textDirection: TextDirection.ltr,
            children: [
              OutlinedButton(
                onPressed: () => context.read<LocaleCubit>().select(
                  Locale(language == 'ar' ? 'en' : 'ar'),
                ),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppColors.navy,
                  backgroundColor: AppColors.white,
                  side: const BorderSide(color: AppColors.border),
                  shape: const StadiumBorder(),
                  minimumSize: const Size(42, 40),
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                ),
                child: Text(language.toUpperCase()),
              ),
              const Spacer(),
              SizedBox.square(
                dimension: 48,
                child: OutlinedButton(
                  onPressed: () => Navigator.maybePop(context),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppColors.navy,
                    backgroundColor: AppColors.white,
                    side: const BorderSide(color: AppColors.border),
                    shape: const CircleBorder(),
                    padding: EdgeInsets.zero,
                  ),
                  child: Semantics(
                    label: strings.back,
                    child: CustomPaint(
                      size: const Size(20, 20),
                      painter: _ArrowPainter(
                        rightToLeft:
                            Directionality.of(context) == TextDirection.rtl,
                        color: AppColors.navy,
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _Footer extends StatelessWidget {
  const _Footer({
    required this.primary,
    required this.onPrimary,
    required this.secondary,
    required this.onSecondary,
  });

  final String primary;
  final VoidCallback onPrimary;
  final String secondary;
  final VoidCallback onSecondary;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.fromLTRB(16, 16, 16, 44),
    decoration: const BoxDecoration(
      border: Border(top: BorderSide(color: AppColors.border)),
    ),
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        AppButton(label: primary, onPressed: onPrimary),
        const SizedBox(height: 8),
        TextButton(
          onPressed: onSecondary,
          style: TextButton.styleFrom(
            foregroundColor: AppColors.supportingText,
          ),
          child: Text(secondary),
        ),
      ],
    ),
  );
}

class _FieldLabel extends StatelessWidget {
  const _FieldLabel({required this.text});
  final String text;

  @override
  Widget build(BuildContext context) => Text(
    text,
    textAlign: TextAlign.end,
    style: const TextStyle(
      color: AppColors.navy,
      fontSize: 13,
      fontWeight: FontWeight.w600,
    ),
  );
}

class _ConsentCard extends StatelessWidget {
  const _ConsentCard({
    required this.checked,
    required this.title,
    required this.body,
    required this.onChanged,
  });

  final bool checked;
  final String title;
  final String body;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) => AppCard(
    padding: const EdgeInsets.all(20),
    borderRadius: 16,
    onTap: () => onChanged(!checked),
    child: Padding(
      padding: const EdgeInsetsDirectional.only(start: 32),
      child: Row(
        textDirection: Directionality.of(context),
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Checkbox(
            value: checked,
            onChanged: (value) => onChanged(value ?? false),
            activeColor: AppColors.teal,
            semanticLabel: title,
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  title,
                  textAlign: TextAlign.end,
                  style: const TextStyle(
                    color: AppColors.navy,
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  body,
                  textAlign: TextAlign.end,
                  style: const TextStyle(
                    color: AppColors.supportingText,
                    fontSize: 12,
                    height: 1.5,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}

class _InfoNote extends StatelessWidget {
  const _InfoNote({required this.text});
  final String text;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(15),
    decoration: BoxDecoration(
      color: AppColors.scopeSurface,
      border: Border.all(color: AppColors.border),
      borderRadius: BorderRadius.circular(16),
    ),
    child: Row(
      textDirection: Directionality.of(context),
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const _InfoMark(size: 18, color: AppColors.navy),
        const SizedBox(width: 10),
        Expanded(
          child: Text(
            text,
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.navy,
              fontSize: 12,
              height: 1.55,
            ),
          ),
        ),
      ],
    ),
  );
}

class _SuccessMark extends StatelessWidget {
  const _SuccessMark();

  @override
  Widget build(BuildContext context) => Container(
    width: 64,
    height: 64,
    decoration: const BoxDecoration(
      color: Color(0xFFE6F3EF),
      shape: BoxShape.circle,
    ),
    child: const Center(
      child: CustomPaint(
        size: Size(30, 30),
        painter: _CheckPainter(color: AppColors.teal),
      ),
    ),
  );
}

class _RequestValueRow extends StatelessWidget {
  const _RequestValueRow({
    required this.label,
    required this.value,
    this.emphasize = false,
    this.status = false,
  });

  final String label;
  final String value;
  final bool emphasize;
  final bool status;

  @override
  Widget build(BuildContext context) => Row(
    textDirection: Directionality.of(context),
    children: [
      Expanded(
        child: Text(
          label,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(color: AppColors.supportingText, fontSize: 12),
        ),
      ),
      const SizedBox(width: 8),
      Flexible(
        child: Container(
          padding: EdgeInsets.symmetric(
            horizontal: status ? 10 : 0,
            vertical: status ? 5 : 0,
          ),
          decoration: status
              ? const BoxDecoration(
                  color: AppColors.scopeSurface,
                  borderRadius: BorderRadius.all(Radius.circular(999)),
                )
              : null,
          child: Text(
            value,
            textAlign: TextAlign.end,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              color: emphasize ? AppColors.navy : AppColors.supportingText,
              fontSize: emphasize ? 17 : 12,
              fontWeight: emphasize ? FontWeight.w700 : FontWeight.w500,
            ),
          ),
        ),
      ),
    ],
  );
}

class _InfoMark extends StatelessWidget {
  const _InfoMark({required this.size, required this.color});

  final double size;
  final Color color;

  @override
  Widget build(BuildContext context) =>
      CustomPaint(size: Size.square(size), painter: _InfoPainter(color));
}

class _InfoPainter extends CustomPainter {
  const _InfoPainter(this.color);
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final stroke = Paint()
      ..color = color
      ..strokeWidth = 1.7
      ..style = PaintingStyle.stroke;
    final center = Offset(size.width / 2, size.height / 2);
    canvas.drawCircle(center, size.width / 2 - 1.5, stroke);
    canvas.drawLine(
      Offset(center.dx, center.dy - 1),
      Offset(center.dx, center.dy + size.height * .22),
      stroke,
    );
    canvas.drawCircle(
      Offset(center.dx, center.dy - size.height * .22),
      1,
      Paint()..color = color,
    );
  }

  @override
  bool shouldRepaint(_InfoPainter oldDelegate) => oldDelegate.color != color;
}

class _ArrowPainter extends CustomPainter {
  const _ArrowPainter({required this.rightToLeft, required this.color});

  final bool rightToLeft;
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..strokeWidth = 1.8
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round
      ..style = PaintingStyle.stroke;
    final y = size.height / 2;
    final start = rightToLeft ? 3.0 : size.width - 3;
    final end = rightToLeft ? size.width - 3 : 3.0;
    final tipX = end;
    final wingX = rightToLeft ? end - 6 : end + 6;
    canvas.drawLine(Offset(start, y), Offset(end, y), paint);
    canvas.drawLine(Offset(tipX, y), Offset(wingX, y - 6), paint);
    canvas.drawLine(Offset(tipX, y), Offset(wingX, y + 6), paint);
  }

  @override
  bool shouldRepaint(_ArrowPainter oldDelegate) =>
      oldDelegate.rightToLeft != rightToLeft || oldDelegate.color != color;
}

class _CheckPainter extends CustomPainter {
  const _CheckPainter({required this.color});
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..strokeWidth = 3
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round
      ..style = PaintingStyle.stroke;
    final path = Path()
      ..moveTo(size.width * .2, size.height * .52)
      ..lineTo(size.width * .42, size.height * .73)
      ..lineTo(size.width * .82, size.height * .28);
    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(_CheckPainter oldDelegate) => oldDelegate.color != color;
}
