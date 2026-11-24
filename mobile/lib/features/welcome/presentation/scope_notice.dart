import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_card.dart';

class ScopeNotice extends StatelessWidget {
  const ScopeNotice({required this.text, super.key});

  final String text;

  @override
  Widget build(BuildContext context) {
    return AppCard(
      backgroundColor: AppColors.scopeSurface,
      child: Row(
        textDirection: TextDirection.ltr,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Directionality(
              textDirection: TextDirection.rtl,
              child: Text(
                text,
                textAlign: TextAlign.right,
                style: const TextStyle(
                  color: AppColors.navy,
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
          ),
          const SizedBox(width: 12),
          SvgPicture.asset('assets/icons/scope-shield.svg'),
        ],
      ),
    );
  }
}
