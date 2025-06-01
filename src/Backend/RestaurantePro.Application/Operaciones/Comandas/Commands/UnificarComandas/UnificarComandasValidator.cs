namespace RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;

/// <summary>
/// Validador para UnificarComandasCommand
/// Valida reglas de negocio para la unificación de múltiples comandas
/// </summary>
public class UnificarComandasValidator : AbstractValidator<UnificarComandasCommand>
{
    private readonly IApplicationDbContext _context;

    public UnificarComandasValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesComandas();
        ConfigurarValidacionesMesaYMesero();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        // Validación de comandas a unificar
        RuleFor(v => v.ComandasIds)
            .NotEmpty()
            .WithMessage("Debe especificar al menos una comanda para unificar.")
            .Must(comandas => comandas.Count >= 2)
            .WithMessage("Debe especificar al menos 2 comandas para unificar.")
            .Must(comandas => comandas.Count <= 10)
            .WithMessage("No se pueden unificar más de 10 comandas a la vez.")
            .Must(TenerIdsUnicos)
            .WithMessage("Los IDs de comandas deben ser únicos.");

        // Validación de mesa destino
        RuleFor(v => v.MesaDestinoId)
            .NotEmpty()
            .WithMessage("El ID de la mesa de destino es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la mesa de destino no puede ser un GUID vacío.")
            .MustAsync(MesaExiste)
            .WithMessage("La mesa de destino especificada no existe.");

        // Validación de mesero
        RuleFor(v => v.MeseroId)
            .NotEmpty()
            .WithMessage("El ID del mesero es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del mesero no puede ser un GUID vacío.")
            .MustAsync(MeseroExiste)
            .WithMessage("El mesero especificado no existe.");

        // Validación del motivo
        RuleFor(v => v.MotivoUnificacion)
            .NotEmpty()
            .WithMessage("El motivo de la unificación es requerido.")
            .MinimumLength(5)
            .WithMessage("El motivo debe tener al menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("El motivo no puede exceder 200 caracteres.");

        // Validación de estrategia de descuentos
        RuleFor(v => v.EstrategiaDescuentos)
            .IsInEnum()
            .WithMessage("La estrategia de descuentos especificada no es válida.");
    }

    private void ConfigurarValidacionesComandas()
    {
        // Validación de que todas las comandas existan
        RuleFor(v => v.ComandasIds)
            .MustAsync(TodasLasComandasExisten)
            .WithMessage("Una o más comandas especificadas no existen.");

        // Validación de que las comandas sean unificables
        RuleFor(v => v.ComandasIds)
            .MustAsync(ComandasSonUnificables)
            .WithMessage("Una o más comandas no pueden ser unificadas en su estado actual.");

        // Validación de que las comandas no estén facturadas
        RuleFor(v => v.ComandasIds)
            .MustAsync(ComandasNoEstanFacturadas)
            .WithMessage("No se pueden unificar comandas que ya han sido facturadas.");

        // Validación de comanda principal si se especifica
        RuleFor(v => v.ComandaPrincipalId)
            .MustAsync(ComandaPrincipalEsValida)
            .WithMessage("La comanda principal especificada no es válida o no está en la lista de comandas a unificar.")
            .When(v => v.ComandaPrincipalId.HasValue);
    }

    private void ConfigurarValidacionesMesaYMesero()
    {
        // Validación de disponibilidad de mesa
        RuleFor(v => v.MesaDestinoId)
            .MustAsync(MesaEstaDisponible)
            .WithMessage("La mesa de destino no está disponible.");

        // Validación de estado de mesa
        RuleFor(v => v.MesaDestinoId)
            .MustAsync(MesaEstaEnServicio)
            .WithMessage("La mesa de destino no está en servicio.");

        // Validación de mesero activo
        RuleFor(v => v.MeseroId)
            .MustAsync(MeseroEstaActivo)
            .WithMessage("El mesero especificado no está activo.");
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validaciones opcionales
        RuleFor(v => v.NotasUnificacion)
            .MaximumLength(500)
            .WithMessage("Las notas de unificación no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasUnificacion));

        RuleFor(v => v.ObservacionesUnificada)
            .MaximumLength(500)
            .WithMessage("Las observaciones de la comanda unificada no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.ObservacionesUnificada));

        // Validación de autorización
        RuleFor(v => v.AutorizadoPor)
            .NotEmpty()
            .WithMessage("Se requiere autorización para unificaciones especiales.")
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

    private static bool TenerIdsUnicos(List<Guid> comandasIds)
    {
        return comandasIds.Count == comandasIds.Distinct().Count();
    }

    private async Task<bool> MesaExiste(Guid mesaId, CancellationToken cancellationToken)
    {
        return await _context.Mesas
            .AnyAsync(m => m.Id == mesaId, cancellationToken);
    }

    private async Task<bool> MeseroExiste(Guid meseroId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Id == meseroId, cancellationToken);
    }

    private async Task<bool> TodasLasComandasExisten(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandasEncontradas = await _context.Comandas
            .Where(c => comandasIds.Contains(c.Id))
            .CountAsync(cancellationToken);

        return comandasEncontradas == comandasIds.Count;
    }

    private async Task<bool> ComandasSonUnificables(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandas = await _context.Comandas
            .Where(c => comandasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        // Solo comandas en estados específicos pueden ser unificadas
        var estadosUnificables = new[] 
        { 
            EstadoComanda.Creada, 
            EstadoComanda.EnProceso, 
            EstadoComanda.Lista 
        };

        return comandas.All(c => estadosUnificables.Contains(c.Estado));
    }

    private async Task<bool> ComandasNoEstanFacturadas(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandasFacturadas = await _context.FacturaItems
            .Where(fi => comandasIds.Contains(fi.ComandaId))
            .AnyAsync(cancellationToken);

        return !comandasFacturadas;
    }

    private async Task<bool> ComandaPrincipalEsValida(Guid? comandaPrincipalId, CancellationToken cancellationToken)
    {
        if (!comandaPrincipalId.HasValue)
            return true;

        // Verificar que la comanda principal exista y esté en la lista
        var comanda = await _context.Comandas
            .FirstOrDefaultAsync(c => c.Id == comandaPrincipalId.Value, cancellationToken);

        return comanda != null;
    }

    private async Task<bool> MesaEstaDisponible(Guid mesaId, CancellationToken cancellationToken)
    {
        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == mesaId, cancellationToken);

        if (mesa == null)
            return false;

        // La mesa puede estar ocupada pero disponible para unificación
        return mesa.Estado == EstadoMesa.Disponible || mesa.Estado == EstadoMesa.Ocupada;
    }

    private async Task<bool> MesaEstaEnServicio(Guid mesaId, CancellationToken cancellationToken)
    {
        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == mesaId, cancellationToken);

        return mesa?.Estado != EstadoMesa.FueraDeServicio;
    }

    private async Task<bool> MeseroEstaActivo(Guid meseroId, CancellationToken cancellationToken)
    {
        var mesero = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == meseroId, cancellationToken);

        return mesero?.Activo == true;
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