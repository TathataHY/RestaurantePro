using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Domain.Common;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Handler para obtener un cliente por su ID
/// Implementa búsqueda directa con manejo de casos de no encontrado
/// </summary>
public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, Result<ClienteDetalleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerClientePorIdHandler> _logger;

    public ObtenerClientePorIdHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerClientePorIdHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClienteDetalleDto>> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo cliente {ClienteId} con opciones: Reservaciones={IncluirReservaciones}, Facturas={IncluirFacturas}, Fidelización={IncluirFidelizacion}", 
                request.ClienteId, request.IncluirReservaciones, request.IncluirFacturas, request.IncluirFidelizacion);

            // 1. Construir query base
            var query = _context.Clientes.AsQueryable();

            // 2. Aplicar filtros de actividad
            if (request.SoloActivos)
            {
                query = query.Where(c => c.Activo);
            }

            // 3. Incluir datos relacionados según los parámetros
            if (request.IncluirReservaciones)
            {
                query = query.Include(c => c.Reservaciones
                    .OrderByDescending(r => r.FechaHora)
                    .Take(request.LimiteHistorial))
                    .ThenInclude(r => r.Mesa);
            }

            if (request.IncluirFacturas)
            {
                query = query.Include(c => c.Facturas
                    .OrderByDescending(f => f.FechaCreacion)
                    .Take(request.LimiteHistorial));
            }

            if (request.IncluirFidelizacion)
            {
                query = query.Include(c => c.TarjetasFidelizacion
                    .Where(t => t.Activa))
                    .ThenInclude(t => t.HistorialPuntos
                        .OrderByDescending(h => h.FechaMovimiento)
                        .Take(request.LimiteHistorial));
            }

            // 4. Obtener el cliente
            var cliente = await query
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure<ClienteDetalleDto>("El cliente especificado no existe.");
            }

            // 5. Crear el DTO completo
            var clienteDetalle = await CrearClienteDetalleDto(cliente, request);

            _logger.LogInformation("Cliente {ClienteId} obtenido exitosamente con {CantidadReservaciones} reservaciones y {CantidadFacturas} facturas", 
                request.ClienteId, 
                clienteDetalle.Reservaciones?.Count ?? 0,
                clienteDetalle.Facturas?.Count ?? 0);

            return Result.Success(clienteDetalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente {ClienteId}", request.ClienteId);
            return Result.Failure<ClienteDetalleDto>("Error interno al obtener el cliente.");
        }
    }

    private async Task<ClienteDetalleDto> CrearClienteDetalleDto(Cliente cliente, ObtenerClientePorIdQuery request)
    {
        var clienteDetalle = new ClienteDetalleDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FechaNacimiento = cliente.FechaNacimiento,
            Activo = cliente.Activo,
            FechaCreacion = cliente.FechaCreacion,
            UltimaActualizacion = cliente.FechaModificacion,
            
            // Información de segmentación
            Segmento = cliente.Segmento.ToString(),
            PuntosAcumulados = cliente.PuntosAcumulados,
            CantidadVisitas = cliente.CantidadVisitas,
            MontoTotalGastado = cliente.MontoTotalGastado,
            
            // Información de desactivación si aplica
            FechaDesactivacion = cliente.FechaDesactivacion,
            MotivoDesactivacion = cliente.MotivoDesactivacion,
            DesactivadoPor = cliente.DesactivadoPor,
            
            // Estadísticas calculadas
            PromedioGastoPorVisita = cliente.CantidadVisitas > 0 ? cliente.MontoTotalGastado / cliente.CantidadVisitas : 0,
            UltimaVisita = await ObtenerUltimaVisita(cliente.Id),
            ProximaReservacion = await ObtenerProximaReservacion(cliente.Id)
        };

        // 6. Mapear reservaciones si se incluyen
        if (request.IncluirReservaciones && cliente.Reservaciones.Any())
        {
            clienteDetalle.Reservaciones = cliente.Reservaciones
                .Select(r => new ReservacionSummaryDto
                {
                    Id = r.Id,
                    FechaHora = r.FechaHora,
                    NumeroComensales = r.NumeroComensales,
                    Estado = r.Estado.ToString(),
                    MesaNumero = r.Mesa?.Numero,
                    NotasEspeciales = r.NotasEspeciales,
                    MontoEstimado = r.MontoEstimado ?? 0
                })
                .ToList();
        }

        // 7. Mapear facturas si se incluyen
        if (request.IncluirFacturas && cliente.Facturas.Any())
        {
            clienteDetalle.Facturas = cliente.Facturas
                .Select(f => new FacturaSummaryDto
                {
                    Id = f.Id,
                    Numero = f.Numero,
                    FechaCreacion = f.FechaCreacion,
                    MontoTotal = f.MontoTotal,
                    Estado = f.Estado.ToString(),
                    MetodoPago = f.MetodoPago
                })
                .ToList();
        }

        // 8. Mapear información de fidelización si se incluye
        if (request.IncluirFidelizacion && cliente.TarjetasFidelizacion.Any())
        {
            var tarjetaActiva = cliente.TarjetasFidelizacion.FirstOrDefault(t => t.Activa);
            if (tarjetaActiva != null)
            {
                clienteDetalle.InformacionFidelizacion = new FidelizacionDto
                {
                    TarjetaId = tarjetaActiva.Id,
                    Numero = tarjetaActiva.Numero,
                    PuntosActuales = tarjetaActiva.PuntosActuales,
                    PuntosCanjeados = tarjetaActiva.PuntosCanjeados,
                    PuntosPorVencer = await CalcularPuntosPorVencer(tarjetaActiva.Id),
                    ProximoVencimiento = await ObtenerProximoVencimiento(tarjetaActiva.Id),
                    NivelFidelizacion = tarjetaActiva.Nivel.ToString(),
                    FechaProximoNivel = await CalcularFechaProximoNivel(tarjetaActiva.Id),
                    PuntosParaProximoNivel = await CalcularPuntosParaProximoNivel(tarjetaActiva.Id)
                };

                // Historial de puntos reciente
                if (tarjetaActiva.HistorialPuntos.Any())
                {
                    clienteDetalle.InformacionFidelizacion.HistorialReciente = tarjetaActiva.HistorialPuntos
                        .Select(h => new MovimientoPuntosDto
                        {
                            Fecha = h.FechaMovimiento,
                            Tipo = h.TipoMovimiento.ToString(),
                            Puntos = h.Puntos,
                            Motivo = h.Motivo,
                            Referencia = h.Referencia
                        })
                        .ToList();
                }
            }
        }

        return clienteDetalle;
    }

    private async Task<DateTime?> ObtenerUltimaVisita(Guid clienteId)
    {
        return await _context.Reservaciones
            .Where(r => r.ClienteId == clienteId && r.Estado == EstadoReservacion.Completada)
            .OrderByDescending(r => r.FechaHora)
            .Select(r => r.FechaHora)
            .FirstOrDefaultAsync();
    }

    private async Task<DateTime?> ObtenerProximaReservacion(Guid clienteId)
    {
        var fechaActual = DateTime.UtcNow;
        return await _context.Reservaciones
            .Where(r => r.ClienteId == clienteId && 
                       r.FechaHora > fechaActual &&
                       (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente))
            .OrderBy(r => r.FechaHora)
            .Select(r => r.FechaHora)
            .FirstOrDefaultAsync();
    }

    private async Task<int> CalcularPuntosPorVencer(Guid tarjetaId)
    {
        var fechaVencimiento = DateTime.UtcNow.AddMonths(6); // Los puntos vencen en 6 meses
        return await _context.HistorialPuntos
            .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
                       h.FechaVencimiento < fechaVencimiento &&
                       h.FechaVencimiento > DateTime.UtcNow)
            .SumAsync(h => h.Puntos);
    }

    private async Task<DateTime?> ObtenerProximoVencimiento(Guid tarjetaId)
    {
        return await _context.HistorialPuntos
            .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
                       h.FechaVencimiento > DateTime.UtcNow)
            .OrderBy(h => h.FechaVencimiento)
            .Select(h => h.FechaVencimiento)
            .FirstOrDefaultAsync();
    }

    private async Task<DateTime?> CalcularFechaProximoNivel(Guid tarjetaId)
    {
        // Lógica simplificada - en producción esto sería más complejo
        var tarjeta = await _context.TarjetasFidelizacion
            .FirstOrDefaultAsync(t => t.Id == tarjetaId);

        if (tarjeta?.Nivel == NivelFidelizacion.VIP) return null; // Ya está en el nivel máximo

        // Estimar basado en el historial de acumulación
        var promedioMensual = await ObtenerPromedioAcumulacionMensual(tarjetaId);
        if (promedioMensual <= 0) return null;

        var puntosNecesarios = await CalcularPuntosParaProximoNivel(tarjetaId);
        if (puntosNecesarios <= 0) return null;

        var mesesEstimados = (int)Math.Ceiling((double)puntosNecesarios / promedioMensual);
        return DateTime.UtcNow.AddMonths(mesesEstimados);
    }

    private async Task<int> CalcularPuntosParaProximoNivel(Guid tarjetaId)
    {
        var tarjeta = await _context.TarjetasFidelizacion
            .FirstOrDefaultAsync(t => t.Id == tarjetaId);

        if (tarjeta == null) return 0;

        return tarjeta.Nivel switch
        {
            NivelFidelizacion.Bronce => 1000 - tarjeta.PuntosActuales, // Plata a los 1000
            NivelFidelizacion.Plata => 2500 - tarjeta.PuntosActuales,  // Oro a los 2500
            NivelFidelizacion.Oro => 5000 - tarjeta.PuntosActuales,    // VIP a los 5000
            NivelFidelizacion.VIP => 0, // Ya está en el nivel máximo
            _ => 1000 - tarjeta.PuntosActuales
        };
    }

    private async Task<decimal> ObtenerPromedioAcumulacionMensual(Guid tarjetaId)
    {
        var tresMesesAtras = DateTime.UtcNow.AddMonths(-3);
        var puntosUltimosTresMeses = await _context.HistorialPuntos
            .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
                       h.FechaMovimiento >= tresMesesAtras &&
                       h.TipoMovimiento == TipoMovimientoPuntos.Acumulacion)
            .SumAsync(h => h.Puntos);

        return puntosUltimosTresMeses / 3m;
    }
} 