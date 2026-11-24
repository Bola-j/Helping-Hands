import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../core/theme/app_colors.dart';
import '../../../l10n/generated/app_localizations.dart';
import 'language_choice_card.dart';
import 'scope_notice.dart';

class WelcomeContent extends StatelessWidget {
  const WelcomeContent({super.key});

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _WelcomeHero(
            title: strings.welcomeTitle,
            description: strings.welcomeSubtitle,
          ),
          const SizedBox(height: 24),
          const LanguageChoiceCard(),
          const SizedBox(height: 24),
          ScopeNotice(text: strings.scopeNotice),
        ],
      ),
    );
  }
}

class _WelcomeHero extends StatelessWidget {
  const _WelcomeHero({required this.title, required this.description});

  final String title;
  final String description;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Align(
            alignment: Alignment.centerRight,
            child: DecoratedBox(
              decoration: BoxDecoration(
                color: AppColors.teal,
                borderRadius: BorderRadius.all(Radius.circular(16)),
                boxShadow: [
                  BoxShadow(
                    color: Color(0x0D000000),
                    offset: Offset(0, 1),
                    blurRadius: 1,
                  ),
                ],
              ),
              child: SizedBox(
                width: 56,
                height: 56,
                child: Center(
                  child: SvgPicture.asset('assets/icons/helping-hands.svg'),
                ),
              ),
            ),
          ),
          const SizedBox(height: 16),
          Align(
            alignment: Alignment.centerRight,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  textAlign: TextAlign.right,
                  maxLines: 1,
                  softWrap: false,
                  style: const TextStyle(
                    color: AppColors.navy,
                    fontFamily: 'NotoSansArabic',
                    fontSize: 28,
                    height: 36 / 28,
                    letterSpacing: -0.25,
                    fontVariations: [
                      FontVariation('wght', 700),
                      FontVariation('wdth', 62.5),
                    ],
                  ),
                ),
                const SizedBox(height: 8),
                ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 324),
                  child: Text(
                    description,
                    textAlign: TextAlign.right,
                    style: const TextStyle(
                      color: AppColors.supportingText,
                      fontFamily: 'Cabin',
                      fontFamilyFallback: ['NotoSansArabic'],
                      fontSize: 14,
                      height: 21 / 14,
                      fontVariations: [
                        FontVariation('wght', 400),
                        FontVariation('wdth', 62.5),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
