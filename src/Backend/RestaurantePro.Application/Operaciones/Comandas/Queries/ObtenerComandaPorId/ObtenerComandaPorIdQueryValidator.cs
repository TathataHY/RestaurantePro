using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

/// <summary>
/// Validator para ObtenerComandaPorIdQuery
/// </summary>
public class ObtenerComandaPorIdQueryValidator : AbstractValidator<ObtenerComandaPorIdQuery>
{
    public ObtenerComandaPorIdQueryValidator()
    {
        // 🎯 Validar ComandaId
        RuleFor(x => x.ComandaId)
            .NotEmpty()
            .WithMessage("🚫 El ID de la comanda es obligatorio");

        // 🎯 Validar que el ComandaId es un GUID válido
        RuleFor(x => x.ComandaId)
            .Must(id => id != Guid.Empty)
            .WithMessage("🚫 El ID de la comanda no puede estar vacío");
    }
} 