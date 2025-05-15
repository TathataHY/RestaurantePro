using MediatR;
using RestaurantePro.Application.Features.Usuarios.Dtos;

namespace RestaurantePro.Application.Features.Usuarios.Queries.ObtenerPerfilUsuarioActual
{
    public class ObtenerPerfilUsuarioActualQuery : IRequest<UsuarioPerfilDto>
    {
    }
} 