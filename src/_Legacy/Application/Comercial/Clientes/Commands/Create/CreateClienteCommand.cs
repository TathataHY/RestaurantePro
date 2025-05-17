using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Clientes.Commands.Create
{
    public class CreateClienteCommand : IRequest<int>
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
    }

    public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, int>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClienteCommandHandler(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = new Domain.Comercial.Clientes.Entities.Cliente
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            await _clienteRepository.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return cliente.Id;
        }
    }
} 