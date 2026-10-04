import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import '../models/inmueble.dart';
import '../services/api_service.dart';

class MapScreen extends StatefulWidget {
  final LatLng? targetLocation;
  final Inmueble? selectedInmueble;

  const MapScreen({super.key, this.targetLocation, this.selectedInmueble});

  @override
  State<MapScreen> createState() => _MapScreenState();
}

class _MapScreenState extends State<MapScreen> {
  final MapController _mapController = MapController();
  List<Inmueble> _inmuebles = [];
  bool _loading = false;
  LatLng _center = const LatLng(-16.377, -60.963);
  int _estadoSeleccionado = 0; // 0=Todos, 1=Normal, 2=Para Corte, 3=Cortado

  @override
  void initState() {
    super.initState();
    if (widget.targetLocation != null) {
      _center = widget.targetLocation!;
    }
    _cargarPuntos();
  }

  Future<void> _cargarPuntos() async {
    setState(() => _loading = true);
    final items = await ApiService.buscarInmuebles(tipoPredio: 'CON_SUMINISTRO');
    setState(() {
      _inmuebles = items.where((i) => i.latitud != null && i.longitud != null).take(150).toList();
      _loading = false;
    });

    if (widget.selectedInmueble != null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        _mostrarFichaTecnica(widget.selectedInmueble!);
      });
    }
  }

  void _mostrarFichaTecnica(Inmueble item) {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(12)),
      ),
      builder: (ctx) {
        return Padding(
          padding: const EdgeInsets.all(20.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    item.tieneSuministro
                        ? 'Código Fijo: ${item.codFijo}'
                        : 'Lote Catastral (Sin Medidor)',
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF2C2D2A),
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                    decoration: BoxDecoration(
                      color: item.tieneSuministro
                          ? const Color(0xFF5E7A4A).withValues(alpha: 0.15)
                          : const Color(0xFFC89D3C).withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: Text(
                      item.estadoDesc,
                      style: TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.bold,
                        color: item.tieneSuministro
                            ? const Color(0xFF5E7A4A)
                            : const Color(0xFF8A6510),
                      ),
                    ),
                  ),
                ],
              ),
              const Divider(height: 20),
              _buildFilaInfo('Titular / Predio:', item.nombre),
              _buildFilaInfo('Ubicación:', 'UV: ${item.uv} | Manzana: ${item.mza} | Lote: ${item.lote}'),
              _buildFilaInfo('Coordenadas:', 'Lat: ${item.latitud?.toStringAsFixed(6)}, Lon: ${item.longitud?.toStringAsFixed(6)}'),
              const SizedBox(height: 12),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: () => Navigator.pop(ctx),
                  icon: const Icon(Icons.close, size: 16),
                  label: const Text('Cerrar Ficha'),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFFFAF9F5),
                    foregroundColor: const Color(0xFF4B4E47),
                    elevation: 0,
                    side: const BorderSide(color: Color(0xFFC9C5B9)),
                  ),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildFilaInfo(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 110,
            child: Text(
              label,
              style: const TextStyle(fontSize: 12, color: Color(0xFF6B6E67)),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Color(0xFF2C2D2A)),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFiltroChip(String label, int estado, Color color) {
    final active = _estadoSeleccionado == estado;
    return GestureDetector(
      onTap: () => setState(() => _estadoSeleccionado = estado),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
        decoration: BoxDecoration(
          color: active ? color : Colors.white.withValues(alpha: 0.95),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: color, width: active ? 1.5 : 1),
          boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 2, offset: Offset(0, 1))],
        ),
        child: Text(
          label,
          style: TextStyle(
            fontSize: 11,
            fontWeight: FontWeight.bold,
            color: active ? Colors.white : color,
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Visor Cartográfico'),
        backgroundColor: const Color(0xFF2E4636),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _cargarPuntos,
            tooltip: 'Recargar puntos',
          ),
        ],
      ),
      body: Stack(
        children: [
          FlutterMap(
            mapController: _mapController,
            options: MapOptions(
              initialCenter: _center,
              initialZoom: 14.5,
              minZoom: 10,
              maxZoom: 19,
            ),
            children: [
              TileLayer(
                urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                userAgentPackageName: 'com.uagrm.visordatossig',
              ),
              MarkerLayer(
                markers: _inmuebles
                    .where((item) => _estadoSeleccionado == 0 || item.estado == _estadoSeleccionado)
                    .map((item) {
                  return Marker(
                    point: LatLng(item.latitud!, item.longitud!),
                    width: 24,
                    height: 24,
                    child: GestureDetector(
                      onTap: () => _mostrarFichaTecnica(item),
                      child: Container(
                        decoration: BoxDecoration(
                          color: item.estado == 2
                              ? const Color(0xFFC27D38)
                              : item.estado == 3
                                  ? const Color(0xFFB84A39)
                                  : const Color(0xFF5E7A4A),
                          shape: BoxShape.circle,
                          border: Border.all(color: Colors.white, width: 1.5),
                          boxShadow: const [
                            BoxShadow(color: Colors.black26, blurRadius: 3),
                          ],
                        ),
                      ),
                    ),
                  );
                }).toList(),
              ),
            ],
          ),
          Positioned(
            top: 10,
            left: 10,
            right: 10,
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  _buildFiltroChip('Todos', 0, const Color(0xFF2E4636)),
                  const SizedBox(width: 6),
                  _buildFiltroChip('Normal', 1, const Color(0xFF5E7A4A)),
                  const SizedBox(width: 6),
                  _buildFiltroChip('Para Corte', 2, const Color(0xFFC27D38)),
                  const SizedBox(width: 6),
                  _buildFiltroChip('Cortados', 3, const Color(0xFFB84A39)),
                ],
              ),
            ),
          ),
          if (_loading)
            Positioned(
              top: 55,
              left: 10,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.9),
                  borderRadius: BorderRadius.circular(4),
                  border: Border.all(color: const Color(0xFFC9C5B9)),
                ),
                child: const Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    SizedBox(width: 14, height: 14, child: CircularProgressIndicator(strokeWidth: 2)),
                    SizedBox(width: 8),
                    Text('Cargando geodatos...', style: TextStyle(fontSize: 11)),
                  ],
                ),
              ),
            ),
          Positioned(
            bottom: 20,
            right: 15,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                FloatingActionButton.small(
                  heroTag: 'btnZoomIn',
                  backgroundColor: Colors.white,
                  foregroundColor: const Color(0xFF2C2D2A),
                  onPressed: () {
                    _mapController.move(_mapController.camera.center, _mapController.camera.zoom + 1);
                  },
                  child: const Icon(Icons.add),
                ),
                const SizedBox(height: 6),
                FloatingActionButton.small(
                  heroTag: 'btnZoomOut',
                  backgroundColor: Colors.white,
                  foregroundColor: const Color(0xFF2C2D2A),
                  onPressed: () {
                    _mapController.move(_mapController.camera.center, _mapController.camera.zoom - 1);
                  },
                  child: const Icon(Icons.remove),
                ),
                const SizedBox(height: 6),
                FloatingActionButton.small(
                  heroTag: 'btnReset',
                  backgroundColor: const Color(0xFF2E4636),
                  foregroundColor: Colors.white,
                  onPressed: () {
                    _mapController.move(const LatLng(-16.377, -60.963), 14.5);
                  },
                  child: const Icon(Icons.fullscreen),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
