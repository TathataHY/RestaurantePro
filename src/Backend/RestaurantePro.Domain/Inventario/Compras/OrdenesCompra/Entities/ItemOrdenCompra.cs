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
            ValidarInvariantes();
        }

        /// <summary>
        /// Actualiza solo la cantidad del ítem
        /// </summary>
        /// <param name="nuevaCantidad">Nueva cantidad</param>
        public void ActualizarCantidad(decimal nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));
            Cantidad = nuevaCantidad;
            Subtotal = Cantidad * PrecioUnitario;
            MarkAsModified();
            ValidarInvariantes();
        }

        /// <summary>
        /// Actualiza solo el precio unitario del ítem
        /// </summary>
        /// <param name="nuevoPrecioUnitario">Nuevo precio unitario</param>
        public void ActualizarPrecio(decimal nuevoPrecioUnitario)
        {
            if (nuevoPrecioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(nuevoPrecioUnitario));
            PrecioUnitario = nuevoPrecioUnitario;
            Subtotal = Cantidad * PrecioUnitario;
            MarkAsModified();
            ValidarInvariantes();
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
            Subtotal = Cantidad * PrecioUnitario;
            MarkAsModified();
            ValidarInvariantes();
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
            ValidarInvariantes();
            
            // Emitir evento de dominio si se completa la recepción
            if (EstaCompletoEnRecepcion)
            {
                AddDomainEvent(new ItemOrdenCompraCompletado(Id, OrdenCompraId, IngredienteId, CantidadRecibida));
            }
        }
        
        /// <summary>
        /// Valida todas las invariantes del ítem.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            if (OrdenCompraId == Guid.Empty)
                throw new InvalidOperationException("El item debe pertenecer a una orden de compra");
                
            if (IngredienteId == Guid.Empty)
                throw new InvalidOperationException("El item debe estar asociado a un ingrediente");
                
            if (Cantidad <= 0)
                throw new InvalidOperationException($"La cantidad debe ser mayor que cero. Valor actual: {Cantidad}");
                
            if (PrecioUnitario < 0)
                throw new InvalidOperationException($"El precio unitario no puede ser negativo. Valor actual: {PrecioUnitario}");
                
            decimal subtotalCalculado = Cantidad * PrecioUnitario;
            if (Math.Abs(Subtotal - subtotalCalculado) > 0.01m)
                throw new InvalidOperationException($"Inconsistencia en el subtotal. Calculado: {subtotalCalculado}, Actual: {Subtotal}");
            
            if (CantidadRecibida < 0)
                throw new InvalidOperationException($"La cantidad recibida no puede ser negativa. Valor actual: {CantidadRecibida}");
                
            if (CantidadRecibida > Cantidad)
                throw new InvalidOperationException($"La cantidad recibida no puede ser mayor que la solicitada. Recibida: {CantidadRecibida}, Solicitada: {Cantidad}");
        }

        private OrdenCompra ObtenerOrden()
        {
            // En un entorno real, esto se haría a través de un repositorio
            // Para los tests, vamos a simular que podemos acceder a la orden
            throw new NotImplementedException("Esta función debe ser implementada en un entorno real con acceso a la BD");
        }
    }
} 
