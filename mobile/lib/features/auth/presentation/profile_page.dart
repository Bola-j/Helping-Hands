import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_card.dart';
import '../../../core/widgets/beneficiary_bottom_navigation.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../../welcome/application/locale_cubit.dart';
import '../application/session_bloc.dart';
import '../domain/session_user.dart';

class ProfilePage extends StatelessWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return Scaffold(
      backgroundColor: AppColors.canvas,
      bottomNavigationBar: BlocBuilder<SessionBloc, SessionState>(
        builder: (context, state) => BeneficiaryBottomNavigation(
          selected: BeneficiaryNavigationItem.account,
          onSelected: (item) {
            final user = _activeUser(state);
            if (user == null) return;
            final route = switch (item) {
              BeneficiaryNavigationItem.home => AppRoutes.home,
              BeneficiaryNavigationItem.requests => AppRoutes.requests,
              BeneficiaryNavigationItem.account => AppRoutes.profile,
            };
            Navigator.pushReplacementNamed(context, route, arguments: user);
          },
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 16),
          child: BlocBuilder<SessionBloc, SessionState>(
            builder: (context, state) {
              final user = _activeUser(state);
              return Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _pageTitle(strings),
                  const SizedBox(height: 20),
                  if (user != null) _accountCard(user),
                  const SizedBox(height: 20),
                  _languageCard(context, strings),
                  if (state is SessionSignedOut && state.failure != null) ...[
                    const SizedBox(height: 12),
                    Text(
                      strings.signOutFailed,
                      style: const TextStyle(color: AppColors.error),
                    ),
                  ],
                  const Spacer(),
                  if (user != null)
                    _SignOutButton(
                      isLoading: state is SessionSigningOut,
                      onPressed: () =>
                          context.read<SessionBloc>().add(SignOutRequested()),
                    )
                  else if (state is SessionSigningOut)
                    const Center(child: CircularProgressIndicator())
                  else
                    _SignedOutAction(
                      message: strings.signedOut,
                      buttonLabel: strings.backToSignIn,
                      onPressed: () => Navigator.pushNamedAndRemoveUntil(
                        context,
                        AppRoutes.signIn,
                        (route) => route.isFirst,
                      ),
                    ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }

  SessionUser? _activeUser(SessionState state) => switch (state) {
    SessionSignedIn(:final user) => user,
    _ => null,
  };

  Widget _pageTitle(AppLocalizations strings) => Column(
    crossAxisAlignment: CrossAxisAlignment.stretch,
    children: [
      Text(
        strings.profileTitle,
        textAlign: TextAlign.end,
        style: const TextStyle(
          color: AppColors.navy,
          fontSize: 24,
          height: 1.3,
          fontWeight: FontWeight.w700,
        ),
      ),
      Text(
        strings.profileSubtitle,
        textAlign: TextAlign.end,
        style: const TextStyle(
          color: AppColors.supportingText,
          fontSize: 12,
          height: 1.5,
        ),
      ),
      const SizedBox(height: 12),
      const Divider(height: 1, color: AppColors.border),
    ],
  );

  Widget _accountCard(SessionUser user) => AppCard(
    padding: const EdgeInsets.all(16),
    child: Row(
      children: [
        CircleAvatar(
          radius: 24,
          backgroundColor: AppColors.selectedSurface,
          child: Text(
            user.displayName.trim().isEmpty
                ? '?'
                : user.displayName.trim().substring(0, 1),
            style: const TextStyle(
              color: AppColors.selectedText,
              fontSize: 18,
              fontWeight: FontWeight.w700,
            ),
          ),
        ),
        const SizedBox(width: 14),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                user.displayName,
                textAlign: TextAlign.end,
                style: const TextStyle(
                  color: AppColors.navy,
                  fontSize: 15,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 3),
              Text(
                user.email,
                textAlign: TextAlign.end,
                textDirection: TextDirection.ltr,
                style: const TextStyle(
                  color: AppColors.supportingText,
                  fontSize: 13,
                ),
              ),
            ],
          ),
        ),
      ],
    ),
  );

  Widget _languageCard(BuildContext context, AppLocalizations strings) {
    final selected = Localizations.localeOf(context).languageCode;
    return AppCard(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            strings.chooseLanguage,
            textAlign: TextAlign.end,
            style: const TextStyle(
              color: AppColors.navy,
              fontSize: 15,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            textDirection: TextDirection.ltr,
            children: [
              Expanded(
                child: _LanguageButton(
                  label: strings.englishLtr,
                  language: 'en',
                  selected: selected == 'en',
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _LanguageButton(
                  label: strings.arabicRtl,
                  language: 'ar',
                  selected: selected == 'ar',
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _LanguageButton extends StatelessWidget {
  const _LanguageButton({
    required this.label,
    required this.language,
    required this.selected,
  });

  final String label;
  final String language;
  final bool selected;

  @override
  Widget build(BuildContext context) => Semantics(
    button: true,
    selected: selected,
    label: label,
    child: AppCard(
      padding: EdgeInsets.zero,
      selected: selected,
      borderRadius: 12,
      borderWidth: selected ? 2 : 1,
      onTap: () => context.read<LocaleCubit>().select(Locale(language)),
      child: SizedBox(
        height: 48,
        child: Center(
          child: FittedBox(
            fit: BoxFit.scaleDown,
            child: Row(
              textDirection: TextDirection.ltr,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  label,
                  style: const TextStyle(
                    color: AppColors.navy,
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                if (selected) ...[
                  const SizedBox(width: 6),
                  const Icon(
                    Icons.check_circle_outline,
                    color: AppColors.teal,
                    size: 16,
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    ),
  );
}

class _SignOutButton extends StatelessWidget {
  const _SignOutButton({required this.isLoading, required this.onPressed});

  final bool isLoading;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return SizedBox(
      height: 48,
      child: OutlinedButton.icon(
        key: const ValueKey('sign-out-button'),
        onPressed: isLoading ? null : onPressed,
        style: OutlinedButton.styleFrom(
          foregroundColor: AppColors.error,
          backgroundColor: AppColors.signOutBackground,
          side: const BorderSide(color: AppColors.signOutBorder),
          shape: const RoundedRectangleBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
          ),
          textStyle: const TextStyle(fontWeight: FontWeight.w600),
        ),
        icon: isLoading
            ? const SizedBox.square(
                dimension: 18,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  color: AppColors.error,
                ),
              )
            : const Icon(Icons.logout, size: 18),
        label: Text(strings.signOut),
      ),
    );
  }
}

class _SignedOutAction extends StatelessWidget {
  const _SignedOutAction({
    required this.message,
    required this.buttonLabel,
    required this.onPressed,
  });

  final String message;
  final String buttonLabel;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.stretch,
    children: [
      Text(message, textAlign: TextAlign.center),
      const SizedBox(height: 8),
      SizedBox(
        height: 48,
        child: OutlinedButton(onPressed: onPressed, child: Text(buttonLabel)),
      ),
    ],
  );
}
