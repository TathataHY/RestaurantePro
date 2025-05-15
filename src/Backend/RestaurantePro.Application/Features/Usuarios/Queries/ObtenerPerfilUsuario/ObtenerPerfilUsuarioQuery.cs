using MediatR;
using RestaurantePro.Application.Features.Usuarios.Dtos;

namespace RestaurantePro.Application.Features.Usuarios.Queries.ObtenerPerfilUsuario
{
    public class ObtenerPerfilUsuarioQuery : IRequest<UsuarioPerfilDto>
    {
        public string UsuarioId { get; set; }
    }
} 