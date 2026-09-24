# VisorDatosSIG 2026 — Geo Visor de Información Geográfica

Sistema integral de información geográfica para la gestión territorial, catastral y de servicios de la ciudad de **San Ignacio de Velasco** (Santa Cruz, Bolivia).

Desarrollado bajo una arquitectura limpia en capas sobre **.NET 8.0 C#**, **Microsoft SQL Server 2022** con extensiones espaciales OGC (`geometry` en WGS 84 / SRID 4326) y cliente interactivo en **Leaflet.js**.

---

### Información Académica
* **Materia**: `[2-2026] SISTEMAS DE INFORM.GEOGRAFICA - DI INF442`
* **Docente**: Ing. PEREZ FERREIRA UBALDO
* **Semestre**: Semestre 2 - 2026

### Integrantes del Proyecto
| # | Integrante (Orden Alfabético por Apellido) | Registro |
| :-: | :--- | :---: |
| 1 | **Guzman Justiniano**, Nohelia | 222049367 |
| 2 | **Jimenez Duarte**, Nils Jonathan | 222008741 |
| 3 | **López Velásquez**, Marco Alejandro | 222008891 |
| 4 | **Quispe Tito**, Jorge Gabriel | 222009527 |

---

## 1. Presentación y Arquitectura del Sistema

El proyecto VisorDatosSIG es una plataforma empresarial diseñada para modernizar y centralizar el catastro municipal y la gestión de suministros de servicios básicos. Permite la visualización de capas cartográficas, la consulta multicriterio de predios y códigos fijos de medidores, la edición controlada del estado operativo de los suministros y la trazabilidad integral de eventos.

### Principios Arquitectónicos
* **Arquitectura Limpia (Clean Architecture)**: Desacoplamiento estricto en 5 capas:
  * `VisorDatosSIG.Domain`: Entidades de dominio puras (`Manzana`, `Lote`, `CodigoFijo`, `Via`, `Usuario`, `Rol`).
  * `VisorDatosSIG.Application`: Interfaces de servicio, DTOs de transporte y contratos de negocio.
  * `VisorDatosSIG.Infrastructure`: Implementación con Dapper, NetTopologySuite, Microsoft SQL Server 2022 Spatial y autenticación PBKDF2 (HMAC-SHA256).
  * `VisorDatosSIG.Migrador`: Módulo de consola transaccional para la carga masiva y validación de shapefiles ESRI.
  * `VisorDatosSIG.Web`: Aplicación web ASP.NET Core MVC con API GeoJSON y cliente Leaflet.js.
* **Seguridad Criptográfica**: Derivación de contraseñas con `Rfc2898DeriveBytes` (PBKDF2 SHA-256, 100,000 iteraciones y sal criptográfica de 32 bytes).
* **Ausencia Total de Emojis**: Interfaz profesional utilizando tipografías distinguidas (`Space Grotesk`, `Plus Jakarta Sans`, `JetBrains Mono`) e iconografía vectorial normalizada con **Bootstrap Icons** (`bi`).

---

## 2. Métricas de Geodatos Migrados

| Capa | Entidad | Registros | Tipo Geométrico | SRID Oficial | Relación Espacial |
| :--- | :--- | :---: | :---: | :---: | :--- |
| **Manzanas** | `dbo.Manzanas` | **863** | MultiPolygon | 4326 (WGS 84) | Polígonos base urbanos |
| **Lotes** | `dbo.Lotes` | **15,280** | MultiPolygon | 4326 (WGS 84) | 9,276 lotes asociados a su Manzana |
| **Códigos Fijos** | `dbo.CodigosFijos` | **6,271** | Point | 4326 (WGS 84) | 5,118 códigos fijos vinculados a su Lote |
| **Red Vial** | `dbo.Vias` | **578** | PolyLine | 4326 (WGS 84) | Ejes de calles y avenidas |

---

## 3. Modelo de Roles y Control de Acceso (RBAC)

El sistema implementa control de acceso basado en roles con el principio de mínimo privilegio:

| Rol | Perfil de Usuario | Alcance y Permisos |
| :--- | :--- | :--- |
| **Administrador** | `admin` | **Control Total**: Gestión de cuentas de usuario, asignación de roles, configuración global de parámetros de visualización, auditoría de bitácora y edición de suministros. |
| **Operador** | `operador` | **Gestión Operativa de Campo**: Consulta cartográfica y actualización del estado de suministros (Normal -> Para Corte -> Cortado -> Baja). Sin acceso a configuración ni gestión de usuarios. |
| **Consultor / Auditor** | `consultor` | **Análisis y Fiscalización**: Acceso completo de solo lectura al mapa interactivo, búsqueda multicriterio de predios, inspección de fichas técnicas y exportación de reportes tabulares a formato CSV. |

### Credenciales Semilla Predeterminadas
* **Administrador**: Usuario `admin` | Contraseña `Admin123!`
* **Operador**: Usuario `operador` | Contraseña `Admin123!`
* **Consultor**: Usuario `consultor` | Contraseña `Admin123!`
* **Lecturador**: Usuario `Juan` | Contraseña `Admin123!`
* **Cortador**: Usuario `Pedro` | Contraseña `Admin123!`

---

## 4. ¿Qué es el Servicio GeoJSON en VisorDatosSIG?

**GeoJSON** es un estándar abierto de intercambio de datos geoespaciales basado en JSON (especificación RFC 7946). Permite estructurar entidades geográficas combinando:
1. **Geometría espacial**: Puntos (`Point`), cadenas de líneas (`LineString`) o polígonos (`Polygon` / `MultiPolygon`) codificados en coordenadas latitud/longitud en WGS 84.
2. **Propiedades alfanuméricas**: Atributos asociados al elemento (ej. código fijo, nombre del titular, UV, manzana, estado de suministro).

### Implementación Técnica en VisorDatosSIG
En la aplicación, los endpoints ubicados en `/api/capas/*`:
* Consultan las geometrías almacenadas nativamente en SQL Server 2022 mediante `Geom.STAsText()`.
* Utilizan **NetTopologySuite** y serializadores JSON de alto rendimiento para generar un objeto `FeatureCollection`.
* Transmiten el payload bajo el MIME type oficial `application/geo+json`.
* El cliente web en **Leaflet.js** consume directamente este servicio, permitiendo renderizado vectorial acelerado por hardware, resaltado dinámico, clústeres y capas interactivas.

---

## 5. Guía de Instalación y Puesta en Marcha

### Requisitos Previos
1. **Windows 10 / 11 (x64)**.
2. **Git**: Instalado y disponible en el PATH ([git-scm.com](https://git-scm.com/)).
3. **.NET 8.0 SDK**: Instalado ([dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)).
4. **Microsoft SQL Server 2022** (Developer o Express Edition en `localhost`).
5. **SQL Server Management Studio (SSMS)**.

---

### Paso 1: Clonar el Repositorio
```powershell
git clone https://github.com/marcolop450/Proyecto_SIG.git
cd "Proyecto_SIG"
```

---

### Paso 2: Crear la Base de Datos en SQL Server
1. Abre **SSMS** y conéctate a tu instancia local (`localhost`).
2. Abre y ejecuta el script principal:
   `ScriptDatabaseV13\01_CrearBD.sql` (Presiona `F5`).
3. Ejecuta los scripts complementarios de roles y vistas si requieres ajustes específicos:
   * `ScriptDatabaseV13\04_Actualizar_CodigosFijos_Estado.sql`
   * `ScriptDatabaseV13\05_Optimizar_Relacion_CodigoFijo_Lote.sql`
   * `ScriptDatabaseV13\06_Agregar_Nombre_Vias.sql`
   * `ScriptDatabaseV13\07_Roles_Usuarios_Menu.sql`

---

### Paso 3: Ejecutar la Migración de Shapefiles
Desde la raíz del proyecto, ejecuta el migrador de consola:

```powershell
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj"
```

El proceso realizará automáticamente:
1. Verificación de archivos ESRI obligatorios (`.shp`, `.shx`, `.dbf`, `.prj`) en WGS 84.
2. Limpieza y resecuenciación en cascada de tablas preexistentes.
3. Inserción transaccional de 863 Manzanas, 15,280 Lotes, 6,271 Códigos Fijos y 578 Vías.
4. Extracción de coordenadas geométricas reales `(X=Longitud, Y=Latitud)` directamente de los binarios SHP.
5. Ejecución del procedimiento `sp_ActualizarLoteCodigosFijos` para vincular lotes a manzanas y medidores a lotes.
6. Validación formal de geometrías según el Anexo C del Pliego.

---

### Paso 4: Iniciar la Aplicación Web
Para iniciar el servidor web:

```powershell
dotnet run --project "src/VisorDatosSIG.Web/VisorDatosSIG.Web.csproj" --urls "http://localhost:5000"
```

Abre tu navegador e ingresa a:
**`http://localhost:5000`**

---

## 6. Pruebas de Verificación y Calidad

### Prueba 1: Búsqueda del Código Fijo 1001 (Validación de Coordenadas)
Para verificar que el punto 1001 se encuentra ubicado en San Ignacio de Velasco y no en coordenadas erróneas:
```sql
USE VisorDatosSIG;
GO
EXEC dbo.sp_BuscarInmueble @Texto = '1001';
```
**Resultado esperado**:
* `CodFijo`: 1001
* `Nombre`: DORADO GREGORIA MONTERO de
* `UV`: U.V. 04 | `MZA`: Mz. 14 | `NroLote`: L50
* `Latitud`: -16.384380 | `Longitud`: -60.959624

### Prueba 2: Validación OGC de Geometrías (Anexo C)
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
**Resultado esperado**: 0 geometrías nulas, 0 SRID inválidos y 0 geometrías inválidas en todas las capas.

---

## 7. Estructura del Repositorio

```
Proyecto_SIG/
|-- 04_Documentacion/               # Documentación formal de hitos y diagnóstico
|   |-- 01_Acta_Inicio_Tablero.md
|   |-- 02_Informe_Diagnostico_Geodatos.md
|   |-- 03_Matriz_Mapeo_SHP_SQL.md
|   `-- 04_Diseno_Tecnico_Arquitectura.md
|-- DatosSIG_Reproj/                # Shapefiles originales en WGS 84
|-- ScriptDatabaseV13/              # Scripts DDL, índices y procedimientos almacenados
|-- DOCUMENTACION_TECNICA.md        # Especificación detallada de clases y métodos
|-- INSTRUCCIONES_DE_INSTALACION.md # Guía paso a paso para despliegue
|-- bitacora_migracion.txt          # Registro histórico de ingesta cartográfica
|
`-- src/                            # Solución .NET 8 en Capas
    |-- VisorDatosSIG.sln
    |-- VisorDatosSIG.Domain/       # Modelos y entidades del dominio
    |-- VisorDatosSIG.Application/  # DTOs, interfaces de repositorio y servicios
    |-- VisorDatosSIG.Infrastructure/# Implementación SQL Server 2022 Spatial y Auth
    |-- VisorDatosSIG.Migrador/     # Herramienta CLI de carga transaccional
    `-- VisorDatosSIG.Web/          # Controladores MVC, API GeoJSON y Visor Leaflet
```
