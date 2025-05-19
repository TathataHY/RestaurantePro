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
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="codigo">Código del ingrediente</param>
        /// <param name="descripcion">Descripción del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida del ingrediente</param>
        /// <param name="stockMinimo">Stock mínimo recomendado</param>
        /// <param name="stockActual">Stock actual del ingrediente</param>
        /// <param name="rotacion">Nivel de rotación del ingrediente (opcional)</param>
        /// <param name="temporada">Temporada del ingrediente (opcional)</param>
        /// <returns>Una nueva instancia de Ingrediente</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos (nombre vacío o stock mínimo negativo)</exception>
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
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));

            if (stockMinimo < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo", nameof(stockMinimo));

            var ingrediente = new Ingrediente
            {
                Nombre = nombre,
                UnidadMedida = unidadMedida,
                StockMinimo = stockMinimo,
                Stock = stockActual,
                EstaActivo = true,
                Rotacion = rotacion,
                Temporada = temporada,
                BloqueadoControlCalidad = false,
                CostoPromedio = 0
            };

            ingrediente.AddDomainEvent(new IngredienteCreado(ingrediente.Id, nombre));

            return ingrediente;
        }

        /// <summary>
        /// Incrementa el stock del ingrediente.
        /// Genera un movimiento de inventario de tipo Ingreso y lo registra en la colección interna.
        /// </summary>
        /// <param name="cantidad">Cantidad a incrementar</param>
        /// <param name="motivo">Motivo del incremento (por ejemplo: "Compra", "Ajuste de inventario")</param>
        /// <returns>Movimiento de inventario generado</returns>
        /// <exception cref="ArgumentException">Si la cantidad es negativa o cero</exception>
        /// <exception cref="InvalidOperationException">Si el ingrediente está desactivado</exception>
        public MovimientoInventario IncrementarStock(decimal cantidad, string motivo)
        {
            ValidarIngredienteActivo();
            
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            var movimiento = MovimientoInventario.CrearIngreso(Id, cantidad, motivo);

            // Aplicar el movimiento
            Stock = movimiento.Aplicar(Stock);
            MarkAsModified();

            // Agregar a la colección de movimientos
            _movimientos.Add(movimiento);

            // Verificar invariantes después de la modificación
            ValidarInvariantes();

            // Emitir evento de stock actualizado
            AddDomainEvent(new StockActualizado(Id, Nombre, Stock));

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
        /// <exception cref="ArgumentException">Si la cantidad es negativa o cero</exception>
        /// <exception cref="InvalidOperationException">Si no hay suficiente stock o si el ingrediente está desactivado</exception>
        public MovimientoInventario DecrementarStock(decimal cantidad, string motivo)
        {
            ValidarIngredienteActivo();
            
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            if (cantidad > Stock)
                throw new InvalidOperationException($"No hay suficiente stock disponible de {Nombre}. Disponible: {Stock}, Solicitado: {cantidad}");

            var movimiento = MovimientoInventario.CrearEgreso(Id, cantidad, motivo);

            // Aplicar el movimiento
            Stock = movimiento.Aplicar(Stock);
            MarkAsModified();

            // Agregar a la colección de movimientos
            _movimientos.Add(movimiento);

            // Verificar invariantes después de la modificación
            ValidarInvariantes();

            // Verificar si estamos por debajo del stock mínimo
            if (Stock < StockMinimo)
            {
                AddDomainEvent(new StockBajoMinimo(Id, Nombre, Stock, StockMinimo));
            }

            // Emitir evento de stock actualizado
            AddDomainEvent(new StockActualizado(Id, Nombre, Stock));

            return movimiento;
        }

        /// <summary>
        /// Actualiza el stock mínimo del ingrediente.
        /// Si el stock actual es menor que el nuevo stock mínimo, genera un evento StockBajoMinimo.
        /// </summary>
        /// <param name="nuevoStockMinimo">Nuevo valor de stock mínimo</param>
        /// <exception cref="ArgumentException">Si el valor es negativo</exception>
        public void ActualizarStockMinimo(decimal nuevoStockMinimo)
        {
            if (nuevoStockMinimo < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo", nameof(nuevoStockMinimo));

            StockMinimo = nuevoStockMinimo;
            MarkAsModified();

            // Verificar invariantes después de la modificación
            ValidarInvariantes();

            if (Stock < StockMinimo)
            {
                AddDomainEvent(new StockBajoMinimo(Id, Nombre, Stock, StockMinimo));
            }
        }

        /// <summary>
        /// Desactiva el ingrediente.
        /// Un ingrediente desactivado no debe utilizarse en nuevas comandas ni recibir movimientos de inventario.
        /// No tiene efecto si el ingrediente ya está desactivado.
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo)
                return;

            EstaActivo = false;
            MarkAsModified();

            AddDomainEvent(new IngredienteDesactivado(Id, Nombre));
        }

        /// <summary>
        /// Activa el ingrediente.
        /// Permite que el ingrediente vuelva a utilizarse en comandas y recibir movimientos.
        /// No tiene efecto si el ingrediente ya está activo.
        /// </summary>
        public void Activar()
        {
            if (EstaActivo)
                return;

            EstaActivo = true;
            MarkAsModified();

            AddDomainEvent(new IngredienteActivado(Id, Nombre));
        }

        /// <summary>
        /// Asocia un proveedor principal a este ingrediente.
        /// Este proveedor se utilizará para generar órdenes de compra automáticas cuando el stock esté bajo.
        /// </summary>
        /// <param name="proveedorId">ID del proveedor a asociar</param>
        public void AsociarProveedorPrincipal(Guid proveedorId)
        {
            ProveedorPrincipalId = proveedorId;
            MarkAsModified();
            
            AddDomainEvent(new ProveedorPrincipalAsociado(Id, proveedorId));
        }
        
        /// <summary>
        /// Actualiza el nivel de rotación del ingrediente.
        /// </summary>
        /// <param name="rotacion">Nuevo nivel de rotación</param>
        public void ActualizarRotacion(RotacionIngrediente rotacion)
        {
            if (Rotacion == rotacion)
                return;
            
            Rotacion = rotacion;
            MarkAsModified();
            
            AddDomainEvent(new RotacionIngredienteActualizada(Id, Nombre, rotacion));
        }
        
        /// <summary>
        /// Actualiza la temporada del ingrediente.
        /// </summary>
        /// <param name="temporada">Nueva temporada del ingrediente</param>
        public void ActualizarTemporada(TemporadaIngrediente temporada)
        {
            if (Temporada == temporada)
                return;
            
            Temporada = temporada;
            MarkAsModified();
            
            AddDomainEvent(new TemporadaIngredienteActualizada(Id, Nombre, temporada));
        }
        
        /// <summary>
        /// Establece o quita el bloqueo de control de calidad.
        /// </summary>
        /// <param name="bloqueado">Indica si debe estar bloqueado</param>
        /// <param name="motivo">Motivo del bloqueo o desbloqueo</param>
        public void ActualizarBloqueoControlCalidad(bool bloqueado, string motivo)
        {
            if (BloqueadoControlCalidad == bloqueado)
                return;
            
            BloqueadoControlCalidad = bloqueado;
            MarkAsModified();
            
            if (bloqueado)
                AddDomainEvent(new IngredienteBloqueadoPorCalidad(Id, Nombre, motivo));
            else
                AddDomainEvent(new IngredienteDesbloqueadoPorCalidad(Id, Nombre, motivo));
        }
        
        /// <summary>
        /// Actualiza el costo promedio del ingrediente.
        /// </summary>
        /// <param name="nuevoCosto">Nuevo costo promedio</param>
        public void ActualizarCostoPromedio(decimal nuevoCosto)
        {
            if (nuevoCosto < 0)
                throw new ArgumentException("El costo no puede ser negativo", nameof(nuevoCosto));
            
            if (CostoPromedio == nuevoCosto)
                return;
            
            CostoPromedio = nuevoCosto;
            MarkAsModified();
            
            AddDomainEvent(new CostoPromedioActualizado(Id, Nombre, nuevoCosto));
        }
        
        /// <summary>
        /// Valida todas las invariantes del agregado Ingrediente.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            // Validar que el stock nunca sea negativo
            if (Stock < 0)
                throw new InvalidOperationException($"El stock del ingrediente '{Nombre}' no puede ser negativo. Valor actual: {Stock}");
            
            // Validar que el stock mínimo no sea negativo
            if (StockMinimo < 0)
                throw new InvalidOperationException($"El stock mínimo del ingrediente '{Nombre}' no puede ser negativo. Valor actual: {StockMinimo}");
            
            // Validar que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(Nombre))
                throw new InvalidOperationException("El nombre del ingrediente no puede estar vacío");
            
            // Validación de consistencia de movimientos con el stock actual
            var stockCalculado = 0m;
            foreach (var movimiento in _movimientos)
            {
                stockCalculado = movimiento.CalcularNuevoStock(stockCalculado);
            }
            
            // Comprobar que el stock calculado coincide con el stock actual
            if (Math.Abs(stockCalculado - Stock) > 0.001m) // Permitir pequeñas diferencias por redondeo
                throw new InvalidOperationException($"Inconsistencia en el stock del ingrediente '{Nombre}'. Stock actual: {Stock}, Stock calculado desde movimientos: {stockCalculado}");
        }

        /// <summary>
        /// Valida que el ingrediente esté activo para realizar operaciones.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si el ingrediente está desactivado</exception>
        private void ValidarIngredienteActivo()
        {
            if (!EstaActivo)
                throw new InvalidOperationException($"No se pueden realizar operaciones en un ingrediente desactivado: {Nombre}");
        }
    }
}
