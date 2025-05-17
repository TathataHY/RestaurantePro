using FluentValidation;
using RestaurantePro.Application.Comercial.Clientes.Commands.Create;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Clientes.Validators
{
    public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
    {
        private readonly IClienteRepository _clienteRepository;

        public CreateClienteCommandValidator(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;

            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");

            RuleFor(v => v.Apellido)
                .NotEmpty().WithMessage("El apellido es requerido.")
                .MaximumLength(100).WithMessage("El apellido no puede tener más de 100 caracteres.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.")
                .MustAsync(async (email, cancellation) => 
                {
                    var clienteExistente = await _clienteRepository.GetByEmailAsync(email);
                    return clienteExistente == null;
                }).WithMessage("Ya existe un cliente con este email.");

            RuleFor(v => v.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido.")
                .Matches(@"^\d{3}-\d{3,4}-\d{4}$").WithMessage("El teléfono debe tener el formato 000-0000-0000.");
        }
    }
} 