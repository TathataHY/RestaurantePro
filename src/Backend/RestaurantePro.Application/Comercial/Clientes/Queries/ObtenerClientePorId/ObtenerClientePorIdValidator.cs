namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Validador para ObtenerClientePorIdQuery
/// </summary>
public class ObtenerClientePorIdValidator : AbstractValidator<ObtenerClientePorIdQuery>
{
    public ObtenerClientePorIdValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del cliente no puede ser un GUID vacío");
    }
} 