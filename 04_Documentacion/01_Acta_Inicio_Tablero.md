# Acta de Inicio y Tablero de Trabajo

**Proyecto**: VisorDatosSIG 2026  
**Entidad**: Universidad Autónoma Gabriel René Moreno (UAGRM) — FICCT  
**Materia**: Sistemas de Información Geográfica  
**Docente**: Ing. Ubaldo Pérez Ferreira  
**Modalidad**: Individual (Marco Lopez)  
**Fecha de Inicio**: Septiembre 2026  

---

## 1. Objetivo General
Diseñar, implementar y validar un sistema de información geográfica integral (Geo Visor) para la gestión territorial y de servicios de la ciudad de San Ignacio de Velasco, soportado sobre Microsoft SQL Server 2022 y .NET 8.0 C#.

## 2. Alcance del Proyecto
1. Inspección y diagnóstico técnico de 4 capas cartográficas en formato ESRI Shapefile (WGS 84 / SRID 4326).
2. Diseño e implementación de la base de datos espacial `VisorDatosSIG` en SQL Server 2022 con restricciones, índices espaciales y procedimientos almacenados.
3. Desarrollo de una herramienta automatizada de migración e ingesta por lotes con soporte transaccional y bitácora de auditoría.
4. Construcción de una aplicación web responsiva en capas (.NET 8 C# MVC + API REST GeoJSON) con visor interactivo en Leaflet.js.
5. Módulo de autenticación segura (PBKDF2 HMAC-SHA256) y control de acceso basado en roles.

## 3. Tablero de Actividades y Hitos Oficiales

| Hito | Actividad | Plazo Oficial | Estado |
| :---: | :--- | :---: | :---: |
| **H1** | 1. Lectura de especificaciones y acta de inicio | Días 1–2 | **Completado** |
| **H1** | 2. Inspección de SHP, atributos y WGS 84 | Días 2–4 | **Completado** |
| **H1** | 3. Diseño físico y matriz de mapeo SHP–SQL | Días 3–6 | **Completado** |
| **H1** | 4. Arquitectura de solución y estrategia Git | Días 5–8 | **Completado** |
| **H2** | 5. Creación de BD, restricciones e índices espaciales | Días 8–13 | **Completado** |
| **H2** | 6. Estructura de solución y modelos de dominio | Días 9–14 | **Completado** |
| **H2** | 7. Migrador: Lectura, validación y transacciones | Días 12–17 | **Completado** |
| **H3** | 8. Migrador: Carga por lotes y bitácora | Días 16–21 | **Completado** |
| **H3** | 9. Web: Autenticación, roles y estructura responsiva | Días 18–24 | **Completado** |
| **H4** | 10. Servicios GeoJSON y consultas por extensión | Días 22–28 | **Completado** |
| **H4** | 11. Visor cartográfico, capas, leyenda y navegación | Días 24–30 | **Completado** |
| **H5** | 12. Módulo de corte/reconexión y visor final | Días 31–45 | *Fase Siguiente* |
