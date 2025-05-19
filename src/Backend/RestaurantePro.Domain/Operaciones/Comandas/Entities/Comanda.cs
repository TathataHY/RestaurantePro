namespace RestaurantePro.Domain.Operaciones.Comandas.Entities
{
    /// <summary>
    /// Agregado que representa una comanda (orden) en el restaurante.
    /// 
    /// Invariantes:
    /// - Una comanda debe tener una mesa asignada y un mesero responsable
    /// - Una comanda finalizada debe tener al menos un producto
    /// - El total de la comanda debe reflejar la suma de los productos con impuestos y descuentos
    /// - Las transiciones de estado deben seguir el flujo establecido: Creada → EnProceso → Lista → Entregada → Finalizada
    /// - Una comanda cancelada no puede ser reactivada
    /// 
    /// Ciclo de vida:
    /// - Creación → EnProceso → Lista → Entregada → Finalizada
    ///           ↘ Cancelada
    /// 
    /// Reglas de negocio:
    /// - Solo se pueden agregar/modificar productos en comandas con estado Creada o EnProceso
    /// - Los descuentos se aplican sobre el subtotal y afectan el cálculo del total
    /// - Cada cambio de estado genera eventos de dominio para notificar a otros subsistemas
    /// - La finalización de una comanda genera eventos para acumular puntos de fidelización
    /// </summary>
    public class Comanda : EntityBase, IAggregateRoot
    {
        private readonly List<ItemComanda> _items = new List<ItemComanda>();

        /// <summary>
        /// ID de la mesa asociada a la comanda
        /// </summary>
        public Guid MesaId { get; private set; }

        /// <summary>
        /// ID del usuario (mesero) que creó la comanda
        /// </summary>
        public Guid MeseroId { get; private set; }

        /// <summary>
        /// ID del cliente asociado a la comanda.
        /// Opcional, pero necesario para aplicar descuentos de fidelización.
        /// </summary>
        public Guid? ClienteId { get; private set; }

        /// <summary>
        /// Fecha de creación de la comanda
        /// </summary>
        public new DateTime FechaCreacion { get; private set; }

        /// <summary>
        /// Fecha de actualización de la comanda.
        /// Se actualiza automáticamente con cada cambio en la comanda.
        /// </summary>
        public new DateTime? FechaActualizacion { get; private set; }

        /// <summary>
        /// Estado actual de la comanda.
        /// Define las operaciones permitidas y el flujo de proceso.
        /// </summary>
        public EstadoComanda Estado { get; private set; }

        /// <summary>
        /// Observaciones adicionales para la comanda.
        /// Puede incluir requisitos especiales, notas para cocina, etc.
        /// </summary>
        public string? Observaciones { get; private set; }

        /// <summary>
        /// Total de la comanda.
        /// Objeto de valor que encapsula el cálculo de subtotal, impuestos y total.
        /// </summary>
        public TotalComanda? Total { get; private set; }

        /// <summary>
        /// Descuento por fidelización aplicado a la comanda.
        /// Solo aplica cuando la comanda está asociada a un cliente con tarjeta de fidelización.
        /// </summary>
        public decimal? DescuentoFidelizacion { get; private set; }

        /// <summary>
        /// Detalles de los productos incluidos en la comanda.
        /// Colección de solo lectura para preservar la encapsulación.
        /// </summary>
        public IReadOnlyCollection<ItemComanda> Items => _items.AsReadOnly();

        /// <summary>
        /// Constructor privado para EF Core.
        /// La creación de comandas debe hacerse a través del factory method Crear().
        /// </summary>
        private Comanda() { }

        /// <summary>
        /// Factory method para crear una nueva comanda.
        /// Este es el único punto de entrada para crear instancias válidas.
        /// </summary>
        /// <param name="mesaId">ID de la mesa donde se crea la comanda</param>
        /// <param name="meseroId">ID del mesero responsable</param>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="observaciones">Observaciones iniciales (opcional)</param>
        /// <returns>Una nueva instancia de Comanda en estado Creada</returns>
        public static Comanda Crear(Guid mesaId, Guid meseroId, Guid? clienteId = null, string? observaciones = null)
        {
            var comanda = new Comanda
            {
                Id = Guid.NewGuid(),
                MesaId = mesaId,
                MeseroId = meseroId,
                ClienteId = clienteId,
                FechaCreacion = DateTime.Now,
                Estado = EstadoComanda.Creada,
                Observaciones = observaciones ?? string.Empty,
                Total = TotalComanda.Crear(0, 0)
            };

            comanda.AddDomainEvent(new ComandaCreada(comanda.Id, mesaId, meseroId));

            return comanda;
        }

        /// <summary>
        /// Agrega un producto a la comanda.
        /// Solo puede ejecutarse para comandas en estado Creada o EnProceso.
        /// </summary>
        /// <param name="productoId">ID del producto a agregar</param>
        /// <param name="cantidad">Cantidad del producto</param>
        /// <param name="precioUnitario">Precio unitario del producto</param>
        /// <param name="observaciones">Observaciones específicas para este producto</param>
        /// <exception cref="InvalidOperationException">Si la comanda no está en estado Creada o EnProceso</exception>
        public void AgregarProducto(Guid productoId, int cantidad, decimal precioUnitario, string? observaciones = null)
        {
            ValidarComandaActiva();

            var item = new ItemComanda(Id, productoId, cantidad, precioUnitario, observaciones ?? string.Empty);
            _items.Add(item);

            RecalcularTotal();
            ActualizarFecha();
            ValidarInvariantes();

            AddDomainEvent(new ProductoAgregadoAComanda(Id, productoId, cantidad));
        }

        /// <summary>
        /// Actualiza el estado de la comanda siguiendo el flujo establecido.
        /// Valida que la transición sea correcta según las reglas de negocio.
        /// </summary>
        /// <param name="nuevoEstado">Nuevo estado de la comanda</param>
        /// <exception cref="InvalidOperationException">Si la transición de estado no es válida</exception>
        public void ActualizarEstado(EstadoComanda nuevoEstado)
        {
            // Validar transición de estado válida
            if (!EsTransicionEstadoValida(nuevoEstado))
            {
                throw new InvalidOperationException($"No se puede cambiar el estado de {Estado} a {nuevoEstado}");
            }

            // Validar que una comanda no pueda finalizarse sin productos
            if (nuevoEstado == EstadoComanda.Finalizada && !Items.Any())
            {
                throw new InvalidOperationException("No se puede finalizar una comanda sin productos");
            }

            var estadoAnterior = Estado;
            Estado = nuevoEstado;
            ActualizarFecha();
            ValidarInvariantes();

            AddDomainEvent(new EstadoComandaActualizado(Id, estadoAnterior, nuevoEstado));

            // Si la comanda se finaliza, agregar evento específico
            if (nuevoEstado == EstadoComanda.Finalizada)
            {
                AddDomainEvent(new ComandaFinalizada(Id, Total!.Total));
            }
        }

        /// <summary>
        /// Método para cancelar la comanda
        /// </summary>
        public void Cancelar(string motivo)
        {
            ValidarComandaActiva();

            Estado = EstadoComanda.Cancelada;
            Observaciones = string.IsNullOrEmpty(Observaciones)
                ? $"Cancelada: {motivo}"
                : $"{Observaciones} | Cancelada: {motivo}";

            ActualizarFecha();
            ValidarInvariantes();

            AddDomainEvent(new ComandaCancelada(Id, motivo));
        }

        /// <summary>
        /// Verifica si la comanda tiene un descuento por fidelización aplicado
        /// </summary>
        /// <returns>True si tiene descuento aplicado, false en caso contrario</returns>
        public bool TieneDescuentoFidelizacion()
        {
            return DescuentoFidelizacion.HasValue && DescuentoFidelizacion > 0;
        }

        /// <summary>
        /// Aplica un descuento de fidelización a la comanda
        /// </summary>
        /// <param name="porcentajeDescuento">Porcentaje de descuento a aplicar (entre 0 y 1)</param>
        public void AplicarDescuentoFidelizacion(decimal porcentajeDescuento)
        {
            if (porcentajeDescuento < 0 || porcentajeDescuento > 1)
                throw new ArgumentException("El porcentaje de descuento debe estar entre 0 y 1", nameof(porcentajeDescuento));

            // Solo se puede aplicar a comandas activas
            ValidarComandaActiva();

            // Calculamos el descuento sobre el subtotal
            DescuentoFidelizacion = Math.Round(Total!.Subtotal * porcentajeDescuento, 2);

            // Recalculamos el total con el descuento
            RecalcularTotal();
            ActualizarFecha();
            ValidarInvariantes();

            // Agregamos un evento de descuento aplicado (si se necesita implementar)
            // AddDomainEvent(new DescuentoFidelizacionAplicado(Id, DescuentoFidelizacion.Value, porcentajeDescuento));
        }

        /// <summary>
        /// Método para remover un producto específico de la comanda
        /// </summary>
        /// <param name="itemId">Id del item a remover</param>
        /// <exception cref="InvalidOperationException">Si la comanda no está en estado activo o el item no existe</exception>
        public void RemoverProducto(Guid itemId)
        {
            ValidarComandaActiva();
            
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new InvalidOperationException($"No existe un item con ID {itemId} en esta comanda");
                
            _items.Remove(item);
            
            RecalcularTotal();
            ActualizarFecha();
            ValidarInvariantes();
            
            AddDomainEvent(new ProductoRemovidoDeComanda(Id, item.ProductoId, item.Cantidad));
        }

        /// <summary>
        /// Valida todas las invariantes del agregado Comanda.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            // Verificar que el estado sea válido
            if (!Enum.IsDefined(typeof(EstadoComanda), Estado))
                throw new InvalidOperationException($"Estado de comanda no válido: {Estado}");
            
            // Verificar que la comanda finalizada tiene productos
            if (Estado == EstadoComanda.Finalizada && !Items.Any())
                throw new InvalidOperationException("Una comanda finalizada debe tener al menos un producto");
            
            // Verificar que el total sea consistente con los items
            decimal subtotalCalculado = _items.Sum(i => i.Subtotal);
            decimal descuento = DescuentoFidelizacion ?? 0;
            
            if (Total == null)
                throw new InvalidOperationException("El total de la comanda no puede ser nulo");
                
            if (Math.Abs(Total.Subtotal - subtotalCalculado) > 0.01m)
                throw new InvalidOperationException($"Inconsistencia en el subtotal de la comanda. Calculado: {subtotalCalculado}, Actual: {Total.Subtotal}");
            
            // Verificar que el descuento sea válido
            if (DescuentoFidelizacion.HasValue)
            {
                if (DescuentoFidelizacion < 0)
                    throw new InvalidOperationException("El descuento no puede ser negativo");
                    
                if (DescuentoFidelizacion > subtotalCalculado)
                    throw new InvalidOperationException("El descuento no puede ser mayor que el subtotal");
            }
            
            // Verificar que cada item tenga un precio y cantidad válidos
            foreach (var item in _items)
            {
                if (item.ComandaId != Id)
                    throw new InvalidOperationException($"Item con ID {item.Id} pertenece a otra comanda");
                    
                if (item.Cantidad <= 0)
                    throw new InvalidOperationException($"Item con ID {item.Id} tiene cantidad inválida: {item.Cantidad}");
                    
                if (item.PrecioUnitario < 0)
                    throw new InvalidOperationException($"Item con ID {item.Id} tiene precio unitario inválido: {item.PrecioUnitario}");
            }
            
            // VALIDACIONES EXISTENTES
            
            // Validar IDs obligatorios
            if (MesaId == Guid.Empty)
                throw new InvalidOperationException("La comanda debe tener una mesa asignada");
                
            if (MeseroId == Guid.Empty)
                throw new InvalidOperationException("La comanda debe tener un mesero asignado");
                
            // Validar fechas
            if (FechaCreacion == default)
                throw new InvalidOperationException("La fecha de creación no puede ser la fecha predeterminada");
                
            if (FechaActualizacion.HasValue && FechaActualizacion < FechaCreacion)
                throw new InvalidOperationException("La fecha de actualización no puede ser anterior a la fecha de creación");
                
            // Validar coherencia de estado con propiedades
            if (Estado == EstadoComanda.Cancelada && string.IsNullOrWhiteSpace(Observaciones))
                throw new InvalidOperationException("Una comanda cancelada debe incluir observaciones con el motivo de cancelación");
                
            // Límites en campos de texto
            if (!string.IsNullOrEmpty(Observaciones) && Observaciones.Length > 500)
                throw new InvalidOperationException("Las observaciones no pueden exceder los 500 caracteres");
                
            // Validar coherencia de descuentos con cliente
            if (DescuentoFidelizacion > 0 && !ClienteId.HasValue)
                throw new InvalidOperationException("No se puede aplicar descuento de fidelización sin un cliente asociado");
                
            // Validar límites y coherencia de valores monetarios
            if (Total.Total < 0)
                throw new InvalidOperationException("El total de la comanda no puede ser negativo");
                
            if (Total.Total > 1000000m) // Un límite razonable para una comanda
                throw new InvalidOperationException("El total de la comanda excede el límite máximo permitido");
                
            // Validar límite de items en la comanda
            if (_items.Count > 100) // Un límite razonable para los items en una comanda
                throw new InvalidOperationException("La comanda excede el número máximo de items permitidos");
                
            // Validar que no haya duplicados de productos si el modelo de negocio no lo permite
            var productosUnicos = _items.Select(i => i.ProductoId).Distinct().Count();
            if (productosUnicos != _items.Count)
                throw new InvalidOperationException("Existen productos duplicados en la comanda. Use la función de modificar cantidad en lugar de agregar el mismo producto múltiples veces");
                
            // Validar que el impuesto calculado sea correcto (asumiendo 16% de IVA)
            decimal subtotalConDescuento = subtotalCalculado - descuento;
            decimal impuestoEsperado = Math.Round(subtotalConDescuento * 0.16m, 2);
            
            if (Math.Abs(Total.Impuestos - impuestoEsperado) > 0.01m)
                throw new InvalidOperationException($"Inconsistencia en los impuestos. Esperado: {impuestoEsperado}, Actual: {Total.Impuestos}");
                
            // Validar coherencia del total
            decimal totalCalculado = subtotalConDescuento + impuestoEsperado;
            
            if (Math.Abs(Total.Total - totalCalculado) > 0.01m)
                throw new InvalidOperationException($"Inconsistencia en el total. Calculado: {totalCalculado}, Actual: {Total.Total}");
            
            // VALIDACIONES ADICIONALES
            
            // Validar que la comanda entregada tenga todos los productos necesarios
            if (Estado == EstadoComanda.Entregada && _items.Count == 0)
                throw new InvalidOperationException("Una comanda entregada debe tener al menos un producto");
            
            // Validar que la comanda lista tenga todos los productos necesarios
            if (Estado == EstadoComanda.Lista && _items.Count == 0)
                throw new InvalidOperationException("Una comanda lista debe tener al menos un producto");
            
            // Validar consistencia de eventos de dominio
            if (DomainEvents.Count == 0)
                throw new InvalidOperationException("La comanda debe tener al menos un evento de dominio registrado");
            
            // Validar que no existan cantidades excesivas por item individual
            foreach (var item in _items)
            {
                if (item.Cantidad > 50) // Un límite razonable para un solo producto en una comanda
                    throw new InvalidOperationException($"La cantidad del item con ID {item.Id} excede el límite máximo permitido por item");
                
                if (item.PrecioUnitario > 100000m) // Un límite razonable para el precio unitario
                    throw new InvalidOperationException($"El precio unitario del item con ID {item.Id} excede el límite máximo permitido");
                
                if (!string.IsNullOrEmpty(item.Observaciones) && item.Observaciones.Length > 200)
                    throw new InvalidOperationException($"Las observaciones del item con ID {item.Id} no pueden exceder los 200 caracteres");
            }
            
            // Validar coherencia del ciclo de vida
            if (Estado == EstadoComanda.Finalizada && !FechaActualizacion.HasValue)
                throw new InvalidOperationException("Una comanda finalizada debe tener fecha de actualización");
            
            // Validar que la diferencia entre la fecha de creación y actualización no sea excesiva
            if (FechaActualizacion.HasValue && (FechaActualizacion.Value - FechaCreacion).TotalDays > 30)
                throw new InvalidOperationException("La comanda no puede estar activa por más de 30 días");
            
            // Validar rangos válidos para descuentos según política de negocio
            if (DescuentoFidelizacion.HasValue && DescuentoFidelizacion.Value > 0)
            {
                decimal porcentajeDescuento = DescuentoFidelizacion.Value / subtotalCalculado;
                if (porcentajeDescuento > 0.5m) // Máximo 50% de descuento permitido
                    throw new InvalidOperationException("El descuento no puede exceder el 50% del subtotal");
                
                // En caso de tener rangos específicos de descuento permitidos
                // decimal[] rangosPermitidos = new decimal[] { 0.05m, 0.10m, 0.15m, 0.20m, 0.25m, 0.5m };
                // if (!rangosPermitidos.Any(r => Math.Abs(porcentajeDescuento - r) < 0.01m))
                //    throw new InvalidOperationException($"El porcentaje de descuento {porcentajeDescuento:P0} no es un valor permitido");
            }
        }

        /// <summary>
        /// Métodos privados para validaciones y lógica interna
        /// </summary>
        private void ValidarComandaActiva()
        {
            if (Estado != EstadoComanda.Creada && Estado != EstadoComanda.EnProceso)
            {
                throw new InvalidOperationException($"No se pueden realizar cambios en una comanda con estado {Estado}");
            }
        }

        private bool EsTransicionEstadoValida(EstadoComanda nuevoEstado)
        {
            return (Estado, nuevoEstado) switch
            {
                (EstadoComanda.Creada, EstadoComanda.EnProceso) => true,
                (EstadoComanda.EnProceso, EstadoComanda.Lista) => true,
                (EstadoComanda.Lista, EstadoComanda.Entregada) => true,
                (EstadoComanda.Entregada, EstadoComanda.Finalizada) => true,
                _ => false
            };
        }

        private void RecalcularTotal()
        {
            decimal subtotal = _items.Sum(i => i.Subtotal);
            
            // Aplicar descuento si existe
            decimal subtotalConDescuento = subtotal;
            if (DescuentoFidelizacion.HasValue && DescuentoFidelizacion > 0)
            {
                subtotalConDescuento = subtotal - DescuentoFidelizacion.Value;
            }
            
            decimal impuesto = subtotalConDescuento * 0.16m; // IVA del 16%

            Total = TotalComanda.Crear(subtotal, impuesto, DescuentoFidelizacion);
        }

        private void ActualizarFecha()
        {
            FechaActualizacion = DateTime.Now;
        }
    }
}
