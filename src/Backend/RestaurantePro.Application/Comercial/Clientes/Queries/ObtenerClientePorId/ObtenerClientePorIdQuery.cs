using MediatR;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Domain.Common;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Query para obtener un cliente específico por su ID
/// Incluye información completa del cliente y datos relacionados
/// </summary>
public class ObtenerClientePorIdQuery : IRequest<Result<ClienteDetalleDto>>
{
    /// <summary>
    /// ID único del cliente a consultar
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Indica si incluir el historial de reservaciones
    /// </summary>
    public bool IncluirReservaciones { get; set; } = false;

    /// <summary>
    /// Indica si incluir el historial de facturas
    /// </summary>
    public bool IncluirFacturas { get; set; } = false;

    /// <summary>
    /// Indica si incluir información de fidelización
    /// </summary>
    public bool IncluirFidelizacion { get; set; } = false;

    /// <summary>
    /// Número máximo de registros de historial a incluir
    /// </summary>
    public int LimiteHistorial { get; set; } = 10;

    /// <summary>
    /// Indica si incluir solo registros activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ObtenerClientePorIdQuery()
    {
    }

    /// <summary>
    /// Constructor con ID del cliente
    /// </summary>
    public ObtenerClientePorIdQuery(Guid clienteId)
    {
        ClienteId = clienteId;
    }

    /// <summary>
    /// Factory method para consulta básica
    /// </summary>
    public static ObtenerClientePorIdQuery Create(Guid clienteId)
    {
        return new ObtenerClientePorIdQuery(clienteId);
    }

    /// <summary>
    /// Factory method para consulta completa con historial
    /// </summary>
    public static ObtenerClientePorIdQuery CreateCompleto(Guid clienteId, int limiteHistorial = 10)
    {
        return new ObtenerClientePorIdQuery
        {
            ClienteId = clienteId,
            IncluirReservaciones = true,
            IncluirFacturas = true,
            IncluirFidelizacion = true,
            LimiteHistorial = limiteHistorial,
            SoloActivos = false
        };
    }

    /// <summary>
    /// Factory method para consulta de fidelización
    /// </summary>
    public static ObtenerClientePorIdQuery CreateParaFidelizacion(Guid clienteId)
    {
        return new ObtenerClientePorIdQuery
        {
            ClienteId = clienteId,
            IncluirFidelizacion = true,
            IncluirReservaciones = true,
            LimiteHistorial = 5
        };
    }
} 