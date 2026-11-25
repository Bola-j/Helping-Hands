import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/app/app_routes.dart';
import 'package:mobile/core/theme/app_theme.dart';
import 'package:mobile/core/widgets/app_button.dart';
import 'package:mobile/features/auth/application/session_bloc.dart';
import 'package:mobile/features/auth/domain/session_failure.dart';
import 'package:mobile/features/auth/domain/session_repository.dart';
import 'package:mobile/features/auth/domain/session_user.dart';
import 'package:mobile/features/auth/presentation/profile_page.dart';
import 'package:mobile/features/auth/presentation/sign_in_page.dart';
import 'package:mobile/features/auth/presentation/session_expired_page.dart';
import 'package:mobile/features/welcome/application/locale_cubit.dart';
import 'package:mobile/l10n/generated/app_localizations.dart';

void main() {
  testWidgets('validates required sign-in fields without calling the API', (
    tester,
  ) async {
    final repository = WidgetSessionRepository();
    await _pumpAtDesignSize(tester, repository);
    await tester.tap(find.byKey(const ValueKey('sign-in-submit')));
    await tester.pumpAndSettle();

    expect(find.text('Enter your email address.'), findsOneWidget);
    expect(find.text('Enter your password.'), findsOneWidget);
    expect(repository.signInCalls, 0);
  });

  testWidgets('sign-in content remains usable at a narrow viewport', (
    tester,
  ) async {
    await _pumpAtDesignSize(
      tester,
      WidgetSessionRepository(),
      size: const Size(320, 640),
    );

    expect(find.byKey(const ValueKey('sign-in-email')), findsOneWidget);
    expect(find.byKey(const ValueKey('sign-in-password')), findsOneWidget);
    expect(find.byKey(const ValueKey('sign-in-submit')), findsOneWidget);
    expect(find.text('Phase 1 · Sign in'), findsOneWidget);
    expect(find.text('Email'), findsOneWidget);
    expect(find.text('Password'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets(
    'shows invalid-credential response without exposing credentials',
    (tester) async {
      final repository = WidgetSessionRepository()
        ..failure = const SessionFailure(SessionFailureType.invalidCredentials);
      await _pumpAtDesignSize(tester, repository);
      await tester.enterText(
        find.byKey(const ValueKey('sign-in-email')),
        'fictional@example.test',
      );
      await tester.enterText(
        find.byKey(const ValueKey('sign-in-password')),
        'fictional-password',
      );
      await tester.tap(find.byKey(const ValueKey('sign-in-submit')));
      await tester.pumpAndSettle();

      expect(
        find.textContaining('Email or password is incorrect'),
        findsOneWidget,
      );
      expect(repository.signInCalls, 1);
    },
  );

  testWidgets('disables submission while the API request is pending', (
    tester,
  ) async {
    final repository = WidgetSessionRepository()
      ..pendingSignIn = Completer<SessionUser>();
    await _pumpAtDesignSize(tester, repository);
    await tester.enterText(
      find.byKey(const ValueKey('sign-in-email')),
      'fictional@example.test',
    );
    await tester.enterText(
      find.byKey(const ValueKey('sign-in-password')),
      'fictional-password',
    );
    await tester.tap(find.byKey(const ValueKey('sign-in-submit')));
    await tester.pump();

    expect(find.byType(CircularProgressIndicator), findsOneWidget);
    expect(
      tester
          .widget<AppButton>(find.byKey(const ValueKey('sign-in-submit')))
          .isLoading,
      isTrue,
    );
    repository.pendingSignIn!.complete(
      const SessionUser(
        id: 'fictional-user',
        displayName: 'Demo Beneficiary',
        email: 'fictional@example.test',
        roles: ['Beneficiary'],
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('Beneficiary home'), findsOneWidget);
  });

  testWidgets('successful sign-in opens Home and can reach account settings', (
    tester,
  ) async {
    final repository = WidgetSessionRepository();
    await _pumpAtDesignSize(tester, repository);
    await tester.enterText(
      find.byKey(const ValueKey('sign-in-email')),
      'fictional@example.test',
    );
    await tester.enterText(
      find.byKey(const ValueKey('sign-in-password')),
      'fictional-password',
    );
    await tester.tap(find.byKey(const ValueKey('sign-in-submit')));
    await tester.pumpAndSettle();

    expect(find.text('Beneficiary home'), findsOneWidget);
    await tester.tap(find.text('Account settings'));
    await tester.pumpAndSettle();
    expect(find.text('Account & settings'), findsOneWidget);
    await tester.tap(find.text('العربية (RTL)'));
    await tester.pumpAndSettle();
    expect(find.text('الحساب والإعدادات'), findsOneWidget);
    await tester.tap(find.text('English (LTR)'));
    await tester.pumpAndSettle();
    expect(find.text('Account & settings'), findsOneWidget);
    await tester.tap(find.text('Sign out'));
    await tester.pumpAndSettle();
    expect(find.text('You are signed out.'), findsOneWidget);
    expect(repository.signOutCalls, 1);
  });

  testWidgets('expired sign-in session routes to recovery screen', (
    tester,
  ) async {
    final repository = WidgetSessionRepository()
      ..failure = const SessionFailure(SessionFailureType.sessionExpired);
    await _pumpAtDesignSize(tester, repository, size: const Size(320, 640));
    await tester.enterText(
      find.byKey(const ValueKey('sign-in-email')),
      'fictional@example.test',
    );
    await tester.enterText(
      find.byKey(const ValueKey('sign-in-password')),
      'fictional-password',
    );
    await tester.tap(find.byKey(const ValueKey('sign-in-submit')));
    await tester.pumpAndSettle();

    expect(
      find.text('You’ve been signed out to protect your account'),
      findsOneWidget,
    );
    expect(
      find.textContaining('Your session ended due to inactivity'),
      findsOneWidget,
    );
    expect(tester.takeException(), isNull);
  });
}

Widget _app(WidgetSessionRepository repository) => MultiRepositoryProvider(
  providers: [RepositoryProvider<SessionRepository>.value(value: repository)],
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
        home: BlocProvider(
          create: (context) => SessionBloc(context.read<SessionRepository>()),
          child: const SignInPage(),
        ),
        onGenerateRoute: (settings) {
          if (settings.name == AppRoutes.home) {
            return MaterialPageRoute<void>(
              settings: settings,
              builder: (context) => Scaffold(
                body: Center(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Text('Beneficiary home'),
                      TextButton(
                        onPressed: () => Navigator.pushNamed(
                          context,
                          AppRoutes.profile,
                          arguments: settings.arguments as SessionUser,
                        ),
                        child: const Text('Account settings'),
                      ),
                    ],
                  ),
                ),
              ),
            );
          }
          if (settings.name == AppRoutes.profile) {
            return MaterialPageRoute<void>(
              settings: settings,
              builder: (context) => BlocProvider(
                create: (context) => SessionBloc(
                  context.read<SessionRepository>(),
                  initialUser: settings.arguments as SessionUser,
                ),
                child: const ProfilePage(),
              ),
            );
          }
          if (settings.name == AppRoutes.sessionExpired) {
            return MaterialPageRoute<void>(
              settings: settings,
              builder: (_) => const SessionExpiredPage(),
            );
          }
          return null;
        },
      ),
    ),
  ),
);

Future<void> _pumpAtDesignSize(
  WidgetTester tester,
  WidgetSessionRepository repository, {
  Size size = const Size(390, 884),
}) async {
  tester.view.devicePixelRatio = 1;
  tester.view.physicalSize = size;
  addTearDown(tester.view.resetPhysicalSize);
  addTearDown(tester.view.resetDevicePixelRatio);
  await tester.pumpWidget(_app(repository));
}

class WidgetSessionRepository implements SessionRepository {
  SessionFailure? failure;
  Completer<SessionUser>? pendingSignIn;
  int signInCalls = 0;
  int signOutCalls = 0;

  @override
  Future<SessionUser> signIn(Map<String, Object?> credentials) async {
    signInCalls++;
    if (pendingSignIn case final completer?) return completer.future;
    if (failure case final value?) throw value;
    return const SessionUser(
      id: 'fictional-user',
      displayName: 'Demo Beneficiary',
      email: 'fictional@example.test',
      roles: ['Beneficiary'],
    );
  }

  @override
  Future<void> signOut() async {
    signOutCalls++;
  }

  @override
  Future<SessionUser> currentUser() async => throw UnimplementedError();

  @override
  Future<void> registerBeneficiary(Map<String, Object?> input) async {}
}
