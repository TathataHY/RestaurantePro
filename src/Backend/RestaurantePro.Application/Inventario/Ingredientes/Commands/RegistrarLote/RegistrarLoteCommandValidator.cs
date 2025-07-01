using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;

/// <summary>
/// Validator para el comando RegistrarLoteCommand
/// </summary>
public class RegistrarLoteCommandValidator : AbstractValidator<RegistrarLoteCommand>
{
    private readonly IApplicationDbContext _context;

    public RegistrarLoteCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Configurar validaciones
        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesNegocio();
        ConfigurarValidacionesFechaVencimiento();
    }

    /// <summary>
    /// Configura validaciones básicas de datos
    /// </summary>
    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(x => x.IngredienteId)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es requerido")
            .MustAsync(IngredienteExisteAsync)
            .WithMessage("El ingrediente especificado no existe");

        RuleFor(x => x.NumeroLote)
            .NotEmpty()
            .WithMessage("El número de lote es requerido")
            .MaximumLength(50)
            .WithMessage("El número de lote no puede exceder 50 caracteres")
            .Matches(@"^[A-Z0-9\-_]+$")
            .WithMessage("El número de lote solo puede contener letras mayúsculas, números, guiones y guiones bajos");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a cero")
            .LessThanOrEqualTo(10000)
            .WithMessage("La cantidad no puede exceder 10,000 unidades");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El precio unitario no puede ser negativo")
            .LessThanOrEqualTo(10000)
            .WithMessage("El precio unitario no puede exceder $10,000");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .MustAsync(UsuarioExisteAsync)
            .WithMessage("El usuario especificado no existe");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));
    }

    /// <summary>
    /// Configura validaciones de negocio específicas
    /// </summary>
    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(x => x)
            .MustAsync(LoteNoExisteAsync)
            .WithMessage("Ya existe un lote con el mismo número para este ingrediente")
            .WithName("LoteUnico");

        RuleFor(x => x)
            .MustAsync(StockNoExcedaMaximoAsync)
            .WithMessage("El stock resultante excedería el máximo permitido para este ingrediente")
            .WithName("StockMaximo");
    }

    /// <summary>
    /// Configura validaciones de fecha de vencimiento
    /// </summary>
    private void ConfigurarValidacionesFechaVencimiento()
    {
        RuleFor(x => x.FechaVencimiento)
            .NotEmpty()
            .WithMessage("La fecha de vencimiento es requerida")
            .GreaterThan(DateTime.Now)
            .WithMessage("La fecha de vencimiento debe ser posterior a la fecha actual")
            .LessThan(DateTime.Now.AddYears(5))
            .WithMessage("La fecha de vencimiento no puede ser más de 5 años en el futuro");

        RuleFor(x => x)
            .MustAsync(FechaVencimientoValidaParaIngredienteAsync)
            .WithMessage("La fecha de vencimiento no es válida para este tipo de ingrediente")
            .WithName("FechaVencimientoValida");
    }

    /// <summary>
    /// Valida que el ingrediente existe
    /// </summary>
    private async Task<bool> IngredienteExisteAsync(Guid ingredienteId, CancellationToken cancellationToken)
    {
        return await _context.Ingredientes.AnyAsync(i => i.Id == ingredienteId, cancellationToken);
    }

    /// <summary>
    /// Valida que el usuario existe
    /// </summary>
    private async Task<bool> UsuarioExisteAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == usuarioId, cancellationToken);
    }

    /// <summary>
    /// Valida que no existe un lote con el mismo número para el ingrediente
    /// </summary>
    private async Task<bool> LoteNoExisteAsync(RegistrarLoteCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implementar cuando tengamos tabla de lotes
        // Por ahora, asumimos que es válido
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Valida que el stock resultante no exceda el máximo
    /// </summary>
    private async Task<bool> StockNoExcedaMaximoAsync(RegistrarLoteCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        if (ingrediente == null) return false;

        var stockMaximo = ingrediente.StockMinimo * 3; // Calculado como 3x el mínimo
        var stockResultante = ingrediente.Stock + command.Cantidad;
        return stockResultante <= stockMaximo;
    }

    /// <summary>
    /// Valida que la fecha de vencimiento sea apropiada para el tipo de ingrediente
    /// </summary>
    private async Task<bool> FechaVencimientoValidaParaIngredienteAsync(RegistrarLoteCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        if (ingrediente == null) return false;

        // Validaciones específicas por tipo de ingrediente
        var diasHastaVencimiento = (command.FechaVencimiento - DateTime.Now).TotalDays;

        // Por ahora, validación básica para todos los ingredientes
        // Los ingredientes deben tener al menos 7 días de vida útil
        if (diasHastaVencimiento < 7)
        {
            return false;
        }

        // Y no más de 2 años
        if (diasHastaVencimiento > 730)
        {
            return false;
        }

        return true;
    }
} 