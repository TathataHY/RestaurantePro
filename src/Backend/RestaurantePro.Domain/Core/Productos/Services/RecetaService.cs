namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Implementación del servicio para gestionar recetas de productos y sus ingredientes.
    /// </summary>
    public class RecetaService : IRecetaService
    {
        private readonly IRecetaRepository _recetaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly INotificationManager _notificationManager;

        /// <summary>
        /// Constructor del servicio de recetas
        /// </summary>
        /// <param name="recetaRepository">Repositorio de recetas</param>
        /// <param name="productoRepository">Repositorio de productos</param>
        /// <param name="ingredienteRepository">Repositorio de ingredientes</param>
        /// <param name="notificationManager">Gestor de notificaciones para validaciones</param>
        public RecetaService(
            IRecetaRepository recetaRepository,
            IProductoRepository productoRepository,
            IIngredienteRepository ingredienteRepository,
            INotificationManager notificationManager)
        {
            _recetaRepository = recetaRepository ?? throw new ArgumentNullException(nameof(recetaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }

        /// <inheritdoc/>
        public async Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", propertyName: "ProductoId");
            
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<Dictionary<Guid, decimal>>(new Dictionary<Guid, decimal>());
            
            // Verificar que el producto exista
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
            {
                return Result.Failure<Dictionary<Guid, decimal>>($"No se encontró el producto con ID {productoId}");
            }

            // Obtener la receta del producto
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId, cancellationToken);
            if (receta == null)
            {
                // Si no hay receta, regresamos un diccionario vacío
                return Result.Success(new Dictionary<Guid, decimal>());
            }

            // Obtener los ingredientes requeridos de la receta
            return Result.Success(receta.ObtenerIngredientesRequeridos());
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", propertyName: "ProductoId")
                .Require(cantidad > 0, "La cantidad debe ser mayor a cero", propertyName: "Cantidad");
                
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult(false);

            // Obtener los ingredientes del producto
            var ingredientesResult = await ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
            if (!ingredientesResult.Succeeded)
            {
                _notificationManager.AddError(
                    ingredientesResult.Error ?? "Error al obtener ingredientes", 
                    propertyName: "Ingredientes");
                return _notificationManager.ToResult(false);
            }
                
            var ingredientesReceta = ingredientesResult.Value;
            if (!ingredientesReceta.Any())
            {
                // Si no hay ingredientes, consideramos que está disponible
                return Result.Success(true);
            }

            // Variable para controlar si todos los ingredientes están disponibles
            bool todosDisponibles = true;
            
            // Lista para almacenar ingredientes faltantes para mensajes más informativos
            var ingredientesFaltantes = new List<(Guid Id, string Nombre, decimal Requerido, decimal Disponible)>();

            // Verificar stock de cada ingrediente
            foreach (var (ingredienteId, cantidadUnitaria) in ingredientesReceta)
            {
                // Obtener el ingrediente actual, sin incluir movimientos
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, false, cancellationToken);
                if (ingrediente == null)
                {
                    // Si el ingrediente no existe, registrarlo como faltante
                    ingredientesFaltantes.Add((ingredienteId, "Ingrediente no encontrado", cantidadUnitaria * cantidad, 0));
                    todosDisponibles = false;
                    continue;
                }

                // Calcular cantidad total necesaria
                decimal cantidadTotal = cantidadUnitaria * cantidad;

                // Verificar si hay suficiente stock
                if (ingrediente.Stock < cantidadTotal)
                {
                    // Registrar el ingrediente como faltante con su información
                    ingredientesFaltantes.Add((ingredienteId, ingrediente.Nombre, cantidadTotal, ingrediente.Stock));
                    todosDisponibles = false;
                }
            }

            // Si hay ingredientes faltantes, agregar información detallada a la notificación
            if (!todosDisponibles)
            {
                foreach (var faltante in ingredientesFaltantes)
                {
                    _notificationManager.AddError(
                        $"Ingrediente insuficiente: {faltante.Nombre} - Se requiere {faltante.Requerido} pero hay disponible {faltante.Disponible}",
                        propertyName: $"Ingrediente_{faltante.Id}");
                }
            }

            return Result.Success(todosDisponibles);
        }

        /// <inheritdoc/>
        public async Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesFaltantesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", propertyName: "ProductoId")
                .Require(cantidad > 0, "La cantidad debe ser mayor a cero", propertyName: "Cantidad");
                
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<Dictionary<Guid, decimal>>(new Dictionary<Guid, decimal>());

            // Obtener los ingredientes del producto
            var ingredientesResult = await ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
            if (!ingredientesResult.Succeeded)
            {
                _notificationManager.AddError(
                    ingredientesResult.Error ?? "Error al obtener ingredientes", 
                    propertyName: "Ingredientes");
                return _notificationManager.ToResult<Dictionary<Guid, decimal>>(new Dictionary<Guid, decimal>());
            }
                
            var ingredientesReceta = ingredientesResult.Value;
            if (!ingredientesReceta.Any())
            {
                // Si no hay ingredientes, regresamos un diccionario vacío
                return Result.Success(new Dictionary<Guid, decimal>());
            }

            // Obtener el producto para incluir información más detallada en notificaciones
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            string nombreProducto = producto?.Nombre ?? $"Producto ID {productoId}";

            // Diccionario para almacenar los ingredientes faltantes y sus cantidades
            var ingredientesFaltantes = new Dictionary<Guid, decimal>();
            
            // Lista para mensajes informativos más detallados
            var detallesFaltantes = new List<(Guid Id, string Nombre, decimal Faltante, decimal Requerido, decimal Disponible)>();

            // Verificar stock de cada ingrediente
            foreach (var (ingredienteId, cantidadUnitaria) in ingredientesReceta)
            {
                // Obtener el ingrediente actual, sin incluir movimientos
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, false, cancellationToken);
                
                // Calcular cantidad total necesaria
                decimal cantidadTotal = cantidadUnitaria * cantidad;
                
                if (ingrediente == null)
                {
                    // Si el ingrediente no existe, registramos la cantidad total como faltante
                    ingredientesFaltantes.Add(ingredienteId, cantidadTotal);
                    detallesFaltantes.Add((ingredienteId, "Ingrediente no encontrado", cantidadTotal, cantidadTotal, 0));
                    continue;
                }

                // Verificar si hay suficiente stock
                if (ingrediente.Stock < cantidadTotal)
                {
                    // Calcular la cantidad faltante
                    decimal cantidadFaltante = cantidadTotal - ingrediente.Stock;
                    
                    // Agregar al diccionario la cantidad faltante
                    ingredientesFaltantes.Add(ingredienteId, cantidadFaltante);
                    
                    // Registrar detalles para mensajes
                    detallesFaltantes.Add((
                        ingredienteId,
                        ingrediente.Nombre,
                        cantidadFaltante,
                        cantidadTotal,
                        ingrediente.Stock
                    ));
                }
            }

            // Agregar información detallada al NotificationManager para proporcionar contexto adicional
            if (ingredientesFaltantes.Any())
            {
                _notificationManager.AddError(
                    $"Stock insuficiente para preparar {cantidad} unidad(es) de {nombreProducto}",
                    propertyName: "Producto");
                    
                foreach (var detalle in detallesFaltantes)
                {
                    _notificationManager.AddError(
                        $"Falta {detalle.Faltante} de {detalle.Nombre} (disponible: {detalle.Disponible}, requerido: {detalle.Requerido})",
                        propertyName: $"Ingrediente_{detalle.Id}");
                }
            }

            return Result.Success(ingredientesFaltantes);
        }
        
        /// <inheritdoc/>
        public async Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", propertyName: "ProductoId");
            
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult(0m);
            
            // Verificar que el producto exista
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
            {
                return Result.Failure<decimal>($"No se encontró el producto con ID {productoId}");
            }
            
            // Obtener la receta del producto
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId, cancellationToken);
            if (receta == null || !receta.Ingredientes.Any())
            {
                // Si no hay receta o no tiene ingredientes, el costo es cero
                return Result.Success(0m);
            }
            
            decimal costoTotal = 0m;
            
            // Sumar el costo de cada ingrediente
            foreach (var ingredienteReceta in receta.Ingredientes)
            {
                // Obtener el ingrediente del repositorio
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(
                    ingredienteReceta.IngredienteId, 
                    false, 
                    cancellationToken);
                
                if (ingrediente != null)
                {
                    // Calcular el costo de este ingrediente según su cantidad
                    decimal costoIngrediente = ingrediente.CostoPromedio * ingredienteReceta.Cantidad;
                    costoTotal += costoIngrediente;
                }
                // Si el ingrediente no existe, no sumamos nada al costo total
            }
            
            return Result.Success(costoTotal);
        }
        
        /// <inheritdoc/>
        public async Task<Result<ValueObjects.RentabilidadProducto>> CalcularRentabilidadProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", propertyName: "ProductoId");
            
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<ValueObjects.RentabilidadProducto>(null);
            
            // Verificar que el producto exista
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
            {
                return Result.Failure<ValueObjects.RentabilidadProducto>($"No se encontró el producto con ID {productoId}");
            }
            
            // Obtener el costo total de los ingredientes
            var costoResult = await CalcularCostoRecetaAsync(productoId, cancellationToken);
            if (!costoResult.Succeeded)
                return Result.Failure<ValueObjects.RentabilidadProducto>(costoResult.Error);
            
            // Obtener el precio de venta del producto
            decimal precioVenta = producto.Precio.Valor;
            
            // Calcular rentabilidad usando el value object
            var rentabilidad = ValueObjects.RentabilidadProducto.Calcular(costoResult.Value, precioVenta);
            
            return Result.Success(rentabilidad);
        }
    }
} 