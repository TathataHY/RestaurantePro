namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities
{
    /// <summary>
    /// Entidad que representa un ítem dentro de una orden de compra
    /// </summary>
    public class ItemOrdenCompra : EntityBase
    {
        /// <summary>
        /// ID de la orden de compra a la que pertenece este ítem
        /// </summary>
        public Guid OrdenCompraId { get; private set; }
        
        /// <summary>
        /// ID del ingrediente solicitado
        /// </summary>
        public Guid IngredienteId { get; private set; }
        
        /// <summary>
        /// Nombre del ingrediente solicitado
        /// </summary>
        public string NombreIngrediente { get; private set; }
        
        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public decimal Cantidad { get; private set; }
        
        /// <summary>
        /// Unidad de medida del ingrediente solicitado
        /// </summary>
        public RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida UnidadMedida { get; private set; }
        
        /// <summary>
        /// Precio unitario acordado
        /// </summary>
        public decimal PrecioUnitario { get; private set; }
        
        /// <summary>
        /// Subtotal (Cantidad * PrecioUnitario)
        /// </summary>
        public decimal Subtotal { get; private set; }
        
        /// <summary>
        /// Cantidad recibida
        /// </summary>
        public decimal CantidadRecibida { get; private set; }
        
        /// <summary>
        /// Indica si la recepción está completa (CantidadRecibida = Cantidad)
        /// </summary>
        public bool EstaCompletoEnRecepcion => CantidadRecibida == Cantidad;
        
        // Constructor privado para EF Core
        private ItemOrdenCompra() { }
        
        /// <summary>
        /// Crea un nuevo item de orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad solicitada</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <returns>Nuevo item de orden de compra</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public static ItemOrdenCompra Crear(
            Guid ordenCompraId, 
            Guid ingredienteId, 
            string nombreIngrediente,
            decimal cantidad, 
            RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            var item = new ItemOrdenCompra
            {
                OrdenCompraId = ordenCompraId,
                IngredienteId = ingredienteId,
                NombreIngrediente = nombreIngrediente,
                Cantidad = cantidad,
                UnidadMedida = unidadMedida,
                CantidadRecibida = 0
            };
            
            return item;
        }
        
        /// <summary>
        /// Actualiza la cantidad y precio del ítem
        /// </summary>
        /// <param name="nuevaCantidad">Nueva cantidad</param>
        /// <param name="nuevoPrecioUnitario">Nuevo precio unitario</param>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public void Actualizar(decimal nuevaCantidad, decimal nuevoPrecioUnitario)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));
                
            if (nuevoPrecioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(nuevoPrecioUnitario));
                
            Cantidad = nuevaCantidad;
            PrecioUnitario = nuevoPrecioUnitario;
            Subtotal = nuevaCantidad * nuevoPrecioUnitario;
            MarkAsModified();
        }

        /// <summary>
        /// Aumenta la cantidad solicitada del item
        /// </summary>
        /// <param name="cantidad">Cantidad a aumentar</param>
        public void AumentarCantidad(decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            Cantidad += cantidad;
            MarkAsModified();
        }
        
        /// <summary>
        /// Registra la cantidad recibida del item
        /// </summary>
        /// <param name="cantidadRecibida">Cantidad recibida</param>
        /// <param name="estadoOrden">Estado actual de la orden</param>
        /// <exception cref="ArgumentException">Si la cantidad no es válida</exception>
        /// <exception cref="InvalidOperationException">Si la orden no está recibida</exception>
        public void RegistrarRecepcion(decimal cantidadRecibida, EstadoOrdenCompra estadoOrden)
        {
            if (estadoOrden != EstadoOrdenCompra.Recibida)
                throw new InvalidOperationException("No se puede registrar la recepción porque la orden aún no ha sido recibida");
                
            if (cantidadRecibida < 0)
                throw new ArgumentException("La cantidad recibida no puede ser negativa", nameof(cantidadRecibida));
                
            if (cantidadRecibida > Cantidad)
                throw new ArgumentException("La cantidad recibida no puede ser mayor que la solicitada", nameof(cantidadRecibida));
                
            CantidadRecibida = cantidadRecibida;
            MarkAsModified();
            
            // Aquí podríamos emitir un evento de dominio si se completa la recepción
            if (EstaCompletoEnRecepcion)
            {
                // AddDomainEvent(new ItemOrdenCompraCompletadoEvent(Id, OrdenCompraId, IngredienteId));
            }
        }
        
        private OrdenCompra ObtenerOrden()
        {
            // En un entorno real, esto se haría a través de un repositorio
            // Para los tests, vamos a simular que podemos acceder a la orden
            throw new NotImplementedException("Esta función debe ser implementada en un entorno real con acceso a la BD");
        }
    }
} 
