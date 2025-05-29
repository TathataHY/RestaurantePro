namespace RestaurantePro.Application.Proveedores.Proveedores.DTOs;

/// <summary>
/// DTO principal para Proveedor con información completa
/// Incluye datos calculados y propiedades para UI
/// </summary>
public class ProveedorDto
{
    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del contacto principal
    /// </summary>
    public string NombreContacto { get; set; } = string.Empty;

    /// <summary>
    /// Email del proveedor
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del proveedor
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Dirección completa del proveedor
    /// </summary>
    public string Direccion { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad del proveedor
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// Código postal del proveedor
    /// </summary>
    public string CodigoPostal { get; set; } = string.Empty;

    /// <summary>
    /// País del proveedor
    /// </summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>
    /// RFC del proveedor
    /// </summary>
    public string RFC { get; set; } = string.Empty;

    /// <summary>
    /// Información bancaria del proveedor
    /// </summary>
    public string InformacionBancaria { get; set; } = string.Empty;

    /// <summary>
    /// Días de crédito otorgados por el proveedor
    /// </summary>
    public int DiasCredito { get; set; }

    /// <summary>
    /// Indica si el proveedor está activo
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de registro del proveedor
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Usuario que creó el registro
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    // === PROPIEDADES CALCULADAS PARA UI ===

    /// <summary>
    /// Dirección completa formateada para mostrar
    /// </summary>
    public string DireccionCompleta => $"{Direccion}, {Ciudad}, {CodigoPostal}, {Pais}".Trim(' ', ',');

    /// <summary>
    /// Estado del proveedor con formato legible
    /// </summary>
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";

    /// <summary>
    /// Color para mostrar el estado en UI
    /// </summary>
    public string ColorEstado => Activo ? "#4CAF50" : "#F44336";

    /// <summary>
    /// Icono para mostrar el estado en UI
    /// </summary>
    public string IconoEstado => Activo ? "✅" : "❌";

    /// <summary>
    /// Información de contacto principal formateada
    /// </summary>
    public string ContactoPrincipal => $"{NombreContacto} - {Email} - {Telefono}".Trim(' ', '-');

    /// <summary>
    /// Condiciones de pago formateadas
    /// </summary>
    public string CondicionesPago => DiasCredito > 0 
        ? $"Crédito a {DiasCredito} días" 
        : "Contado";

    /// <summary>
    /// Tiempo desde el registro del proveedor
    /// </summary>
    public string TiempoRegistrado
    {
        get
        {
            var tiempo = DateTime.Now - FechaRegistro;
            return tiempo.Days switch
            {
                < 1 => "Hoy",
                < 7 => $"{tiempo.Days} días",
                < 30 => $"{tiempo.Days / 7} semanas",
                < 365 => $"{tiempo.Days / 30} meses",
                _ => $"{tiempo.Days / 365} años"
            };
        }
    }

    /// <summary>
    /// Tooltip informativo con resumen del proveedor
    /// </summary>
    public string TooltipInfo => $"🏢 {Nombre}\n📍 {Ciudad}, {Pais}\n📧 {Email}\n📞 {Telefono}\n💳 {CondicionesPago}\n📅 Registrado hace {TiempoRegistrado}";

    /// <summary>
    /// Resumen del estado del proveedor
    /// </summary>
    public string ResumenEstado => Activo 
        ? $"✅ Proveedor activo - {CondicionesPago} - {Ciudad}"
        : $"❌ Proveedor inactivo desde {FechaRegistro:dd/MM/yyyy}";

    /// <summary>
    /// Lista de contactos del proveedor
    /// </summary>
    public List<ContactoProveedorDto> Contactos { get; set; } = new();

    /// <summary>
    /// Cantidad total de contactos
    /// </summary>
    public int TotalContactos => Contactos?.Count ?? 0;

    /// <summary>
    /// Indica si tiene contactos registrados
    /// </summary>
    public bool TieneContactos => TotalContactos > 0;
} 