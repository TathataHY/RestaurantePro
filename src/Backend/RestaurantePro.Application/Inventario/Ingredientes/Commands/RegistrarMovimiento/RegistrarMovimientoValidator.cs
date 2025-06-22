using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;

public class RegistrarMovimientoValidator : AbstractValidator<RegistrarMovimientoCommand>
{
    private readonly IApplicationDbContext _context;

    public RegistrarMovimientoValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.IngredienteId)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es requerido")
            .MustAsync(IngredienteExisteAsync)
            .WithMessage("El ingrediente especificado no existe");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("La cantidad no puede exceder 10,000");

        RuleFor(x => x.TipoMovimiento)
            .IsInEnum()
            .WithMessage("El tipo de movimiento no es válido");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo del movimiento es requerido")
            .MaximumLength(200)
            .WithMessage("El motivo no puede exceder los 200 caracteres");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Observaciones))
            .WithMessage("Las observaciones no pueden exceder los 500 caracteres");

        RuleFor(x => x.UsuarioId)
            .MustAsync(UsuarioExisteAsync)
            .When(x => x.UsuarioId.HasValue)
            .WithMessage("El usuario especificado no existe");
    }

    private async Task<bool> IngredienteExisteAsync(Guid ingredienteId, CancellationToken cancellationToken)
    {
        return await _context.Ingredientes.FindAsync(new object[] { ingredienteId }, cancellationToken) != null;
    }

    private async Task<bool> UsuarioExisteAsync(Guid? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue) return true;
        return await _context.Usuarios.FindAsync(new object[] { usuarioId.Value }, cancellationToken) != null;
    }
} 