using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActualizarTarjetaFidelizacion;

/// <summary>
/// Handler para actualizar una tarjeta de fidelización existente
/// </summary>
public class ActualizarTarjetaFidelizacionCommandHandler : IRequestHandler<ActualizarTarjetaFidelizacionCommand, Result<TarjetaFidelizacionDto>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IClienteRepository _clienteRepository;

    public ActualizarTarjetaFidelizacionCommandHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        IClienteRepository clienteRepository)
    {
        _tarjetaRepository = tarjetaRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<TarjetaFidelizacionDto>> Handle(
        ActualizarTarjetaFidelizacionCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener la tarjeta existente usando un método que retorne null si no existe
            var tarjeta = await _tarjetaRepository.ObtenerPorIdSinExcepcionAsync(request.Id, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<TarjetaFidelizacionDto>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            // Verificar que el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(tarjeta.ClienteId, cancellationToken);
            if (cliente == null)
            {
                return Result.Failure<TarjetaFidelizacionDto>(new List<string> { "Cliente asociado a la tarjeta no encontrado" });
            }

            // Log de debug para verificar los valores
            Console.WriteLine($"DEBUG: Nivel actual: {tarjeta.NivelFidelizacion}, Nuevo nivel solicitado: {request.Nivel}");
            Console.WriteLine($"DEBUG: Multiplicador actual: {tarjeta.MultiplicadorPuntos}, Nuevo multiplicador: {request.MultiplicadorPuntos}");

            // Actualizar las propiedades de la tarjeta usando los métodos de la entidad
            tarjeta.ActualizarNivel(request.Nivel);
            tarjeta.ConfigurarMultiplicadorPuntos(request.MultiplicadorPuntos);
            
            if (request.LimiteMensual.HasValue)
            {
                tarjeta.ConfigurarLimiteMensual(request.LimiteMensual.Value);
            }

            // Log de debug después de la actualización
            Console.WriteLine($"DEBUG: Nivel después de actualizar: {tarjeta.NivelFidelizacion}");
            Console.WriteLine($"DEBUG: Multiplicador después de actualizar: {tarjeta.MultiplicadorPuntos}");

            // Guardar los cambios
            await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);

            // Mapear a DTO
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
                FechaUltimaActividad = tarjeta.FechaActualizacion,
                Activa = tarjeta.Estado == RestaurantePro.Domain.Comercial.Clientes.Enums.EstadoTarjeta.Activa,
                Observaciones = request.Observaciones,
                BeneficiosDisponibles = new List<BeneficioDto>(),
                TransaccionesRecientes = new List<TransaccionPuntosDto>()
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<TarjetaFidelizacionDto>(new List<string> { $"Error al actualizar la tarjeta de fidelización: {ex.Message}" });
        }
    }
} 