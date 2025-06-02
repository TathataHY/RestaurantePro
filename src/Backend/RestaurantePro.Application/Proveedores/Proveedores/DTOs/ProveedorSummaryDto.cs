namespace RestaurantePro.Application.Proveedores.Proveedores.DTOs;

/// <summary>
/// DTO resumido para Proveedor - Optimizado para listas y performance
/// Contiene solo los campos esenciales para mostrar en grids y listas
/// </summary>
public class ProveedorSummaryDto
{
    /// <summary>
    /// Identificador único del proveedor
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre o razón social del proveedor
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Número de identificación fiscal (NIT, RUC, etc.)
    /// </summary>
    public string NumeroIdentificacion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de proveedor (texto legible)
    /// </summary>
    public string TipoProveedor { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono principal de contacto
    /// </summary>
    public string TelefonoPrincipal { get; set; } = string.Empty;

    /// <summary>
    /// Email principal de contacto
    /// </summary>
    public string EmailPrincipal { get; set; } = string.Empty;

    /// <summary>
    /// Estado activo del proveedor
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Usuario que creó el proveedor
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad principal del proveedor
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// País del proveedor
    /// </summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>
    /// Calificación del proveedor (1-5 estrellas)
    /// </summary>
    public int CalificacionPromedio { get; set; }

    /// <summary>
    /// Condición de pago principal (texto legible)
    /// </summary>
    public string CondicionPago { get; set; } = string.Empty;

    /// <summary>
    /// Número total de contactos registrados
    /// </summary>
    public int TotalContactos { get; set; }

    /// <summary>
    /// Número total de órdenes de compra
    /// </summary>
    public int TotalOrdenes { get; set; }

    /// <summary>
    /// Monto total de compras históricas
    /// </summary>
    public decimal MontoTotalCompras { get; set; }

    /// <summary>
    /// Fecha de la última orden de compra
    /// </summary>
    public DateTime? FechaUltimaOrden { get; set; }

    /// <summary>
    /// Indicador de proveedor preferido
    /// </summary>
    public bool EsPreferido { get; set; }

    /// <summary>
    /// Categoría del proveedor
    /// </summary>
    public CategoriaProveedor Categoria { get; set; }

    /// <summary>
    /// Días promedio de entrega
    /// </summary>
    public int DiasPromedioEntrega { get; set; }

    /// <summary>
    /// Indicador de cumplimiento de entregas (porcentaje)
    /// </summary>
    public decimal PorcentajeCumplimiento { get; set; }

    // === PROPIEDADES FALTANTES AGREGADAS ===
    
    /// <summary>
    /// Días de crédito del proveedor
    /// </summary>
    public int DiasCredito { get; set; }
    
    /// <summary>
    /// Nombre del contacto principal
    /// </summary>
    public string NombreContacto { get; set; } = string.Empty;
    
    /// <summary>
    /// Email del contacto principal
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de registro del proveedor
    /// </summary>
    public DateTime FechaRegistro { get; set; }
    
    /// <summary>
    /// Indicador si está activo (copia de Activo para consistencia)
    /// </summary>
    public bool EstaActivo => Activo;

    // === PROPIEDADES CALCULADAS PARA UI ===

    /// <summary>
    /// Ubicación resumida (Ciudad, País)
    /// </summary>
    public string Ubicacion => $"{Ciudad}, {Pais}";

    /// <summary>
    /// Estado del proveedor para mostrar en listas
    /// </summary>
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";

    /// <summary>
    /// Color para mostrar el estado
    /// </summary>
    public string ColorEstado => Activo ? "#4CAF50" : "#F44336";

    /// <summary>
    /// Icono para el estado
    /// </summary>
    public string IconoEstado => Activo ? "✅" : "❌";

    /// <summary>
    /// Condiciones de pago resumidas
    /// </summary>
    public string CondicionesPago => DiasCredito > 0 
        ? $"{DiasCredito}d crédito" 
        : "Contado";

    /// <summary>
    /// Color para condiciones de pago
    /// </summary>
    public string ColorCondiciones => DiasCredito > 0 ? "#2196F3" : "#FF9800";

    /// <summary>
    /// Contacto principal resumido
    /// </summary>
    public string ContactoResumido => $"{NombreContacto} ({Email})";

    /// <summary>
    /// Tiempo desde registro (resumido)
    /// </summary>
    public string TiempoRegistrado
    {
        get
        {
            var dias = (DateTime.Now - FechaRegistro).Days;
            return dias switch
            {
                < 7 => $"{dias}d",
                < 30 => $"{dias / 7}sem",
                < 365 => $"{dias / 30}mes",
                _ => $"{dias / 365}años"
            };
        }
    }

    /// <summary>
    /// Resumen completo para tooltips
    /// </summary>
    public string ResumenCompleto => $"🏢 {Nombre}\n📍 {Ubicacion}\n👤 {NombreContacto}\n📧 {Email}\n💳 {CondicionesPago}\n📅 {TiempoRegistrado}";

    /// <summary>
    /// Prioridad para ordenamiento (activos primero, luego por nombre)
    /// </summary>
    public int Prioridad => Activo ? 1 : 2;

    /// <summary>
    /// Calcula el color visual del estado crediticio basado en días de crédito
    /// </summary>
    public string ColorEstadoCredito => 
        DiasCredito <= 7 ? "#22c55e" :    // Verde: Excelente 
        DiasCredito <= 15 ? "#eab308" :   // Amarillo: Bueno
        "#ef4444";                        // Rojo: Requiere seguimiento

    /// <summary>
    /// Proporciona un resumen textual del estado crediticio
    /// </summary>
    public string ResumenCredito =>
        DiasCredito <= 7 ? "Excelente" :
        DiasCredito <= 15 ? "Bueno" :
        "Requiere seguimiento";

    /// <summary>
    /// Genera el contacto principal con formato para mostrar
    /// </summary>
    public string ContactoPrincipalDisplay =>
        !string.IsNullOrEmpty(NombreContacto) ? $"{NombreContacto} ({Email})" : "Sin contacto asignado";

    /// <summary>
    /// Información adicional sobre el proveedor para el resumen
    /// </summary>
    public string InformacionAdicional =>
        $"Registrado: {FechaRegistro:dd/MM/yyyy}";

    /// <summary>
    /// Determina el color de prioridad de contacto
    /// </summary>
    public string ColorPrioridad => EstaActivo ? "#22c55e" : "#ef4444";

    /// <summary>
    /// Resumen de contacto con validación
    /// </summary>
    public string ResumenContacto => 
        !string.IsNullOrEmpty(NombreContacto) ? $"Contacto: {NombreContacto} ({Email})" :
        "Sin información de contacto disponible";
} 