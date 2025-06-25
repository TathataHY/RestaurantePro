using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.EliminarTarjetaFidelizacion;

public class EliminarTarjetaFidelizacionCommandHandler : IRequestHandler<EliminarTarjetaFidelizacionCommand, Result<bool>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;

    public EliminarTarjetaFidelizacionCommandHandler(ITarjetaFidelizacionRepository tarjetaRepository)
    {
        _tarjetaRepository = tarjetaRepository;
    }

    public async Task<Result<bool>> Handle(
        EliminarTarjetaFidelizacionCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener la tarjeta existente
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            
            if (tarjeta == null)
            {
                return Result.Failure<bool>(new List<string> { "Tarjeta de fidelización no encontrada" });
            }

            // Eliminar la tarjeta (soft delete)
            await _tarjetaRepository.EliminarAsync(request.Id, cancellationToken);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new List<string> { $"Error al eliminar la tarjeta: {ex.Message}" });
        }
    }
} 