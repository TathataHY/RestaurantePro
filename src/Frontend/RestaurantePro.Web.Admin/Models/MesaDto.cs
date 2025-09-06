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
    public string Tipo { get; set; } = string.Empty;
    public DateTime UltimaActualizacion { get; set; }
}

public class CrearMesaRequest
{
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class ActualizarMesaRequest
{
    public string Numero { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}


