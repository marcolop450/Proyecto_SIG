using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Authorize]
public class MapaController : Controller
{
    private readonly IGeoDataService _geoDataService;

    public MapaController(IGeoDataService geoDataService)
    {
        _geoDataService = geoDataService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? texto, string? uv, string? mza, string? lote)
    {
        ViewBag.Texto = texto;
        ViewBag.UV = uv;
        ViewBag.Mza = mza;
        ViewBag.Lote = lote;

        var filtros = await _geoDataService.ObtenerFiltrosDisponiblesAsync(uv);
        ViewBag.ListaUV = filtros.ListaUV;
        ViewBag.ListaMZA = filtros.ListaMZA;
        ViewBag.ListaLotes = filtros.ListaLotes;

        return View();
    }
}
