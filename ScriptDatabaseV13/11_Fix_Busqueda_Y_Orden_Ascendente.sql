USE VisorDatosSIG;
GO

-- ==============================================================================
-- PROCEDIMIENTO sp_BuscarPrediosTotal CORREGIDO:
-- 1. No busca por IdCodigo (evita que CodFijo 1051 aparezca al buscar 4936).
-- 2. Ordenamiento ASCENDENTE estricto por Código Fijo e IdLote (1, 2, 3...).
-- 3. Cero favoritismos o anclajes artificiales.
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuscarPrediosTotal
    @Texto NVARCHAR(100) = NULL,
    @UV NVARCHAR(15) = NULL,
    @Mza NVARCHAR(10) = NULL,
    @Lote NVARCHAR(15) = NULL,
    @TipoSuministro NVARCHAR(30) = NULL,
    @Estado INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH UniversoPredios AS (
        -- A. Predios desde Lotes Catastrales
        SELECT
            l.IdLote,
            l.NroLote,
            COALESCE(m.UV, N'S/UV') AS UV,
            COALESCE(m.MZA, N'S/MZA') AS MZA,
            c.IdCodigo,
            c.CodFijo,
            COALESCE(c.Nombre, N'(Sin Suministro / Terreno Baldío)') AS Nombre,
            COALESCE(c.Estado, 0) AS Estado,
            c.FechaCambioEstado,
            COALESCE(c.Latitud, l.Latitud) AS Latitud,
            COALESCE(c.Longitud, l.Longitud) AS Longitud,
            CASE WHEN c.IdCodigo IS NOT NULL THEN 1 ELSE 0 END AS TieneSuministro
        FROM dbo.Lotes l
        LEFT JOIN dbo.CodigosFijos c ON c.IdLote = l.IdLote
        LEFT JOIN dbo.Manzanas m ON m.IdManzana = l.IdManzana

        UNION ALL

        -- B. Códigos Fijos / Medidores sin lote poligonal vinculado
        SELECT
            NULL AS IdLote,
            COALESCE(N'L' + PARSENAME(c.CodF_SIG, 1), N'S/L') AS NroLote,
            COALESCE(N'UV ' + PARSENAME(c.CodF_SIG, 3), N'S/UV') AS UV,
            COALESCE(N'Mz. ' + PARSENAME(c.CodF_SIG, 2), N'S/MZA') AS MZA,
            c.IdCodigo,
            c.CodFijo,
            c.Nombre,
            c.Estado,
            c.FechaCambioEstado,
            c.Latitud,
            c.Longitud,
            1 AS TieneSuministro
        FROM dbo.CodigosFijos c
        WHERE c.IdLote IS NULL
    )
    SELECT
        IdLote,
        NroLote,
        UV,
        MZA,
        IdCodigo,
        CodFijo,
        Nombre,
        Estado,
        FechaCambioEstado,
        Latitud,
        Longitud,
        TieneSuministro,
        CASE Estado
            WHEN 0 THEN N'Sin Suministro'
            WHEN 1 THEN N'Normal'
            WHEN 2 THEN N'Para Corte'
            WHEN 3 THEN N'Cortado'
            WHEN 4 THEN N'Baja Parcial'
            WHEN 5 THEN N'Baja Total'
            ELSE N'Otro'
        END AS EstadoDesc
    FROM UniversoPredios
    WHERE (@Texto IS NULL 
           OR CONVERT(NVARCHAR(30), CodFijo) = @Texto 
           OR NroLote LIKE N'%' + @Texto + N'%'
           OR Nombre LIKE N'%' + @Texto + N'%')
      AND (@UV IS NULL OR UV = @UV OR UV LIKE N'%' + @UV + N'%')
      AND (@Mza IS NULL OR MZA = @Mza OR MZA LIKE N'%' + @Mza + N'%')
      AND (@Lote IS NULL OR NroLote = @Lote OR NroLote LIKE N'%' + @Lote + N'%')
      AND (
          @TipoSuministro IS NULL 
          OR @TipoSuministro = N'TODOS'
          OR (@TipoSuministro = N'CON_SUMINISTRO' AND TieneSuministro = 1)
          OR (@TipoSuministro = N'SIN_SUMINISTRO' AND TieneSuministro = 0)
          OR (@TipoSuministro = N'PARA_CORTE' AND Estado = 2)
          OR (@TipoSuministro = N'CORTADO' AND Estado = 3)
          OR (@TipoSuministro = N'NORMAL' AND Estado = 1)
      )
      AND (@Estado IS NULL OR Estado = @Estado)
    ORDER BY 
        CASE WHEN CodFijo IS NOT NULL THEN 0 ELSE 1 END,
        CodFijo ASC,
        IdLote ASC;
END;
GO
