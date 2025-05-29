namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

/// <summary>
/// Validador para ObtenerComandaPorIdQuery
/// </summary>
public class ObtenerComandaPorIdValidator : AbstractValidator<ObtenerComandaPorIdQuery>
{
    public ObtenerComandaPorIdValidator()
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la comanda no puede ser un GUID vacío");
    }
} 