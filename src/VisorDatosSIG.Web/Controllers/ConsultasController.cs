using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Index(string? texto, string? uv, string? mza, string? lote)
    {
        ViewBag.Texto = texto;
        ViewBag.UV = uv;
        ViewBag.Mza = mza;
        ViewBag.Lote = lote;

        if (string.IsNullOrWhiteSpace(texto) && string.IsNullOrWhiteSpace(uv) && 
            string.IsNullOrWhiteSpace(mza) && string.IsNullOrWhiteSpace(lote))
        {
            return View(Enumerable.Empty<VisorDatosSIG.Application.DTOs.InmuebleSearchResultDto>());
        }

        var results = await _geoDataService.BuscarInmueblesAsync(texto, uv, mza, lote);
        return View(results);
    }
}
