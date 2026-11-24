import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../core/network/dio_factory.dart';
import '../core/theme/app_theme.dart';
import '../features/welcome/application/locale_cubit.dart';
import '../features/welcome/presentation/welcome_page.dart';
import '../l10n/generated/app_localizations.dart';
import 'app_routes.dart';
import 'placeholder_page.dart';

class HelpingHandsApp extends StatelessWidget {
  const HelpingHandsApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiRepositoryProvider(
      providers: [
        RepositoryProvider<Dio>(
          create: (_) => DioFactory.create(),
          dispose: (dio) => dio.close(force: true),
        ),
      ],
      child: BlocProvider(
        create: (_) => LocaleCubit(),
        child: BlocBuilder<LocaleCubit, Locale>(
          builder: (context, locale) => MaterialApp(
            title: 'Helping Hands',
            debugShowCheckedModeBanner: false,
            theme: AppTheme.light,
            locale: locale,
            supportedLocales: AppLocalizations.supportedLocales,
            localizationsDelegates: AppLocalizations.localizationsDelegates,
            initialRoute: AppRoutes.welcome,
            routes: {
              AppRoutes.welcome: (_) => const WelcomePage(),
              AppRoutes.createAccount: (_) => const PlaceholderPage(
                titleKey: PlaceholderTitle.createAccount,
              ),
              AppRoutes.signIn: (_) =>
                  const PlaceholderPage(titleKey: PlaceholderTitle.signIn),
            },
          ),
        ),
      ),
    );
  }
}
