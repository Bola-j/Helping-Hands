import 'package:flutter/material.dart';

import 'app_colors.dart';

abstract final class AppTheme {
  static ThemeData get light => ThemeData(
    useMaterial3: true,
    fontFamily: 'Cabin',
    fontFamilyFallback: const ['NotoSansArabic'],
    scaffoldBackgroundColor: AppColors.canvas,
    colorScheme: ColorScheme.fromSeed(
      seedColor: AppColors.teal,
      primary: AppColors.teal,
      surface: AppColors.canvas,
      onPrimary: AppColors.white,
      onSurface: AppColors.navy,
    ),
    splashFactory: InkRipple.splashFactory,
  );
}
