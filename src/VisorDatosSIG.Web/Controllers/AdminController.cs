using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly IGeoDataService _geoDataService;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AdminController(IGeoDataService geoDataService, IConfiguration configuration)
    {
        _geoDataService = geoDataService;
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=VisorDatosSIG;Integrated Security=True;TrustServerCertificate=True;";
    }

    [HttpGet]
    public async Task<IActionResult> Configuracion()
    {
        var stats = await _geoDataService.ObtenerEstadisticasAsync();
        ViewBag.Stats = stats;
        ViewBag.ConnectionString = _connectionString;
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

    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sqlUsuarios = @"
            SELECT u.IdUsuario, u.Login, u.Nombre, u.Activo, u.FechaRegistro,
                   STRING_AGG(r.NombreRol, ', ') AS RolesAsignados
            FROM dbo.Usuarios u
            LEFT JOIN dbo.UsuariosRoles ur ON ur.IdUsuario = u.IdUsuario
            LEFT JOIN dbo.Roles r ON r.IdRol = ur.IdRol
            GROUP BY u.IdUsuario, u.Login, u.Nombre, u.Activo, u.FechaRegistro
            ORDER BY u.IdUsuario;";

        const string sqlRoles = @"
            SELECT IdRol, NombreRol, Descripcion, Estado
            FROM dbo.Roles
            ORDER BY IdRol;";

        var usuarios = await conn.QueryAsync<dynamic>(sqlUsuarios);
        var roles = await conn.QueryAsync<dynamic>(sqlRoles);

        ViewBag.Roles = roles;
        return View(usuarios);
    }
}
