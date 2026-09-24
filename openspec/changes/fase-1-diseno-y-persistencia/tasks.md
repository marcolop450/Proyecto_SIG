## 1. Diagnóstico de Datos Cartográficos (Actividad 2 - Hito H1)

- [x] 1.1 Inspeccionar archivos .shp, .shx, .dbf y .prj de las cuatro capas y generar informe técnico de diagnóstico con conteos y bounding boxes
- [x] 1.2 Validar proyección WGS 84 (EPSG:4326) y tipos de geometrías (PointZ, PolygonZ, PolyLine)

## 2. Matriz de Mapeo SHP a SQL Server (Actividad 3 - Hito H1)

- [x] 2.1 Elaborar la matriz formal de mapeo de campos DBF a columnas SQL Server para Manzanas, Lotes, Códigos Fijos y Vías
- [x] 2.2 Definir reglas de transformación de geometrías, cálculo de centroides y relaciones espaciales (Lote - Manzana, Código Fijo - Lote)

## 3. Persistencia Espacial en SQL Server 2022 (Actividad 5 - Hito H2)

- [x] 3.1 Consolidar y validar el script maestro ordenado de base de datos (01_CrearBD.sql) incluyendo tipos geometry, claves y restricciones
- [x] 3.2 Verificar creación de índices espaciales (GEOMETRY_GRID) e índices convencionales para búsquedas
- [x] 3.3 Configurar tablas de usuarios, roles (Administrador, Consultor) y opciones de menú según el Anexo E del pliego

## 4. Estructura de Solución y Control de Versiones (Actividades 4 y 6 - Hito H1 y H2)

- [x] 4.1 Configurar el repositorio Git con .gitignore, vincular a https://github.com/marcolop450/Proyecto_SIG.git y preparar commit inicial
- [x] 4.2 Documentar los requisitos previos del entorno (.NET SDK y SQL Server 2022) y la guía de acciones manuales
