using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class ApiController : ControllerBase
{
    private readonly IGeoDataService _geoDataService;

    public ApiController(IGeoDataService geoDataService)
    {
        _geoDataService = geoDataService;
    }

    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticas()
    {
        var stats = await _geoDataService.ObtenerEstadisticasAsync();
        return Ok(stats);
    }

    [HttpGet("capas/manzanas")]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetManzanas([FromQuery] string? bbox)
    {
        var geojson = await _geoDataService.ObtenerManzanasGeoJsonAsync(bbox);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("capas/lotes")]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetLotes([FromQuery] string? bbox, [FromQuery] int limit = 3000)
    {
        var geojson = await _geoDataService.ObtenerLotesGeoJsonAsync(bbox, limit);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("capas/vias")]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetVias([FromQuery] string? bbox)
    {
        var geojson = await _geoDataService.ObtenerViasGeoJsonAsync(bbox);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("capas/codigosfijos")]
    [Produces("application/geo+json")]
    public async Task<IActionResult> GetCodigosFijos([FromQuery] string? bbox, [FromQuery] int limit = 6500)
    {
        var geojson = await _geoDataService.ObtenerCodigosFijosGeoJsonAsync(bbox, limit);
        return Content(geojson, "application/geo+json");
    }

    [HttpGet("busqueda")]
    public async Task<IActionResult> Buscar([FromQuery] string? texto, [FromQuery] string? uv, [FromQuery] string? mza, [FromQuery] string? lote)
    {
        var resultados = await _geoDataService.BuscarInmueblesAsync(texto, uv, mza, lote);
        return Ok(resultados);
    }

    [HttpGet("filtros")]
    public async Task<IActionResult> GetFiltros()
    {
        var filtros = await _geoDataService.ObtenerFiltrosDisponiblesAsync();
        return Ok(filtros);
    }

    [HttpGet("detalle")]
    public async Task<IActionResult> Detalle([FromQuery] string capa, [FromQuery] int id)
    {
        var detalle = await _geoDataService.ObtenerDetalleEntidadAsync(capa, id);
        if (detalle == null) return NotFound(new { mensaje = "Entidad no encontrada" });
        return Ok(detalle);
    }
}

