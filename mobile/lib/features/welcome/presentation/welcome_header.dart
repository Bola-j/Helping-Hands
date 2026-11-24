import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../core/theme/app_colors.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../application/locale_cubit.dart';

class WelcomeHeader extends StatelessWidget {
  const WelcomeHeader({super.key});

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 48,
      child: Padding(
        padding: const EdgeInsets.only(top: 8),
        child: Row(
          textDirection: TextDirection.ltr,
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            const _LanguageSwitch(),
            Flexible(
              child: Align(
                alignment: Alignment.centerRight,
                child: FittedBox(
                  fit: BoxFit.scaleDown,
                  alignment: Alignment.centerRight,
                  child: _PhaseBadge(
                    text: AppLocalizations.of(context).phaseBadge,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LanguageSwitch extends StatelessWidget {
  const _LanguageSwitch();

  @override
  Widget build(BuildContext context) {
    final locale = Localizations.localeOf(context);
    final nextLocale = locale.languageCode == 'ar'
        ? const Locale('en')
        : const Locale('ar');
    return Semantics(
      button: true,
      label: AppLocalizations.of(context).switchLanguage,
      child: Material(
        color: AppColors.white,
        shape: const StadiumBorder(side: BorderSide(color: AppColors.border)),
        child: InkWell(
          customBorder: const StadiumBorder(),
          onTap: () => context.read<LocaleCubit>().select(nextLocale),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 15),
            child: SizedBox(
              height: 40,
              child: Row(
                textDirection: TextDirection.ltr,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    AppLocalizations.of(context).switchLanguage,
                    style: const TextStyle(
                      color: AppColors.navy,
                      fontFamily: 'Cabin',
                      fontSize: 13,
                      height: 18 / 13,
                      letterSpacing: 0.2,
                      fontVariations: [
                        FontVariation('wght', 600),
                        FontVariation('wdth', 75),
                      ],
                    ),
                  ),
                  const SizedBox(width: 6),
                  SvgPicture.asset('assets/icons/language-globe.svg'),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _PhaseBadge extends StatelessWidget {
  const _PhaseBadge({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: AppColors.phaseBadge,
        borderRadius: BorderRadius.all(Radius.circular(9999)),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      child: Row(
        textDirection: TextDirection.ltr,
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            text,
            textAlign: TextAlign.right,
            style: const TextStyle(
              color: AppColors.phaseText,
              fontFamily: 'Cabin',
              fontSize: 12,
              height: 16 / 12,
              letterSpacing: 0.2,
              fontVariations: [
                FontVariation('wght', 500),
                FontVariation('wdth', 75),
              ],
            ),
          ),
          const SizedBox(width: 6),
          Container(
            width: 8,
            height: 8,
            decoration: const BoxDecoration(
              color: AppColors.teal,
              shape: BoxShape.circle,
            ),
          ),
        ],
      ),
    );
  }
}
