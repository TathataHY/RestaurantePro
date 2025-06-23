using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CrearMesa;

public class CrearMesaCommand : IRequest<Result<MesaDto>>
{
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
} 