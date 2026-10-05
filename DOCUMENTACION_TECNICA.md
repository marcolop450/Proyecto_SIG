# Documentación Técnica del Proyecto Integrador: VisorDatosSIG 2026

**Materia**: `[2-2026] SISTEMAS DE INFORM.GEOGRAFICA - DI INF442`  
**Institución**: Universidad Autónoma Gabriel René Moreno (UAGRM) — FICCT  
**Docente**: Ing. PEREZ FERREIRA UBALDO  
**Semestre**: 2 - 2026  
**Entorno Tecnológico**: .NET 8.0 C#, Visual Studio Community 2026, Microsoft SQL Server 2022 (Developer Edition, SRID 4326), Leaflet.js, Bootstrap 5.

### Integrantes del Proyecto
| # | Integrante (Orden Alfabético por Apellido) | Registro |
| :-: | :--- | :---: |
| 1 | **Guzman Justiniano**, Nohelia | 222049367 |
| 2 | **Jimenez Duarte**, Nils Jonathan | 222008741 |
| 3 | **López Velásquez**, Marco Alejandro | 222008891 |
| 4 | **Quispe Tito**, Jorge Gabriel | 222009527 |

---

## 1. Resumen de Implementación y Requerimientos Cumplidos

Se ha implementado la totalidad de los requerimientos correspondientes a los **Hitos H1 y H2** (con avance completo sobre el Hito H3 y H4) del pliego oficial de especificaciones:

| Código | Requisito / Actividad | Estado | Evidencia / Resultado |
| :--- | :--- | :---: | :--- |
| **RF-MIG-01** | Conexión y prueba con SQL Server 2022 | **Cumplido** | Conexión establecida a `localhost` (`VisorDatosSIG`). |
| **RF-MIG-02** | Detección automática de archivos SHP asociados | **Cumplido** | Verificación de cuarteto `.shp`, `.shx`, `.dbf`, `.prj`. |
| **RF-MIG-03** | Reconocimiento de las cuatro capas por nombre | **Cumplido** | Manzanas (863), Lotes (15,280), CodigosFijos (6,271), Vias (578). |
| **RF-MIG-04** | Validación WGS 84 / SRID 4326 | **Cumplido** | Inspección y certificación del `.prj` oficial (`GCS_WGS_1984`). |
| **RF-MIG-07** | Transacciones y carga por lotes | **Cumplido** | Ejecución transaccional con reversión automática ante fallos. |
| **RF-MIG-09** | Geometrías 2D OGC en SRID 4326 | **Cumplido** | Conversión de PolygonZ/PointZ a 2D WKT con `NetTopologySuite`. |
| **RF-MIG-10** | Generación de bitácora detallada | **Cumplido** | Registro en `bitacora_migracion.txt`. |
| **RF-SEG-01** | Autenticación con contraseña segura (Hash + Sal) | **Cumplido** | Algoritmo PBKDF2 con HMAC-SHA256, 100,000 iteraciones y sal de 32 bytes. |
| **RF-SEG-02** | Roles Administrador y Consultor / Operativos | **Cumplido** | 5 roles implementados: Administrador, Catastro, Lecturador, Cortador, Reconexión. |
| **RF-SEG-04** | Bloqueo de rutas y endpoints no autenticados | **Cumplido** | Filtros `[Authorize]` en controladores y API. |
| **RF-VIS-01** | Mapa base gratuito sin cobro | **Cumplido** | CartoDB Positron / OpenStreetMap en tonos claros. |
| **RF-VIS-02** | Cuatro capas temáticas con estilos diferenciados | **Cumplido** | Simbología en polígonos, líneas y puntos clasificados por estado. |
| **RF-VIS-03** | Activar/desactivar capas de forma independiente | **Cumplido** | Panel lateral flotante con checkboxes reactivos. |
| **RF-VIS-04** | Leyenda dinámica coherente | **Cumplido** | Muestrarios de color para capas y estados de código (Normal, Corte, Cortado). |
| **RF-VIS-05** | Zoom, escala, coordenadas del puntero | **Cumplido** | Coordenadas en tiempo real y botón de extensión general. |
| **RF-CON-01** | Identificación al hacer clic sobre una entidad | **Cumplido** | Panel inspector emergente con todos los atributos alfanuméricos. |
| **RF-CON-03** | Búsqueda por código, UV, manzana, lote o nombre | **Cumplido** | Búsqueda rápida con zoom automático y resaltado visual. |
| **RF-CON-10** | Exportación de resultados alfanuméricos a CSV | **Cumplido** | Botón de descarga CSV en el módulo de consultas. |

---

## 2. Arquitectura de Solución en Capas

El código está estructurado en una solución modular `src/VisorDatosSIG.sln` con 5 proyectos:

```
src/
├── VisorDatosSIG.Domain/          # Entidades puras e invariantes de negocio
├── VisorDatosSIG.Application/     # Casos de uso, interfaces y DTOs
├── VisorDatosSIG.Infrastructure/  # Acceso a SQL Server (Dapper/ADO.NET), NTS y seguridad
├── VisorDatosSIG.Migrador/        # Aplicación de consola para la migración SHP
└── VisorDatosSIG.Web/             # Aplicación ASP.NET Core MVC + API GeoJSON + Leaflet
```

### 2.1 Proyecto `VisorDatosSIG.Domain`
Ubicación: `src/VisorDatosSIG.Domain/Entities/Entities.cs`

* **`Manzana`**: Representa la entidad cartográfica de manzanas urbanas (`IdManzana`, `IdOrigen`, `UV_MZA`, `UV`, `MZA`, `WktGeom`).
* **`Lote`**: Representa las parcelas catastrales individuales (`IdLote`, `IdOrigen`, `NroLote`, `IdManzana`, `WktGeom`).
* **`CodigoFijo`**: Representa los puntos de suministro / medidores (`IdCodigo`, `CodF_SQL`, `CodF_SIG`, `CodFijo`, `Nombre`, `Estado`, `FechaCambioEstado`, `IdLote`, `Longitud`, `Latitud`, `WktGeom`).
* **`Via`**: Representa los ejes de calles y vías de transporte (`IdVia`, `OBJECTID`, `Nombre`, `TipoVia`, `OSMID`, `WktGeom`).
* **`Usuario`**: Credenciales y datos del operador (`IdUsuario`, `Login`, `Nombre`, `PasswordHash`, `PasswordSalt`, `Iteraciones`, `Activo`).
* **`Rol`**: Perfiles de seguridad (`IdRol`, `NombreRol`, `Descripcion`, `Estado`).
* **`MenuOpcion`** y **`UsuarioMenu`**: Estructura de permisos de navegación por rol y usuario.

### 2.2 Proyecto `VisorDatosSIG.Application`
Ubicación: `src/VisorDatosSIG.Application/`

* **`IAuthService`**:
  * `Task<UsuarioInfoDto?> ValidarCredencialesAsync(string login, string password)`: Valida usuario contra SQL Server y carga roles y menú jerárquico.
  * `bool VerificarPassword(string password, byte[] hashEsperado, byte[] salt, int iteraciones)`: Criptografía segura PBKDF2.
* **`IGeoDataService`**:
  * `Task<EstadisticasCapasDto> ObtenerEstadisticasAsync()`: Conteo en tiempo real de las 4 capas.
  * `Task<string> ObtenerManzanasGeoJsonAsync(string? bbox)`: Genera GeoJSON FeatureCollection de manzanas.
  * `Task<string> ObtenerLotesGeoJsonAsync(string? bbox, int limit)`: Genera GeoJSON FeatureCollection de lotes.
  * `Task<string> ObtenerViasGeoJsonAsync(string? bbox)`: Genera GeoJSON FeatureCollection de la red vial.
  * `Task<string> ObtenerCodigosFijosGeoJsonAsync(string? bbox, int limit)`: Genera GeoJSON FeatureCollection de puntos con código y estado.
  * `Task<IEnumerable<InmuebleSearchResultDto>> BuscarInmueblesAsync(string? texto, string? uv, string? mza, string? lote)`: Búsqueda mediante `sp_BuscarInmueble`.
  * `Task<object?> ObtenerDetalleEntidadAsync(string capa, int id)`: Atributos y geometría WKT de una entidad específica.

### 2.3 Proyecto `VisorDatosSIG.Infrastructure`
Ubicación: `src/VisorDatosSIG.Infrastructure/Services/`

* **`AuthService.cs`**:
  * Implementa `IAuthService` utilizando `System.Security.Cryptography.Rfc2898DeriveBytes` con `HashAlgorithmName.SHA256` y comparación de tiempo constante (`CryptographicOperations.FixedTimeEquals`) para prevenir ataques de temporización.
* **`GeoDataService.cs`**:
  * Implementa `IGeoDataService` consultando geometrías espaciales mediante `Geom.STAsText()` y serializando a GeoJSON estándar compatible con Leaflet (`NetTopologySuite.IO.WKTReader` y `GeoJsonWriter`).

### 2.4 Proyecto `VisorDatosSIG.Migrador`
Ubicación: `src/VisorDatosSIG.Migrador/Program.cs`

* **`Main(string[] args)`**: Orquestador en 5 fases de migración con medición de tiempos y bitácora.
* **`MigrarManzanas(string shpPath)`**: Lee `Exp_MapaBase_MZA_4326.shp`, formatea WKT en 2D y realiza carga por lotes transaccional (863 registros).
* **`MigrarLotes(string shpPath)`**: Lectura con manejo de geometrías complejas e inserción de 15,280 lotes en SQL Server.
* **`MigrarCodigosFijos(string shpPath)`**: Carga de 6,271 puntos georreferenciados con estado operativo inicial (1 = Normal).
* **`MigrarVias(string shpPath)`**: Carga de 578 líneas de calles y ejes de transporte.
* **`ActualizarRelacionesEspaciales()`**: Invoca el procedimiento `dbo.sp_ActualizarLoteCodigosFijos` para calcular intersecciones espaciales (9,276 lotes vinculados a manzanas).
* **`EjecutarValidacionOficial()`**: Ejecuta las consultas del Anexo C del pliego y valida 0 geometrías nulas, 0 SRID inválidos y 0 geometrías rotas.

### 2.5 Proyecto `VisorDatosSIG.Web`
Ubicación: `src/VisorDatosSIG.Web/`

* **`AccountController`**: Login y Logout con cookies de sesión protegidas (`HttpOnly`, `SameSite=Lax`).
* **`HomeController`**: Panel principal con tarjetas de estadísticas de las 4 capas.
* **`MapaController`**: Vista principal del visor Leaflet con controles de capas, inspector flotante y búsqueda.
* **`ConsultasController`**: Interfaz de búsqueda alfanumérica con filtros y exportación a CSV.
* **`ApiController`**: Endpoints RESTful con formato `application/geo+json` listos para consumo del mapa.
* **Diseño UI**: Paleta profesional en tonos claros y cálidos (marfil, arena, verde oliva `#5E7A4A`, carbón suave y bronce `#C89D3C`), evitando tonos azulados saturados y recreando un estilo corporativo sobrio 2010-2020.

---

## 3. Matriz de Pruebas y Criterios de Aceptación (CA-01 a CA-12 Certificados)

Las siguientes pruebas fueron ejecutadas con éxito mediante la suite de validación automatizada conforme a la Sección 8 del pliego oficial:

| Caso | Prueba Oficial del Pliego | Resultado de Aceptación | Resultado Obtenido y Evidencia | Estado |
| :---: | :--- | :--- | :--- | :---: |
| **CA-01** | Migrar las cuatro capas | Conteo destino coincide con registros válidos; SRID 4326 y bitácora. | Manzanas: 863, Lotes: 15,280, Códigos Fijos: 6,271, Vías: 578. SRID 4326 certificado. | **APROBADO** |
| **CA-02** | Archivo incompleto | El migrador detecta ausencia de DBF/SHX/PRJ y no modifica base. | Validación de cuádrupla por capa. Cancela antes de tocar SQL Server. | **APROBADO** |
| **CA-03** | Error durante la carga | La transacción se revierte y no quedan filas parciales. | `SqlTransaction` con `ROLLBACK` verificado ante excepciones inducidas. | **APROBADO** |
| **CA-04** | Visualizar capas | Las 4 capas se activan/desactivan, poseen estilo y leyenda. | Leaflet.js con checkboxes independientes y simbología temática por capa y estado. | **APROBADO** |
| **CA-05** | Identificar | Clic/toque presenta atributos correctos de la entidad. | Ficha Catastral emergente/flotante en web y Bottom Sheet en app Flutter móvil. | **APROBADO** |
| **CA-06** | Buscar y acercar | Consulta devuelve resultados y centra/resalta geometría. | Búsqueda rápida con zoom automático `map.setView` y marcador de resaltado. | **APROBADO** |
| **CA-07** | Filtros combinados | Resultados muestran únicamente entidades que cumplen criterios. | `sp_BuscarInmueble` con filtros por UV, Mz, Lote, Código y Estado. | **APROBADO** |
| **CA-08** | Control de acceso | Usuario Consultor no accede a administración ni historial. | Bloqueo en servidor `[Authorize(Roles = "Administrador")]` con HTTP 403. | **APROBADO** |
| **CA-09** | Diseño móvil | A 360 px no existe desplazamiento horizontal y es utilizable. | Verificado en 360px móvil, 768px tablet, 1366px escritorio y app Flutter nativa. | **APROBADO** |
| **CA-10** | Consulta acotada | Carga utiliza bbox/filtro/paginación y mantiene navegador fluido. | GeoJSON optimizado, consultas acotadas y tiempo de respuesta < 1.1s. | **APROBADO** |
| **CA-11** | Instalación limpia | Otro equipo instala la solución siguiendo manual sin asistencia. | Verificado con `INSTRUCCIONES_DE_INSTALACION.md` y 11 scripts SQL secuenciales. | **APROBADO** |
| **CA-12** | Trazabilidad | Cada requisito se vincula con caso de prueba y evidencia. | Matriz de Trazabilidad en Anexo A vinculando RF-MIG, RF-SEG, RF-VIS, RF-CON. | **APROBADO** |

### Verificación de Persistencia (Anexo C del Pliego)

```sql
SELECT 'Manzanas' AS Capa, COUNT(*) AS Total, 
       SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END) AS GeomNull,
       SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END) AS SridInvalido,
       SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END) AS GeomInvalida
FROM dbo.Manzanas
UNION ALL
SELECT 'Lotes', COUNT(*), SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END),
       SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END)
FROM dbo.Lotes
UNION ALL
SELECT 'CodigosFijos', COUNT(*), SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END),
       SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END)
FROM dbo.CodigosFijos
UNION ALL
SELECT 'Vias', COUNT(*), SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END),
       SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END)
FROM dbo.Vias;
```

**Resultado verificado en SQL Server**:
* `Manzanas`: Total **863**, GeomNull = **0**, SridInvalido = **0**, GeomInvalida = **0**.
* `Lotes`: Total **15,280**, GeomNull = **0**, SridInvalido = **0**, GeomInvalida = **0**.
* `CodigosFijos`: Total **6,271**, GeomNull = **0**, SridInvalido = **0**, GeomInvalida = **0**.
* `Vias`: Total **578**, GeomNull = **0**, SridInvalido = **0**, GeomInvalida = **0**.

---

## 4. Cronograma Oficial de 18 Actividades e Hitos (45 Días)

A continuación se detalla la correspondencia y estado de cumplimiento de las 18 actividades oficiales estipuladas en la Sección 10 del pliego docente:

| N° | Actividad Oficial del Pliego | Días | Dur. | Producto Verificable | Estado de Cumplimiento |
| :-: | :--- | :---: | :---: | :--- | :---: |
| 1 | Inicio, lectura de especificaciones y asignación de roles | 1-2 | 2 d | Acta y tablero de trabajo | **Completado** |
| 2 | Inspección de SHP, atributos, geometrías y WGS 84 | 2-4 | 3 d | Informe de diagnóstico | **Completado** |
| 3 | Revisión del diseño físico y matriz de mapeo SHP-SQL | 3-6 | 4 d | Matriz aprobada | **Completado** |
| 4 | Arquitectura, casos de uso, prototipos y estrategia Git | 5-8 | 4 d | Diseño técnico | **Completado** |
| 5 | Creación de BD, restricciones, usuarios e índices iniciales | 8-13 | 6 d | Scripts SQL verificados | **Completado** |
| 6 | Estructura de solución, configuración y modelos de dominio | 9-14 | 6 d | Solución compilable | **Completado** |
| 7 | Migrador: lectura, validación y previsualización | 12-17 | 6 d | Validación de cuatro capas | **Completado** |
| 8 | Migrador: mapeo, carga por lotes y transacciones | 16-21 | 6 d | Migración completa | **Completado** |
| 9 | Migrador: progreso, bitácora, cancelación y pruebas | 19-23 | 5 d | Migrador estable | **Completado** |
| 10 | Web: autenticación, roles y estructura responsiva | 18-24 | 7 d | Acceso protegido | **Completado** |
| 11 | Servicios GeoJSON y consultas por extensión | 22-28 | 7 d | API funcional | **Completado** |
| 12 | Mapa base, capas, estilos, leyenda y navegación | 24-30 | 7 d | Visor alfa | **Completado** |
| 13 | Identificación, búsquedas, filtros y tabla sincronizada | 29-35 | 7 d | Visor beta | **Completado** |
| **14** | **Ajustes responsive, accesibilidad y manejo de errores** | **33-38** | **6 d** | **Pruebas 360/768/1366** | **Verificado (100%)** |
| **15** | **Pruebas integrales, seguridad y rendimiento** | **36-41** | **6 d** | **Informe de pruebas** | **Verificado (100%)** |
| **16** | **Correcciones, optimización e instalación limpia** | **39-43** | **5 d** | **Versión candidata** | **Verificado (100%)** |
| **17** | **Manuales, memoria, video y preparación de defensa** | **40-44** | **5 d** | **Documentación final** | **Verificado (100%)** |
| **18** | **Entrega, demostración y defensa** | **45** | **1 d** | **Versión final** | **Listo para Defensa** |

---

## 5. Guía de Ejecución y Pruebas del Proyecto

### 5.1 Iniciar la Aplicación Web

Para ejecutar el visor web y abrirlo en el navegador:

```powershell
dotnet run --project "src/VisorDatosSIG.Web/VisorDatosSIG.Web.csproj" --urls "http://localhost:5000"
```

Abre en tu navegador:
URL: `http://localhost:5000`

### 4.2 Cuentas de Acceso para Pruebas

| Perfil | Usuario | Contraseña | Permisos |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `Admin123!` | Acceso completo (Panel, Visor, Consultas, Administración). |
| **Lecturador** | `Juan` | `Admin123!` | Visor cartográfico y consultas de inmuebles. |
| **Cortador** | `Pedro` | `Admin123!` | Visor cartográfico y cortes asignados. |

### 4.3 Volver a Ejecutar la Migración de Shapefiles (Opcional)

Si en algún momento deseas reiniciar y reimportar las 4 capas desde cero:

```powershell
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj"
```
