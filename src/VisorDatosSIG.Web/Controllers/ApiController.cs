using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly IGeoDataService _geoDataService;

    public ApiController(IGeoDataService geoDataService)
    {
        _geoDataService = geoDataService;
    }

    [HttpGet("estadisticas")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEstadisticas()
    {
        var stats = await _geoDataService.ObtenerEstadisticasAsync();
        return Ok(stats);
    }

    [HttpGet("capas/manzanas")]
    [AllowAnonymous]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetManzanas([FromQuery] string? bbox)
    {
        var geojson = await _geoDataService.ObtenerManzanasGeoJsonAsync(bbox);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("capas/lotes")]
    [AllowAnonymous]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetLotes([FromQuery] string? bbox, [FromQuery] int limit = 3000)
    {
        var geojson = await _geoDataService.ObtenerLotesGeoJsonAsync(bbox, limit);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("capas/vias")]
    [AllowAnonymous]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetVias([FromQuery] string? bbox)
    {
        var geojson = await _geoDataService.ObtenerViasGeoJsonAsync(bbox);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("capas/codigosfijos")]
    [AllowAnonymous]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetCodigosFijos([FromQuery] string? bbox, [FromQuery] int limit = 6500)
    {
        var geojson = await _geoDataService.ObtenerCodigosFijosGeoJsonAsync(bbox, limit);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("busqueda")]
    [AllowAnonymous]
    public async Task<IActionResult> Buscar([FromQuery] string? texto, [FromQuery] string? uv, [FromQuery] string? mza, [FromQuery] string? lote, [FromQuery] string? tipoPredio = "TODOS")
    {
        var resultados = await _geoDataService.BuscarInmueblesAsync(texto, uv, mza, lote, tipoPredio);
        return Ok(resultados);
    }

    [HttpGet("filtros")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFiltros([FromQuery] string? uv)
    {
        var filtros = await _geoDataService.ObtenerFiltrosDisponiblesAsync(uv);
        return Ok(filtros);
    }

    [HttpGet("detalle")]
    [AllowAnonymous]
    public async Task<IActionResult> Detalle([FromQuery] string capa, [FromQuery] int id)
    {
        var detalle = await _geoDataService.ObtenerDetalleEntidadAsync(capa, id);
        if (detalle == null) return NotFound(new { mensaje = "Entidad no encontrada" });
        return Ok(detalle);
    }

    [HttpPost("suministros/cambiar-estado")]
    public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoDto dto)
    {
        if (dto.CodFijo <= 0 || dto.NuevoEstado < 1 || dto.NuevoEstado > 5)
        {
            return BadRequest(new { success = false, mensaje = "Parámetros de estado o código inválidos." });
        }

        string usuario = User.Identity?.Name ?? "admin";
        bool exito = await _geoDataService.CambiarEstadoSuministroAsync(dto.CodFijo, dto.NuevoEstado, usuario, dto.Motivo);
        if (!exito)
        {
            return StatusCode(500, new { success = false, mensaje = "No se pudo actualizar el estado del suministro." });
        }

        string estadoDesc = dto.NuevoEstado switch {
            1 => "Normal",
            2 => "Para Corte",
            3 => "Cortado",
            4 => "Baja Parcial",
            5 => "Baja Total",
            _ => "Otro"
        };

        return Ok(new {
            success = true,
            codFijo = dto.CodFijo,
            nuevoEstado = dto.NuevoEstado,
            estadoDesc,
            mensaje = $"Estado actualizado con éxito a {estadoDesc}."
        });
    }
}

public class CambiarEstadoDto
{
    public int CodFijo { get; set; }
    public byte NuevoEstado { get; set; }
    public string? Motivo { get; set; }
}

