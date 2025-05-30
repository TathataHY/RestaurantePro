using MediatR;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesCliente;

/// <summary>
/// Query para obtener las reservaciones de un cliente específico
/// </summary>
public class ObtenerReservacionesClienteQuery : IRequest<Result<PaginatedList<ReservacionDto>>>
{
    /// <summary>
    /// ID del cliente cuyas reservaciones se desean consultar
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Filtrar por estado específico (opcional)
    /// </summary>
    public Domain.Operaciones.Reservaciones.Enums.EstadoReservacion? Estado { get; set; }

    /// <summary>
    /// Fecha desde la cual buscar reservaciones (opcional)
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta la cual buscar reservaciones (opcional)
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Incluir solo reservaciones futuras
    /// </summary>
    public bool SoloFuturas { get; set; } = false;

    /// <summary>
    /// Incluir solo reservaciones activas (Pendiente, Confirmada, EnProgreso)
    /// </summary>
    public bool SoloActivas { get; set; } = false;

    /// <summary>
    /// Incluir solo el historial de reservaciones (pasadas)
    /// </summary>
    public bool SoloHistorial { get; set; } = false;

    /// <summary>
    /// Página para paginación
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    public int TamanoPagina { get; set; } = 20;

    /// <summary>
    /// Ordenar por fecha (más recientes primero)
    /// </summary>
    public bool OrdenarPorFecha { get; set; } = true;

    /// <summary>
    /// Incluir información detallada de mesa y servicios
    /// </summary>
    public bool IncluirDetalles { get; set; } = true;

    /// <summary>
    /// Factory method para obtener todas las reservaciones del cliente
    /// </summary>
    public static ObtenerReservacionesClienteQuery Todas(Guid clienteId, int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerReservacionesClienteQuery
        {
            ClienteId = clienteId,
            SoloActivas = false,
            SoloFuturas = false,
            SoloHistorial = false,
            OrdenarPorFecha = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    /// <summary>
    /// Factory method para obtener solo reservaciones futuras del cliente
    /// </summary>
    public static ObtenerReservacionesClienteQuery Futuras(Guid clienteId, int pagina = 1)
    {
        return new ObtenerReservacionesClienteQuery
        {
            ClienteId = clienteId,
            SoloFuturas = true,
            SoloActivas = true,
            OrdenarPorFecha = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = 10
        };
    }

    /// <summary>
    /// Factory method para obtener el historial de reservaciones del cliente
    /// </summary>
    public static ObtenerReservacionesClienteQuery Historial(Guid clienteId, int pagina = 1, int tamanoPagina = 20)
    {
        return new ObtenerReservacionesClienteQuery
        {
            ClienteId = clienteId,
            SoloHistorial = true,
            OrdenarPorFecha = true,
            IncluirDetalles = false,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    /// <summary>
    /// Factory method para obtener reservaciones por estado específico
    /// </summary>
    public static ObtenerReservacionesClienteQuery PorEstado(Guid clienteId, Domain.Operaciones.Reservaciones.Enums.EstadoReservacion estado, int pagina = 1)
    {
        return new ObtenerReservacionesClienteQuery
        {
            ClienteId = clienteId,
            Estado = estado,
            OrdenarPorFecha = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = 15
        };
    }

    /// <summary>
    /// Factory method para obtener reservaciones en un rango de fechas
    /// </summary>
    public static ObtenerReservacionesClienteQuery PorRangoFechas(Guid clienteId, DateTime fechaDesde, DateTime fechaHasta, int pagina = 1)
    {
        return new ObtenerReservacionesClienteQuery
        {
            ClienteId = clienteId,
            FechaDesde = fechaDesde.Date,
            FechaHasta = fechaHasta.Date,
            OrdenarPorFecha = true,
            IncluirDetalles = true,
            Pagina = pagina,
            TamanoPagina = 25
        };
    }
} 