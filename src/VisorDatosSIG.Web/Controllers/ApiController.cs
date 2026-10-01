using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisorDatosSIG.Application.Interfaces;
using VisorDatosSIG.Application.DTOs;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace VisorDatosSIG.Web.Controllers;



[Route("api")]
[ApiController]
[Authorize]
public class ApiController : ControllerBase
{
    private readonly IGeoDataService _geoDataService;
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public ApiController(IGeoDataService geoDataService, IAuthService authService, IConfiguration config)
    {
        _geoDataService = geoDataService;
        _authService = authService;
        _config = config;
    }

    [AllowAnonymous]
    [HttpPost("mobile/login")]
    public async Task<IActionResult> MobileLogin([FromBody] LoginMobileRequest request)
    {
        var usuario = await _authService.ValidarCredencialesAsync(request.Username, request.Password);
        if (usuario == null)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, usuario.Login)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            usuario = usuario.Login
        });
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
    public async Task<IActionResult> GetFiltros([FromQuery] string? uv)
    {
        var filtros = await _geoDataService.ObtenerFiltrosDisponiblesAsync(uv);
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

