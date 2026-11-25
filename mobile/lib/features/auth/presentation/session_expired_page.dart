import 'package:flutter/material.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/widgets/app_card.dart';
import '../../../core/widgets/flow_screen_header.dart';
import '../../../l10n/generated/app_localizations.dart';

class SessionExpiredPage extends StatelessWidget {
  const SessionExpiredPage({super.key});

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return Scaffold(
      backgroundColor: AppColors.canvas,
      body: SafeArea(
        child: Column(
          children: [
            FlowScreenHeader(
              phaseLabel: strings.phaseSessionExpired,
              showBack: false,
            ),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 20, 16, 16),
                child: Column(
                  children: [
                    Expanded(
                      child: SingleChildScrollView(
                        child: ConstrainedBox(
                          constraints: BoxConstraints(
                            minHeight: MediaQuery.sizeOf(context).height * .56,
                          ),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Container(
                                width: 64,
                                height: 64,
                                decoration: const BoxDecoration(
                                  color: AppColors.warningBackground,
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(
                                  Icons.lock_clock_outlined,
                                  color: AppColors.warningText,
                                  size: 30,
                                ),
                              ),
                              const SizedBox(height: 20),
                              Text(
                                strings.sessionExpiredTitle,
                                textAlign: TextAlign.center,
                                style: const TextStyle(
                                  color: AppColors.navy,
                                  fontSize: 24,
                                  height: 1.3,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                              const SizedBox(height: 12),
                              Text(
                                strings.sessionExpiredBody,
                                textAlign: TextAlign.center,
                                style: const TextStyle(
                                  color: AppColors.supportingText,
                                  fontSize: 14,
                                  height: 1.5,
                                ),
                              ),
                              const SizedBox(height: 20),
                              AppCard(
                                padding: const EdgeInsets.all(16),
                                child: Row(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Icon(
                                      Icons.verified_user_outlined,
                                      color: AppColors.teal,
                                      size: 20,
                                    ),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Text(
                                        strings.sessionExpiredPrivacy,
                                        textAlign: TextAlign.start,
                                        style: const TextStyle(
                                          color: AppColors.supportingText,
                                          fontSize: 13,
                                          height: 1.45,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                    AppButton(
                      label: strings.signInAgain,
                      onPressed: () => Navigator.pushNamedAndRemoveUntil(
                        context,
                        AppRoutes.signIn,
                        (route) => route.isFirst,
                      ),
                    ),
                    const SizedBox(height: 12),
                    AppButton(
                      label: strings.returnWelcome,
                      variant: AppButtonVariant.secondary,
                      onPressed: () => Navigator.pushNamedAndRemoveUntil(
                        context,
                        AppRoutes.welcome,
                        (_) => false,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
