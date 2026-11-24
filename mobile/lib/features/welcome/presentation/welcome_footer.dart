import 'package:flutter/material.dart';

import '../../../app/app_routes.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/theme/app_colors.dart';
import '../../../l10n/generated/app_localizations.dart';

class WelcomeFooter extends StatelessWidget {
  const WelcomeFooter({super.key});

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return SizedBox(
      height: 140,
      child: Column(
        children: [
          const SizedBox(height: 8),
          AppButton(
            label: strings.createBeneficiaryAccount,
            variant: AppButtonVariant.primary,
            onPressed: () =>
                Navigator.pushNamed(context, AppRoutes.createAccount),
          ),
          const SizedBox(height: 12),
          AppButton(
            label: strings.signInExistingAccount,
            variant: AppButtonVariant.secondary,
            onPressed: () => Navigator.pushNamed(context, AppRoutes.signIn),
          ),
          const Spacer(),
          Container(
            width: 128,
            height: 4,
            decoration: const BoxDecoration(
              color: AppColors.border,
              borderRadius: BorderRadius.all(Radius.circular(999)),
            ),
          ),
        ],
      ),
    );
  }
}
