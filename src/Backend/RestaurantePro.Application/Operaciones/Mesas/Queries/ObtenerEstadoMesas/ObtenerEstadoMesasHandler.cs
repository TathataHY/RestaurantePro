namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerEstadoMesas;

/// <summary>
/// Handler para ObtenerEstadoMesasQuery
/// </summary>
public class ObtenerEstadoMesasHandler : IRequestHandler<ObtenerEstadoMesasQuery, Result<EstadoMesasDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerEstadoMesasHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ObtenerEstadoMesasHandler(
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ObtenerEstadoMesasHandler> logger,
        ICurrentUserService currentUser)
    {
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<EstadoMesasDto>> Handle(ObtenerEstadoMesasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📊 Obteniendo estado de mesas - Zona: {Zona}, Solo activas: {SoloActivas}, Incluir estadísticas: {IncluirEstadisticas}", 
                request.Zona ?? "Todas", request.SoloActivas, request.IncluirEstadisticas);

            // Obtener todas las mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();

            if (mesas == null || !mesas.Any())
            {
                _logger.LogWarning("⚠️ No se encontraron mesas en el sistema");
                return Result.Success(new EstadoMesasDto
                {
                    Zona = request.Zona,
                    Estadisticas = new EstadisticasMesasDto()
                });
            }

            // Filtrar por criterios
            var mesasFiltradas = mesas.AsEnumerable();

            // Filtrar por zona si se especifica
            if (!string.IsNullOrEmpty(request.Zona))
            {
                mesasFiltradas = mesasFiltradas.Where(m => m.Ubicacion.Equals(request.Zona, StringComparison.OrdinalIgnoreCase));
            }

            var listaMesas = mesasFiltradas.OrderBy(m => m.Numero).ToList();

            // Mapear a DTOs
            var mesasDto = _mapper.Map<List<MesaDto>>(listaMesas);

            // Crear el resultado
            var resultado = new EstadoMesasDto
            {
                Mesas = mesasDto,
                Zona = request.Zona,
                FechaConsulta = DateTime.Now
            };

            // Calcular estadísticas si se solicita
            if (request.IncluirEstadisticas)
            {
                resultado.Estadisticas = CalcularEstadisticas(listaMesas);
                _logger.LogDebug("📈 Estadísticas calculadas: {Disponibles} disponibles, {Ocupadas} ocupadas, {Reservadas} reservadas, {FueraServicio} fuera de servicio", 
                    resultado.Estadisticas.MesasDisponibles,
                    resultado.Estadisticas.MesasOcupadas,
                    resultado.Estadisticas.MesasReservadas,
                    resultado.Estadisticas.MesasFueraDeServicio);
            }

            _logger.LogInformation("✅ Estado de mesas obtenido correctamente: {TotalMesas} mesas, {Ocupacion}% ocupación", 
                resultado.TotalMesas, 
                resultado.Estadisticas.PorcentajeOcupacion.ToString("F1"));

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estado de mesas");
            return Result.Failure<EstadoMesasDto>("Error interno del servidor al obtener el estado de las mesas");
        }
    }

    private EstadisticasMesasDto CalcularEstadisticas(List<Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa> mesas)
    {
        var estadisticas = new EstadisticasMesasDto();

        if (!mesas.Any())
            return estadisticas;

        // Contar por estado
        estadisticas.MesasDisponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible);
        estadisticas.MesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);
        estadisticas.MesasReservadas = mesas.Count(m => m.Estado == EstadoMesa.Reservada);
        estadisticas.MesasFueraDeServicio = mesas.Count(m => m.Estado == EstadoMesa.FueraDeServicio);

        // Para las estadísticas, tratamos todas las mesas como "activas" ya que no hay propiedad Activa
        var totalMesas = mesas.Count;
        estadisticas.MesasActivas = totalMesas;
        estadisticas.MesasInactivas = 0; // No hay concepto de inactivas en la entidad actual

        // Calcular porcentajes
        if (totalMesas > 0)
        {
            var ocupadasYReservadas = estadisticas.MesasOcupadas + estadisticas.MesasReservadas;
            estadisticas.PorcentajeOcupacion = Math.Round((decimal)ocupadasYReservadas / totalMesas * 100, 2);
            estadisticas.PorcentajeDisponibilidad = Math.Round((decimal)estadisticas.MesasDisponibles / totalMesas * 100, 2);
        }

        // Calcular capacidades
        estadisticas.CapacidadTotalDisponible = mesas
            .Where(m => m.Estado == EstadoMesa.Disponible)
            .Sum(m => m.Capacidad);

        estadisticas.CapacidadTotalOcupada = mesas
            .Where(m => m.Estado == EstadoMesa.Ocupada || m.Estado == EstadoMesa.Reservada)
            .Sum(m => m.Capacidad);

        // Estadísticas por zona
        var estadisticasPorZona = mesas
            .GroupBy(m => m.Ubicacion)
            .Select(g => new Operaciones.Mesas.DTOs.EstadisticasZonaDto
            {
                Zona = g.Key,
                TotalMesas = g.Count(),
                Disponibles = g.Count(m => m.Estado == EstadoMesa.Disponible),
                Ocupadas = g.Count(m => m.Estado == EstadoMesa.Ocupada),
                Reservadas = g.Count(m => m.Estado == EstadoMesa.Reservada),
                FueraDeServicio = g.Count(m => m.Estado == EstadoMesa.FueraDeServicio),
                PorcentajeOcupacion = g.Any() ? Math.Round(
                    (decimal)g.Count(m => m.Estado == EstadoMesa.Ocupada || m.Estado == EstadoMesa.Reservada) / g.Count() * 100, 2) : 0
            })
            .OrderBy(z => z.Zona)
            .ToList();

        estadisticas.PorZona = estadisticasPorZona;

        // Nota: Como la entidad Mesa actual no tiene FechaOcupacion ni ComandaActualId,
        // las estadísticas de tiempo de ocupación no se pueden calcular con la estructura actual
        // Esto requeriría extender la entidad Mesa o consultar información de comandas

        return estadisticas;
    }
} 