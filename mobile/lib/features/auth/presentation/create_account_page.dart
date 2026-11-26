import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/widgets/app_card.dart';
import '../../../core/widgets/app_text_field.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../application/create_account_bloc.dart';
import '../domain/beneficiary_registration.dart';
import '../domain/beneficiary_profile_hint.dart';
import '../domain/beneficiary_registration_failure.dart';

class CreateAccountPage extends StatefulWidget {
  const CreateAccountPage({super.key});

  @override
  State<CreateAccountPage> createState() => _CreateAccountPageState();
}

class _CreateAccountPageState extends State<CreateAccountPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _contactController = TextEditingController();
  bool _isPasswordVisible = false;
  bool? _isFamily;
  bool _attemptedSubmit = false;

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _contactController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return Scaffold(
      backgroundColor: AppColors.canvas,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 25),
          child: BlocBuilder<CreateAccountBloc, CreateAccountState>(
            builder: (context, state) => Column(
              children: [
                _buildHeader(context, strings),
                Expanded(
                  child: SizedBox.expand(
                    key: const ValueKey('create-account-form-area'),
                    child: state is CreateAccountSuccess
                        ? _buildSuccess(strings, state.registration)
                        : _buildForm(strings, state),
                  ),
                ),
                _buildFooter(context, strings, state),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeader(BuildContext context, AppLocalizations strings) =>
      SizedBox(
        key: const ValueKey('create-account-header'),
        height: 73,
        child: Row(
          children: [
            IconButton(
              tooltip: strings.back,
              onPressed: () => Navigator.maybePop(context),
              icon: const Icon(Icons.arrow_back),
              color: AppColors.navy,
            ),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                strings.createBeneficiaryAccount,
                style: const TextStyle(
                  color: AppColors.navy,
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ],
        ),
      );

  Widget _buildForm(AppLocalizations strings, CreateAccountState state) {
    final submitting = state is CreateAccountSubmitting;
    return SingleChildScrollView(
      keyboardDismissBehavior: ScrollViewKeyboardDismissBehavior.onDrag,
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              strings.registrationIntro,
              style: const TextStyle(color: AppColors.supportingText),
            ),
            const SizedBox(height: 20),
            AppTextField(
              controller: _nameController,
              enabled: !submitting,
              label: strings.fullName,
              keyboardType: TextInputType.name,
              textInputAction: TextInputAction.next,
              autofillHints: const [AutofillHints.name],
              validator: (value) => _validateName(value, strings),
            ),
            const SizedBox(height: 14),
            AppTextField(
              controller: _emailController,
              enabled: !submitting,
              label: strings.email,
              keyboardType: TextInputType.emailAddress,
              textInputAction: TextInputAction.next,
              autofillHints: const [AutofillHints.email],
              autocorrect: false,
              enableSuggestions: false,
              validator: (value) => _validateEmail(value, strings),
            ),
            const SizedBox(height: 14),
            AppTextField(
              controller: _passwordController,
              enabled: !submitting,
              label: strings.password,
              obscureText: !_isPasswordVisible,
              textInputAction: TextInputAction.next,
              autofillHints: const [AutofillHints.newPassword],
              autocorrect: false,
              enableSuggestions: false,
              suffixIcon: IconButton(
                tooltip: _isPasswordVisible
                    ? strings.hidePassword
                    : strings.showPassword,
                onPressed: submitting
                    ? null
                    : () => setState(
                        () => _isPasswordVisible = !_isPasswordVisible,
                      ),
                icon: Icon(
                  _isPasswordVisible ? Icons.visibility_off : Icons.visibility,
                ),
              ),
              validator: (value) => _validatePassword(value, strings),
            ),
            const SizedBox(height: 14),
            AppTextField(
              controller: _contactController,
              enabled: !submitting,
              label: strings.contactDetailsOptional,
              hintText: strings.contactDetailsHint,
              keyboardType: TextInputType.text,
              textInputAction: TextInputAction.done,
              validator: (value) => _validateContact(value, strings),
            ),
            const SizedBox(height: 18),
            Text(
              strings.profileFor,
              style: const TextStyle(
                color: AppColors.navy,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(
                  child: _familyChoice(
                    label: strings.forMe,
                    value: false,
                    enabled: !submitting,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _familyChoice(
                    label: strings.forFamily,
                    value: true,
                    enabled: !submitting,
                  ),
                ),
              ],
            ),
            if (_attemptedSubmit && _isFamily == null)
              Padding(
                padding: const EdgeInsetsDirectional.only(start: 12, top: 6),
                child: Text(
                  strings.profileForRequired,
                  style: const TextStyle(color: AppColors.error, fontSize: 12),
                ),
              ),
            if (state is CreateAccountFailureState) ...[
              const SizedBox(height: 16),
              _buildError(strings, state.type),
            ],
          ],
        ),
      ),
    );
  }

  Widget _familyChoice({
    required String label,
    required bool value,
    required bool enabled,
  }) => AppCard(
    selected: _isFamily == value,
    onTap: enabled ? () => setState(() => _isFamily = value) : null,
    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 13),
    borderRadius: 12,
    child: Text(label, textAlign: TextAlign.center),
  );

  Widget _buildError(
    AppLocalizations strings,
    BeneficiaryRegistrationFailureType type,
  ) {
    final message = switch (type) {
      BeneficiaryRegistrationFailureType.duplicateEmail =>
        strings.registrationDuplicateEmail,
      BeneficiaryRegistrationFailureType.invalidRequest =>
        strings.registrationInvalidRequest,
      BeneficiaryRegistrationFailureType.apiUnavailable =>
        strings.registrationApiUnavailable,
      BeneficiaryRegistrationFailureType.unexpected =>
        strings.registrationUnexpected,
    };
    return AppCard(
      backgroundColor: AppColors.white,
      padding: const EdgeInsets.all(12),
      borderRadius: 12,
      child: Text(message, style: const TextStyle(color: AppColors.error)),
    );
  }

  Widget _buildSuccess(
    AppLocalizations strings,
    BeneficiaryRegistration registration,
  ) => SingleChildScrollView(
    child: AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Icon(
            Icons.check_circle_outline,
            color: AppColors.teal,
            size: 40,
          ),
          const SizedBox(height: 12),
          Text(
            strings.registrationSuccess,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: AppColors.navy,
              fontSize: 20,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            strings.registrationNextStep,
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppColors.supportingText),
          ),
          const SizedBox(height: 18),
          Text(
            strings.beneficiaryReference(registration.reference),
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: AppColors.teal,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 8),
          Text(registration.email, textAlign: TextAlign.center),
        ],
      ),
    ),
  );

  Widget _buildFooter(
    BuildContext context,
    AppLocalizations strings,
    CreateAccountState state,
  ) {
    if (state is CreateAccountSuccess) {
      return SizedBox(
        key: const ValueKey('create-account-footer'),
        height: 117,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            AppButton(
              label: strings.continueToSignIn,
              onPressed: () => Navigator.pushNamed(
                context,
                AppRoutes.signIn,
                arguments: SignInRouteArguments(
                  email: state.registration.email,
                  profileHint: BeneficiaryProfileHint.fromRegistration(
                    state.registration,
                  ),
                ),
              ),
            ),
          ],
        ),
      );
    }
    return SizedBox(
      key: const ValueKey('create-account-footer'),
      height: 117,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          AppButton(
            label: strings.createAccount,
            onPressed: _submit,
            isLoading: state is CreateAccountSubmitting,
            isEnabled: state is! CreateAccountSubmitting,
          ),
          const SizedBox(height: 10),
          AppButton(
            label: strings.alreadyHaveAccount,
            variant: AppButtonVariant.secondary,
            onPressed: state is CreateAccountSubmitting
                ? null
                : () => Navigator.pushNamed(context, AppRoutes.signIn),
            isEnabled: state is! CreateAccountSubmitting,
          ),
          const Spacer(),
        ],
      ),
    );
  }

  void _submit() {
    setState(() => _attemptedSubmit = true);
    final valid = _formKey.currentState?.validate() ?? false;
    if (!valid) return;
    if (_isFamily == null) {
      setState(() {});
      return;
    }
    context.read<CreateAccountBloc>().add(
      CreateAccountSubmitted(
        BeneficiaryRegistrationRequest(
          displayName: _nameController.text.trim(),
          email: _emailController.text.trim(),
          password: _passwordController.text,
          contactDetails: _contactController.text.trim().isEmpty
              ? null
              : _contactController.text.trim(),
          isFamily: _isFamily!,
        ),
      ),
    );
  }

  String? _validateName(String? value, AppLocalizations strings) {
    final name = value?.trim() ?? '';
    if (name.isEmpty) return strings.nameRequired;
    if (name.length > 200) return strings.nameTooLong;
    return null;
  }

  String? _validateEmail(String? value, AppLocalizations strings) {
    final email = value?.trim() ?? '';
    if (email.isEmpty) return strings.emailRequired;
    if (email.length > 320 ||
        !RegExp(r'^[^\s@]+@[^\s@]+\.[^\s@]+$').hasMatch(email)) {
      return strings.emailInvalid;
    }
    return null;
  }

  String? _validatePassword(String? value, AppLocalizations strings) {
    final password = value ?? '';
    if (password.length < 12 ||
        !RegExp(r'[a-z]').hasMatch(password) ||
        !RegExp(r'[A-Z]').hasMatch(password) ||
        !RegExp(r'\d').hasMatch(password) ||
        !RegExp(r'[^a-zA-Z0-9]').hasMatch(password)) {
      return strings.passwordRequirements;
    }
    return null;
  }

  String? _validateContact(String? value, AppLocalizations strings) =>
      (value?.length ?? 0) > 500 ? strings.contactDetailsTooLong : null;
}
