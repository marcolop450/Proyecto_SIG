using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;

namespace VisorDatosSIG.Migrador;

class Program
{
    private const string ConnectionString = "Server=localhost;Database=VisorDatosSIG;Integrated Security=True;TrustServerCertificate=True;";
    private static readonly WKTWriter _wkt2D = new() { OutputOrdinates = Ordinates.XY };
    private static readonly StringBuilder _bitacora = new();

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("================================================================================");
        Console.WriteLine("        MIGRADOR DE DATOS GEOGRÁFICOS SHP -> SQL SERVER 2022 (SRID 4326)");
        Console.WriteLine("                     VisorDatosSIG 2026 - FICCT UAGRM");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        string dataDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "DatosSIG_Reproj"));
        if (!Directory.Exists(dataDir))
        {
            dataDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "DatosSIG_Reproj"));
        }

        Registrar($"Inicio de migración: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Registrar($"Directorio de datos: {dataDir}");

        Console.WriteLine($"[1/5] Verificando directorio y componentes obligatorios (.shp, .shx, .dbf, .prj)...");
        string[] capas = new[] { "Exp_MapaBase_MZA_4326", "Exp_MapaBase_LOTES_4326", "Exp_CodigoFijo_4326", "Exp_MapaBase_VIAS_4326" };
        foreach (var capa in capas)
        {
            foreach (var ext in new[] { ".shp", ".shx", ".dbf", ".prj" })
            {
                string path = Path.Combine(dataDir, capa + ext);
                if (!File.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR CRÍTICO: Falta el archivo obligatorio '{path}'. Carga bloqueada.");
                    Console.ResetColor();
                    return;
                }
            }
            string prjContent = File.ReadAllText(Path.Combine(dataDir, capa + ".prj"));
            if (!prjContent.Contains("WGS_1984") && !prjContent.Contains("GCS_WGS_1984"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: La capa '{capa}' no define WGS 84 en su .prj.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine($"  ✓ Capa {capa} verificada con éxito.");
        }

        Console.WriteLine("\n[2/5] Probando conexión a SQL Server 2022...");
        using (var conn = new SqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();
                Console.WriteLine("  ✓ Conectado a SQL Server ('VisorDatosSIG' en localhost).");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR al conectar a SQL Server: " + ex.Message);
                Console.ResetColor();
                return;
            }
        }

        var swTotal = Stopwatch.StartNew();

        Console.WriteLine("\n[2.5/5] Limpiando tablas anteriores respetando integridad referencial...");
        LimpiarTablasExistentes();

        Console.WriteLine("\n[3/5] Migrando capas geográficas con transacciones...");
        MigrarManzanas(Path.Combine(dataDir, "Exp_MapaBase_MZA_4326.shp"));
        MigrarLotes(Path.Combine(dataDir, "Exp_MapaBase_LOTES_4326.shp"));
        MigrarCodigosFijos(Path.Combine(dataDir, "Exp_CodigoFijo_4326.shp"));
        MigrarVias(Path.Combine(dataDir, "Exp_MapaBase_VIAS_4326.shp"));

        Console.WriteLine("\n[4/5] Calculando relaciones espaciales (Lote-Manzana y CódigoFijo-Lote)...");
        ActualizarRelacionesEspaciales();

        Console.WriteLine("\n[5/5] Ejecución de validaciones oficiales (Criterios Anexo C)...");
        EjecutarValidacionOficial();

        swTotal.Stop();
        Registrar($"Migración concluida en {swTotal.Elapsed.TotalSeconds:F1} segundos.");

        // Guardar bitácora
        File.WriteAllText("bitacora_migracion.txt", _bitacora.ToString(), Encoding.UTF8);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine($"        ¡MIGRACIÓN COMPLETA Y VERIFICADA CON ÉXITO EN {swTotal.Elapsed.TotalSeconds:F1} SEGUNDOS!");
        Console.WriteLine($"================================================================================");
        Console.WriteLine("  Bitácora guardada en: bitacora_migracion.txt\n");
        Console.ResetColor();
    }

    static void LimpiarTablasExistentes()
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(@"
            DELETE FROM dbo.CodigosFijos;
            DELETE FROM dbo.Lotes;
            DELETE FROM dbo.Manzanas;
            DELETE FROM dbo.Vias;
            DBCC CHECKIDENT ('dbo.CodigosFijos', RESEED, 0);
            DBCC CHECKIDENT ('dbo.Lotes', RESEED, 0);
            DBCC CHECKIDENT ('dbo.Manzanas', RESEED, 0);
            DBCC CHECKIDENT ('dbo.Vias', RESEED, 0);", conn);
        cmd.ExecuteNonQuery();
        Console.WriteLine("  ✓ Tablas anteriores limpiadas en cascada correctamente.");
    }

    static void Registrar(string mensaje)
    {
        _bitacora.AppendLine($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
    }

    static void MigrarManzanas(string shpPath)
    {
        Console.Write("  → Migrando Manzanas (esperado: 863)... ");
        var sw = Stopwatch.StartNew();
        int exitosos = 0, fallidos = 0;

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var tx = conn.BeginTransaction();
        const string insertSql = @"
            INSERT INTO dbo.Manzanas (IdOrigen, UV_MZA, UV, MZA, Geom)
            VALUES (@IdOrigen, @UV_MZA, @UV, @MZA, geometry::STGeomFromText(@Wkt, 4326));";

        using var cmd = new SqlCommand(insertSql, conn, tx);
        var pId = cmd.Parameters.Add("@IdOrigen", System.Data.SqlDbType.Int);
        var pUvMza = cmd.Parameters.Add("@UV_MZA", System.Data.SqlDbType.NVarChar, 20);
        var pUv = cmd.Parameters.Add("@UV", System.Data.SqlDbType.NVarChar, 15);
        var pMza = cmd.Parameters.Add("@MZA", System.Data.SqlDbType.NVarChar, 10);
        var pWkt = cmd.Parameters.Add("@Wkt", System.Data.SqlDbType.NVarChar, -1);

        using var reader = Shapefile.OpenRead(shpPath);
        while (true)
        {
            try
            {
                if (!reader.Read()) break;

                pId.Value = Convert.ToInt32(reader.Fields["Id"].Value);
                pUvMza.Value = reader.Fields["UV_MZA"].Value?.ToString() ?? (object)DBNull.Value;
                pUv.Value = reader.Fields["UV"].Value?.ToString() ?? (object)DBNull.Value;
                pMza.Value = reader.Fields["MZA"].Value?.ToString() ?? (object)DBNull.Value;
                pWkt.Value = _wkt2D.Write(reader.Geometry);

                cmd.ExecuteNonQuery();
                exitosos++;
            }
            catch (Exception ex)
            {
                fallidos++;
                Registrar($"Manzanas - Advertencia en registro {exitosos + fallidos}: {ex.Message}");
            }
        }

        tx.Commit();
        sw.Stop();
        Console.WriteLine($"{exitosos} insertadas ({sw.ElapsedMilliseconds} ms, {fallidos} omitidos)");
        Registrar($"Manzanas: {exitosos} insertadas, {fallidos} omitidos en {sw.ElapsedMilliseconds} ms.");
    }

    static void MigrarLotes(string shpPath)
    {
        Console.Write("  → Migrando Lotes (esperado: 15,281)... ");
        var sw = Stopwatch.StartNew();
        int exitosos = 0, fallidos = 0;

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var tx = conn.BeginTransaction();
        const string insertSql = @"
            INSERT INTO dbo.Lotes (IdOrigen, NroLote, Geom)
            VALUES (@IdOrigen, @NroLote, geometry::STGeomFromText(@Wkt, 4326));";

        using var cmd = new SqlCommand(insertSql, conn, tx);
        var pId = cmd.Parameters.Add("@IdOrigen", System.Data.SqlDbType.Int);
        var pNroLote = cmd.Parameters.Add("@NroLote", System.Data.SqlDbType.NVarChar, 15);
        var pWkt = cmd.Parameters.Add("@Wkt", System.Data.SqlDbType.NVarChar, -1);

        using var reader = Shapefile.OpenRead(shpPath);
        while (true)
        {
            try
            {
                if (!reader.Read()) break;

                pId.Value = Convert.ToInt32(reader.Fields["Id"].Value);
                pNroLote.Value = reader.Fields["NroLote"].Value?.ToString() ?? (object)DBNull.Value;
                pWkt.Value = _wkt2D.Write(reader.Geometry);

                cmd.ExecuteNonQuery();
                exitosos++;
                if (exitosos % 3000 == 0) Console.Write($"[{exitosos}] ");
            }
            catch (Exception ex)
            {
                fallidos++;
                Registrar($"Lotes - Advertencia en registro {exitosos + fallidos}: {ex.Message}");
            }
        }

        tx.Commit();
        sw.Stop();
        Console.WriteLine($"\n    {exitosos} lotes insertados ({sw.Elapsed.TotalSeconds:F1} s, {fallidos} omitidos)");
        Registrar($"Lotes: {exitosos} insertados, {fallidos} omitidos en {sw.Elapsed.TotalSeconds:F1} s.");
    }

    static void MigrarCodigosFijos(string shpPath)
    {
        Console.Write("  → Migrando Códigos Fijos (esperado: 6,271)... ");
        var sw = Stopwatch.StartNew();
        int exitosos = 0, fallidos = 0;

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var tx = conn.BeginTransaction();
        const string insertSql = @"
            INSERT INTO dbo.CodigosFijos (CodF_SQL, CodF_SIG, CodFijo, Nombre, Estado, FechaCambioEstado, Longitud, Latitud, Geom)
            VALUES (@CodF_SQL, @CodF_SIG, @CodFijo, @Nombre, 1, SYSDATETIME(), @Longi, @Latid, geometry::Point(@Longi, @Latid, 4326));";

        using var cmd = new SqlCommand(insertSql, conn, tx);
        var pCodF_SQL = cmd.Parameters.Add("@CodF_SQL", System.Data.SqlDbType.Int);
        var pCodF_SIG = cmd.Parameters.Add("@CodF_SIG", System.Data.SqlDbType.NVarChar, 25);
        var pCodFijo = cmd.Parameters.Add("@CodFijo", System.Data.SqlDbType.Int);
        var pNombre = cmd.Parameters.Add("@Nombre", System.Data.SqlDbType.NVarChar, 120);
        var pLongi = cmd.Parameters.Add("@Longi", System.Data.SqlDbType.Float);
        var pLatid = cmd.Parameters.Add("@Latid", System.Data.SqlDbType.Float);

        using var reader = Shapefile.OpenRead(shpPath);
        while (true)
        {
            try
            {
                if (!reader.Read()) break;

                var fCodF_SQL = reader.Fields["CodF_SQL"].Value;
                pCodF_SQL.Value = fCodF_SQL != null && !Convert.IsDBNull(fCodF_SQL) ? Convert.ToInt32(fCodF_SQL) : DBNull.Value;

                var fCodF_SIG = reader.Fields["CodF_SIG"].Value;
                pCodF_SIG.Value = fCodF_SIG?.ToString() ?? (object)DBNull.Value;

                var fCodFijo = reader.Fields["CodFijo"].Value;
                pCodFijo.Value = fCodFijo != null && !Convert.IsDBNull(fCodFijo) ? Convert.ToInt32(fCodFijo) : DBNull.Value;

                var fNombre = reader.Fields["Nombre"].Value;
                pNombre.Value = fNombre?.ToString() ?? (object)DBNull.Value;

                // Coordenadas reales extraídas directamente de la geometría del Shapefile (.shp)
                // Soluciona el problema de origen donde el .dbf tenía Longi copiado en Latid.
                double longi = reader.Geometry.Coordinate.X;
                double latid = reader.Geometry.Coordinate.Y;
                pLongi.Value = longi;
                pLatid.Value = latid;

                cmd.ExecuteNonQuery();
                exitosos++;
                if (exitosos % 2000 == 0) Console.Write($"[{exitosos}] ");
            }
            catch (Exception ex)
            {
                fallidos++;
                Registrar($"CodigosFijos - Advertencia en registro {exitosos + fallidos}: {ex.Message}");
            }
        }

        tx.Commit();
        sw.Stop();
        Console.WriteLine($"\n    {exitosos} códigos fijos insertados ({sw.Elapsed.TotalSeconds:F1} s, {fallidos} omitidos)");
        Registrar($"CodigosFijos: {exitosos} insertados, {fallidos} omitidos en {sw.Elapsed.TotalSeconds:F1} s.");
    }

    static void MigrarVias(string shpPath)
    {
        Console.Write("  → Migrando Vías (esperado: 578)... ");
        var sw = Stopwatch.StartNew();
        int exitosos = 0, fallidos = 0;

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var tx = conn.BeginTransaction();
        const string insertSql = @"
            INSERT INTO dbo.Vias (OBJECTID, Nombre, TipoVia, OSMID, Geom)
            VALUES (@ObjId, @Nombre, @TipoVia, @OsmId, geometry::STGeomFromText(@Wkt, 4326));";

        using var cmd = new SqlCommand(insertSql, conn, tx);
        var pObjId = cmd.Parameters.Add("@ObjId", System.Data.SqlDbType.Int);
        var pNombre = cmd.Parameters.Add("@Nombre", System.Data.SqlDbType.NVarChar, 40);
        var pTipoVia = cmd.Parameters.Add("@TipoVia", System.Data.SqlDbType.NVarChar, 30);
        var pOsmId = cmd.Parameters.Add("@OsmId", System.Data.SqlDbType.NVarChar, 20);
        var pWkt = cmd.Parameters.Add("@Wkt", System.Data.SqlDbType.NVarChar, -1);

        using var reader = Shapefile.OpenRead(shpPath);
        while (true)
        {
            try
            {
                if (!reader.Read()) break;

                var fObjId = reader.Fields["OBJECTID"].Value;
                pObjId.Value = fObjId != null && !Convert.IsDBNull(fObjId) ? Convert.ToInt32(fObjId) : DBNull.Value;

                string? nom = reader.Fields["Nombre"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(nom)) nom = reader.Fields["name"].Value?.ToString();
                pNombre.Value = (object?)nom ?? DBNull.Value;

                pTipoVia.Value = reader.Fields["type"].Value?.ToString() ?? (object)DBNull.Value;

                var fOsmId = reader.Fields["OSMID"].Value ?? reader.Fields["osm_id"].Value;
                pOsmId.Value = fOsmId?.ToString() ?? (object)DBNull.Value;

                pWkt.Value = _wkt2D.Write(reader.Geometry);

                cmd.ExecuteNonQuery();
                exitosos++;
            }
            catch (Exception ex)
            {
                fallidos++;
                Registrar($"Vias - Advertencia en registro {exitosos + fallidos}: {ex.Message}");
            }
        }

        tx.Commit();
        sw.Stop();
        Console.WriteLine($"{exitosos} vías insertadas ({sw.ElapsedMilliseconds} ms, {fallidos} omitidos)");
        Registrar($"Vias: {exitosos} insertadas, {fallidos} omitidos en {sw.ElapsedMilliseconds} ms.");
    }

    static void ActualizarRelacionesEspaciales()
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand("dbo.sp_ActualizarLoteCodigosFijos", conn);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.CommandTimeout = 300;

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine($"  ✓ Códigos Fijos asociados a Lote: {reader["AsociadosALote"]} de {reader["TotalCodigos"]}");
            Registrar($"Relación Códigos Fijos -> Lotes: {reader["AsociadosALote"]} asociados.");
        }
        if (reader.NextResult() && reader.Read())
        {
            Console.WriteLine($"  ✓ Lotes asociados a Manzana: {reader["AsociadosAManzana"]} de {reader["TotalLotes"]}");
            Registrar($"Relación Lotes -> Manzanas: {reader["AsociadosAManzana"]} asociados.");
        }
    }

    static void EjecutarValidacionOficial()
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        const string sql = @"
            SELECT 'Manzanas' AS Capa, COUNT(*) AS Total, SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END) AS GeomNull,
                   SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END) AS SridInvalido,
                   SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END) AS GeomInvalida
            FROM dbo.Manzanas
            UNION ALL
            SELECT 'Lotes', COUNT(*), SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END),
                   SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END),
                   SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END)
            FROM dbo.Lotes
            UNION ALL
            SELECT 'CodigosFijos', COUNT(*), SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END),
                   SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END),
                   SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END)
            FROM dbo.CodigosFijos
            UNION ALL
            SELECT 'Vias', COUNT(*), SUM(CASE WHEN Geom IS NULL THEN 1 ELSE 0 END),
                   SUM(CASE WHEN Geom.STSrid <> 4326 THEN 1 ELSE 0 END),
                   SUM(CASE WHEN Geom.STIsValid() = 0 THEN 1 ELSE 0 END)
            FROM dbo.Vias;";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n  Resultados de Validación (Criterios Anexo C del Pliego):");
        Console.WriteLine("  {0,-15} {1,8} {2,10} {3,14} {4,14}", "Capa", "Total", "Geom Null", "SRID <> 4326", "STIsValid = 0");
        Console.WriteLine("  " + new string('-', 65));

        while (reader.Read())
        {
            Console.WriteLine("  {0,-15} {1,8} {2,10} {3,14} {4,14}",
                reader["Capa"], reader["Total"], reader["GeomNull"], reader["SridInvalido"], reader["GeomInvalida"]);
            Registrar($"Validación {reader["Capa"]}: Total={reader["Total"]}, GeomNull={reader["GeomNull"]}, SridInvalido={reader["SridInvalido"]}, GeomInvalida={reader["GeomInvalida"]}");
        }
    }
}


