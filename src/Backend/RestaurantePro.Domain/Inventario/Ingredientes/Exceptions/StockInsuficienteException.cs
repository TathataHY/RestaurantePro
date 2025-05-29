namespace RestaurantePro.Domain.Inventario.Ingredientes.Exceptions;

/// <summary>
/// Excepción que se lanza cuando no hay suficiente stock de un ingrediente para realizar una operación.
/// </summary>
public class StockInsuficienteException : DomainException
{
    /// <summary>
    /// ID del ingrediente con stock insuficiente
    /// </summary>
    public Guid IngredienteId { get; }

    /// <summary>
    /// Cantidad requerida
    /// </summary>
    public decimal CantidadRequerida { get; }

    /// <summary>
    /// Cantidad disponible actualmente
    /// </summary>
    public decimal CantidadDisponible { get; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; }

    /// <summary>
    /// Constructor para stock insuficiente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="cantidadRequerida">Cantidad requerida</param>
    /// <param name="cantidadDisponible">Cantidad disponible</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    public StockInsuficienteException(
        Guid ingredienteId,
        string nombreIngrediente,
        decimal cantidadRequerida,
        decimal cantidadDisponible,
        string operacion = "operación")
        : base(
            $"stock insuficiente de '{nombreIngrediente}' para {operacion}. Requerido: {cantidadRequerida}, Disponible: {cantidadDisponible}",
            "INSUFFICIENT_STOCK",
            "Inventario")
    {
        IngredienteId = ingredienteId;
        CantidadRequerida = cantidadRequerida;
        CantidadDisponible = cantidadDisponible;
        NombreIngrediente = nombreIngrediente;

        WithData("IngredienteId", ingredienteId);
        WithData("CantidadRequerida", cantidadRequerida);
        WithData("StockDisponible", cantidadDisponible);
        WithData("NombreIngrediente", nombreIngrediente);
        WithData("Operacion", operacion);
        WithData("Deficit", cantidadRequerida - cantidadDisponible);
    }

    /// <summary>
    /// Crea excepción para preparación de comanda
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="cantidadRequerida">Cantidad requerida</param>
    /// <param name="cantidadDisponible">Cantidad disponible</param>
    /// <param name="comandaId">ID de la comanda</param>
    /// <returns>Nueva instancia de StockInsuficienteException</returns>
    public static StockInsuficienteException ParaComanda(
        Guid ingredienteId,
        string nombreIngrediente,
        decimal cantidadRequerida,
        decimal cantidadDisponible,
        Guid comandaId)
    {
        var excepcion = new StockInsuficienteException(
            ingredienteId,
            nombreIngrediente,
            cantidadRequerida,
            cantidadDisponible,
            "preparar comanda");
        excepcion.WithData("ComandaId", comandaId);
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para preparación de receta
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="cantidadRequerida">Cantidad requerida</param>
    /// <param name="cantidadDisponible">Cantidad disponible</param>
    /// <param name="recetaId">ID de la receta</param>
    /// <param name="productoNombre">Nombre del producto</param>
    /// <returns>Nueva instancia de StockInsuficienteException</returns>
    public static StockInsuficienteException ParaReceta(
        Guid ingredienteId,
        string nombreIngrediente,
        decimal cantidadRequerida,
        decimal cantidadDisponible,
        Guid recetaId,
        string productoNombre)
    {
        var excepcion = new StockInsuficienteException(
            ingredienteId,
            nombreIngrediente,
            cantidadRequerida,
            cantidadDisponible,
            $"preparar receta de {productoNombre}");
        excepcion.WithData("RecetaId", recetaId);
        excepcion.WithData("ProductoNombre", productoNombre);
        return excepcion;
    }

    /// <summary>
    /// Crea excepción para múltiples ingredientes con stock insuficiente
    /// </summary>
    /// <param name="ingredientesFaltantes">Lista de ingredientes con stock insuficiente</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de StockInsuficienteException</returns>
    public static StockInsuficienteException ParaMultiplesIngredientes(
        Dictionary<Guid, (string nombre, decimal requerido, decimal disponible)> ingredientesFaltantes,
        string operacion = "operación")
    {
        var primerIngrediente = ingredientesFaltantes.First();
        var mensaje = $"Stock insuficiente para {ingredientesFaltantes.Count} ingrediente(s) en {operacion}";
        
        var exception = new StockInsuficienteException(
            primerIngrediente.Key,
            primerIngrediente.Value.nombre,
            primerIngrediente.Value.requerido,
            primerIngrediente.Value.disponible,
            operacion);

        exception.WithData("CantidadIngredientesFaltantes", ingredientesFaltantes.Count);
        exception.WithData("IngredientesFaltantes", ingredientesFaltantes);

        return exception;
    }

    /// <summary>
    /// Obtiene un resumen del déficit de stock
    /// </summary>
    /// <returns>String con el resumen del déficit</returns>
    public string GetDeficitSummary()
    {
        var deficit = CantidadRequerida - CantidadDisponible;
        return $"Déficit de {deficit:F2} unidades de '{NombreIngrediente}'";
    }

    /// <summary>
    /// Indica si el déficit es crítico (más del 50% del stock disponible)
    /// </summary>
    public bool EsDeficitCritico => CantidadDisponible > 0 && 
                                   (CantidadRequerida - CantidadDisponible) > (CantidadDisponible * 0.5m);
} 