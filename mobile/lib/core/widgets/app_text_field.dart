import 'package:flutter/material.dart';

import '../theme/app_colors.dart';

class AppTextField extends StatelessWidget {
  const AppTextField({
    required this.label,
    this.controller,
    this.hintText,
    this.errorText,
    this.enabled = true,
    this.keyboardType,
    this.onChanged,
    this.obscureText = false,
    this.validator,
    this.suffixIcon,
    this.textInputAction,
    this.autofillHints,
    this.autocorrect = true,
    this.enableSuggestions = true,
    this.labelAbove = false,
    this.minLines = 1,
    this.maxLines = 1,
    this.maxLength,
    super.key,
  });

  final String label;
  final TextEditingController? controller;
  final String? hintText;
  final String? errorText;
  final bool enabled;
  final TextInputType? keyboardType;
  final ValueChanged<String>? onChanged;
  final bool obscureText;
  final FormFieldValidator<String>? validator;
  final Widget? suffixIcon;
  final TextInputAction? textInputAction;
  final Iterable<String>? autofillHints;
  final bool autocorrect;
  final bool enableSuggestions;
  final bool labelAbove;
  final int minLines;
  final int? maxLines;
  final int? maxLength;

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: labelAbove ? label : null,
      child: TextFormField(
        controller: controller,
        enabled: enabled,
        keyboardType: keyboardType,
        onChanged: onChanged,
        obscureText: obscureText,
        minLines: minLines,
        maxLines: maxLines,
        maxLength: maxLength,
        validator: validator,
        textInputAction: textInputAction,
        autofillHints: autofillHints,
        autocorrect: autocorrect,
        enableSuggestions: enableSuggestions,
        style: const TextStyle(
          color: AppColors.navy,
          fontFamily: 'Cabin',
          fontFamilyFallback: ['NotoSansArabic'],
          fontSize: 16,
        ),
        decoration: InputDecoration(
          labelText: labelAbove ? null : label,
          hintText: hintText,
          errorText: errorText,
          counterText: maxLength == null ? null : '',
          suffixIcon: suffixIcon,
          filled: true,
          fillColor: enabled ? AppColors.white : AppColors.canvas,
          contentPadding: const EdgeInsets.symmetric(
            horizontal: 16,
            vertical: 14,
          ),
          border: const OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
            borderSide: BorderSide(color: AppColors.border),
          ),
          enabledBorder: const OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
            borderSide: BorderSide(color: AppColors.border),
          ),
          focusedBorder: const OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
            borderSide: BorderSide(color: AppColors.teal, width: 2),
          ),
          errorBorder: const OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
            borderSide: BorderSide(color: AppColors.error),
          ),
        ),
      ),
    );
  }
}
