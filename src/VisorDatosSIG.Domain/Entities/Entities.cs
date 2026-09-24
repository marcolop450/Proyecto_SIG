namespace VisorDatosSIG.Domain.Entities;

public class Manzana
{
    public int IdManzana { get; set; }
    public int? IdOrigen { get; set; }
    public string? UV_MZA { get; set; }
    public string? UV { get; set; }
    public string? MZA { get; set; }
    public string? WktGeom { get; set; }
}

public class Lote
{
    public int IdLote { get; set; }
    public int? IdOrigen { get; set; }
    public string? NroLote { get; set; }
    public int? IdManzana { get; set; }
    public string? WktGeom { get; set; }
}

public class CodigoFijo
{
    public int IdCodigo { get; set; }
    public int? CodF_SQL { get; set; }
    public string? CodF_SIG { get; set; }
    public int? CodFijo { get; set; }
    public string? Nombre { get; set; }
    public byte Estado { get; set; } = 1;
    public DateTime FechaCambioEstado { get; set; } = DateTime.UtcNow;
    public int? IdLote { get; set; }
    public double? Longitud { get; set; }
    public double? Latitud { get; set; }
    public string? WktGeom { get; set; }
}

public class Via
{
    public int IdVia { get; set; }
    public int? OBJECTID { get; set; }
    public string? Nombre { get; set; }
    public string? TipoVia { get; set; }
    public string? OSMID { get; set; }
    public string? WktGeom { get; set; }
}

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public int Iteraciones { get; set; } = 100000;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}

public class Rol
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; } = true;
}

public class MenuOpcion
{
    public int IdMenu { get; set; }
    public int? IdMenuPadre { get; set; }
    public int Nivel { get; set; }
    public string NombreMenu { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public bool Estado { get; set; } = true;
}

public class UsuarioMenu
{
    public int IdUsuarioMenu { get; set; }
    public int IdUsuario { get; set; }
    public int IdMenu { get; set; }
    public bool PuedeVer { get; set; } = true;
    public bool PuedeCrear { get; set; }
    public bool PuedeEditar { get; set; }
    public bool PuedeEliminar { get; set; }
}
