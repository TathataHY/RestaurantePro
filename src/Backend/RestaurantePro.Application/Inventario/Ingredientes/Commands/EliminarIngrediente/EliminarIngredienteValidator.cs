using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.EliminarIngrediente;

public class EliminarIngredienteValidator : AbstractValidator<EliminarIngredienteCommand>
{
    private readonly IApplicationDbContext _context;

    public EliminarIngredienteValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es requerido")
            .MustAsync(IngredienteExisteAsync)
            .WithMessage("El ingrediente especificado no existe");
    }

    private async Task<bool> IngredienteExisteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Ingredientes.FindAsync(new object[] { id }, cancellationToken) != null;
    }
} 