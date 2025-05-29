namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;

/// <summary>
/// Command para actualizar un contacto existente de un proveedor
/// Permite modificar toda la información del contacto manteniendo la integridad de datos
/// </summary>
public class ActualizarContactoCommand : IRequest<Result<ContactoProveedorDto>>
{
    /// <summary>
    /// ID del contacto a actualizar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del proveedor al que pertenece el contacto
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Nombre completo del contacto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellidos del contacto
    /// </summary>
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>
    /// Cargo o posición dentro de la empresa proveedora
    /// </summary>
    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Departamento al que pertenece
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Email principal de contacto
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Email secundario (opcional)
    /// </summary>
    public string? EmailSecundario { get; set; }

    /// <summary>
    /// Teléfono principal
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono móvil/celular
    /// </summary>
    public string? TelefonoMovil { get; set; }

    /// <summary>
    /// Extensión telefónica interna
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Indica si es el contacto principal del proveedor
    /// </summary>
    public bool EsPrincipal { get; set; } = false;

    /// <summary>
    /// Indica si tiene permisos para autorizar pedidos
    /// </summary>
    public bool PuedeAutorizarPedidos { get; set; } = false;

    /// <summary>
    /// Límite de autorización en monto para pedidos
    /// </summary>
    public decimal? LimiteAutorizacion { get; set; }

    /// <summary>
    /// Indica si puede recibir notificaciones de pedidos
    /// </summary>
    public bool RecibeNotificaciones { get; set; } = true;

    /// <summary>
    /// Tipos de notificaciones que recibe (Pedidos, Pagos, Generales, etc.)
    /// </summary>
    public List<string> TiposNotificaciones { get; set; } = new();

    /// <summary>
    /// Horario de contacto preferido
    /// </summary>
    public string? HorarioContacto { get; set; }

    /// <summary>
    /// Notas adicionales sobre el contacto
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Indica si el contacto está activo
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Datos adicionales del contacto
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    /// <summary>
    /// Motivo de la actualización (para auditoría)
    /// </summary>
    public string? MotivoActualizacion { get; set; }
} 