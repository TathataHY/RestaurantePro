namespace RestaurantePro.Domain.Inventario.Ingredientes.Entities
{
    /// <summary>
    /// Agregado que representa un ingrediente en el inventario del restaurante.
    /// 
    /// Invariantes:
    /// - El stock nunca puede ser negativo
    /// - El stock mínimo nunca puede ser negativo
    /// - Todo movimiento de inventario debe estar registrado en la colección interna de movimientos
    /// - Un ingrediente desactivado no puede recibir movimientos de inventario
    /// - El nombre del ingrediente no puede estar vacío
    /// 
    /// Ciclo de vida:
    /// - Creación → Activo → [Desactivado ↔ Activado] → Eliminado lógico
    /// 
    /// Reglas de negocio:
    /// - Cuando el stock cae por debajo del mínimo, se emite un evento StockBajoMinimo
    /// - Los movimientos de inventario son inmutables y deben aplicarse secuencialmente
    /// - Las operaciones de modificación de stock generan movimientos que se registran internamente
    /// </summary>
    public class Ingrediente : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Código único del ingrediente
        /// </summary>
        public string Codigo { get; private set; }

        /// <summary>
        /// Descripción del ingrediente
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Unidad de medida del ingrediente
        /// </summary>
        public RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida UnidadMedida { get; private set; }

        /// <summary>
        /// Stock mínimo recomendado del ingrediente.
        /// Cuando el stock actual cae por debajo de este valor, se genera un evento StockBajoMinimo
        /// que puede desencadenar la generación automática de órdenes de compra.
        /// </summary>
        public decimal StockMinimo { get; private set; }

        /// <summary>
        /// Stock actual del ingrediente.
        /// Este valor se modifica exclusivamente a través de los métodos IncrementarStock y DecrementarStock
        /// que generan y registran los movimientos de inventario correspondientes.
        /// </summary>
        public decimal Stock { get; private set; }

        /// <summary>
        /// Indica si el ingrediente está activo en el sistema.
        /// Un ingrediente inactivo no debe utilizarse en nuevas comandas ni recibir movimientos de inventario.
        /// </summary>
        public bool EstaActivo { get; private set; }

        /// <summary>
        /// Movimientos de inventario asociados a este ingrediente.
        /// Esta colección interna mantiene un registro de todos los movimientos que afectan al stock.
        /// </summary>
        private readonly List<MovimientoInventario> _movimientos = new List<MovimientoInventario>();

        /// <summary>
        /// Acceso de solo lectura a los movimientos de inventario.
        /// Proporciona visibilidad a la colección interna sin permitir su modificación directa.
        /// </summary>
        public IReadOnlyCollection<MovimientoInventario> Movimientos => _movimientos.AsReadOnly();

        /// <summary>
        /// Proveedor principal para este ingrediente.
        /// Este ID se utiliza para generar órdenes de compra automáticas cuando el stock cae por debajo del mínimo.
        /// Relación por ID para mantener la independencia entre agregados.
        /// </summary>
        public Guid? ProveedorPrincipalId { get; private set; }
        
        /// <summary>
        /// Nivel de rotación del ingrediente, usado para priorizar en políticas de stock.
        /// </summary>
        public RotacionIngrediente Rotacion { get; private set; }
        
        /// <summary>
        /// Temporada principal del ingrediente.
        /// </summary>
        public TemporadaIngrediente Temporada { get; private set; }
        
        /// <summary>
        /// Indica si el ingrediente está bloqueado por control de calidad.
        /// </summary>
        public bool BloqueadoControlCalidad { get; private set; }
        
        /// <summary>
        /// Costo promedio en el inventario.
        /// </summary>
        public decimal CostoPromedio { get; private set; }

        // Constructor privado para EF Core
        private Ingrediente() { }

        /// <summary>
        /// Factory Method para crear un nuevo ingrediente.
        /// Este es el único punto de entrada para crear instancias válidas de Ingrediente.
        /// </summary>
        /// <param name="id">Identificador único del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="codigo">Código del ingrediente</param>
        /// <param name="descripcion">Descripción del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida del ingrediente</param>
        /// <param name="stockMinimo">Stock mínimo recomendado</param>
        /// <param name="stockActual">Stock actual del ingrediente</param>
        /// <param name="rotacion">Nivel de rotación del ingrediente (opcional)</param>
        /// <param name="temporada">Temporada del ingrediente (opcional)</param>
        /// <returns>Una nueva instancia de Ingrediente</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        /// <exception cref="BusinessRuleViolationException">Si se violan reglas de negocio</exception>
        public static Ingrediente Crear(
            Guid id,
            string nombre, 
            string codigo, 
            string descripcion, 
            RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida, 
            decimal stockMinimo, 
            decimal stockActual,
            RotacionIngrediente rotacion = RotacionIngrediente.Media,
            TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño)
        {
            // Validaciones usando excepciones estándar con nuestras excepciones específicas
            if (id == Guid.Empty)
                throw new ArgumentException("El ID no puede estar vacío", nameof(id));
                
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código no puede estar vacío", nameof(codigo));
                
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción no puede estar vacía", nameof(descripcion));
                
            if (stockMinimo < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo", nameof(stockMinimo));
                
            if (stockActual < 0)
                throw new ArgumentException("El stock actual no puede ser negativo", nameof(stockActual));

            // Validaciones de longitud
            if (nombre.Length > 200)
                throw new ArgumentException("El nombre no puede exceder 200 caracteres", nameof(nombre));
                
            if (codigo.Length > 50)
                throw new ArgumentException("El código no puede exceder 50 caracteres", nameof(codigo));
                
            if (descripcion.Length > 500)
                throw new ArgumentException("La descripción no puede exceder 500 caracteres", nameof(descripcion));

            var ingrediente = new Ingrediente
            {
                Id = id,
                Nombre = nombre,
                Codigo = codigo,
                Descripcion = descripcion,
                UnidadMedida = unidadMedida,
                StockMinimo = stockMinimo,
                Stock = stockActual,
                EstaActivo = true,
                Rotacion = rotacion,
                Temporada = temporada,
                BloqueadoControlCalidad = false,
                CostoPromedio = 0
            };

            ingrediente.AddDomainEvent(new Events.Ingrediente.IngredienteCreado(ingrediente.Id, nombre));

            return ingrediente;
        }

        /// <summary>
        /// Factory Method alternativo con ID generado automáticamente
        /// </summary>
        public static Ingrediente Crear(
            string nombre, 
            string codigo, 
            string descripcion, 
            RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida, 
            decimal stockMinimo, 
            decimal stockActual,
            RotacionIngrediente rotacion = RotacionIngrediente.Media,
            TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño)
        {
            return Crear(Guid.NewGuid(), nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual, rotacion, temporada);
        }

        /// <summary>
        /// Incrementa el stock del ingrediente.
        /// Genera un movimiento de inventario de tipo Ingreso y lo registra en la colección interna.
        /// </summary>
        /// <param name="cantidad">Cantidad a incrementar</param>
        /// <param name="motivo">Motivo del incremento (por ejemplo: "Compra", "Ajuste de inventario")</param>
        /// <returns>Movimiento de inventario generado</returns>
        /// <exception cref="BusinessRuleViolationException">Si el ingrediente está desactivado</exception>
        /// <exception cref="ArgumentException">Si la cantidad es negativa o cero</exception>
        public MovimientoInventario IncrementarStock(decimal cantidad, string motivo)
        {
            ValidarIngredienteActivo();
            
            Guard.AgainstNegativeOrZero(cantidad, nameof(cantidad));
            Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo));

            var movimiento = MovimientoInventario.CrearIngreso(Id, cantidad, motivo);

            // Aplicar el movimiento
            Stock = movimiento.Aplicar(Stock);
            MarkAsModified();

            // Agregar a la colección de movimientos
            _movimientos.Add(movimiento);

            // Verificar invariantes después de la modificación
            ValidarInvariantes();

            // Emitir evento de stock actualizado
            AddDomainEvent(new Events.Ingrediente.StockActualizado(Id, Nombre, Stock));

            return movimiento;
        }

        /// <summary>
        /// Decrementa el stock del ingrediente.
        /// Genera un movimiento de inventario de tipo Egreso y lo registra en la colección interna.
        /// Si el stock resultante es menor que el stock mínimo, se genera un evento StockBajoMinimo.
        /// </summary>
        /// <param name="cantidad">Cantidad a decrementar</param>
        /// <param name="motivo">Motivo del decremento (por ejemplo: "Consumo", "Merma")</param>
        /// <returns>Movimiento de inventario generado</returns>
        /// <exception cref="StockInsuficienteException">Si no hay suficiente stock</exception>
        /// <exception cref="BusinessRuleViolationException">Si el ingrediente está desactivado</exception>
        public MovimientoInventario DecrementarStock(decimal cantidad, string motivo)
        {
            ValidarIngredienteActivo();
            
            Guard.AgainstNegativeOrZero(cantidad, nameof(cantidad));
            Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo));

            // Usar nuestra excepción específica para stock insuficiente
            if (cantidad > Stock)
            {
                throw new StockInsuficienteException(
                    Id, 
                    Nombre, 
                    cantidad, 
                    Stock, 
                    $"decrementar stock por {motivo}");
            }

            var movimiento = MovimientoInventario.CrearEgreso(Id, cantidad, motivo);

            // Aplicar el movimiento
            Stock = movimiento.Aplicar(Stock);
            MarkAsModified();

            // Agregar a la colección de movimientos
            _movimientos.Add(movimiento);

            // Verificar invariantes después de la modificación
            ValidarInvariantes();

            // Emitir evento de stock actualizado
            AddDomainEvent(new Events.Ingrediente.StockActualizado(Id, Nombre, Stock));

            // Verificar si el stock está por debajo del mínimo
            if (Stock < StockMinimo)
            {
                AddDomainEvent(new Events.Ingrediente.StockBajoMinimo(Id, Nombre, Stock, StockMinimo));
            }

            return movimiento;
        }

        /// <summary>
        /// Reserva una cantidad específica de stock para una operación.
        /// No modifica el stock real, pero valida disponibilidad.
        /// </summary>
        /// <param name="cantidad">Cantidad a reservar</param>
        /// <param name="motivo">Motivo de la reserva</param>
        /// <exception cref="StockInsuficienteException">Si no hay suficiente stock para reservar</exception>
        public void ReservarStock(decimal cantidad, string motivo)
        {
            ValidarIngredienteActivo();
            
            Guard.AgainstNegativeOrZero(cantidad, nameof(cantidad));
            Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo));

            if (cantidad > Stock)
            {
                throw new StockInsuficienteException(
                    Id, 
                    Nombre, 
                    cantidad, 
                    Stock, 
                    $"reservar stock para {motivo}");
            }

            // Emitir evento de reserva (no modifica stock físico)
            // TODO: Implementar evento StockReservado
            // AddDomainEvent(new Events.Ingrediente.StockReservado(Id, Nombre, cantidad, motivo));
        }

        /// <summary>
        /// Actualiza el stock mínimo del ingrediente.
        /// Si el stock actual está por debajo del nuevo mínimo, genera un evento StockBajoMinimo.
        /// </summary>
        /// <param name="nuevoStockMinimo">Nuevo valor del stock mínimo</param>
        /// <exception cref="ArgumentException">Si el stock mínimo es negativo</exception>
        public void ActualizarStockMinimo(decimal nuevoStockMinimo)
        {
            Guard.AgainstNegative(nuevoStockMinimo, nameof(nuevoStockMinimo));

            if (nuevoStockMinimo == StockMinimo)
                return;

            var stockMinimoAnterior = StockMinimo;
            StockMinimo = nuevoStockMinimo;
            MarkAsModified();
            ValidarInvariantes();

            // TODO: Implementar evento StockMinimoActualizado
            // AddDomainEvent(new Events.Ingrediente.StockMinimoActualizado(Id, Nombre, stockMinimoAnterior, nuevoStockMinimo));

            // Verificar si el stock actual está por debajo del nuevo mínimo
            if (Stock < StockMinimo)
            {
                AddDomainEvent(new Events.Ingrediente.StockBajoMinimo(Id, Nombre, Stock, StockMinimo));
            }
        }

        /// <summary>
        /// Desactiva el ingrediente en el sistema.
        /// Un ingrediente desactivado no puede recibir movimientos de inventario.
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo)
                return;

            EstaActivo = false;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new Events.Ingrediente.IngredienteDesactivado(Id, Nombre));
        }

        /// <summary>
        /// Activa el ingrediente en el sistema.
        /// Permite que un ingrediente previamente desactivado vuelva a recibir movimientos.
        /// </summary>
        public void Activar()
        {
            if (EstaActivo)
                return;

            EstaActivo = true;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new Events.Ingrediente.IngredienteActivado(Id, Nombre));
        }

        /// <summary>
        /// Asocia un proveedor principal al ingrediente.
        /// </summary>
        /// <param name="proveedorId">ID del proveedor principal</param>
        public void AsociarProveedorPrincipal(Guid proveedorId)
        {
            Guard.AgainstEmpty(proveedorId, nameof(proveedorId));

            if (ProveedorPrincipalId == proveedorId)
                return;

            var proveedorAnterior = ProveedorPrincipalId;
            ProveedorPrincipalId = proveedorId;
            MarkAsModified();

            AddDomainEvent(new Events.Ingrediente.ProveedorPrincipalAsociado(Id, proveedorId));
        }

        /// <summary>
        /// Actualiza el nivel de rotación del ingrediente.
        /// </summary>
        /// <param name="rotacion">Nuevo nivel de rotación</param>
        public void ActualizarRotacion(RotacionIngrediente rotacion)
        {
            if (Rotacion == rotacion)
                return;

            var rotacionAnterior = Rotacion;
            Rotacion = rotacion;
            MarkAsModified();

            // TODO: Implementar evento RotacionActualizada
            // AddDomainEvent(new Events.Ingrediente.RotacionActualizada(Id, Nombre, rotacionAnterior, rotacion));
        }

        /// <summary>
        /// Actualiza la temporada del ingrediente.
        /// </summary>
        /// <param name="temporada">Nueva temporada</param>
        public void ActualizarTemporada(TemporadaIngrediente temporada)
        {
            if (Temporada == temporada)
                return;

            var temporadaAnterior = Temporada;
            Temporada = temporada;
            MarkAsModified();

            // TODO: Implementar evento TemporadaActualizada
            // AddDomainEvent(new Events.Ingrediente.TemporadaActualizada(Id, Nombre, temporadaAnterior, temporada));
        }

        /// <summary>
        /// Actualiza el estado de bloqueo por control de calidad.
        /// </summary>
        /// <param name="bloqueado">Si el ingrediente debe estar bloqueado</param>
        /// <param name="motivo">Motivo del bloqueo/desbloqueo</param>
        public void ActualizarBloqueoControlCalidad(bool bloqueado, string motivo)
        {
            Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo));

            if (BloqueadoControlCalidad == bloqueado)
                return;

            BloqueadoControlCalidad = bloqueado;
            MarkAsModified();

            // TODO: Implementar evento BloqueoControlCalidadActualizado
            // AddDomainEvent(new Events.Ingrediente.BloqueoControlCalidadActualizado(Id, Nombre, bloqueado, motivo));
        }

        /// <summary>
        /// Actualiza el costo promedio del ingrediente.
        /// </summary>
        /// <param name="nuevoCosto">Nuevo costo promedio</param>
        public void ActualizarCostoPromedio(decimal nuevoCosto)
        {
            Guard.AgainstNegative(nuevoCosto, nameof(nuevoCosto));

            if (CostoPromedio == nuevoCosto)
                return;

            var costoAnterior = CostoPromedio;
            CostoPromedio = nuevoCosto;
            MarkAsModified();

            AddDomainEvent(new Events.Ingrediente.CostoPromedioActualizado(Id, Nombre, nuevoCosto));
        }

        /// <summary>
        /// Verifica si hay suficiente stock para una cantidad específica.
        /// </summary>
        /// <param name="cantidadRequerida">Cantidad requerida</param>
        /// <returns>True si hay stock suficiente</returns>
        public bool TieneStockSuficiente(decimal cantidadRequerida)
        {
            Guard.AgainstNegative(cantidadRequerida, nameof(cantidadRequerida));
            return EstaActivo && Stock >= cantidadRequerida && !BloqueadoControlCalidad;
        }

        /// <summary>
        /// Calcula el déficit de stock si no hay suficiente para la cantidad requerida.
        /// </summary>
        /// <param name="cantidadRequerida">Cantidad requerida</param>
        /// <returns>Déficit de stock (0 si hay suficiente)</returns>
        public decimal CalcularDeficitStock(decimal cantidadRequerida)
        {
            Guard.AgainstNegative(cantidadRequerida, nameof(cantidadRequerida));
            return Math.Max(0, cantidadRequerida - Stock);
        }

        /// <summary>
        /// Determina si el ingrediente está en estado crítico (stock muy bajo).
        /// </summary>
        /// <returns>True si el stock está por debajo del 25% del mínimo</returns>
        public bool EstaEnEstadoCritico()
        {
            return EstaActivo && Stock < (StockMinimo * 0.25m);
        }

        /// <summary>
        /// Valida las invariantes del agregado Ingrediente.
        /// Se ejecuta después de cada operación que modifica el estado.
        /// </summary>
        /// <exception cref="BusinessRuleViolationException">Si alguna invariante es violada</exception>
        private void ValidarInvariantes()
        {
            // Validar que el stock nunca sea negativo
            if (Stock < 0)
            {
                throw BusinessRuleViolationException.ForInvalidState(
                    "Ingrediente",
                    $"Stock = {Stock}",
                    "Stock >= 0",
                    "Inventario",
                    Id);
            }

            // Validar que el stock mínimo nunca sea negativo
            if (StockMinimo < 0)
            {
                throw BusinessRuleViolationException.ForInvalidState(
                    "Ingrediente",
                    $"StockMinimo = {StockMinimo}",
                    "StockMinimo >= 0",
                    "Inventario",
                    Id);
            }

            // Validar que el costo promedio no sea negativo
            if (CostoPromedio < 0)
            {
                throw BusinessRuleViolationException.ForInvalidState(
                    "Ingrediente",
                    $"CostoPromedio = {CostoPromedio}",
                    "CostoPromedio >= 0",
                    "Inventario",
                    Id);
            }
        }

        /// <summary>
        /// Valida que el ingrediente esté activo para operaciones de modificación de stock.
        /// </summary>
        /// <exception cref="BusinessRuleViolationException">Si el ingrediente está inactivo</exception>
        private void ValidarIngredienteActivo()
        {
            if (!EstaActivo)
            {
                throw BusinessRuleViolationException.ForInactiveEntity(
                    "Ingrediente",
                    "Inventario",
                    Id);
            }

            if (BloqueadoControlCalidad)
            {
                throw BusinessRuleViolationException.ForOperationNotAllowed(
                    "Modificar stock",
                    "Ingrediente",
                    "El ingrediente está bloqueado por control de calidad",
                    "Inventario",
                    Id)
                    .WithData("NombreIngrediente", Nombre) as BusinessRuleViolationException;
            }
        }
    }
}
