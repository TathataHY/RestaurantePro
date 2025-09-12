using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Request para crear una nueva preparación
/// </summary>
public class CrearPreparacionRequest
{
    [Required(ErrorMessage = "El ID de la comanda es obligatorio")]
    public Guid ComandaId { get; set; }

    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public Guid ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria")]
    public PrioridadPreparacion Prioridad { get; set; } = PrioridadPreparacion.Normal;

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo estimado debe ser mayor o igual a 0")]
    public int TiempoEstimadoMinutos { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    [StringLength(500, ErrorMessage = "Las notas del cocinero no pueden exceder 500 caracteres")]
    public string? NotasCocinero { get; set; }

    public bool RequiereConfirmacionChef { get; set; } = false;
}

/// <summary>
/// Request para actualizar una preparación existente
/// </summary>
public class ActualizarPreparacionRequest
{
    [Required(ErrorMessage = "El ID de la preparación es obligatorio")]
    public Guid Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int? Cantidad { get; set; }

    public PrioridadPreparacion? Prioridad { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo estimado debe ser mayor o igual a 0")]
    public int? TiempoEstimadoMinutos { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo real debe ser mayor o igual a 0")]
    public int? TiempoRealMinutos { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    [StringLength(500, ErrorMessage = "Las notas del cocinero no pueden exceder 500 caracteres")]
    public string? NotasCocinero { get; set; }

    public bool? RequiereConfirmacionChef { get; set; }
}

/// <summary>
/// Request para cambiar el estado de una preparación
/// </summary>
public class CambiarEstadoPreparacionRequest
{
    [Required(ErrorMessage = "El ID de la preparación es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    public EstadoPreparacion Estado { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notas { get; set; }
}

/// <summary>
/// Request para iniciar una preparación
/// </summary>
public class IniciarPreparacionRequest
{
    [Required(ErrorMessage = "El ID de la preparación es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El ID del cocinero es obligatorio")]
    public Guid CocineroId { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para completar una preparación
/// </summary>
public class CompletarPreparacionRequest
{
    [Required(ErrorMessage = "El ID de la preparación es obligatorio")]
    public Guid Id { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo real debe ser mayor o igual a 0")]
    public int? TiempoRealMinutos { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para cancelar una preparación
/// </summary>
public class CancelarPreparacionRequest
{
    [Required(ErrorMessage = "El ID de la preparación es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El motivo de cancelación es obligatorio")]
    [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    public string MotivoCancelacion { get; set; } = string.Empty;

    public Guid? UsuarioId { get; set; }
}

