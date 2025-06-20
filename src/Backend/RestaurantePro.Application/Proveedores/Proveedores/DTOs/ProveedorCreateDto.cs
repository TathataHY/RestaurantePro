namespace RestaurantePro.Application.Proveedores.Proveedores.DTOs;

/// <summary>
/// DTO para crear un nuevo proveedor
/// Contiene los campos necesarios para el registro
/// </summary>
public class ProveedorCreateDto
{
    /// <summary>
    /// Nombre del proveedor (obligatorio)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del contacto principal (obligatorio)
    /// </summary>
    public string NombreContacto { get; set; } = string.Empty;

    /// <summary>
    /// Email del proveedor (obligatorio)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del proveedor (obligatorio)
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del proveedor (obligatorio)
    /// </summary>
    public string Direccion { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad del proveedor (obligatorio)
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// Código postal (opcional)
    /// </summary>
    public string CodigoPostal { get; set; } = string.Empty;

    /// <summary>
    /// País (opcional, por defecto Chile)
    /// </summary>
    public string Pais { get; set; } = "Chile";

    /// <summary>
    /// RFC del proveedor (obligatorio)
    /// </summary>
    public string RFC { get; set; } = string.Empty;

    /// <summary>
    /// RUT del proveedor (obligatorio para Chile)
    /// </summary>
    public string RUT { get; set; } = string.Empty;

    /// <summary>
    /// Información bancaria (opcional)
    /// </summary>
    public string InformacionBancaria { get; set; } = string.Empty;

    /// <summary>
    /// Días de crédito (opcional, por defecto 0 = contado)
    /// </summary>
    public int DiasCredito { get; set; } = 0;
} 