import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/app/app_routes.dart';
import 'package:mobile/core/theme/app_theme.dart';
import 'package:mobile/features/auth/application/session_bloc.dart';
import 'package:mobile/features/auth/domain/session_failure.dart';
import 'package:mobile/features/auth/domain/session_repository.dart';
import 'package:mobile/features/auth/domain/session_user.dart';
import 'package:mobile/features/auth/presentation/profile_page.dart';
import 'package:mobile/features/welcome/application/locale_cubit.dart';
import 'package:mobile/l10n/generated/app_localizations.dart';

void main() {
  testWidgets('sign-out outage clears profile UI and offers a safe next step', (
    tester,
  ) async {
    final repository = ProfileSessionRepository()
      ..signOutFailure = const SessionFailure(
        SessionFailureType.apiUnavailable,
      );
    await tester.pumpWidget(_profileApp(repository));

    expect(find.text('beneficiary@example.test'), findsOneWidget);
    await tester.tap(find.byKey(const ValueKey('sign-out-button')));
    await tester.pumpAndSettle();

    expect(repository.signOutCalls, 1);
    expect(
      find.text(
        'We could not confirm sign out with the service. Please try again.',
      ),
      findsOneWidget,
    );
    expect(find.text('beneficiary@example.test'), findsNothing);
    expect(find.text('You are signed out.'), findsOneWidget);
    expect(find.text('Go to sign in'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}

Widget _profileApp(ProfileSessionRepository repository) =>
    RepositoryProvider<SessionRepository>.value(
      value: repository,
      child: BlocProvider(
        create: (context) => SessionBloc(
          context.read<SessionRepository>(),
          initialUser: _beneficiary,
        ),
        child: BlocProvider(
          create: (_) => LocaleCubit()..select(const Locale('en')),
          child: BlocBuilder<LocaleCubit, Locale>(
            builder: (context, locale) => MaterialApp(
              locale: locale,
              theme: AppTheme.light,
              localizationsDelegates: const [
                AppLocalizations.delegate,
                GlobalMaterialLocalizations.delegate,
                GlobalWidgetsLocalizations.delegate,
                GlobalCupertinoLocalizations.delegate,
              ],
              supportedLocales: AppLocalizations.supportedLocales,
              home: const ProfilePage(),
              onGenerateRoute: (settings) => settings.name == AppRoutes.signIn
                  ? MaterialPageRoute<void>(
                      builder: (_) =>
                          const Scaffold(body: Text('Sign in after sign-out')),
                    )
                  : null,
            ),
          ),
        ),
      ),
    );

const _beneficiary = SessionUser(
  id: 'fictional-account',
  displayName: 'Fictional Beneficiary',
  email: 'beneficiary@example.test',
  roles: ['Beneficiary'],
);

class ProfileSessionRepository implements SessionRepository {
  SessionFailure? signOutFailure;
  int signOutCalls = 0;

  @override
  Future<SessionUser> currentUser() async => _beneficiary;

  @override
  Future<void> registerBeneficiary(Map<String, Object?> input) async {}

  @override
  Future<SessionUser> signIn(Map<String, Object?> credentials) async =>
      _beneficiary;

  @override
  Future<void> signOut() async {
    signOutCalls++;
    if (signOutFailure case final failure?) throw failure;
  }
}
