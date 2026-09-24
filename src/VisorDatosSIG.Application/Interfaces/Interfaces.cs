using VisorDatosSIG.Application.DTOs;
using VisorDatosSIG.Domain.Entities;

namespace VisorDatosSIG.Application.Interfaces;

public interface IAuthService
{
    Task<UsuarioInfoDto?> ValidarCredencialesAsync(string login, string password);
    bool VerificarPassword(string password, byte[] hashEsperado, byte[] salt, int iteraciones);
}

public interface IGeoDataService
{
    Task<EstadisticasCapasDto> ObtenerEstadisticasAsync();
    Task<string> ObtenerManzanasGeoJsonAsync(string? bbox = null);
    Task<string> ObtenerLotesGeoJsonAsync(string? bbox = null, int limit = 2000);
    Task<string> ObtenerViasGeoJsonAsync(string? bbox = null);
    Task<string> ObtenerCodigosFijosGeoJsonAsync(string? bbox = null, int limit = 3000);
    Task<IEnumerable<InmuebleSearchResultDto>> BuscarInmueblesAsync(string? texto, string? uv, string? mza, string? lote);
    Task<object?> ObtenerDetalleEntidadAsync(string capa, int id);
}
