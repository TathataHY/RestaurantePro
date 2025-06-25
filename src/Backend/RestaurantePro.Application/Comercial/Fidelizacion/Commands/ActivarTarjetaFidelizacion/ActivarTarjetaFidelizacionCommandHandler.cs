using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActivarTarjetaFidelizacion;

public class ActivarTarjetaFidelizacionCommandHandler : IRequestHandler<ActivarTarjetaFidelizacionCommand, Result<TarjetaFidelizacionDto>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivarTarjetaFidelizacionCommandHandler> _logger;

    public ActivarTarjetaFidelizacionCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        IClienteRepository clienteRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivarTarjetaFidelizacionCommandHandler> logger)
    {
        _tarjetaRepository = tarjetaRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TarjetaFidelizacionDto>> Handle(ActivarTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Activando tarjeta de fidelización con ID: {TarjetaId}", request.Id);

            // Obtener la tarjeta
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (tarjeta == null)
            {
                return Result.Failure<TarjetaFidelizacionDto>("Tarjeta de fidelización no encontrada");
            }

            // Debug: Verificar estado inicial
            Console.WriteLine($"DEBUG HANDLER ACTIVAR: Estado inicial de tarjeta: {tarjeta.Estado}");

            // Verificar que la tarjeta no esté ya activa
            if (tarjeta.Estado == EstadoTarjeta.Activa)
            {
                return Result.Failure<TarjetaFidelizacionDto>("La tarjeta ya está activa");
            }

            // Activar la tarjeta
            tarjeta.Activar();

            // Debug: Verificar estado después de activar
            Console.WriteLine($"DEBUG HANDLER ACTIVAR: Estado después de activar: {tarjeta.Estado}");

            // Actualizar la tarjeta en el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            // Debug: Verificar estado después de guardar
            Console.WriteLine($"DEBUG HANDLER ACTIVAR: Estado después de guardar: {tarjeta.Estado}");

            // Obtener el cliente para el DTO
            var cliente = await _clienteRepository.ObtenerPorIdAsync(tarjeta.ClienteId, cancellationToken);
            if (cliente == null)
            {
                return Result.Failure<TarjetaFidelizacionDto>("Cliente no encontrado");
            }

            // Crear el DTO de respuesta
            var dto = new TarjetaFidelizacionDto
            {
                Id = tarjeta.Id,
                NumeroTarjeta = tarjeta.Codigo,
                ClienteId = tarjeta.ClienteId,
                NombreCliente = cliente.Nombre.NombreCompleto,
                Nivel = tarjeta.NivelFidelizacion,
                PuntosActuales = tarjeta.PuntosDisponibles,
                TotalPuntosGanados = tarjeta.PuntosAcumulados,
                TotalPuntosCanjeados = tarjeta.PuntosAcumulados - tarjeta.PuntosDisponibles,
                FechaEmision = tarjeta.FechaEmision,
                FechaVencimiento = tarjeta.FechaExpiracion,
                Estado = tarjeta.Estado.ToString(),
                FechaUltimaActividad = null, // No disponible en la entidad
                CodigoQr = null, // No disponible en la entidad
                Observaciones = null, // No disponible en la entidad
                Activa = tarjeta.Estado == EstadoTarjeta.Activa,
                BeneficiosDisponibles = new List<BeneficioDto>(),
                TransaccionesRecientes = new List<TransaccionPuntosDto>()
            };

            _logger.LogInformation("Tarjeta de fidelización activada exitosamente. ID: {TarjetaId}", tarjeta.Id);

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar tarjeta de fidelización con ID: {TarjetaId}", request.Id);
            return Result.Failure<TarjetaFidelizacionDto>("Error interno al activar la tarjeta de fidelización");
        }
    }
} 