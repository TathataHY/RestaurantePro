using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerTarjetasFidelizacion;

/// <summary>
/// Handler para obtener tarjetas de fidelización con filtros opcionales
/// </summary>
public class ObtenerTarjetasFidelizacionQueryHandler : IRequestHandler<ObtenerTarjetasFidelizacionQuery, Result<List<TarjetaFidelizacionDto>>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IClienteRepository _clienteRepository;

    public ObtenerTarjetasFidelizacionQueryHandler(
        ITarjetaFidelizacionRepository tarjetaRepository,
        IClienteRepository clienteRepository)
    {
        _tarjetaRepository = tarjetaRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<List<TarjetaFidelizacionDto>>> Handle(
        ObtenerTarjetasFidelizacionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var tarjetas = new List<TarjetaFidelizacion>();

            // Aplicar filtros según los parámetros proporcionados
            if (request.ClienteId.HasValue)
            {
                // Filtrar por cliente específico
                var tarjetasCliente = await _tarjetaRepository.ObtenerPorClienteIdAsync(request.ClienteId.Value, cancellationToken);
                tarjetas.AddRange(tarjetasCliente);
            }
            else if (request.Estado.HasValue)
            {
                // Filtrar por estado
                var tarjetasEstado = await _tarjetaRepository.ObtenerPorEstadoAsync(request.Estado.Value, cancellationToken);
                tarjetas.AddRange(tarjetasEstado);
            }
            else if (request.Nivel.HasValue)
            {
                // Filtrar por nivel
                var tarjetasNivel = await _tarjetaRepository.ObtenerPorNivelAsync(request.Nivel.Value, cancellationToken);
                tarjetas.AddRange(tarjetasNivel);
            }
            else
            {
                // Obtener todas las tarjetas (limitado para evitar sobrecarga)
                var todasTarjetas = await _tarjetaRepository.ObtenerTodosAsync(cancellationToken);
                tarjetas.AddRange(todasTarjetas);
            }

            // Aplicar paginación
            var tarjetasPaginadas = tarjetas
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Convertir a DTOs
            var tarjetasDto = new List<TarjetaFidelizacionDto>();
            foreach (var tarjeta in tarjetasPaginadas)
            {
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

                tarjetasDto.Add(tarjetaDto);
            }

            return Result.Success(tarjetasDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TarjetaFidelizacionDto>>(new List<string> { $"Error al obtener tarjetas de fidelización: {ex.Message}" });
        }
    }
} 