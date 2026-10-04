class Inmueble {
  final int idLote;
  final int? idCodigo;
  final int? codFijo;
  final String nombre;
  final int estado;
  final bool tieneSuministro;
  final String estadoDesc;
  final String? fechaCambioEstado;
  final String uv;
  final String mza;
  final String lote;
  final double? latitud;
  final double? longitud;

  Inmueble({
    required this.idLote,
    this.idCodigo,
    this.codFijo,
    required this.nombre,
    required this.estado,
    required this.tieneSuministro,
    required this.estadoDesc,
    this.fechaCambioEstado,
    required this.uv,
    required this.mza,
    required this.lote,
    this.latitud,
    this.longitud,
  });

  factory Inmueble.fromJson(Map<String, dynamic> json) {
    return Inmueble(
      idLote: json['idLote'] as int? ?? 0,
      idCodigo: json['idCodigo'] as int?,
      codFijo: json['codFijo'] as int?,
      nombre: json['nombre'] as String? ?? '(Sin Suministro)',
      estado: json['estado'] as int? ?? 0,
      tieneSuministro: json['tieneSuministro'] as bool? ?? false,
      estadoDesc: json['estadoDesc'] as String? ?? 'Sin Suministro',
      fechaCambioEstado: json['fechaCambioEstado'] as String?,
      uv: json['uv'] as String? ?? 'S/UV',
      mza: json['mza'] as String? ?? 'S/MZA',
      lote: json['lote'] as String? ?? 'S/L',
      latitud: (json['latitud'] as num?)?.toDouble(),
      longitud: (json['longitud'] as num?)?.toDouble(),
    );
  }
}
