namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO para crear una personalización de item de comanda
/// </summary>
public class CrearPersonalizacionItemDto
{
    /// <summary>
    /// Nombre de la personalización
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de personalización
    /// </summary>
    public string TipoPersonalizacion { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la personalización
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Costo adicional de la personalización
    /// </summary>
    public decimal CostoAdicional { get; set; }

    /// <summary>
    /// ID del ingrediente relacionado (si aplica)
    /// </summary>
    public Guid? IngredienteId { get; set; }

    /// <summary>
    /// Cantidad del ingrediente personalizado
    /// </summary>
    public decimal CantidadIngrediente { get; set; }

    /// <summary>
    /// Indica si es una adición o una eliminación
    /// </summary>
    public bool EsAdicion { get; set; } = true;

    /// <summary>
    /// Instrucciones especiales para la cocina
    /// </summary>
    public string? InstruccionesEspeciales { get; set; }

    /// <summary>
    /// Prioridad de la personalización
    /// </summary>
    public int Prioridad { get; set; } = 1;

    /// <summary>
    /// Indica si afecta el tiempo de preparación
    /// </summary>
    public bool AfectaTiempoPreparacion { get; set; }

    /// <summary>
    /// Tiempo adicional en minutos que agrega la personalización
    /// </summary>
    public int TiempoAdicionalMinutos { get; set; }
} 