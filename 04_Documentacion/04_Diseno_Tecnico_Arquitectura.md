# Diseño Técnico de Arquitectura

## 1. Patrón Arquitectónico en Capas Limpias

La solución `VisorDatosSIG.sln` sigue el principio de separación de responsabilidades:

```
[ Capa de Presentación: VisorDatosSIG.Web ]
                 │
                 ▼
[ Capa de Aplicación: VisorDatosSIG.Application ]
                 │
                 ▼
[ Capa de Dominio: VisorDatosSIG.Domain ]
                 ▲
                 │
[ Capa de Infraestructura: VisorDatosSIG.Infrastructure ]
                 │
                 ▼
[ Motor de Persistencia: Microsoft SQL Server 2022 ]
```

## 2. Seguridad y Criptografía
* **Esquema de Hash**: Algoritmo PBKDF2 (HMAC-SHA256).
* **Parámetros de Seguridad**:
  * 100,000 iteraciones.
  * Sal criptográfica aleatoria de 32 bytes (256 bits).
  * Salida de hash de 32 bytes (256 bits).
* **Protección contra ataques**:
  * Verificación en tiempo constante (`CryptographicOperations.FixedTimeEquals`) para prevenir vulnerabilidades de timing attack.
  * Cookies de sesión con marcas `HttpOnly`, `Secure` y `SameSite=Lax`.

## 3. Servicios GeoJSON
Los controladores exponen endpoints optimizados con serialización `application/geo+json`:
* `GET /api/capas/manzanas`
* `GET /api/capas/lotes`
* `GET /api/capas/vias`
* `GET /api/capas/codigosfijos`
* `GET /api/busqueda?texto=...`
