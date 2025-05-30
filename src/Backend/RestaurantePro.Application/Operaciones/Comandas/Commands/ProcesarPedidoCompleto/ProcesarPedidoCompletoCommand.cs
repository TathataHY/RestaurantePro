namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ProcesarPedidoCompleto;

/// <summary>
/// 🚀 Command para procesar un pedido completo que cruza múltiples bounded contexts
/// Orquesta: Comanda + Inventario + Promociones + Facturación + Fidelización
/// </summary>
public class ProcesarPedidoCompletoCommand : IRequest<Result<PedidoCompletoResult>>
{
    public Guid? ClienteId { get; init; }
    public Guid? MesaId { get; init; }
    public Guid MeseroId { get; init; }
    public List<ItemPedido> Items { get; init; } = new();
    public string? ObservacionesComanda { get; init; }
    public bool AplicarDescuentoAutomatico { get; init; } = true;
    public bool GenerarFacturaInmediata { get; init; } = false;
    public int? PuntosAUtilizar { get; init; }

    /// <summary>
    /// Factory method para crear el command con validaciones básicas
    /// </summary>
    public static ProcesarPedidoCompletoCommand Crear(
        Guid meseroId,
        List<ItemPedido> items,
        Guid? clienteId = null,
        Guid? mesaId = null,
        string? observaciones = null,
        bool aplicarDescuento = true,
        bool generarFactura = false,
        int? puntosAUtilizar = null)
    {
        if (meseroId == Guid.Empty)
            throw new ArgumentException("MeseroId no puede estar vacío", nameof(meseroId));
        
        if (items == null || !items.Any())
            throw new ArgumentException("Debe incluir al menos un item en el pedido", nameof(items));

        return new ProcesarPedidoCompletoCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            MeseroId = meseroId,
            Items = items,
            ObservacionesComanda = observaciones,
            AplicarDescuentoAutomatico = aplicarDescuento,
            GenerarFacturaInmediata = generarFactura,
            PuntosAUtilizar = puntosAUtilizar
        };
    }
}

/// <summary>
/// 🍽️ Item del pedido con detalles completos
/// </summary>
public class ItemPedido
{
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Observaciones { get; set; }
    public List<PersonalizacionItem>? Personalizaciones { get; set; }
}

/// <summary>
/// 🎨 Personalización para items del pedido
/// </summary>
public class PersonalizacionItem
{
    public string Tipo { get; set; } = string.Empty; // "Agregar", "Quitar", "Sustituir"
    public Guid IngredienteId { get; set; }
    public string IngredienteNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioAdicional { get; set; } = 0;
}

/// <summary>
/// 🎯 Resultado del procesamiento completo del pedido
/// </summary>
public class PedidoCompletoResult
{
    public Guid ComandaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? MesaId { get; set; }
    public Guid MeseroId { get; set; }
    public decimal TotalOriginal { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalFinal { get; set; }
    public int ItemsProcesados { get; set; }
    public int PuntosUtilizados { get; set; }
    public int PuntosGanados { get; set; }
    public Guid? FacturaId { get; set; }
    public List<string> Advertencias { get; set; } = new();
    public List<string> Mensajes { get; set; } = new();
    public DateTime FechaProcesamiento { get; set; }
    public FlujoPedidoEstadisticas Estadisticas { get; set; } = new();
}

/// <summary>
/// 📊 Estadísticas del flujo de procesamiento
/// </summary>
public class FlujoPedidoEstadisticas
{
    public int ItemsDisponiblesPreparacion { get; set; }
    public int ItemsRequirieronInventario { get; set; }
    public int DescuentosAplicados { get; set; }
    public decimal TiempoProcesamientoMs { get; set; }
    public bool FueExitoso { get; set; }
    public List<string> PasosEjecutados { get; set; } = new();
} 