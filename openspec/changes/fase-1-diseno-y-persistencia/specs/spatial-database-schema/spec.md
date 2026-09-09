## Purpose

Establece el diseño físico, persistencia espacial mediante SQL Server 2022 con tipos geometry (SRID 4326), restricciones de integridad, índices espaciales y seguridad por roles requeridos para el proyecto.

## ADDED Requirements

### Requirement: Creación de base de datos y tablas espaciales
El sistema de persistencia SHALL crear la base de datos VisorDatosSIG y las tablas espaciales Manzanas, Lotes, CodigosFijos y Vias con columnas de tipo geometry en SRID 4326.

#### Scenario: Ejecución de scripts DDL iniciales
- **WHEN** se ejecuta el script 01_CrearBD.sql en SQL Server 2022
- **THEN** la base de datos se genera con sus llaves primarias, llaves foráneas y tipos geometry asignados

### Requirement: Índices espaciales y convencionales
El sistema SHALL contar con índices espaciales de tipo GEOMETRY_GRID sobre cada columna Geom y con índices convencionales sobre campos de búsqueda frecuente (CodFijo, UV, MZA, NroLote, Nombre).

#### Scenario: Consulta espacial o alfanumérica
- **WHEN** se realiza una búsqueda de lotes, códigos o una intersección espacial
- **THEN** SQL Server utiliza los índices optimizados para responder en tiempos menores a 2 segundos

### Requirement: Seguridad y control de acceso por roles
El sistema SHALL soportar usuarios con contraseñas seguras (hash con sal) y asignación de al menos los roles Administrador y Consultor.

#### Scenario: Acceso de usuario Consultor
- **WHEN** un usuario con rol Consultor inicia sesión
- **THEN** solo tiene acceso a consultas y visor cartográfico, quedando bloqueados los módulos de administración y auditoría
