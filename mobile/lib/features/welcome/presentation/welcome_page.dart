import 'package:flutter/material.dart';

import 'welcome_content.dart';
import 'welcome_footer.dart';
import 'welcome_header.dart';

class WelcomePage extends StatelessWidget {
  const WelcomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF6F9FA),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
          child: Column(
            children: [
              const WelcomeHeader(),
              const Expanded(child: WelcomeContentViewport()),
              const WelcomeFooter(),
            ],
          ),
        ),
      ),
    );
  }
}

class WelcomeContentViewport extends StatelessWidget {
  const WelcomeContentViewport({super.key});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) => SingleChildScrollView(
        child: ConstrainedBox(
          constraints: BoxConstraints(minHeight: constraints.maxHeight),
          child: const Center(child: WelcomeContent()),
        ),
      ),
    );
  }
}
