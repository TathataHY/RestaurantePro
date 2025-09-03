using FluentValidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;

public class ObtenerMesasQueryValidator : AbstractValidator<ObtenerMesasQuery>
{
    public ObtenerMesasQueryValidator()
    {
        RuleFor(x => x.CapacidadMinima)
            .GreaterThanOrEqualTo(1).When(x => x.CapacidadMinima.HasValue)
            .WithMessage("La capacidad mínima debe ser mayor o igual a 1.");

        RuleFor(x => x.Estado)
            .MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Estado))
            .WithMessage("El estado no debe superar los 30 caracteres.")
            .Must(estado => string.IsNullOrWhiteSpace(estado) || 
                           estado.Equals("Todas", StringComparison.OrdinalIgnoreCase) ||
                           Enum.TryParse<EstadoMesa>(estado, true, out _))
            .WithMessage("El estado especificado no es válido.");

        RuleFor(x => x.Ubicacion)
            .MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Ubicacion))
            .WithMessage("La ubicación no debe superar los 30 caracteres.");
    }
} 