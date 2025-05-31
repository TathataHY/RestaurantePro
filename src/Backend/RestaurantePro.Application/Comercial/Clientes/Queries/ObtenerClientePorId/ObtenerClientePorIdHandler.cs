namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Handler para obtener un cliente por su ID
/// Implementa búsqueda directa con manejo de casos de no encontrado
/// </summary>
public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, Result<ClienteDto>>
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

    public async Task<Result<ClienteDto>> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Consultando cliente por ID: {ClienteId}", request.ClienteId);

            // 1. Obtener el cliente con las relaciones necesarias
            var cliente = await _context.Clientes
                // TODO: Incluir Contactos cuando esté disponible en Cliente
                // .Include(c => c.Contactos)
                // TODO: Incluir Reservaciones cuando esté disponible en Cliente
                // .Include(c => c.Reservaciones)
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure<ClienteDto>("El cliente especificado no existe.");
            }

            // 2. Crear el DTO completo
            var clienteDto = await CrearClienteDto(cliente, request);

            _logger.LogInformation("Cliente {ClienteId} consultado exitosamente", request.ClienteId);

            return Result.Success(clienteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar cliente {ClienteId}", request.ClienteId);
            return Result.Failure<ClienteDto>("Error interno al obtener el cliente.");
        }
    }

    private async Task<ClienteDto> CrearClienteDto(Cliente cliente, ObtenerClientePorIdQuery request)
    {
        var clienteDto = new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre.ToString(),
            Apellido = "",
            Email = cliente.Email.ToString(),
            Telefono = cliente.Telefono.ToString(),
            // TODO: Implementar cuando Cliente tenga Direccion
            Direccion = "", // cliente.Direccion?.ToString(),
            // TODO: ClienteDto y Cliente necesitan propiedad Estado
            // Estado = cliente.Estado,
            // TODO: ClienteDto necesita SegmentoCliente, no string
            // Segmento = "Regular",
            // TODO: Implementar cuando Cliente tenga SegmentoCliente
            // Segmento = SegmentoCliente.Regular,
            // TODO: Implementar cuando Cliente tenga Contactos y ContactoClienteDto exista
            // Contactos = cliente.Contactos.Select(c => new ContactoClienteDto
            // {
            //     Id = c.Id,
            //     TipoContacto = c.TipoContacto,
            //     Valor = c.Valor,
            //     EsPrincipal = c.EsPrincipal
            // }).ToList(),
        };

        // TODO: Implementar cuando Cliente tenga Reservaciones y ReservacionSummaryDto exista
        // 6. Mapear reservaciones si se incluyen
        // if (request.IncluirReservaciones && cliente.Reservaciones.Any())
        // {
        //     clienteDto.Reservaciones = cliente.Reservaciones
        //         .Select(r => new ReservacionSummaryDto
        //         {
        //             Id = r.Id,
        //             FechaHora = r.FechaHora,
        //             NumeroComensales = r.NumeroComensales,
        //             Estado = r.Estado.ToString(),
        //             MesaNumero = r.Mesa?.Numero,
        //             NotasEspeciales = r.NotasEspeciales,
        //             MontoEstimado = r.MontoEstimado ?? 0
        //         })
        //         .ToList();
        // }

        // TODO: Implementar cuando Cliente tenga Facturas y FacturaSummaryDto exista
        // 7. Mapear facturas si se incluyen
        // if (request.IncluirFacturas && cliente.Facturas.Any())
        // {
        //     clienteDto.Facturas = cliente.Facturas
        //         .Select(f => new FacturaSummaryDto
        //         {
        //             Id = f.Id,
        //             Numero = f.Numero,
        //             FechaCreacion = f.FechaCreacion,
        //             MontoTotal = f.MontoTotal,
        //             Estado = f.Estado.ToString(),
        //             MetodoPago = f.MetodoPago
        //         })
        //         .ToList();
        // }

        // TODO: Implementar cuando Cliente tenga TarjetasFidelizacion y FidelizacionDto exista
        // 8. Mapear información de fidelización si se incluye
        // if (request.IncluirFidelizacion && cliente.TarjetasFidelizacion.Any())
        // {
        //     var tarjetaActiva = cliente.TarjetasFidelizacion.FirstOrDefault(t => t.Activa);
        //     if (tarjetaActiva != null)
        //     {
        //         clienteDto.InformacionFidelizacion = new FidelizacionDto
        //         {
        //             TarjetaId = tarjetaActiva.Id,
        //             Numero = tarjetaActiva.Numero,
        //             PuntosActuales = tarjetaActiva.PuntosActuales,
        //             PuntosCanjeados = tarjetaActiva.PuntosCanjeados,
        //             PuntosPorVencer = await CalcularPuntosPorVencer(tarjetaActiva.Id),
        //             ProximoVencimiento = await ObtenerProximoVencimiento(tarjetaActiva.Id),
        //             NivelFidelizacion = tarjetaActiva.Nivel.ToString(),
        //             FechaProximoNivel = await CalcularFechaProximoNivel(tarjetaActiva.Id),
        //             PuntosParaProximoNivel = await CalcularPuntosParaProximoNivel(tarjetaActiva.Id)
        //         };
        //
        //         // Historial de puntos reciente
        //         if (tarjetaActiva.HistorialPuntos.Any())
        //         {
        //             clienteDto.InformacionFidelizacion.HistorialReciente = tarjetaActiva.HistorialPuntos
        //                 .Select(h => new MovimientoPuntosDto
        //                 {
        //                     Fecha = h.FechaMovimiento,
        //                     Tipo = h.TipoMovimiento.ToString(),
        //                     Puntos = h.Puntos,
        //                     Motivo = h.Motivo,
        //                     Referencia = h.Referencia
        //                 })
        //                 .ToList();
        //         }
        //     }
        // }

        return clienteDto;
    }

    private async Task<DateTime?> ObtenerUltimaVisita(Guid clienteId)
    {
        // TODO: Descomentar cuando tengamos tabla Reservaciones
        // return await _context.Reservaciones
        //     .Where(r => r.ClienteId == clienteId && r.Estado == EstadoReservacion.Completada)
        //     .OrderByDescending(r => r.FechaHora)
        //     .Select(r => r.FechaHora)
        //     .FirstOrDefaultAsync();
        return await Task.FromResult<DateTime?>(null); // Temporal
    }

    private async Task<DateTime?> ObtenerProximaReservacion(Guid clienteId)
    {
        // TODO: Descomentar cuando tengamos tabla Reservaciones
        // var fechaActual = DateTime.UtcNow;
        // return await _context.Reservaciones
        //     .Where(r => r.ClienteId == clienteId && 
        //                r.FechaHora > fechaActual &&
        //                (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente))
        //     .OrderBy(r => r.FechaHora)
        //     .Select(r => r.FechaHora)
        //     .FirstOrDefaultAsync();
        return await Task.FromResult<DateTime?>(null); // Temporal
    }

    private async Task<int> CalcularPuntosPorVencer(Guid tarjetaId)
    {
        // TODO: Descomentar cuando tengamos tabla HistorialPuntos
        // var fechaVencimiento = DateTime.UtcNow.AddMonths(6); // Los puntos vencen en 6 meses
        // return await _context.HistorialPuntos
        //     .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
        //                h.FechaVencimiento < fechaVencimiento &&
        //                h.FechaVencimiento > DateTime.UtcNow)
        //     .SumAsync(h => h.Puntos);
        return await Task.FromResult(0); // Temporal
    }

    private async Task<DateTime?> ObtenerProximoVencimiento(Guid tarjetaId)
    {
        // TODO: Descomentar cuando tengamos tabla HistorialPuntos
        // return await _context.HistorialPuntos
        //     .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
        //                h.FechaVencimiento > DateTime.UtcNow)
        //     .OrderBy(h => h.FechaVencimiento)
        //     .Select(h => h.FechaVencimiento)
        //     .FirstOrDefaultAsync();
        return await Task.FromResult<DateTime?>(null); // Temporal
    }

    private async Task<DateTime?> CalcularFechaProximoNivel(Guid tarjetaId)
    {
        // TODO: Descomentar cuando tengamos tabla TarjetasFidelizacion
        // Lógica simplificada - en producción esto sería más complejo
        // var tarjeta = await _context.TarjetasFidelizacion
        //     .FirstOrDefaultAsync(t => t.Id == tarjetaId);
        //
        // if (tarjeta?.Nivel == NivelFidelizacion.VIP) return null; // Ya está en el nivel máximo
        //
        // // Estimar basado en el historial de acumulación
        // var promedioMensual = await ObtenerPromedioAcumulacionMensual(tarjetaId);
        // if (promedioMensual <= 0) return null;
        //
        // var puntosNecesarios = await CalcularPuntosParaProximoNivel(tarjetaId);
        // if (puntosNecesarios <= 0) return null;
        //
        // var mesesEstimados = (int)Math.Ceiling((double)puntosNecesarios / promedioMensual);
        // return DateTime.UtcNow.AddMonths(mesesEstimados);
        return await Task.FromResult<DateTime?>(null); // Temporal
    }

    private async Task<int> CalcularPuntosParaProximoNivel(Guid tarjetaId)
    {
        // TODO: Descomentar cuando tengamos tabla TarjetasFidelizacion
        // var tarjeta = await _context.TarjetasFidelizacion
        //     .FirstOrDefaultAsync(t => t.Id == tarjetaId);
        //
        // if (tarjeta == null) return 0;
        //
        // return tarjeta.Nivel switch
        // {
        //     NivelFidelizacion.Bronce => 1000 - tarjeta.PuntosActuales, // Plata a los 1000
        //     NivelFidelizacion.Plata => 2500 - tarjeta.PuntosActuales,  // Oro a los 2500
        //     NivelFidelizacion.Oro => 5000 - tarjeta.PuntosActuales,    // VIP a los 5000
        //     NivelFidelizacion.VIP => 0, // Ya está en el nivel máximo
        //     _ => 1000 - tarjeta.PuntosActuales
        // };
        return await Task.FromResult(0); // Temporal
    }

    private async Task<decimal> ObtenerPromedioAcumulacionMensual(Guid tarjetaId)
    {
        // TODO: Descomentar cuando tengamos tabla HistorialPuntos
        // var tresMesesAtras = DateTime.UtcNow.AddMonths(-3);
        // var puntosUltimosTresMeses = await _context.HistorialPuntos
        //     .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
        //                h.FechaMovimiento >= tresMesesAtras &&
        //                h.TipoMovimiento == TipoMovimientoPuntos.Acumulacion)
        //     .SumAsync(h => h.Puntos);
        //
        // return puntosUltimosTresMeses / 3m;
        return await Task.FromResult(0m); // Temporal
    }
} 