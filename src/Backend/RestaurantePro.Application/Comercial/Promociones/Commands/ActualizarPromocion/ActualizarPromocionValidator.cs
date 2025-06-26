using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;

public class ActualizarPromocionValidator : AbstractValidator<ActualizarPromocionCommand>
{
    private readonly IApplicationDbContext _context;

    public ActualizarPromocionValidator(IApplicationDbContext context)
    {
        _context = context;
        ConfigurarValidaciones();
    }

    private void ConfigurarValidaciones()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la promoción es obligatorio");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio");
        RuleFor(x => x.Descripcion)
            .NotEmpty().MaximumLength(500);
        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("El tipo de promoción es inválido");
        RuleFor(x => x.ValorDescuento)
            .GreaterThan(0).WithMessage("El valor de descuento debe ser mayor a cero");
        RuleFor(x => x.MontoMinimo)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.PuntosRequeridos)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.FechaInicio)
            .NotEmpty();
        RuleFor(x => x.FechaFin)
            .NotEmpty().GreaterThan(x => x.FechaInicio).WithMessage("La fecha de fin debe ser mayor a la de inicio");
        RuleFor(x => x.MaximoUsos)
            .GreaterThan(0).When(x => x.MaximoUsos.HasValue);
        RuleFor(x => x.Prioridad)
            .GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.Condiciones)
            .MaximumLength(1000);
        RuleFor(x => x.ProductosAplicablesIds)
            .Must(productos => productos == null || productos.Count <= 100)
            .WithMessage("No se pueden especificar más de 100 productos aplicables")
            .When(x => x.ProductosAplicablesIds != null);
        RuleFor(x => x.CategoriasAplicablesIds)
            .Must(categorias => categorias == null || categorias.Count <= 50)
            .WithMessage("No se pueden especificar más de 50 categorías aplicables")
            .When(x => x.CategoriasAplicablesIds != null);
        RuleFor(x => x.DiasValidos)
            .Must(dias => dias == null || dias.Count <= 7)
            .WithMessage("No se pueden especificar más de 7 días válidos")
            .When(x => x.DiasValidos != null);
    }
} 