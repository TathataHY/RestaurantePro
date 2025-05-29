namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Validador para CrearReservacionCommand
/// Valida reglas de negocio para la creación de reservaciones en el restaurante
/// </summary>
public class CrearReservacionValidator : AbstractValidator<CrearReservacionCommand>
{
    public CrearReservacionValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-\.]+$")
            .WithMessage("El nombre solo puede contener letras, espacios, guiones y puntos");

        RuleFor(x => x.Telefono)
            .NotEmpty()
            .WithMessage("El teléfono es obligatorio")
            .Matches(@"^[\d\-\+\(\)\s]{7,20}$")
            .WithMessage("El teléfono debe tener un formato válido (7-20 dígitos)");

        RuleFor(x => x.FechaHoraReservacion)
            .NotEmpty()
            .WithMessage("La fecha y hora de reservación es obligatoria")
            .GreaterThan(DateTime.Now.AddMinutes(30))
            .WithMessage("La reservación debe ser al menos 30 minutos en el futuro")
            .LessThan(DateTime.Now.AddDays(90))
            .WithMessage("No se pueden hacer reservaciones con más de 90 días de anticipación");

        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0")
            .LessThanOrEqualTo(20)
            .WithMessage("No se pueden hacer reservaciones para más de 20 personas por mesa")
            .When(x => !x.EsRecurrente); // Para grupos grandes requieren gestión especial

        RuleFor(x => x.DuracionEstimadaMinutos)
            .GreaterThan(30)
            .WithMessage("La duración debe ser de al menos 30 minutos")
            .LessThanOrEqualTo(480)
            .WithMessage("La duración no puede exceder 8 horas");

        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal de reservación es obligatorio")
            .MaximumLength(30)
            .WithMessage("El canal no puede exceder 30 caracteres")
            .Must(BeValidCanal)
            .WithMessage("Canal no válido. Valores permitidos: Web, Telefono, App, Presencial, WhatsApp");

        // Validaciones opcionales pero con reglas específicas
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .MaximumLength(150)
            .WithMessage("El email no puede exceder 150 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TipoMesaPreferida)
            .MaximumLength(50)
            .WithMessage("El tipo de mesa no puede exceder 50 caracteres")
            .Must(BeValidTipoMesa)
            .WithMessage("Tipo de mesa no válido. Valores permitidos: Interior, Terraza, VIP, Bar, Privado")
            .When(x => !string.IsNullOrEmpty(x.TipoMesaPreferida));

        RuleFor(x => x.ZonaPreferida)
            .MaximumLength(50)
            .WithMessage("La zona preferida no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ZonaPreferida));

        RuleFor(x => x.Comentarios)
            .MaximumLength(500)
            .WithMessage("Los comentarios no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Comentarios));

        RuleFor(x => x.OcasionEspecial)
            .MaximumLength(100)
            .WithMessage("La ocasión especial no puede exceder 100 caracteres")
            .Must(BeValidOcasionEspecial)
            .WithMessage("Ocasión especial no válida. Valores sugeridos: Cumpleanos, Aniversario, Cita_Negocios, Celebracion, Romantica")
            .When(x => !string.IsNullOrEmpty(x.OcasionEspecial));

        RuleFor(x => x.PreferenciasAlimentarias)
            .MaximumLength(300)
            .WithMessage("Las preferencias alimentarias no pueden exceder 300 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PreferenciasAlimentarias));

        RuleFor(x => x.NotasInternas)
            .MaximumLength(500)
            .WithMessage("Las notas internas no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NotasInternas));

        // Validaciones de lógica de negocio
        RuleFor(x => x.MontoAnticipo)
            .GreaterThan(0)
            .WithMessage("El monto del anticipo debe ser mayor a 0")
            .LessThanOrEqualTo(5000)
            .WithMessage("El anticipo no puede exceder $5,000")
            .When(x => x.MontoAnticipo.HasValue);

        RuleFor(x => x.MetodoPagoAnticipo)
            .NotEmpty()
            .WithMessage("Si especifica anticipo, debe indicar el método de pago")
            .Must(BeValidMetodoPago)
            .WithMessage("Método de pago no válido. Valores permitidos: Efectivo, Tarjeta, Transferencia, PayPal")
            .When(x => x.MontoAnticipo.HasValue);

        // Validaciones condicionales
        RuleFor(x => x.PatronRecurrencia)
            .NotEmpty()
            .WithMessage("Para reservaciones recurrentes debe especificar el patrón de recurrencia")
            .Must(BeValidPatronRecurrencia)
            .WithMessage("Patrón de recurrencia no válido. Valores permitidos: Semanal, Quincenal, Mensual")
            .When(x => x.EsRecurrente);

        // Validaciones de horario de negocio
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeWithinBusinessHours)
            .WithMessage("La reservación debe estar dentro del horario de atención (11:00 AM - 11:00 PM)")
            .Must(NotBeOnRestDay)
            .WithMessage("No se pueden hacer reservaciones en días de descanso (verificar calendario)");

        // Validación para grupos grandes
        RuleFor(x => x.NumeroPersonas)
            .LessThanOrEqualTo(8)
            .WithMessage("Para grupos de más de 8 personas, debe contactar directamente al restaurante")
            .When(x => x.Canal.Equals("Web", StringComparison.OrdinalIgnoreCase) || 
                      x.Canal.Equals("App", StringComparison.OrdinalIgnoreCase));

        // Validación de anticipación mínima para ocasiones especiales
        RuleFor(x => x.FechaHoraReservacion)
            .GreaterThan(DateTime.Now.AddHours(24))
            .WithMessage("Las ocasiones especiales requieren al menos 24 horas de anticipación")
            .When(x => !string.IsNullOrEmpty(x.OcasionEspecial));
    }

    private static bool BeValidCanal(string canal)
    {
        var canalesValidos = new[] { "Web", "Telefono", "App", "Presencial", "WhatsApp", "Delivery" };
        return canalesValidos.Contains(canal, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidTipoMesa(string tipoMesa)
    {
        var tiposValidos = new[] { "Interior", "Terraza", "VIP", "Bar", "Privado", "Ventana", "Centro" };
        return tiposValidos.Contains(tipoMesa, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidOcasionEspecial(string ocasion)
    {
        var ocasionesValidas = new[] { "Cumpleanos", "Aniversario", "Cita_Negocios", "Celebracion", "Romantica", "Familiar", "Graduacion" };
        return ocasionesValidas.Contains(ocasion, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidMetodoPago(string metodoPago)
    {
        var metodosValidos = new[] { "Efectivo", "Tarjeta", "Transferencia", "PayPal", "Crypto" };
        return metodosValidos.Contains(metodoPago, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidPatronRecurrencia(string patron)
    {
        var patronesValidos = new[] { "Semanal", "Quincenal", "Mensual", "Diario" };
        return patronesValidos.Contains(patron, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeWithinBusinessHours(DateTime fechaHora)
    {
        // Validar horario de atención (11:00 AM - 11:00 PM)
        var hora = fechaHora.TimeOfDay;
        return hora >= TimeSpan.FromHours(11) && hora <= TimeSpan.FromHours(23);
    }

    private static bool NotBeOnRestDay(DateTime fechaHora)
    {
        // Por ahora solo validamos que no sea muy pasado, 
        // en producción aquí se consultaría el calendario de días de descanso
        return fechaHora.DayOfWeek != DayOfWeek.Monday; // Ejemplo: los lunes cerrado
    }
} 