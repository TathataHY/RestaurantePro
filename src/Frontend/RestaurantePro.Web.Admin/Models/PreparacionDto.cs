using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models
{
    /// <summary>
    /// DTO para gestión de preparaciones de cocina
    /// </summary>
    public class PreparacionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de la comanda es requerido")]
        public int ComandaId { get; set; }

        [Required(ErrorMessage = "El ID del producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [StringLength(200, ErrorMessage = "El nombre del producto no puede exceder 200 caracteres")]
        public string ProductoNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El estado de preparación es requerido")]
        public EstadoPreparacion Estado { get; set; }

        [Required(ErrorMessage = "La prioridad es requerida")]
        public PrioridadPreparacion Prioridad { get; set; }

        public int? CocineroId { get; set; }
        public string? CocineroNombre { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El tiempo estimado debe ser mayor o igual a 0")]
        public int TiempoEstimadoMinutos { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El tiempo real debe ser mayor o igual a 0")]
        public int? TiempoRealMinutos { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [StringLength(500, ErrorMessage = "Las notas del cocinero no pueden exceder 500 caracteres")]
        public string? NotasCocinero { get; set; }

        // Información de la comanda
        public string? ComandaNumero { get; set; }
        public int? MesaNumero { get; set; }
        public string? ClienteNombre { get; set; }

        // Información del producto
        public string? CategoriaNombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Tiempos calculados
        public int? TiempoTranscurridoMinutos { get; set; }
        public int? TiempoRestanteMinutos { get; set; }
        public bool EstaAtrasada { get; set; }
    }

    /// <summary>
    /// DTO para filtros de preparaciones
    /// </summary>
    public class PreparacionFiltrosDto
    {
        public EstadoPreparacion? Estado { get; set; }
        public PrioridadPreparacion? Prioridad { get; set; }
        public int? CocineroId { get; set; }
        public int? ProductoId { get; set; }
        public int? ComandaId { get; set; }
        public int? MesaNumero { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool? EstaAtrasada { get; set; }
        public string? Busqueda { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de preparaciones
    /// </summary>
    public class PreparacionEstadisticasDto
    {
        public int PreparacionesHoy { get; set; }
        public int PreparacionesPendientes { get; set; }
        public int PreparacionesEnProceso { get; set; }
        public int PreparacionesCompletadas { get; set; }
        public int PreparacionesAtrasadas { get; set; }
        public double TiempoPromedioPreparacion { get; set; }
        public int PreparacionesPorCocinero { get; set; }
        public double EficienciaCocina { get; set; }
    }

    /// <summary>
    /// DTO para lista paginada de preparaciones
    /// </summary>
    public class PaginatedList<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO para cola de preparaciones
    /// </summary>
    public class ColaPreparacionesDto
    {
        public List<PreparacionDto> Pendientes { get; set; } = new();
        public List<PreparacionDto> EnProceso { get; set; } = new();
        public List<PreparacionDto> Completadas { get; set; } = new();
        public List<PreparacionDto> Atrasadas { get; set; } = new();
        public int TotalPendientes { get; set; }
        public int TotalEnProceso { get; set; }
        public int TotalCompletadas { get; set; }
        public int TotalAtrasadas { get; set; }
    }

    /// <summary>
    /// DTO para asignar cocinero a preparación
    /// </summary>
    public class AsignarCocineroRequest
    {
        [Required(ErrorMessage = "El ID de la preparación es requerido")]
        public int PreparacionId { get; set; }

        [Required(ErrorMessage = "El ID del cocinero es requerido")]
        public int CocineroId { get; set; }
    }

    /// <summary>
    /// DTO para actualizar estado de preparación
    /// </summary>
    public class ActualizarEstadoPreparacionRequest
    {
        [Required(ErrorMessage = "El ID de la preparación es requerido")]
        public int PreparacionId { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public EstadoPreparacion Estado { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO para actualizar tiempo de preparación
    /// </summary>
    public class ActualizarTiempoPreparacionRequest
    {
        [Required(ErrorMessage = "El ID de la preparación es requerido")]
        public int PreparacionId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El tiempo estimado debe ser mayor o igual a 0")]
        public int? TiempoEstimadoMinutos { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El tiempo real debe ser mayor o igual a 0")]
        public int? TiempoRealMinutos { get; set; }
    }

    /// <summary>
    /// DTO para detalles de preparación
    /// </summary>
    public class PreparacionDetalleDto : PreparacionDto
    {
        public List<IngredientePreparacionDto> Ingredientes { get; set; } = new();
        public List<PreparacionHistorialDto> Historial { get; set; } = new();
        public string? Receta { get; set; }
        public List<string> Instrucciones { get; set; } = new();
    }

    /// <summary>
    /// DTO para ingredientes de preparación
    /// </summary>
    public class IngredientePreparacionDto
    {
        public int IngredienteId { get; set; }
        public string IngredienteNombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public bool Disponible { get; set; }
    }

    /// <summary>
    /// DTO para historial de preparación
    /// </summary>
    public class PreparacionHistorialDto
    {
        public DateTime Fecha { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Estados de preparación
    /// </summary>
    public enum EstadoPreparacion
    {
        Pendiente = 1,
        EnProceso = 2,
        Lista = 3,
        Entregada = 4,
        Cancelada = 5
    }

    /// <summary>
    /// Prioridades de preparación
    /// </summary>
    public enum PrioridadPreparacion
    {
        Baja = 1,
        Normal = 2,
        Alta = 3,
        Urgente = 4
    }
}
