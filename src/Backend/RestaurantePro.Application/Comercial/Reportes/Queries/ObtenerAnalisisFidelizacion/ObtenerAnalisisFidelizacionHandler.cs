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
                analisis.ResumenExecutivo.TotalClientesAnalizados, analisis.RecomendacionesEstrategicas.Count);

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

        if (diferenciaDias <= 6) // Mínimo 1 semana (7 días)
        {
            return Result.Failure("El período de análisis debe ser de al menos 1 semana");
        }

        return Result.Success();
    }

    private async Task<DatosFidelizacionAnalisis> ObtenerDatosFidelizacion(ObtenerAnalisisFidelizacionQuery request, CancellationToken cancellationToken)
    {
        var clientesQuery = _context.Clientes.AsQueryable();

        // Aplicar filtros opcionales usando las propiedades que realmente existen
        if (!request.IncluirClientesInactivos)
        {
            clientesQuery = clientesQuery.Where(c => c.EstaActivo);
        }

        // Filtro por clientes específicos si se proporcionan
        if (request.ClientesEspecificos?.Any() == true)
        {
            clientesQuery = clientesQuery.Where(c => request.ClientesEspecificos.Contains(c.Id));
        }

        // Filtro por nivel mínimo (usando TarjetaFidelizacionPrincipalId)
        // TODO: Implementar cuando tengamos acceso a las tarjetas de fidelización
        // if (request.NivelMinimo.HasValue)
        // {
        //     clientesQuery = clientesQuery.Where(c => c.TarjetaFidelizacionPrincipalId.HasValue);
        // }

        var clientes = await clientesQuery.ToListAsync(cancellationToken);

        // Obtener facturas del período - ClienteId puede ser null en algunas facturas
        var facturas = await _context.Facturas
            .Where(f => f.FechaEmision >= request.FechaInicio && 
                       f.FechaEmision <= request.FechaFin &&
                       f.ClienteId != null)
            .Include(f => f.Cliente)
            .ToListAsync(cancellationToken);

        // Obtener reservaciones del período - todas las reservaciones tienen ClienteId
        var reservaciones = await _context.Reservaciones
            .Where(r => r.FechaReservacion >= request.FechaInicio && 
                       r.FechaReservacion <= request.FechaFin)
            .Include(r => r.Cliente)
            .ToListAsync(cancellationToken);

        return new DatosFidelizacionAnalisis
        {
            Clientes = clientes,
            Facturas = facturas,
            MovimientosPuntos = new List<MovimientoPuntosDto>(), // Lista vacía por ahora
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
        metricas.ClientesConTarjeta = datos.Clientes.Count(c => c.TarjetaFidelizacionPrincipalId.HasValue);
        metricas.ClientesActivos = datos.Clientes.Count(c => c.EstaActivo);
        metricas.PorcentajeConTarjeta = metricas.TotalClientes > 0 ? 
            (decimal)metricas.ClientesConTarjeta / metricas.TotalClientes * 100 : 0;

        // Métricas de transacciones
        metricas.TotalFacturas = datos.Facturas.Count;
        metricas.MontoTotalVentas = datos.Facturas.Sum(f => f.Total);
        metricas.TicketPromedio = metricas.TotalFacturas > 0 ? 
            metricas.MontoTotalVentas / metricas.TotalFacturas : 0;

        // Métricas de puntos - simplificadas usando PuntosAcumulados de Cliente
        var totalPuntosClientes = datos.Clientes.Where(c => c.EstaActivo).Sum(c => c.PuntosAcumulados);
        metricas.PuntosAcumulados = totalPuntosClientes;
        metricas.PuntosCanjeados = 0; // TODO: Calcular cuando tengamos acceso a movimientos de puntos
        metricas.PuntosPendientes = totalPuntosClientes;
        metricas.TasaCanje = 0; // TODO: Calcular cuando tengamos los canjes

        // Métricas de frecuencia
        var clientesConCompras = datos.Facturas
            .Where(f => f.ClienteId != null)
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

        // Análisis de temporalidad de canjes - simplificado sin MovimientosPuntos
        // TODO: Implementar cuando tengamos acceso a movimientos de puntos reales
        patrones.MesesMayorCanje = new Dictionary<string, int>();

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
                Titulo = "Incrementar adopción de tarjetas de fidelización",
                Descripcion = $"Solo el {metricas.PorcentajeConTarjeta:F1}% de clientes tiene tarjeta de fidelización",
                Accion = "Campaña de incentivos para registro en programa de fidelización",
                ImpactoEsperado = 25, // 25% de incremento esperado
                Prioridad = NivelPrioridad.Alta,
                Categoria = "Penetración"
            });
        }

        // Recomendación para tasa de canje
        if (metricas.TasaCanje < 30)
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Titulo = "Mejorar tasa de canje de puntos",
                Descripcion = $"Tasa de canje baja: {metricas.TasaCanje:F1}% (objetivo: >40%)",
                Accion = "Crear promociones atractivas y comunicar beneficios disponibles",
                ImpactoEsperado = 15, // 15% de incremento esperado
                Prioridad = NivelPrioridad.Media,
                Categoria = "Activación"
            });
        }

        // Recomendación para clientes en riesgo
        if (segmentacion.ResumenSegmentos.ContainsKey("At Risk"))
        {
            var clientesEnRiesgo = segmentacion.ResumenSegmentos["At Risk"].CantidadClientes;
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Titulo = "Programa de retención para clientes en riesgo",
                Descripcion = $"{clientesEnRiesgo} clientes identificados en riesgo de abandono",
                Accion = "Campaña personalizada con ofertas especiales y comunicación directa",
                ImpactoEsperado = 50, // 50% de recuperación esperada
                Prioridad = NivelPrioridad.Alta,
                Categoria = "Retención"
            });
        }

        // Recomendación para recurrencia
        if (metricas.PorcentajeRecurrencia < 40)
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Titulo = "Incrementar frecuencia de visitas",
                Descripcion = $"Solo {metricas.PorcentajeRecurrencia:F1}% de clientes son recurrentes",
                Accion = "Programa de visitas frecuentes con recompensas progresivas",
                ImpactoEsperado = 30, // 30% de incremento esperado
                Prioridad = NivelPrioridad.Media,
                Categoria = "Frecuencia"
            });
        }

        // Recomendación para Champions
        if (segmentacion.ResumenSegmentos.ContainsKey("Champions"))
        {
            recomendaciones.Add(new RecomendacionFidelizacion
            {
                Titulo = "Programa VIP para mejores clientes",
                Descripcion = "Desarrollar experiencias exclusivas para clientes Champions",
                Accion = "Beneficios premium, acceso anticipado, eventos exclusivos",
                ImpactoEsperado = 35, // 35% de incremento en valor de vida
                Prioridad = NivelPrioridad.Media,
                Categoria = "Premium"
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

        // Tendencia de nuevos clientes - usando fecha de creación de clientes
        var nuevosClientesActual = await _context.Clientes
            .CountAsync(c => c.FechaCreacion >= request.FechaInicio && c.FechaCreacion <= request.FechaFin, cancellationToken);

        var nuevosClientesAnterior = await _context.Clientes
            .CountAsync(c => c.FechaCreacion >= fechaInicioComparacion && c.FechaCreacion < request.FechaInicio, cancellationToken);

        tendencias.TendenciaNuevasTarjetas = nuevosClientesAnterior > 0 ? 
            ((double)(nuevosClientesActual - nuevosClientesAnterior) / nuevosClientesAnterior) * 100 : 0;

        // Tendencia de ventas (como proxy para actividad)
        var ventasActual = await _context.Facturas
            .Where(f => f.FechaEmision >= request.FechaInicio && f.FechaEmision <= request.FechaFin)
            .SumAsync(f => f.Total, cancellationToken);

        var ventasAnterior = await _context.Facturas
            .Where(f => f.FechaEmision >= fechaInicioComparacion && f.FechaEmision < request.FechaInicio)
            .SumAsync(f => f.Total, cancellationToken);

        tendencias.TendenciaCanjes = ventasAnterior > 0 ? 
            ((double)(ventasActual - ventasAnterior) / (double)ventasAnterior) * 100 : 0;

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
            // Información del análisis
            InfoAnalisis = new InfoAnalisisFidelizacionDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                TipoAnalisis = request.TipoAnalisis.ToString(),
                PeriodoAnalisis = $"{(request.FechaFin - request.FechaInicio).TotalDays:F0} días",
                FechaGeneracion = _dateTimeService.Now,
                UsuarioSolicitante = _currentUserService.UserId ?? "Sistema",
                NivelConfiabilidad = "Media"
            },

            // Resumen ejecutivo
            ResumenExecutivo = new ResumenFidelizacionDto
            {
                TotalClientesAnalizados = metricas.TotalClientes,
                ClientesActivos = metricas.ClientesActivos,
                ClientesNuevos = 0, // TODO: Calcular clientes nuevos del período
                TasaRetencion = metricas.PorcentajeRecurrencia,
                ValorPromedioCliente = metricas.TicketPromedio,
                FrecuenciaPromedioVisitas = metricas.FrecuenciaCompraPromedio,
                TicketPromedio = metricas.TicketPromedio,
                TotalPuntosAcumulados = metricas.PuntosAcumulados,
                TotalPuntosCanjeados = metricas.PuntosCanjeados,
                TasaCanjeoPuntos = metricas.TasaCanje,
                IngresosTotales = metricas.MontoTotalVentas,
                CrecimientoVsPeriodoAnterior = (decimal)tendencias.TendenciaNuevasTarjetas,
                EstadoGeneralPrograma = GenerarEstadoGeneral(metricas)
            },

            // Métricas del programa
            MetricasPrograma = new MetricasProgramaFidelizacionDto
            {
                TasaParticipacion = metricas.PorcentajeConTarjeta,
                TasaActivacion = metricas.PorcentajeRecurrencia,
                ValorVidaClientePromedio = metricas.ValorVidaClientePromedio,
                ROIPrograma = 0, // TODO: Calcular ROI cuando tengamos más datos
                CostoPorClienteAdquirido = 0, // TODO: Calcular cuando tengamos costos
                EfectividadCampanas = 0 // TODO: Calcular cuando tengamos datos de campañas
            },

            // Recomendaciones estratégicas
            RecomendacionesEstrategicas = recomendaciones.Select(r => new RecomendacionEstrategicaDto
            {
                Titulo = r.Titulo,
                Descripcion = r.Descripcion,
                Categoria = r.Categoria,
                Prioridad = r.Prioridad.ToString(),
                ImpactoEsperado = $"{r.ImpactoEsperado}% de mejora esperada",
                EsfuerzoRequerido = "Medio",
                TiempoImplementacion = "1-3 meses",
                ROIEsperado = r.ImpactoEsperado
            }).ToList()
        };
    }

    private string GenerarEstadoGeneral(MetricasFidelizacion metricas)
    {
        if (metricas.PorcentajeConTarjeta > 70 && metricas.PorcentajeRecurrencia > 50)
            return "Excelente";
        else if (metricas.PorcentajeConTarjeta > 50 && metricas.PorcentajeRecurrencia > 30)
            return "Bueno";
        else if (metricas.PorcentajeConTarjeta > 30 && metricas.PorcentajeRecurrencia > 20)
            return "Regular";
        else
            return "Necesita Mejora";
    }

    // DTOs internos para el análisis
    private class DatosFidelizacionAnalisis
    {
        public List<Cliente> Clientes { get; set; } = new();
        public List<Factura> Facturas { get; set; } = new();
        public List<MovimientoPuntosDto> MovimientosPuntos { get; set; } = new();
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