import 'package:flutter/material.dart';
import 'package:latlong2/latlong.dart';
import '../models/inmueble.dart';
import '../services/api_service.dart';
import 'map_screen.dart';

class SearchScreen extends StatefulWidget {
  const SearchScreen({super.key});

  @override
  State<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends State<SearchScreen> {
  final _searchController = TextEditingController();
  String _tipoPredio = 'TODOS';
  List<Inmueble> _resultados = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _ejecutarBusqueda();
  }

  Future<void> _ejecutarBusqueda() async {
    setState(() => _loading = true);
    final items = await ApiService.buscarInmuebles(
      texto: _searchController.text.trim(),
      tipoPredio: _tipoPredio,
    );
    setState(() {
      _resultados = items;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFFAF9F5),
      appBar: AppBar(
        title: const Text('Consultas Temáticas'),
        backgroundColor: const Color(0xFF2E4636),
        foregroundColor: Colors.white,
      ),
      body: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            color: Colors.white,
            child: Column(
              children: [
                TextField(
                  controller: _searchController,
                  decoration: InputDecoration(
                    hintText: 'Buscar por código, titular o UV...',
                    prefixIcon: const Icon(Icons.search, size: 20),
                    suffixIcon: IconButton(
                      icon: const Icon(Icons.clear, size: 18),
                      onPressed: () {
                        _searchController.clear();
                        _ejecutarBusqueda();
                      },
                    ),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(4)),
                  ),
                  onSubmitted: (_) => _ejecutarBusqueda(),
                ),
                const SizedBox(height: 8),
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _buildChip('TODOS', 'Todos'),
                      const SizedBox(width: 6),
                      _buildChip('CON_SUMINISTRO', 'Con Suministro'),
                      const SizedBox(width: 6),
                      _buildChip('SIN_SUMINISTRO', 'Baldíos'),
                      const SizedBox(width: 6),
                      _buildChip('PARA_CORTE', 'Para Corte'),
                      const SizedBox(width: 6),
                      _buildChip('CORTADO', 'Cortados'),
                    ],
                  ),
                ),
              ],
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
            color: const Color(0xFFF5F3EC),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Mostrando ${_resultados.length} registros',
                  style: const TextStyle(fontSize: 12, color: Color(0xFF6B6E67)),
                ),
                if (_loading)
                  const SizedBox(
                    width: 14,
                    height: 14,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  ),
              ],
            ),
          ),
          Expanded(
            child: _resultados.isEmpty && !_loading
                ? const Center(
                    child: Text(
                      'No se encontraron inmuebles con los criterios seleccionados.',
                      style: TextStyle(color: Color(0xFF6B6E67), fontSize: 13),
                    ),
                  )
                : ListView.separated(
                    padding: const EdgeInsets.all(8),
                    itemCount: _resultados.length,
                    separatorBuilder: (context, index) => const Divider(height: 1),
                    itemBuilder: (ctx, idx) {
                      final item = _resultados[idx];
                      return Card(
                        elevation: 0,
                        margin: const EdgeInsets.symmetric(vertical: 2),
                        color: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(4),
                          side: const BorderSide(color: Color(0xFFE8E5DC)),
                        ),
                        child: ListTile(
                          dense: true,
                          title: Text(
                            item.tieneSuministro
                                ? 'Cod: ${item.codFijo} - ${item.nombre}'
                                : item.nombre,
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                          ),
                          subtitle: Text(
                            'UV: ${item.uv} | MZA: ${item.mza} | Lote: ${item.lote} · ${item.estadoDesc}',
                            style: const TextStyle(fontSize: 11, color: Color(0xFF6B6E67)),
                          ),
                          trailing: item.latitud != null && item.longitud != null
                              ? IconButton(
                                  icon: const Icon(Icons.pin_drop_outlined, color: Color(0xFF2E4636)),
                                  tooltip: 'Ver en Mapa',
                                  onPressed: () {
                                    Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (_) => MapScreen(
                                          targetLocation: LatLng(item.latitud!, item.longitud!),
                                          selectedInmueble: item,
                                        ),
                                      ),
                                    );
                                  },
                                )
                              : null,
                        ),
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildChip(String val, String label) {
    final selected = _tipoPredio == val;
    return ChoiceChip(
      label: Text(label, style: TextStyle(fontSize: 11, color: selected ? Colors.white : const Color(0xFF2C2D2A))),
      selected: selected,
      selectedColor: const Color(0xFF2E4636),
      backgroundColor: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4)),
      onSelected: (s) {
        if (s) {
          setState(() => _tipoPredio = val);
          _ejecutarBusqueda();
        }
      },
    );
  }
}
