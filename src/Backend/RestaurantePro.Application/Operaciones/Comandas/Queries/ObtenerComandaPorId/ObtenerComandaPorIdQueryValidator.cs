using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

/// <summary>
/// Validator para ObtenerComandaPorIdQuery
/// </summary>
public class ObtenerComandaPorIdQueryValidator : AbstractValidator<ObtenerComandaPorIdQuery>
{
    public ObtenerComandaPorIdQueryValidator()
    {
        // 🎯 Validar Id
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("🚫 El ID de la comanda es obligatorio");

        // 🎯 Validar que el ID es un GUID válido
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("🚫 El ID de la comanda no puede estar vacío");
    }
} 