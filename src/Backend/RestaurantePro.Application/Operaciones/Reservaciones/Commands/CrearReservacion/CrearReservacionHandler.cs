using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Handler para crear reservaciones con validaciones de negocio completas
/// </summary>
public class CrearReservacionHandler : IRequestHandler<CrearReservacionCommand, Result<ReservacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearReservacionHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public CrearReservacionHandler(
        IApplicationDbContext context,
        IReservacionRepository reservacionRepository,
        IClienteRepository clienteRepository,
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<CrearReservacionHandler> logger,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _reservacionRepository = reservacionRepository;
        _clienteRepository = clienteRepository;
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<ReservacionDto>> Handle(CrearReservacionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📅 Iniciando creación de reservación - Cliente: {ClienteId}, Fecha: {Fecha}, Personas: {Personas}",
            request.ClienteId, request.FechaHoraReservacion, request.NumeroPersonas);

        try
        {
            // 1. Validar cliente
            var clienteResult = await ValidarCliente(request.ClienteId, cancellationToken);
            if (!clienteResult.Succeeded)
            {
                return Result.Failure<ReservacionDto>(clienteResult.Error);
            }

            // 2. Validar fecha y hora
            var validacionFechaResult = ValidarFechaHora(request.FechaHoraReservacion);
            if (!validacionFechaResult.Succeeded)
            {
                return Result.Failure<ReservacionDto>(validacionFechaResult.Error);
            }

            // 3. Verificar disponibilidad
            var disponibilidadResult = await VerificarDisponibilidad(request, cancellationToken);
            if (!disponibilidadResult.Succeeded)
            {
                return Result.Failure<ReservacionDto>(disponibilidadResult.Error);
            }

            // 4. Crear la reservación
            var reservacion = CrearReservacion(request);

            // 5. Guardar en base de datos
            await _reservacionRepository.AgregarAsync(reservacion, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 6. Mapear a DTO
            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);

            _logger.LogInformation("✅ Reservación creada exitosamente - ID: {ReservacionId}", reservacion.Id);

            return Result.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creando reservación: {Error}", ex.Message);
            return Result.Failure<ReservacionDto>($"Error al crear la reservación: {ex.Message}");
        }
    }

    private async Task<Result<Domain.Comercial.Clientes.Entities.Cliente>> ValidarCliente(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
        if (cliente == null)
        {
            return Result.Failure<Domain.Comercial.Clientes.Entities.Cliente>("Cliente no encontrado");
        }

        if (!cliente.EstaActivo)
        {
            return Result.Failure<Domain.Comercial.Clientes.Entities.Cliente>("El cliente está inactivo");
        }

        return Result.Success(cliente);
    }

    private Result ValidarFechaHora(DateTime fechaHora)
    {
        var ahora = _dateTimeService.Now;

        // Validar que no sea en el pasado
        if (fechaHora <= ahora)
        {
            return Result.Failure("La fecha y hora de la reservación no puede ser en el pasado");
        }

        // Validar que no sea más de 90 días en el futuro
        if (fechaHora > ahora.AddDays(90))
        {
            return Result.Failure("La reservación no puede ser para más de 90 días en el futuro");
        }

        // Validar horario de negocio (10:00 - 22:30)
        var hora = fechaHora.TimeOfDay;
        if (hora < TimeSpan.FromHours(10) || hora > TimeSpan.FromHours(22.5))
        {
            return Result.Failure("El horario de reservación debe estar entre las 10:00 y las 22:30");
        }

        return Result.Success();
    }

    private async Task<Result> VerificarDisponibilidad(CrearReservacionCommand request, CancellationToken cancellationToken)
    {
        // Verificar si hay mesas disponibles para el número de personas usando el repositorio de reservaciones
        var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
            request.FechaHoraReservacion.Date,
            request.FechaHoraReservacion.TimeOfDay,
            request.NumeroPersonas,
            90, // Duración por defecto en minutos
            cancellationToken);

        if (!mesasDisponibles.Any())
        {
            return Result.Failure("No hay mesas disponibles para la fecha y hora solicitadas");
        }

        // Si se especificó una mesa específica, verificar que esté disponible
        if (request.MesaEspecificaId.HasValue)
        {
            var mesaEspecifica = mesasDisponibles.FirstOrDefault(m => m == request.MesaEspecificaId.Value);
            if (mesaEspecifica == Guid.Empty)
            {
                return Result.Failure("La mesa especificada no está disponible para la fecha y hora solicitadas");
            }
        }

        return Result.Success();
    }

    private Reservacion CrearReservacion(CrearReservacionCommand request)
    {
        // Si no se especificó una mesa específica, usar la primera disponible
        var mesaId = request.MesaEspecificaId ?? Guid.Empty; // TODO: Obtener primera mesa disponible

        // Crear la reservación usando el método de fábrica del dominio
        var reservacion = Reservacion.Crear(
            mesaId: mesaId,
            clienteId: request.ClienteId,
            fecha: request.FechaHoraReservacion,
            duracionEstimada: TimeSpan.FromHours(2), // Duración por defecto
            cantidadPersonas: request.NumeroPersonas,
            telefono: request.Telefono,
            email: request.Email ?? string.Empty,
            observaciones: request.SolicitudesEspeciales
        );

        return reservacion;
    }
} 