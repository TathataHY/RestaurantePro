using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.EliminarMesa;
 
public class EliminarMesaCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
} 