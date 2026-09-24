using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IGeoDataService _geoDataService;

    public HomeController(IGeoDataService geoDataService)
    {
        _geoDataService = geoDataService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _geoDataService.ObtenerEstadisticasAsync();
        return View(stats);
    }
}
