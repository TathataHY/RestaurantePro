using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerPlanoMesas;

public class ObtenerPlanoMesasQueryHandler : IRequestHandler<ObtenerPlanoMesasQuery, Result<PlanoMesasDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<ObtenerPlanoMesasQueryHandler> _logger;

    public ObtenerPlanoMesasQueryHandler(
        IMesaRepository mesaRepository,
        ILogger<ObtenerPlanoMesasQueryHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _logger = logger;
    }

    public async Task<Result<PlanoMesasDto>> Handle(ObtenerPlanoMesasQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo plano de mesas con filtros: Estado={Estado}, Ubicacion={Ubicacion}", 
            request.Estado, request.Ubicacion);

        try
        {
            // Obtener todas las mesas
            var mesas = (await _mesaRepository.ObtenerTodasAsync()).ToList();

            // Aplicar filtros en memoria
            if (!string.IsNullOrEmpty(request.Estado))
            {
                mesas = mesas.Where(m => m.Estado.ToString() == request.Estado).ToList();
            }

            if (!string.IsNullOrEmpty(request.Ubicacion))
            {
                mesas = mesas.Where(m => m.Ubicacion == request.Ubicacion).ToList();
            }

            if (!request.IncluirOcupadas)
            {
                mesas = mesas.Where(m => m.Estado != EstadoMesa.Ocupada).ToList();
            }

            if (!request.IncluirReservadas)
            {
                mesas = mesas.Where(m => m.Estado != EstadoMesa.Reservada).ToList();
            }

            // Mapear a DTOs
            var mesasPlano = mesas.Select(m => new MesaPlanoDto
            {
                Id = m.Id,
                Numero = m.Numero,
                Capacidad = m.Capacidad,
                Ubicacion = m.Ubicacion,
                Estado = m.Estado.ToString()
            }).ToList();

            // Calcular estadísticas
            var estadisticas = new EstadisticasPlanoDto
            {
                TotalMesas = mesas.Count,
                MesasDisponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible),
                MesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada),
                MesasReservadas = mesas.Count(m => m.Estado == EstadoMesa.Reservada),
                MesasMantenimiento = mesas.Count(m => m.Estado == EstadoMesa.FueraDeServicio || m.Estado == EstadoMesa.EnLimpieza),
                CapacidadTotal = mesas.Sum(m => m.Capacidad),
                CapacidadDisponible = mesas.Where(m => m.Estado == EstadoMesa.Disponible).Sum(m => m.Capacidad)
            };

            var plano = new PlanoMesasDto
            {
                Mesas = mesasPlano,
                Estadisticas = estadisticas,
                FechaGeneracion = DateTime.Now
            };

            _logger.LogInformation("Plano de mesas generado exitosamente con {CantidadMesas} mesas", mesas.Count);

            return Result.Success(plano);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el plano de mesas");
            return Result.Failure<PlanoMesasDto>(new List<string> { "Error al obtener el plano de mesas" });
        }
    }
} 