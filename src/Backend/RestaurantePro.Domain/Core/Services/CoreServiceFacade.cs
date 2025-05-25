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
        private readonly IEventBasedNotificationService _notificationService;
        private readonly SharedKernel.Services.IDateTimeService _dateTimeService;

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
            IEventBasedNotificationService notificationService,
            SharedKernel.Services.IDateTimeService dateTimeService)
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
        }

        #region Productos

        /// <inheritdoc/>
        public async Task<Productos.Entities.Producto?> ObtenerProductoPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Productos.Entities.Producto> RegistrarProductoAsync(
            string nombre, 
            string descripcion, 
            decimal precio, 
            Guid? categoriaId = null, 
            string? categoriaNombre = null, 
            CancellationToken cancellationToken = default)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del producto no puede estar vacío", nameof(nombre));
            
            if (precio <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero", nameof(precio));

            // Obtener o crear categoría si es necesario
            Guid efectivaCategoriaId;
            
            if (categoriaId.HasValue)
            {
                // Verificar que la categoría existe
                var categoria = await _productoCategoriaRepository.ObtenerPorIdAsync(categoriaId.Value, cancellationToken);
                if (categoria == null)
                    throw new ArgumentException($"La categoría con ID {categoriaId} no existe", nameof(categoriaId));
                
                efectivaCategoriaId = categoriaId.Value;
            }
            else
            {
                // Usar categoría por defecto (primera categoría activa)
                var categorias = await _productoCategoriaRepository.ObtenerActivasAsync(cancellationToken);
                var categoriaPorDefecto = categorias.FirstOrDefault();
                
                if (categoriaPorDefecto == null)
                {
                    // Crear categoría por defecto si no existe ninguna
                    categoriaPorDefecto = Productos.Entities.ProductoCategoria.Crear("General", "Categoría general", 0);
                    await _productoCategoriaRepository.AgregarAsync(categoriaPorDefecto, cancellationToken);
                }
                
                efectivaCategoriaId = categoriaPorDefecto.Id;
            }

            // Crear precio y producto
            var precioProducto = new Productos.ValueObjects.PrecioProducto(precio);
            var categoriaNombreFinal = (await _productoCategoriaRepository.ObtenerPorIdAsync(efectivaCategoriaId, cancellationToken))?.Nombre ?? "General";
            
            var producto = Productos.Entities.Producto.Crear(
                nombre,
                descripcion,
                precioProducto,
                efectivaCategoriaId,
                categoriaNombreFinal);
            
            await _productoRepository.AgregarAsync(producto, cancellationToken);
            
            return producto;
        }

        /// <inheritdoc/>
        public async Task<Productos.Entities.Producto?> ActualizarProductoAsync(
            Guid id, 
            string? nombre = null, 
            string? descripcion = null, 
            decimal? precio = null, 
            Guid? categoriaId = null, 
            bool? activo = null, 
            CancellationToken cancellationToken = default)
        {
            // Implementación simplificada que solo obtiene el producto y lo actualiza con los valores proporcionados
            var producto = await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (producto == null)
                return null;
            
            // Aquí se actualizarían las propiedades del producto
            // pero eso depende de la implementación concreta de Producto
            
            await _productoRepository.ActualizarAsync(producto, cancellationToken);
            
            return producto;
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

        #endregion

        #region Recetas

        /// <inheritdoc/>
        public async Task<Productos.Entities.Receta> RegistrarRecetaProductoAsync(
            Guid productoId, 
            string instrucciones, 
            int tiempoPreparacion, 
            Dictionary<Guid, decimal> ingredientes, 
            CancellationToken cancellationToken = default)
        {
            // Validar que el producto existe
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
                throw new ArgumentException($"El producto con ID {productoId} no existe", nameof(productoId));
            
            // Buscar si ya existe una receta para este producto
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId, cancellationToken);
            
            if (receta == null)
            {
                // Crear nueva receta
                receta = Productos.Entities.Receta.Crear(productoId, instrucciones, tiempoPreparacion);
                
                // Aquí se agregarían los ingredientes pero para simplificar la implementación
                // lo dejamos pendiente
                
                await _recetaRepository.AgregarAsync(receta, cancellationToken);
            }
            else
            {
                // Actualizar receta existente
                receta.ActualizarPreparacion(instrucciones);
                receta.ActualizarTiempoPreparacion(tiempoPreparacion);
                
                // Aquí se actualizarían los ingredientes pero para simplificar la implementación
                // lo dejamos pendiente
                
                await _recetaRepository.ActualizarAsync(receta, cancellationToken);
            }
            
            return receta;
        }

        /// <inheritdoc/>
        public async Task<bool> VerificarDisponibilidadProductoAsync(
            Guid productoId, 
            int cantidad, 
            CancellationToken cancellationToken = default)
        {
            return await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, decimal>> ObtenerIngredientesFaltantesProductoAsync(
            Guid productoId, 
            int cantidad, 
            CancellationToken cancellationToken = default)
        {
            return await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, cancellationToken);
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
        public async Task<Usuarios.Entities.Usuario> RegistrarUsuarioAsync(
            string nombreUsuario, 
            string nombre, 
            string email, 
            List<Guid>? rolesIds = null, 
            CancellationToken cancellationToken = default)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(nombreUsuario));
            
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));
            
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));
            
            // Verificar que el nombre de usuario no exista
            var usuarioExistente = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(nombreUsuario, cancellationToken);
            if (usuarioExistente != null)
                throw new ArgumentException($"El nombre de usuario '{nombreUsuario}' ya está en uso", nameof(nombreUsuario));
            
            // Verificar que el email no exista
            usuarioExistente = await _usuarioRepository.ObtenerPorEmailAsync(email, cancellationToken);
            if (usuarioExistente != null)
                throw new ArgumentException($"El email '{email}' ya está en uso", nameof(email));
            
            // Crear y persistir usuario
            var emailVO = new SharedKernel.ValueObjects.Email(email);
            var usuario = Usuarios.Entities.Usuario.Crear(nombreUsuario, nombre, emailVO);
            
            // Asignar roles si se proporcionaron
            if (rolesIds != null && rolesIds.Count > 0)
            {
                foreach (var rolId in rolesIds)
                {
                    var rol = await _rolRepository.ObtenerPorIdAsync(rolId, cancellationToken);
                    if (rol != null)
                    {
                        usuario.AsignarRol(rol);
                    }
                }
            }
            
            await _usuarioRepository.AgregarAsync(usuario, cancellationToken);
            
            return usuario;
        }

        /// <inheritdoc/>
        public async Task<Usuarios.Entities.Usuario?> ActualizarUsuarioAsync(
            Guid usuarioId, 
            string? nombre = null, 
            string? email = null, 
            bool? activo = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
                return null;
            
            bool modificado = false;
            
            // Actualizar nombre si se proporcionó
            if (!string.IsNullOrWhiteSpace(nombre) && nombre != usuario.Nombre)
            {
                usuario.ActualizarNombre(nombre);
                modificado = true;
            }
            
            // Actualizar email si se proporcionó
            if (!string.IsNullOrWhiteSpace(email) && email != usuario.Email.Value)
            {
                // Verificar que el email no exista para otro usuario
                var usuarioExistente = await _usuarioRepository.ObtenerPorEmailAsync(email, cancellationToken);
                if (usuarioExistente != null && usuarioExistente.Id != usuarioId)
                    throw new ArgumentException($"El email '{email}' ya está en uso", nameof(email));
                
                // Crear valor de objeto Email y actualizar
                var emailVO = new SharedKernel.ValueObjects.Email(email);
                usuario.ActualizarEmail(emailVO);
                modificado = true;
            }
            
            // Actualizar estado activo si se proporcionó
            if (activo.HasValue && activo.Value != usuario.EstaActivo)
            {
                if (activo.Value)
                    usuario.Activar();
                else
                    usuario.Desactivar();
                
                modificado = true;
            }
            
            // Solo persistir si hubo cambios
            if (modificado)
            {
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            }
            
            return usuario;
        }

        /// <inheritdoc/>
        public async Task<bool> AsignarRolesUsuarioAsync(
            Guid usuarioId, 
            List<Guid> rolesIds, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el usuario
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
                return false;
            
            // Limpiar roles existentes
            usuario.LimpiarRoles();
            
            // Asignar nuevos roles
            foreach (var rolId in rolesIds)
            {
                var rol = await _rolRepository.ObtenerPorIdAsync(rolId, cancellationToken);
                if (rol != null)
                {
                    usuario.AsignarRol(rol);
                }
            }
            
            // Persistir cambios
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return true;
        }

        #endregion

        #region Notificaciones

        /// <inheritdoc/>
        public async Task<Notificaciones.Entities.Notificacion> EnviarNotificacionAsync(
            Guid? destinatarioId, 
            string tipo, 
            string titulo, 
            string mensaje, 
            string datos = "", 
            int prioridad = 0, 
            CancellationToken cancellationToken = default)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("El tipo de notificación no puede estar vacío", nameof(tipo));
            
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la notificación no puede estar vacío", nameof(titulo));
            
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje de la notificación no puede estar vacío", nameof(mensaje));
            
            // Convertir el tipo a TipoNotificacion
            if (!Enum.TryParse<Notificaciones.Enums.TipoNotificacion>(tipo, true, out var tipoNotificacion))
            {
                tipoNotificacion = Notificaciones.Enums.TipoNotificacion.Informativa;
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
            
            return notificacion;
        }

        /// <inheritdoc/>
        public async Task<bool> MarcarNotificacionComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default)
        {
            var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId, cancellationToken);
            if (notificacion == null)
                return false;
            
            notificacion.MarcarComoLeida();
            
            await _notificacionRepository.ActualizarAsync(notificacion, cancellationToken);
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<Notificaciones.Entities.Notificacion>> ObtenerNotificacionesUsuarioAsync(
            Guid usuarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default)
        {
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
            
            return notificacionesUsuario;
        }

        #endregion
    }
} 