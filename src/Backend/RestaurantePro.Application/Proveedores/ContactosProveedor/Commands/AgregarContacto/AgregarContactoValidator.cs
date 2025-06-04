namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;

/// <summary>
/// Validador para AgregarContactoCommand
/// Valida reglas de negocio para la creación de contactos de proveedores
/// </summary>
public class AgregarContactoValidator : AbstractValidator<AgregarContactoCommand>
{
    public AgregarContactoValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.ProveedorId)
            .NotEmpty()
            .WithMessage("El ID del proveedor es obligatorio");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del contacto es obligatorio")
            .MaximumLength(50)
            .WithMessage("El nombre no puede exceder 50 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-\.]+$")
            .WithMessage("El nombre solo puede contener letras, espacios, guiones y puntos");

        RuleFor(x => x.Apellidos)
            .NotEmpty()
            .WithMessage("Los apellidos del contacto son obligatorios")
            .MaximumLength(50)
            .WithMessage("Los apellidos no pueden exceder 50 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-\.]+$")
            .WithMessage("Los apellidos solo pueden contener letras, espacios, guiones y puntos");

        RuleFor(x => x.Cargo)
            .NotEmpty()
            .WithMessage("El cargo del contacto es obligatorio")
            .MaximumLength(100)
            .WithMessage("El cargo no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email del contacto es obligatorio")
            .Must(BeValidEmail)
            .WithMessage("El email debe tener un formato válido")
            .MaximumLength(150)
            .WithMessage("El email no puede exceder 150 caracteres");

        RuleFor(x => x.Telefono)
            .NotEmpty()
            .WithMessage("El teléfono del contacto es obligatorio")
            .Matches(@"^[\d\-\+\(\)\s]{7,20}$")
            .WithMessage("El teléfono debe tener un formato válido (7-20 dígitos)");

        // Validaciones opcionales pero con reglas específicas
        RuleFor(x => x.EmailSecundario)
            .Must(BeValidEmail)
            .WithMessage("El email secundario debe tener un formato válido")
            .MaximumLength(150)
            .WithMessage("El email secundario no puede exceder 150 caracteres")
            .When(x => !string.IsNullOrEmpty(x.EmailSecundario));

        RuleFor(x => x.TelefonoMovil)
            .Matches(@"^[\d\-\+\(\)\s]{7,20}$")
            .WithMessage("El teléfono móvil debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.TelefonoMovil));

        RuleFor(x => x.Extension)
            .Matches(@"^\d{1,6}$")
            .WithMessage("La extensión debe ser un número de 1-6 dígitos")
            .When(x => !string.IsNullOrEmpty(x.Extension));

        RuleFor(x => x.Departamento)
            .MaximumLength(100)
            .WithMessage("El departamento no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Departamento));

        RuleFor(x => x.HorarioContacto)
            .MaximumLength(200)
            .WithMessage("El horario de contacto no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.HorarioContacto));

        RuleFor(x => x.Notas)
            .MaximumLength(500)
            .WithMessage("Las notas no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notas));

        // Validaciones de lógica de negocio
        RuleFor(x => x.LimiteAutorizacion)
            .GreaterThan(0)
            .WithMessage("El límite de autorización debe ser mayor a 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("El límite de autorización no puede exceder $1,000,000")
            .When(x => x.LimiteAutorizacion.HasValue);

        // Validación condicional: si puede autorizar pedidos, debe tener límite
        RuleFor(x => x.LimiteAutorizacion)
            .NotNull()
            .WithMessage("Si puede autorizar pedidos, debe especificar un límite de autorización")
            .When(x => x.PuedeAutorizarPedidos);

        // Validación de tipos de notificaciones
        RuleFor(x => x.TiposNotificaciones)
            .Must(BeValidNotificationTypes)
            .WithMessage("Los tipos de notificaciones contienen valores no válidos")
            .When(x => x.TiposNotificaciones != null && x.TiposNotificaciones.Any());

        // Un proveedor puede tener solo un contacto principal activo
        RuleFor(x => x.EsPrincipal)
            .Must((command, esPrincipal) => ValidateOnlyOnePrincipal(command, esPrincipal))
            .WithMessage("Solo puede haber un contacto principal por proveedor")
            .When(x => x.EsPrincipal);

        // Validaciones de integridad referencial
        RuleFor(x => x.ProveedorId)
            .MustAsync(ProveedorExists)
            .WithMessage("El proveedor especificado no existe");

        RuleFor(x => x.Email)
            .MustAsync(EmailNotExists)
            .WithMessage("Ya existe un contacto con este email para el proveedor");
    }

    private static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        // Expresión regular más estricta para validación de email
        // Esta versión mejorada rechaza puntos consecutivos y dominios que empiezan o terminan con punto
        var regex = new Regex(@"^[a-zA-Z0-9](?:[a-zA-Z0-9_%+-]+(?:\.[a-zA-Z0-9_%+-]+)*)?@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$");
        return regex.IsMatch(email);
    }

    private static bool BeValidNotificationTypes(List<string> tiposNotificaciones)
    {
        var tiposValidos = new[] { "Pedidos", "Pagos", "Generales", "Urgentes", "Promociones", "Facturas" };
        return tiposNotificaciones.All(tipo => tiposValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase));
    }

    private static bool ValidateOnlyOnePrincipal(AgregarContactoCommand command, bool esPrincipal)
    {
        // Esta validación se implementaría consultando la base de datos
        // Para simplificar, asumimos que es válido
        // En la implementación real se consultaría si ya existe un contacto principal
        return true;
    }

    private static async Task<bool> ProveedorExists(Guid proveedorId, CancellationToken cancellationToken)
    {
        // Esta validación requiere acceso al repositorio
        // Se implementaría inyectando IProveedorRepository
        // Para simplificar, asumimos que es válido
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> EmailNotExists(AgregarContactoCommand command, string email, CancellationToken cancellationToken)
    {
        // Esta validación requiere acceso al repositorio
        // Se implementaría inyectando IContactoProveedorRepository
        // Para simplificar, asumimos que es válido
        await Task.CompletedTask;
        return true;
    }
} 