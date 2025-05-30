using MediatR;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;

/// <summary>
/// Query para obtener mesas disponibles con filtros opcionales
/// </summary>
public class ObtenerMesasDisponiblesQuery : IRequest<Result<PaginatedList<MesaDto>>>
{
    /// <summary>
    /// Capacidad mínima requerida
    /// </summary>
    public int? CapacidadMinima { get; set; }

    /// <summary>
    /// Zona específica (Interior, Terraza, VIP, etc.)
    /// </summary>
    public string? Zona { get; set; }

    /// <summary>
    /// Incluir solo mesas activas
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Página para paginación
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    public int TamanoPagina { get; set; } = 20;

    /// <summary>
    /// Ordenar por número de mesa
    /// </summary>
    public bool OrdenarPorNumero { get; set; } = true;

    /// <summary>
    /// Factory method para consulta básica
    /// </summary>
    public static ObtenerMesasDisponiblesQuery Basica(int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerMesasDisponiblesQuery
        {
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            SoloActivas = true,
            OrdenarPorNumero = true
        };
    }

    /// <summary>
    /// Factory method para consulta con capacidad específica
    /// </summary>
    public static ObtenerMesasDisponiblesQuery ConCapacidad(int capacidadMinima, int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = capacidadMinima,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            SoloActivas = true,
            OrdenarPorNumero = true
        };
    }

    /// <summary>
    /// Factory method para consulta por zona
    /// </summary>
    public static ObtenerMesasDisponiblesQuery PorZona(string zona, int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerMesasDisponiblesQuery
        {
            Zona = zona,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            SoloActivas = true,
            OrdenarPorNumero = true
        };
    }
} 