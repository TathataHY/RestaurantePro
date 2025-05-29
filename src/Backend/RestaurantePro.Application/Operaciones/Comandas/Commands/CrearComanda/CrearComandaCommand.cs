using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

/// <summary>
/// Comando para crear una nueva comanda en el restaurante
/// Implementa el inicio del flujo operativo de pedidos
/// </summary>
public class CrearComandaCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID del mesero responsable de la comanda
    /// </summary>
    public Guid MeseroId { get; set; }

    /// <summary>
    /// ID de la mesa donde se toma la comanda (opcional para delivery/takeout)
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// ID del cliente asociado (opcional)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Observaciones iniciales de la comanda
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Lista de productos iniciales para agregar a la comanda
    /// Una comanda puede iniciarse vacía o con productos iniciales
    /// </summary>
    public List<AgregarProductoDto> ProductosIniciales { get; set; } = new();

    /// <summary>
    /// Constructor para facilitar la creación desde controladores
    /// </summary>
    public CrearComandaCommand(Guid meseroId, Guid? mesaId = null, Guid? clienteId = null)
    {
        MeseroId = meseroId;
        MesaId = mesaId;
        ClienteId = clienteId;
    }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public CrearComandaCommand()
    {
    }
} 