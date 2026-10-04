import 'package:flutter/material.dart';
import 'map_screen.dart';
import 'search_screen.dart';
import 'about_screen.dart';
import 'login_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _currentIndex = 0;

  final List<Widget> _screens = const [
    MapScreen(),
    SearchScreen(),
    AboutScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _screens[_currentIndex],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentIndex,
        onDestinationSelected: (idx) {
          setState(() => _currentIndex = idx);
        },
        backgroundColor: Colors.white,
        indicatorColor: const Color(0xFF2E4636).withValues(alpha: 0.15),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.map_outlined),
            selectedIcon: Icon(Icons.map, color: Color(0xFF2E4636)),
            label: 'Visor',
          ),
          NavigationDestination(
            icon: Icon(Icons.search_outlined),
            selectedIcon: Icon(Icons.search, color: Color(0xFF2E4636)),
            label: 'Consultas',
          ),
          NavigationDestination(
            icon: Icon(Icons.info_outline),
            selectedIcon: Icon(Icons.info, color: Color(0xFF2E4636)),
            label: 'Acerca de',
          ),
        ],
      ),
      drawer: Drawer(
        backgroundColor: const Color(0xFFFAF9F5),
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            const DrawerHeader(
              decoration: BoxDecoration(color: Color(0xFF2E4636)),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  Icon(Icons.terrain, color: Colors.white, size: 36),
                  SizedBox(height: 8),
                  Text(
                    'VisorDatosSIG Móvil',
                    style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  Text(
                    'San Ignacio de Velasco · SRID 4326',
                    style: TextStyle(color: Colors.white70, fontSize: 11),
                  ),
                ],
              ),
            ),
            ListTile(
              leading: const Icon(Icons.map, color: Color(0xFF2C2D2A)),
              title: const Text('Visor Cartográfico', style: TextStyle(fontSize: 13)),
              onTap: () {
                Navigator.pop(context);
                setState(() => _currentIndex = 0);
              },
            ),
            ListTile(
              leading: const Icon(Icons.search, color: Color(0xFF2C2D2A)),
              title: const Text('Consultas Temáticas', style: TextStyle(fontSize: 13)),
              onTap: () {
                Navigator.pop(context);
                setState(() => _currentIndex = 1);
              },
            ),
            ListTile(
              leading: const Icon(Icons.info_outline, color: Color(0xFF2C2D2A)),
              title: const Text('Acerca del Sistema', style: TextStyle(fontSize: 13)),
              onTap: () {
                Navigator.pop(context);
                setState(() => _currentIndex = 2);
              },
            ),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.exit_to_app, color: Color(0xFFB84A39)),
              title: const Text('Cerrar Sesión', style: TextStyle(fontSize: 13, color: Color(0xFFB84A39))),
              onTap: () {
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(builder: (_) => const LoginScreen()),
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
