using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Query para obtener un cliente específico por su ID
/// Retorna el DTO completo con toda la información del cliente
/// </summary>
public class ObtenerClientePorIdQuery : IRequest<Result<ClienteDto>>
{
    /// <summary>
    /// Identificador único del cliente a buscar
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public ObtenerClientePorIdQuery(Guid clienteId)
    {
        ClienteId = clienteId;
    }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerClientePorIdQuery()
    {
    }
} 