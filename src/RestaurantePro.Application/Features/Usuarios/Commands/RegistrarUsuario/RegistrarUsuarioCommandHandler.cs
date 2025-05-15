using MediatR;
using RestaurantePro.Domain.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Usuarios.Commands.RegistrarUsuario
{
    public class RegistrarUsuarioCommandHandler : IRequestHandler<RegistrarUsuarioCommand, int>
    {
        private readonly IIdentityService _identityService;

        public RegistrarUsuarioCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<int> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.CreateUserAsync(
                request.Nombre,
                request.Apellido,
                request.Email,
                request.Password,
                request.Rol
            );

            if (!result.Success)
            {
                throw new System.Exception(result.Message);
            }

            return 1; // Indicar que se creó correctamente
        }
    }
} 