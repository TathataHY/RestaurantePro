namespace RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;

/// <summary>
/// Validador para DividirComandaCommand
/// Valida reglas de negocio para la división de comandas en múltiples comandas
/// </summary>
public class DividirComandaValidator : AbstractValidator<DividirComandaCommand>
{
    private readonly IApplicationDbContext _context;

    public DividirComandaValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesComanda();
        ConfigurarValidacionesDivision();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        // Validación de comanda original
        RuleFor(v => v.ComandaOriginalId)
            .NotEmpty()
            .WithMessage("El ID de la comanda original es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la comanda original no puede ser un GUID vacío.")
            .MustAsync(ComandaExiste)
            .WithMessage("La comanda original especificada no existe.");

        // Validación del tipo de división
        RuleFor(v => v.TipoDivision)
            .IsInEnum()
            .WithMessage("El tipo de división especificado no es válido.");

        // Validación del motivo
        RuleFor(v => v.MotivoDivision)
            .NotEmpty()
            .WithMessage("El motivo de la división es requerido.")
            .MinimumLength(5)
            .WithMessage("El motivo debe tener al menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("El motivo no puede exceder 200 caracteres.");

        // Validación de divisiones
        RuleFor(v => v.DivisionItems)
            .NotEmpty()
            .WithMessage("Debe especificar al menos una división de items.")
            .Must(divisionItems => divisionItems.Count >= 1)
            .WithMessage("Debe crear al menos 1 comanda nueva.")
            .Must(divisionItems => divisionItems.Count <= 10)
            .WithMessage("No se pueden crear más de 10 comandas nuevas.");
    }

    private void ConfigurarValidacionesComanda()
    {
        // Validación de estado de comanda
        RuleFor(v => v.ComandaOriginalId)
            .MustAsync(ComandaEsDivisible)
            .WithMessage("La comanda no puede ser dividida en su estado actual.");

        // Validación de que la comanda tenga items
        RuleFor(v => v.ComandaOriginalId)
            .MustAsync(ComandaTieneItems)
            .WithMessage("La comanda debe tener items para poder ser dividida.");

        // Validación de que la comanda no esté facturada
        RuleFor(v => v.ComandaOriginalId)
            .MustAsync(ComandaNoEstaFacturada)
            .WithMessage("No se puede dividir una comanda que ya ha sido facturada.");
    }

    private void ConfigurarValidacionesDivision()
    {
        // Validaciones para cada división
        RuleForEach(v => v.DivisionItems)
            .SetValidator(new DivisionComandaDtoValidator(_context));

        // Validación de números únicos de comanda
        RuleFor(v => v.DivisionItems)
            .Must(TenerNumerosComandaUnicos)
            .WithMessage("Los números de comanda nueva deben ser únicos.");

        // Validación de distribución completa de items
        RuleFor(v => v)
            .MustAsync(TodosLosItemsEstanDistribuidos)
            .WithMessage("Todos los items de la comanda original deben ser distribuidos.")
            .WithName("DistribucionCompleta");

        // Validación de cantidades consistentes
        RuleFor(v => v)
            .MustAsync(CantidadesConsistentes)
            .WithMessage("Las cantidades distribuidas no pueden exceder las cantidades originales.")
            .WithName("CantidadesConsistentes");
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validaciones opcionales
        RuleFor(v => v.NotasDivision)
            .MaximumLength(500)
            .WithMessage("Las notas de división no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasDivision));

        // Validación de autorización
        RuleFor(v => v.AutorizadoPor)
            .NotEmpty()
            .WithMessage("Se requiere autorización para divisiones especiales.")
            .MustAsync(UsuarioEsValido)
            .WithMessage("El usuario autorizador no es válido.")
            .When(v => v.AutorizadoPor.HasValue);

        // Validación de datos adicionales
        RuleFor(v => v.DatosAdicionales)
            .Must(DatosAdicionalesValidos)
            .WithMessage("Los datos adicionales contienen información inválida.")
            .When(v => v.DatosAdicionales != null && v.DatosAdicionales.Any());
    }

    #region Métodos de validación privados

    private async Task<bool> ComandaExiste(Guid comandaId, CancellationToken cancellationToken)
    {
        return await _context.Comandas
            .AnyAsync(c => c.Id == comandaId, cancellationToken);
    }

    private async Task<bool> ComandaEsDivisible(Guid comandaId, CancellationToken cancellationToken)
    {
        var comanda = await _context.Comandas
            .FirstOrDefaultAsync(c => c.Id == comandaId, cancellationToken);

        if (comanda == null)
            return false;

        // Solo comandas en estados específicos pueden ser divididas
        var estadosDivisibles = new[] 
        { 
            EstadoComanda.Creada, 
            EstadoComanda.EnProceso 
        };

        return estadosDivisibles.Contains(comanda.Estado);
    }

    private async Task<bool> ComandaTieneItems(Guid comandaId, CancellationToken cancellationToken)
    {
        return await _context.ItemsComanda
            .AnyAsync(i => i.ComandaId == comandaId, cancellationToken);
    }

    private async Task<bool> ComandaNoEstaFacturada(Guid comandaId, CancellationToken cancellationToken)
    {
        return !await _context.FacturaItems
            .AnyAsync(fi => fi.ComandaId == comandaId, cancellationToken);
    }

    private static bool TenerNumerosComandaUnicos(List<DivisionComandaDto> divisionItems)
    {
        var numeros = divisionItems.Select(d => d.NumeroComandaNueva).ToList();
        return numeros.Count == numeros.Distinct().Count();
    }

    private async Task<bool> TodosLosItemsEstanDistribuidos(DividirComandaCommand command, CancellationToken cancellationToken)
    {
        var itemsOriginales = await _context.ItemsComanda
            .Where(i => i.ComandaId == command.ComandaOriginalId)
            .ToListAsync(cancellationToken);

        var itemsDistribuidos = command.DivisionItems
            .SelectMany(d => d.Items)
            .GroupBy(i => i.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

        // Si se mantiene la comanda original, no es necesario distribuir todos los items
        if (command.MantenerComandaOriginal)
            return true;

        // Verificar que todos los items originales estén incluidos en la distribución
        foreach (var itemOriginal in itemsOriginales)
        {
            if (!itemsDistribuidos.ContainsKey(itemOriginal.Id))
                return false;
        }

        return true;
    }

    private async Task<bool> CantidadesConsistentes(DividirComandaCommand command, CancellationToken cancellationToken)
    {
        var itemsOriginales = await _context.ItemsComanda
            .Where(i => i.ComandaId == command.ComandaOriginalId)
            .ToDictionaryAsync(i => i.Id, i => i.Cantidad, cancellationToken);

        var itemsDistribuidos = command.DivisionItems
            .SelectMany(d => d.Items)
            .GroupBy(i => i.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

        // Verificar que las cantidades distribuidas no excedan las originales
        foreach (var kvp in itemsDistribuidos)
        {
            if (!itemsOriginales.ContainsKey(kvp.Key))
                continue;

            var cantidadOriginal = itemsOriginales[kvp.Key];
            var cantidadDistribuida = kvp.Value;

            if (cantidadDistribuida > cantidadOriginal)
                return false;
        }

        return true;
    }

    private async Task<bool> UsuarioEsValido(Guid? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue)
            return false;

        return await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId.Value && u.Activo, cancellationToken);
    }

    private static bool DatosAdicionalesValidos(Dictionary<string, object> datosAdicionales)
    {
        // Validar que no haya demasiados datos adicionales
        if (datosAdicionales.Count > 10)
            return false;

        // Validar que las claves no sean muy largas
        return datosAdicionales.All(kvp => 
            !string.IsNullOrEmpty(kvp.Key) && 
            kvp.Key.Length <= 50);
    }

    #endregion
}

/// <summary>
/// Validador para DivisionComandaDto
/// </summary>
public class DivisionComandaDtoValidator : AbstractValidator<DivisionComandaDto>
{
    private readonly IApplicationDbContext _context;

    public DivisionComandaDtoValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.NumeroComandaNueva)
            .GreaterThan(0)
            .WithMessage("El número de comanda nueva debe ser mayor a 0.");

        RuleFor(v => v.Items)
            .NotEmpty()
            .WithMessage("Cada división debe tener al menos un item.");

        RuleForEach(v => v.Items)
            .SetValidator(new ItemDivisionDtoValidator());

        RuleFor(v => v.MesaDestinoId)
            .MustAsync(MesaEsValida)
            .WithMessage("La mesa de destino especificada no es válida.")
            .When(v => v.MesaDestinoId.HasValue);

        RuleFor(v => v.MeseroId)
            .MustAsync(MeseroEsValido)
            .WithMessage("El mesero especificado no es válido.")
            .When(v => v.MeseroId.HasValue);

        RuleFor(v => v.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Observaciones));
    }

    private async Task<bool> MesaEsValida(Guid? mesaId, CancellationToken cancellationToken)
    {
        if (!mesaId.HasValue)
            return true;

        return await _context.Mesas
            .AnyAsync(m => m.Id == mesaId.Value && m.Estado != EstadoMesa.FueraDeServicio, cancellationToken);
    }

    private async Task<bool> MeseroEsValido(Guid? meseroId, CancellationToken cancellationToken)
    {
        if (!meseroId.HasValue)
            return true;

        return await _context.Usuarios
            .AnyAsync(u => u.Id == meseroId.Value && u.Activo, cancellationToken);
    }
}

/// <summary>
/// Validador para ItemDivisionDto
/// </summary>
public class ItemDivisionDtoValidator : AbstractValidator<ItemDivisionDto>
{
    public ItemDivisionDtoValidator()
    {
        RuleFor(v => v.ItemId)
            .NotEmpty()
            .WithMessage("El ID del item es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del item no puede ser un GUID vacío.");

        RuleFor(v => v.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("La cantidad no puede exceder 100 unidades por item.");

        RuleFor(v => v.ObservacionesItem)
            .MaximumLength(200)
            .WithMessage("Las observaciones del item no pueden exceder 200 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.ObservacionesItem));
    }
} 