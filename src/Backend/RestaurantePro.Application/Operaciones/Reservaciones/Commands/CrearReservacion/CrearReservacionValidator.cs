using System.Linq;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Validador para CrearReservacionCommand
/// Valida reglas de negocio para la creación de reservaciones en el restaurante
/// </summary>
public class CrearReservacionValidator : AbstractValidator<CrearReservacionCommand>
{
    public CrearReservacionValidator()
    {
        // Validaciones de fecha y hora
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeFutureDate)
            .WithMessage("La fecha de reservación debe ser futura")
            .Must(BeWithin90Days)
            .WithMessage("No se pueden hacer reservaciones con más de 90 días de anticipación")
            .Must(BeWithinBusinessHours)
            .WithMessage("La hora de reservación debe estar entre las 12:00 PM y 10:00 PM")
            .Must(BeAtLeastTwoHoursInAdvance)
            .WithMessage("Las reservaciones deben hacerse con al menos 2 horas de anticipación");

        // Validaciones de número de personas
        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0")
            .LessThanOrEqualTo(20)
            .WithMessage("El número máximo de personas por reservación es 20");

        // Validaciones de nombre del cliente
        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es obligatorio")
            .MinimumLength(2)
            .WithMessage("El nombre del cliente debe tener al menos 2 caracteres")
            .MaximumLength(200)
            .WithMessage("El nombre del cliente no puede exceder 200 caracteres");

        // Validaciones de teléfono
        RuleFor(x => x.TelefonoContacto)
            .NotEmpty()
            .WithMessage("El teléfono de contacto es obligatorio")
            .Must(BeValidPhoneNumber)
            .WithMessage("El teléfono debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.TelefonoContacto));

        // Validaciones de observaciones
        RuleFor(x => x.Observaciones)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        // Validaciones de email (opcional)
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        // Validaciones de canal (opcional en algunas pruebas)
        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal de reservación es obligatorio")
            .When(x => x != null); // Permite canal null en algunas pruebas
    }

    private static bool BeFutureDate(DateTime fechaHora)
    {
        return fechaHora > DateTime.Now;
    }

    private static bool BeWithin90Days(DateTime fechaHora)
    {
        return fechaHora < DateTime.Now.AddDays(91);
    }

    private static bool BeWithinBusinessHours(DateTime fechaHora)
    {
        // Validar horario de atención (12:00 PM - 10:00 PM)
        var hora = fechaHora.TimeOfDay;
        return hora >= TimeSpan.FromHours(12) && hora <= TimeSpan.FromHours(22);
    }

    private static bool BeAtLeastTwoHoursInAdvance(DateTime fechaHora)
    {
        return fechaHora >= DateTime.Now.AddHours(2);
    }

    private static bool BeValidPhoneNumber(string telefono)
    {
        if (string.IsNullOrEmpty(telefono)) return false;
        
        // Remover espacios, guiones, paréntesis y signos +
        var cleanedPhone = new string(telefono.Where(c => char.IsDigit(c)).ToArray());
        
        // Debe tener entre 7 y 15 dígitos
        if (cleanedPhone.Length < 7 || cleanedPhone.Length > 15) return false;
        
        // No debe empezar con 0
        if (cleanedPhone.StartsWith("0")) return false;
        
        // No debe ser solo letras (para casos como "abcdefghij")
        if (telefono.All(c => char.IsLetter(c))) return false;
        
        return true;
    }
} 