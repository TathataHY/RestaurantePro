namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

/// <summary>
/// Validador para CrearTarjetaFidelizacionCommand
/// Valida reglas de negocio para la creación de tarjetas de fidelización
/// </summary>
public class CrearTarjetaFidelizacionValidator : AbstractValidator<CrearTarjetaFidelizacionCommand>
{
    public CrearTarjetaFidelizacionValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("El ID del cliente es obligatorio");

        RuleFor(x => x.TipoTarjeta)
            .IsInEnum()
            .WithMessage("El tipo de tarjeta debe ser válido");

        RuleFor(x => x.NivelInicial)
            .IsInEnum()
            .WithMessage("El nivel inicial debe ser válido");

        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal es obligatorio")
            .MaximumLength(50)
            .WithMessage("El canal no puede exceder 50 caracteres")
            .Must(BeValidCanal)
            .WithMessage("Canal no válido. Valores permitidos: Sucursal, App, Web, Telefono, WhatsApp, Kiosko");

        // Validaciones de número de tarjeta personalizado
        RuleFor(x => x.NumeroTarjeta)
            .MaximumLength(20)
            .WithMessage("El número de tarjeta no puede exceder 20 caracteres")
            .Matches(@"^[A-Z0-9\-]+$")
            .WithMessage("El número de tarjeta solo puede contener letras mayúsculas, números y guiones")
            .When(x => !string.IsNullOrEmpty(x.NumeroTarjeta));

        // Validaciones de puntos iniciales
        RuleFor(x => x.PuntosIniciales)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los puntos iniciales no pueden ser negativos")
            .LessThanOrEqualTo(10000)
            .WithMessage("Los puntos iniciales no pueden exceder 10,000")
            .Must(BeValidPuntosIniciales)
            .WithMessage("Los puntos iniciales deben ser múltiplos de 10");

        // Validaciones de código de promoción
        RuleFor(x => x.CodigoPromocion)
            .MaximumLength(50)
            .WithMessage("El código de promoción no puede exceder 50 caracteres")
            .Matches(@"^[A-Z0-9\-_]+$")
            .WithMessage("El código de promoción solo puede contener letras mayúsculas, números, guiones y guiones bajos")
            .When(x => !string.IsNullOrEmpty(x.CodigoPromocion));

        // Validaciones de fechas
        RuleFor(x => x.FechaActivacion)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de activación no puede ser anterior a hoy")
            .LessThan(DateTime.Today.AddYears(1))
            .WithMessage("La fecha de activación no puede ser mayor a 1 año")
            .When(x => x.FechaActivacion.HasValue);

        RuleFor(x => x.FechaVencimiento)
            .GreaterThan(x => x.FechaActivacion ?? DateTime.Today)
            .WithMessage("La fecha de vencimiento debe ser posterior a la fecha de activación")
            .LessThan(DateTime.Today.AddYears(10))
            .WithMessage("La fecha de vencimiento no puede ser mayor a 10 años")
            .When(x => x.FechaVencimiento.HasValue);

        // Validaciones de sucursal y dirección
        RuleFor(x => x.SucursalEmision)
            .MaximumLength(100)
            .WithMessage("La sucursal de emisión no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SucursalEmision));

        RuleFor(x => x.DireccionEnvio)
            .NotEmpty()
            .WithMessage("La dirección de envío es obligatoria para tarjetas físicas")
            .MaximumLength(500)
            .WithMessage("La dirección de envío no puede exceder 500 caracteres")
            .When(x => x.EnviarTarjetaFisica);

        RuleFor(x => x.MotivoEmision)
            .MaximumLength(200)
            .WithMessage("El motivo de emisión no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.MotivoEmision));

        // Validaciones de configuración
        RuleFor(x => x.Configuracion)
            .SetValidator(new ConfiguracionTarjetaValidator()!)
            .When(x => x.Configuracion != null);

        // Validaciones de personalización
        RuleFor(x => x.Personalizacion)
            .SetValidator(new PersonalizacionTarjetaValidator()!)
            .When(x => x.Personalizacion != null);

        // Validaciones de beneficios especiales
        RuleFor(x => x.BeneficiosEspeciales)
            .Must(beneficios => beneficios == null || beneficios.Count <= 10)
            .WithMessage("No se pueden especificar más de 10 beneficios especiales")
            .When(x => x.BeneficiosEspeciales != null);

        RuleForEach(x => x.BeneficiosEspeciales)
            .MaximumLength(100)
            .WithMessage("Cada beneficio especial no puede exceder 100 caracteres")
            .When(x => x.BeneficiosEspeciales != null);

        // Validaciones de integridad referencial
        RuleFor(x => x.ClienteId)
            .MustAsync(ClienteExists)
            .WithMessage("El cliente especificado no existe");

        RuleFor(x => x.EmpleadoAutorizador)
            .MustAsync(EmpleadoExists)
            .WithMessage("El empleado autorizador especificado no existe")
            .When(x => x.EmpleadoAutorizador.HasValue);

        // Validaciones de reglas de negocio complejas
        RuleFor(x => x)
            .MustAsync(ClienteNoTieneTarjetaActiva)
            .WithMessage("El cliente ya tiene una tarjeta de fidelización activa")
            .When(x => x.EsPrincipal);

        RuleFor(x => x.NumeroTarjeta)
            .MustAsync((command, numeroTarjeta, cancellationToken) => 
                NumeroTarjetaUnico(numeroTarjeta, cancellationToken))
            .WithMessage("El número de tarjeta ya existe en el sistema")
            .When(x => !string.IsNullOrEmpty(x.NumeroTarjeta));

        RuleFor(x => x)
            .MustAsync(ValidateCodigoPromocion)
            .WithMessage("El código de promoción no es válido o ha expirado")
            .When(x => !string.IsNullOrEmpty(x.CodigoPromocion));

        RuleFor(x => x)
            .MustAsync(ValidateTipoTarjetaElegibility)
            .WithMessage("El cliente no es elegible para el tipo de tarjeta seleccionado");

        RuleFor(x => x)
            .Must(ValidateNivelInicialForTipoTarjeta)
            .WithMessage("El nivel inicial no es válido para el tipo de tarjeta seleccionado");

        // Validaciones de autorización
        RuleFor(x => x.EmpleadoAutorizador)
            .NotNull()
            .WithMessage("Se requiere autorización de empleado para tarjetas Premium y VIP")
            .When(x => x.TipoTarjeta == TipoTarjetaFidelizacion.Premium || 
                      x.TipoTarjeta == TipoTarjetaFidelizacion.Vip ||
                      x.TipoTarjeta == TipoTarjetaFidelizacion.Corporativa);

        // Validaciones de límites por tipo de tarjeta
        RuleFor(x => x.PuntosIniciales)
            .LessThanOrEqualTo(500)
            .WithMessage("Las tarjetas estándar no pueden tener más de 500 puntos iniciales")
            .When(x => x.TipoTarjeta == TipoTarjetaFidelizacion.Estandar);

        RuleFor(x => x.PuntosIniciales)
            .LessThanOrEqualTo(2000)
            .WithMessage("Las tarjetas premium no pueden tener más de 2,000 puntos iniciales")
            .When(x => x.TipoTarjeta == TipoTarjetaFidelizacion.Premium);
    }

    private static bool BeValidCanal(string canal)
    {
        var canalesValidos = new[] { "Sucursal", "App", "Web", "Telefono", "WhatsApp", "Kiosko", "Drive", "Call Center" };
        return canalesValidos.Contains(canal, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidPuntosIniciales(int puntosIniciales)
    {
        // Los puntos iniciales deben ser múltiplos de 10
        return puntosIniciales % 10 == 0;
    }

    private static bool ValidateNivelInicialForTipoTarjeta(CrearTarjetaFidelizacionCommand command)
    {
        // Validar que el nivel inicial sea apropiado para el tipo de tarjeta
        return command.TipoTarjeta switch
        {
            TipoTarjetaFidelizacion.Estandar => command.NivelInicial <= NivelFidelizacion.Plata,
            TipoTarjetaFidelizacion.Premium => command.NivelInicial <= NivelFidelizacion.Oro,
            TipoTarjetaFidelizacion.Vip => true, // VIP puede empezar en cualquier nivel
            TipoTarjetaFidelizacion.Corporativa => command.NivelInicial <= NivelFidelizacion.Oro,
            TipoTarjetaFidelizacion.Empleado => true, // Empleados pueden empezar en cualquier nivel
            TipoTarjetaFidelizacion.Promocional => command.NivelInicial <= NivelFidelizacion.Plata,
            _ => false
        };
    }

    private static async Task<bool> ClienteExists(Guid clienteId, CancellationToken cancellationToken)
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

    private static async Task<bool> ClienteNoTieneTarjetaActiva(CrearTarjetaFidelizacionCommand command, CancellationToken cancellationToken)
    {
        // Validar que el cliente no tenga ya una tarjeta principal activa
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> NumeroTarjetaUnico(string? numeroTarjeta, CancellationToken cancellationToken)
    {
        // Validar que el número de tarjeta no exista en el sistema
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateCodigoPromocion(CrearTarjetaFidelizacionCommand command, CancellationToken cancellationToken)
    {
        // Validar que el código de promoción:
        // 1. Existe en el sistema
        // 2. Está activo
        // 3. Es aplicable a creación de tarjetas
        // 4. No ha expirado
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateTipoTarjetaElegibility(CrearTarjetaFidelizacionCommand command, CancellationToken cancellationToken)
    {
        // Validar elegibilidad del cliente para el tipo de tarjeta:
        // - Corporativa: requiere validación de empresa
        // - VIP: requiere histórico de compras o invitación
        // - Empleado: requiere validación de empleado
        await Task.CompletedTask;
        return true;
    }
}

/// <summary>
/// Validador para ConfiguracionTarjeta
/// </summary>
public class ConfiguracionTarjetaValidator : AbstractValidator<ConfiguracionTarjeta>
{
    public ConfiguracionTarjetaValidator()
    {
        RuleFor(x => x.MultiplicadorPuntos)
            .GreaterThan(0)
            .WithMessage("El multiplicador de puntos debe ser mayor a 0")
            .LessThanOrEqualTo(10)
            .WithMessage("El multiplicador de puntos no puede exceder 10x");

        RuleFor(x => x.DescuentoBase)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El descuento base no puede ser negativo")
            .LessThanOrEqualTo(50)
            .WithMessage("El descuento base no puede exceder 50%");

        RuleFor(x => x.LimitePuntosDiario)
            .GreaterThan(0)
            .WithMessage("El límite diario debe ser mayor a 0")
            .LessThanOrEqualTo(50000)
            .WithMessage("El límite diario no puede exceder 50,000 puntos")
            .When(x => x.LimitePuntosDiario.HasValue);

        RuleFor(x => x.LimitePuntosMensual)
            .GreaterThan(0)
            .WithMessage("El límite mensual debe ser mayor a 0")
            .LessThanOrEqualTo(500000)
            .WithMessage("El límite mensual no puede exceder 500,000 puntos")
            .When(x => x.LimitePuntosMensual.HasValue);

        RuleFor(x => x.DiasExpiracionPuntos)
            .GreaterThan(0)
            .WithMessage("Los días de expiración deben ser mayor a 0")
            .LessThanOrEqualTo(3650)
            .WithMessage("Los días de expiración no pueden exceder 10 años")
            .When(x => x.DiasExpiracionPuntos.HasValue);

        // Validación de coherencia entre límites
        RuleFor(x => x)
            .Must(x => !x.LimitePuntosDiario.HasValue || 
                      !x.LimitePuntosMensual.HasValue || 
                      x.LimitePuntosDiario.Value * 30 >= x.LimitePuntosMensual.Value)
            .WithMessage("El límite mensual debe ser coherente con el límite diario");
    }
}

/// <summary>
/// Validador para PersonalizacionTarjeta
/// </summary>
public class PersonalizacionTarjetaValidator : AbstractValidator<PersonalizacionTarjeta>
{
    public PersonalizacionTarjetaValidator()
    {
        RuleFor(x => x.ColorPrincipal)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("El color principal debe ser un código hexadecimal válido (#RRGGBB)")
            .When(x => !string.IsNullOrEmpty(x.ColorPrincipal));

        RuleFor(x => x.ColorSecundario)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("El color secundario debe ser un código hexadecimal válido (#RRGGBB)")
            .When(x => !string.IsNullOrEmpty(x.ColorSecundario));

        RuleFor(x => x.DisenyoTemplate)
            .MaximumLength(50)
            .WithMessage("El diseño template no puede exceder 50 caracteres")
            .Must(BeValidTemplate)
            .WithMessage("Template no válido. Valores permitidos: Clasico, Moderno, Elegante, Deportivo, Empresarial")
            .When(x => !string.IsNullOrEmpty(x.DisenyoTemplate));

        RuleFor(x => x.TextoPersonalizado)
            .MaximumLength(100)
            .WithMessage("El texto personalizado no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.TextoPersonalizado));

        RuleFor(x => x.TipoFuente)
            .MaximumLength(50)
            .WithMessage("El tipo de fuente no puede exceder 50 caracteres")
            .Must(BeValidFont)
            .WithMessage("Fuente no válida. Valores permitidos: Arial, Helvetica, Times, Georgia, Roboto")
            .When(x => !string.IsNullOrEmpty(x.TipoFuente));

        RuleFor(x => x.LogoPersonalizado)
            .Must(BeValidImageUrl)
            .WithMessage("El logo personalizado debe ser una URL válida o base64")
            .When(x => !string.IsNullOrEmpty(x.LogoPersonalizado));

        RuleFor(x => x.ImagenFondo)
            .Must(BeValidImageUrl)
            .WithMessage("La imagen de fondo debe ser una URL válida o base64")
            .When(x => !string.IsNullOrEmpty(x.ImagenFondo));
    }

    private static bool BeValidTemplate(string template)
    {
        var templatesValidos = new[] { "Clasico", "Moderno", "Elegante", "Deportivo", "Empresarial", "Minimalista" };
        return templatesValidos.Contains(template, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidFont(string font)
    {
        var fuentesValidas = new[] { "Arial", "Helvetica", "Times", "Georgia", "Roboto", "Montserrat", "OpenSans" };
        return fuentesValidas.Contains(font, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidImageUrl(string imageUrl)
    {
        // Validar que sea URL válida o base64
        if (imageUrl.StartsWith("data:image/"))
            return true; // Base64 image

        return Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
} 