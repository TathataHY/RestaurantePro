namespace RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;

/// <summary>
/// Command para dividir una comanda en múltiples comandas separadas
/// Permite separar items de una comanda en nuevas comandas independientes
/// </summary>
public class DividirComandaCommand : IRequest<Result<DividirComandaDto>>
{
    /// <summary>
    /// ID de la comanda original a dividir
    /// </summary>
    public Guid ComandaOriginalId { get; set; }

    /// <summary>
    /// Tipo de división
    /// </summary>
    public TipoDivisionComanda TipoDivision { get; set; }

    /// <summary>
    /// Distribución de items por comanda nueva
    /// </summary>
    public List<DivisionComandaDto> DivisionItems { get; set; } = new();

    /// <summary>
    /// Motivo de la división
    /// </summary>
    public string MotivoDivision { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que autoriza la división
    /// </summary>
    public Guid? AutorizadoPor { get; set; }

    /// <summary>
    /// Notas adicionales sobre la división
    /// </summary>
    public string? NotasDivision { get; set; }

    /// <summary>
    /// Indica si se debe mantener la comanda original
    /// </summary>
    public bool MantenerComandaOriginal { get; set; } = false;

    /// <summary>
    /// Indica si se debe distribuir el descuento proporcional
    /// </summary>
    public bool DistribuirDescuentos { get; set; } = true;

    /// <summary>
    /// Datos adicionales de la división
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public DividirComandaCommand(Guid comandaOriginalId, TipoDivisionComanda tipoDivision, string motivoDivision)
    {
        ComandaOriginalId = comandaOriginalId;
        TipoDivision = tipoDivision;
        MotivoDivision = motivoDivision;
    }

    public DividirComandaCommand() { }
}

/// <summary>
/// DTO para especificar la división de items
/// </summary>
public class DivisionComandaDto
{
    /// <summary>
    /// Número de la nueva comanda
    /// </summary>
    public int NumeroComandaNueva { get; set; }

    /// <summary>
    /// Items que van a esta comanda
    /// </summary>
    public List<ItemDivisionDto> Items { get; set; } = new();

    /// <summary>
    /// Mesa de destino para esta comanda (opcional)
    /// </summary>
    public Guid? MesaDestinoId { get; set; }

    /// <summary>
    /// Mesero asignado a esta comanda (opcional)
    /// </summary>
    public Guid? MeseroId { get; set; }

    /// <summary>
    /// Observaciones específicas para esta comanda
    /// </summary>
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para especificar los items en la división
/// </summary>
public class ItemDivisionDto
{
    /// <summary>
    /// ID del item original
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Cantidad del item para esta comanda
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Observaciones específicas para este item
    /// </summary>
    public string? ObservacionesItem { get; set; }
}

/// <summary>
/// Tipos de división de comanda
/// </summary>
public enum TipoDivisionComanda
{
    /// <summary>
    /// División por items específicos
    /// </summary>
    PorItems = 1,

    /// <summary>
    /// División por mesas diferentes
    /// </summary>
    PorMesas = 2,

    /// <summary>
    /// División por personas/cuentas separadas
    /// </summary>
    PorCuentas = 3,

    /// <summary>
    /// División por categorías de productos
    /// </summary>
    PorCategorias = 4
}

/// <summary>
/// DTO de respuesta para la división de comanda
/// </summary>
public class DividirComandaDto
{
    public Guid ComandaOriginalId { get; set; }
    public List<Guid> ComandasNuevasIds { get; set; } = new();
    public TipoDivisionComanda TipoDivision { get; set; }
    public string MotivoDivision { get; set; } = string.Empty;
    public DateTime FechaDivision { get; set; }
    public Guid? AutorizadoPor { get; set; }
    public bool DivisionExitosa { get; set; }
    public int TotalComandasCreadas { get; set; }
} 