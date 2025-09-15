namespace RestaurantePro.Web.Admin.Models;

public class MesaDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Notas { get; set; }
    public bool TieneVentana { get; set; }
    public bool TieneSofa { get; set; }
    public bool EsAccesible { get; set; }
    public bool TieneEnchufe { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class CrearMesaRequest
{
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string Estado { get; set; } = "Disponible";
    public string? Descripcion { get; set; }
    public string? Notas { get; set; }
    public bool TieneVentana { get; set; }
    public bool TieneSofa { get; set; }
    public bool EsAccesible { get; set; }
    public bool TieneEnchufe { get; set; }
}

public class ActualizarMesaRequest
{
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Notas { get; set; }
    public bool TieneVentana { get; set; }
    public bool TieneSofa { get; set; }
    public bool EsAccesible { get; set; }
    public bool TieneEnchufe { get; set; }
}


