import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/widgets/app_card.dart';
import '../../../core/widgets/beneficiary_bottom_navigation.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../../auth/domain/session_user.dart';
import '../../welcome/application/locale_cubit.dart';
import '../application/requests_bloc.dart';
import '../domain/beneficiary_request.dart';
import '../domain/requests_failure.dart';
import 'request_widgets.dart';

class BeneficiaryHomePage extends StatelessWidget {
  const BeneficiaryHomePage({required this.user, super.key});

  final SessionUser user;

  @override
  Widget build(BuildContext context) => Scaffold(
    backgroundColor: AppColors.canvas,
    appBar: PreferredSize(
      preferredSize: const Size.fromHeight(80),
      child: _HomeHeader(user: user),
    ),
    bottomNavigationBar: BeneficiaryBottomNavigation(
      selected: BeneficiaryNavigationItem.home,
      onSelected: (item) => _navigate(context, item, user),
    ),
    body: SafeArea(
      top: false,
      child: BlocListener<RequestsBloc, RequestsState>(
        listenWhen: (_, state) =>
            state is RequestsFailed &&
            state.failure == RequestsFailureType.sessionExpired,
        listener: (context, _) => Navigator.pushNamedAndRemoveUntil(
          context,
          AppRoutes.sessionExpired,
          (route) => route.isFirst,
        ),
        child: BlocBuilder<RequestsBloc, RequestsState>(
          builder: (context, state) => RefreshIndicator(
            color: AppColors.teal,
            onRefresh: () async => context.read<RequestsBloc>().add(
              const RequestsRefreshRequested(),
            ),
            child: switch (state) {
              RequestsInitial() ||
              RequestsLoading() => const _HomeLoadingBody(),
              RequestsFailed(:final failure) => _HomeFailureBody(
                failure: failure,
              ),
              RequestsLoaded(:final items) => _HomeBody(
                user: user,
                request: items.isEmpty ? null : items.first,
              ),
            },
          ),
        ),
      ),
    ),
  );
}

class _HomeHeader extends StatelessWidget implements PreferredSizeWidget {
  const _HomeHeader({required this.user});
  final SessionUser user;
  @override
  Size get preferredSize => const Size.fromHeight(80);

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final language = Localizations.localeOf(context).languageCode;
    return SafeArea(
      bottom: false,
      child: Container(
        height: 80,
        padding: const EdgeInsets.symmetric(horizontal: 16),
        decoration: const BoxDecoration(
          color: AppColors.white,
          border: Border(bottom: BorderSide(color: AppColors.border)),
        ),
        child: Row(
          textDirection: TextDirection.ltr,
          children: [
            TextButton(
              onPressed: () => context.read<LocaleCubit>().select(
                Locale(language == 'ar' ? 'en' : 'ar'),
              ),
              style: TextButton.styleFrom(
                foregroundColor: AppColors.navy,
                side: const BorderSide(color: AppColors.border),
                shape: const StadiumBorder(),
                padding: const EdgeInsets.symmetric(horizontal: 13),
                minimumSize: const Size(44, 40),
              ),
              child: Text(language == 'ar' ? 'AR' : 'EN'),
            ),
            const Spacer(),
            Expanded(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    strings.beneficiaryGreeting,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: AppColors.supportingText,
                      fontSize: 11,
                    ),
                  ),
                  Text(
                    user.displayName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: AppColors.navy,
                      fontSize: 17,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(width: 10),
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: AppColors.teal,
                borderRadius: BorderRadius.circular(12),
              ),
              padding: const EdgeInsets.all(7),
              child: SvgPicture.asset(
                'assets/icons/helping-hands.svg',
                colorFilter: const ColorFilter.mode(
                  AppColors.white,
                  BlendMode.srcIn,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _HomeBody extends StatelessWidget {
  const _HomeBody({required this.user, required this.request});
  final SessionUser user;
  final BeneficiaryRequest? request;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(16, 20, 16, 24),
      children: [
        const _NewRequestCard(),
        const SizedBox(height: 22),
        Row(
          children: [
            Text(
              strings.requestsTitle,
              style: const TextStyle(
                color: AppColors.navy,
                fontSize: 21,
                fontWeight: FontWeight.w700,
              ),
            ),
            const Spacer(),
            TextButton(
              onPressed: () =>
                  _navigate(context, BeneficiaryNavigationItem.requests, user),
              child: Text(
                strings.viewAllRequests,
                style: const TextStyle(
                  color: AppColors.teal,
                  fontSize: 12,
                  decoration: TextDecoration.underline,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        if (request == null)
          _HomeEmptyRequestCard()
        else
          RequestCard(request: request!),
        const SizedBox(height: 18),
        _PrivacyNote(text: strings.requestPrivacyNote),
      ],
    );
  }
}

class _NewRequestCard extends StatelessWidget {
  const _NewRequestCard();

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return AppCard(
      padding: const EdgeInsets.all(16),
      borderRadius: 18,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Align(
            alignment: AlignmentDirectional.centerEnd,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
              decoration: BoxDecoration(
                color: AppColors.phaseBadge,
                borderRadius: BorderRadius.circular(20),
              ),
              child: Text(
                strings.homeNewRequestLabel,
                style: const TextStyle(
                  color: AppColors.phaseText,
                  fontSize: 10,
                ),
              ),
            ),
          ),
          const SizedBox(height: 5),
          Text(
            strings.homeQuestion,
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.navy,
              fontSize: 20,
              height: 1.3,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 6),
          Text(
            strings.homeQuestionBody,
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.supportingText,
              fontSize: 13,
              height: 1.55,
            ),
          ),
          const SizedBox(height: 14),
          AppButton(
            label: strings.singleNeedRequest,
            onPressed: () => ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text(strings.supportSingleNeedUnavailable)),
            ),
          ),
          const SizedBox(height: 9),
          AppButton(
            label: strings.multipleNeedsRequest,
            onPressed: null,
            isEnabled: false,
            variant: AppButtonVariant.secondary,
          ),
        ],
      ),
    );
  }
}

class _HomeEmptyRequestCard extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return AppCard(
      padding: const EdgeInsets.all(18),
      child: Column(
        children: [
          const Icon(Icons.inbox_outlined, color: AppColors.teal),
          const SizedBox(height: 8),
          Text(
            strings.requestEmptyTitle,
            style: const TextStyle(
              color: AppColors.navy,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            strings.requestEmptyBody,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: AppColors.supportingText,
              fontSize: 13,
            ),
          ),
        ],
      ),
    );
  }
}

class _HomeLoadingBody extends StatelessWidget {
  const _HomeLoadingBody();
  @override
  Widget build(BuildContext context) => ListView(
    physics: const AlwaysScrollableScrollPhysics(),
    padding: const EdgeInsets.all(16),
    children: [
      const LinearProgressIndicator(color: AppColors.teal),
      const SizedBox(height: 20),
      const _NewRequestCard(),
      const SizedBox(height: 24),
      Center(child: Text(AppLocalizations.of(context).requestLoading)),
    ],
  );
}

class _HomeFailureBody extends StatelessWidget {
  const _HomeFailureBody({required this.failure});
  final RequestsFailureType failure;
  @override
  Widget build(BuildContext context) => RequestsContent(
    state: RequestsFailed(failure),
    onRetry: () =>
        context.read<RequestsBloc>().add(const RequestsRefreshRequested()),
  );
}

class _PrivacyNote extends StatelessWidget {
  const _PrivacyNote({required this.text});
  final String text;
  @override
  Widget build(BuildContext context) => AppCard(
    padding: const EdgeInsets.all(14),
    backgroundColor: AppColors.scopeSurface,
    borderRadius: 16,
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Icon(Icons.info_outline, color: AppColors.navy, size: 20),
        const SizedBox(width: 10),
        Expanded(
          child: Text(
            text,
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.navy,
              fontSize: 12,
              height: 1.5,
            ),
          ),
        ),
      ],
    ),
  );
}

void _navigate(
  BuildContext context,
  BeneficiaryNavigationItem item,
  SessionUser user,
) {
  final route = switch (item) {
    BeneficiaryNavigationItem.home => AppRoutes.home,
    BeneficiaryNavigationItem.requests => AppRoutes.requests,
    BeneficiaryNavigationItem.account => AppRoutes.profile,
  };
  Navigator.pushReplacementNamed(context, route, arguments: user);
}
