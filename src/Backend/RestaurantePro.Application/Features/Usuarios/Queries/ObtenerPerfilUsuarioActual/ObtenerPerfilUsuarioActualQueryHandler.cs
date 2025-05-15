using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Features.Usuarios.Dtos;
using RestaurantePro.Domain.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Usuarios.Queries.ObtenerPerfilUsuarioActual
{
    public class ObtenerPerfilUsuarioActualQueryHandler : IRequestHandler<ObtenerPerfilUsuarioActualQuery, UsuarioPerfilDto>
    {
        private readonly IUsuarioActualService _usuarioActualService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ObtenerPerfilUsuarioActualQueryHandler(
            IUsuarioActualService usuarioActualService,
            IUserService userService,
            IMapper mapper)
        {
            _usuarioActualService = usuarioActualService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<UsuarioPerfilDto> Handle(ObtenerPerfilUsuarioActualQuery request, CancellationToken cancellationToken)
        {
            if (!_usuarioActualService.EsUsuarioAutenticado())
            {
                throw new AppException("No hay un usuario autenticado");
            }

            var email = _usuarioActualService.GetUsuarioEmail();
            var usuario = await _userService.GetUserByEmailAsync(email);
            
            if (usuario == null)
            {
                throw new AppException("Usuario no encontrado");
            }

            return _mapper.Map<UsuarioPerfilDto>(usuario);
        }
    }
} 