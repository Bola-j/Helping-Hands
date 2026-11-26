import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/app/app_routes.dart';
import 'package:mobile/core/theme/app_theme.dart';
import 'package:mobile/features/auth/domain/session_user.dart';
import 'package:mobile/features/requests/application/requests_bloc.dart';
import 'package:mobile/features/requests/domain/beneficiary_request.dart';
import 'package:mobile/features/requests/domain/requests_repository.dart';
import 'package:mobile/features/requests/presentation/beneficiary_home_page.dart';
import 'package:mobile/features/requests/presentation/my_requests_page.dart';
import 'package:mobile/l10n/generated/app_localizations.dart';

void main() {
  testWidgets('home renders the authorized summary at the Figma width', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(390, 884);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    await pump(tester, const BeneficiaryHomePage(user: beneficiary));
    expect(find.text('Demo Beneficiary'), findsOneWidget);
    expect(find.text('REQ-2026-0042'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('my requests adapts to smaller viewports', (tester) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(320, 640);
    tester.view.viewPadding = const FakeViewPadding(top: 24, bottom: 24);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    addTearDown(tester.view.resetViewPadding);
    await pump(tester, const MyRequestsPage(user: beneficiary));
    expect(find.text('My requests').first, findsOneWidget);
    expect(find.text('REQ-2026-0042'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('empty request list offers a working Home action', (
    tester,
  ) async {
    await pump(
      tester,
      const MyRequestsPage(user: beneficiary),
      items: const [],
    );
    final action = find.text('Go to Home to start a request');
    expect(action, findsOneWidget);
    await tester.tap(action);
    await tester.pumpAndSettle();
    expect(find.text('Home request options'), findsOneWidget);
  });
}

Future<void> pump(
  WidgetTester tester,
  Widget page, {
  List<BeneficiaryRequest> items = const [request],
}) async {
  await tester.pumpWidget(
    MaterialApp(
      theme: AppTheme.light,
      locale: const Locale('en'),
      localizationsDelegates: const [
        AppLocalizations.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: AppLocalizations.supportedLocales,
      onGenerateRoute: (settings) => settings.name == AppRoutes.home
          ? MaterialPageRoute<void>(
              builder: (_) => const Scaffold(
                body: Center(child: Text('Home request options')),
              ),
            )
          : null,
      home: BlocProvider(
        create: (_) =>
            RequestsBloc(FakeRepository(items))
              ..add(const RequestsLoadRequested()),
        child: page,
      ),
    ),
  );
  await tester.pumpAndSettle();
}

const beneficiary = SessionUser(
  id: 'fictional-user',
  displayName: 'Demo Beneficiary',
  email: 'beneficiary@example.test',
  roles: ['Beneficiary'],
);
const request = BeneficiaryRequest(
  id: 'fictional-42',
  reference: 'REQ-2026-0042',
  isMultiNeed: false,
  description: 'Fictional food support',
  status: 'Submitted',
  createdAt: null,
);

class FakeRepository implements RequestsRepository {
  const FakeRepository(this.items);
  final List<BeneficiaryRequest> items;

  @override
  Future<RequestPage> load({String? cursor}) async => RequestPage(items: items);
}
