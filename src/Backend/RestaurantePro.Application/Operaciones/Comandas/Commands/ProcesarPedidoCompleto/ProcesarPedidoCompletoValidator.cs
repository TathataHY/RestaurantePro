namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ProcesarPedidoCompleto;

/// <summary>
/// 🔍 Validator para ProcesarPedidoCompletoCommand con validaciones de negocio complejas
/// </summary>
public class ProcesarPedidoCompletoValidator : AbstractValidator<ProcesarPedidoCompletoCommand>
{
    public ProcesarPedidoCompletoValidator()
    {
        // 🎯 Validar MeseroId
        RuleFor(x => x.MeseroId)
            .NotEmpty()
            .WithMessage("🚫 El ID del mesero es obligatorio");

        // 🎯 Validar Items del pedido
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("🚫 El pedido debe contener al menos un item")
            .Must(items => items.Count <= 50)
            .WithMessage("🚫 El pedido no puede tener más de 50 items");

        // 🎯 Validaciones para cada item
        RuleForEach(x => x.Items).SetValidator(new ItemPedidoValidator());

        // 🎯 Validar observaciones
        RuleFor(x => x.ObservacionesComanda)
            .MaximumLength(1000)
            .WithMessage("🚫 Las observaciones no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ObservacionesComanda));

        // 🎯 Validar puntos a utilizar
        RuleFor(x => x.PuntosAUtilizar)
            .GreaterThan(0)
            .WithMessage("🚫 Los puntos a utilizar deben ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("🚫 No se pueden utilizar más de 10,000 puntos en un pedido")
            .When(x => x.PuntosAUtilizar.HasValue);

        // 🎯 Validar lógica de negocio: Si hay puntos, debe haber cliente
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("🚫 Debe especificar un cliente para utilizar puntos de fidelización")
            .When(x => x.PuntosAUtilizar.HasValue && x.PuntosAUtilizar > 0);

        // 🎯 Validar lógica de negocio: Si se genera factura inmediata, debe haber cliente
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("🚫 Debe especificar un cliente para generar factura inmediata")
            .When(x => x.GenerarFacturaInmediata);
    }
}

/// <summary>
/// 🔍 Validator para ItemPedido individual
/// </summary>
public class ItemPedidoValidator : AbstractValidator<ItemPedido>
{
    public ItemPedidoValidator()
    {
        // 🎯 Validar ProductoId
        RuleFor(x => x.ProductoId)
            .NotEmpty()
            .WithMessage("🚫 El ID del producto es obligatorio");

        // 🎯 Validar ProductoNombre
        RuleFor(x => x.ProductoNombre)
            .NotEmpty()
            .WithMessage("🚫 El nombre del producto es obligatorio")
            .MaximumLength(200)
            .WithMessage("🚫 El nombre del producto no puede exceder 200 caracteres");

        // 🎯 Validar cantidad
        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("🚫 La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("🚫 No se pueden pedir más de 100 unidades del mismo producto");

        // 🎯 Validar precio unitario
        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0)
            .WithMessage("🚫 El precio unitario debe ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("🚫 El precio unitario no puede exceder $10,000");

        // 🎯 Validar observaciones del item
        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("🚫 Las observaciones del item no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        // 🎯 Validar personalizaciones si existen
        RuleForEach(x => x.Personalizaciones)
            .SetValidator(new PersonalizacionItemValidator())
            .When(x => x.Personalizaciones != null);

        // 🎯 Validar límite de personalizaciones
        RuleFor(x => x.Personalizaciones)
            .Must(p => p == null || p.Count <= 10)
            .WithMessage("🚫 No se pueden tener más de 10 personalizaciones por item");
    }
}

/// <summary>
/// 🔍 Validator para PersonalizacionItem
/// </summary>
public class PersonalizacionItemValidator : AbstractValidator<PersonalizacionItem>
{
    public PersonalizacionItemValidator()
    {
        // 🎯 Validar tipo de personalización
        RuleFor(x => x.Tipo)
            .NotEmpty()
            .WithMessage("🚫 El tipo de personalización es obligatorio")
            .Must(tipo => new[] { "Agregar", "Quitar", "Sustituir" }.Contains(tipo))
            .WithMessage("🚫 El tipo de personalización debe ser: Agregar, Quitar o Sustituir");

        // 🎯 Validar IngredienteId
        RuleFor(x => x.IngredienteId)
            .NotEmpty()
            .WithMessage("🚫 El ID del ingrediente es obligatorio");

        // 🎯 Validar IngredienteNombre
        RuleFor(x => x.IngredienteNombre)
            .NotEmpty()
            .WithMessage("🚫 El nombre del ingrediente es obligatorio")
            .MaximumLength(200)
            .WithMessage("🚫 El nombre del ingrediente no puede exceder 200 caracteres");

        // 🎯 Validar cantidad
        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("🚫 La cantidad de personalización debe ser mayor a 0")
            .LessThanOrEqualTo(10)
            .WithMessage("🚫 No se puede personalizar con más de 10 unidades");

        // 🎯 Validar precio adicional
        RuleFor(x => x.PrecioAdicional)
            .GreaterThanOrEqualTo(0)
            .WithMessage("🚫 El precio adicional no puede ser negativo")
            .LessThanOrEqualTo(500)
            .WithMessage("🚫 El precio adicional no puede exceder $500");
    }
} 