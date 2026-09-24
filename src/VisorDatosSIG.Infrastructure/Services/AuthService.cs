using System.Security.Cryptography;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using VisorDatosSIG.Application.DTOs;
using VisorDatosSIG.Application.Interfaces;

namespace VisorDatosSIG.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly string _connectionString;

    public AuthService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=VisorDatosSIG;Integrated Security=True;TrustServerCertificate=True;";
    }

    public bool VerificarPassword(string password, byte[] hashEsperado, byte[] salt, int iteraciones)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteraciones, HashAlgorithmName.SHA256);
        byte[] hashCalculado = pbkdf2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }

    public async Task<UsuarioInfoDto?> ValidarCredencialesAsync(string login, string password)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sqlUser = @"
            SELECT IdUsuario, Login, Nombre, PasswordHash, PasswordSalt, Iteraciones, Activo
            FROM dbo.Usuarios
            WHERE Login = @Login AND Activo = 1;";

        var user = await conn.QuerySingleOrDefaultAsync<dynamic>(sqlUser, new { Login = login });
        if (user == null) return null;

        byte[] hashEsperado = (byte[])user.PasswordHash;
        byte[] salt = (byte[])user.PasswordSalt;
        int iteraciones = (int)user.Iteraciones;

        if (!VerificarPassword(password, hashEsperado, salt, iteraciones))
            return null;

        int idUsuario = (int)user.IdUsuario;

        const string sqlRoles = @"
            SELECT r.NombreRol
            FROM dbo.UsuariosRoles ur
            JOIN dbo.Roles r ON r.IdRol = ur.IdRol
            WHERE ur.IdUsuario = @IdUsuario AND r.Estado = 1;";

        var roles = (await conn.QueryAsync<string>(sqlRoles, new { IdUsuario = idUsuario })).ToList();

        const string sqlMenu = @"
            SELECT m.IdMenu, m.IdMenuPadre, m.Nivel, m.NombreMenu, m.Url, m.Icono, m.Orden,
                   um.PuedeVer, um.PuedeCrear, um.PuedeEditar, um.PuedeEliminar
            FROM dbo.UsuarioMenu um
            JOIN dbo.MenuOpciones m ON m.IdMenu = um.IdMenu
            WHERE um.IdUsuario = @IdUsuario AND m.Estado = 1 AND um.PuedeVer = 1
            ORDER BY m.Nivel, m.Orden;";

        var menuFlat = (await conn.QueryAsync<MenuItemDto>(sqlMenu, new { IdUsuario = idUsuario })).ToList();

        var menuJerarquico = new List<MenuItemDto>();
        var lookup = menuFlat.ToDictionary(m => m.IdMenu);
        foreach (var item in menuFlat)
        {
            if (item.IdMenuPadre.HasValue && lookup.TryGetValue(item.IdMenuPadre.Value, out var padre))
            {
                padre.Submenus.Add(item);
            }
            else
            {
                menuJerarquico.Add(item);
            }
        }

        return new UsuarioInfoDto
        {
            IdUsuario = idUsuario,
            Login = (string)user.Login,
            Nombre = (string)user.Nombre,
            Roles = roles,
            PermisosMenu = menuJerarquico
        };
    }
}
