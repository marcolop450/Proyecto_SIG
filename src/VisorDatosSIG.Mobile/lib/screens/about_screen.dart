import 'package:flutter/material.dart';

class AboutScreen extends StatelessWidget {
  const AboutScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F3EC),
      appBar: AppBar(
        title: const Text('Acerca del Sistema'),
        backgroundColor: const Color(0xFF2E4636),
        foregroundColor: Colors.white,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Card(
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(6),
                side: const BorderSide(color: Color(0xFFC9C5B9)),
              ),
              color: Colors.white,
              child: const Padding(
                padding: EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'VisorDatosSIG Móvil 2026',
                      style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF2C2D2A)),
                    ),
                    SizedBox(height: 4),
                    Text(
                      'Cliente nativo Flutter para la consulta territorial de San Ignacio de Velasco.',
                      style: TextStyle(fontSize: 12, color: Color(0xFF6B6E67)),
                    ),
                    Divider(height: 20),
                    Text('Universidad: Universidad Autónoma Gabriel René Moreno', style: TextStyle(fontSize: 12)),
                    SizedBox(height: 3),
                    Text('Facultad: Fac. de Ingeniería en Ciencias de la Computación y Telecomunicaciones', style: TextStyle(fontSize: 12)),
                    SizedBox(height: 3),
                    Text('Materia: [2-2026] SISTEMAS DE INFORM.GEOGRAFICA - DI INF442', style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                    SizedBox(height: 3),
                    Text('Docente: Ing. PEREZ FERREIRA UBALDO', style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                    SizedBox(height: 3),
                    Text('Semestre: Semestre 2 - 2026', style: TextStyle(fontSize: 12)),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            Card(
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(6),
                side: const BorderSide(color: Color(0xFFC9C5B9)),
              ),
              color: Colors.white,
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Equipo de Desarrollo (Orden Alfabético por Apellido)',
                      style: TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: Color(0xFF2C2D2A)),
                    ),
                    const Divider(height: 16),
                    _buildIntegrante('1', 'Guzman Justiniano, Nohelia', '222049367'),
                    _buildIntegrante('2', 'Jimenez Duarte, Nils Jonathan', '222008741'),
                    _buildIntegrante('3', 'López Velásquez, Marco Alejandro', '222008891'),
                    _buildIntegrante('4', 'Quispe Tito, Jorge Gabriel', '222009527'),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  static Widget _buildIntegrante(String num, String nombre, String registro) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Row(
        children: [
          Container(
            width: 20,
            alignment: Alignment.center,
            child: Text(num, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12, color: Color(0xFF2E4636))),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(nombre, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Color(0xFF2C2D2A))),
          ),
          Text(registro, style: const TextStyle(fontSize: 12, fontFamily: 'monospace', color: Color(0xFF6B6E67))),
        ],
      ),
    );
  }
}
