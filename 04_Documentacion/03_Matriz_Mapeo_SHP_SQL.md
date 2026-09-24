# Matriz Formal de Mapeo SHP a SQL Server 2022

## 1. Capa Manzanas (`Exp_MapaBase_MZA_4326.shp` -> `dbo.Manzanas`)

| Campo DBF | Tipo Origen | Columna SQL | Tipo Destino | Regla de Transformación |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | Integer | `IdOrigen` | INT NULL | Copia directa |
| `UV_MZA` | String(20) | `UV_MZA` | NVARCHAR(20) NULL | Trim de espacios |
| `UV` | String(15) | `UV` | NVARCHAR(15) NULL | Trim de espacios |
| `MZA` | String(10) | `MZA` | NVARCHAR(10) NULL | Trim de espacios |
| *Geometry* | PolygonZ | `Geom` | `geometry` (SRID 4326) | Conversión a 2D OGC WKT |

## 2. Capa Lotes (`Exp_MapaBase_LOTES_4326.shp` -> `dbo.Lotes`)

| Campo DBF | Tipo Origen | Columna SQL | Tipo Destino | Regla de Transformación |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | Integer | `IdOrigen` | INT NULL | Copia directa |
| `NroLote` | String(15) | `NroLote` | NVARCHAR(15) NULL | Trim de espacios |
| *Calculado*| Relación Espacial | `IdManzana` | INT NULL | Intersección centroide con Manzanas (`sp_ActualizarLoteCodigosFijos`) |
| *Geometry* | PolygonZ | `Geom` | `geometry` (SRID 4326) | Conversión a 2D OGC WKT |

## 3. Capa Códigos Fijos (`Exp_CodigoFijo_4326.shp` -> `dbo.CodigosFijos`)

| Campo DBF | Tipo Origen | Columna SQL | Tipo Destino | Regla de Transformación |
| :--- | :--- | :--- | :--- | :--- |
| `CodF_SQL` | Integer | `CodF_SQL` | INT NULL | Copia directa |
| `CodF_SIG` | String(25) | `CodF_SIG` | NVARCHAR(25) NULL | Trim de espacios |
| `CodFijo` | Integer | `CodFijo` | INT NULL | Clave de búsqueda |
| `Nombre` | String(120) | `Nombre` | NVARCHAR(120) NULL | Titular del servicio |
| *Default* | - | `Estado` | TINYINT NOT NULL | Valor inicial = 1 (Normal) |
| *Default* | - | `FechaCambioEstado` | DATETIME2 NOT NULL | `SYSDATETIME()` |
| `Longi` | Double | `Longitud` | FLOAT NULL | Coordenada X |
| `Latid` | Double | `Latitud` | FLOAT NULL | Coordenada Y |
| *Calculado*| Relación Espacial | `IdLote` | INT NULL | Intersección espacial con polígono de Lote |
| *Geometry* | PointZ | `Geom` | `geometry` (SRID 4326) | `geometry::Point(Longi, Latid, 4326)` |

## 4. Capa Vías (`Exp_MapaBase_VIAS_4326.shp` -> `dbo.Vias`)

| Campo DBF | Tipo Origen | Columna SQL | Tipo Destino | Regla de Transformación |
| :--- | :--- | :--- | :--- | :--- |
| `OBJECTID` | Integer | `OBJECTID` | INT NULL | Copia directa |
| `Nombre`/`name` | String(40) | `Nombre` | NVARCHAR(40) NULL | Coalescencia de campo nombre |
| `type`/`highway` | String(30) | `TipoVia` | NVARCHAR(30) NULL | Clasificación funcional |
| `OSMID`/`osm_id`| String(20) | `OSMID` | NVARCHAR(20) NULL | Identificador OSM |
| *Geometry* | PolyLine | `Geom` | `geometry` (SRID 4326) | Conversión a 2D OGC LineString/MultiLineString |
