using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.IO;
using VisorDatosSIG.Application.DTOs;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Infrastructure.Services;

public class GeoDataService : IGeoDataService
{
    private readonly string _connectionString;
    private readonly WKTReader _wktReader = new();
    private readonly GeoJsonWriter _geoJsonWriter = new();

    public GeoDataService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=VisorDatosSIG;Integrated Security=True;TrustServerCertificate=True;";
    }

    public async Task<EstadisticasCapasDto> ObtenerEstadisticasAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM dbo.Manzanas WHERE Geom IS NOT NULL) AS TotalManzanas,
                (SELECT COUNT(*) FROM dbo.Lotes WHERE Geom IS NOT NULL) AS TotalLotes,
                (SELECT COUNT(*) FROM dbo.CodigosFijos WHERE Geom IS NOT NULL) AS TotalCodigosFijos,
                (SELECT COUNT(*) FROM dbo.Vias WHERE Geom IS NOT NULL) AS TotalVias;";

        return await conn.QuerySingleAsync<EstadisticasCapasDto>(sql);
    }

    public async Task<string> ObtenerManzanasGeoJsonAsync(string? bbox = null)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT IdManzana, UV_MZA, UV, MZA, Geom.STAsText() AS Wkt
            FROM dbo.Manzanas
            WHERE Geom IS NOT NULL;";

        var rows = await conn.QueryAsync<dynamic>(sql);
        return ConstruirFeatureCollection(rows, (r, props) => {
            props["id"] = (int)r.IdManzana;
            props["uv_mza"] = (string?)r.UV_MZA;
            props["uv"] = (string?)r.UV;
            props["mza"] = (string?)r.MZA;
        });
    }

    public async Task<string> ObtenerLotesGeoJsonAsync(string? bbox = null, int limit = 5000)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @$"
            SELECT TOP ({limit}) l.IdLote, l.NroLote, l.IdManzana, m.UV, m.MZA, l.Geom.STAsText() AS Wkt
            FROM dbo.Lotes l
            LEFT JOIN dbo.Manzanas m ON m.IdManzana = l.IdManzana
            WHERE l.Geom IS NOT NULL;";

        var rows = await conn.QueryAsync<dynamic>(sql);
        return ConstruirFeatureCollection(rows, (r, props) => {
            props["id"] = (int)r.IdLote;
            props["nroLote"] = (string?)r.NroLote;
            props["idManzana"] = (int?)r.IdManzana;
            props["uv"] = (string?)r.UV;
            props["mza"] = (string?)r.MZA;
        });
    }

    public async Task<string> ObtenerViasGeoJsonAsync(string? bbox = null)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT IdVia, OBJECTID, Nombre, TipoVia, OSMID, Geom.STAsText() AS Wkt
            FROM dbo.Vias
            WHERE Geom IS NOT NULL;";

        var rows = await conn.QueryAsync<dynamic>(sql);
        return ConstruirFeatureCollection(rows, (r, props) => {
            props["id"] = (int)r.IdVia;
            props["objectid"] = (int?)r.OBJECTID;
            props["nombre"] = (string?)r.Nombre;
            props["tipoVia"] = (string?)r.TipoVia;
            props["osmid"] = (string?)r.OSMID;
        });
    }

    public async Task<string> ObtenerCodigosFijosGeoJsonAsync(string? bbox = null, int limit = 6500)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @$"
            SELECT TOP ({limit}) 
                c.IdCodigo, c.CodFijo, c.Nombre, c.Estado, c.FechaCambioEstado,
                c.Longitud, c.Latitud, c.IdLote, l.NroLote, m.UV, m.MZA,
                c.Geom.STAsText() AS Wkt
            FROM dbo.CodigosFijos c
            LEFT JOIN dbo.Lotes l ON l.IdLote = c.IdLote
            LEFT JOIN dbo.Manzanas m ON m.IdManzana = l.IdManzana
            WHERE c.Geom IS NOT NULL;";

        var rows = await conn.QueryAsync<dynamic>(sql);
        return ConstruirFeatureCollection(rows, (r, props) => {
            props["id"] = (int)r.IdCodigo;
            props["codFijo"] = (int?)r.CodFijo;
            props["nombre"] = (string?)r.Nombre;
            props["estado"] = (byte)r.Estado;
            props["estadoDesc"] = ((byte)r.Estado) switch {
                1 => "Normal",
                2 => "Para Corte",
                3 => "Cortado",
                4 => "Baja Parcial",
                5 => "Baja Total",
                _ => "Otro"
            };
            props["nroLote"] = (string?)r.NroLote;
            props["uv"] = (string?)r.UV;
            props["mza"] = (string?)r.MZA;
        });
    }

    public async Task<IEnumerable<InmuebleSearchResultDto>> BuscarInmueblesAsync(string? texto, string? uv, string? mza, string? lote)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var p = new DynamicParameters();
        p.Add("@Texto", string.IsNullOrWhiteSpace(texto) ? null : texto.Trim());
        p.Add("@UV", string.IsNullOrWhiteSpace(uv) ? null : uv.Trim());
        p.Add("@Mza", string.IsNullOrWhiteSpace(mza) ? null : mza.Trim());
        p.Add("@Lote", string.IsNullOrWhiteSpace(lote) ? null : lote.Trim());

        return await conn.QueryAsync<InmuebleSearchResultDto>(
            "dbo.sp_BuscarInmueble",
            p,
            commandType: System.Data.CommandType.StoredProcedure
        );
    }

    public async Task<FiltrosDisponiblesDto> ObtenerFiltrosDisponiblesAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT DISTINCT LTRIM(RTRIM(UV)) AS UV 
            FROM dbo.Manzanas 
            WHERE UV IS NOT NULL AND LTRIM(RTRIM(UV)) <> '' 
            ORDER BY UV;

            SELECT DISTINCT LTRIM(RTRIM(MZA)) AS MZA 
            FROM dbo.Manzanas 
            WHERE MZA IS NOT NULL AND LTRIM(RTRIM(MZA)) <> '' 
            ORDER BY MZA;

            SELECT DISTINCT TOP 100 LTRIM(RTRIM(NroLote)) AS NroLote 
            FROM dbo.Lotes 
            WHERE NroLote IS NOT NULL AND LTRIM(RTRIM(NroLote)) <> '' 
            ORDER BY NroLote;";

        using var multi = await conn.QueryMultipleAsync(sql);
        var uvs = (await multi.ReadAsync<string>()).ToList();
        var mzas = (await multi.ReadAsync<string>()).ToList();
        var lotes = (await multi.ReadAsync<string>()).ToList();

        return new FiltrosDisponiblesDto
        {
            ListaUV = uvs,
            ListaMZA = mzas,
            ListaLotes = lotes
        };
    }

    public async Task<object?> ObtenerDetalleEntidadAsync(string capa, int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        string capaNormal = capa.Trim().ToLowerInvariant();
        switch (capaNormal)
        {
            case "manzanas":
                return await conn.QuerySingleOrDefaultAsync(
                    "SELECT IdManzana, UV_MZA, UV, MZA, Geom.STAsText() AS Wkt FROM dbo.Manzanas WHERE IdManzana = @Id", new { Id = id });

            case "lotes":
                return await conn.QuerySingleOrDefaultAsync(@"
                    SELECT l.IdLote, l.NroLote, l.IdManzana, m.UV, m.MZA, l.Geom.STAsText() AS Wkt
                    FROM dbo.Lotes l
                    LEFT JOIN dbo.Manzanas m ON m.IdManzana = l.IdManzana
                    WHERE l.IdLote = @Id", new { Id = id });

            case "codigosfijos":
                return await conn.QuerySingleOrDefaultAsync(@"
                    SELECT c.IdCodigo, c.CodF_SQL, c.CodF_SIG, c.CodFijo, c.Nombre, c.Estado, c.FechaCambioEstado,
                           c.Longitud, c.Latitud, l.NroLote, m.UV, m.MZA, c.Geom.STAsText() AS Wkt
                    FROM dbo.CodigosFijos c
                    LEFT JOIN dbo.Lotes l ON l.IdLote = c.IdLote
                    LEFT JOIN dbo.Manzanas m ON m.IdManzana = l.IdManzana
                    WHERE c.IdCodigo = @Id", new { Id = id });

            case "vias":
                return await conn.QuerySingleOrDefaultAsync(
                    "SELECT IdVia, OBJECTID, Nombre, TipoVia, OSMID, Geom.STAsText() AS Wkt FROM dbo.Vias WHERE IdVia = @Id", new { Id = id });

            default:
                return null;
        }
    }

    private string ConstruirFeatureCollection(IEnumerable<dynamic> rows, Action<dynamic, Dictionary<string, object?>> fillProps)
    {
        var sb = new StringBuilder();
        sb.Append("{\"type\":\"FeatureCollection\",\"features\":[");
        bool first = true;

        foreach (var r in rows)
        {
            string? wkt = (string?)r.Wkt;
            if (string.IsNullOrWhiteSpace(wkt)) continue;

            try
            {
                var geom = _wktReader.Read(wkt);
                string geomJson = _geoJsonWriter.Write(geom);

                var props = new Dictionary<string, object?>();
                fillProps(r, props);
                string propsJson = JsonSerializer.Serialize(props);

                if (!first) sb.Append(',');
                first = false;

                sb.Append("{\"type\":\"Feature\",\"geometry\":");
                sb.Append(geomJson);
                sb.Append(",\"properties\":");
                sb.Append(propsJson);
                sb.Append('}');
            }
            catch
            {
                // Ignorar geometrías corruptas para no cortar la colección
            }
        }

        sb.Append("]}");
        return sb.ToString();
    }
}
