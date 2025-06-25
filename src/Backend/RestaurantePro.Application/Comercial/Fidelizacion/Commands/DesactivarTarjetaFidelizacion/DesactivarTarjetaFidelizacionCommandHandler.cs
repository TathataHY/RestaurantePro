using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.DesactivarTarjetaFidelizacion;

public class DesactivarTarjetaFidelizacionCommandHandler : IRequestHandler<DesactivarTarjetaFidelizacionCommand, Result<TarjetaFidelizacionDto>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DesactivarTarjetaFidelizacionCommandHandler> _logger;

    public DesactivarTarjetaFidelizacionCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        IClienteRepository clienteRepository,
        IUnitOfWork unitOfWork,
        ILogger<DesactivarTarjetaFidelizacionCommandHandler> logger)
    {
        _tarjetaRepository = tarjetaRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TarjetaFidelizacionDto>> Handle(DesactivarTarjetaFidelizacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Desactivando tarjeta de fidelización con ID: {TarjetaId}", request.Id);

            // Obtener la tarjeta
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (tarjeta == null)
            {
                return Result.Failure<TarjetaFidelizacionDto>("Tarjeta de fidelización no encontrada");
            }

            // Debug: Verificar estado inicial
            Console.WriteLine($"DEBUG HANDLER DESACTIVAR: Estado inicial de tarjeta: {tarjeta.Estado}");

            // Verificar que la tarjeta no esté ya cancelada
            if (tarjeta.Estado == EstadoTarjeta.Cancelada)
            {
                return Result.Failure<TarjetaFidelizacionDto>("La tarjeta ya está cancelada");
            }

            // Si la tarjeta está activa, suspender; si está emitida, cancelar
            if (tarjeta.Estado == EstadoTarjeta.Activa)
            {
                tarjeta.Suspender("Desactivación solicitada");
            }
            else
            {
                tarjeta.Cancelar("Desactivación solicitada");
            }

            // Debug: Verificar estado después de desactivar
            Console.WriteLine($"DEBUG HANDLER DESACTIVAR: Estado después de desactivar: {tarjeta.Estado}");

            // Actualizar la tarjeta en el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            // Debug: Verificar estado después de guardar
            Console.WriteLine($"DEBUG HANDLER DESACTIVAR: Estado después de guardar: {tarjeta.Estado}");

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

            _logger.LogInformation("Tarjeta de fidelización desactivada exitosamente. ID: {TarjetaId}", tarjeta.Id);

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar tarjeta de fidelización con ID: {TarjetaId}", request.Id);
            return Result.Failure<TarjetaFidelizacionDto>("Error interno al desactivar la tarjeta de fidelización");
        }
    }
} 