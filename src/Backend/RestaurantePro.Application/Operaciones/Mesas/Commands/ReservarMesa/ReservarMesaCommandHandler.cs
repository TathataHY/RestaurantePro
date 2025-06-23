using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.ReservarMesa;

public class ReservarMesaCommandHandler : IRequestHandler<ReservarMesaCommand, Result<Unit>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IReservacionRepository _reservacionRepository;
    private readonly ILogger<ReservarMesaCommandHandler> _logger;

    public ReservarMesaCommandHandler(
        IMesaRepository mesaRepository,
        IClienteRepository clienteRepository,
        IReservacionRepository reservacionRepository,
        ILogger<ReservarMesaCommandHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _clienteRepository = clienteRepository;
        _reservacionRepository = reservacionRepository;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(ReservarMesaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando reserva de mesa {MesaId} para cliente {ClienteId}", 
            request.MesaId, request.ClienteId);

        try
        {
            // Verificar que la mesa existe
            var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId, cancellationToken);
            if (mesa == null)
            {
                _logger.LogWarning("Mesa {MesaId} no encontrada", request.MesaId);
                return Result.Failure<Unit>(new List<string> { "La mesa especificada no existe" });
            }

            // Verificar que el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure<Unit>(new List<string> { "El cliente especificado no existe" });
            }

            // Verificar disponibilidad de la mesa
            var disponible = await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                request.MesaId, request.FechaReserva, request.HoraReserva, request.DuracionMinutos, cancellationToken);
            if (!disponible)
            {
                _logger.LogWarning("Mesa {MesaId} ya está reservada para {Fecha} a las {Hora}", 
                    request.MesaId, request.FechaReserva, request.HoraReserva);
                return Result.Failure<Unit>(new List<string> { "La mesa ya está reservada para esa fecha y hora" });
            }

            // Crear la reservación usando el método de fábrica
            var reservacion = RestaurantePro.Domain.Operaciones.Reservaciones.Entities.Reservacion.Crear(
                mesa.Id,
                cliente.Id,
                request.FechaReserva,
                TimeSpan.FromMinutes(request.DuracionMinutos),
                request.NumeroPersonas,
                request.Telefono,
                request.Email,
                request.Observaciones
            );

            // Cambiar estado de la mesa a reservada
            mesa.MarcarComoReservada();

            // Guardar reservación y mesa
            await _reservacionRepository.AgregarAsync(reservacion, cancellationToken);
            await _mesaRepository.ActualizarAsync(mesa, cancellationToken);

            _logger.LogInformation("Reserva creada exitosamente para mesa {MesaId} el {Fecha} a las {Hora}", 
                request.MesaId, request.FechaReserva, request.HoraReserva);

            return Result.Success<Unit>(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error de validación al crear reserva: {Message}", ex.Message);
            return Result.Failure<Unit>(new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear reserva para mesa {MesaId}", request.MesaId);
            return Result.Failure<Unit>(new List<string> { "Error interno al procesar la reserva" });
        }
    }
} 