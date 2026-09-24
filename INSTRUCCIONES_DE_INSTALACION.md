# Guía de Instalación y Despliegue Local — VisorDatosSIG 2026

Esta guía detalla los pasos para clonar, configurar e iniciar el proyecto desde cero en cualquier equipo con sistema operativo Windows.

---

## 1. Requisitos Previos del Sistema

Antes de comenzar, asegúrate de tener instaladas las siguientes herramientas en tu sistema:

1. **Git**: [git-scm.com](https://git-scm.com/)
2. **.NET 8.0 SDK**: [.NET 8 SDK Installer](https://dotnet.microsoft.com/download/dotnet/8.0)  
   *(Verificar abriendo PowerShell y ejecutando `dotnet --version` — debe indicar `8.0.x`)*
3. **Microsoft SQL Server 2022** (Developer o Express Edition): [SQL Server Downloads](https://www.microsoft.com/sql-server/sql-server-downloads)  
   *Instancia predeterminada recomendada: `localhost` o `MSSQLSERVER` con Autenticación de Windows.*
4. **SQL Server Management Studio (SSMS)**: [Descargar SSMS](https://aka.ms/ssmsfullsetup)

---

## 2. Clonar el Repositorio

Abre PowerShell o Terminal y clona el proyecto:

```powershell
git clone https://github.com/marcolop450/Proyecto_SIG.git
cd "Proyecto_SIG"
```

---

## 3. Configuración de la Base de Datos

### Paso 3.1: Ejecutar el script maestro DDL
Abre SSMS, conéctate a tu servidor local (`localhost`) y abre el archivo:
`ScriptDatabaseV13\01_CrearBD.sql`

Ejecuta el script completo (presiona `F5`).  
Esto creará:
* La base de datos `VisorDatosSIG`.
* Las tablas del modelo: `Manzanas`, `Lotes`, `CodigosFijos`, `Vias`, `Usuarios`, `Roles`, `MenuOpciones`, etc.
* Los 4 índices espaciales `GEOMETRY_GRID`.
* Los procedimientos almacenados (`sp_BuscarInmueble`, `sp_ActualizarLoteCodigosFijos`).
* Los usuarios semilla iniciales (`admin`, `Juan`, `Pedro`).

---

## 4. Migración de los Shapefiles a SQL Server

El proyecto incluye un módulo automatizado que lee los shapefiles de `DatosSIG_Reproj/`, valida su proyección WGS 84 (SRID 4326), inserta las geometrías en SQL Server y calcula las relaciones espaciales.

Para ejecutar la migración:

```powershell
dotnet run --project "src/VisorDatosSIG.Migrador/VisorDatosSIG.Migrador.csproj"
```

**Resultado esperado:**
* Manzanas: **863** insertadas.
* Lotes: **15,280** insertados.
* Códigos Fijos: **6,271** insertados.
* Vías: **578** insertadas.
* Relaciones espaciales: **5,118** Códigos Fijos asociados a sus respectivos Lotes y **9,276** Lotes asociados a sus Manzanas.
* Se genera el archivo de auditoría `bitacora_migracion.txt`.

---

## 5. Ejecución del Visor Web

Para iniciar el servidor web interactivo:

```powershell
dotnet run --project "src/VisorDatosSIG.Web/VisorDatosSIG.Web.csproj" --urls "http://localhost:5000"
```

Abre tu navegador e ingresa a:
**`http://localhost:5000`**

---

## 6. Credenciales de Acceso

| Perfil | Cuenta de Usuario | Contraseña | Permisos |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `Admin123!` | Acceso completo (Panel, Visor, Consultas, Configuración y Bitácora). |
| **Consultor** | `consultor` | `Admin123!` | Visualización, búsqueda temática, análisis cartográfico y descarga CSV. |
| **Operador** | `operador` | `Admin123!` | Visor cartográfico y actualización de estados de suministro. |
| **Lecturador** | `Juan` | `Admin123!` | Visor cartográfico y consulta temática de inmuebles. |
| **Cortador** | `Pedro` | `Admin123!` | Visor cartográfico y monitoreo operativo. |

---

## 7. Solución de Problemas Frecuentes

* **Error de conexión a SQL Server**:
  Verifica que el servicio esté corriendo en PowerShell:
  ```powershell
  Get-Service MSSQLSERVER
  ```
  Si el servicio está detenido, inícialo con:
  ```powershell
  Start-Service MSSQLSERVER
  ```

* **Cambiar la cadena de conexión**:
  Si tu instancia no es `localhost`, edita el archivo:
  `src/VisorDatosSIG.Web/appsettings.json`
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=VisorDatosSIG;Integrated Security=True;TrustServerCertificate=True;"
  }
  ```
