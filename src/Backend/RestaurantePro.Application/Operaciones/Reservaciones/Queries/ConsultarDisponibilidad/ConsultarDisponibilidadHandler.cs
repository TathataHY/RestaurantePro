namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ConsultarDisponibilidad;

public class ConsultarDisponibilidadHandler : IRequestHandler<ConsultarDisponibilidadQuery, Result<DisponibilidadDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ConsultarDisponibilidadHandler> _logger;

    public ConsultarDisponibilidadHandler(
        IApplicationDbContext context,
        ILogger<ConsultarDisponibilidadHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<DisponibilidadDto>> Handle(ConsultarDisponibilidadQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Consultando disponibilidad para {NumeroPersonas} personas el {FechaHora}, Mesa preferida: {MesaPreferida}, Zona: {ZonaPreferida}", 
                request.NumeroPersonas, request.FechaHora, request.MesaPreferida, request.ZonaPreferida);

            // 1. Verificar si es una consulta para mesa específica
            if (request.MesaPreferida.HasValue)
            {
                return await ConsultarMesaEspecifica(request, cancellationToken);
            }

            // 2. Consultar disponibilidad general
            var disponibilidad = await ConsultarDisponibilidadGeneral(request, cancellationToken);

            _logger.LogInformation("Consulta de disponibilidad completada: {DisponibilidadDirecta} mesas disponibles directamente, {Alternativas} alternativas", 
                disponibilidad.MesasDisponibles.Count, disponibilidad.AlternativasSugeridas?.Count ?? 0);

            return Result.Success(disponibilidad);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🚫 Operación cancelada al consultar disponibilidad para {NumeroPersonas} personas el {FechaHora}", 
                request.NumeroPersonas, request.FechaHora);
            throw; // Re-throw para que se propague correctamente
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar disponibilidad para {NumeroPersonas} personas el {FechaHora}", 
                request.NumeroPersonas, request.FechaHora);
            return Result.Failure<DisponibilidadDto>("Error interno al consultar disponibilidad.");
        }
    }

    private async Task<Result<DisponibilidadDto>> ConsultarMesaEspecifica(ConsultarDisponibilidadQuery request, CancellationToken cancellationToken)
    {
        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == request.MesaPreferida.Value, cancellationToken);

        if (mesa == null)
        {
            return Result.Failure<DisponibilidadDto>("La mesa especificada no existe.");
        }

        var estaDisponible = await VerificarDisponibilidadMesa(mesa.Id, request.FechaHora, request.DuracionEstimadaMinutos, cancellationToken);

        var disponibilidad = new DisponibilidadDto
        {
            HayDisponibilidad = estaDisponible,
            FechaHoraConsultada = request.FechaHora,
            NumeroPersonasSolicitadas = request.NumeroPersonas,
            MesasDisponibles = new List<MesaDisponibleDto>()
        };

        if (estaDisponible)
        {
            // Verificar capacidad
            if (mesa.Capacidad >= request.NumeroPersonas - request.MargenToleranciaPersonas)
            {
                disponibilidad.MesasDisponibles.Add(await CrearMesaDisponibleDto(mesa, request.FechaHora, true));
            }
            else
            {
                disponibilidad.HayDisponibilidad = false;
                disponibilidad.MotivoNoDisponibilidad = $"La capacidad de la mesa ({mesa.Capacidad} personas) es insuficiente para el número solicitado ({request.NumeroPersonas} personas).";
            }
        }
        else
        {
            disponibilidad.MotivoNoDisponibilidad = "La mesa está ocupada en el horario solicitado.";
        }

        // Buscar alternativas si se solicita
        if (!disponibilidad.HayDisponibilidad && request.MostrarAlternativas)
        {
            disponibilidad.AlternativasSugeridas = await BuscarAlternativas(request, cancellationToken);
        }

        return Result.Success(disponibilidad);
    }

    private async Task<DisponibilidadDto> ConsultarDisponibilidadGeneral(ConsultarDisponibilidadQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtener todas las mesas que podrían servir
        var mesasCandidatas = await ObtenerMesasCandidatas(request, cancellationToken);

        var disponibilidad = new DisponibilidadDto
        {
            FechaHoraConsultada = request.FechaHora,
            NumeroPersonasSolicitadas = request.NumeroPersonas,
            MesasDisponibles = new List<MesaDisponibleDto>()
        };

        // 2. Verificar disponibilidad de cada mesa candidata
        foreach (var mesa in mesasCandidatas)
        {
            var estaDisponible = await VerificarDisponibilidadMesa(mesa.Id, request.FechaHora, request.DuracionEstimadaMinutos, cancellationToken);
            
            if (estaDisponible)
            {
                disponibilidad.MesasDisponibles.Add(await CrearMesaDisponibleDto(mesa, request.FechaHora, true));
            }
        }

        // 3. Ordenar por preferencia (capacidad óptima, zona preferida, etc.)
        disponibilidad.MesasDisponibles = OrdenarMesasPorPreferencia(disponibilidad.MesasDisponibles, request);

        // 4. Determinar si hay disponibilidad
        disponibilidad.HayDisponibilidad = disponibilidad.MesasDisponibles.Any();

        // 5. Buscar alternativas si no hay disponibilidad directa
        if (!disponibilidad.HayDisponibilidad && request.MostrarAlternativas)
        {
            disponibilidad.AlternativasSugeridas = await BuscarAlternativas(request, cancellationToken);
            disponibilidad.MotivoNoDisponibilidad = "No hay mesas disponibles en el horario exacto solicitado.";
        }
        else if (!disponibilidad.HayDisponibilidad)
        {
            disponibilidad.MotivoNoDisponibilidad = "No hay mesas disponibles para la fecha y hora solicitadas.";
        }

        // 6. Calcular estadísticas
        disponibilidad.EstadisticasOcupacion = await CalcularEstadisticasOcupacion(request.FechaHora, cancellationToken);

        return disponibilidad;
    }

    private async Task<List<Mesa>> ObtenerMesasCandidatas(ConsultarDisponibilidadQuery request, CancellationToken cancellationToken)
    {
        // TODO: Usar propiedad real Estado en lugar de Activa
        var query = _context.Mesas.Where(m => m.Estado != EstadoMesa.FueraDeServicio);

        // Filtrar por capacidad
        var capacidadMinima = request.NumeroPersonas - request.MargenToleranciaPersonas;
        var capacidadMaxima = request.PermitirCapacidadMayor ? int.MaxValue : request.NumeroPersonas + request.MargenToleranciaPersonas;

        query = query.Where(m => m.Capacidad >= capacidadMinima && m.Capacidad <= capacidadMaxima);

        // TODO: Filtrar por zona cuando Mesa tenga propiedad Zona
        // if (!string.IsNullOrEmpty(request.ZonaPreferida))
        // {
        //     query = query.Where(m => m.Zona == request.ZonaPreferida);
        // }

        // Excluir mesas fuera de servicio
        query = query.Where(m => m.Estado != EstadoMesa.FueraDeServicio);

        return await query.OrderBy(m => Math.Abs(m.Capacidad - request.NumeroPersonas)).ToListAsync(cancellationToken);
    }

    private async Task<bool> VerificarDisponibilidadMesa(Guid mesaId, DateTime fechaHora, int duracionMinutos, CancellationToken cancellationToken)
    {
        var horaInicio = fechaHora;
        var horaFin = fechaHora.AddMinutes(duracionMinutos);

        // Verificar si hay reservaciones que se solapan
        // TODO: Usar propiedades reales de Reservacion en lugar de FechaHora y DuracionEstimadaMinutos
        var tieneReservacionSolapada = await _context.Reservaciones
            .AnyAsync(r => r.MesaId == mesaId &&
                          r.Estado != Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada &&
                          r.Estado != Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.NoShow &&
                          // Usar propiedades reales de Reservacion
                          r.FechaReservacion >= horaInicio && r.FechaReservacion <= horaFin,
                          cancellationToken);

        return !tieneReservacionSolapada;
    }

    private async Task<MesaDisponibleDto> CrearMesaDisponibleDto(Mesa mesa, DateTime fechaHora, bool disponible)
    {
        return new MesaDisponibleDto
        {
            MesaId = mesa.Id,
            Numero = mesa.Numero,
            Capacidad = mesa.Capacidad,
            // TODO: Mesa debería tener propiedades Zona, Caracteristicas, PrecioBase, TieneVentana
            // Zona = mesa.Zona,
            Zona = "General", // Temporal
            Disponible = disponible,
            Ubicacion = mesa.Ubicacion,
            // Caracteristicas = mesa.Caracteristicas?.Split(',').ToList() ?? new List<string>(),
            // PrecioBase = mesa.PrecioBase,
            Caracteristicas = new List<string>(), // Temporal
            PrecioBase = 0, // Temporal
            EsVIP = false, // Temporal
            // TieneVentana = mesa.TieneVentana,
            TieneVentana = false, // Temporal
            ProximaDisponibilidad = disponible ? fechaHora : await CalcularProximaDisponibilidad(mesa.Id)
        };
    }

    private List<MesaDisponibleDto> OrdenarMesasPorPreferencia(List<MesaDisponibleDto> mesas, ConsultarDisponibilidadQuery request)
    {
        return mesas.OrderBy(m => Math.Abs(m.Capacidad - request.NumeroPersonas)) // Capacidad óptima primero
                   .ThenBy(m => string.IsNullOrEmpty(request.ZonaPreferida) ? 0 : 
                              m.Zona == request.ZonaPreferida ? 0 : 1) // Zona preferida primero
                   .ThenBy(m => m.PrecioBase) // Precio más bajo primero
                   .ThenBy(m => m.Numero) // Número de mesa como tiebreaker
                   .ToList();
    }

    private async Task<List<AlternativaDto>> BuscarAlternativas(ConsultarDisponibilidadQuery request, CancellationToken cancellationToken)
    {
        var alternativas = new List<AlternativaDto>();
        var horaBase = request.FechaHora;

        // Buscar en ventanas de tiempo antes y después
        var intervalos = new[]
        {
            -30, -60, 30, 60, -90, 90, -120, 120
        };

        foreach (var minutos in intervalos.Take(6)) // Limitar a 6 alternativas
        {
            if (Math.Abs(minutos) > request.RangoAlternativasMinutos) continue;

            var fechaAlternativa = horaBase.AddMinutes(minutos);
            
            // Verificar que siga en horario de atención
            if (fechaAlternativa.TimeOfDay < TimeSpan.FromHours(11) || 
                fechaAlternativa.TimeOfDay > TimeSpan.FromHours(23)) continue;

            var mesasDisponibles = await VerificarDisponibilidadEnFecha(fechaAlternativa, request, cancellationToken);
            
            if (mesasDisponibles.Any())
            {
                alternativas.Add(new AlternativaDto
                {
                    FechaHora = fechaAlternativa,
                    CantidadMesasDisponibles = mesasDisponibles.Count,
                    MejorOpcion = Math.Abs(minutos) <= 30, // Es mejor opción si está dentro de 30 minutos
                    DiferenciaMinutos = minutos
                });
            }
        }

        return alternativas.OrderBy(a => Math.Abs(a.DiferenciaMinutos)).ToList();
    }

    private async Task<List<MesaDisponibleDto>> VerificarDisponibilidadEnFecha(DateTime fechaHora, ConsultarDisponibilidadQuery request, CancellationToken cancellationToken)
    {
        var mesasCandidatas = await ObtenerMesasCandidatas(request, cancellationToken);
        var mesasDisponibles = new List<MesaDisponibleDto>();

        foreach (var mesa in mesasCandidatas.Take(3)) // Limitar para performance
        {
            var disponible = await VerificarDisponibilidadMesa(mesa.Id, fechaHora, request.DuracionEstimadaMinutos, cancellationToken);
            if (disponible)
            {
                mesasDisponibles.Add(await CrearMesaDisponibleDto(mesa, fechaHora, true));
            }
        }

        return mesasDisponibles;
    }

    private async Task<DateTime?> CalcularProximaDisponibilidad(Guid mesaId)
    {
        var ahora = DateTime.UtcNow;
        var proximaReservacion = await _context.Reservaciones
            .Where(r => r.MesaId == mesaId && 
                       r.FechaReservacion > ahora &&
                       r.Estado != Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada)
            .OrderBy(r => r.FechaReservacion)
            // TODO: Usar propiedades reales de Reservacion
            .Select(r => new { r.FechaReservacion, DuracionEstimadaMinutos = 120 }) // Duración temporal
            .FirstOrDefaultAsync();

        if (proximaReservacion == null)
        {
            return ahora.AddMinutes(15); // Disponible en 15 minutos
        }

        return proximaReservacion.FechaReservacion.AddMinutes(proximaReservacion.DuracionEstimadaMinutos);
    }

    private async Task<EstadisticasOcupacionDto> CalcularEstadisticasOcupacion(DateTime fechaHora, CancellationToken cancellationToken)
    {
        var inicioDelDia = fechaHora.Date;
        var finDelDia = inicioDelDia.AddDays(1);

        // TODO: Usar propiedad real Estado en lugar de Activa
        var totalMesas = await _context.Mesas.CountAsync(m => m.Estado != EstadoMesa.FueraDeServicio, cancellationToken);
        var mesasOcupadas = await _context.Reservaciones
            .Where(r => r.FechaReservacion >= inicioDelDia && 
                       r.FechaReservacion < finDelDia &&
                       r.Estado != Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada)
            .Select(r => r.MesaId)
            .Distinct()
            .CountAsync(cancellationToken);

        var porcentajeOcupacion = totalMesas > 0 ? (decimal)mesasOcupadas / totalMesas * 100 : 0;

        return new EstadisticasOcupacionDto
        {
            TotalMesas = totalMesas,
            MesasOcupadas = mesasOcupadas,
            MesasDisponibles = totalMesas - mesasOcupadas,
            PorcentajeOcupacion = porcentajeOcupacion,
            NivelOcupacion = porcentajeOcupacion switch
            {
                >= 90 => "Muy Alto",
                >= 70 => "Alto", 
                >= 50 => "Medio",
                >= 30 => "Bajo",
                _ => "Muy Bajo"
            }
        };
    }
} 