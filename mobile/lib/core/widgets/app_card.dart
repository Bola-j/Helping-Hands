import 'package:flutter/material.dart';

import '../theme/app_colors.dart';

class AppCard extends StatelessWidget {
  const AppCard({
    required this.child,
    this.padding = const EdgeInsets.all(17),
    this.selected = false,
    this.onTap,
    this.backgroundColor = AppColors.white,
    this.borderRadius = 16,
    this.borderWidth,
    super.key,
  });

  final Widget child;
  final EdgeInsetsGeometry padding;
  final bool selected;
  final VoidCallback? onTap;
  final Color backgroundColor;
  final double borderRadius;
  final double? borderWidth;

  @override
  Widget build(BuildContext context) {
    final shape = RoundedRectangleBorder(
      side: BorderSide(
        color: selected ? AppColors.teal : AppColors.border,
        width: borderWidth ?? (selected ? 2 : 1),
      ),
      borderRadius: BorderRadius.all(Radius.circular(borderRadius)),
    );
    return Material(
      color: selected ? AppColors.selectedSurface : backgroundColor,
      shape: shape,
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        customBorder: shape,
        child: Padding(padding: padding, child: child),
      ),
    );
  }
}
