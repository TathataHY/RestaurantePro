namespace RestaurantePro.Application.Comercial.Clientes.DTOs;

/// <summary>
/// DTO resumido para Cliente - Optimizado para listas y performance
/// Contiene solo los campos esenciales para mostrar en grids y listas
/// </summary>
public class ClienteSummaryDto
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Email principal del cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono principal del cliente
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de cliente (texto legible)
    /// </summary>
    public string TipoCliente { get; set; } = string.Empty;

    /// <summary>
    /// Estado activo del cliente
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Usuario que registró el cliente
    /// </summary>
    public string RegistradoPor { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Ciudad del cliente
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// País del cliente
    /// </summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>
    /// Puntos de fidelización actuales
    /// </summary>
    public int PuntosFidelizacion { get; set; }

    /// <summary>
    /// Nivel de fidelización (Bronce, Plata, Oro, Platino)
    /// </summary>
    public string NivelFidelizacion { get; set; } = string.Empty;

    /// <summary>
    /// Número total de órdenes realizadas
    /// </summary>
    public int TotalOrdenes { get; set; }

    /// <summary>
    /// Monto total de compras históricas
    /// </summary>
    public decimal MontoTotalCompras { get; set; }

    /// <summary>
    /// Fecha de la última orden
    /// </summary>
    public DateTime? FechaUltimaOrden { get; set; }

    /// <summary>
    /// Promedio de compra por orden
    /// </summary>
    public decimal PromedioCompra { get; set; }

    /// <summary>
    /// Indicador de cliente frecuente
    /// </summary>
    public bool EsFrecuente { get; set; }

    /// <summary>
    /// Días desde la última visita
    /// </summary>
    public int DiasSinVisitar { get; set; }

    /// <summary>
    /// Indicador de cliente VIP
    /// </summary>
    public bool EsVIP { get; set; }

    /// <summary>
    /// Edad calculada del cliente
    /// </summary>
    public int? Edad => FechaNacimiento.HasValue 
        ? DateTime.Now.Year - FechaNacimiento.Value.Year 
        : null;

    /// <summary>
    /// Estado de actividad (Activo, Inactivo, Nuevo)
    /// </summary>
    public string EstadoActividad => DiasSinVisitar switch
    {
        <= 30 => "Activo",
        <= 90 => "Regular", 
        <= 180 => "Inactivo",
        _ => "Perdido"
    };

    /// <summary>
    /// Categoría de valor del cliente
    /// </summary>
    public string CategoriaValor => MontoTotalCompras switch
    {
        >= 10000 => "Alto Valor",
        >= 5000 => "Medio Valor",
        >= 1000 => "Valor Regular",
        _ => "Nuevo Cliente"
    };
} 