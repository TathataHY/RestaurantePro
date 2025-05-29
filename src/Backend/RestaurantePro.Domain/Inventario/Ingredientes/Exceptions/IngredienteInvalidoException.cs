namespace RestaurantePro.Domain.Inventario.Ingredientes.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación inválida con un ingrediente.
/// </summary>
public class IngredienteInvalidoException : BusinessRuleViolationException
{
    /// <summary>
    /// ID del ingrediente involucrado
    /// </summary>
    public Guid IngredienteId { get; }

    /// <summary>
    /// Stock actual del ingrediente
    /// </summary>
    public decimal StockActual { get; }

    /// <summary>
    /// Constructor principal para ingrediente inválido
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual del ingrediente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razon">Razón por la cual la operación es inválida</param>
    public IngredienteInvalidoException(
        Guid ingredienteId,
        decimal stockActual,
        string operacion,
        string razon)
        : base(
            "IngredienteInvalido",
            "Ingrediente",
            $"No se puede realizar '{operacion}' con ingrediente (Stock: {stockActual}): {razon}",
            "Inventario",
            ingredienteId)
    {
        IngredienteId = ingredienteId;
        StockActual = stockActual;
        
        WithData("StockActual", StockActual)
            .WithData("Operacion", operacion)
            .WithData("Razon", razon);
    }

    /// <summary>
    /// Crea excepción para stock insuficiente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual disponible</param>
    /// <param name="cantidadSolicitada">Cantidad que se intentó usar</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaStockInsuficiente(
        Guid ingredienteId, 
        decimal stockActual, 
        decimal cantidadSolicitada,
        string operacion = "usar")
    {
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"Stock insuficiente: disponible {stockActual}, solicitado {cantidadSolicitada}")
            .WithData("CantidadSolicitada", cantidadSolicitada)
            .WithData("Deficit", cantidadSolicitada - stockActual) as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para ingrediente vencido
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="fechaVencimiento">Fecha de vencimiento</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaIngredienteVencido(
        Guid ingredienteId, 
        decimal stockActual, 
        DateTime fechaVencimiento,
        string operacion = "usar")
    {
        var diasVencido = (DateTime.Now - fechaVencimiento).Days;
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"Ingrediente vencido desde hace {diasVencido} días (venció {fechaVencimiento:dd/MM/yyyy})")
            .WithData("FechaVencimiento", fechaVencimiento)
            .WithData("DiasVencido", diasVencido) as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para ingrediente próximo a vencer
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="fechaVencimiento">Fecha de vencimiento</param>
    /// <param name="diasLimite">Días límite para considerar próximo a vencer</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaIngredienteProximoAVencer(
        Guid ingredienteId, 
        decimal stockActual, 
        DateTime fechaVencimiento,
        int diasLimite = 3,
        string operacion = "usar en producción")
    {
        var diasRestantes = (fechaVencimiento - DateTime.Now).Days;
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"Ingrediente próximo a vencer en {diasRestantes} días (vence {fechaVencimiento:dd/MM/yyyy})")
            .WithData("FechaVencimiento", fechaVencimiento)
            .WithData("DiasRestantes", diasRestantes)
            .WithData("DiasLimite", diasLimite) as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para proveedor sin stock
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaProveedorSinStock(
        Guid ingredienteId, 
        decimal stockActual, 
        Guid proveedorId,
        string operacion = "reabastecer")
    {
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            "El proveedor no tiene stock disponible para este ingrediente")
            .WithData("ProveedorId", proveedorId)
            .WithData("TipoProblema", "ProveedorSinStock") as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para unidad de medida incompatible
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="unidadActual">Unidad de medida actual</param>
    /// <param name="unidadSolicitada">Unidad de medida solicitada</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaUnidadIncompatible(
        Guid ingredienteId, 
        decimal stockActual, 
        string unidadActual,
        string unidadSolicitada,
        string operacion = "convertir")
    {
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"No se puede convertir de '{unidadActual}' a '{unidadSolicitada}': unidades incompatibles")
            .WithData("UnidadActual", unidadActual)
            .WithData("UnidadSolicitada", unidadSolicitada)
            .WithData("TipoProblema", "UnidadIncompatible") as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para ingrediente fuera de temporada
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="temporadaActual">Temporada actual</param>
    /// <param name="temporadaRequerida">Temporada requerida del ingrediente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaFueraDeTemporada(
        Guid ingredienteId, 
        decimal stockActual, 
        string temporadaActual,
        string temporadaRequerida,
        string operacion = "usar")
    {
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"Ingrediente fuera de temporada: está en '{temporadaRequerida}' pero estamos en '{temporadaActual}'")
            .WithData("TemporadaActual", temporadaActual)
            .WithData("TemporadaRequerida", temporadaRequerida)
            .WithData("TipoProblema", "FueraDeTemporada") as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para stock por debajo del mínimo crítico
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="stockMinimoCritico">Stock mínimo crítico</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaStockCritico(
        Guid ingredienteId, 
        decimal stockActual, 
        decimal stockMinimoCritico,
        string operacion = "usar")
    {
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"Stock en nivel crítico: {stockActual} (mínimo crítico: {stockMinimoCritico})")
            .WithData("StockMinimoCritico", stockMinimoCritico)
            .WithData("DeficitCritico", stockMinimoCritico - stockActual)
            .WithData("TipoProblema", "StockCritico") as IngredienteInvalidoException;
    }

    /// <summary>
    /// Crea excepción para calidad del ingrediente no aceptable
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="calidadActual">Calidad actual del ingrediente</param>
    /// <param name="calidadMinima">Calidad mínima requerida</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de IngredienteInvalidoException</returns>
    public static IngredienteInvalidoException ParaCalidadInaceptable(
        Guid ingredienteId, 
        decimal stockActual, 
        string calidadActual,
        string calidadMinima,
        string operacion = "usar en producción")
    {
        return new IngredienteInvalidoException(
            ingredienteId,
            stockActual,
            operacion,
            $"Calidad inaceptable: '{calidadActual}' (mínimo requerido: '{calidadMinima}')")
            .WithData("CalidadActual", calidadActual)
            .WithData("CalidadMinima", calidadMinima)
            .WithData("TipoProblema", "CalidadInaceptable") as IngredienteInvalidoException;
    }
} 