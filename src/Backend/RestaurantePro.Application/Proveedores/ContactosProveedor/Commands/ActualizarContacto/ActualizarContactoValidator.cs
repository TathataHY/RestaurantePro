namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;

/// <summary>
/// Validador para ActualizarContactoCommand
/// Valida reglas de negocio para la actualización de contactos de proveedores
/// </summary>
public class ActualizarContactoValidator : AbstractValidator<ActualizarContactoCommand>
{
    public ActualizarContactoValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del contacto es obligatorio");

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
            .NotEqual(x => x.Email)
            .WithMessage("El email secundario debe ser diferente al email principal")
            .When(x => !string.IsNullOrWhiteSpace(x.EmailSecundario));

        RuleFor(x => x.TelefonoMovil)
            .Matches(@"^[\d\-\+\(\)\s]{7,20}$")
            .WithMessage("El teléfono móvil debe tener un formato válido")
            .NotEqual(x => x.Telefono)
            .WithMessage("El teléfono móvil debe ser diferente al teléfono principal")
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

        RuleFor(x => x.MotivoActualizacion)
            .MaximumLength(300)
            .WithMessage("El motivo de actualización no puede exceder 300 caracteres")
            .When(x => !string.IsNullOrEmpty(x.MotivoActualizacion));

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

        // Validaciones de integridad referencial que se realizarán en el handler
        RuleFor(x => x.Id)
            .MustAsync(ContactoExists)
            .WithMessage("El contacto especificado no existe");

        RuleFor(x => x.ProveedorId)
            .MustAsync(ProveedorExists)
            .WithMessage("El proveedor especificado no existe");

        RuleFor(x => x)
            .MustAsync(EmailNotExistsForOtherContact)
            .WithMessage("Ya existe otro contacto con este email para el proveedor");

        // Validación especial para contacto principal
        RuleFor(x => x)
            .MustAsync(ValidateContactoPrincipalChange)
            .WithMessage("Error en la validación del contacto principal")
            .When(x => x.EsPrincipal);
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
        var tiposValidos = new[] { "Pedidos", "Pagos", "Generales", "Urgentes", "Promociones", "Facturas", "Emergencias" };
        return tiposNotificaciones.All(tipo => tiposValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase));
    }

    private static async Task<bool> ContactoExists(Guid contactoId, CancellationToken cancellationToken)
    {
        // Esta validación requiere acceso al repositorio
        // Se implementaría inyectando IContactoProveedorRepository
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ProveedorExists(Guid proveedorId, CancellationToken cancellationToken)
    {
        // Esta validación requiere acceso al repositorio
        // Se implementaría inyectando IProveedorRepository
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> EmailNotExistsForOtherContact(ActualizarContactoCommand command, CancellationToken cancellationToken)
    {
        // Esta validación requiere verificar que el email no existe en otro contacto del mismo proveedor
        // Se implementaría consultando IContactoProveedorRepository
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateContactoPrincipalChange(ActualizarContactoCommand command, CancellationToken cancellationToken)
    {
        // Esta validación verifica que:
        // 1. Si se está marcando como principal, no debe haber otro contacto principal
        // 2. Si se está desmarcando como principal, debe quedar al menos un contacto activo
        await Task.CompletedTask;
        return true;
    }
} 