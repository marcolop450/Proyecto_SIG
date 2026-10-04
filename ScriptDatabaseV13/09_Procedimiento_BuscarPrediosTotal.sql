USE VisorDatosSIG;
GO

/* =========================================================================
   Procedimiento: dbo.sp_BuscarPrediosTotal
   Descripción: Consulta integral de los 15,280 predios/lotes catastrales,
                vinculando sus medidores (Códigos Fijos) si existen,
                o indicando si se trata de terrenos sin servicio / baldíos.
   ========================================================================= */

IF COL_LENGTH('dbo.Lotes', 'Latitud') IS NULL
BEGIN
    ALTER TABLE dbo.Lotes ADD Latitud FLOAT NULL, Longitud FLOAT NULL;
END
GO

UPDATE dbo.Lotes 
SET Latitud = Geom.STCentroid().STY, Longitud = Geom.STCentroid().STX 
WHERE Geom IS NOT NULL AND Latitud IS NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_BuscarPrediosTotal
    @Texto NVARCHAR(100) = NULL,
    @UV NVARCHAR(15) = NULL,
    @Mza NVARCHAR(10) = NULL,
    @Lote NVARCHAR(15) = NULL,
    @TipoSuministro NVARCHAR(30) = NULL -- 'TODOS', 'CON_SUMINISTRO', 'SIN_SUMINISTRO'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.IdLote,
        l.NroLote,
        COALESCE(m.UV, 'S/UV') AS UV,
        COALESCE(m.MZA, 'S/MZA') AS MZA,
        c.IdCodigo,
        c.CodFijo,
        COALESCE(c.Nombre, N'(Sin Suministro / Terreno Baldío)') AS Nombre,
        COALESCE(c.Estado, 0) AS Estado, -- 0 = Sin Suministro, 1 = Normal, 2 = Para Corte, 3 = Cortado, 4 = Baja
        c.FechaCambioEstado,
        COALESCE(c.Latitud, l.Latitud) AS Latitud,
        COALESCE(c.Longitud, l.Longitud) AS Longitud,
        CASE WHEN c.IdCodigo IS NOT NULL THEN 1 ELSE 0 END AS TieneSuministro
    FROM dbo.Lotes l
    LEFT JOIN dbo.CodigosFijos c ON c.IdLote = l.IdLote
    LEFT JOIN dbo.Manzanas m ON m.IdManzana = l.IdManzana
    WHERE (@Texto IS NULL 
           OR CONVERT(NVARCHAR(30), c.CodFijo) = @Texto 
           OR l.NroLote LIKE N'%' + @Texto + N'%'
           OR c.Nombre LIKE N'%' + @Texto + N'%')
      AND (@UV IS NULL OR m.UV = @UV)
      AND (@Mza IS NULL OR m.MZA = @Mza)
      AND (@Lote IS NULL OR l.NroLote = @Lote)
      AND (
          @TipoSuministro IS NULL 
          OR @TipoSuministro = 'TODOS'
          OR (@TipoSuministro = 'CON_SUMINISTRO' AND c.IdCodigo IS NOT NULL)
          OR (@TipoSuministro = 'SIN_SUMINISTRO' AND c.IdCodigo IS NULL)
      )
    ORDER BY 
        CASE WHEN c.IdCodigo IS NOT NULL THEN 0 ELSE 1 END,
        m.UV, m.MZA, l.NroLote;
END;
GO

/* Actualizar sp_BuscarInmueble para redirigir a sp_BuscarPrediosTotal */
CREATE OR ALTER PROCEDURE dbo.sp_BuscarInmueble
    @Texto NVARCHAR(100) = NULL,
    @UV NVARCHAR(15) = NULL,
    @Mza NVARCHAR(10) = NULL,
    @Lote NVARCHAR(15) = NULL,
    @TipoSuministro NVARCHAR(30) = 'TODOS'
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_BuscarPrediosTotal @Texto, @UV, @Mza, @Lote, @TipoSuministro;
END;
GO
