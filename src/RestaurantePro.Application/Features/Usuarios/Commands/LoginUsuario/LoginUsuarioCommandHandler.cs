using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Features.Usuarios.Dtos;
using RestaurantePro.Domain.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Usuarios.Commands.LoginUsuario
{
    public class LoginUsuarioCommandHandler : IRequestHandler<LoginUsuarioCommand, UsuarioAutenticadoDto>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IUserService _userService;

        public LoginUsuarioCommandHandler(
            IIdentityService identityService,
            IJwtGenerator jwtGenerator,
            IUserService userService)
        {
            _identityService = identityService;
            _jwtGenerator = jwtGenerator;
            _userService = userService;
        }

        public async Task<UsuarioAutenticadoDto> Handle(LoginUsuarioCommand request, CancellationToken cancellationToken)
        {
            var (success, userId, message) = await _identityService.ValidateUserAsync(request.Email, request.Password);

            if (!success)
            {
                throw new AppException(message);
            }

            var user = await _userService.GetUserAsync(userId);
            
            // Generar token JWT
            var token = _jwtGenerator.GenerateToken(user);

            return new UsuarioAutenticadoDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                Rol = user.Rol,
                Token = token
            };
        }
    }
} 