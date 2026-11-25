import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/app/app_routes.dart';
import 'package:mobile/features/auth/application/create_account_bloc.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration_failure.dart';
import 'package:mobile/features/auth/domain/beneficiary_registration_repository.dart';
import 'package:mobile/features/auth/presentation/create_account_page.dart';
import 'package:mobile/l10n/generated/app_localizations.dart';

void main() {
  testWidgets('matches the measured Create Account frame at 390 by 884', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(390, 884);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    await _pumpPage(tester, FakeRegistrationRepository());

    expect(
      tester.getRect(find.byKey(const ValueKey('create-account-header'))),
      const Rect.fromLTWH(16, 16, 358, 73),
    );
    expect(
      tester.getRect(find.byKey(const ValueKey('create-account-form-area'))),
      const Rect.fromLTWH(16, 89, 358, 653),
    );
    expect(
      tester.getRect(find.byKey(const ValueKey('create-account-footer'))),
      const Rect.fromLTWH(16, 742, 358, 117),
    );
    expect(tester.takeException(), isNull);
  });

  testWidgets('validates API-required fields and explicit profile choice', (
    tester,
  ) async {
    final repository = FakeRegistrationRepository();
    await _pumpPage(tester, repository);

    await tester.tap(find.text('Create account').last);
    await tester.pumpAndSettle();

    expect(find.text('Enter your name.'), findsOneWidget);
    expect(find.text('Enter your email address.'), findsOneWidget);
    expect(find.text('Choose who the profile represents.'), findsOneWidget);
    expect(repository.submitted, isEmpty);
  });

  testWidgets('shows returned reference and navigates separately to sign in', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(390, 884);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    final repository = FakeRegistrationRepository();
    await _pumpPage(tester, repository);

    await tester.enterText(
      find.byType(TextFormField).at(0),
      'Fictional person',
    );
    await tester.enterText(
      find.byType(TextFormField).at(1),
      'person@example.test',
    );
    await tester.enterText(
      find.byType(TextFormField).at(2),
      'FictionalPass#123',
    );
    await tester.enterText(
      find.byType(TextFormField).at(3),
      'Fictional contact',
    );
    await tester.tap(find.text('My family'));
    await tester.tap(find.text('Create account').last);
    await tester.pumpAndSettle();

    expect(repository.submitted.single.isFamily, isTrue);
    expect(repository.submitted.single.email, 'person@example.test');
    expect(find.text('Beneficiary reference: BEN-00000001'), findsOneWidget);
    expect(
      find.textContaining('Continue to sign in with your email'),
      findsOneWidget,
    );
    expect(find.text('Continue to sign in'), findsOneWidget);

    await tester.tap(find.text('Continue to sign in'));
    await tester.pumpAndSettle();
    expect(find.text('Sign in'), findsOneWidget);
  });

  testWidgets('password visibility can be toggled', (tester) async {
    final repository = FakeRegistrationRepository();
    await _pumpPage(tester, repository);

    var password = tester.widget<TextField>(find.byType(TextField).at(2));
    expect(password.obscureText, isTrue);
    await tester.tap(find.byTooltip('Show password'));
    await tester.pump();
    password = tester.widget<TextField>(find.byType(TextField).at(2));
    expect(password.obscureText, isFalse);
  });

  testWidgets('shows busy and API-unavailable states during submission', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(320, 640);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    final repository = FakeRegistrationRepository()
      ..pending = Completer<BeneficiaryRegistration>();
    await _pumpPage(tester, repository);

    await tester.enterText(
      find.byType(TextFormField).at(0),
      'Fictional person',
    );
    await tester.enterText(
      find.byType(TextFormField).at(1),
      'person@example.test',
    );
    await tester.enterText(
      find.byType(TextFormField).at(2),
      'FictionalPass#123',
    );
    await tester.ensureVisible(find.text('Me'));
    await tester.tap(find.text('Me'));
    await tester.tap(find.text('Create account').last);
    await tester.pump();

    expect(find.byType(CircularProgressIndicator), findsOneWidget);
    expect(tester.takeException(), isNull);
    repository.pending!.completeError(
      const BeneficiaryRegistrationFailure(
        BeneficiaryRegistrationFailureType.apiUnavailable,
      ),
    );
    await tester.pumpAndSettle();

    expect(
      find.text(
        'Registration is temporarily unavailable. Check your connection and try again.',
      ),
      findsOneWidget,
    );
    expect(tester.takeException(), isNull);
  });
}

Future<void> _pumpPage(
  WidgetTester tester,
  FakeRegistrationRepository repository,
) async {
  await tester.pumpWidget(
    RepositoryProvider<BeneficiaryRegistrationRepository>.value(
      value: repository,
      child: BlocProvider(
        create: (context) => CreateAccountBloc(
          context.read<BeneficiaryRegistrationRepository>(),
        ),
        child: MaterialApp(
          locale: const Locale('en'),
          localizationsDelegates: AppLocalizations.localizationsDelegates,
          supportedLocales: AppLocalizations.supportedLocales,
          routes: {
            AppRoutes.signIn: (_) => const Scaffold(body: Text('Sign in')),
          },
          home: const CreateAccountPage(),
        ),
      ),
    ),
  );
  await tester.pumpAndSettle();
}

class FakeRegistrationRepository implements BeneficiaryRegistrationRepository {
  final submitted = <BeneficiaryRegistrationRequest>[];
  Completer<BeneficiaryRegistration>? pending;

  @override
  Future<BeneficiaryRegistration> register(
    BeneficiaryRegistrationRequest request,
  ) async {
    submitted.add(request);
    if (pending case final completer?) return completer.future;
    return const BeneficiaryRegistration(
      accountId: 'account-1',
      beneficiaryId: 'beneficiary-1',
      reference: 'BEN-00000001',
      displayName: 'Fictional person',
      email: 'person@example.test',
    );
  }
}
