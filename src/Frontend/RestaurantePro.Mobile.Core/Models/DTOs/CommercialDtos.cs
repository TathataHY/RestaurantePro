namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para factura
/// </summary>
public class FacturaDto
{
    public Guid Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public string ClienteEmail { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public List<ItemFacturaDto> Items { get; set; } = new();
    
    // Propiedades adicionales para la UI
    public string Numero { get; set; } = string.Empty;
    public string MesaNumero { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Descuentos { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? ReferenciaPago { get; set; }
}

/// <summary>
/// DTO para item de factura
/// </summary>
public class ItemFacturaDto
{
    public Guid Id { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty; // Alias para compatibilidad
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Precio { get; set; } // Alias para compatibilidad
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DTO para estadísticas de facturas
/// </summary>
public class EstadisticasFacturasDto
{
    public int TotalFacturas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioFactura { get; set; }
    public int FacturasPagadas { get; set; }
    public int FacturasPendientes { get; set; }
}

/// <summary>
/// DTO para cliente
/// </summary>
public class ClienteDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int TotalComandas { get; set; }
    public decimal TotalGastado { get; set; }
}

/// <summary>
/// DTO para estadísticas de clientes
/// </summary>
public class EstadisticasClientesDto
{
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int ClientesNuevosHoy { get; set; }
    public int ClientesInactivos { get; set; }
}

/// <summary>
/// DTO para tarjeta de fidelización
/// </summary>
public class TarjetaFidelizacionDto
{
    public Guid Id { get; set; }
    public string CodigoTarjeta { get; set; } = string.Empty;
    public string NumeroTarjeta { get; set; } = string.Empty;
    public int PuntosDisponibles { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public DateTime FechaActivacion { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
}

/// <summary>
/// DTO para transacción de puntos
/// </summary>
public class TransaccionPuntosDto
{
    public Guid Id { get; set; }
    public string TipoTransaccion { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaTransaccion { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// DTO para historial de puntos
/// </summary>
public class HistorialPuntosDto
{
    public Guid Id { get; set; }
    public string CodigoTarjeta { get; set; } = string.Empty;
    public List<TransaccionPuntosDto> Transacciones { get; set; } = new();
    public int PuntosAcumulados { get; set; }
    public int PuntosCanjeados { get; set; }
    public int PuntosDisponibles { get; set; }
    public DateTime FechaUltimaTransaccion { get; set; }
}

/// <summary>
/// DTO resumido para Cliente - Optimizado para listas y performance
/// </summary>
public class ClienteSummaryDto
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Email principal del cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono principal del cliente
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de cliente (texto legible)
    /// </summary>
    public string TipoCliente { get; set; } = string.Empty;

    /// <summary>
    /// Estado activo del cliente
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Usuario que registró el cliente
    /// </summary>
    public string RegistradoPor { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Ciudad del cliente
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// País del cliente
    /// </summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>
    /// Puntos de fidelización actuales
    /// </summary>
    public int PuntosFidelizacion { get; set; }

    /// <summary>
    /// Nivel de fidelización (Bronce, Plata, Oro, Platino)
    /// </summary>
    public string NivelFidelizacion { get; set; } = string.Empty;

    /// <summary>
    /// Número total de órdenes realizadas
    /// </summary>
    public int TotalOrdenes { get; set; }

    /// <summary>
    /// Monto total de compras históricas
    /// </summary>
    public decimal MontoTotalCompras { get; set; }

    /// <summary>
    /// Fecha de la última orden
    /// </summary>
    public DateTime? FechaUltimaOrden { get; set; }

    /// <summary>
    /// Promedio de compra por orden
    /// </summary>
    public decimal PromedioCompra { get; set; }

    /// <summary>
    /// Indicador de cliente frecuente
    /// </summary>
    public bool EsFrecuente { get; set; }

    /// <summary>
    /// Días desde la última visita
    /// </summary>
    public int DiasSinVisitar { get; set; }

    /// <summary>
    /// Indicador de cliente VIP
    /// </summary>
    public bool EsVIP { get; set; }

    /// <summary>
    /// Edad calculada del cliente
    /// </summary>
    public int? Edad => FechaNacimiento.HasValue 
        ? DateTime.Now.Year - FechaNacimiento.Value.Year 
        : null;

    /// <summary>
    /// Estado de actividad del cliente
    /// </summary>
    public string EstadoActividad => DiasSinVisitar switch
    {
        <= 7 => "Activo",
        <= 30 => "Regular",
        <= 90 => "Inactivo",
        _ => "Muy Inactivo"
    };

    /// <summary>
    /// Categoría de valor del cliente
    /// </summary>
    public string CategoriaValor => MontoTotalCompras switch
    {
        >= 10000 => "Alto Valor",
        >= 5000 => "Medio Valor",
        >= 1000 => "Bajo Valor",
        _ => "Nuevo"
    };
}

/// <summary>
/// DTO para estadísticas de tarjeta de fidelización
/// </summary>
public class EstadisticasTarjetaDto
{
    public Guid TarjetaId { get; set; }
    public string CodigoTarjeta { get; set; } = string.Empty;
    public int PuntosAcumulados { get; set; }
    public int PuntosCanjeados { get; set; }
    public int PuntosDisponibles { get; set; }
    public decimal MontoTotalGastado { get; set; }
    public int TotalTransacciones { get; set; }
    public DateTime FechaUltimaTransaccion { get; set; }
    public string NivelActual { get; set; } = string.Empty;
    public int PuntosParaSiguienteNivel { get; set; }
    public string BeneficiosDisponibles { get; set; } = string.Empty;
}

/// <summary>
/// DTO para filtro de clientes
/// </summary>
public class FiltroClientesDto
{
    public string? Busqueda { get; set; }
    public string? SearchTerm { get; set; }
    public string? Segmento { get; set; }
    public bool? SoloActivos { get; set; }
    public bool? SoloConTarjetaFidelizacion { get; set; }
    public bool? SoloFrecuentes { get; set; }
    public bool? SoloClientesFrecuentes { get; set; }
    public DateTime? FechaRegistroDesde { get; set; }
    public DateTime? FechaRegistroHasta { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para filtro de facturas
/// </summary>
public class FiltroFacturasDto
{
    public string? SearchTerm { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? Estado { get; set; }
    public string? MetodoPago { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public Guid? ClienteId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para filtro de tarjetas de fidelización
/// </summary>
public class FiltroTarjetasFidelizacionDto
{
    public string? Busqueda { get; set; }
    public string? Estado { get; set; }
    public string? Nivel { get; set; }
    public bool? SoloActivas { get; set; }
    public Guid? ClienteId { get; set; }
    public int? PuntosMinimos { get; set; }
    public int? PuntosMaximos { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para registrar pago de factura
/// </summary>
public class RegistrarPagoDto
{
    public Guid FacturaId { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public decimal MontoPagado { get; set; }
    public string? ReferenciaPago { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para anular factura
/// </summary>
public class AnularFacturaDto
{
    public Guid FacturaId { get; set; }
    public string MotivoAnulacion { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
} 