---
target: "d:\\Proyecto SIG\\src\\VisorDatosSIG.Web"
total_score: 21
max_score: 40
na_heuristics: 
p0_count: 4
p1_count: 5
target_identity: "file:d:\\Proyecto SIG\\src\\VisorDatosSIG.Web"
timestamp: 2026-10-05T15-09-18Z
slug: src-visordatossig-web
---
# Combined Critique & Technical Audit Snapshot
Target: d:\Proyecto SIG\src\VisorDatosSIG.Web
Date: 2026-10-05

## Method
Method: dual-agent (A: 4d654a6e-6fce-40d2-8cca-c1cf63a79d89 · B: 809674a2-7713-462c-b465-2a3ccb4f9e8a)

## Design Health Score (Nielsen 10 Heuristics)
Total: 21/40 (Aceptable - Requiere mejoras sustanciales)

| # | Heuristica | Puntaje | Problema Clave |
|---|---|---|---|
| 1 | Visibilidad del Estado del Sistema | 3/4 | Buena respuesta en coordenadas y spinners; guardado y copiado recurren a alert() bloqueantes. |
| 2 | Coincidencia con el Mundo Real | 3/4 | Jerarquia territorial natural (UV/Mz/Lote); persisten fugas tecnicas (SRID 4326, ID Sistema). |
| 3 | Control y Libertad del Usuario | 2/4 | Filtros reseteables, pero sin opcion de deshacer (undo) tras cambio de corte y panel que tapa controles. |
| 4 | Consistencia y Estandares | 1/4 | Colores de estados dispares entre mapa y tabla; paleta fragmentada (azul vs oliva) y parametro URL (lng vs lon). |
| 5 | Prevencion de Errores | 2/4 | Listas desplegables operativas; falta confirmacion previa para ordenar corte de agua potable. |
| 6 | Reconocimiento Antes que Recuerdo | 3/4 | Tarjetas laterales reducen carga mental; falta hover preview en mapa y falta filtro de UV en mapa. |
| 7 | Flexibilidad y Eficiencia de Uso | 2/4 | Busqueda reactiva con debounce de 250ms; sin atajos de teclado ni operaciones masivas (batch). |
| 8 | Diseno Estetico y Minimalista | 2/4 | Ficha y tabla estructuradas; barra lateral del mapa saturada con 4 bloques apilados sin pestanas. |
| 9 | Ayuda ante Errores | 2/4 | Alertas rojas en modales capturan fallos; mensajes en tablas son genericos y alert() no es copiable. |
| 10 | Ayuda y Documentacion | 1/4 | Enlaces globales a Manual en navbar; falta microcopia contextual in situ en campos y codigos. |

## Audit Health Score (Technical Dimensions)
Total: 7.0/20 (Deficiente - Requiere remediacion estructural)

| # | Dimension | Puntaje | Diagnostico Clave |
|---|---|---|---|
| 1 | Accesibilidad (A11y) | 1.5/4 | Incumplimiento de contraste WCAG AA, bloqueo de zoom viewport y navegacion por teclado ausente en tarjetas. |
| 2 | Rendimiento (Performance) | 1.5/4 | 15,300 poligonos y 6,500 puntos renderizados en SVG directo sin preferCanvas ni recorte por BBOX. |
| 3 | Diseno Responsivo | 1.0/4 | Sin navegacion movil (_Layout d-none d-md-flex sin hamburguesa), sidebar fija de 340px en 360px. |
| 4 | Coherencia Visual / Tokens | 1.5/4 | Fragmentacion entre 3 paletas (Azul Marino #17243B, Verde Oliva #5E7A4A, Azul Bootstrap). |
| 5 | Integridad de Implementacion | 1.5/4 | CLI scan limpio, pero 4 fallos bloqueantes P0 detectados en codigo e integracion entre vistas. |

## Priority Issues
- [P0] Desconexion critica entre Consultas y Mapa: enlace genera 'lng' y mapa busca 'lon'.
- [P0] Quiebre de navegacion global en moviles (< 768px) por d-none d-md-flex sin menu hamburguesa.
- [P0] Colapso de layout en visor cartografico en viewport movil (360px) por sidebar fija de 340px.
- [P0] Infraccion WCAG SC 1.4.4 / 1.4.10 por bloqueo de zoom (maximum-scale=1.0, user-scalable=no).
- [P1] Sobrecarga del DOM por 21,000 elementos vectoriales SVG sin canvas en Leaflet.
- [P1] Incumplimiento de ratios de contraste 4.5:1 en badges de estado y textos secundarios.
- [P1] Inaccesibilidad por teclado en lista de resultados de predios del mapa.
- [P1] Fragmentacion de colores de estado (verde/ambar/rojo dispar entre mapa y tabla).
- [P1] Falta de confirmacion en dos pasos e historial en suspension de suministro.
- [P2] Barra lateral saturada en 340px y colision con Ficha Catastral flotante.
- [P2] Touch targets por debajo de 44px en acciones de tabla y controles.
- [P2] Dialogos alert() y prompt() bloqueantes del hilo de interfaz.

## Persona Red Flags
- Alex (Power User): Sin atajos de teclado, sin operaciones por lotes para cortes multiples, sin filtros guardables.
- Jordan (First-Timer): Sin filtro de UV en mapa, datos con guiones vacios sin explicar, alerts bloqueantes en tablet.
