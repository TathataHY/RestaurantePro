using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO base para comandas del restaurante
/// </summary>
public class ComandaDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El número de comanda es obligatorio")]
    [StringLength(20, ErrorMessage = "El número de comanda no puede exceder 20 caracteres")]
    public string NumeroComanda { get; set; } = string.Empty;
    
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public DateTime? FechaEntrega { get; set; }
    
    [Required(ErrorMessage = "La mesa es obligatoria")]
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    public int MesaNumero { get; set; }
    public string MesaUbicacion { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El mesero es obligatorio")]
    public Guid MeseroId { get; set; }
    public string MeseroNombre { get; set; } = string.Empty;
    
    public Guid? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El estado es obligatorio")]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, EnProceso, Lista, Entregada, Cancelada
    public string Prioridad { get; set; } = "Normal"; // Baja, Normal, Alta, Urgente
    public string TipoComanda { get; set; } = "Mesa"; // Mesa, Domicilio, Mostrador
    
    [Range(1, int.MaxValue, ErrorMessage = "El número de personas debe ser mayor a 0")]
    public int NumeroPersonas { get; set; } = 1;
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    [StringLength(500, ErrorMessage = "Las notas de cocina no pueden exceder 500 caracteres")]
    public string? NotasCocina { get; set; }
    
    [StringLength(500, ErrorMessage = "Las notas de entrega no pueden exceder 500 caracteres")]
    public string? NotasEntrega { get; set; }
    
    public decimal Subtotal { get; set; } = 0;
    public decimal TotalDescuentos { get; set; } = 0;
    public decimal TotalImpuestos { get; set; } = 0;
    public decimal Total { get; set; } = 0;
    
    public bool EsUrgente { get; set; } = false;
    public bool RequiereFactura { get; set; } = true;
    public bool EsDomicilio { get; set; } = false;
    
    // Información de domicilio (si aplica)
    public string? DireccionDomicilio { get; set; }
    public string? TelefonoDomicilio { get; set; }
    public string? ReferenciasDomicilio { get; set; }
    public DateTime? TiempoEstimadoEntrega { get; set; }
    
    // Relaciones
    public List<ComandaDetalleDto> Detalles { get; set; } = new();
    public List<ComandaEstadoDto> HistorialEstados { get; set; } = new();
    
    // Propiedades calculadas
    public TimeSpan TiempoTranscurrido => DateTime.Now - FechaCreacion;
    public TimeSpan? TiempoProcesamiento => FechaFinalizacion?.Subtract(FechaInicio ?? FechaCreacion);
    public TimeSpan? TiempoTotal => FechaEntrega?.Subtract(FechaCreacion);
    public int CantidadItems => Detalles.Sum(d => d.Cantidad);
    public bool EstaPendiente => Estado == "Pendiente";
    public bool EstaEnProceso => Estado == "EnProceso";
    public bool EstaLista => Estado == "Lista";
    public bool EstaEntregada => Estado == "Entregada";
    public bool EstaCancelada => Estado == "Cancelada";
    public string EstadoVisual => Estado switch
    {
        "Pendiente" => "Pendiente",
        "EnProceso" => "En Proceso",
        "Lista" => "Lista",
        "Entregada" => "Entregada",
        "Cancelada" => "Cancelada",
        _ => "Desconocido"
    };
    public string ClaseEstado => Estado switch
    {
        "Pendiente" => "warning",
        "EnProceso" => "info",
        "Lista" => "success",
        "Entregada" => "primary",
        "Cancelada" => "danger",
        _ => "secondary"
    };
    public string ClasePrioridad => Prioridad switch
    {
        "Baja" => "secondary",
        "Normal" => "primary",
        "Alta" => "warning",
        "Urgente" => "danger",
        _ => "primary"
    };
    public int MinutosTranscurridos => (int)TiempoTranscurrido.TotalMinutes;
    public bool EsLenta => MinutosTranscurridos > 30 && !EstaEntregada;
    public bool EsMuyLenta => MinutosTranscurridos > 60 && !EstaEntregada;
}

/// <summary>
/// DTO para crear una nueva comanda
/// </summary>
public class CrearComandaRequest
{
    [Required(ErrorMessage = "La mesa es obligatoria")]
    public Guid MesaId { get; set; }
    
    [Required(ErrorMessage = "El mesero es obligatorio")]
    public Guid MeseroId { get; set; }
    
    public Guid? ClienteId { get; set; }
    public string TipoComanda { get; set; } = "Mesa";
    public int NumeroPersonas { get; set; } = 1;
    public string Prioridad { get; set; } = "Normal";
    public string? Observaciones { get; set; }
    public string? NotasCocina { get; set; }
    public bool EsUrgente { get; set; } = false;
    public bool RequiereFactura { get; set; } = true;
    public bool EsDomicilio { get; set; } = false;
    public string? DireccionDomicilio { get; set; }
    public string? TelefonoDomicilio { get; set; }
    public string? ReferenciasDomicilio { get; set; }
    
    [Required(ErrorMessage = "Los detalles son obligatorios")]
    public List<CrearComandaDetalleRequest> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para actualizar una comanda existente
/// </summary>
public class ActualizarComandaRequest
{
    [Required(ErrorMessage = "El ID es obligatorio")]
    public Guid Id { get; set; }
    
    public string Estado { get; set; } = "Pendiente";
    public string Prioridad { get; set; } = "Normal";
    public string? Observaciones { get; set; }
    public string? NotasCocina { get; set; }
    public string? NotasEntrega { get; set; }
    public bool EsUrgente { get; set; } = false;
    public int NumeroPersonas { get; set; } = 1;
}

/// <summary>
/// DTO para detalles de comanda
/// </summary>
public class ComandaDetalleDto
{
    public Guid Id { get; set; }
    public Guid ComandaId { get; set; }
    
    [Required(ErrorMessage = "El producto es obligatorio")]
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCategoria { get; set; } = string.Empty;
    public string ProductoDescripcion { get; set; } = string.Empty;
    public string? ProductoImagen { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal PrecioUnitario { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal Descuento { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
    public decimal Impuesto { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
    public decimal Subtotal { get; set; }
    
    public string Estado { get; set; } = "Pendiente"; // Pendiente, EnPreparacion, Listo, Entregado, Cancelado
    public string Prioridad { get; set; } = "Normal";
    public int TiempoEstimadoPreparacion { get; set; } = 15; // en minutos
    public DateTime? FechaInicioPreparacion { get; set; }
    public DateTime? FechaFinPreparacion { get; set; }
    public DateTime? FechaEntrega { get; set; }
    
    [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder 200 caracteres")]
    public string? Observaciones { get; set; }
    
    [StringLength(200, ErrorMessage = "Las modificaciones no pueden exceder 200 caracteres")]
    public string? Modificaciones { get; set; }
    
    // Propiedades calculadas
    public decimal Total => Subtotal - Descuento + Impuesto;
    public TimeSpan? TiempoPreparacion => FechaFinPreparacion?.Subtract(FechaInicioPreparacion ?? DateTime.Now);
    public bool EstaPendiente => Estado == "Pendiente";
    public bool EstaEnPreparacion => Estado == "EnPreparacion";
    public bool EstaListo => Estado == "Listo";
    public bool EstaEntregado => Estado == "Entregado";
    public bool EstaCancelado => Estado == "Cancelado";
    public int MinutosTranscurridos => FechaInicioPreparacion.HasValue ? 
        (int)(DateTime.Now - FechaInicioPreparacion.Value).TotalMinutes : 0;
    public bool EsLento => MinutosTranscurridos > TiempoEstimadoPreparacion && !EstaListo;
}

/// <summary>
/// DTO para crear detalle de comanda
/// </summary>
public class CrearComandaDetalleRequest
{
    [Required(ErrorMessage = "El producto es obligatorio")]
    public Guid ProductoId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal PrecioUnitario { get; set; }
    
    public decimal Descuento { get; set; } = 0;
    public decimal Impuesto { get; set; } = 0;
    public string? Observaciones { get; set; }
    public string? Modificaciones { get; set; }
    public string Prioridad { get; set; } = "Normal";
}

/// <summary>
/// DTO para historial de estados de comanda
/// </summary>
public class ComandaEstadoDto
{
    public Guid Id { get; set; }
    public Guid ComandaId { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTime FechaCambio { get; set; }
    public Guid UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string TipoCambio { get; set; } = "Estado"; // Estado, Prioridad, Observaciones
}

/// <summary>
/// DTO para estadísticas de comandas
/// </summary>
public class ComandaEstadisticasDto
{
    public int TotalComandas { get; set; }
    public int ComandasPendientes { get; set; }
    public int ComandasEnProceso { get; set; }
    public int ComandasListas { get; set; }
    public int ComandasEntregadas { get; set; }
    public int ComandasCanceladas { get; set; }
    public decimal TiempoPromedioPreparacion { get; set; } // en minutos
    public decimal TiempoPromedioTotal { get; set; } // en minutos
    public int ComandasUrgentes { get; set; }
    public int ComandasLentas { get; set; }
    public decimal TotalVentas { get; set; }
    public List<ComandaEstadoEstadisticasDto> Estados { get; set; } = new();
    public List<ComandaTiempoEstadisticasDto> TiemposPreparacion { get; set; } = new();
    public List<ComandaProductoEstadisticasDto> ProductosMasPedidos { get; set; } = new();
    public List<ComandaMeseroEstadisticasDto> RendimientoMeseros { get; set; } = new();
}

/// <summary>
/// DTO para estadísticas por estado
/// </summary>
public class ComandaEstadoEstadisticasDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal TiempoPromedio { get; set; }
}

/// <summary>
/// DTO para estadísticas de tiempos
/// </summary>
public class ComandaTiempoEstadisticasDto
{
    public string RangoTiempo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para estadísticas de productos
/// </summary>
public class ComandaProductoEstadisticasDto
{
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int CantidadPedida { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TiempoPromedioPreparacion { get; set; }
}

/// <summary>
/// DTO para estadísticas de meseros
/// </summary>
public class ComandaMeseroEstadisticasDto
{
    public Guid MeseroId { get; set; }
    public string MeseroNombre { get; set; } = string.Empty;
    public int TotalComandas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TiempoPromedioAtencion { get; set; }
    public int ComandasUrgentes { get; set; }
}

/// <summary>
/// DTO para filtros de comandas
/// </summary>
public class ComandaFiltrosDto
{
    public string? Busqueda { get; set; }
    public string? Estado { get; set; }
    public string? Prioridad { get; set; }
    public string? TipoComanda { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? MeseroId { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool? EsUrgente { get; set; }
    public bool? EsDomicilio { get; set; }
    public bool? EsLenta { get; set; }
    public int? TiempoMinimo { get; set; } // en minutos
    public int? TiempoMaximo { get; set; } // en minutos
    public string? OrdenarPor { get; set; } = "FechaCreacion";
    public string? DireccionOrden { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para cambiar estado de comanda
/// </summary>
public class CambiarEstadoComandaRequest
{
    [Required(ErrorMessage = "El ID de comanda es obligatorio")]
    public Guid ComandaId { get; set; }
    
    [Required(ErrorMessage = "El nuevo estado es obligatorio")]
    public string NuevoEstado { get; set; } = string.Empty;
    
    public string? Observaciones { get; set; }
    public string? NotasCocina { get; set; }
    public string? NotasEntrega { get; set; }
}

/// <summary>
/// DTO para cambiar prioridad de comanda
/// </summary>
public class CambiarPrioridadComandaRequest
{
    [Required(ErrorMessage = "El ID de comanda es obligatorio")]
    public Guid ComandaId { get; set; }
    
    [Required(ErrorMessage = "La nueva prioridad es obligatoria")]
    public string NuevaPrioridad { get; set; } = string.Empty;
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para asignar cocinero a comanda
/// </summary>
public class AsignarCocineroRequest
{
    [Required(ErrorMessage = "El ID de comanda es obligatorio")]
    public Guid ComandaId { get; set; }
    
    [Required(ErrorMessage = "El ID de cocinero es obligatorio")]
    public Guid CocineroId { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para reasignar mesa
/// </summary>
public class ReasignarMesaRequest
{
    [Required(ErrorMessage = "El ID de comanda es obligatorio")]
    public Guid ComandaId { get; set; }
    
    [Required(ErrorMessage = "El ID de nueva mesa es obligatorio")]
    public Guid NuevaMesaId { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para dividir comanda
/// </summary>
public class DividirComandaRequest
{
    [Required(ErrorMessage = "El ID de comanda es obligatorio")]
    public Guid ComandaId { get; set; }
    
    [Required(ErrorMessage = "Los detalles a dividir son obligatorios")]
    public List<Guid> DetallesIds { get; set; } = new();
    
    [Required(ErrorMessage = "La nueva mesa es obligatoria")]
    public Guid NuevaMesaId { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para fusionar comandas
/// </summary>
public class FusionarComandasRequest
{
    [Required(ErrorMessage = "El ID de comanda principal es obligatorio")]
    public Guid ComandaPrincipalId { get; set; }
    
    [Required(ErrorMessage = "Los IDs de comandas a fusionar son obligatorios")]
    public List<Guid> ComandasAFusionar { get; set; } = new();
    
    public string? Observaciones { get; set; }
}
