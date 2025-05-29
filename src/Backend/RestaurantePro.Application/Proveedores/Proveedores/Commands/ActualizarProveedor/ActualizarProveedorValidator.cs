namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;

public class ActualizarProveedorValidator : AbstractValidator<ActualizarProveedorCommand>
{
    public ActualizarProveedorValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del proveedor es obligatorio");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del proveedor es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\u00f1\u00d1\s\-\.&]+$")
            .WithMessage("El nombre solo puede contener letras, espacios, guiones, puntos y ampersand");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es obligatorio")
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .MaximumLength(100)
            .WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Telefono)
            .Matches(@"^[\+]?[0-9\s\-\(\)]{8,20}$")
            .WithMessage("El teléfono debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.Telefono));

        RuleFor(x => x.Direccion)
            .MaximumLength(200)
            .WithMessage("La dirección no puede exceder 200 caracteres");

        RuleFor(x => x.PaginaWeb)
            .Must(BeValidUrl)
            .WithMessage("La página web debe tener un formato de URL válido")
            .When(x => !string.IsNullOrEmpty(x.PaginaWeb));

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("El tipo de proveedor no es válido");

        RuleFor(x => x.CondicionesPago)
            .IsInEnum()
            .WithMessage("Las condiciones de pago no son válidas");

        RuleFor(x => x.DiasEntrega)
            .GreaterThan(0)
            .WithMessage("Los días de entrega deben ser mayor a 0")
            .LessThanOrEqualTo(365)
            .WithMessage("Los días de entrega no pueden exceder 365 días");

        RuleFor(x => x.Calificacion)
            .IsInEnum()
            .WithMessage("La calificación no es válida");

        RuleFor(x => x.Notas)
            .MaximumLength(1000)
            .WithMessage("Las notas no pueden exceder 1000 caracteres");
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
} 