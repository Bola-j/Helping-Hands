import 'package:dio/dio.dart';
import 'package:cookie_jar/cookie_jar.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../core/network/dio_factory.dart';
import '../core/theme/app_theme.dart';
import '../features/welcome/application/locale_cubit.dart';
import '../features/auth/data/session_remote_data_source.dart';
import '../features/auth/data/session_repository_impl.dart';
import '../features/auth/domain/session_repository.dart';
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
        RepositoryProvider<CookieJar>(create: (_) => CookieJar()),
        RepositoryProvider<Dio>(
          create: (context) =>
              DioFactory.create(cookieJar: context.read<CookieJar>()),
          dispose: (dio) => dio.close(force: true),
        ),
        RepositoryProvider<SessionRepository>(
          create: (context) => SessionRepositoryImpl(
            SessionRemoteDataSource(context.read<Dio>()),
          ),
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
