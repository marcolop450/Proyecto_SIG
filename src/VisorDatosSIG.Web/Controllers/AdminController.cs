using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly IGeoDataService _geoDataService;
    private readonly IConfiguration _configuration;

    public AdminController(IGeoDataService geoDataService, IConfiguration configuration)
    {
        _geoDataService = geoDataService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Configuracion()
    {
        var stats = await _geoDataService.ObtenerEstadisticasAsync();
        ViewBag.Stats = stats;
        ViewBag.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
        return View();
    }

    [HttpGet]
    public IActionResult Bitacora()
    {
        string logPath = Path.Combine(Directory.GetCurrentDirectory(), "bitacora_migracion.txt");
        if (!System.IO.File.Exists(logPath))
        {
            logPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "bitacora_migracion.txt");
            logPath = Path.GetFullPath(logPath);
        }

        string logContent = System.IO.File.Exists(logPath) 
            ? System.IO.File.ReadAllText(logPath) 
            : "No se encontró el archivo de bitácora local.";

        ViewBag.LogPath = logPath;
        return View(model: logContent);
    }
}
