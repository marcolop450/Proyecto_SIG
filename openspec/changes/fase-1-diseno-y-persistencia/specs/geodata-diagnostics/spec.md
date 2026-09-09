## Purpose

Permite inspeccionar, auditar y diagnosticar la integridad geométrica, atributos y referencia espacial WGS84 de las cuatro capas cartográficas entregadas (Manzanas, Lotes, Códigos Fijos y Vías) antes de su migración.

## ADDED Requirements

### Requirement: Validación de integridad de archivos Shapefile
El sistema SHALL verificar que cada conjunto geográfico cuente con sus componentes obligatorios (.shp, .shx, .dbf y .prj).

#### Scenario: Capa con componentes completos
- **WHEN** se inspecciona una capa que contiene sus cuatro archivos indispensables
- **THEN** el sistema reporta la capa como íntegra y apta para la lectura de registros

#### Scenario: Falta de componente obligatorio
- **WHEN** falta alguno de los archivos obligatorios (.shp, .shx, .dbf o .prj)
- **THEN** el sistema bloquea la carga, identifica el archivo faltante y genera una alerta explícita

### Requirement: Validación de referencia espacial WGS 84 (SRID 4326)
El sistema SHALL leer el archivo .prj de cada capa y verificar que el sistema de coordenadas corresponda estrictamente a WGS 84 (SRID 4326).

#### Scenario: Referencia espacial válida WGS 84
- **WHEN** el archivo .prj contiene la definición estándar GCS_WGS_1984
- **THEN** el sistema asigna el SRID 4326 y aprueba el paso a la fase de mapeo

#### Scenario: Referencia espacial no WGS 84 o archivo .prj ausente
- **WHEN** el archivo .prj está ausente o define un sistema diferente a WGS 84
- **THEN** el sistema bloquea la carga y nunca asigna un SRID de forma silenciosa

### Requirement: Diagnóstico de geometrías y conteos oficiales
El sistema SHALL validar la estructura geométrica de cada registro y constatar los conteos oficiales (863 Manzanas, 15,281 Lotes, 6,271 Códigos Fijos y 578 Vías).

#### Scenario: Verificación de geometrías válidas
- **WHEN** se analizan las geometrías de las cuatro capas
- **THEN** el sistema valida que las geometrías no sean nulas y correspondan a los tipos PolygonZ, PointZ o PolyLine según la capa
