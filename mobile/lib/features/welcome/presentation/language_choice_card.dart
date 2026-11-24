import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../core/widgets/app_card.dart';
import '../../../core/theme/app_colors.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../application/locale_cubit.dart';

class LanguageChoiceCard extends StatelessWidget {
  const LanguageChoiceCard({super.key});

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final selectedLanguage = Localizations.localeOf(context).languageCode;
    return AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Align(
            alignment: Alignment.centerRight,
            child: Text(
              strings.chooseLanguage,
              textAlign: TextAlign.right,
              style: const TextStyle(
                color: AppColors.navy,
                fontFamily: 'Cabin',
                fontFamilyFallback: ['NotoSansArabic'],
                fontSize: 16,
                height: 24 / 16,
                fontVariations: [
                  FontVariation('wght', 600),
                  FontVariation('wdth', 75),
                ],
              ),
            ),
          ),
          const SizedBox(height: 12),
          Row(
            textDirection: TextDirection.ltr,
            children: [
              Expanded(
                child: _LanguageOption(
                  languageCode: 'en',
                  label: strings.english,
                  selected: selectedLanguage == 'en',
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _LanguageOption(
                  languageCode: 'ar',
                  label: strings.arabic,
                  selected: selectedLanguage == 'ar',
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _LanguageOption extends StatelessWidget {
  const _LanguageOption({
    required this.languageCode,
    required this.label,
    required this.selected,
  });

  final String languageCode;
  final String label;
  final bool selected;

  @override
  Widget build(BuildContext context) {
    final borderWidth = selected ? 2.0 : 1.0;
    return Semantics(
      button: true,
      selected: selected,
      label: label,
      child: AppCard(
        padding: EdgeInsets.zero,
        borderRadius: 12,
        borderWidth: borderWidth,
        selected: selected,
        onTap: () => context.read<LocaleCubit>().select(Locale(languageCode)),
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
                    style: TextStyle(
                      color: selected ? AppColors.selectedText : AppColors.navy,
                      fontFamily: languageCode == 'ar' && selected
                          ? 'FreeSerif'
                          : 'Cabin',
                      fontFamilyFallback: languageCode == 'ar'
                          ? ['NotoSansArabic']
                          : null,
                      fontSize: 15,
                      height: 20 / 15,
                      fontVariations: languageCode == 'ar' && selected
                          ? null
                          : const [
                              FontVariation('wght', 600),
                              FontVariation('wdth', 75),
                            ],
                    ),
                  ),
                  if (selected) ...[
                    const SizedBox(width: 8),
                    SvgPicture.asset('assets/icons/selected-check.svg'),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
