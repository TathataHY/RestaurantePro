namespace RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadProducto;

/// <summary>
/// Query para verificar la disponibilidad de un producto con diferentes tipos de análisis
/// </summary>
public class VerificarDisponibilidadProductoQuery : IRequest<Result<DisponibilidadProductoDto>>
{
    public Guid ProductoId { get; set; }
    public int CantidadSolicitada { get; set; }
    public string TipoVerificacion { get; set; } = "Simple";
    public bool IncluirAnalisisIngredientes { get; set; }
    public bool IncluirRecomendacionesAlternativas { get; set; }
    public bool VerificarTodasLasVariantes { get; set; }
    public bool CalcularTiempoPreparacion { get; set; }
    public DateTime? FechaHoraDeseada { get; set; }
    public Guid? ComandaId { get; set; }
    public bool PriorizarVelocidadPreparacion { get; set; }
    public int PrioridadVerificacion { get; set; } = 1;
    public bool OptimizarConsultasBaseDatos { get; set; }
    public bool AgruparPorCategoria { get; set; }
    public List<(Guid ProductoId, int Cantidad)> ProductosAVerificar { get; set; } = new();

    #region Factory Methods

    /// <summary>
    /// Crea una verificación simple de disponibilidad
    /// </summary>
    public static VerificarDisponibilidadProductoQuery VerificacionSimple(Guid productoId, int cantidad)
    {
        return new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = cantidad,
            TipoVerificacion = "Simple",
            IncluirAnalisisIngredientes = false,
            IncluirRecomendacionesAlternativas = false,
            VerificarTodasLasVariantes = false,
            CalcularTiempoPreparacion = false,
            PriorizarVelocidadPreparacion = false,
            PrioridadVerificacion = 1
        };
    }

    /// <summary>
    /// Crea una verificación completa con todos los análisis
    /// </summary>
    public static VerificarDisponibilidadProductoQuery VerificacionCompleta(Guid productoId, int cantidad, DateTime? fechaDeseada = null)
    {
        return new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = cantidad,
            TipoVerificacion = "Completa",
            IncluirAnalisisIngredientes = true,
            IncluirRecomendacionesAlternativas = true,
            VerificarTodasLasVariantes = true,
            CalcularTiempoPreparacion = true,
            FechaHoraDeseada = fechaDeseada,
            PriorizarVelocidadPreparacion = false,
            PrioridadVerificacion = 2
        };
    }

    /// <summary>
    /// Crea una verificación específica para una comanda
    /// </summary>
    public static VerificarDisponibilidadProductoQuery VerificacionParaComanda(Guid productoId, Guid comandaId, int cantidad)
    {
        return new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = cantidad,
            ComandaId = comandaId,
            TipoVerificacion = "ParaComanda",
            IncluirAnalisisIngredientes = true,
            IncluirRecomendacionesAlternativas = false,
            VerificarTodasLasVariantes = false,
            CalcularTiempoPreparacion = true,
            PriorizarVelocidadPreparacion = true,
            PrioridadVerificacion = 3
        };
    }

    /// <summary>
    /// Crea una verificación masiva para múltiples productos
    /// </summary>
    public static VerificarDisponibilidadProductoQuery VerificacionMasiva(List<(Guid ProductoId, int Cantidad)> productos)
    {
        return new VerificarDisponibilidadProductoQuery
        {
            TipoVerificacion = "Masiva",
            ProductosAVerificar = productos ?? new List<(Guid, int)>(),
            OptimizarConsultasBaseDatos = true,
            AgruparPorCategoria = true,
            IncluirAnalisisIngredientes = false,
            IncluirRecomendacionesAlternativas = false,
            PrioridadVerificacion = 1
        };
    }

    #endregion
} 