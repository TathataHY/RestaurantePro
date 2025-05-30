namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;

/// <summary>
/// Validador para CanjearPuntosCommand
/// Valida reglas de negocio para el canje de puntos por recompensas
/// </summary>
public class CanjearPuntosValidator : AbstractValidator<CanjearPuntosCommand>
{
    public CanjearPuntosValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("El ID del cliente es obligatorio");

        RuleFor(x => x.RecompensaId)
            .NotEmpty()
            .WithMessage("El ID de la recompensa es obligatorio");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(10)
            .WithMessage("No se pueden canjear más de 10 unidades por transacción");

        RuleFor(x => x.PuntosEspecificos)
            .GreaterThan(0)
            .WithMessage("Los puntos específicos deben ser mayor a 0")
            .LessThanOrEqualTo(100000)
            .WithMessage("Los puntos específicos no pueden exceder 100,000")
            .When(x => x.PuntosEspecificos.HasValue);

        RuleFor(x => x.TipoCanje)
            .IsInEnum()
            .WithMessage("El tipo de canje debe ser válido");

        RuleFor(x => x.MetodoEntrega)
            .IsInEnum()
            .WithMessage("El método de entrega debe ser válido");

        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal es obligatorio")
            .MaximumLength(50)
            .WithMessage("El canal no puede exceder 50 caracteres")
            .Must(BeValidCanal)
            .WithMessage("Canal no válido. Valores permitidos: App, Web, Presencial, Telefono, WhatsApp");

        // Validaciones condicionales según método de entrega
        RuleFor(x => x.SucursalRecogida)
            .NotEmpty()
            .WithMessage("Debe especificar la sucursal de recogida")
            .MaximumLength(100)
            .WithMessage("La sucursal no puede exceder 100 caracteres")
            .When(x => x.MetodoEntrega == MetodoEntrega.Presencial || x.MetodoEntrega == MetodoEntrega.ProximaVisita);

        RuleFor(x => x.DireccionEntrega)
            .NotEmpty()
            .WithMessage("Debe especificar la dirección de entrega")
            .MaximumLength(500)
            .WithMessage("La dirección no puede exceder 500 caracteres")
            .When(x => x.MetodoEntrega == MetodoEntrega.Domicilio || x.MetodoEntrega == MetodoEntrega.Correo);

        // Validaciones condicionales según tipo de canje
        RuleFor(x => x.FechaPreferida)
            .NotNull()
            .WithMessage("Para canje programado debe especificar fecha preferida")
            .GreaterThan(DateTime.Now)
            .WithMessage("La fecha preferida debe ser futura")
            .LessThan(DateTime.Now.AddYears(1))
            .WithMessage("La fecha preferida no puede ser mayor a 1 año")
            .When(x => x.TipoCanje == TipoCanje.Programado);

        RuleFor(x => x.FechaPreferida)
            .GreaterThan(DateTime.Now.AddHours(-1))
            .WithMessage("La fecha preferida debe ser al menos dentro de 1 hora")
            .LessThan(DateTime.Now.AddDays(30))
            .WithMessage("La fecha preferida no puede ser mayor a 30 días")
            .When(x => x.FechaPreferida.HasValue && x.TipoCanje != TipoCanje.Programado);

        // Validaciones de campos opcionales
        RuleFor(x => x.HorarioPreferido)
            .MaximumLength(100)
            .WithMessage("El horario preferido no puede exceder 100 caracteres")
            .Matches(@"^[0-9]{1,2}:[0-9]{2}\s*(AM|PM|am|pm)?(\s*-\s*[0-9]{1,2}:[0-9]{2}\s*(AM|PM|am|pm)?)?$")
            .WithMessage("El horario debe tener formato válido (ej: 10:00 AM - 12:00 PM)")
            .When(x => !string.IsNullOrEmpty(x.HorarioPreferido));

        RuleFor(x => x.Comentarios)
            .MaximumLength(500)
            .WithMessage("Los comentarios no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Comentarios));

        RuleFor(x => x.CodigoPromocion)
            .MaximumLength(50)
            .WithMessage("El código de promoción no puede exceder 50 caracteres")
            .Matches(@"^[A-Z0-9\-_]+$")
            .WithMessage("El código de promoción solo puede contener letras mayúsculas, números, guiones y guiones bajos")
            .When(x => !string.IsNullOrEmpty(x.CodigoPromocion));

        // Validaciones de integridad referencial
        RuleFor(x => x.ClienteId)
            .MustAsync(ClienteExists)
            .WithMessage("El cliente especificado no existe");

        RuleFor(x => x.TarjetaFidelizacionId)
            .MustAsync((command, tarjetaId, cancellationToken) => 
                TarjetaFidelizacionBelongsToCliente(command, tarjetaId, cancellationToken))
            .WithMessage("La tarjeta de fidelización no pertenece al cliente especificado")
            .When(x => x.TarjetaFidelizacionId.HasValue);

        RuleFor(x => x.RecompensaId)
            .MustAsync(RecompensaExists)
            .WithMessage("La recompensa especificada no existe");

        RuleFor(x => x.EmpleadoId)
            .MustAsync(EmpleadoExists)
            .WithMessage("El empleado especificado no existe")
            .When(x => x.EmpleadoId.HasValue);

        // Validaciones de reglas de negocio complejas
        RuleFor(x => x)
            .MustAsync(ClienteHasSufficientPoints)
            .WithMessage("El cliente no tiene suficientes puntos para este canje");

        RuleFor(x => x)
            .MustAsync(RecompensaIsAvailable)
            .WithMessage("La recompensa no está disponible o ha expirado");

        RuleFor(x => x)
            .MustAsync(ValidateRecompensaStock)
            .WithMessage("No hay suficiente stock de la recompensa para la cantidad solicitada");

        RuleFor(x => x)
            .MustAsync(ValidateCanjeFrequency)
            .WithMessage("Se ha excedido la frecuencia máxima de canjes para este cliente");

        RuleFor(x => x)
            .MustAsync(ValidateMetodoEntregaForRecompensa)
            .WithMessage("El método de entrega seleccionado no es válido para esta recompensa");

        RuleFor(x => x)
            .MustAsync(ValidatePromocionCode)
            .WithMessage("El código de promoción no es válido para este canje")
            .When(x => !string.IsNullOrEmpty(x.CodigoPromocion));

        // Validaciones de límites de entrega
        RuleFor(x => x.SucursalRecogida)
            .MustAsync((command, sucursal, cancellationToken) => 
                SucursalSupportsRecompensa(command, sucursal, cancellationToken))
            .WithMessage("La sucursal seleccionada no maneja este tipo de recompensa")
            .When(x => !string.IsNullOrEmpty(x.SucursalRecogida));

        // Validación de horario de servicio
        RuleFor(x => x)
            .Must(ValidateServiceHours)
            .WithMessage("El canje está fuera del horario de servicio")
            .When(x => x.TipoCanje == TipoCanje.Inmediato);
    }

    private static bool BeValidCanal(string canal)
    {
        var canalesValidos = new[] { "App", "Web", "Presencial", "Telefono", "WhatsApp", "Kiosko", "Drive" };
        return canalesValidos.Contains(canal, StringComparer.OrdinalIgnoreCase);
    }

    private static bool ValidateServiceHours(CanjearPuntosCommand command)
    {
        // Validar que el canje inmediato esté dentro del horario de servicio
        var horaActual = DateTime.Now.TimeOfDay;
        var horaApertura = new TimeSpan(6, 0, 0);  // 6:00 AM
        var horaCierre = new TimeSpan(23, 0, 0);   // 11:00 PM

        return horaActual >= horaApertura && horaActual <= horaCierre;
    }

    private static async Task<bool> ClienteExists(Guid clienteId, CancellationToken cancellationToken)
    {
        // Validación implementada en el repositorio
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> TarjetaFidelizacionBelongsToCliente(CanjearPuntosCommand command, Guid? tarjetaId, CancellationToken cancellationToken)
    {
        // Validación implementada en el repositorio
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> RecompensaExists(Guid recompensaId, CancellationToken cancellationToken)
    {
        // Validación implementada en el repositorio
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> EmpleadoExists(Guid? empleadoId, CancellationToken cancellationToken)
    {
        // Validación implementada en el repositorio
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ClienteHasSufficientPoints(CanjearPuntosCommand command, CancellationToken cancellationToken)
    {
        // Validar que el cliente tenga suficientes puntos
        // Considerar puntos específicos o puntos requeridos por la recompensa
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> RecompensaIsAvailable(CanjearPuntosCommand command, CancellationToken cancellationToken)
    {
        // Validar que la recompensa:
        // 1. Esté activa
        // 2. No haya expirado
        // 3. Esté dentro del período de validez
        // 4. Sea elegible para el nivel del cliente
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateRecompensaStock(CanjearPuntosCommand command, CancellationToken cancellationToken)
    {
        // Validar que hay suficiente stock para la cantidad solicitada
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateCanjeFrequency(CanjearPuntosCommand command, CancellationToken cancellationToken)
    {
        // Validar límites de canje:
        // 1. Máximo número de canjes por día
        // 2. Máximo número de canjes de la misma recompensa por período
        // 3. Enfriamiento entre canjes
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateMetodoEntregaForRecompensa(CanjearPuntosCommand command, CancellationToken cancellationToken)
    {
        // Validar que el método de entrega sea compatible con el tipo de recompensa
        // Ej: recompensas digitales solo por entrega digital
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidatePromocionCode(CanjearPuntosCommand command, CancellationToken cancellationToken)
    {
        // Validar código de promoción:
        // 1. Existe y está activo
        // 2. Es aplicable a canjes
        // 3. Es válido para la recompensa seleccionada
        // 4. El cliente es elegible
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> SucursalSupportsRecompensa(CanjearPuntosCommand command, string? sucursal, CancellationToken cancellationToken)
    {
        // Validar que la sucursal puede manejar el tipo de recompensa
        await Task.CompletedTask;
        return true;
    }
} 