import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/core/theme/app_theme.dart';
import 'package:mobile/features/auth/domain/session_user.dart';
import 'package:mobile/features/auth/domain/beneficiary_profile_hint.dart';
import 'package:mobile/features/requests/application/single_need_submission_bloc.dart';
import 'package:mobile/features/requests/domain/beneficiary_request.dart';
import 'package:mobile/features/requests/domain/single_need_submission.dart';
import 'package:mobile/features/requests/presentation/single_need_request_page.dart';
import 'package:mobile/features/welcome/application/locale_cubit.dart';
import 'package:mobile/l10n/generated/app_localizations.dart';

void main() {
  testWidgets('requires a description before moving to confirmations', (
    tester,
  ) async {
    await pumpPage(tester, DemoSubmissionRepository());

    await tester.tap(find.text('Continue to review'));
    await tester.pumpAndSettle();

    expect(
      find.text('Describe the assistance you need before continuing.'),
      findsOneWidget,
    );
    expect(
      find.text('Confirm authority and information sharing'),
      findsNothing,
    );
  });

  testWidgets(
    'requires both confirmations and submits only after both are checked',
    (tester) async {
      final repository = DemoSubmissionRepository();
      await pumpPage(tester, repository);
      await enterDescriptionAndContinue(tester);

      await tester.tap(find.text('Submit request'));
      await tester.pumpAndSettle();
      expect(find.text('Confirm both statements to submit.'), findsOneWidget);
      expect(repository.submissions, isEmpty);

      await tapConsent(tester, 0);
      await tester.tap(find.text('Submit request'));
      await tester.pumpAndSettle();
      expect(find.text('Confirm both statements to submit.'), findsOneWidget);
      expect(repository.submissions, isEmpty);

      await tapConsent(tester, 1);
      await tester.tap(find.text('Submit request'));
      await tester.pumpAndSettle();
      expect(repository.submissions, hasLength(1));
      expect(repository.submissions.single.authorityConfirmed, isTrue);
      expect(repository.submissions.single.sharingAgreed, isTrue);
      expect(find.text('Your request was submitted'), findsOneWidget);
      expect(find.text('REQ-2026-0042'), findsOneWidget);
      expect(find.text('Submitted'), findsOneWidget);
    },
  );

  testWidgets('shows uncertain result without displaying success', (
    tester,
  ) async {
    final repository = DemoSubmissionRepository()
      ..failure = const SingleNeedFailure(
        SingleNeedFailureType.outcomeUnconfirmed,
      );
    await pumpPage(tester, repository);
    await enterDescriptionAndContinue(tester);
    await tapConsent(tester, 0);
    await tapConsent(tester, 1);
    await tester.tap(find.text('Submit request'));
    await tester.pumpAndSettle();

    expect(find.text("We couldn't confirm the submission"), findsOneWidget);
    expect(
      find.text(
        'The service may have received your request. Check My requests before trying again to avoid creating a duplicate.',
      ),
      findsOneWidget,
    );
    expect(find.text('Your request was submitted'), findsNothing);
    expect(find.text('Go to My requests'), findsOneWidget);
    expect(repository.submissions, hasLength(1));
  });

  testWidgets('shows progress until the API confirms success', (tester) async {
    final repository = DemoSubmissionRepository()
      ..pending = Completer<BeneficiaryRequest>();
    await pumpPage(tester, repository);
    await enterDescriptionAndContinue(tester);
    await tapConsent(tester, 0);
    await tapConsent(tester, 1);
    await tester.tap(find.text('Submit request'));
    await tester.pump();

    expect(find.text('Submitting your request…'), findsOneWidget);
    expect(find.byType(CircularProgressIndicator), findsOneWidget);
    expect(find.text('Your request was submitted'), findsNothing);

    repository.pending!.complete(
      const BeneficiaryRequest(
        id: 'fictional-request-id',
        reference: 'REQ-2026-0042',
        isMultiNeed: false,
        description: 'Fictional food support',
        status: 'Submitted',
        createdAt: null,
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('Your request was submitted'), findsOneWidget);
  });

  testWidgets('keeps the form usable on a smaller viewport', (tester) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(320, 640);
    tester.view.viewPadding = const FakeViewPadding(top: 24, bottom: 20);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    addTearDown(tester.view.resetViewPadding);
    await pumpPage(tester, DemoSubmissionRepository());

    expect(find.text('Describe the support you need'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}

Future<void> enterDescriptionAndContinue(WidgetTester tester) async {
  await tester.enterText(
    find.byKey(const ValueKey('single-need-description')),
    'Fictional food support',
  );
  await tester.tap(find.text('Continue to review'));
  await tester.pumpAndSettle();
  expect(
    find.text('Confirm authority and information sharing'),
    findsOneWidget,
  );
}

Future<void> tapConsent(WidgetTester tester, int index) async {
  final checkbox = find.byType(Checkbox).at(index);
  await tester.ensureVisible(checkbox);
  await tester.tap(checkbox);
  await tester.pumpAndSettle();
}

Future<void> pumpPage(
  WidgetTester tester,
  DemoSubmissionRepository repository,
) async {
  await tester.pumpWidget(
    BlocProvider<LocaleCubit>(
      create: (_) => LocaleCubit()..select(const Locale('en')),
      child: MaterialApp(
        theme: AppTheme.light,
        locale: const Locale('en'),
        localizationsDelegates: const [
          AppLocalizations.delegate,
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
        ],
        supportedLocales: AppLocalizations.supportedLocales,
        home: BlocProvider<SingleNeedSubmissionBloc>(
          create: (_) => SingleNeedSubmissionBloc(repository),
          child: const SingleNeedRequestPage(user: demoUser),
        ),
      ),
    ),
  );
  await tester.pumpAndSettle();
}

const demoUser = SessionUser(
  id: 'fictional-user',
  displayName: 'Demo Beneficiary',
  email: 'beneficiary@example.test',
  roles: ['Beneficiary'],
);

class DemoSubmissionRepository implements SingleNeedSubmissionRepository {
  final submissions = <SingleNeedSubmission>[];
  SingleNeedFailure? failure;
  Completer<BeneficiaryRequest>? pending;

  @override
  Future<BeneficiaryRequest> submit(
    SingleNeedSubmission submission, {
    BeneficiaryProfileHint? profileHint,
  }) async {
    submissions.add(submission);
    if (failure case final value?) throw value;
    if (pending case final completer?) return completer.future;
    return const BeneficiaryRequest(
      id: 'fictional-request-id',
      reference: 'REQ-2026-0042',
      isMultiNeed: false,
      description: 'Fictional food support',
      status: 'Submitted',
      createdAt: null,
    );
  }
}
