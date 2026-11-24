import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/app/app.dart';
import 'package:mobile/core/theme/app_theme.dart';
import 'package:mobile/core/widgets/app_button.dart';
import 'package:mobile/core/widgets/app_card.dart';
import 'package:mobile/core/widgets/app_text_field.dart';

Future<void> _loadAppFonts() async {
  for (final entry in <(String, String)>[
    ('Cabin', 'Cabin[wdth,wght].ttf'),
    ('NotoSansArabic', 'NotoSansArabic[wdth,wght].ttf'),
    ('FreeSerif', 'FreeSerifBold.ttf'),
  ]) {
    final loader = FontLoader(entry.$1)
      ..addFont(rootBundle.load('assets/fonts/${entry.$2}'));
    await loader.load();
  }
}

void main() {
  setUpAll(_loadAppFonts);

  testWidgets('starts on the Arabic welcome screen at the Figma viewport', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(390, 884);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    await tester.pumpWidget(const HelpingHandsApp());
    await tester.pumpAndSettle();
    expect(find.text('أهلاً بك في أيادي الخير'), findsOneWidget);
    expect(find.text('اختر لغة التطبيق'), findsOneWidget);
    expect(find.text('إنشاء حساب مستفيد'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('language controls switch all welcome copy to English', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(390, 884);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    await tester.pumpWidget(const HelpingHandsApp());
    await tester.tap(find.text('English').first);
    await tester.pumpAndSettle();

    expect(find.text('Welcome to Helping Hands'), findsOneWidget);
    expect(find.text('Choose app language'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('welcome actions navigate to their foundation placeholders', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(390, 884);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    await tester.pumpWidget(const HelpingHandsApp());
    await tester.tap(find.text('إنشاء حساب مستفيد'));
    await tester.pumpAndSettle();
    expect(find.text('إنشاء حساب'), findsOneWidget);

    tester.state<NavigatorState>(find.byType(Navigator).first).pop();
    await tester.pumpAndSettle();
    await tester.tap(find.text('تسجيل الدخول إلى حساب حالي'));
    await tester.pumpAndSettle();
    expect(find.text('تسجيل الدخول'), findsOneWidget);
  });

  testWidgets('welcome layout remains usable at a narrow viewport', (
    tester,
  ) async {
    tester.view.devicePixelRatio = 1;
    tester.view.physicalSize = const Size(320, 640);
    tester.view.viewPadding = const FakeViewPadding(top: 24, bottom: 24);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    addTearDown(tester.view.resetViewPadding);

    await tester.pumpWidget(const HelpingHandsApp());
    await tester.pumpAndSettle();

    expect(find.text('أهلاً بك في أيادي الخير'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('shared button exposes disabled and loading states', (
    tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light,
        home: const Scaffold(
          body: Column(
            children: [
              AppButton(label: 'Disabled', onPressed: null, isEnabled: false),
              AppButton(label: 'Loading', onPressed: _noop, isLoading: true),
            ],
          ),
        ),
      ),
    );

    final buttons = tester.widgetList<OutlinedButton>(
      find.byType(OutlinedButton),
    );
    expect(buttons.every((button) => button.onPressed == null), isTrue);
    expect(find.byType(CircularProgressIndicator), findsOneWidget);
  });

  testWidgets('shared form field exposes error and disabled states', (
    tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light,
        home: const Scaffold(
          body: AppTextField(
            label: 'Email',
            enabled: false,
            errorText: 'Enter a valid email',
          ),
        ),
      ),
    );

    final field = tester.widget<TextFormField>(find.byType(TextFormField));
    expect(field.enabled, isFalse);
    expect(find.text('Enter a valid email'), findsOneWidget);
  });

  testWidgets('shared card exposes selection and tap behavior', (tester) async {
    var tapped = false;
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light,
        home: Scaffold(
          body: AppCard(
            selected: true,
            onTap: () => tapped = true,
            child: const Text('Selected card'),
          ),
        ),
      ),
    );

    expect(tester.widget<AppCard>(find.byType(AppCard)).selected, isTrue);
    await tester.tap(find.text('Selected card'));
    expect(tapped, isTrue);
  });
}

void _noop() {}
