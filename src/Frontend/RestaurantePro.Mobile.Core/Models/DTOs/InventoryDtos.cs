namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para ingrediente
/// </summary>
public class IngredienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string UnidadMedida { get; set; } = string.Empty; // kg, litros, unidades, etc.
    public decimal PrecioUnitario { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Disponible { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string Proveedor { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resumen de ingrediente
/// </summary>
public class IngredienteSummaryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}

/// <summary>
/// DTO para estadísticas de ingredientes
/// </summary>
public class EstadisticasIngredientesDto
{
    public int TotalIngredientes { get; set; }
    public int IngredientesDisponibles { get; set; }
    public int IngredientesBajoStock { get; set; }
    public int IngredientesAgotados { get; set; }
    public decimal ValorTotalInventario { get; set; }
    public List<IngredienteDto> TopIngredientesBajoStock { get; set; } = new();
}

/// <summary>
/// DTO para preparación
/// </summary>
public class PreparacionDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int TiempoPreparacionMinutos { get; set; }
    public bool Disponible { get; set; }
    public List<IngredientePreparacionDto> Ingredientes { get; set; } = new();
    public string InstruccionesPreparacion { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO para ingrediente de preparación
/// </summary>
public class IngredientePreparacionDto
{
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
}

/// <summary>
/// DTO para respuesta paginada de preparaciones
/// </summary>
public class PreparacionesPaginadasDto
{
    public List<PreparacionDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// DTO para estadísticas de preparaciones
/// </summary>
public class EstadisticasPreparacionesDto
{
    public int TotalPreparaciones { get; set; }
    public int PreparacionesDisponibles { get; set; }
    public int PreparacionesNoDisponibles { get; set; }
    public decimal ValorPromedioPreparacion { get; set; }
    public List<PreparacionDto> TopPreparaciones { get; set; } = new();
}

/// <summary>
/// DTO para reservación
/// </summary>
public class ReservacionDto
{
    public Guid Id { get; set; }
    public Guid? ClienteId { get; set; } // ID del cliente registrado (opcional)
    public string NombreCliente { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaReservacion { get; set; }
    public TimeSpan HoraReservacion { get; set; }
    
    /// <summary>
    /// Fecha y hora completa de la reservación (compatibilidad con backend)
    /// </summary>
    public DateTime FechaHoraReservacion 
    { 
        get => FechaReservacion.Add(HoraReservacion);
        set 
        { 
            FechaReservacion = value.Date;
            HoraReservacion = value.TimeOfDay;
        }
    }
    
    public int NumeroPersonas { get; set; }
    public string Estado { get; set; } = string.Empty; // "Confirmada", "Pendiente", "Cancelada", "Completada"
    public string? Comentarios { get; set; }
    public string? MesaAsignada { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// DTO para estadísticas de reservaciones
/// </summary>
public class EstadisticasReservacionesDto
{
    public int TotalReservaciones { get; set; }
    public int ReservacionesConfirmadas { get; set; }
    public int ReservacionesPendientes { get; set; }
    public int ReservacionesCanceladas { get; set; }
    public int ReservacionesCompletadas { get; set; }
    public decimal TasaOcupacionPromedio { get; set; }
    public List<ReservacionDto> ReservacionesHoy { get; set; } = new();
    public List<ReservacionDto> ReservacionesProximas { get; set; } = new();
}

/// <summary>
/// DTO para filtro de ingredientes
/// </summary>
public class FiltroIngredientesDto
{
    public string? Busqueda { get; set; }
    public string? SearchTerm { get; set; }
    public string? Categoria { get; set; }
    public bool? SoloDisponibles { get; set; }
    public bool? SoloBajoStock { get; set; }
    public bool? SoloActivos { get; set; }
    public string? Proveedor { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para filtro de preparaciones
/// </summary>
public class FiltroPreparacionesDto
{
    public string? Busqueda { get; set; }
    public string? SearchTerm { get; set; }
    public string? Categoria { get; set; }
    public string? Estado { get; set; }
    public bool? SoloDisponibles { get; set; }
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public int? TiempoPreparacionMaximo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para reporte de valoración de ingredientes
/// </summary>
public class ReporteValoracionDto
{
    public Guid Id { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public decimal ValorActual { get; set; }
    public decimal ValorPromedio { get; set; }
    public decimal VariacionPorcentual { get; set; }
    public DateTime FechaReporte { get; set; }
    public string Tendencia { get; set; } = string.Empty; // "Subiendo", "Bajando", "Estable"
} 

/// <summary>
/// DTO para iniciar preparación
/// </summary>
public class IniciarPreparacionDto
{
    public Guid PreparacionId { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaInicio { get; set; }
}

/// <summary>
/// DTO para cancelar preparación
/// </summary>
public class CancelarPreparacionDto
{
    public Guid PreparacionId { get; set; }
    public string MotivoCancelacion { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
} 

public class MovimientoInventarioDto
{
    public Guid Id { get; set; }
    public Guid IngredienteId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // Entrada, Salida, Ajuste
    public decimal Cantidad { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;
    public decimal StockAnterior { get; set; }
    public decimal StockPosterior { get; set; }
} 