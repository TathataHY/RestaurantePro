using FluentValidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CambiarEstadoMesa;

/// <summary>
/// Validator para CambiarEstadoMesaCommand
/// </summary>
public class CambiarEstadoMesaValidator : AbstractValidator<CambiarEstadoMesaCommand>
{
    public CambiarEstadoMesaValidator()
    {
        RuleFor(x => x.MesaId)
            .NotEmpty()
            .WithMessage("El ID de la mesa es obligatorio");

        RuleFor(x => x.NuevoEstado)
            .IsInEnum()
            .WithMessage("El estado especificado no es válido");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo es obligatorio cuando se marca una mesa como fuera de servicio")
            .When(x => x.NuevoEstado == EstadoMesa.FueraDeServicio);

        RuleFor(x => x.Motivo)
            .MaximumLength(200)
            .WithMessage("El motivo no puede exceder los 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Motivo));

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder los 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        RuleFor(x => x.UsuarioId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario no puede ser un GUID vacío")
            .When(x => x.UsuarioId.HasValue);
    }
} 