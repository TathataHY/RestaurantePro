using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.EliminarTarjetaFidelizacion;

public class EliminarTarjetaFidelizacionCommandHandler : IRequestHandler<EliminarTarjetaFidelizacionCommand, Result<bool>>
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarTarjetaFidelizacionCommandHandler(ITarjetaFidelizacionRepository tarjetaRepository, IUnitOfWork unitOfWork)
    {
        _tarjetaRepository = tarjetaRepository;
        _unitOfWork = unitOfWork;
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

            // Debug: Verificar estado inicial
            Console.WriteLine($"DEBUG HANDLER: Estado inicial de tarjeta: {tarjeta.Estado}, EstaEliminado: {tarjeta.EstaEliminado}");

            // Marcar la tarjeta como eliminada (soft delete)
            tarjeta.Eliminar();
            
            // Debug: Verificar estado después de eliminar
            Console.WriteLine($"DEBUG HANDLER: Estado después de eliminar: {tarjeta.Estado}, EstaEliminado: {tarjeta.EstaEliminado}");
            
            // Actualizar la tarjeta en el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            // Debug: Verificar estado después de guardar
            Console.WriteLine($"DEBUG HANDLER: Estado después de guardar: {tarjeta.Estado}, EstaEliminado: {tarjeta.EstaEliminado}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new List<string> { $"Error al eliminar la tarjeta: {ex.Message}" });
        }
    }
} 