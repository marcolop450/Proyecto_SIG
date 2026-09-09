## Why

Para cumplir con la primera fase del Proyecto Integrador ""VisorDatosSIG 2026"" (UAGRM - FICCT) con fecha límite del 22 de septiembre (Hitos H1 y H2), es indispensable formalizar la lectura de requerimientos, inspeccionar y validar los datos geográficos de entrada (archivos SHP en WGS84), definir la matriz de correspondencia hacia el diseño físico oficial de SQL Server 2022, consolidar los scripts reproducibles de base de datos e inicializar la arquitectura de la solución de software en capas.

## What Changes

- **Inspección y Diagnóstico de Geodatos**: Verificación técnica de los 4 shapefiles entregados (Exp_CodigoFijo_4326, Exp_MapaBase_LOTES_4326, Exp_MapaBase_MZA_4326, Exp_MapaBase_VIAS_4326), confirmando existencia de .shp, .shx, .dbf, .prj, conteo de registros, tipo de geometrías y sistema WGS 84 (SRID 4326).
- **Matriz de Mapeo SHP a SQL Server**: Mapeo explícito campo a campo entre cada DBF y las tablas del diseño físico (Manzanas, Lotes, CodigosFijos, Vias), estableciendo tipos de datos, transformaciones de coordenadas y claves foráneas.
- **Scripts de Persistencia Espacial SQL Server**: Consolidación y validación de los scripts de creación de base de datos (VisorDatosSIG), tablas espaciales con tipo geometry (SRID 4326), índices espaciales (GEOMETRY_GRID), funciones/triggers, tablas de seguridad (Usuarios, Roles, MenuOpciones) y procedimientos almacenados.
- **Estructura Base de la Solución**: Andamiaje de la solución de Visual Studio / .NET con la arquitectura limpia en capas (Domain, Application, Infrastructure, Migrador, Web/Api).
- **Estrategia Git y Tablero**: Integración con el repositorio oficial https://github.com/marcolop450/Proyecto_SIG.git, configuración de .gitignore y definición de hitos.

## Capabilities

### New Capabilities
- geodata-diagnostics: Inspección, diagnóstico de integridad y validación de geometrías y atributos de las 4 capas SHP (WGS84 / SRID 4326).
- spatial-database-schema: Esquema físico, restricciones, índices espaciales, seguridad y procedimientos almacenados en SQL Server 2022.
- solution-scaffolding: Estructura modular de la solución .NET en capas lista para compilar e implementar el migrador y el visor web.

### Modified Capabilities
<!-- Ninguna capacidad modificada por tratarse de la fase inicial -->

## Impact

- Repositorio y control de versiones: Establece la rama principal y configuración de exclusión.
- Base de datos: Base de datos VisorDatosSIG creada y lista para recibir la migración.
- Entorno de desarrollo: Requiere instalación local de SQL Server 2022 y .NET SDK para compilación y pruebas.
