using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Features.Usuarios.Dtos;
using RestaurantePro.Domain.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Usuarios.Queries.ObtenerPerfilUsuario
{
    public class ObtenerPerfilUsuarioQueryHandler : IRequestHandler<ObtenerPerfilUsuarioQuery, UsuarioPerfilDto>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        public ObtenerPerfilUsuarioQueryHandler(
            IUsuarioRepository usuarioRepository,
            IMapper mapper)
        {
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<UsuarioPerfilDto> Handle(ObtenerPerfilUsuarioQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);
            
            if (usuario == null)
            {
                throw new AppException("Usuario no encontrado");
            }

            return _mapper.Map<UsuarioPerfilDto>(usuario);
        }
    }
} 