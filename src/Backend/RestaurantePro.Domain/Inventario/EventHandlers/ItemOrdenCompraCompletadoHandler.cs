namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador para el evento ItemOrdenCompraCompletado
    /// Cuando un item de orden de compra se completa, se incrementa el stock del ingrediente correspondiente
    /// </summary>
    public class ItemOrdenCompraCompletadoHandler : IDomainEventHandler<Compras.OrdenesCompra.Events.ItemOrdenCompra.ItemOrdenCompraCompletado>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteRepository">Repositorio de ingredientes</param>
        public ItemOrdenCompraCompletadoHandler(IIngredienteRepository ingredienteRepository)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        }
        
        /// <summary>
        /// Maneja el evento cuando un item de orden de compra se completa
        /// </summary>
        /// <param name="notification">Evento de item completado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(Compras.OrdenesCompra.Events.ItemOrdenCompra.ItemOrdenCompraCompletado notification, CancellationToken cancellationToken)
        {
            // Obtener el ingrediente por su ID
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(notification.IngredienteId, cancellationToken);
            
            if (ingrediente == null)
            {
                // Manejar el caso donde no se encuentra el ingrediente
                // Podría lanzar una excepción, registrar un evento de error, etc.
                return;
            }
            
            // Incrementar el stock del ingrediente con la cantidad recibida
            string motivo = $"Recepción de orden de compra {notification.OrdenCompraId}";
            ingrediente.IncrementarStock(notification.CantidadRecibida, motivo);
            
            // Actualizar el ingrediente en el repositorio
            await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
        }
    }
} 