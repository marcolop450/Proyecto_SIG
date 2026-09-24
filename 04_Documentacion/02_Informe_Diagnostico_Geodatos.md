# Informe Técnico de Diagnóstico de Geodatos

**Proyecto**: VisorDatosSIG 2026  
**Fecha de Evaluación**: Septiembre 2026  
**Ubicación de Capas**: `DatosSIG_Reproj/`  

---

## 1. Verificación de Integridad de Componentes
Se verificó la existencia obligatoria de los cuatro archivos constitutivos por cada capa (.shp, .shx, .dbf, .prj):

| Capa | Archivos Presentes | SRS (.prj) | Tipo de Geometría | Registros |
| :--- | :---: | :---: | :---: | :---: |
| **Exp_MapaBase_MZA_4326** | .shp, .shx, .dbf, .prj | WGS 84 (GCS_WGS_1984) | MultiPolygon (Z) | **863** |
| **Exp_MapaBase_LOTES_4326** | .shp, .shx, .dbf, .prj | WGS 84 (GCS_WGS_1984) | MultiPolygon (Z) | **15,280** (*1 omitido por corrupción OGC) |
| **Exp_CodigoFijo_4326** | .shp, .shx, .dbf, .prj | WGS 84 (GCS_WGS_1984) | Point (Z) | **6,271** |
| **Exp_MapaBase_VIAS_4326** | .shp, .shx, .dbf, .prj | WGS 84 (GCS_WGS_1984) | PolyLine | **578** |

## 2. Bounding Box General (Extensión Espacial)
* **Longitud Mínima (Oeste)**: -61.007°
* **Longitud Máxima (Este)**: -60.919°
* **Latitud Mínima (Sur)**: -16.440°
* **Latitud Máxima (Norte)**: -16.321°
* **Municipio**: San Ignacio de Velasco, Santa Cruz, Bolivia.

## 3. Conclusiones del Diagnóstico
1. Todas las capas coinciden en el sistema de referencia de coordenadas WGS 84 (EPSG:4326), eliminando la necesidad de reproyección en tiempo de ejecución.
2. Las geometrías contienen coordenadas Z que deben ser reducidas a 2D OGC para su compatibilidad nativa con SQL Server `geometry::STGeomFromText`.
3. Se detectó 1 registro en Lotes con auto-intersección de anillos que fue capturado y documentado en la bitácora sin detener el proceso de migración masiva.
