import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../features/welcome/application/locale_cubit.dart';
import '../../l10n/generated/app_localizations.dart';
import '../theme/app_colors.dart';

class FlowScreenHeader extends StatelessWidget {
  const FlowScreenHeader({
    required this.phaseLabel,
    this.showBack = true,
    super.key,
  });

  final String phaseLabel;
  final bool showBack;

  @override
  Widget build(BuildContext context) {
    final locale = Localizations.localeOf(context);
    final nextLocale = locale.languageCode == 'ar'
        ? const Locale('en')
        : const Locale('ar');
    final strings = AppLocalizations.of(context);
    return Container(
      height: 72,
      padding: const EdgeInsets.only(top: 8),
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: AppColors.border)),
      ),
      child: SizedBox(
        height: 56,
        child: Stack(
          alignment: Alignment.center,
          children: [
            Positioned.fill(
              child: Center(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 58),
                  child: _PhasePill(label: phaseLabel),
                ),
              ),
            ),
            Row(
              textDirection: TextDirection.ltr,
              children: [
                _LanguagePill(
                  languageCode: locale.languageCode,
                  onPressed: () =>
                      context.read<LocaleCubit>().select(nextLocale),
                ),
                const Spacer(),
                if (showBack)
                  SizedBox.square(
                    dimension: 48,
                    child: OutlinedButton(
                      onPressed: () => Navigator.maybePop(context),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: AppColors.navy,
                        side: const BorderSide(color: AppColors.border),
                        shape: const CircleBorder(),
                        padding: EdgeInsets.zero,
                      ),
                      child: Icon(
                        Icons.arrow_back,
                        semanticLabel: strings.back,
                        textDirection: Directionality.of(context),
                      ),
                    ),
                  )
                else
                  const SizedBox(width: 48),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _LanguagePill extends StatelessWidget {
  const _LanguagePill({required this.languageCode, required this.onPressed});

  final String languageCode;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) => OutlinedButton(
    onPressed: onPressed,
    style: OutlinedButton.styleFrom(
      foregroundColor: AppColors.navy,
      backgroundColor: AppColors.white,
      side: const BorderSide(color: AppColors.border),
      shape: const StadiumBorder(),
      minimumSize: const Size(44, 36),
      padding: const EdgeInsets.symmetric(horizontal: 12),
    ),
    child: Text(
      languageCode.toUpperCase(),
      style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600),
    ),
  );
}

class _PhasePill extends StatelessWidget {
  const _PhasePill({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) => Container(
    constraints: const BoxConstraints(maxWidth: 190),
    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 5),
    decoration: const BoxDecoration(
      color: AppColors.selectedSurface,
      borderRadius: BorderRadius.all(Radius.circular(999)),
    ),
    child: Text(
      label,
      maxLines: 1,
      overflow: TextOverflow.ellipsis,
      textAlign: TextAlign.center,
      style: const TextStyle(
        color: AppColors.selectedText,
        fontSize: 11,
        height: 1.2,
        fontWeight: FontWeight.w500,
      ),
    ),
  );
}
