namespace RestaurantePro.Domain.Inventario.Ingredientes.Entities
{
    /// <summary>
    /// Entidad que representa un ingrediente en el inventario
    /// </summary>
    public class Ingrediente : EntityBase
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
        /// Stock mínimo recomendado del ingrediente
        /// </summary>
        public decimal StockMinimo { get; private set; }

        /// <summary>
        /// Stock actual del ingrediente
        /// </summary>
        public decimal Stock { get; private set; }

        /// <summary>
        /// Indica si el ingrediente está activo en el sistema
        /// </summary>
        public bool EstaActivo { get; private set; }

        /// <summary>
        /// Movimientos de inventario asociados a este ingrediente
        /// </summary>
        private readonly List<MovimientoInventario> _movimientos = new List<MovimientoInventario>();

        /// <summary>
        /// Acceso de solo lectura a los movimientos de inventario
        /// </summary>
        public IReadOnlyCollection<MovimientoInventario> Movimientos => _movimientos.AsReadOnly();

        /// <summary>
        /// Proveedor principal para este ingrediente
        /// </summary>
        public Guid? ProveedorPrincipalId { get; private set; }

        // Constructor privado para EF Core
        private Ingrediente() { }

        /// <summary>
        /// Factory Method para crear un nuevo ingrediente
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="codigo">Código del ingrediente</param>
        /// <param name="descripcion">Descripción del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida del ingrediente</param>
        /// <param name="stockMinimo">Stock mínimo recomendado</param>
        /// <param name="stockActual">Stock actual del ingrediente</param>
        /// <returns>Una nueva instancia de Ingrediente</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
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

            ingrediente.AddDomainEvent(new IngredienteCreadoEvent(ingrediente.Id, nombre));

            return ingrediente;
        }

        /// <summary>
        /// Incrementa el stock del ingrediente
        /// </summary>
        /// <param name="cantidad">Cantidad a incrementar</param>
        /// <param name="motivo">Motivo del incremento</param>
        /// <returns>Movimiento de inventario generado</returns>
        /// <exception cref="ArgumentException">Si la cantidad es negativa</exception>
        public MovimientoInventario IncrementarStock(decimal cantidad, string motivo)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            var movimiento = MovimientoInventario.CrearIngreso(Id, cantidad, motivo);

            // Aplicar el movimiento
            Stock = movimiento.Aplicar(Stock);
            MarkAsModified();

            // Agregar a la colección de movimientos
            _movimientos.Add(movimiento);

            // Emitir evento de stock actualizado
            AddDomainEvent(new StockActualizadoEvent(Id, Nombre, Stock));

            return movimiento;
        }

        /// <summary>
        /// Decrementa el stock del ingrediente
        /// </summary>
        /// <param name="cantidad">Cantidad a decrementar</param>
        /// <param name="motivo">Motivo del decremento</param>
        /// <returns>Movimiento de inventario generado</returns>
        /// <exception cref="ArgumentException">Si la cantidad es negativa</exception>
        /// <exception cref="InvalidOperationException">Si no hay suficiente stock</exception>
        public MovimientoInventario DecrementarStock(decimal cantidad, string motivo)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            if (cantidad > Stock)
                throw new InvalidOperationException("No hay suficiente stock disponible");

            var movimiento = MovimientoInventario.CrearEgreso(Id, cantidad, motivo);

            // Aplicar el movimiento
            Stock = movimiento.Aplicar(Stock);
            MarkAsModified();

            // Agregar a la colección de movimientos
            _movimientos.Add(movimiento);

            // Verificar si estamos por debajo del stock mínimo
            if (Stock < StockMinimo)
            {
                AddDomainEvent(new StockBajoMinimoEvent(Id, Nombre, Stock, StockMinimo));
            }

            // Emitir evento de stock actualizado
            AddDomainEvent(new StockActualizadoEvent(Id, Nombre, Stock));

            return movimiento;
        }

        /// <summary>
        /// Actualiza el stock mínimo del ingrediente
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
                AddDomainEvent(new StockBajoMinimoEvent(Id, Nombre, Stock, StockMinimo));
            }
        }

        /// <summary>
        /// Desactiva el ingrediente
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo)
                return;

            EstaActivo = false;
            MarkAsModified();

            AddDomainEvent(new IngredienteDesactivadoEvent(Id, Nombre));
        }

        /// <summary>
        /// Activa el ingrediente
        /// </summary>
        public void Activar()
        {
            if (EstaActivo)
                return;

            EstaActivo = true;
            MarkAsModified();

            AddDomainEvent(new IngredienteActivadoEvent(Id, Nombre));
        }

        /// <summary>
        /// Asocia un proveedor principal a este ingrediente
        /// </summary>
        /// <param name="proveedorId">ID del proveedor a asociar</param>
        public void AsociarProveedorPrincipal(Guid proveedorId)
        {
            ProveedorPrincipalId = proveedorId;
            MarkAsModified();
            
            AddDomainEvent(new ProveedorPrincipalAsociadoEvent(Id, Nombre, proveedorId));
        }
    }
}
