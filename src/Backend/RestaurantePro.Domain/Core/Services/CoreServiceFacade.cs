namespace RestaurantePro.Domain.Core.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto Core
    /// </summary>
    public class CoreServiceFacade : ICoreServiceFacade
    {
        private readonly Productos.Interfaces.IProductoRepository _productoRepository;
        private readonly Productos.Interfaces.IProductoCategoriaRepository _productoCategoriaRepository;
        private readonly Productos.Interfaces.IRecetaRepository _recetaRepository;
        private readonly Usuarios.Interfaces.IUsuarioRepository _usuarioRepository;
        private readonly Usuarios.Interfaces.IRolRepository _rolRepository;
        private readonly Notificaciones.Interfaces.INotificacionRepository _notificacionRepository;
        private readonly Productos.Services.IProductoCategoriaService _productoCategoriaService;
        private readonly Productos.Services.IRecetaService _recetaService;
        private readonly SharedKernel.Services.Notification.IEventBasedNotificationService _notificationService;
        private readonly Core.Base.Services.IDateTimeService _dateTimeService;
        private readonly SharedKernel.Validation.INotificationManager _notificationManager;
        private readonly ILogger<ProductoBuilder> _productoBuilderLogger;
        private readonly ILogger<CoreServiceFacade> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public CoreServiceFacade(
            Productos.Interfaces.IProductoRepository productoRepository,
            Productos.Interfaces.IProductoCategoriaRepository productoCategoriaRepository,
            Productos.Interfaces.IRecetaRepository recetaRepository,
            Usuarios.Interfaces.IUsuarioRepository usuarioRepository,
            Usuarios.Interfaces.IRolRepository rolRepository,
            Notificaciones.Interfaces.INotificacionRepository notificacionRepository,
            Productos.Services.IProductoCategoriaService productoCategoriaService,
            Productos.Services.IRecetaService recetaService,
            SharedKernel.Services.Notification.IEventBasedNotificationService notificationService,
            Core.Base.Services.IDateTimeService dateTimeService,
            SharedKernel.Validation.INotificationManager notificationManager,
            ILogger<ProductoBuilder> productoBuilderLogger,
            ILogger<CoreServiceFacade> logger)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _productoCategoriaRepository = productoCategoriaRepository ?? throw new ArgumentNullException(nameof(productoCategoriaRepository));
            _recetaRepository = recetaRepository ?? throw new ArgumentNullException(nameof(recetaRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _notificacionRepository = notificacionRepository ?? throw new ArgumentNullException(nameof(notificacionRepository));
            _productoCategoriaService = productoCategoriaService ?? throw new ArgumentNullException(nameof(productoCategoriaService));
            _recetaService = recetaService ?? throw new ArgumentNullException(nameof(recetaService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _productoBuilderLogger = productoBuilderLogger ?? throw new ArgumentNullException(nameof(productoBuilderLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Productos

        /// <inheritdoc/>
        public async Task<Productos.Entities.Producto?> ObtenerProductoPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Result<Productos.Entities.Producto>> RegistrarProductoAsync(
            string nombre, 
            string descripcion, 
            decimal precio, 
            Guid? categoriaId = null, 
            string? categoriaNombre = null, 
            CancellationToken cancellationToken = default)
        {
            // Limpiar notificaciones previas
            _notificationManager.ClearErrors();

            try
            {
                // Obtener o crear categoría si es necesario
                Guid efectivaCategoriaId;
                string efectivaCategoriaNombre;
                
                if (categoriaId.HasValue)
                {
                    // Verificar que la categoría existe
                    var categoriaExistente = await _productoCategoriaRepository.ObtenerPorIdAsync(categoriaId.Value);
                    if (categoriaExistente == null)
                    {
                        _notificationManager.AddError($"La categoría con ID {categoriaId.Value} no existe", nameof(categoriaId));
                        return _notificationManager.ToResult<Productos.Entities.Producto>(null);
                    }
                    
                    efectivaCategoriaId = categoriaId.Value;
                    efectivaCategoriaNombre = categoriaExistente.Nombre;
                }
                else if (!string.IsNullOrWhiteSpace(categoriaNombre))
                {
                    // Buscar categoría por nombre
                    var categorias = await _productoCategoriaRepository.ObtenerTodasAsync(cancellationToken);
                    var categoriaExistente = categorias.FirstOrDefault(c => c.Nombre.Equals(categoriaNombre, StringComparison.OrdinalIgnoreCase));
                    if (categoriaExistente != null)
                    {
                        efectivaCategoriaId = categoriaExistente.Id;
                        efectivaCategoriaNombre = categoriaExistente.Nombre;
                    }
                    else
                    {
                        // Crear nueva categoría
                        var nuevaCategoria = Productos.Entities.ProductoCategoria.Crear(categoriaNombre, $"Categoría {categoriaNombre}", 0);
                        await _productoCategoriaRepository.AgregarAsync(nuevaCategoria, cancellationToken);
                        
                        efectivaCategoriaId = nuevaCategoria.Id;
                        efectivaCategoriaNombre = nuevaCategoria.Nombre;
                        
                        _notificationManager.AddInformation($"Nueva categoría '{categoriaNombre}' creada automáticamente");
                    }
                }
                else
                {
                    _notificationManager.AddError("Debe especificar categoriaId o categoriaNombre", nameof(categoriaId));
                    return _notificationManager.ToResult<Productos.Entities.Producto>(null);
                }

                // Usar ProductoBuilder para crear el producto con validaciones robustas
                var resultado = new ProductoBuilder(_notificationManager, _productoBuilderLogger)
                    .ConNombre(nombre)
                    .ConDescripcion(descripcion)
                    .ConPrecio(precio)
                    .EnCategoria(efectivaCategoriaId, efectivaCategoriaNombre)
                    .Construir();
                
                if (!resultado.Succeeded)
                {
                    _logger.LogWarning("Error al construir el producto: {Errores}", 
                        string.Join(", ", _notificationManager.GetErrors()));
                    return resultado;
                }
                
                var producto = resultado.Value!;
                
                // Guardar el producto
                await _productoRepository.AgregarAsync(producto, cancellationToken);
                
                _logger.LogInformation("Producto registrado exitosamente: {ProductoId} - {Nombre}", 
                    producto.Id, producto.Nombre);
                
                return Result.Success(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar producto {Nombre}", nombre);
                _notificationManager.AddError($"Error interno: {ex.Message}", "RegistrarProducto");
                return _notificationManager.ToResult<Productos.Entities.Producto>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<Productos.Entities.Producto?>> ActualizarProductoAsync(
            Guid id, 
            string? nombre = null, 
            string? descripcion = null, 
            decimal? precio = null, 
            Guid? categoriaId = null, 
            bool? activo = null, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(id != Guid.Empty, "El ID del producto no puede estar vacío", "Id");
            
            if (precio.HasValue)
            {
                _notificationManager.Require(precio.Value > 0, "El precio debe ser mayor a cero", "Precio");
            }
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Productos.Entities.Producto?>(null);
            }
            
            try
            {
                // Verificar que el producto existe
                var producto = await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
                if (producto == null)
                {
                    _notificationManager.AddError($"No se encontró el producto con ID {id}", "Producto");
                    return _notificationManager.ToResult<Productos.Entities.Producto?>(null);
                }
                
                // Verificar que la categoría existe si se proporciona
                if (categoriaId.HasValue)
                {
                    var categoria = await _productoCategoriaRepository.ObtenerPorIdAsync(categoriaId.Value, cancellationToken);
                    if (categoria == null)
                    {
                        _notificationManager.AddError($"La categoría con ID {categoriaId} no existe", "CategoriaId");
                        return _notificationManager.ToResult<Productos.Entities.Producto?>(null);
                    }
                }
                
                // Actualizar propiedades si se proporcionan valores
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    producto.Actualizar(nombre, producto.Descripcion, producto.Precio);
                }
                
                if (!string.IsNullOrWhiteSpace(descripcion))
                {
                    producto.Actualizar(producto.Nombre, descripcion, producto.Precio);
                }
                
                if (precio.HasValue)
                {
                    var nuevoPrecio = new Productos.ValueObjects.PrecioProducto(precio.Value);
                    producto.Actualizar(producto.Nombre, producto.Descripcion, nuevoPrecio);
                }
                
                if (categoriaId.HasValue)
                {
                    var categoriaNombre = (await _productoCategoriaRepository.ObtenerPorIdAsync(categoriaId.Value, cancellationToken))?.Nombre ?? "General";
                    producto.ActualizarCategoria(categoriaId.Value, categoriaNombre);
                }
                
                if (activo.HasValue)
                {
                    if (activo.Value)
                        producto.Activar();
                    else
                        producto.Desactivar();
                }
                
                // Persistir los cambios
                await _productoRepository.ActualizarAsync(producto, cancellationToken);
                
                return Result.Success<Productos.Entities.Producto?>(producto);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar producto: {ex.Message}", "ActualizarProducto");
                return _notificationManager.ToResult<Productos.Entities.Producto?>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<List<Productos.Entities.Producto>> ObtenerProductosPorCategoriaAsync(
            Guid categoriaId, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default)
        {
            return await _productoCategoriaService.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> ActualizarCategoriaProductosAsync(
            List<Guid> productosIds, 
            Guid categoriaId, 
            CancellationToken cancellationToken = default)
        {
            return await _productoCategoriaService.ActualizarCategoriaProductosAsync(productosIds, categoriaId, cancellationToken);
        }

        /// <summary>
        /// Registra un nuevo producto usando el ProductoBuilder con opciones avanzadas
        /// </summary>
        /// <param name="nombre">Nombre del producto</param>
        /// <param name="descripcion">Descripción del producto</param>
        /// <param name="precio">Precio del producto</param>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="popularidadInicial">Popularidad inicial del producto (0-10)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el producto registrado</returns>
        public async Task<Result<Productos.Entities.Producto>> RegistrarProductoAvanzadoAsync(
            string nombre,
            string descripcion,
            decimal precio,
            Guid categoriaId,
            int popularidadInicial = 0,
            CancellationToken cancellationToken = default)
        {
            // Limpiar notificaciones previas
            _notificationManager.ClearErrors();

            try
            {
                // Verificar que la categoría existe
                var categoriaExistente = await _productoCategoriaRepository.ObtenerPorIdAsync(categoriaId);
                if (categoriaExistente == null)
                {
                    _notificationManager.AddError($"La categoría con ID {categoriaId} no existe", nameof(categoriaId));
                    return _notificationManager.ToResult<Productos.Entities.Producto>(null);
                }

                // Usar ProductoBuilder para crear el producto con validaciones robustas
                var resultado = new ProductoBuilder(_notificationManager, _productoBuilderLogger)
                    .ConNombre(nombre)
                    .ConDescripcion(descripcion)
                    .ConPrecio(precio)
                    .EnCategoria(categoriaId, categoriaExistente.Nombre)
                    .ConPopularidadInicial(popularidadInicial)
                    .Construir();

                if (!resultado.Succeeded)
                {
                    _logger.LogWarning("Error al construir el producto avanzado: {Errores}", 
                        string.Join(", ", _notificationManager.GetErrors()));
                    return resultado;
                }

                var producto = resultado.Value!;

                // Guardar el producto
                await _productoRepository.AgregarAsync(producto, cancellationToken);
                
                _logger.LogInformation("Producto avanzado registrado exitosamente: {ProductoId} - {Nombre} con popularidad {Popularidad}", 
                    producto.Id, producto.Nombre, popularidadInicial);
                
                return Result.Success(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar producto avanzado {Nombre}", nombre);
                _notificationManager.AddError($"Error interno: {ex.Message}", "RegistrarProductoAvanzado");
                return _notificationManager.ToResult<Productos.Entities.Producto>(null);
            }
        }

        #endregion

        #region Recetas

        /// <inheritdoc/>
        public async Task<Result<Productos.Entities.Receta>> RegistrarRecetaProductoAsync(
            Guid productoId, 
            string instrucciones, 
            int tiempoPreparacion, 
            Dictionary<Guid, decimal> ingredientes, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validaciones básicas
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(instrucciones), "Las instrucciones no pueden estar vacías", "Instrucciones");
            _notificationManager.Require(tiempoPreparacion > 0, "El tiempo de preparación debe ser mayor a cero", "TiempoPreparacion");
            _notificationManager.RequireNotNull(ingredientes, "La lista de ingredientes no puede ser nula", "Ingredientes");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Productos.Entities.Receta>(null);
            }
            
            try
            {
                // Validar que el producto existe
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                if (producto == null)
                {
                    _notificationManager.AddError($"El producto con ID {productoId} no existe", "ProductoId");
                    return _notificationManager.ToResult<Productos.Entities.Receta>(null);
                }
                
                // Buscar si ya existe una receta para este producto
                var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId, cancellationToken);
                
                if (receta == null)
                {
                    // Crear nueva receta
                    receta = Productos.Entities.Receta.Crear(productoId, instrucciones, tiempoPreparacion);
                    
                    // Agregar ingredientes
                    foreach (var kvp in ingredientes)
                    {
                        var ingredienteId = kvp.Key;
                        var cantidad = kvp.Value;
                        
                        // Verificar que la cantidad es válida
                        if (cantidad <= 0)
                        {
                            _notificationManager.AddError($"La cantidad para el ingrediente {ingredienteId} debe ser mayor a cero", "Ingredientes");
                            continue;
                        }
                        
                        // Obtener nombre de ingrediente
                        string nombreIngrediente = await ObtenerNombreIngredienteAsync(ingredienteId, cancellationToken) ?? 
                                                  $"Ingrediente {ingredienteId.ToString().Substring(0, 4)}";
                        
                        // Agregar ingrediente a la receta con los parámetros requeridos
                        receta.AgregarIngrediente(
                            ingredienteId, 
                            nombreIngrediente, 
                            cantidad, 
                            RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Gramo);
                    }
                    
                    await _recetaRepository.AgregarAsync(receta, cancellationToken);
                }
                else
                {
                    // Actualizar receta existente
                    receta.ActualizarPreparacion(instrucciones);
                    receta.ActualizarTiempoPreparacion(tiempoPreparacion);
                    
                    // Eliminar ingredientes existentes uno por uno
                    var ingredientesActuales = receta.Ingredientes.ToList();
                    foreach (var ingrediente in ingredientesActuales)
                    {
                        receta.EliminarIngrediente(ingrediente.IngredienteId);
                    }
                    
                    // Agregar ingredientes
                    foreach (var kvp in ingredientes)
                    {
                        var ingredienteId = kvp.Key;
                        var cantidad = kvp.Value;
                        
                        // Verificar que la cantidad es válida
                        if (cantidad <= 0)
                        {
                            _notificationManager.AddError($"La cantidad para el ingrediente {ingredienteId} debe ser mayor a cero", "Ingredientes");
                            continue;
                        }
                        
                        // Obtener nombre de ingrediente
                        string nombreIngrediente = await ObtenerNombreIngredienteAsync(ingredienteId, cancellationToken) ?? 
                                                  $"Ingrediente {ingredienteId.ToString().Substring(0, 4)}";
                        
                        // Agregar ingrediente a la receta con los parámetros requeridos
                        receta.AgregarIngrediente(
                            ingredienteId, 
                            nombreIngrediente, 
                            cantidad, 
                            RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Gramo);
                    }
                    
                    await _recetaRepository.ActualizarAsync(receta, cancellationToken);
                }
                
                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult<Productos.Entities.Receta>(receta);
                }
                
                return Result.Success(receta);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al registrar receta: {ex.Message}", "RegistrarReceta");
                return _notificationManager.ToResult<Productos.Entities.Receta>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VerificarDisponibilidadProductoAsync(
            Guid productoId, 
            int cantidad, 
            CancellationToken cancellationToken = default)
        {
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, cancellationToken);
            return resultado.Succeeded && resultado.Value;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, decimal>> ObtenerIngredientesFaltantesProductoAsync(
            Guid productoId, 
            int cantidad, 
            CancellationToken cancellationToken = default)
        {
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, cancellationToken);
            return resultado.Succeeded ? resultado.Value : new Dictionary<Guid, decimal>();
        }

        /// <inheritdoc/>
        public async Task<decimal> CalcularCostoRecetaProductoAsync(
            Guid productoId,
            CancellationToken cancellationToken = default)
        {
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, cancellationToken);
            return resultado.Succeeded ? resultado.Value : 0m;
        }

        /// <inheritdoc/>
        public async Task<Productos.ValueObjects.RentabilidadProducto> CalcularRentabilidadProductoAsync(
            Guid productoId,
            CancellationToken cancellationToken = default)
        {
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, cancellationToken);
            return resultado.Succeeded ? resultado.Value : Productos.ValueObjects.RentabilidadProducto.Calcular(0m, 0m);
        }

        /// <inheritdoc/>
        public async Task<Inventario.Ingredientes.Entities.Ingrediente?> BuscarSustitutoIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            var resultado = await _recetaService.BuscarSustitutoIngredienteAsync(ingredienteId, cancellationToken);
            
            if (!resultado.Succeeded)
            {
                if (resultado.Errors != null && resultado.Errors.Any())
                {
                    foreach (var error in resultado.Errors)
                    {
                        _notificationManager.AddError(error, "ERR001", "Ingrediente");
                    }
                }
                else if (!string.IsNullOrEmpty(resultado.Error))
                {
                    _notificationManager.AddError(resultado.Error, "ERR001", "Ingrediente");
                }
                else
                {
                    _notificationManager.AddError("Error desconocido al buscar sustituto de ingrediente", "ERR001", "Ingrediente");
                }
                return null;
            }
            
            return resultado.Value;
        }

        // Método auxiliar para obtener nombre de ingrediente
        private async Task<string?> ObtenerNombreIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken)
        {
            // Implementación simplificada
            return $"Ingrediente {ingredienteId.ToString().Substring(0, 8)}";
        }

        #endregion

        #region Usuarios

        /// <inheritdoc/>
        public async Task<Result<Usuario>> CrearUsuarioAsync(
            string nombreUsuario, 
            string nombreCompleto, 
            string email, 
            string rol, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreUsuario), "El nombre de usuario no puede estar vacío", "NombreUsuario");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreCompleto), "El nombre completo no puede estar vacío", "NombreCompleto");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(email), "El email no puede estar vacío", "Email");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(rol), "El rol no puede estar vacío", "Rol");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Verificar si ya existe un usuario con el mismo nombre
            var usuarioExistente = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(nombreUsuario, cancellationToken);
            if (usuarioExistente != null)
            {
                _notificationManager.AddError($"Ya existe un usuario con el nombre '{nombreUsuario}'", "NombreUsuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Convertir el rol a enum
            if (!Enum.TryParse<RolUsuario>(rol, true, out var rolEnum))
            {
                _notificationManager.AddError($"Rol no válido: {rol}", "Rol");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            try
            {
                // Crear el usuario
                var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, email, rolEnum);
                
                // Persistir el usuario
                await _usuarioRepository.AgregarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear usuario: {ex.Message}", "CrearUsuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<Usuario>> AsignarRolUsuarioAsync(
            Guid usuarioId, 
            string rol, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(usuarioId != Guid.Empty, "El ID del usuario no puede estar vacío", "UsuarioId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(rol), "El rol no puede estar vacío", "Rol");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                _notificationManager.AddError($"No se encontró el usuario con ID {usuarioId}", "Usuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Convertir el rol a enum
            if (!Enum.TryParse<RolUsuario>(rol, true, out var rolEnum))
            {
                _notificationManager.AddError($"Rol no válido: {rol}", "Rol");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            try
            {
                // Asignar el rol
                usuario.AsignarRol(rolEnum);
                
                // Persistir los cambios
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al asignar rol: {ex.Message}", "AsignarRol");
                return _notificationManager.ToResult<Usuario>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<Usuario>> ActualizarNombreUsuarioAsync(
            Guid usuarioId, 
            string nuevoNombre, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(usuarioId != Guid.Empty, "El ID del usuario no puede estar vacío", "UsuarioId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nuevoNombre), "El nuevo nombre no puede estar vacío", "NuevoNombre");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                _notificationManager.AddError($"No se encontró el usuario con ID {usuarioId}", "Usuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            try
            {
                // Actualizar el nombre
                usuario.Actualizar(nuevoNombre, usuario.Email);
                
                // Persistir los cambios
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar el nombre: {ex.Message}", "ActualizarNombre");
                return _notificationManager.ToResult<Usuario>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<Usuario>> ActualizarEmailUsuarioAsync(
            Guid usuarioId, 
            string nuevoEmail, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(usuarioId != Guid.Empty, "El ID del usuario no puede estar vacío", "UsuarioId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nuevoEmail), "El nuevo email no puede estar vacío", "NuevoEmail");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                _notificationManager.AddError($"No se encontró el usuario con ID {usuarioId}", "Usuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            try
            {
                // Actualizar el email
                usuario.Actualizar(usuario.NombreCompleto, nuevoEmail);
                
                // Persistir los cambios
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar el email: {ex.Message}", "ActualizarEmail");
                return _notificationManager.ToResult<Usuario>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<Usuario>> CambiarEstadoUsuarioAsync(
            Guid usuarioId, 
            bool activar, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(usuarioId != Guid.Empty, "El ID del usuario no puede estar vacío", "UsuarioId");
            
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                _notificationManager.AddError($"No se encontró el usuario con ID {usuarioId}", "Usuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            try
            {
                // Cambiar el estado
                if (activar)
                    usuario.Activar();
                else
                    usuario.Desactivar();
                
                // Persistir los cambios
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al cambiar estado del usuario: {ex.Message}", "CambiarEstado");
                return _notificationManager.ToResult<Usuario>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<Usuario>> LimpiarRolesUsuarioAsync(
            Guid usuarioId, 
            string rolPredeterminado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(usuarioId != Guid.Empty, "El ID del usuario no puede estar vacío", "UsuarioId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(rolPredeterminado), "El rol predeterminado no puede estar vacío", "RolPredeterminado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                _notificationManager.AddError($"No se encontró el usuario con ID {usuarioId}", "Usuario");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            // Convertir el rol predeterminado a enum
            if (!Enum.TryParse<RolUsuario>(rolPredeterminado, true, out var rolEnum))
            {
                _notificationManager.AddError($"Rol no válido: {rolPredeterminado}", "RolPredeterminado");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            try
            {
                // Primero asegurar que el usuario tenga el rol predeterminado
                if (!usuario.TieneRol(rolEnum))
                    usuario.AsignarRol(rolEnum);
                
                // Ahora recorremos los roles actuales y eliminamos todos excepto el predeterminado
                foreach (var rol in usuario.Roles.ToList())
                {
                    if (rol != rolEnum)
                    {
                        try
                        {
                            usuario.RemoverRol(rol);
                        }
                        catch (InvalidOperationException)
                        {
                            // Ignoramos la excepción si es que estamos intentando quitar el último rol
                            break;
                        }
                    }
                }
                
                // Persistir los cambios
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al limpiar roles: {ex.Message}", "LimpiarRoles");
                return _notificationManager.ToResult<Usuario>(null);
            }
        }

        #endregion

        #region Notificaciones

        /// <inheritdoc/>
        public async Task<Result<Notificaciones.Entities.Notificacion>> EnviarNotificacionAsync(
            Guid? destinatarioId, 
            string tipo, 
            string titulo, 
            string mensaje, 
            string datos = "", 
            int prioridad = 0, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validaciones básicas
            _notificationManager.Require(!string.IsNullOrWhiteSpace(tipo), "El tipo de notificación no puede estar vacío", "Tipo");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(titulo), "El título de la notificación no puede estar vacío", "Titulo");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(mensaje), "El mensaje de la notificación no puede estar vacío", "Mensaje");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Notificaciones.Entities.Notificacion>(null);
            }
            
            try
            {
                // Convertir el tipo a TipoNotificacion
                if (!Enum.TryParse<Notificaciones.Enums.TipoNotificacion>(tipo, true, out var tipoNotificacion))
                {
                    tipoNotificacion = Notificaciones.Enums.TipoNotificacion.Informativa;
                    _notificationManager.AddError($"Tipo de notificación no válido: {tipo}. Se usará 'Informativa' por defecto.", "Tipo");
                }
                
                // Crear notificación
                var notificacion = Notificaciones.Entities.Notificacion.Crear(
                    titulo,
                    mensaje,
                    tipoNotificacion,
                    destinatarioId ?? Guid.Empty);
                
                // Persistir
                await _notificacionRepository.AgregarAsync(notificacion, cancellationToken);
                
                // Notificar a través del servicio de notificaciones si hay destinatario
                if (destinatarioId.HasValue)
                {
                    await _notificationService.EnviarNotificacionAUsuarioAsync(
                        destinatarioId.Value,
                        titulo,
                        mensaje,
                        tipoNotificacion,
                        cancellationToken);
                }
                
                return Result.Success(notificacion);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al enviar notificación: {ex.Message}", "EnviarNotificacion");
                return _notificationManager.ToResult<Notificaciones.Entities.Notificacion>(null);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> MarcarNotificacionComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(notificacionId != Guid.Empty, "El ID de la notificación no puede estar vacío", "NotificacionId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId, cancellationToken);
                if (notificacion == null)
                {
                    _notificationManager.AddError($"No se encontró la notificación con ID {notificacionId}", "Notificacion");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                notificacion.MarcarComoLeida();
                
                await _notificacionRepository.ActualizarAsync(notificacion, cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al marcar notificación como leída: {ex.Message}", "MarcarComoLeida");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<List<Notificaciones.Entities.Notificacion>>> ObtenerNotificacionesUsuarioAsync(
            Guid usuarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(usuarioId != Guid.Empty, "El ID del usuario no puede estar vacío", "UsuarioId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<List<Notificaciones.Entities.Notificacion>>(new List<Notificaciones.Entities.Notificacion>());
            }
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
                if (usuario == null)
                {
                    _notificationManager.AddError($"No se encontró el usuario con ID {usuarioId}", "Usuario");
                    return _notificationManager.ToResult<List<Notificaciones.Entities.Notificacion>>(new List<Notificaciones.Entities.Notificacion>());
                }
                
                // Obtener todas las notificaciones
                var todasLasNotificaciones = await _notificacionRepository.ObtenerTodosAsync(cancellationToken);
                
                // Filtrar por destinatario
                var notificacionesUsuario = todasLasNotificaciones
                    .Where(n => n.DestinatarioId == usuarioId)
                    .ToList();
                    
                // Filtrar por no leídas si es necesario
                if (soloNoLeidas)
                {
                    notificacionesUsuario = notificacionesUsuario
                        .Where(n => !n.EstaLeida)
                        .ToList();
                }
                
                return Result.Success(notificacionesUsuario);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener notificaciones: {ex.Message}", "ObtenerNotificaciones");
                return _notificationManager.ToResult<List<Notificaciones.Entities.Notificacion>>(new List<Notificaciones.Entities.Notificacion>());
            }
        }

        #endregion
    }
} 