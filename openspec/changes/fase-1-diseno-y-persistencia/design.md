## Context

Ver proposal.md para motivación y justificación general. El proyecto parte de un repositorio vacío vinculado a GitHub (marcolop450/Proyecto_SIG), con 4 paquetes SHP entregados en DatosSIG_Reproj/ (Manzanas, Lotes, CodigosFijos y Vias en WGS 84 / SRID 4326) y un conjunto de scripts T-SQL en ScriptDatabaseV13/. El entorno oficial solicitado por la FICCT - UAGRM es Visual Studio / .NET y SQL Server 2022 con herramientas espaciales.

## Goals / Non-Goals

**Goals:**
- Validar formalmente la integridad de los shapefiles y su correspondencia con el modelo relacional.
- Proporcionar la estructura limpia y scripts ordenados de persistencia SQL Server (DDL, DML inicial, roles, índices espaciales).
- Establecer la arquitectura de la solución (.NET C# en capas) y la estrategia Git requerida para el Hito del 22 de septiembre.
- Documentar el diagnóstico técnico y la matriz de mapeo para entrega académica (Hitos H1 y H2).

**Non-Goals:**
- Implementar la interfaz gráfica final del visor web en esta etapa (corresponde a fases posteriores, Visor Alfa para octubre).
- Edición cartográfica interactiva en el cliente web (fuera de alcance del proyecto general).

## Decisions

### Decisión 1: Persistencia espacial con SQL Server 2022 y tipo geometry (SRID 4326)
- **Opción seleccionada**: Utilizar el tipo geometry con SRID 4326 y convención X = Longitud, Y = Latitud, con índices espaciales GEOMETRY_GRID.
- **Alternativas consideradas**: Tipo geography de SQL Server o PostgreSQL/PostGIS. Se descartó PostGIS porque el pliego de especificaciones oficial exige expresamente SQL Server 2022 (RT-04). Se mantuvo geometry según los scripts base entregados por la cátedra.

### Decisión 2: Arquitectura en capas limpia en .NET
- **Opción seleccionada**: Solución multicapa modular:
  - VisorDatosSIG.Domain: Entidades del dominio SIG (Manzana, Lote, CodigoFijo, Via, Usuario, Rol).
  - VisorDatosSIG.Application: Interfaces de servicios, DTOs y validaciones espaciales.
  - VisorDatosSIG.Infrastructure: Implementación de persistencia con ADO.NET/Dapper o EF Core, y lectura de SHP con NetTopologySuite.
  - VisorDatosSIG.Migrador: Módulo de migración con validación por lotes, transacciones y bitácora.
  - VisorDatosSIG.Web / VisorDatosSIG.Api: Controladores MVC/Razor, Leaflet y endpoints GeoJSON.
- **Alternativas consideradas**: Aplicación monolítica sin separación. Se descartó para cumplir con los requisitos arquitectónicos obligatorios (sección 4 del pliego).

### Decisión 3: Proyección y lectura de Shapefiles
- **Opción seleccionada**: Verificar obligatoriamente la presencia de .prj, corroborar la referencia WGS 84 (EPSG:4326) y rechazar silencios de proyección. Los tipos encontrados son PointZ, PolygonZ y PolyLine.

## Risks / Trade-offs

- **[Riesgo: Ausencia de .NET SDK en la máquina local]** → El host actual solo tiene runtime .NET 6.0 sin SDK compilador. Mitigación: Indicar al usuario la instalación de .NET SDK (.NET 8/10 o Visual Studio 2026/2022).
- **[Riesgo: Servicio de SQL Server 2022 no presente o inactivo]** → La máquina tiene PostgreSQL 18 activo, pero no SQL Server. Mitigación: Proporcionar instrucciones manuales exactas para instalar SQL Server 2022 Developer / Express y SSMS.
- **[Riesgo: Autenticación remota de Git]** → El push hacia GitHub requerirá credenciales del usuario (marcolop450). Mitigación: Preparar el commit inicial y dejar el comando git push listo para ejecución por parte del usuario.
