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
import '../features/auth/domain/session_user.dart';
import '../features/auth/data/beneficiary_registration_remote_data_source.dart';
import '../features/auth/data/beneficiary_registration_repository_impl.dart';
import '../features/auth/domain/beneficiary_registration_repository.dart';
import '../features/auth/application/create_account_bloc.dart';
import '../features/auth/application/session_bloc.dart';
import '../features/auth/presentation/create_account_page.dart';
import '../features/auth/presentation/sign_in_page.dart';
import '../features/auth/presentation/session_expired_page.dart';
import '../features/auth/presentation/profile_page.dart';
import '../features/requests/application/requests_bloc.dart';
import '../features/requests/data/requests_remote_data_source.dart';
import '../features/requests/data/requests_repository_impl.dart';
import '../features/requests/data/single_need_remote_data_source.dart';
import '../features/requests/data/single_need_submission_repository_impl.dart';
import '../features/requests/domain/requests_repository.dart';
import '../features/requests/domain/single_need_submission.dart';
import '../features/requests/application/single_need_submission_bloc.dart';
import '../features/requests/presentation/single_need_request_page.dart';
import '../features/requests/presentation/beneficiary_home_page.dart';
import '../features/requests/presentation/my_requests_page.dart';
import '../features/welcome/presentation/welcome_page.dart';
import '../l10n/generated/app_localizations.dart';
import 'app_routes.dart';

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
            SessionRemoteDataSource(
              context.read<Dio>(),
              context.read<CookieJar>(),
            ),
          ),
        ),
        RepositoryProvider<BeneficiaryRegistrationRepository>(
          create: (context) => BeneficiaryRegistrationRepositoryImpl(
            BeneficiaryRegistrationRemoteDataSource(context.read<Dio>()),
          ),
        ),
        RepositoryProvider<RequestsRepository>(
          create: (context) => RequestsRepositoryImpl(
            RequestsRemoteDataSource(context.read<Dio>()),
            context.read<SessionRepository>(),
          ),
        ),
        RepositoryProvider<SingleNeedSubmissionRepository>(
          create: (context) => SingleNeedSubmissionRepositoryImpl(
            SingleNeedRemoteDataSource(context.read<Dio>()),
            context.read<SessionRepository>(),
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
              AppRoutes.createAccount: (context) => BlocProvider(
                create: (context) => CreateAccountBloc(
                  context.read<BeneficiaryRegistrationRepository>(),
                ),
                child: const CreateAccountPage(),
              ),
              AppRoutes.signIn: (context) => BlocProvider(
                create: (context) =>
                    SessionBloc(context.read<SessionRepository>()),
                child: _signInPage(context),
              ),
              AppRoutes.sessionExpired: (_) => const SessionExpiredPage(),
              AppRoutes.profile: (context) => BlocProvider(
                create: (context) => SessionBloc(
                  context.read<SessionRepository>(),
                  initialUser:
                      ModalRoute.of(context)?.settings.arguments
                          as SessionUser?,
                ),
                child: const ProfilePage(),
              ),
              AppRoutes.home: (context) =>
                  _requestsPage(context, _homePage(context)),
              AppRoutes.requests: (context) => _requestsPage(
                context,
                MyRequestsPage(
                  user:
                      ModalRoute.of(context)!.settings.arguments as SessionUser,
                ),
              ),
              AppRoutes.singleNeed: (context) {
                final arguments = _homeArguments(context);
                return BlocProvider(
                  create: (context) => SingleNeedSubmissionBloc(
                    context.read<SingleNeedSubmissionRepository>(),
                  ),
                  child: SingleNeedRequestPage(
                    user: arguments.user,
                    profileHint: arguments.profileHint,
                  ),
                );
              },
            },
          ),
        ),
      ),
    );
  }

  Widget _requestsPage(BuildContext context, Widget child) => BlocProvider(
    create: (context) =>
        RequestsBloc(context.read<RequestsRepository>())
          ..add(const RequestsLoadRequested()),
    child: child,
  );

  Widget _signInPage(BuildContext context) {
    final argument = ModalRoute.of(context)?.settings.arguments;
    final parameters = argument is SignInRouteArguments
        ? argument
        : SignInRouteArguments(email: argument as String?);
    return SignInPage(
      initialEmail: parameters.email,
      profileHint: parameters.profileHint,
    );
  }

  BeneficiaryHomePage _homePage(BuildContext context) {
    final arguments = _homeArguments(context);
    return BeneficiaryHomePage(
      user: arguments.user,
      profileHint: arguments.profileHint,
    );
  }

  BeneficiaryHomeRouteArguments _homeArguments(BuildContext context) {
    final argument = ModalRoute.of(context)!.settings.arguments;
    return argument is BeneficiaryHomeRouteArguments
        ? argument
        : BeneficiaryHomeRouteArguments(argument as SessionUser);
  }
}
