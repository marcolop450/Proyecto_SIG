# VisorDatosSIG 2026 — Geo Visor de Información Geográfica

Sistema integral de información geográfica para la gestión territorial, catastral y de servicios de la ciudad de **San Ignacio de Velasco** (Santa Cruz, Bolivia).

Desarrollado bajo una arquitectura limpia en capas sobre **.NET 8.0 C#**, **Microsoft SQL Server 2022** con extensiones espaciales OGC (`geometry` en WGS 84 / SRID 4326) y cliente interactivo en **Leaflet.js**.

---

## 📌 Características Principales

* **Arquitectura Limpia en Capas**: Desacoplamiento estricto en 5 proyectos (`Domain`, `Application`, `Infrastructure`, `Migrador`, `Web`).
* **Motor Espacial Robusto**: Almacenamiento nativo en SQL Server 2022 con tipo `geometry`, 4 índices espaciales `GEOMETRY_GRID` y procedimientos almacenados optimizados.
* **Migrador Automatizado**: Lector transaccional de shapefiles ESRI que valida componentes obligatorios (`.shp`, `.shx`, `.dbf`, `.prj`), depura geometrías 2D y calcula relaciones espaciales entre capas.
* **Visor Cartográfico Interactivo**:
  * Mosaicos base 100% libres y sin marcas de agua (**OpenStreetMap**, **OpenTopoMap** y **Esri Satelital**).
  * Control independiente de capas temáticas con leyenda dinámica por estados de servicio.
  * Niveles de zoom ampliados desde escala regional departamental hasta nivel predial submétrico.
  * Inspector lateral de atributos al hacer clic sobre cualquier elemento.
  * Búsqueda ágil por código fijo, titular, UV, manzana o lote con centrado automático y resaltado.
* **Seguridad y Auditoría**: Autenticación mediante derivación de claves PBKDF2 (HMAC-SHA256 con 100,000 iteraciones y sal de 32 bytes), control de acceso por roles y registro de bitácora en tiempo real.

---

## 📊 Métricas de Geodatos Migrados

| Capa | Entidad | Registros | Tipo Geométrico | SRID Oficial |
| :--- | :--- | :---: | :---: | :---: |
| **Manzanas** | `dbo.Manzanas` | **863** | MultiPolygon | 4326 (WGS 84) |
| **Lotes** | `dbo.Lotes` | **15,280** | MultiPolygon | 4326 (WGS 84) |
| **Códigos Fijos** | `dbo.CodigosFijos` | **6,271** | Point | 4326 (WGS 84) |
| **Red Vial** | `dbo.Vias` | **578** | PolyLine | 4326 (WGS 84) |

* **Relaciones Espaciales**: `9,276 lotes` asociados automáticamente a sus manzanas y códigos fijos vinculados a sus respectivos predios mediante `sp_ActualizarLoteCodigosFijos`.

---

## 🛠️ Guía Rápida de Instalación y Despliegue

### 1. Requisitos del Sistema
* Sistema Operativo: Windows 10 / 11 (x64).
* **Git**: [git-scm.com](https://git-scm.com/)
* **.NET 8.0 SDK**: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* **Microsoft SQL Server 2022** (Developer o Express Edition en `localhost`).
* **SQL Server Management Studio (SSMS)**.

---

### 2. Clonar el Repositorio
```powershell
git clone https://github.com/marcolop450/Proyecto_SIG.git
cd "Proyecto_SIG"
```

---

### 3. Crear la Base de Datos en SQL Server
1. Abre **SSMS** y conéctate a tu instancia local (`localhost`).
2. Abre y ejecuta el script maestro DDL ubicado en:
   📁 `ScriptDatabaseV13\01_CrearBD.sql` (Presiona `F5`).
3. Esto creará la base de datos `VisorDatosSIG` con sus 9 tablas, índices espaciales, roles y procedimientos almacenados.

---

### 4. Ejecutar la Migración de Shapefiles
Desde la raíz del proyecto, ejecuta el migrador de consola:

```powershell
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj"
```

El proceso leerá las 4 capas desde `DatosSIG_Reproj/`, insertará las geometrías en SQL Server, ejecutará la actualización de relaciones espaciales y generará la auditoría en `bitacora_migracion.txt`.

---

### 5. Iniciar la Aplicación Web
Para levantar el servidor web:

```powershell
dotnet run --project "src/VisorDatosSIG.Web/VisorDatosSIG.Web.csproj" --urls "http://localhost:5000"
```

Abre tu navegador e ingresa a:
👉 **`http://localhost:5000`**

---

## 🔑 Credenciales de Acceso

| Perfil de Usuario | Cuenta | Contraseña | Alcance y Permisos |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `Admin123!` | Acceso total: Panel, Visor, Consultas, Configuración y Bitácora. |
| **Lecturador** | `Juan` | `Admin123!` | Visor cartográfico y consulta temática de medidores. |
| **Cortador** | `Pedro` | `Admin123!` | Visor cartográfico y seguimiento de órdenes de corte. |

---

## 📁 Estructura del Proyecto

```
Proyecto_SIG/
├── 04_Documentacion/               # Documentación formal de hitos y diagnóstico
│   ├── 01_Acta_Inicio_Tablero.md
│   ├── 02_Informe_Diagnostico_Geodatos.md
│   ├── 03_Matriz_Mapeo_SHP_SQL.md
│   └── 04_Diseno_Tecnico_Arquitectura.md
├── DatosSIG_Reproj/                # Shapefiles originales en WGS 84
├── ScriptDatabaseV13/              # Scripts SQL oficiales (DDL, usuarios, roles)
├── DOCUMENTACION_TECNICA.md        # Documentación técnica exhaustiva de clases y métodos
├── INSTRUCCIONES_DE_INSTALACION.md # Guía detallada de instalación
├── bitacora_migracion.txt          # Registro histórico de la carga de datos
│
└── src/                            # Solución C# .NET 8 en Capas
    ├── VisorDatosSIG.sln
    ├── VisorDatosSIG.Domain/       # Entidades Manzana, Lote, CodigoFijo, Via, Usuario
    ├── VisorDatosSIG.Application/  # Interfaces, servicios y DTOs
    ├── VisorDatosSIG.Infrastructure/# Implementación SQL Server, NTS y AuthService
    ├── VisorDatosSIG.Migrador/     # Módulo transaccional de ingesta cartográfica
    └── VisorDatosSIG.Web/          # Controladores MVC, API GeoJSON y Visor Leaflet
```

---

## 🧪 Pruebas de Persistencia y Calidad (Anexo C)

Para verificar la integridad de la base de datos en SSMS:

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

**Resultado esperado**: 0 geometrías nulas, 0 SRID inválidos y 0 geometrías corruptas en todas las tablas.
