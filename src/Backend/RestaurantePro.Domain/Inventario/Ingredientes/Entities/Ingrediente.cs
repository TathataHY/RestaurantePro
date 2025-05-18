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
    /// - Cuando el stock cae por debajo del mínimo, se genera un evento StockBajoMinimo
    /// - Se pueden asociar proveedores principales por ID para generar órdenes automáticas
    /// - Todas las modificaciones al stock se realizan a través de movimientos de inventario
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
        /// <returns>Una nueva instancia de Ingrediente</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos (nombre vacío o stock mínimo negativo)</exception>
        public static Ingrediente Crear(string nombre, string codigo, string descripcion, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida, decimal stockMinimo, decimal stockActual)
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
                EstaActivo = true
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
            
            AddDomainEvent(new ProveedorPrincipalAsociado(Id, Nombre, proveedorId));
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
