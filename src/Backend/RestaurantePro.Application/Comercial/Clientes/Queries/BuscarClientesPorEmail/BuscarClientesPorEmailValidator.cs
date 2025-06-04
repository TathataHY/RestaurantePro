namespace RestaurantePro.Application.Comercial.Clientes.Queries.BuscarClientesPorEmail;
using System.Text.RegularExpressions;

public class BuscarClientesPorEmailValidator : AbstractValidator<BuscarClientesPorEmailQuery>
{
    private readonly string[] _camposOrdenValidos = { "Email", "Nombre", "FechaCreacion", "PuntosAcumulados", "CantidadVisitas", "Segmento" };
    private readonly string[] _segmentosValidos = { "SinClasificar", "Nuevo", "Regular", "Premium", "VIP" };

    public BuscarClientesPorEmailValidator()
    {
        // Validación de email o dominio requerido
        RuleFor(v => v)
            .Must(TenerEmailODominio)
            .WithMessage("Debe proporcionar un email o un dominio para buscar.")
            .WithName("EmailODominio");

        // Validaciones de email
        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("El email es requerido cuando no se especifica un dominio.")
            .When(v => string.IsNullOrEmpty(v.Dominio))
            .MinimumLength(3)
            .WithMessage("El email debe tener al menos 3 caracteres.")
            .MaximumLength(320)
            .WithMessage("El email no puede exceder 320 caracteres.")
            .Must(BeValidEmailFormat)
            .WithMessage("El formato del email no es válido para búsqueda.")
            .When(v => !string.IsNullOrEmpty(v.Email));

        // Validaciones de dominio
        RuleFor(v => v.Dominio)
            .NotEmpty()
            .WithMessage("El dominio es requerido cuando no se especifica un email.")
            .When(v => string.IsNullOrEmpty(v.Email))
            .MinimumLength(3)
            .WithMessage("El dominio debe tener al menos 3 caracteres.")
            .MaximumLength(100)
            .WithMessage("El dominio no puede exceder 100 caracteres.")
            .Must(BeValidDomainFormat)
            .WithMessage("El formato del dominio no es válido.")
            .When(v => !string.IsNullOrEmpty(v.Dominio));

        // Validaciones de paginación
        RuleFor(v => v.Pagina)
            .GreaterThan(0)
            .WithMessage("La página debe ser mayor a 0.")
            .LessThanOrEqualTo(1000)
            .WithMessage("La página no puede exceder 1000.");

        RuleFor(v => v.TamanoPagina)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder 100.")
            .Must(BeValidPageSize)
            .WithMessage("El tamaño de página debe ser uno de los valores permitidos: 5, 10, 20, 25, 50, 100.");

        // Validaciones de ordenamiento
        RuleFor(v => v.OrdenarPor)
            .Must(campo => string.IsNullOrEmpty(campo) || _camposOrdenValidos.Contains(campo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", _camposOrdenValidos)}.");

        RuleFor(v => v.DireccionOrden)
            .Must(direccion => direccion.Equals("asc", StringComparison.OrdinalIgnoreCase) || 
                              direccion.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("La dirección de ordenamiento debe ser 'asc' o 'desc'.");

        // Validaciones de filtros
        RuleFor(v => v.Segmento)
            .Must(segmento => string.IsNullOrEmpty(segmento) || _segmentosValidos.Contains(segmento, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El segmento debe ser uno de: {string.Join(", ", _segmentosValidos)}.")
            .When(v => !string.IsNullOrEmpty(v.Segmento));

        // Validación de lógica de búsqueda
        RuleFor(v => v.Email)
            .MinimumLength(1)
            .WithMessage("Para búsqueda exacta, el email debe estar completo.")
            .EmailAddress()
            .WithMessage("Para búsqueda exacta, el email debe tener formato válido.")
            .When(v => v.BusquedaExacta && !string.IsNullOrEmpty(v.Email));

        // Validación de eficiencia de búsqueda
        RuleFor(v => v.Email)
            .MinimumLength(2)
            .WithMessage("Para búsqueda parcial, ingrese al menos 2 caracteres para mejorar la eficiencia.")
            .When(v => !v.BusquedaExacta && !string.IsNullOrEmpty(v.Email));
    }

    private static bool TenerEmailODominio(BuscarClientesPorEmailQuery query)
    {
        return !string.IsNullOrEmpty(query.Email) || !string.IsNullOrEmpty(query.Dominio);
    }

    private static bool BeValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true; // Validación opcional

        // Para búsqueda parcial no necesitamos una validación estricta
        // ya que se trata de buscar coincidencias, no de validar formato completo
        if (email.Length < 3) return false;

        // Verificar caracteres válidos básicos para email
        var allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.-_+";
        return email.All(c => allowedChars.Contains(c));
    }

    private static bool BeValidDomainFormat(string dominio)
    {
        if (string.IsNullOrWhiteSpace(dominio)) return true; // Validación opcional

        // Formato básico de dominio
        var domainPattern = @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$";
        return System.Text.RegularExpressions.Regex.IsMatch(dominio, domainPattern);
    }

    private static bool BeValidPageSize(int pageSize)
    {
        var allowedSizes = new[] { 5, 10, 20, 25, 50, 100 };
        return allowedSizes.Contains(pageSize);
    }
} 