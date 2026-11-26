import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/core/theme/app_theme.dart';
import 'package:mobile/features/requests/application/requests_bloc.dart';
import 'package:mobile/features/requests/domain/beneficiary_request.dart';
import 'package:mobile/features/requests/domain/requests_failure.dart';
import 'package:mobile/features/requests/presentation/request_widgets.dart';
import 'package:mobile/l10n/generated/app_localizations.dart';

void main() {
  testWidgets('shows API reference and status', (tester) async {
    await pump(tester, const RequestsLoaded([request]));
    expect(find.text('REQ-2026-0042'), findsOneWidget);
    expect(find.text('Submitted'), findsOneWidget);
  });
  testWidgets('shows empty only for successful empty response', (tester) async {
    await pump(tester, const RequestsLoaded([]));
    expect(find.text('No requests yet'), findsOneWidget);
    expect(find.text('Requests are temporarily unavailable'), findsNothing);
  });
  testWidgets('expired session is not empty and hides rows', (tester) async {
    await pump(
      tester,
      const RequestsFailed(RequestsFailureType.sessionExpired),
    );
    expect(find.text('Your session has expired'), findsOneWidget);
    expect(find.text('No requests yet'), findsNothing);
    expect(find.text('REQ-2026-0042'), findsNothing);
  });
  testWidgets('unavailable API is not empty and can retry', (tester) async {
    await pump(tester, const RequestsFailed(RequestsFailureType.unavailable));
    expect(find.text('Requests are temporarily unavailable'), findsOneWidget);
    expect(find.text('No requests yet'), findsNothing);
    expect(find.text('Try again'), findsOneWidget);
  });
  testWidgets('fits narrow viewport', (tester) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(320, 640);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    await pump(tester, const RequestsLoaded([request]));
    expect(tester.takeException(), isNull);
  });
}

Future<void> pump(WidgetTester tester, RequestsState state) async {
  await tester.pumpWidget(
    MaterialApp(
      locale: const Locale('en'),
      theme: AppTheme.light,
      localizationsDelegates: const [
        AppLocalizations.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: AppLocalizations.supportedLocales,
      home: Scaffold(
        body: RequestsContent(state: state, onRetry: () {}, onLoadMore: () {}),
      ),
    ),
  );
  await tester.pumpAndSettle();
}

const request = BeneficiaryRequest(
  id: 'fictional-42',
  reference: 'REQ-2026-0042',
  isMultiNeed: false,
  description: 'Fictional food support',
  status: 'Submitted',
  createdAt: null,
);
