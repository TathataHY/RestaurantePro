using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO base para clientes del restaurante
/// </summary>
public class ClienteDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
    public string Apellidos { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }
    
    [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
    public string? CodigoPostal { get; set; }
    
    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }
    
    public DateTime FechaNacimiento { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaVisita { get; set; }
    public bool EstaActivo { get; set; } = true;
    public bool AceptaMarketing { get; set; } = false;
    public bool AceptaTerminos { get; set; } = true;
    
    // Información comercial
    public decimal TotalGastado { get; set; } = 0;
    public int TotalCompras { get; set; } = 0;
    public int TotalVisitas { get; set; } = 0;
    public decimal PromedioGasto { get; set; } = 0;
    public string Segmento { get; set; } = "Nuevo";
    public int PuntosFidelizacion { get; set; } = 0;
    
    // Preferencias
    public string? PreferenciasAlimentarias { get; set; }
    public string? Alergias { get; set; }
    public string? NotasEspeciales { get; set; }
    
    // Relaciones
    public List<ClienteComprasDto> ComprasRecientes { get; set; } = new();
    public List<ClienteReservacionDto> Reservaciones { get; set; } = new();
    public List<ClienteTarjetaFidelizacionDto> TarjetasFidelizacion { get; set; } = new();
    
    // Propiedades calculadas
    public string NombreCompleto => $"{Nombre} {Apellidos}";
    public int Edad => DateTime.Today.Year - FechaNacimiento.Year - (DateTime.Today.DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);
    public bool EsClienteFrecuente => TotalVisitas >= 5;
    public bool EsClienteVIP => TotalGastado >= 1000 || PuntosFidelizacion >= 1000;
    public string EstadoCliente => EstaActivo ? "Activo" : "Inactivo";
    public string NivelFidelizacion => PuntosFidelizacion switch
    {
        >= 5000 => "Oro",
        >= 2000 => "Plata",
        >= 500 => "Bronce",
        _ => "Básico"
    };
}

/// <summary>
/// DTO para crear un nuevo cliente
/// </summary>
public class CrearClienteRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
    public string Apellidos { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }
    
    [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
    public string? CodigoPostal { get; set; }
    
    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }
    
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-25);
    
    public bool AceptaMarketing { get; set; } = false;
    public bool AceptaTerminos { get; set; } = true;
    public string? PreferenciasAlimentarias { get; set; }
    public string? Alergias { get; set; }
    public string? NotasEspeciales { get; set; }
}

/// <summary>
/// DTO para actualizar un cliente existente
/// </summary>
public class ActualizarClienteRequest
{
    [Required(ErrorMessage = "El ID es obligatorio")]
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
    public string Apellidos { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }
    
    [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
    public string? CodigoPostal { get; set; }
    
    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }
    
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; }
    
    public bool EstaActivo { get; set; } = true;
    public bool AceptaMarketing { get; set; } = false;
    public string? PreferenciasAlimentarias { get; set; }
    public string? Alergias { get; set; }
    public string? NotasEspeciales { get; set; }

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
    public string Estado { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El segmento no puede exceder 50 caracteres")]
    public string Segmento { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El nivel de fidelización no puede exceder 50 caracteres")]
    public string NivelFidelizacion { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para historial de compras del cliente
/// </summary>
public class ClienteComprasDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public decimal TotalCompra { get; set; }
    public string EstadoPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string? MesaNombre { get; set; }
    public string? MeseroNombre { get; set; }
    public List<ClienteDetalleCompraDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalles de compra del cliente
/// </summary>
public class ClienteDetalleCompraDto
{
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para reservaciones del cliente
/// </summary>
public class ClienteReservacionDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateTime FechaReservacion { get; set; }
    public TimeSpan HoraReservacion { get; set; }
    public int NumeroPersonas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MesaNombre { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
}

/// <summary>
/// DTO para tarjetas de fidelización del cliente
/// </summary>
public class ClienteTarjetaFidelizacionDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string NumeroTarjeta { get; set; } = string.Empty;
    public string TipoTarjeta { get; set; } = string.Empty;
    public int PuntosAcumulados { get; set; }
    public int PuntosCanjeados { get; set; }
    public int PuntosDisponibles { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool EstaActiva { get; set; } = true;
    public string Estado { get; set; } = "Activa";
}

/// <summary>
/// DTO para estadísticas de clientes
/// </summary>
public class ClienteEstadisticasDto
{
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int ClientesInactivos { get; set; }
    public int ClientesNuevos { get; set; }
    public int ClientesFrecuentes { get; set; }
    public int ClientesVIP { get; set; }
    public decimal TotalGastado { get; set; }
    public decimal PromedioGasto { get; set; }
    public int TotalCompras { get; set; }
    public int TotalVisitas { get; set; }
    public List<ClienteSegmentoDto> Segmentos { get; set; } = new();
    public List<ClienteEdadDto> DistribucionEdades { get; set; } = new();
    public List<ClienteCiudadDto> DistribucionCiudades { get; set; } = new();
}

/// <summary>
/// DTO para segmentos de clientes
/// </summary>
public class ClienteSegmentoDto
{
    public string Segmento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal TotalGastado { get; set; }
    public decimal PromedioGasto { get; set; }
}

/// <summary>
/// DTO para distribución por edades
/// </summary>
public class ClienteEdadDto
{
    public string RangoEdad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para distribución por ciudades
/// </summary>
public class ClienteCiudadDto
{
    public string Ciudad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para filtros de clientes
/// </summary>
public class ClienteFiltrosDto
{
    public string? Busqueda { get; set; }
    public string? Segmento { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; } // Activo, Inactivo
    public string? NivelFidelizacion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal? GastoMinimo { get; set; }
    public decimal? GastoMaximo { get; set; }
    public int? VisitasMinimas { get; set; }
    public bool? AceptaMarketing { get; set; }
    public string? OrdenarPor { get; set; } = "NombreCompleto";
    public string? DireccionOrden { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para búsqueda de clientes
/// </summary>
public class BuscarClienteRequest
{
    [Required(ErrorMessage = "El término de búsqueda es obligatorio")]
    [StringLength(100, ErrorMessage = "El término de búsqueda no puede exceder 100 caracteres")]
    public string Termino { get; set; } = string.Empty;
    
    public bool IncluirInactivos { get; set; } = false;
    public int Limite { get; set; } = 10;
}

/// <summary>
/// DTO para respuesta de búsqueda de clientes
/// </summary>
public class BuscarClienteResponse
{
    public List<ClienteDto> Clientes { get; set; } = new();
    public int TotalEncontrados { get; set; }
    public bool TieneMasResultados { get; set; }
}
