namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;

/// <summary>
/// Handler para generar análisis completo de fidelización de clientes
/// Proporciona métricas, patrones de comportamiento y recomendaciones
/// </summary>
public class ObtenerAnalisisFidelizacionHandler : IRequestHandler<ObtenerAnalisisFidelizacionQuery, Result<AnalisisFidelizacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerAnalisisFidelizacionHandler> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComercialServiceFacade _comercialServiceFacade;

    public ObtenerAnalisisFidelizacionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerAnalisisFidelizacionHandler> logger,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IComercialServiceFacade comercialServiceFacade)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _comercialServiceFacade = comercialServiceFacade;
    }

    public async Task<Result<AnalisisFidelizacionDto>> Handle(ObtenerAnalisisFidelizacionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📊 Iniciando análisis de fidelización - Período: {FechaInicio} a {FechaFin}, Usuario: {UserId}",
            request.FechaInicio, request.FechaFin, _currentUserService.UserId);

        try
        {
            // 1. Validar parámetros
            var validacionResult = ValidarParametros(request);
            if (!validacionResult.Succeeded)
            {
                return Result.Failure<AnalisisFidelizacionDto>(validacionResult.Error);
            }

            // 2. Obtener datos base
            var datosFidelizacion = await ObtenerDatosFidelizacion(request, cancellationToken);

            // 3. Calcular métricas principales
            var metricas = await CalcularMetricasFidelizacion(datosFidelizacion, request, cancellationToken);

            // 4. Analizar segmentación de clientes
            var segmentacion = await AnalizarSegmentacionClientes(datosFidelizacion, cancellationToken);

            // 5. Identificar patrones de comportamiento
            var patrones = await IdentificarPatronesComportamiento(datosFidelizacion, cancellationToken);

            // 6. Generar recomendaciones
            var recomendaciones = await GenerarRecomendacionesFidelizacion(datosFidelizacion, metricas, segmentacion, patrones, cancellationToken);

            // 7. Calcular tendencias
            var tendencias = await CalcularTendenciasFidelizacion(request, cancellationToken);

            // 8. Construir análisis completo
            var analisis = ConstruirAnalisisCompleto(datosFidelizacion, metricas, segmentacion, patrones, recomendaciones, tendencias, request);

            _logger.LogInformation("✅ Análisis de fidelización completado - {TotalClientes} clientes analizados, {TotalRecomendaciones} recomendaciones generadas",
                analisis.TotalClientes, analisis.Recomendaciones.Count);

            return Result.Success(analisis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando análisis de fidelización: {Error}", ex.Message);
            return Result.Failure<AnalisisFidelizacionDto>("Error interno al generar el análisis de fidelización");
        }
    }

    private Result ValidarParametros(ObtenerAnalisisFidelizacionQuery request)
    {
        if (request.FechaInicio > request.FechaFin)
        {
            return Result.Failure("La fecha de inicio no puede ser posterior a la fecha de fin");
        }

        if (request.FechaFin > _dateTimeService.Now)
        {
            return Result.Failure("La fecha de fin no puede ser futura");
        }

        var diferenciaDias = (request.FechaFin - request.FechaInicio).TotalDays;
        if (diferenciaDias > 730) // 2 años máximo
        {
            return Result.Failure("El período de análisis no puede exceder 2 años");
        }

        if (diferenciaDias < 7) // Mínimo 1 semana
        {
            return Result.Failure("El período de análisis debe ser de al menos 1 semana");
        }

        return Result.Success();
    }

    private async Task<DatosFidelizacionAnalisis> ObtenerDatosFidelizacion(ObtenerAnalisisFidelizacionQuery request, CancellationToken cancellationToken)
    {
        var clientesQuery = _context.Clientes
            .Include(c => c.TarjetaFidelizacion)
            .ThenInclude(t => t.MovimientosPuntos)
            .Include(c => c.Reservaciones)
            .Include(c => c.Comandas)
            .ThenInclude(com => com.Facturas)
            .AsQueryable();

        // Aplicar filtros opcionales
        if (request.SoloClientesActivos)
        {
            clientesQuery = clientesQuery.Where(c => c.Estado == EstadoCliente.Activo);
        }

        if (request.SoloConTarjetaFidelizacion)
        {
            clientesQuery = clientesQuery.Where(c => c.TarjetaFidelizacion != null);
        }

        if (request.NivelFidelizacionId.HasValue)
        {
            clientesQuery = clientesQuery.Where(c => c.TarjetaFidelizacion != null && 
                                                    c.TarjetaFidelizacion.NivelFidelizacionId == request.NivelFidelizacionId);
        }

        var clientes = await clientesQuery.ToListAsync(cancellationToken);

        // Obtener facturas del período
        var facturas = await _context.Facturas
            .Where(f => f.FechaEmision >= request.FechaInicio && 
                       f.FechaEmision <= request.FechaFin &&
                       f.ClienteId.HasValue)
            .Include(f => f.Cliente)
            .ToListAsync(cancellationToken);

        // Obtener movimientos de puntos del período
        var movimientosPuntos = await _context.MovimientosPuntos
            .Where(m => m.FechaMovimiento >= request.FechaInicio && 
                       m.FechaMovimiento <= request.FechaFin)
            .Include(m => m.TarjetaFidelizacion)
            .ThenInclude(t => t.Cliente)
            .ToListAsync(cancellationToken);

        // Obtener reservaciones del período
        var reservaciones = await _context.Reservaciones
            .Where(r => r.FechaReservacion >= request.FechaInicio && 
                       r.FechaReservacion <= request.FechaFin &&
                       r.ClienteId.HasValue)
            .Include(r => r.Cliente)
            .ToListAsync(cancellationToken);

        return new DatosFidelizacionAnalisis
        {
            Clientes = clientes,
            Facturas = facturas,
            MovimientosPuntos = movimientosPuntos,
            Reservaciones = reservaciones,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin
        };
    }

    private async Task<MetricasFidelizacion> CalcularMetricasFidelizacion(DatosFidelizacionAnalisis datos, ObtenerAnalisisFidelizacionQuery request, CancellationToken cancellationToken)
    {
        var metricas = new MetricasFidelizacion();

        // Métricas básicas de clientes
        metricas.TotalClientes = datos.Clientes.Count;
        metricas.ClientesConTarjeta = datos.Clientes.Count(c => c.TarjetaFidelizacion != null);
        metricas.ClientesActivos = datos.Clientes.Count(c => c.Estado == EstadoCliente.Activo);
        metricas.PorcentajeConTarjeta = metricas.TotalClientes > 0 ? 
            (decimal)metricas.ClientesConTarjeta / metricas.TotalClientes * 100 : 0;

        // Métricas de transacciones
        metricas.TotalFacturas = datos.Facturas.Count;
        metricas.MontoTotalVentas = datos.Facturas.Sum(f => f.Total);
        metricas.TicketPromedio = metricas.TotalFacturas > 0 ? 
            metricas.MontoTotalVentas / metricas.TotalFacturas : 0;

        // Métricas de puntos
        var puntosAcumulados = datos.MovimientosPuntos
            .Where(m => m.TipoMovimiento == TipoMovimientoPuntos.Acumulacion)
            .Sum(m => m.Puntos);
        var puntosCanjeados = datos.MovimientosPuntos
            .Where(m => m.TipoMovimiento == TipoMovimientoPuntos.Canje)
            .Sum(m => m.Puntos);

        metricas.PuntosAcumulados = puntosAcumulados;
        metricas.PuntosCanjeados = puntosCanjeados;
        metricas.PuntosPendientes = puntosAcumulados - puntosCanjeados;
        metricas.TasaCanje = puntosAcumulados > 0 ? (decimal)puntosCanjeados / puntosAcumulados * 100 : 0;

        // Métricas de frecuencia
        var clientesConCompras = datos.Facturas
            .Where(f => f.ClienteId.HasValue)
            .GroupBy(f => f.ClienteId.Value)
            .ToList();

        metricas.FrecuenciaCompraPromedio = clientesConCompras.Count > 0 ? 
            (decimal)clientesConCompras.Average(g => g.Count()) : 0;

        metricas.ClientesRecurrentes = clientesConCompras.Count(g => g.Count() > 1);
        metricas.PorcentajeRecurrencia = metricas.TotalClientes > 0 ? 
            (decimal)metricas.ClientesRecurrentes / metricas.TotalClientes * 100 : 0;

        // Valor de vida del cliente (CLV simplificado)
        if (clientesConCompras.Any())
        {
            var diasPeriodo = (datos.FechaFin - datos.FechaInicio).TotalDays;
            var ventasPorClientePromedio = clientesConCompras.Average(g => g.Sum(f => f.Total));
            var frecuenciaAnual = metricas.FrecuenciaCompraPromedio * (365 / (decimal)diasPeriodo);
            metricas.ValorVidaClientePromedio = ventasPorClientePromedio * frecuenciaAnual * 3; // Estimado 3 años
        }

        return metricas;
    }

    private async Task<SegmentacionClientes> AnalizarSegmentacionClientes(DatosFidelizacionAnalisis datos, CancellationToken cancellationToken)
    {
        var segmentacion = new SegmentacionClientes();

        var clientesConVentas = datos.Facturas
            .Where(f => f.ClienteId.HasValue)
            .GroupBy(f => f.ClienteId.Value)
            .Select(g => new {
                ClienteId = g.Key,
                TotalCompras = g.Sum(f => f.Total),
                NumeroCompras = g.Count(),
                UltimaCompra = g.Max(f => f.FechaEmision)
            })
            .ToList();

        if (clientesConVentas.Any())
        {
            // Segmentación RFM simplificada
            var medianaRecencia = clientesConVentas.Select(c => (_dateTimeService.Now - c.UltimaCompra).TotalDays).Median();
            var medianaFrecuencia = clientesConVentas.Select(c => c.NumeroCompras).Median();
            var medianaMonetario = clientesConVentas.Select(c => (double)c.TotalCompras).Median();

            foreach (var cliente in clientesConVentas)
            {
                var recencia = (_dateTimeService.Now - cliente.UltimaCompra).TotalDays;
                var frecuencia = cliente.NumeroCompras;
                var monetario = (double)cliente.TotalCompras;

                string segmento;
                if (recencia < medianaRecencia && frecuencia > medianaFrecuencia && monetario > medianaMonetario)
                {
                    segmento = "Champions"; // Mejores clientes
                }
                else if (recencia < medianaRecencia && frecuencia > medianaFrecuencia)
                {
                    segmento = "Loyal Customers"; // Clientes leales
                }
                else if (monetario > medianaMonetario)
                {
                    segmento = "Big Spenders"; // Grandes gastadores
                }
                else if (recencia < medianaRecencia)
                {
                    segmento = "New Customers"; // Clientes nuevos
                }
                else if (recencia > medianaRecencia * 2)
                {
                    segmento = "At Risk"; // En riesgo
                }
                else
                {
                    segmento = "Potential Loyalists"; // Potenciales leales
                }

                if (!segmentacion.SegmentosPorCliente.ContainsKey(segmento))
                {
                    segmentacion.SegmentosPorCliente[segmento] = new List<Guid>();
                }
                segmentacion.SegmentosPorCliente[segmento].Add(cliente.ClienteId);
            }
        }

        // Calcular resumen de segmentos
        segmentacion.ResumenSegmentos = segmentacion.SegmentosPorCliente
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new ResumenSegmento
                {
                    CantidadClientes = kvp.Value.Count,
                    PorcentajeTotal = clientesConVentas.Count > 0 ? 
                        (decimal)kvp.Value.Count / clientesConVentas.Count * 100 : 0
                }
            );

        return segmentacion;
    }

    private async Task<PatronesComportamiento> IdentificarPatronesComportamiento(DatosFidelizacionAnalisis datos, CancellationToken cancellationToken)
    {
        var patrones = new PatronesComportamiento();

        // Análisis temporal de compras
        var ventasPorDia = datos.Facturas
            .GroupBy(f => f.FechaEmision.DayOfWeek)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var ventasPorHora = datos.Facturas
            .GroupBy(f => f.FechaEmision.Hour)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        patrones.DiasPreferidos = ventasPorDia
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        patrones.HorasPreferidas = ventasPorHora
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Análisis de temporalidad de canjes
        var canjesPorMes = datos.MovimientosPuntos
            .Where(m => m.TipoMovimiento == TipoMovimientoPuntos.Canje)
            .GroupBy(m => m.FechaMovimiento.Month)
            .ToDictionary(g => g.Key.ToString(), g => g.Sum(m => m.Puntos));

        patrones.MesesMayorCanje = canjesPorMes
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Patrones de gasto
        if (datos.Facturas.Any())
        {
            patrones.TicketPromedioMenor = datos.Facturas.Min(f => f.Total);
            patrones.TicketPromedioMayor = datos.Facturas.Max(f => f.Total);
            patrones.DesviacionStandardTicket = CalcularDesviacionStandard(datos.Facturas.Select(f => (double)f.Total));
        }

        return patrones;
    }

    private double CalcularDesviacionStandard(IEnumerable<double> valores)
    {
        var valoresArray = valores.ToArray();
        if (valoresArray.Length == 0) return 0;

        var promedio = valoresArray.Average();
        var sumaCuadrados = valoresArray.Sum(x => Math.Pow(x - promedio, 2));
        return Math.Sqrt(sumaCuadrados / valoresArray.Length);
    }

    private async Task<List<RecomendacionFidelizacion>> GenerarRecomendacionesFidelizacion(
        DatosFidelizacionAnalisis datos,
        MetricasFidelizacion metricas,
        SegmentacionClientes segmentacion,
        PatronesComportamiento patrones,
        CancellationToken cancellationToken)
    {
        var recomendaciones = new List<RecomendacionFidelizacion>();

        // Recomendación para penetración de tarjetas
        if (metricas.PorcentajeConTarjeta < 60)
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Tipo = "PENETRACIÓN",
                Prioridad = "ALTA",
                Titulo = "Incrementar adopción de tarjetas de fidelización",
                Descripcion = $"Solo el {metricas.PorcentajeConTarjeta:F1}% de clientes tiene tarjeta de fidelización",
                Accion = "Campaña de incentivos para registro en programa de fidelización",
                ImpactoEstimado = "Aumentar retención y frecuencia de compra en 15-25%",
                MetricaObjetivo = "Alcanzar 70% de penetración en 6 meses"
            });
        }

        // Recomendación para tasa de canje
        if (metricas.TasaCanje < 30)
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Tipo = "ACTIVACIÓN",
                Prioridad = "MEDIA",
                Titulo = "Mejorar tasa de canje de puntos",
                Descripcion = $"Tasa de canje baja: {metricas.TasaCanje:F1}% (objetivo: >40%)",
                Accion = "Crear promociones atractivas y comunicar beneficios disponibles",
                ImpactoEstimado = "Incrementar satisfacción y engagement",
                MetricaObjetivo = "Alcanzar 40% de tasa de canje"
            });
        }

        // Recomendación para clientes en riesgo
        if (segmentacion.ResumenSegmentos.ContainsKey("At Risk"))
        {
            var clientesEnRiesgo = segmentacion.ResumenSegmentos["At Risk"].CantidadClientes;
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Tipo = "RETENCIÓN",
                Prioridad = "ALTA",
                Titulo = "Programa de retención para clientes en riesgo",
                Descripcion = $"{clientesEnRiesgo} clientes identificados en riesgo de abandono",
                Accion = "Campaña personalizada con ofertas especiales y comunicación directa",
                ImpactoEstimado = "Recuperar 40-60% de clientes en riesgo",
                MetricaObjetivo = $"Reactivar al menos {clientesEnRiesgo * 0.5:F0} clientes"
            });
        }

        // Recomendación para recurrencia
        if (metricas.PorcentajeRecurrencia < 40)
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Tipo = "FRECUENCIA",
                Prioridad = "MEDIA",
                Titulo = "Incrementar frecuencia de visitas",
                Descripcion = $"Solo {metricas.PorcentajeRecurrencia:F1}% de clientes son recurrentes",
                Accion = "Programa de visitas frecuentes con recompensas progresivas",
                ImpactoEstimado = "Aumentar frecuencia promedio en 20-30%",
                MetricaObjetivo = "Alcanzar 50% de clientes recurrentes"
            });
        }

        // Recomendación para Champions
        if (segmentacion.ResumenSegmentos.ContainsKey("Champions"))
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Tipo = "PREMIUM",
                Prioridad = "MEDIA",
                Titulo = "Programa VIP para mejores clientes",
                Descripcion = "Desarrollar experiencias exclusivas para clientes Champions",
                Accion = "Beneficios premium, acceso anticipado, eventos exclusivos",
                ImpactoEstimado = "Incrementar valor de vida del cliente en 25-40%",
                MetricaObjetivo = "Mantener y crecer el segmento Champions"
            });
        }

        return recomendaciones.OrderByDescending(r => r.Prioridad).ToList();
    }

    private async Task<TendenciasFidelizacion> CalcularTendenciasFidelizacion(ObtenerAnalisisFidelizacionQuery request, CancellationToken cancellationToken)
    {
        var tendencias = new TendenciasFidelizacion();

        // Calcular tendencias comparando con período anterior
        var diasPeriodo = (request.FechaFin - request.FechaInicio).TotalDays;
        var fechaInicioComparacion = request.FechaInicio.AddDays(-diasPeriodo);

        // Tendencia de nuevos clientes con tarjeta
        var nuevasTarjetasActual = await _context.TarjetasFidelizacion
            .CountAsync(t => t.FechaCreacion >= request.FechaInicio && t.FechaCreacion <= request.FechaFin, cancellationToken);

        var nuevasTarjetasAnterior = await _context.TarjetasFidelizacion
            .CountAsync(t => t.FechaCreacion >= fechaInicioComparacion && t.FechaCreacion < request.FechaInicio, cancellationToken);

        tendencias.TendenciaNuevasTarjetas = nuevasTarjetasAnterior > 0 ? 
            ((double)(nuevasTarjetasActual - nuevasTarjetasAnterior) / nuevasTarjetasAnterior) * 100 : 0;

        // Tendencia de canjes
        var canjesActual = await _context.MovimientosPuntos
            .Where(m => m.TipoMovimiento == TipoMovimientoPuntos.Canje &&
                       m.FechaMovimiento >= request.FechaInicio && m.FechaMovimiento <= request.FechaFin)
            .SumAsync(m => m.Puntos, cancellationToken);

        var canjesAnterior = await _context.MovimientosPuntos
            .Where(m => m.TipoMovimiento == TipoMovimientoPuntos.Canje &&
                       m.FechaMovimiento >= fechaInicioComparacion && m.FechaMovimiento < request.FechaInicio)
            .SumAsync(m => m.Puntos, cancellationToken);

        tendencias.TendenciaCanjes = canjesAnterior > 0 ? 
            ((double)(canjesActual - canjesAnterior) / canjesAnterior) * 100 : 0;

        return tendencias;
    }

    private AnalisisFidelizacionDto ConstruirAnalisisCompleto(
        DatosFidelizacionAnalisis datos,
        MetricasFidelizacion metricas,
        SegmentacionClientes segmentacion,
        PatronesComportamiento patrones,
        List<RecomendacionFidelizacion> recomendaciones,
        TendenciasFidelizacion tendencias,
        ObtenerAnalisisFidelizacionQuery request)
    {
        return new AnalisisFidelizacionDto
        {
            FechaGeneracion = _dateTimeService.Now,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            UsuarioGeneradorId = _currentUserService.UserId,

            // Métricas principales
            TotalClientes = metricas.TotalClientes,
            ClientesConTarjeta = metricas.ClientesConTarjeta,
            ClientesActivos = metricas.ClientesActivos,
            PorcentajeConTarjeta = metricas.PorcentajeConTarjeta,
            PorcentajeRecurrencia = metricas.PorcentajeRecurrencia,
            FrecuenciaCompraPromedio = metricas.FrecuenciaCompraPromedio,
            TicketPromedio = metricas.TicketPromedio,
            ValorVidaClientePromedio = metricas.ValorVidaClientePromedio,

            // Métricas de puntos
            PuntosAcumulados = metricas.PuntosAcumulados,
            PuntosCanjeados = metricas.PuntosCanjeados,
            PuntosPendientes = metricas.PuntosPendientes,
            TasaCanje = metricas.TasaCanje,

            // Segmentación
            SegmentosClientes = segmentacion.ResumenSegmentos,
            
            // Patrones de comportamiento
            DiasPreferidos = patrones.DiasPreferidos,
            HorasPreferidas = patrones.HorasPreferidas,
            MesesMayorCanje = patrones.MesesMayorCanje,

            // Recomendaciones
            Recomendaciones = recomendaciones,

            // Tendencias
            TendenciaNuevasTarjetas = tendencias.TendenciaNuevasTarjetas,
            TendenciaCanjes = tendencias.TendenciaCanjes,

            // Resumen ejecutivo
            ResumenEjecutivo = GenerarResumenEjecutivoFidelizacion(metricas, segmentacion, recomendaciones, tendencias)
        };
    }

    private string GenerarResumenEjecutivoFidelizacion(
        MetricasFidelizacion metricas,
        SegmentacionClientes segmentacion,
        List<RecomendacionFidelizacion> recomendaciones,
        TendenciasFidelizacion tendencias)
    {
        var resumen = new List<string>();

        // Estado del programa
        resumen.Add($"📊 Programa fidelización: {metricas.ClientesConTarjeta} tarjetas activas ({metricas.PorcentajeConTarjeta:F1}% penetración).");

        // Efectividad del programa
        if (metricas.TasaCanje >= 40)
        {
            resumen.Add($"✅ Alta actividad: {metricas.TasaCanje:F1}% tasa de canje.");
        }
        else if (metricas.TasaCanje < 30)
        {
            resumen.Add($"⚠️ Baja actividad: {metricas.TasaCanje:F1}% tasa de canje.");
        }

        // Segmentación principal
        var segmentoPrincipal = segmentacion.ResumenSegmentos
            .OrderByDescending(s => s.Value.CantidadClientes)
            .FirstOrDefault();
        
        if (segmentoPrincipal.Key != null)
        {
            resumen.Add($"👥 Segmento principal: {segmentoPrincipal.Key} ({segmentoPrincipal.Value.PorcentajeTotal:F1}% clientes).");
        }

        // Valor del cliente
        if (metricas.ValorVidaClientePromedio > 0)
        {
            resumen.Add($"💰 CLV promedio: ${metricas.ValorVidaClientePromedio:N0}.");
        }

        // Tendencias
        if (Math.Abs(tendencias.TendenciaNuevasTarjetas) > 10)
        {
            var direccion = tendencias.TendenciaNuevasTarjetas > 0 ? "crecimiento" : "disminución";
            resumen.Add($"📈 {direccion} {Math.Abs(tendencias.TendenciaNuevasTarjetas):F1}% en nuevas tarjetas.");
        }

        // Recomendaciones críticas
        var recomendacionesCriticas = recomendaciones.Count(r => r.Prioridad == "ALTA");
        if (recomendacionesCriticas > 0)
        {
            resumen.Add($"🎯 {recomendacionesCriticas} acciones prioritarias identificadas.");
        }

        return string.Join(" ", resumen);
    }

    // DTOs internos para el análisis
    private class DatosFidelizacionAnalisis
    {
        public List<Cliente> Clientes { get; set; } = new();
        public List<Factura> Facturas { get; set; } = new();
        public List<MovimientoPuntos> MovimientosPuntos { get; set; } = new();
        public List<Reservacion> Reservaciones { get; set; } = new();
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    private class MetricasFidelizacion
    {
        public int TotalClientes { get; set; }
        public int ClientesConTarjeta { get; set; }
        public int ClientesActivos { get; set; }
        public decimal PorcentajeConTarjeta { get; set; }
        public int TotalFacturas { get; set; }
        public decimal MontoTotalVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public int PuntosAcumulados { get; set; }
        public int PuntosCanjeados { get; set; }
        public int PuntosPendientes { get; set; }
        public decimal TasaCanje { get; set; }
        public decimal FrecuenciaCompraPromedio { get; set; }
        public int ClientesRecurrentes { get; set; }
        public decimal PorcentajeRecurrencia { get; set; }
        public decimal ValorVidaClientePromedio { get; set; }
    }

    private class SegmentacionClientes
    {
        public Dictionary<string, List<Guid>> SegmentosPorCliente { get; set; } = new();
        public Dictionary<string, ResumenSegmento> ResumenSegmentos { get; set; } = new();
    }

    private class ResumenSegmento
    {
        public int CantidadClientes { get; set; }
        public decimal PorcentajeTotal { get; set; }
    }

    private class PatronesComportamiento
    {
        public Dictionary<string, int> DiasPreferidos { get; set; } = new();
        public Dictionary<string, int> HorasPreferidas { get; set; } = new();
        public Dictionary<string, int> MesesMayorCanje { get; set; } = new();
        public decimal TicketPromedioMenor { get; set; }
        public decimal TicketPromedioMayor { get; set; }
        public double DesviacionStandardTicket { get; set; }
    }

    private class TendenciasFidelizacion
    {
        public double TendenciaNuevasTarjetas { get; set; }
        public double TendenciaCanjes { get; set; }
    }
}

// Extensión para calcular mediana
public static class EnumerableExtensions
{
    public static double Median(this IEnumerable<double> source)
    {
        var sorted = source.OrderBy(x => x).ToArray();
        var count = sorted.Length;
        if (count == 0) return 0;
        
        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    public static double Median(this IEnumerable<int> source)
    {
        return source.Select(x => (double)x).Median();
    }
} 