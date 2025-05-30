using MediatR;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha;

/// <summary>
/// Query para obtener reservaciones por fecha con filtros avanzados
/// </summary>
public class ObtenerReservacionesPorFechaQuery : IRequest<Result<PaginatedList<ReservacionDto>>>
{
    /// <summary>
    /// Fecha de las reservaciones a consultar
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Filtrar por estado específico (opcional)
    /// </summary>
    public Domain.Operaciones.Reservaciones.Enums.EstadoReservacion? Estado { get; set; }

    /// <summary>
    /// Filtrar por mesa específica (opcional)
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// Incluir solo reservaciones futuras
    /// </summary>
    public bool SoloFuturas { get; set; } = false;

    /// <summary>
    /// Incluir solo reservaciones activas (Pendiente, Confirmada, EnProgreso)
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
    /// Ordenar por hora de reservación
    /// </summary>
    public bool OrdenarPorHora { get; set; } = true;

    /// <summary>
    /// Incluir información detallada de cliente y mesa
    /// </summary>
    public bool IncluirDetalles { get; set; } = true;

    /// <summary>
    /// Factory method para consulta básica del día
    /// </summary>
    public static ObtenerReservacionesPorFechaQuery DelDia(DateTime fecha, int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerReservacionesPorFechaQuery
        {
            Fecha = fecha.Date,
            SoloActivas = true,
            OrdenarPorHora = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    /// <summary>
    /// Factory method para consulta por estado específico
    /// </summary>
    public static ObtenerReservacionesPorFechaQuery PorEstado(DateTime fecha, Domain.Operaciones.Reservaciones.Enums.EstadoReservacion estado, int pagina = 1)
    {
        return new ObtenerReservacionesPorFechaQuery
        {
            Fecha = fecha.Date,
            Estado = estado,
            SoloActivas = false,
            OrdenarPorHora = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = 20
        };
    }

    /// <summary>
    /// Factory method para consulta de mesa específica
    /// </summary>
    public static ObtenerReservacionesPorFechaQuery PorMesa(DateTime fecha, Guid mesaId, int pagina = 1)
    {
        return new ObtenerReservacionesPorFechaQuery
        {
            Fecha = fecha.Date,
            MesaId = mesaId,
            SoloActivas = true,
            OrdenarPorHora = true,
            IncluirDetalles = false,
            Pagina = pagina,
            TamanoPagina = 50
        };
    }

    /// <summary>
    /// Factory method para consulta de reservaciones futuras
    /// </summary>
    public static ObtenerReservacionesPorFechaQuery Futuras(DateTime fechaDesde, int pagina = 1)
    {
        return new ObtenerReservacionesPorFechaQuery
        {
            Fecha = fechaDesde.Date,
            SoloFuturas = true,
            SoloActivas = true,
            OrdenarPorHora = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = 30
        };
    }
} 