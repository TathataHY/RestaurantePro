namespace RestaurantePro.Domain.Core.Productos.Entities
{
    /// <summary>
    /// Entidad que representa la receta de un producto, que define los ingredientes necesarios para elaborarlo.
    /// </summary>
    public class Receta : EntityBase
    {
        /// <summary>
        /// ID del producto al que pertenece esta receta
        /// </summary>
        public Guid ProductoId { get; private set; }

        private readonly List<ValueObjects.IngredienteReceta> _ingredientes = new();

        /// <summary>
        /// Lista de ingredientes que componen la receta
        /// </summary>
        public IReadOnlyCollection<ValueObjects.IngredienteReceta> Ingredientes => _ingredientes.AsReadOnly();

        /// <summary>
        /// Indicaciones de preparación
        /// </summary>
        public string Preparacion { get; private set; }

        /// <summary>
        /// Tiempo estimado de preparación en minutos
        /// </summary>
        public int TiempoPreparacionMinutos { get; private set; }

        /// <summary>
        /// Constructor protegido para EF Core
        /// </summary>
        protected Receta() { }

        /// <summary>
        /// Constructor privado para crear una receta
        /// </summary>
        private Receta(
            Guid productoId,
            string preparacion,
            int tiempoPreparacionMinutos)
        {
            ProductoId = productoId;
            Preparacion = preparacion;
            TiempoPreparacionMinutos = tiempoPreparacionMinutos;

            ValidarInvariantes();
        }

        /// <summary>
        /// Factory method para crear una nueva receta
        /// </summary>
        /// <param name="productoId">ID del producto asociado</param>
        /// <param name="preparacion">Indicaciones de preparación</param>
        /// <param name="tiempoPreparacionMinutos">Tiempo de preparación en minutos</param>
        /// <returns>Nueva instancia de Receta</returns>
        public static Receta Crear(
            Guid productoId,
            string preparacion,
            int tiempoPreparacionMinutos)
        {
            if (productoId == Guid.Empty)
                throw new ArgumentException("El ID del producto no puede estar vacío", nameof(productoId));

            if (string.IsNullOrWhiteSpace(preparacion))
                throw new ArgumentException("Las indicaciones de preparación no pueden estar vacías", nameof(preparacion));

            if (tiempoPreparacionMinutos <= 0)
                throw new ArgumentException("El tiempo de preparación debe ser mayor a cero", nameof(tiempoPreparacionMinutos));

            return new Receta(productoId, preparacion, tiempoPreparacionMinutos);
        }

        /// <summary>
        /// Agrega un ingrediente a la receta
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad requerida</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <param name="esOpcional">Indica si el ingrediente es opcional</param>
        public void AgregarIngrediente(
            Guid ingredienteId,
            string nombre,
            decimal cantidad,
            Inventario.Ingredientes.Enums.UnidadMedida unidadMedida,
            bool esOpcional = false)
        {
            if (_ingredientes.Any(i => i.IngredienteId == ingredienteId))
                throw new InvalidOperationException($"El ingrediente {nombre} ya existe en la receta");

            var ingrediente = ValueObjects.IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            _ingredientes.Add(ingrediente);
            MarkAsModified();
            ValidarInvariantes();
        }

        /// <summary>
        /// Actualiza la cantidad de un ingrediente existente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nuevaCantidad">Nueva cantidad</param>
        public void ActualizarCantidadIngrediente(Guid ingredienteId, decimal nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(nuevaCantidad));

            var indice = _ingredientes.FindIndex(i => i.IngredienteId == ingredienteId);
            if (indice < 0)
                throw new InvalidOperationException($"El ingrediente con ID {ingredienteId} no existe en la receta");

            var ingredienteActual = _ingredientes[indice];
            var ingredienteActualizado = ingredienteActual.ConCantidad(nuevaCantidad);
            
            _ingredientes[indice] = ingredienteActualizado;
            MarkAsModified();
            ValidarInvariantes();
        }

        /// <summary>
        /// Elimina un ingrediente de la receta
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente a eliminar</param>
        public void EliminarIngrediente(Guid ingredienteId)
        {
            var indice = _ingredientes.FindIndex(i => i.IngredienteId == ingredienteId);
            if (indice < 0)
                throw new InvalidOperationException($"El ingrediente con ID {ingredienteId} no existe en la receta");

            _ingredientes.RemoveAt(indice);
            MarkAsModified();
            ValidarInvariantes();
        }

        /// <summary>
        /// Actualiza las indicaciones de preparación
        /// </summary>
        /// <param name="preparacion">Nuevas indicaciones</param>
        public void ActualizarPreparacion(string preparacion)
        {
            if (string.IsNullOrWhiteSpace(preparacion))
                throw new ArgumentException("Las indicaciones de preparación no pueden estar vacías", nameof(preparacion));

            Preparacion = preparacion;
            MarkAsModified();
            ValidarInvariantes();
        }

        /// <summary>
        /// Actualiza el tiempo de preparación
        /// </summary>
        /// <param name="tiempoMinutos">Nuevo tiempo en minutos</param>
        public void ActualizarTiempoPreparacion(int tiempoMinutos)
        {
            if (tiempoMinutos <= 0)
                throw new ArgumentException("El tiempo de preparación debe ser mayor a cero", nameof(tiempoMinutos));

            TiempoPreparacionMinutos = tiempoMinutos;
            MarkAsModified();
            ValidarInvariantes();
        }

        /// <summary>
        /// Obtiene todos los ingredientes requeridos y sus cantidades
        /// </summary>
        /// <returns>Diccionario con IDs de ingredientes y cantidades</returns>
        public Dictionary<Guid, decimal> ObtenerIngredientesRequeridos()
        {
            return _ingredientes
                .Where(i => !i.EsOpcional)
                .ToDictionary(i => i.IngredienteId, i => i.Cantidad);
        }

        /// <summary>
        /// Valida las invariantes de la entidad
        /// </summary>
        private void ValidarInvariantes()
        {
            if (ProductoId == Guid.Empty)
                throw new InvalidOperationException("El ID del producto no puede estar vacío");

            if (string.IsNullOrWhiteSpace(Preparacion))
                throw new InvalidOperationException("Las indicaciones de preparación no pueden estar vacías");

            if (TiempoPreparacionMinutos <= 0)
                throw new InvalidOperationException("El tiempo de preparación debe ser mayor a cero");
        }
    }
} 