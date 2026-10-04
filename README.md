# VisorDatosSIG 2026 — Sistema de Información Geográfica

Sistema integral de información geográfica para la gestión territorial, catastral y de servicios de la ciudad de **San Ignacio de Velasco** (Santa Cruz, Bolivia).

Desarrollado bajo una arquitectura limpia en capas sobre **.NET 8.0 C#**, **Microsoft SQL Server 2022** con extensiones espaciales OGC (`geometry` en WGS 84 / SRID 4326), cliente web interactivo en **Leaflet.js** y aplicación móvil nativa en **Flutter**.

---

### Información Académica
* **Materia**: `[2-2026] SISTEMAS DE INFORM.GEOGRAFICA - DI INF442`
* **Docente Evaluador**: Ing. PEREZ FERREIRA UBALDO
* **Semestre Académico**: Semestre 2 - 2026

### Integrantes del Proyecto (Orden Alfabético por Apellido)
| # | Apellidos y Nombres | Registro | Rol Asignado |
| :-: | :--- | :---: | :--- |
| 1 | **Guzman Justiniano**, Nohelia | 222049367 | Líder de Calidad, QA y Documentación |
| 2 | **Jimenez Duarte**, Nils Jonathan | 222008741 | Ingeniero de Datos SIG y Migrador |
| 3 | **López Velásquez**, Marco Alejandro | 222008891 | Arquitecto de Software Backend .NET 8 |
| 4 | **Quispe Tito**, Jorge Gabriel | 222009527 | Desarrollador Frontend Web y Móvil Flutter |

---

## 1. Arquitectura del Sistema

El proyecto VisorDatosSIG implementa el patrón **Clean Architecture** con desacoplamiento en 5 componentes:

* `VisorDatosSIG.Core`: Entidades del dominio espacial y relacional (`Manzana`, `Lote`, `CodigoFijo`, `Via`, `Usuario`, `Bitacora`), enums e interfaces de repositorio.
* `VisorDatosSIG.Data`: Acceso a datos con Dapper 2.1, NetTopologySuite 2.5, Microsoft SQL Server 2022 Spatial y autenticación criptográfica PBKDF2 (HMAC-SHA256, 100,000 iteraciones).
* `VisorDatosSIG.Migrador`: Módulo de consola interactivo y CLI para previsualización de 20 registros, mapeo explícito de campos (RF-MIG-05), modalidades Reemplazar/Anexar (RF-MIG-06), exportación de resumen a CSV (RF-MIG-13) y reconstrucción de índices espaciales (RF-MIG-14).
* `VisorDatosSIG.Web`: Aplicación web ASP.NET Core 8.0 MVC con API RESTful GeoJSON, visor Leaflet.js, catálogo de capas, historial de migraciones, control de acceso RBAC y bitácora de auditoría.
* `VisorDatosSIG.Mobile`: Aplicación móvil nativa compilada en Flutter 3.41 / Dart 3.11 con `flutter_map`, OpenStreetMap, panel inferior deslizable (Bottom Sheet Inspector Mockup F.7) y geolocalización GPS en tiempo real.

---

## 2. Métricas Oficiales de Geodatos en SQL Server 2022

| Capa Cartográfica | Tabla SQL Destino | Registros | Tipo Geométrico | SRID Oficial | Conformidad OGC |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **Manzanas** | `dbo.Manzanas` | **863** | MultiPolygon | 4326 (WGS 84) | 100% Válidas (0 nulos, 0 inválidos) |
| **Lotes (Predios)** | `dbo.Lotes` | **15,280** | MultiPolygon | 4326 (WGS 84) | 100% Válidas (0 nulos, 0 inválidos) |
| **Códigos Fijos** | `dbo.CodigosFijos` | **6,271** | Point | 4326 (WGS 84) | 100% Válidas (0 nulos, 0 inválidos) |
| **Red Vial** | `dbo.Vias` | **578** | MultiLineString | 4326 (WGS 84) | 100% Válidas (0 nulos, 0 inválidos) |
| **Total Entidades** | — | **22,992** | — | 4326 (WGS 84) | 100% Aprobado (Script Anexo C) |

---

## 3. Modelo de Roles y Control de Acceso (RBAC)

El sistema aplica el principio de mínimo privilegio en servidor:

| Rol | Usuario Semilla | Contraseña | Alcance y Permisos |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `Admin123!` | Acceso total: Gestión de usuarios, roles, catálogo de capas, historial de migraciones, auditoría y visor. |
| **Operador** | `operador` | `Admin123!` | Gestión operativa de campo: Visor cartográfico y actualización del estado de suministros. |
| **Consultor** | `consultor` | `Admin123!` | Análisis y fiscalización: Visor de solo lectura, búsqueda temática y exportación a CSV. Acceso bloqueado a `/Admin/*`. |
| **Lecturador** | `Juan` | `Admin123!` | Operación de campo: Consulta cartográfica y registro de lecturas. |
| **Cortador** | `Pedro` | `Admin123!` | Operación de campo: Consulta de predios con estado 'Para Corte' y 'Cortado'. |

---

## 4. Estructura de Menú Web y Matriz de Permisos (Anexo E)

La aplicación web reproduce la navegación en 6 módulos estipulada en el Anexo E del pliego:

1. **1.0 Inicio**: Bienvenida, accesos directos y panel de métricas generales (`/Home/Index`).
2. **2.0 Visor Cartográfico**: Mapa interactivo a pantalla completa con conmutador de capas, simbología semafórica, escala gráfica y buscador (`/Home/Index` en modo visor).
3. **3.0 Consultas y Búsqueda Temática**: Búsqueda multicriterio (UV, MZA, Lote, Titular, Estado) con exportación a CSV (`/Inmuebles/Index`).
4. **4.0 Administración y Capas**:
   * 4.1 Panel Administrativo (`/Admin/Index`)
   * 4.2 Catálogo de Capas (`/Admin/CatalogoCapas`)
   * 4.3 Historial de Migraciones (`/Admin/HistorialMigraciones`)
5. **5.0 Seguridad y Auditoría**:
   * 5.1 Gestión de Usuarios y Roles (`/Admin/Usuarios`, `/Admin/Roles`)
   * 5.2 Bitácora de Auditoría Inmutable (`/Admin/Bitacora`)
6. **6.0 Ayuda y Soporte**:
   * 6.0 Manual de Usuario (`/Home/Manual`)
   * 6.1 Acerca del Sistema (`/Home/AcercaDe`)
   * Perfil del Usuario Autenticado (`/Account/Perfil`)

---

## 5. Aplicación Móvil Nativa Flutter (`src/VisorDatosSIG.Mobile`)

La aplicación móvil nativa complementa el visor web:

* **Framework**: Flutter 3.41.7 / Dart 3.11.5.
* **Cartografía**: `flutter_map` con mosaicos OpenStreetMap y renderizado de geometrías vectoriales.
* **Inspector Móvil tipo Bottom Sheet (Mockup F.7)**: Al tocar cualquier lote o medidor, se despliega una tarjeta inferior deslizable con los atributos del predio sin ocultar el mapa.
* **Geolocalización GPS**: Botón de ubicación actual con coordenadas en tiempo real y radio de precisión.
* **Búsqueda Avanzada**: Filtros rápidos por Unidad Vecinal (UV), Manzana, Código Fijo y tipo de predio.

---

## 6. Documento Técnico Oficial (13 Secciones y 6 Anexos)

La memoria técnica del proyecto (`VisorDatosSIG_Documento_Tecnico_Oficial.docx`), ubicada en la raíz y en `04_Documentacion/`, reproduce punto por punto la estructura del pliego docente:

* **Sección 1**: Identificación y Propósito
* **Sección 2**: Alcance del Proyecto
* **Sección 3**: Datos Geográficos de Entrada y Reglas de Mapeo
* **Sección 4**: Arquitectura de Solución (.NET 8 Clean Architecture y Flutter)
* **Sección 5**: Requisitos Funcionales del Sistema (RF-MIG, RF-SEG, RF-VIS, RF-CON)
* **Sección 6**: Requisitos Técnicos y Persistencia Espacial SQL Server 2022
* **Sección 7**: Diseño de Seguridad Criptográfica y Manejo de Contingencias
* **Sección 8**: Pruebas y Criterios de Aceptación (CA-01 a CA-12 Certificados)
* **Sección 9**: Entregables del Proyecto y Estructura Física (E1 a E8)
* **Sección 10**: Cronograma de Actividades e Hitos (45 Días Calendario)
* **Sección 11**: Organización del Equipo de Trabajo y Rúbrica de Evaluación
* **Sección 12**: Gestión de Riesgos Técnicos y Medidas Preventivas
* **Sección 13**: Anexos Normativos y Técnicos
  * **Anexo A**: Matriz de Trazabilidad de Requisitos
  * **Anexo B**: Checklist Oficial de Verificación del Sistema
  * **Anexo C**: Consultas SQL de Validación OGC y Cobertura Catastral
  * **Anexo D**: Mejoras e Innovaciones Implementadas (Flutter, GPS, 15,280 Predios, Menú Interactivo)
  * **Anexo E**: Estructura de Menú y Matriz de Permisos RBAC
  * **Anexo F**: Fichas Técnicas de Mockups de Pantalla (F.1 a F.7)

---

## 7. Guía de Instalación y Puesta en Marcha

### Requisitos Previos
1. Windows 10 u 11 (x64).
2. .NET 8.0 SDK instalado (`dotnet --version`).
3. Microsoft SQL Server 2022 (Developer o Express en `localhost`).
4. SQL Server Management Studio (SSMS).
5. Flutter SDK 3.41+ (para la aplicación móvil).

---

### Paso 1: Configurar la Base de Datos en SQL Server
1. Abre SSMS y conéctate a la instancia local (`localhost`).
2. Ejecuta el script de creación:
   `ScriptDatabaseV13\01_CrearBD.sql`
3. Ejecuta los scripts complementarios:
   * `ScriptDatabaseV13\04_Actualizar_CodigosFijos_Estado.sql`
   * `ScriptDatabaseV13\05_Optimizar_Relacion_CodigoFijo_Lote.sql`
   * `ScriptDatabaseV13\06_Agregar_Nombre_Vias.sql`
   * `ScriptDatabaseV13\07_Roles_Usuarios_Menu.sql`
   * `ScriptDatabaseV13\08_MenuOpciones_UsuarioMenu.sql`
   * `ScriptDatabaseV13\09_Procedimiento_BuscarPrediosTotal.sql`

---

### Paso 2: Ejecutar el Migrador de Geodatos

El migrador cuenta con menú interactivo y soporte CLI por argumentos:

```powershell
# Modo Interactivo (Menú en Consola):
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj"

# Modo Automático por Argumentos:
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj" -- --preview
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj" -- --migrar-reemplazar
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj" -- --rebuild-indexes
```

---

### Paso 3: Iniciar la Aplicación Web

```powershell
dotnet run --project "src/VisorDatosSIG.Web/VisorDatosSIG.Web.csproj" --urls "http://localhost:5000"
```

Ingresa desde el navegador a: **`http://localhost:5000`**

---

### Paso 4: Ejecutar la Aplicación Móvil en Flutter

```powershell
cd "src/VisorDatosSIG.Mobile"
flutter pub get
flutter analyze
flutter run -d chrome
# O para emulador/dispositivo Android:
# flutter run -d android
```

---

## 8. Verificación OGC de Geometrías (Script Oficial Anexo C)

```sql
USE VisorDatosSIG;
GO
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

**Resultado verificado**: 0 nulos, 0 SRID inválidos y 0 geometrías inválidas en las 22,992 entidades.
