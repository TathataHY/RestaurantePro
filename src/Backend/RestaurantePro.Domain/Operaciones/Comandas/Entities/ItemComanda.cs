namespace RestaurantePro.Domain.Operaciones.Comandas.Entities
{
    /// <summary>
    /// Representa un producto incluido en una comanda
    /// </summary>
    public class ItemComanda : EntityBase
    {
        /// <summary>
        /// ID de la comanda a la que pertenece este ítem
        /// </summary>
        public Guid ComandaId { get; private set; }

        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Cantidad solicitada del producto
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Precio unitario del producto al momento de crear la comanda
        /// </summary>
        public decimal PrecioUnitario { get; private set; }

        /// <summary>
        /// Subtotal (cantidad * precio unitario)
        /// </summary>
        public decimal Subtotal { get; private set; }

        /// <summary>
        /// Observaciones específicas para este ítem (e.g., "sin cebolla")
        /// </summary>
        public string Observaciones { get; private set; }

        /// <summary>
        /// Estado actual del ítem
        /// </summary>
        public EstadoItemComanda Estado { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'EnPreparacion'
        /// </summary>
        public DateTime? FechaPreparacion { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'Listo'
        /// </summary>
        public DateTime? FechaListo { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'Entregado'
        /// </summary>
        public DateTime? FechaEntrega { get; private set; }

        /// <summary>
        /// Fecha y hora en que pasó a estado 'Cancelado'
        /// </summary>
        public DateTime? FechaCancelacion { get; private set; }

        /// <summary>
        /// Motivo de cancelación (si aplica)
        /// </summary>
        public string MotivoCancelacion { get; private set; }

        /// <summary>
        /// Personalizaciones aplicadas a este ítem
        /// </summary>
        private readonly List<PersonalizacionItem> _personalizaciones = new List<PersonalizacionItem>();

        /// <summary>
        /// Personalizaciones aplicadas a este ítem (colección de solo lectura)
        /// </summary>
        public IReadOnlyCollection<PersonalizacionItem> Personalizaciones => _personalizaciones.AsReadOnly();

        // Constructor privado para EF Core
        private ItemComanda() { }

        /// <summary>
        /// Constructor principal
        /// </summary>
        public ItemComanda(Guid comandaId, Guid productoId, int cantidad, decimal precioUnitario, string observaciones = "")
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            if (precioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(precioUnitario));

            Id = Guid.NewGuid();
            ComandaId = comandaId;
            ProductoId = productoId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = cantidad * precioUnitario;
            Observaciones = observaciones ?? string.Empty;
            Estado = EstadoItemComanda.Pendiente;
            MotivoCancelacion = string.Empty;
            
            // No emitimos evento de creación, la comanda ya lo hace
        }
        
        /// <summary>
        /// Factory method para crear un nuevo ItemComanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda a la que pertenece</param>
        /// <param name="productoId">ID del producto</param>
        /// <param name="nombreProducto">Nombre del producto</param>
        /// <param name="cantidad">Cantidad solicitada</param>
        /// <param name="precioUnitario">Precio unitario</param>
        /// <param name="observaciones">Observaciones opcionales</param>
        /// <returns>Nuevo ItemComanda</returns>
        public static ItemComanda Crear(Guid comandaId, Guid productoId, string nombreProducto, int cantidad, decimal precioUnitario, string observaciones = "")
        {
            var item = new ItemComanda(comandaId, productoId, cantidad, precioUnitario, observaciones)
            {
                MotivoCancelacion = string.Empty
            };
            
            // Emitir evento de ítem de comanda creado
            item.AddDomainEvent(new Events.ItemComanda.ItemComandaCreado(
                comandaId,
                item.Id,
                productoId,
                nombreProducto,
                cantidad));
                
            return item;
        }

        /// <summary>
        /// Marca el ítem como "En Preparación"
        /// </summary>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda MarcarEnPreparacion()
        {
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se puede marcar como en preparación un ítem con estado {Estado}");
                
            Estado = EstadoItemComanda.EnPreparacion;
            FechaPreparacion = DateTime.Now;
            
            AddDomainEvent(new ItemComandaEnPreparacion(Id, ProductoId, ComandaId));
            
            return this;
        }
        
        /// <summary>
        /// Marca el ítem como "Listo" para ser entregado
        /// </summary>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda MarcarComoListo()
        {
            if (Estado != EstadoItemComanda.EnPreparacion)
                throw new InvalidOperationException($"No se puede marcar como listo un ítem que no está en preparación");
                
            Estado = EstadoItemComanda.Listo;
            FechaListo = DateTime.Now;
            
            AddDomainEvent(new ItemComandaListo(Id, ProductoId, ComandaId));
            
            return this;
        }
        
        /// <summary>
        /// Marca el ítem como "Entregado" al cliente
        /// Esto desencadena la actualización del inventario
        /// </summary>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda MarcarComoEntregado()
        {
            if (Estado != EstadoItemComanda.Listo)
                throw new InvalidOperationException($"No se puede entregar un ítem que no está listo");
                
            Estado = EstadoItemComanda.Entregado;
            FechaEntrega = DateTime.Now;
            
            AddDomainEvent(new ItemComandaEntregado(Id, ProductoId, ComandaId, Cantidad));
            
            return this;
        }
        
        /// <summary>
        /// Cancela el ítem con un motivo
        /// </summary>
        /// <param name="motivo">Razón por la que se cancela el ítem</param>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda Cancelar(string motivo)
        {
            if (Estado == EstadoItemComanda.Entregado || Estado == EstadoItemComanda.Cancelado)
                throw new InvalidOperationException($"No se puede cancelar un ítem con estado {Estado}");
                
            Estado = EstadoItemComanda.Cancelado;
            FechaCancelacion = DateTime.Now;
            MotivoCancelacion = motivo;
            
            // Agregamos el motivo a las observaciones para referencia histórica
            Observaciones = string.IsNullOrEmpty(Observaciones) 
                ? $"Cancelado: {motivo}" 
                : $"{Observaciones} | Cancelado: {motivo}";
                
            AddDomainEvent(new ItemComandaCancelado(Id, ProductoId, ComandaId, motivo));
            
            return this;
        }

        /// <summary>
        /// Actualiza la cantidad del producto en la comanda
        /// Solo permitido en estado Pendiente
        /// </summary>
        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(nuevaCantidad));
                
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se puede modificar la cantidad de un ítem con estado {Estado}");

            Cantidad = nuevaCantidad;
            RecalcularSubtotal();
        }

        /// <summary>
        /// Actualiza el precio unitario
        /// Solo permitido en estado Pendiente
        /// </summary>
        public void ActualizarPrecioUnitario(decimal nuevoPrecioUnitario)
        {
            if (nuevoPrecioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(nuevoPrecioUnitario));
                
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se puede modificar el precio de un ítem con estado {Estado}");

            PrecioUnitario = nuevoPrecioUnitario;
            RecalcularSubtotal();
        }

        /// <summary>
        /// Actualiza las observaciones del ítem
        /// Solo permitido en estado Pendiente o EnPreparacion
        /// </summary>
        public void ActualizarObservaciones(string nuevasObservaciones)
        {
            if (Estado != EstadoItemComanda.Pendiente && Estado != EstadoItemComanda.EnPreparacion)
                throw new InvalidOperationException($"No se pueden modificar las observaciones de un ítem con estado {Estado}");
                
            Observaciones = nuevasObservaciones ?? string.Empty;
        }

        /// <summary>
        /// Recalcula el subtotal basado en cantidad y precio unitario
        /// </summary>
        private void RecalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;
        }

        /// <summary>
        /// Método para obtener el ID del producto.
        /// Facilita el mockeo de la clase.
        /// </summary>
        public virtual Guid ObtenerProductoId()
        {
            return ProductoId;
        }
        
        /// <summary>
        /// Método para obtener la cantidad.
        /// Facilita el mockeo de la clase.
        /// </summary>
        public virtual int ObtenerCantidad()
        {
            return Cantidad;
        }

        /// <summary>
        /// Agrega una personalización para agregar mayor cantidad de un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a agregar</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad a agregar</param>
        /// <param name="precioAdicional">Precio adicional (si aplica)</param>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda AgregarPersonalizacionExtra(
            Guid ingredienteId, 
            string nombreIngrediente, 
            decimal cantidad, 
            decimal precioAdicional = 0)
        {
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se pueden agregar personalizaciones a un ítem con estado {Estado}");

            var personalizacion = PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidad,
                precioAdicional);

            _personalizaciones.Add(personalizacion);

            // Si tiene precio adicional, actualizar el precio del ítem
            if (personalizacion.AfectaPrecio())
            {
                PrecioUnitario += precioAdicional;
                RecalcularSubtotal();
            }

            AddDomainEvent(new PersonalizacionAgregadaAItem(Id, ComandaId, PersonalizacionItemDto.FromPersonalizacionItem(personalizacion)));

            return this;
        }

        /// <summary>
        /// Agrega una personalización para quitar un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a quitar</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda AgregarPersonalizacionQuitar(
            Guid ingredienteId, 
            string nombreIngrediente)
        {
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se pueden agregar personalizaciones a un ítem con estado {Estado}");

            var personalizacion = PersonalizacionItem.CrearQuitar(ingredienteId, nombreIngrediente);
            _personalizaciones.Add(personalizacion);

            AddDomainEvent(new PersonalizacionAgregadaAItem(Id, ComandaId, PersonalizacionItemDto.FromPersonalizacionItem(personalizacion)));

            return this;
        }

        /// <summary>
        /// Agrega una personalización para sustituir un ingrediente por otro
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a sustituir</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente a sustituir</param>
        /// <param name="ingredienteSustitucionId">ID del ingrediente de sustitución</param>
        /// <param name="nombreIngredienteSustitucion">Nombre del ingrediente de sustitución</param>
        /// <param name="cantidad">Cantidad del ingrediente de sustitución</param>
        /// <param name="precioAdicional">Precio adicional por la sustitución (si aplica)</param>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda AgregarPersonalizacionSustituir(
            Guid ingredienteId, 
            string nombreIngrediente,
            Guid ingredienteSustitucionId,
            string nombreIngredienteSustitucion,
            decimal cantidad = 1,
            decimal precioAdicional = 0)
        {
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se pueden agregar personalizaciones a un ítem con estado {Estado}");

            var personalizacion = PersonalizacionItem.CrearSustituir(
                ingredienteId,
                nombreIngrediente,
                ingredienteSustitucionId,
                nombreIngredienteSustitucion,
                cantidad,
                precioAdicional);

            _personalizaciones.Add(personalizacion);

            // Si tiene precio adicional, actualizar el precio del ítem
            if (personalizacion.AfectaPrecio())
            {
                PrecioUnitario += precioAdicional;
                RecalcularSubtotal();
            }

            AddDomainEvent(new PersonalizacionAgregadaAItem(Id, ComandaId, PersonalizacionItemDto.FromPersonalizacionItem(personalizacion)));

            return this;
        }

        /// <summary>
        /// Elimina una personalización específica
        /// </summary>
        /// <param name="personalizacion">Personalización a eliminar</param>
        /// <returns>El ítem actualizado para chaining</returns>
        public ItemComanda EliminarPersonalizacion(PersonalizacionItem personalizacion)
        {
            if (Estado != EstadoItemComanda.Pendiente)
                throw new InvalidOperationException($"No se pueden eliminar personalizaciones de un ítem con estado {Estado}");

            if (_personalizaciones.Contains(personalizacion))
            {
                _personalizaciones.Remove(personalizacion);

                // Si tenía precio adicional, actualizar el precio del ítem
                if (personalizacion.AfectaPrecio())
                {
                    PrecioUnitario -= personalizacion.PrecioAdicional;
                    RecalcularSubtotal();
                }

                AddDomainEvent(new PersonalizacionEliminadaDeItem(Id, ComandaId, PersonalizacionItemDto.FromPersonalizacionItem(personalizacion)));
            }

            return this;
        }

        /// <summary>
        /// Verifica si el ítem tiene personalizaciones
        /// </summary>
        /// <returns>True si tiene al menos una personalización, false en caso contrario</returns>
        public bool TienePersonalizaciones() => _personalizaciones.Any();

        /// <summary>
        /// Calcula el precio adicional total por personalizaciones
        /// </summary>
        /// <returns>Suma de precios adicionales de todas las personalizaciones</returns>
        public decimal CalcularPrecioAdicionalPersonalizaciones()
        {
            return _personalizaciones.Sum(p => p.PrecioAdicional);
        }
    }
}
