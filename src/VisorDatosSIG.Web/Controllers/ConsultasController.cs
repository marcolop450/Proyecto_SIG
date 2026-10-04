using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.DTOs;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Authorize]
public class ConsultasController : Controller
{
    private readonly IGeoDataService _geoDataService;

    public ConsultasController(IGeoDataService geoDataService)
    {
        _geoDataService = geoDataService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? texto, string? uv, string? mza, string? lote, string? tipoPredio = "TODOS")
    {
        ViewBag.Texto = texto;
        ViewBag.UV = uv;
        ViewBag.Mza = mza;
        ViewBag.Lote = lote;
        ViewBag.TipoPredio = string.IsNullOrWhiteSpace(tipoPredio) ? "TODOS" : tipoPredio;

        var filtros = await _geoDataService.ObtenerFiltrosDisponiblesAsync(uv);
        ViewBag.ListaUV = filtros.ListaUV;
        ViewBag.ListaMZA = filtros.ListaMZA;
        ViewBag.ListaLotes = filtros.ListaLotes;

        return View(Enumerable.Empty<InmuebleSearchResultDto>());
    }
}
