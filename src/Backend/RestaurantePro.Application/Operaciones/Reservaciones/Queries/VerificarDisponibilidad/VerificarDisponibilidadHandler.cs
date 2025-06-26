using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.VerificarDisponibilidad;

/// <summary>
/// Handler para verificar disponibilidad de reservaciones
/// </summary>
public class VerificarDisponibilidadHandler : IRequestHandler<VerificarDisponibilidadQuery, Result<DisponibilidadDto>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VerificarDisponibilidadHandler> _logger;

    public VerificarDisponibilidadHandler(
        IReservacionRepository reservacionRepository,
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<VerificarDisponibilidadHandler> logger)
    {
        _reservacionRepository = reservacionRepository;
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<DisponibilidadDto>> Handle(
        VerificarDisponibilidadQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Verificando disponibilidad para {Fecha} a las {Hora} - {NumeroPersonas} personas", 
            request.Fecha.ToShortDateString(), 
            request.Hora.ToString(@"hh\:mm"), 
            request.NumeroPersonas);

        try
        {
            // Obtener todas las mesas disponibles
            var mesasDisponibles = await _mesaRepository.ObtenerMesasDisponiblesAsync();
            
            if (!mesasDisponibles.Any())
            {
                _logger.LogWarning("⚠️ No hay mesas disponibles en el sistema");
                return Result<DisponibilidadDto>.Success(new DisponibilidadDto
                {
                    Fecha = request.Fecha,
                    Hora = request.Hora,
                    NumeroPersonas = request.NumeroPersonas,
                    Disponible = false,
                    Mensaje = "No hay mesas disponibles en el sistema",
                    MesasDisponibles = new List<MesaDisponibleDto>(),
                    Alternativas = new List<AlternativaDto>()
                });
            }

            // Obtener reservaciones existentes para la fecha y hora
            var reservacionesExistentes = await _reservacionRepository.ObtenerPorFechaAsync(request.Fecha, cancellationToken);
            
            _logger.LogInformation("🔍 Encontradas {Count} reservaciones para la fecha {Fecha}", 
                reservacionesExistentes.Count(), request.Fecha.ToShortDateString());
            
            // Calcular el rango de la consulta
            var inicioConsulta = request.Hora;
            var duracionConsulta = request.DuracionEstimada ?? TimeSpan.FromHours(2);
            var finConsulta = inicioConsulta.Add(duracionConsulta);

            _logger.LogInformation("⏰ Rango de consulta: {Inicio} - {Fin} (duración: {Duracion})", 
                inicioConsulta.ToString(@"hh\:mm"), 
                finConsulta.ToString(@"hh\:mm"), 
                duracionConsulta.ToString(@"hh\:mm"));

            // Filtrar reservaciones activas que se solapan con la hora consultada
            var reservacionesActivas = reservacionesExistentes
                .Where(r =>
                    // inicioA < finB && finA > inicioB
                    r.Hora < finConsulta && r.Hora.Add(r.DuracionEstimada) > inicioConsulta &&
                    (r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada))
                .ToList();

            _logger.LogInformation("🎯 Encontradas {Count} reservaciones activas que se solapan", reservacionesActivas.Count);
            foreach (var r in reservacionesActivas)
            {
                _logger.LogInformation("   - Mesa {MesaId}: {Hora} - {Fin} (Estado: {Estado})", 
                    r.MesaId, 
                    r.Hora.ToString(@"hh\:mm"), 
                    r.Hora.Add(r.DuracionEstimada).ToString(@"hh\:mm"),
                    r.Estado);
            }

            // Excluir la reservación actual si se está modificando
            if (request.ReservacionId.HasValue)
            {
                reservacionesActivas = reservacionesActivas
                    .Where(r => r.Id != request.ReservacionId.Value)
                    .ToList();
            }

            // Obtener IDs de mesas ocupadas
            var mesasOcupadas = reservacionesActivas.Select(r => r.MesaId).ToHashSet();

            _logger.LogInformation("🚫 Mesas ocupadas: {MesasOcupadas}", string.Join(", ", mesasOcupadas));

            // Filtrar mesas disponibles que tengan capacidad suficiente
            var mesasAdecuadas = mesasDisponibles
                .Where(m => !mesasOcupadas.Contains(m.Id) && m.Capacidad >= request.NumeroPersonas)
                .OrderBy(m => m.Capacidad) // Priorizar mesas más pequeñas
                .ToList();

            _logger.LogInformation("✅ Mesas adecuadas después de filtrar: {Count} mesas", mesasAdecuadas.Count);
            foreach (var m in mesasAdecuadas)
            {
                _logger.LogInformation("   - Mesa {MesaId} (Capacidad: {Capacidad})", m.Id, m.Capacidad);
            }

            // Verificar disponibilidad
            var disponible = mesasAdecuadas.Any();

            // Mapear mesas disponibles a DTOs
            var mesasDisponibilidadDto = _mapper.Map<List<MesaDisponibleDto>>(mesasAdecuadas);

            // Generar horarios alternativos si no hay disponibilidad
            var horariosAlternativos = new List<AlternativaDto>();
            if (!disponible)
            {
                // Generar alternativas para las próximas 2 horas
                for (int i = 1; i <= 4; i++)
                {
                    var horaAlternativa = request.Hora.Add(TimeSpan.FromMinutes(30 * i));
                    if (horaAlternativa.Hours < 23) // No generar horarios después de las 23:00
                    {
                        horariosAlternativos.Add(new AlternativaDto
                        {
                            Fecha = request.Fecha,
                            Hora = horaAlternativa,
                            NumeroPersonas = request.NumeroPersonas,
                            Disponible = true, // Asumimos que está disponible
                            FechaHora = request.Fecha.Add(horaAlternativa),
                            CantidadMesasDisponibles = 1,
                            MejorOpcion = i == 1,
                            DiferenciaMinutos = 30 * i
                        });
                    }
                }
            }

            var resultado = new DisponibilidadDto
            {
                Disponible = disponible,
                Mensaje = disponible 
                    ? $"Hay {mesasAdecuadas.Count} mesa(s) disponible(s) para {request.NumeroPersonas} personas"
                    : "No hay mesas disponibles para el horario solicitado",
                MesasDisponibles = mesasDisponibilidadDto,
                Alternativas = horariosAlternativos,
                Fecha = request.Fecha,
                Hora = request.Hora,
                NumeroPersonas = request.NumeroPersonas,
                HayDisponibilidad = disponible,
                FechaHoraConsultada = request.Fecha.Add(request.Hora),
                NumeroPersonasSolicitadas = request.NumeroPersonas
            };

            _logger.LogInformation("✅ Verificación completada - Disponible: {Disponible}, Mesas: {MesasDisponibles}, Alternativas: {Alternativas}", 
                disponible, 
                mesasDisponibilidadDto.Count, 
                horariosAlternativos.Count);

            return Result.Success(resultado);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🚫 Operación cancelada al verificar disponibilidad");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al verificar disponibilidad");
            return Result.Failure<DisponibilidadDto>("Error interno del servidor al verificar disponibilidad");
        }
    }

    private List<TimeSpan> GenerarHorariosAlternativos(TimeSpan horaSolicitada, DateTime fecha, IEnumerable<Domain.Operaciones.Reservaciones.Entities.Reservacion> reservacionesExistentes)
    {
        var horariosAlternativos = new List<TimeSpan>();
        var horaBase = new TimeSpan(12, 0, 0); // 12:00 PM
        var horaFin = new TimeSpan(22, 0, 0);  // 10:00 PM
        var intervalo = TimeSpan.FromMinutes(30); // Intervalos de 30 minutos

        for (var hora = horaBase; hora <= horaFin; hora += intervalo)
        {
            // Saltar la hora solicitada
            if (hora == horaSolicitada)
                continue;

            // Verificar si hay disponibilidad en esta hora
            var reservacionesEnHora = reservacionesExistentes
                .Where(r => r.Hora == hora && 
                           (r.Estado == EstadoReservacion.Pendiente || 
                            r.Estado == EstadoReservacion.Confirmada))
                .ToList();

            // Si hay menos de 3 reservaciones en esta hora, considerarla como alternativa
            if (reservacionesEnHora.Count < 3)
            {
                horariosAlternativos.Add(hora);
            }

            // Limitar a 5 alternativas
            if (horariosAlternativos.Count >= 5)
                break;
        }

        return horariosAlternativos;
    }
} 