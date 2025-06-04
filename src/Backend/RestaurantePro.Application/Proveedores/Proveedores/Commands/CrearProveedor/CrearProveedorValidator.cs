namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;

/// <summary>
/// Validador para CrearProveedorCommand
/// Implementa validaciones de negocio específicas para proveedores
/// </summary>
public class CrearProveedorValidator : AbstractValidator<CrearProveedorCommand>
{
    public CrearProveedorValidator()
    {
        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesContacto();
        ConfigurarValidacionesUbicacion();
        ConfigurarValidacionesFiscales();
        ConfigurarValidacionesNegocio();
    }

    /// <summary>
    /// Configura validaciones básicas obligatorias
    /// </summary>
    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del proveedor es obligatorio")
            .DependentRules(() => {
                RuleFor(x => x.Nombre)
                    .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres")
                    .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
                    .Must(BeValidCompanyName).WithMessage("El nombre contiene caracteres no válidos");
            });

        RuleFor(x => x.NombreContacto)
            .MaximumLength(100).WithMessage("El nombre del contacto no puede exceder 100 caracteres")
            .MinimumLength(2).WithMessage("El nombre del contacto debe tener al menos 2 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.NombreContacto));

        RuleFor(x => x.UsuarioId)
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío")
            .When(x => x.UsuarioId != Guid.Empty);
    }

    /// <summary>
    /// Configura validaciones de información de contacto
    /// </summary>
    private void ConfigurarValidacionesContacto()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .DependentRules(() => {
                RuleFor(x => x.Email)
                    .Must(BeValidEmail).WithMessage("El formato del email no es válido")
                    .MaximumLength(254).WithMessage("El email no puede exceder 254 caracteres");
            });

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio")
            .DependentRules(() => {
                RuleFor(x => x.Telefono)
                    .Must(BeValidPhoneNumber).WithMessage("El teléfono debe contener solo números, espacios, guiones y paréntesis");
            });
    }

    /// <summary>
    /// Configura validaciones de ubicación
    /// </summary>
    private void ConfigurarValidacionesUbicacion()
    {
        RuleFor(x => x.Direccion)
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Direccion));

        RuleFor(x => x.Ciudad)
            .MaximumLength(100).WithMessage("La ciudad no puede exceder 100 caracteres")
            .MinimumLength(2).WithMessage("La ciudad debe tener al menos 2 caracteres")
            .Must(BeValidCityName).WithMessage("La ciudad contiene caracteres no válidos")
            .When(x => !string.IsNullOrWhiteSpace(x.Ciudad));

        RuleFor(x => x.Pais)
            .MaximumLength(50).WithMessage("El país no puede exceder 50 caracteres")
            .Must(BeValidCountryName).WithMessage("El país contiene caracteres no válidos")
            .When(x => !string.IsNullOrWhiteSpace(x.Pais));

        RuleFor(x => x.CodigoPostal)
            .MaximumLength(10).WithMessage("El código postal no puede exceder 10 caracteres")
            .Must(BeValidPostalCode).WithMessage("El código postal debe contener solo números y letras")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoPostal));
    }

    /// <summary>
    /// Configura validaciones fiscales
    /// </summary>
    private void ConfigurarValidacionesFiscales()
    {
        // RFC es opcional, pero si se proporciona debe ser válido
        RuleFor(x => x.RFC)
            .Must(BeValidRFC).WithMessage("El RFC no tiene un formato válido")
            .When(x => !string.IsNullOrWhiteSpace(x.RFC));

        RuleFor(x => x.InformacionBancaria)
            .MaximumLength(500).WithMessage("La información bancaria no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.InformacionBancaria));
    }

    /// <summary>
    /// Configura validaciones de negocio
    /// </summary>
    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(x => x.DiasCredito)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de crédito no pueden ser negativos")
            .LessThanOrEqualTo(180).WithMessage("Los días de crédito no pueden exceder 180 días");

        // Validación condicional: si hay días de crédito > 0, debe haber información bancaria
        RuleFor(x => x.InformacionBancaria)
            .NotEmpty().WithMessage("La información bancaria es obligatoria para proveedores con crédito")
            .When(x => x.DiasCredito > 0);

        // Validación para proveedores internacionales
        RuleFor(x => x.DiasCredito)
            .LessThanOrEqualTo(90).WithMessage("Para proveedores internacionales, el crédito máximo es 90 días")
            .When(x => !string.IsNullOrWhiteSpace(x.Pais) && !x.Pais.Equals("México", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Valida que el nombre de la empresa sea válido
    /// </summary>
    private bool BeValidCompanyName(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return false;
        
        // Permitir letras, números, espacios y algunos caracteres especiales comunes en nombres de empresas
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s\.\-&,()_'""]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(nombre, allowedChars);
    }

    /// <summary>
    /// Valida que el número de teléfono sea válido
    /// </summary>
    private bool BeValidPhoneNumber(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return false;
        
        // Permitir números, espacios, guiones, paréntesis y el símbolo +
        var phonePattern = @"^[\d\s\-\(\)\+]+$";
        var isValidFormat = System.Text.RegularExpressions.Regex.IsMatch(telefono, phonePattern);
        
        // Verificar que tenga al menos 10 dígitos
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(telefono, @"[^\d]", "");
        var hasMinDigits = digitsOnly.Length >= 10;
        
        return isValidFormat && hasMinDigits;
    }

    /// <summary>
    /// Valida que el formato del email sea válido
    /// </summary>
    private bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        // Expresión regular más estricta para validación de email
        // Esta versión mejorada rechaza puntos consecutivos y dominios que empiezan o terminan con punto
        var regex = new Regex(@"^[a-zA-Z0-9](?:[a-zA-Z0-9_%+-]+(?:\.[a-zA-Z0-9_%+-]+)*)?@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$");
        return regex.IsMatch(email);
    }

    /// <summary>
    /// Valida que el RFC sea válido (mexicano o genérico)
    /// </summary>
    private bool BeValidRFC(string rfc)
    {
        if (string.IsNullOrWhiteSpace(rfc)) return true; // Es opcional
        
        // RFC para personas físicas: 4 letras + 6 números + 3 alfanuméricos
        // RFC para personas morales: 3 letras + 6 números + 3 alfanuméricos
        var rfcPattern = @"^[A-Z&Ñ]{3,4}[0-9]{6}[A-Z0-9]{3}$";
        return System.Text.RegularExpressions.Regex.IsMatch(rfc.ToUpper(), rfcPattern);
    }

    /// <summary>
    /// Valida que el nombre de ciudad sea válido
    /// </summary>
    private bool BeValidCityName(string ciudad)
    {
        if (string.IsNullOrWhiteSpace(ciudad)) return false;
        
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\.\-']+$";
        return System.Text.RegularExpressions.Regex.IsMatch(ciudad, allowedChars);
    }

    /// <summary>
    /// Valida que el nombre del país sea válido
    /// </summary>
    private bool BeValidCountryName(string pais)
    {
        if (string.IsNullOrWhiteSpace(pais)) return false;
        
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\.\-']+$";
        return System.Text.RegularExpressions.Regex.IsMatch(pais, allowedChars);
    }

    /// <summary>
    /// Valida que el código postal sea válido
    /// </summary>
    private bool BeValidPostalCode(string codigoPostal)
    {
        if (string.IsNullOrWhiteSpace(codigoPostal)) return true; // Es opcional
        
        var allowedChars = @"^[a-zA-Z0-9\s\-]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, allowedChars);
    }
} 