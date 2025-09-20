using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;

namespace RestaurantePro.Mobile.Core.Services
{
    /// <summary>
    /// Interfaz para el servicio de preparaciones diarias en mobile
    /// </summary>
    public interface IDailyPreparationsService
    {
        /// <summary>
        /// Obtiene todas las preparaciones diarias (para gestión)
        /// </summary>
        Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasAsync();

        /// <summary>
        /// Obtiene el menú del día (solo preparaciones disponibles, de hoy, no vencidas)
        /// </summary>
        Task<Result<List<PreparacionDiariaDto>>> GetMenuDelDiaAsync(DateTime? fecha = null, int limite = 10);

        /// <summary>
        /// Obtiene una preparación diaria específica por ID
        /// </summary>
        Task<Result<PreparacionDiariaDto>> GetPreparacionDiariaAsync(Guid id);

        /// <summary>
        /// Crea una nueva preparación diaria
        /// </summary>
        Task<Result<PreparacionDiariaDto>> CrearPreparacionDiariaAsync(CrearPreparacionDiariaCommand command);

        /// <summary>
        /// Actualiza una preparación diaria existente
        /// </summary>
        Task<Result<PreparacionDiariaDto>> ActualizarPreparacionDiariaAsync(Guid id, ActualizarPreparacionDiariaCommand command);

        /// <summary>
        /// Elimina una preparación diaria
        /// </summary>
        Task<Result> EliminarPreparacionDiariaAsync(Guid id);

        /// <summary>
        /// Consume una cantidad específica de una preparación diaria
        /// </summary>
        Task<Result<PreparacionDiariaDto>> ConsumirPreparacionDiariaAsync(Guid id, int cantidad, string? observaciones = null);

        /// <summary>
        /// Marca una preparación diaria como disponible
        /// </summary>
        Task<Result<PreparacionDiariaDto>> MarcarComoDisponibleAsync(Guid id);

        /// <summary>
        /// Obtiene preparaciones diarias por estado
        /// </summary>
        Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasPorEstadoAsync(string estado);

        /// <summary>
        /// Obtiene preparaciones diarias por producto
        /// </summary>
        Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasPorProductoAsync(Guid productoId);

        /// <summary>
        /// Obtiene estadísticas de preparaciones diarias
        /// </summary>
        Task<Result<EstadisticasPreparacionesDiariasDto>> GetEstadisticasAsync();
    }

    /// <summary>
    /// Comando para crear preparación diaria
    /// </summary>
    public class CrearPreparacionDiariaCommand
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public Guid ChefId { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Comando para actualizar preparación diaria
    /// </summary>
    public class ActualizarPreparacionDiariaCommand
    {
        public Guid ProductoId { get; set; }
        public int CantidadPreparada { get; set; }
        public int CantidadDisponible { get; set; }
        public Guid ChefId { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? Observaciones { get; set; }
    }
} 