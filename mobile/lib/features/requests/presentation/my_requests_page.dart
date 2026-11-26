import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/beneficiary_bottom_navigation.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../../auth/domain/session_user.dart';
import '../../welcome/application/locale_cubit.dart';
import '../application/requests_bloc.dart';
import '../domain/requests_failure.dart';
import 'request_widgets.dart';

class MyRequestsPage extends StatelessWidget {
  const MyRequestsPage({required this.user, super.key});
  final SessionUser user;

  @override
  Widget build(BuildContext context) => Scaffold(
    backgroundColor: AppColors.canvas,
    appBar: PreferredSize(
      preferredSize: const Size.fromHeight(88),
      child: const _RequestsHeader(),
    ),
    bottomNavigationBar: BeneficiaryBottomNavigation(
      selected: BeneficiaryNavigationItem.requests,
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
            child: RequestsContent(
              state: state,
              onRetry: () => context.read<RequestsBloc>().add(
                const RequestsRefreshRequested(),
              ),
              onLoadMore: () => context.read<RequestsBloc>().add(
                const RequestsLoadMoreRequested(),
              ),
              onEmptyAction: () => Navigator.pushReplacementNamed(
                context,
                AppRoutes.home,
                arguments: user,
              ),
            ),
          ),
        ),
      ),
    ),
  );
}

class _RequestsHeader extends StatelessWidget implements PreferredSizeWidget {
  const _RequestsHeader();
  @override
  Size get preferredSize => const Size.fromHeight(88);

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final language = Localizations.localeOf(context).languageCode;
    return SafeArea(
      bottom: false,
      child: Container(
        height: 88,
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
                    strings.requestsTitle,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: AppColors.navy,
                      fontSize: 22,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  Text(
                    strings.requestsSubtitle,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: AppColors.supportingText,
                      fontSize: 12,
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
