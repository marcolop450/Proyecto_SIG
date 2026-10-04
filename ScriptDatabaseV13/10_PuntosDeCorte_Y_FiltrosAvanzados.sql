USE VisorDatosSIG;
GO

-- ==============================================================================
-- 1. ASIGNACIÓN DE PUNTOS DE CORTE REALISTAS PARA DEMOSTRACIÓN (RF-CON-10 / PLIEGO)
-- ==============================================================================
-- 1 = Normal (Verde)
-- 2 = Para Corte (Ámbar / Naranja)
-- 3 = Cortado (Rojo)
-- 4 = Baja (Gris)

-- Restablecer todos a Normal inicialmente con fecha reciente
UPDATE dbo.CodigosFijos
   SET Estado = 1,
       FechaCambioEstado = DATEADD(DAY, -(IdCodigo % 30), SYSDATETIME());

-- Asignar ~650 medidores como "Para Corte" (Estado = 2) de manera distribuida
UPDATE dbo.CodigosFijos
   SET Estado = 2,
       FechaCambioEstado = DATEADD(DAY, -(IdCodigo % 7), SYSDATETIME())
 WHERE (IdCodigo % 10 = 3) AND CodFijo <> 4936;

-- Asignar ~320 medidores como "Cortado" (Estado = 3)
UPDATE dbo.CodigosFijos
   SET Estado = 3,
       FechaCambioEstado = DATEADD(DAY, -(IdCodigo % 14), SYSDATETIME())
 WHERE (IdCodigo % 20 = 7) AND CodFijo <> 4936;

-- Asignar ~60 medidores como "Baja" (Estado = 4)
UPDATE dbo.CodigosFijos
   SET Estado = 4,
       FechaCambioEstado = DATEADD(DAY, -(IdCodigo % 60), SYSDATETIME())
 WHERE (IdCodigo % 100 = 13) AND CodFijo <> 4936;

-- Asegurar explícitamente que el Código Fijo 4936 (Ing. Pérez Ferreira Ubaldo) esté activo y normal
UPDATE dbo.CodigosFijos
   SET Estado = 1,
       FechaCambioEstado = SYSDATETIME()
 WHERE CodFijo = 4936;
GO

-- ==============================================================================
-- 2. PROCEDIMIENTO sp_CambiarEstadoSuministro (OPERADOR / ADMIN)
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_CambiarEstadoSuministro
    @CodFijo INT,
    @NuevoEstado TINYINT,
    @Usuario NVARCHAR(100) = N'admin',
    @Motivo NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @NuevoEstado NOT BETWEEN 1 AND 5
    BEGIN
        RAISERROR(N'Estado inválido. Debe estar entre 1 y 5.', 16, 1);
        RETURN;
    END;

    DECLARE @EstadoAnterior TINYINT;
    DECLARE @NombreTitular NVARCHAR(150);
    DECLARE @IdCodigo INT;

    SELECT @IdCodigo = IdCodigo,
           @EstadoAnterior = Estado,
           @NombreTitular = Nombre
      FROM dbo.CodigosFijos
     WHERE CodFijo = @CodFijo;

    IF @IdCodigo IS NULL
    BEGIN
        RAISERROR(N'Código Fijo no encontrado.', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;

    UPDATE dbo.CodigosFijos
       SET Estado = @NuevoEstado,
           FechaCambioEstado = SYSDATETIME()
     WHERE CodFijo = @CodFijo;

    DECLARE @DescAnt NVARCHAR(30) = CASE @EstadoAnterior 
        WHEN 1 THEN N'Normal' WHEN 2 THEN N'Para Corte' WHEN 3 THEN N'Cortado' WHEN 4 THEN N'Baja Parcial' ELSE N'Baja Total' END;
    DECLARE @DescNuevo NVARCHAR(30) = CASE @NuevoEstado 
        WHEN 1 THEN N'Normal' WHEN 2 THEN N'Para Corte' WHEN 3 THEN N'Cortado' WHEN 4 THEN N'Baja Parcial' ELSE N'Baja Total' END;

    -- Registrar en bitácora de auditoría
    IF OBJECT_ID(N'dbo.BitacoraAuditoria', N'U') IS NOT NULL
    BEGIN
        INSERT INTO dbo.BitacoraAuditoria (Fecha, Usuario, Accion, Modulo, Detalle, DireccionIP)
        VALUES (
            SYSDATETIME(),
            @Usuario,
            N'CAMBIO_ESTADO',
            N'Suministros',
            CONCAT(N'Cambio de estado para suministro ', @CodFijo, N' (', @NombreTitular, N') de ', @DescAnt, N' a ', @DescNuevo, ISNULL(N'. Motivo: ' + @Motivo, N'')),
            N'127.0.0.1'
        );
    END;

    COMMIT TRANSACTION;

    SELECT @CodFijo AS CodFijo, @NuevoEstado AS Estado, @DescNuevo AS EstadoDesc, SYSDATETIME() AS FechaCambioEstado;
END;
GO

-- ==============================================================================
-- 3. PROCEDIMIENTO sp_BuscarPrediosTotal MEJORADO (CON 4936 Y PUNTOS DE CORTE)
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuscarPrediosTotal
    @Texto NVARCHAR(100) = NULL,
    @UV NVARCHAR(15) = NULL,
    @Mza NVARCHAR(10) = NULL,
    @Lote NVARCHAR(15) = NULL,
    @TipoSuministro NVARCHAR(30) = NULL, -- 'TODOS', 'CON_SUMINISTRO', 'SIN_SUMINISTRO', 'PARA_CORTE', 'CORTADO', 'NORMAL'
    @Estado INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH UniversoPredios AS (
        -- A. Predios desde Lotes Catastrales (con medidor o terrenos baldíos)
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

        -- B. Códigos Fijos / Medidores sin lote poligonal vinculado (incluye al 4936 PEREZ FERREIRA UBALDO)
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
           OR CONVERT(NVARCHAR(30), IdCodigo) = @Texto
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
        CASE WHEN CodFijo = 4936 THEN -1 WHEN TieneSuministro = 1 THEN 0 ELSE 1 END,
        UV, MZA, NroLote;
END;
GO

-- ==============================================================================
-- 4. PROCEDIMIENTO sp_BuscarInmueble MEJORADO (CON RETORNO GARANTIZADO DE 4936)
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuscarInmueble
    @Texto NVARCHAR(100) = NULL,
    @UV NVARCHAR(15) = NULL,
    @Mza NVARCHAR(10) = NULL,
    @Lote NVARCHAR(15) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.sp_BuscarPrediosTotal 
        @Texto = @Texto, 
        @UV = @UV, 
        @Mza = @Mza, 
        @Lote = @Lote, 
        @TipoSuministro = N'TODOS';
END;
GO
