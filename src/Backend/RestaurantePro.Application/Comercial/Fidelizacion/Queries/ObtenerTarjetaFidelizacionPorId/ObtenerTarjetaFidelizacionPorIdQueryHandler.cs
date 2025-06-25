using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerTarjetaFidelizacionPorId;

/// <summary>
/// Handler para obtener una tarjeta de fidelización específica por ID
/// </summary>
public class ObtenerTarjetaFidelizacionPorIdQueryHandler : IRequestHandler<ObtenerTarjetaFidelizacionPorIdQuery, Result<TarjetaFidelizacionDto>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IClienteRepository _clienteRepository;

    public ObtenerTarjetaFidelizacionPorIdQueryHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        IClienteRepository clienteRepository)
    {
        _tarjetaRepository = tarjetaRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<TarjetaFidelizacionDto>> Handle(
        ObtenerTarjetaFidelizacionPorIdQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<TarjetaFidelizacionDto>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            var cliente = await _clienteRepository.ObtenerPorIdAsync(tarjeta.ClienteId, cancellationToken);
            
            var tarjetaDto = new TarjetaFidelizacionDto
            {
                Id = tarjeta.Id,
                NumeroTarjeta = tarjeta.Codigo,
                ClienteId = tarjeta.ClienteId,
                NombreCliente = cliente?.Nombre.NombreCompleto ?? "Cliente no encontrado",
                Nivel = tarjeta.NivelFidelizacion,
                PuntosActuales = tarjeta.PuntosDisponibles,
                TotalPuntosGanados = tarjeta.PuntosAcumulados,
                TotalPuntosCanjeados = tarjeta.PuntosAcumulados - tarjeta.PuntosDisponibles,
                FechaEmision = tarjeta.FechaEmision,
                FechaVencimiento = tarjeta.FechaExpiracion,
                Estado = tarjeta.Estado.ToString(),
                FechaUltimaActividad = tarjeta.FechaActivacion,
                Activa = tarjeta.Estado == EstadoTarjeta.Activa,
                Observaciones = null
            };

            return Result.Success(tarjetaDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<TarjetaFidelizacionDto>(new List<string> { $"Error al obtener tarjeta de fidelización: {ex.Message}" });
        }
    }
} 