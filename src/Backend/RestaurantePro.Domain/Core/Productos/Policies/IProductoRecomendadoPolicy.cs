namespace RestaurantePro.Domain.Core.Productos.Policies
{
    /// <summary>
    /// Resultado de la ejecución de la política de recomendación de productos
    /// </summary>
    public class ResultadoProductoRecomendadoPolicy
    {
        /// <summary>
        /// Lista de productos recomendados con su puntuación
        /// </summary>
        public List<ProductoRecomendado> ProductosRecomendados { get; } = new List<ProductoRecomendado>();
        
        /// <summary>
        /// Fecha de generación de las recomendaciones
        /// </summary>
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Criterios utilizados para la recomendación
        /// </summary>
        public string Criterios { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Representa un producto recomendado con su puntuación
    /// </summary>
    public class ProductoRecomendado
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Nombre { get; set; } = string.Empty;
        
        /// <summary>
        /// Puntuación de recomendación (0-100)
        /// </summary>
        public int Puntuacion { get; set; }
        
        /// <summary>
        /// Razón principal de la recomendación
        /// </summary>
        public string RazonRecomendacion { get; set; } = string.Empty;
        
        /// <summary>
        /// ID de la categoría a la que pertenece el producto
        /// </summary>
        public Guid CategoriaId { get; set; }
        
        /// <summary>
        /// Nombre de la categoría
        /// </summary>
        public string CategoriaNombre { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Interfaz para la política de recomendación de productos.
    /// Esta política se encarga de analizar el historial de consumo y tendencias
    /// para recomendar productos a los clientes.
    /// </summary>
    public interface IProductoRecomendadoPolicy
    {
        /// <summary>
        /// Genera recomendaciones para un cliente específico basado en su historial
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cantidadRecomendaciones">Cantidad máxima de productos a recomendar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos recomendados</returns>
        Task<ResultadoProductoRecomendadoPolicy> GenerarRecomendacionesParaCliente(
            Guid clienteId, 
            int cantidadRecomendaciones = 5, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Genera recomendaciones basadas en productos populares globalmente
        /// </summary>
        /// <param name="cantidadRecomendaciones">Cantidad máxima de productos a recomendar</param>
        /// <param name="diasAnalisis">Días hacia atrás para analizar tendencias</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos recomendados</returns>
        Task<ResultadoProductoRecomendadoPolicy> GenerarRecomendacionesPopulares(
            int cantidadRecomendaciones = 5,
            int diasAnalisis = 30,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Genera recomendaciones para complementar una comanda actual
        /// </summary>
        /// <param name="comandaId">ID de la comanda actual</param>
        /// <param name="cantidadRecomendaciones">Cantidad máxima de productos a recomendar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos recomendados como complemento</returns>
        Task<ResultadoProductoRecomendadoPolicy> GenerarRecomendacionesComplementarias(
            Guid comandaId,
            int cantidadRecomendaciones = 3,
            CancellationToken cancellationToken = default);
    }
} 