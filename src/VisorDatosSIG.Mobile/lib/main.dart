import 'package:flutter/material.dart';
import 'screens/login_screen.dart';

void main() {
  runApp(const VisorDatosSigMobileApp());
}

class VisorDatosSigMobileApp extends StatelessWidget {
  const VisorDatosSigMobileApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'VisorDatosSIG Móvil',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF2E4636),
          primary: const Color(0xFF2E4636),
          secondary: const Color(0xFFC89D3C),
          surface: const Color(0xFFFAF9F5),
        ),
        scaffoldBackgroundColor: const Color(0xFFF5F3EC),
        appBarTheme: const AppBarTheme(
          elevation: 0,
          centerTitle: false,
          titleTextStyle: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
            color: Colors.white,
            letterSpacing: -0.3,
          ),
        ),
      ),
      home: const LoginScreen(),
    );
  }
}
