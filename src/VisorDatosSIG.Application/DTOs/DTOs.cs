namespace VisorDatosSIG.Application.DTOs;

public class LoginDto
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UsuarioInfoDto
{
    public int IdUsuario { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<MenuItemDto> PermisosMenu { get; set; } = new();
}

public class MenuItemDto
{
    public int IdMenu { get; set; }
    public int? IdMenuPadre { get; set; }
    public int Nivel { get; set; }
    public string NombreMenu { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public bool PuedeVer { get; set; }
    public bool PuedeCrear { get; set; }
    public bool PuedeEditar { get; set; }
    public bool PuedeEliminar { get; set; }
    public List<MenuItemDto> Submenus { get; set; } = new();
}

public class InmuebleSearchResultDto
{
    public int IdCodigo { get; set; }
    public int? CodFijo { get; set; }
    public string? Nombre { get; set; }
    public byte Estado { get; set; }
    public string EstadoDesc => Estado switch
    {
        1 => "Normal",
        2 => "Para Corte",
        3 => "Cortado",
        4 => "Baja Parcial",
        5 => "Baja Total",
        _ => "Desconocido"
    };
    public DateTime FechaCambioEstado { get; set; }
    public string? UV { get; set; }
    public string? MZA { get; set; }
    public string? NroLote { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
}

public class EstadisticasCapasDto
{
    public int TotalManzanas { get; set; }
    public int TotalLotes { get; set; }
    public int TotalCodigosFijos { get; set; }
    public int TotalVias { get; set; }
}
