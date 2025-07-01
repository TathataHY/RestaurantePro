using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;

/// <summary>
/// Validator para el comando ConsumirStockCommand
/// </summary>
public class ConsumirStockCommandValidator : AbstractValidator<ConsumirStockCommand>
{
    private readonly IApplicationDbContext _context;

    public ConsumirStockCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Configurar validaciones
        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesNegocio();
        ConfigurarValidacionesStock();
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

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .MustAsync(UsuarioExisteAsync)
            .WithMessage("El usuario especificado no existe");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad a consumir debe ser mayor a cero")
            .LessThanOrEqualTo(10000)
            .WithMessage("La cantidad a consumir no puede exceder 10,000 unidades");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo del consumo es requerido")
            .MaximumLength(200)
            .WithMessage("El motivo no puede exceder 200 caracteres")
            .Matches(@"^[a-zA-Z0-9\s\-_.,]+$")
            .WithMessage("El motivo solo puede contener letras, números, espacios, guiones, guiones bajos, puntos y comas");

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
            .MustAsync(IngredienteEstaActivoAsync)
            .WithMessage("No se puede consumir stock de un ingrediente inactivo")
            .WithName("IngredienteActivo");

        RuleFor(x => x)
            .MustAsync(StockSuficienteAsync)
            .WithMessage("No hay suficiente stock disponible para consumir la cantidad especificada")
            .WithName("StockSuficiente");

        RuleFor(x => x)
            .MustAsync(NoExcederStockDisponibleAsync)
            .WithMessage("El consumo excedería el stock disponible")
            .WithName("StockDisponible");
    }

    /// <summary>
    /// Configura validaciones específicas de stock
    /// </summary>
    private void ConfigurarValidacionesStock()
    {
        RuleFor(x => x)
            .MustAsync(StockNoCriticoAsync)
            .WithMessage("No se puede consumir stock cuando está en nivel crítico")
            .WithName("StockNoCritico")
            .When(x => x.Cantidad > 0);

        RuleFor(x => x)
            .MustAsync(ValidarConsumoPorTipoIngredienteAsync)
            .WithMessage("El consumo no es válido para este tipo de ingrediente")
            .WithName("TipoIngrediente");
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
    /// Valida que el ingrediente está activo
    /// </summary>
    private async Task<bool> IngredienteEstaActivoAsync(ConsumirStockCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        return ingrediente?.EstaActivo ?? false;
    }

    /// <summary>
    /// Valida que hay suficiente stock disponible
    /// </summary>
    private async Task<bool> StockSuficienteAsync(ConsumirStockCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        if (ingrediente == null) return false;

        return ingrediente.Stock >= command.Cantidad;
    }

    /// <summary>
    /// Valida que no se exceda el stock disponible
    /// </summary>
    private async Task<bool> NoExcederStockDisponibleAsync(ConsumirStockCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        if (ingrediente == null) return false;

        var stockRestante = ingrediente.Stock - command.Cantidad;
        return stockRestante >= 0;
    }

    /// <summary>
    /// Valida que el stock no esté en nivel crítico
    /// </summary>
    private async Task<bool> StockNoCriticoAsync(ConsumirStockCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        if (ingrediente == null) return false;

        // No permitir consumo si el stock está en nivel crítico (menos del 50% del mínimo)
        var stockCritico = ingrediente.StockMinimo * 0.5m;
        return ingrediente.Stock > stockCritico;
    }

    /// <summary>
    /// Valida el consumo según el tipo de ingrediente
    /// </summary>
    private async Task<bool> ValidarConsumoPorTipoIngredienteAsync(ConsumirStockCommand command, CancellationToken cancellationToken)
    {
        var ingrediente = await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == command.IngredienteId, cancellationToken);

        if (ingrediente == null) return false;

        // Validaciones específicas por tipo de ingrediente
        switch (ingrediente.Rotacion)
        {
            case Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta:
                // Para ingredientes de alta rotación, permitir consumo más flexible
                return command.Cantidad <= ingrediente.Stock * 0.8m; // Máximo 80% del stock disponible

            case Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media:
                // Para ingredientes de rotación media, limitar consumo
                return command.Cantidad <= ingrediente.Stock * 0.6m; // Máximo 60% del stock disponible

            case Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Baja:
                // Para ingredientes de baja rotación, consumo muy limitado
                return command.Cantidad <= ingrediente.Stock * 0.3m; // Máximo 30% del stock disponible

            default:
                return true;
        }
    }
} 