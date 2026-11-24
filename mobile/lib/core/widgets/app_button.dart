import 'package:flutter/material.dart';

import '../theme/app_colors.dart';

enum AppButtonVariant { primary, secondary }

class AppButton extends StatelessWidget {
  const AppButton({
    required this.label,
    required this.onPressed,
    this.variant = AppButtonVariant.primary,
    this.isLoading = false,
    this.isEnabled = true,
    super.key,
  });

  final String label;
  final VoidCallback? onPressed;
  final AppButtonVariant variant;
  final bool isLoading;
  final bool isEnabled;

  @override
  Widget build(BuildContext context) {
    final primary = variant == AppButtonVariant.primary;
    final foreground = primary ? AppColors.white : AppColors.teal;
    return SizedBox(
      width: double.infinity,
      height: 48,
      child: OutlinedButton(
        onPressed: isEnabled && !isLoading ? onPressed : null,
        style: OutlinedButton.styleFrom(
          backgroundColor: primary ? AppColors.teal : AppColors.white,
          foregroundColor: foreground,
          disabledBackgroundColor: primary
              ? AppColors.teal.withValues(alpha: 0.55)
              : AppColors.white,
          disabledForegroundColor: foreground.withValues(alpha: 0.65),
          side: BorderSide(color: AppColors.teal, width: primary ? 0 : 1),
          shape: const RoundedRectangleBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 16),
          textStyle: const TextStyle(
            fontFamily: 'Cabin',
            fontFamilyFallback: ['NotoSansArabic'],
            fontSize: 16,
            height: 24 / 16,
            fontWeight: FontWeight.w600,
          ),
        ),
        child: isLoading
            ? Semantics(
                label: label,
                liveRegion: true,
                child: SizedBox.square(
                  dimension: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: foreground,
                  ),
                ),
              )
            : Text(label, textAlign: TextAlign.center),
      ),
    );
  }
}
