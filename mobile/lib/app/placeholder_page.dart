import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../l10n/generated/app_localizations.dart';

enum PlaceholderTitle { createAccount, signIn }

class PlaceholderPage extends StatelessWidget {
  const PlaceholderPage({required this.titleKey, super.key});

  final PlaceholderTitle titleKey;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final title = titleKey == PlaceholderTitle.createAccount
        ? strings.createAccount
        : strings.signIn;
    return Scaffold(
      backgroundColor: AppColors.canvas,
      appBar: AppBar(
        backgroundColor: AppColors.canvas,
        foregroundColor: AppColors.navy,
        title: Text(title),
      ),
      body: Center(
        child: Text(
          strings.comingSoon,
          textAlign: TextAlign.center,
          style: const TextStyle(color: AppColors.supportingText),
        ),
      ),
    );
  }
}
