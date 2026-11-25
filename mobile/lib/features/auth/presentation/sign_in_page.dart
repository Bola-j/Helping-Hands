import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../app/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_button.dart';
import '../../../core/widgets/app_text_field.dart';
import '../../../core/widgets/flow_screen_header.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../application/session_bloc.dart';
import '../domain/session_failure.dart';

class SignInPage extends StatefulWidget {
  const SignInPage({super.key, this.initialEmail});

  final String? initialEmail;

  @override
  State<SignInPage> createState() => _SignInPageState();
}

class _SignInPageState extends State<SignInPage> {
  final _formKey = GlobalKey<FormState>();
  late final _email = TextEditingController(text: widget.initialEmail ?? '');
  final _password = TextEditingController();
  bool _passwordVisible = false;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return Scaffold(
      backgroundColor: AppColors.canvas,
      body: SafeArea(
        child: Column(
          children: [
            FlowScreenHeader(phaseLabel: strings.phaseSignIn),
            Expanded(
              child: BlocListener<SessionBloc, SessionState>(
                listenWhen: (_, state) =>
                    state is SessionSignedIn ||
                    (state is SessionSignedOut &&
                        state.failure == SessionFailureType.sessionExpired),
                listener: (context, state) {
                  if (state case SessionSignedIn(:final user)) {
                    Navigator.pushReplacementNamed(
                      context,
                      AppRoutes.home,
                      arguments: user,
                    );
                  } else if (state is SessionSignedOut &&
                      state.failure == SessionFailureType.sessionExpired) {
                    Navigator.pushReplacementNamed(
                      context,
                      AppRoutes.sessionExpired,
                    );
                  }
                },
                child: BlocBuilder<SessionBloc, SessionState>(
                  builder: (context, state) => Padding(
                    padding: const EdgeInsets.fromLTRB(16, 20, 16, 8),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Text(
                          strings.signInTitle,
                          textAlign: TextAlign.end,
                          style: const TextStyle(
                            color: AppColors.navy,
                            fontSize: 24,
                            height: 1.3,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          strings.signInSubtitle,
                          textAlign: TextAlign.end,
                          style: const TextStyle(
                            color: AppColors.supportingText,
                            fontSize: 13,
                            height: 1.45,
                          ),
                        ),
                        const SizedBox(height: 28),
                        Expanded(
                          child: SingleChildScrollView(
                            keyboardDismissBehavior:
                                ScrollViewKeyboardDismissBehavior.onDrag,
                            child: Form(
                              key: _formKey,
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: [
                                  _FieldLabel(text: strings.email),
                                  const SizedBox(height: 6),
                                  AppTextField(
                                    key: const ValueKey('sign-in-email'),
                                    controller: _email,
                                    enabled: state is! SessionSubmitting,
                                    label: strings.email,
                                    labelAbove: true,
                                    keyboardType: TextInputType.emailAddress,
                                    textInputAction: TextInputAction.next,
                                    autofillHints: const [
                                      AutofillHints.username,
                                      AutofillHints.email,
                                    ],
                                    autocorrect: false,
                                    enableSuggestions: false,
                                    validator: (value) =>
                                        _validEmail(value, strings),
                                  ),
                                  const SizedBox(height: 24),
                                  _FieldLabel(text: strings.password),
                                  const SizedBox(height: 6),
                                  AppTextField(
                                    key: const ValueKey('sign-in-password'),
                                    controller: _password,
                                    enabled: state is! SessionSubmitting,
                                    label: strings.password,
                                    labelAbove: true,
                                    obscureText: !_passwordVisible,
                                    textInputAction: TextInputAction.done,
                                    autofillHints: const [
                                      AutofillHints.password,
                                    ],
                                    autocorrect: false,
                                    enableSuggestions: false,
                                    suffixIcon: IconButton(
                                      tooltip: _passwordVisible
                                          ? strings.hidePassword
                                          : strings.showPassword,
                                      onPressed: state is SessionSubmitting
                                          ? null
                                          : () => setState(
                                              () => _passwordVisible =
                                                  !_passwordVisible,
                                            ),
                                      icon: Icon(
                                        _passwordVisible
                                            ? Icons.visibility_off
                                            : Icons.visibility,
                                      ),
                                    ),
                                    validator: (value) =>
                                        value == null || value.isEmpty
                                        ? strings.passwordRequired
                                        : null,
                                  ),
                                  if (state case SessionSignedOut(
                                    :final failure?,
                                  )) ...[
                                    const SizedBox(height: 18),
                                    _message(strings, failure),
                                  ],
                                ],
                              ),
                            ),
                          ),
                        ),
                        AppButton(
                          key: const ValueKey('sign-in-submit'),
                          label: strings.signIn,
                          isLoading: state is SessionSubmitting,
                          onPressed: () => _submit(context),
                        ),
                        const SizedBox(height: 8),
                        Wrap(
                          alignment: WrapAlignment.center,
                          crossAxisAlignment: WrapCrossAlignment.center,
                          children: [
                            Text(
                              strings.noBeneficiaryAccount,
                              style: const TextStyle(
                                color: AppColors.supportingText,
                                fontSize: 13,
                              ),
                            ),
                            TextButton(
                              onPressed: state is SessionSubmitting
                                  ? null
                                  : () => Navigator.pushNamed(
                                      context,
                                      AppRoutes.createAccount,
                                    ),
                              style: TextButton.styleFrom(
                                foregroundColor: AppColors.teal,
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 6,
                                ),
                                minimumSize: const Size(0, 40),
                              ),
                              child: Text(strings.createBeneficiaryAccount),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _submit(BuildContext context) {
    if (!_formKey.currentState!.validate()) return;
    context.read<SessionBloc>().add(
      SignInSubmitted(email: _email.text, password: _password.text),
    );
  }

  String? _validEmail(String? value, AppLocalizations strings) {
    final email = value?.trim() ?? '';
    if (email.isEmpty) return strings.emailRequired;
    return RegExp(r'^[^\s@]+@[^\s@]+\.[^\s@]+$').hasMatch(email)
        ? null
        : strings.invalidEmail;
  }

  Widget _message(AppLocalizations strings, SessionFailureType failure) =>
      Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: AppColors.error.withValues(alpha: .08),
          borderRadius: BorderRadius.circular(12),
        ),
        child: Text(
          _failureMessage(strings, failure),
          style: const TextStyle(color: AppColors.error),
          textAlign: TextAlign.start,
        ),
      );

  String _failureMessage(AppLocalizations strings, SessionFailureType type) =>
      switch (type) {
        SessionFailureType.invalidCredentials =>
          strings.signInInvalidCredentials,
        SessionFailureType.sessionExpired => strings.signInExpired,
        SessionFailureType.denied => strings.signInDenied,
        SessionFailureType.apiUnavailable => strings.signInUnavailable,
        SessionFailureType.invalidRequest => strings.signInInvalidRequest,
        _ => strings.signInUnexpected,
      };
}

class _FieldLabel extends StatelessWidget {
  const _FieldLabel({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(horizontal: 4),
    child: Text(
      text,
      textAlign: TextAlign.end,
      style: const TextStyle(
        color: AppColors.navy,
        fontSize: 13,
        height: 1.3,
        fontWeight: FontWeight.w600,
      ),
    ),
  );
}
