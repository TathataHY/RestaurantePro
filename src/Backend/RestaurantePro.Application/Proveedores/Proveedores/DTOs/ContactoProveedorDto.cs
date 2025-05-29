namespace RestaurantePro.Application.Proveedores.Proveedores.DTOs;

/// <summary>
/// DTO para ContactoProveedor
/// Representa los contactos de un proveedor
/// </summary>
public class ContactoProveedorDto
{
    /// <summary>
    /// ID del contacto
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del proveedor al que pertenece
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Nombre del contacto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Cargo del contacto en la empresa
    /// </summary>
    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del contacto
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Email del contacto
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del contacto
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    // === PROPIEDADES CALCULADAS PARA UI ===

    /// <summary>
    /// Información completa del contacto formateada
    /// </summary>
    public string InformacionCompleta => $"{Nombre} ({Cargo}) - {Email} - {Telefono}";

    /// <summary>
    /// Iniciales del contacto para avatares
    /// </summary>
    public string Iniciales => string.Join("", Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(n => n[0])).ToUpper();

    /// <summary>
    /// Tooltip informativo del contacto
    /// </summary>
    public string TooltipInfo => $"👤 {Nombre}\n💼 {Cargo}\n📧 {Email}\n📞 {Telefono}";
} 