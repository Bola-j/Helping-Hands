import 'package:flutter/material.dart';

import '../../l10n/generated/app_localizations.dart';
import '../theme/app_colors.dart';

enum BeneficiaryNavigationItem { home, requests, account }

class BeneficiaryBottomNavigation extends StatelessWidget {
  const BeneficiaryBottomNavigation({
    required this.selected,
    this.onSelected,
    super.key,
  });

  final BeneficiaryNavigationItem selected;
  final ValueChanged<BeneficiaryNavigationItem>? onSelected;

  @override
  Widget build(BuildContext context) {
    final appStrings = AppLocalizations.of(context);
    return Container(
      decoration: const BoxDecoration(
        color: AppColors.white,
        border: Border(top: BorderSide(color: AppColors.border)),
      ),
      child: SafeArea(
        top: false,
        child: SizedBox(
          height: 76,
          child: Row(
            children: [
              _NavigationEntry(
                icon: Icons.home_outlined,
                selectedIcon: Icons.home,
                label: appStrings.homeNavigation,
                item: BeneficiaryNavigationItem.home,
                selected: selected == BeneficiaryNavigationItem.home,
                onTap: onSelected,
              ),
              _NavigationEntry(
                icon: Icons.fact_check_outlined,
                selectedIcon: Icons.fact_check,
                label: appStrings.requestsNavigation,
                item: BeneficiaryNavigationItem.requests,
                selected: selected == BeneficiaryNavigationItem.requests,
                onTap: onSelected,
              ),
              _NavigationEntry(
                icon: Icons.person_outline,
                selectedIcon: Icons.person,
                label: appStrings.accountNavigation,
                item: BeneficiaryNavigationItem.account,
                selected: selected == BeneficiaryNavigationItem.account,
                onTap: onSelected,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _NavigationEntry extends StatelessWidget {
  const _NavigationEntry({
    required this.icon,
    required this.selectedIcon,
    required this.label,
    required this.item,
    required this.selected,
    required this.onTap,
  });

  final IconData icon;
  final IconData selectedIcon;
  final String label;
  final BeneficiaryNavigationItem item;
  final bool selected;
  final ValueChanged<BeneficiaryNavigationItem>? onTap;

  @override
  Widget build(BuildContext context) {
    final color = selected ? AppColors.teal : AppColors.supportingText;
    return Expanded(
      child: Semantics(
        button: onTap != null,
        selected: selected,
        label: label,
        child: InkWell(
          onTap: onTap == null ? null : () => onTap!(item),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(selected ? selectedIcon : icon, color: color, size: 22),
              const SizedBox(height: 4),
              Text(label, style: TextStyle(color: color, fontSize: 12)),
            ],
          ),
        ),
      ),
    );
  }
}
