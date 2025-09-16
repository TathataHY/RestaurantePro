using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CrearMesa;

public class CrearMesaCommand : IRequest<Result<MesaDto>>
{
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Disponible";
    public string? Descripcion { get; set; }
    public string? Notas { get; set; }
    public bool TieneVentana { get; set; }
    public bool TieneSofa { get; set; }
    public bool EsAccesible { get; set; }
    public bool TieneEnchufe { get; set; }
} 