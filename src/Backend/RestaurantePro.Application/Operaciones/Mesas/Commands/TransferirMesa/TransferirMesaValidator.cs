namespace RestaurantePro.Application.Operaciones.Mesas.Commands.TransferirMesa;

/// <summary>
/// Validador para TransferirMesaCommand
/// Valida reglas de negocio para la transferencia de comandas entre mesas
/// </summary>
public class TransferirMesaValidator : AbstractValidator<TransferirMesaCommand>
{
    private readonly IApplicationDbContext _context;

    public TransferirMesaValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesComandas();
        ConfigurarValidacionesMesas();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        // Validación de comanda
        RuleFor(v => v.ComandaId)
            .NotEmpty()
            .WithMessage("El ID de la comanda es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la comanda no puede ser un GUID vacío.")
            .MustAsync(ComandaExiste)
            .WithMessage("La comanda especificada no existe.");

        // Validación de mesa origen
        RuleFor(v => v.MesaOrigenId)
            .NotEmpty()
            .WithMessage("El ID de la mesa de origen es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la mesa de origen no puede ser un GUID vacío.")
            .MustAsync(MesaExiste)
            .WithMessage("La mesa de origen especificada no existe.");

        // Validación de mesa destino
        RuleFor(v => v.MesaDestinoId)
            .NotEmpty()
            .WithMessage("El ID de la mesa de destino es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la mesa de destino no puede ser un GUID vacío.")
            .MustAsync(MesaExiste)
            .WithMessage("La mesa de destino especificada no existe.");

        // Validación que las mesas sean diferentes
        RuleFor(v => v.MesaDestinoId)
            .NotEqual(v => v.MesaOrigenId)
            .WithMessage("La mesa de destino debe ser diferente a la mesa de origen.");

        // Validación del motivo
        RuleFor(v => v.MotivoTransferencia)
            .NotEmpty()
            .WithMessage("El motivo de la transferencia es requerido.")
            .MinimumLength(5)
            .WithMessage("El motivo debe tener al menos 5 caracteres.")
            .MaximumLength(200)
            .WithMessage("El motivo no puede exceder 200 caracteres.");
    }

    private void ConfigurarValidacionesComandas()
    {
        // Validación de que la comanda pertenece a la mesa origen
        RuleFor(v => v)
            .MustAsync(ComandaPerteneceAMesaOrigen)
            .WithMessage("La comanda no pertenece a la mesa de origen especificada.")
            .WithName("ConsistenciaMesaComanda");

        // Validación de estado de comanda
        RuleFor(v => v.ComandaId)
            .MustAsync(ComandaEsTransferible)
            .WithMessage("La comanda no puede ser transferida en su estado actual.");
    }

    private void ConfigurarValidacionesMesas()
    {
        // Validación de disponibilidad de mesa destino
        RuleFor(v => v.MesaDestinoId)
            .MustAsync(MesaDestinoEstaDisponible)
            .WithMessage("La mesa de destino no está disponible.");

        // Validación de capacidad de mesa destino
        RuleFor(v => v)
            .MustAsync(MesaDestinoTieneCapacidadSuficiente)
            .WithMessage("La mesa de destino no tiene capacidad suficiente para la comanda.")
            .WithName("CapacidadMesa");

        // Validación de estado de mesas
        RuleFor(v => v.MesaOrigenId)
            .MustAsync(MesaEstaEnServicio)
            .WithMessage("La mesa de origen no está en servicio.");

        RuleFor(v => v.MesaDestinoId)
            .MustAsync(MesaEstaEnServicio)
            .WithMessage("La mesa de destino no está en servicio.");
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validaciones opcionales
        RuleFor(v => v.NotasTransferencia)
            .MaximumLength(500)
            .WithMessage("Las notas de transferencia no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasTransferencia));

        // Validación de autorización
        RuleFor(v => v.AutorizadoPor)
            .NotEmpty()
            .WithMessage("Se requiere autorización para transferencias especiales.")
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

    private async Task<bool> MesaExiste(Guid mesaId, CancellationToken cancellationToken)
    {
        return await _context.Mesas
            .AnyAsync(m => m.Id == mesaId, cancellationToken);
    }

    private async Task<bool> ComandaPerteneceAMesaOrigen(TransferirMesaCommand command, CancellationToken cancellationToken)
    {
        var comanda = await _context.Comandas
            .FirstOrDefaultAsync(c => c.Id == command.ComandaId, cancellationToken);

        return comanda?.MesaId == command.MesaOrigenId;
    }

    private async Task<bool> ComandaEsTransferible(Guid comandaId, CancellationToken cancellationToken)
    {
        var comanda = await _context.Comandas
            .FirstOrDefaultAsync(c => c.Id == comandaId, cancellationToken);

        if (comanda == null)
            return false;

        // Solo comandas en estados específicos pueden ser transferidas
        var estadosTransferibles = new[] 
        { 
            EstadoComanda.Creada, 
            EstadoComanda.EnProceso, 
            EstadoComanda.Lista 
        };

        return estadosTransferibles.Contains(comanda.Estado);
    }

    private async Task<bool> MesaDestinoEstaDisponible(Guid mesaDestinoId, CancellationToken cancellationToken)
    {
        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == mesaDestinoId, cancellationToken);

        if (mesa == null)
            return false;

        // Verificar que la mesa no tenga comandas activas
        var tieneComandaActiva = await _context.Comandas
            .AnyAsync(c => c.MesaId == mesaDestinoId && 
                          c.Estado != EstadoComanda.Finalizada && 
                          c.Estado != EstadoComanda.Cancelada, 
                     cancellationToken);

        return !tieneComandaActiva;
    }

    private async Task<bool> MesaDestinoTieneCapacidadSuficiente(TransferirMesaCommand command, CancellationToken cancellationToken)
    {
        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == command.MesaDestinoId, cancellationToken);

        var comanda = await _context.Comandas
            .FirstOrDefaultAsync(c => c.Id == command.ComandaId, cancellationToken);

        if (mesa == null || comanda == null)
            return false;

        // Asumir que la capacidad de la mesa es suficiente si es >= al número de personas de la comanda
        // TODO: Implementar lógica específica cuando esté disponible NumeroPersonas en Comanda
        return mesa.Capacidad >= 1; // Temporalmente permitir si tiene capacidad > 0
    }

    private async Task<bool> MesaEstaEnServicio(Guid mesaId, CancellationToken cancellationToken)
    {
        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == mesaId, cancellationToken);

        return mesa?.Estado == EstadoMesa.Disponible || mesa?.Estado == EstadoMesa.Ocupada;
    }

    private async Task<bool> UsuarioEsValido(Guid? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue)
            return false;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId.Value, cancellationToken);

        // TODO: Cambiar por la propiedad correcta cuando esté disponible en Usuario
        // return usuario?.EstaActivo == true;
        return usuario != null; // Temporal: asumimos que si existe, está activo
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