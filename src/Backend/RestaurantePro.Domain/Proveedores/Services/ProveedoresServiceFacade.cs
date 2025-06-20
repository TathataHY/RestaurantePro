namespace RestaurantePro.Domain.Proveedores.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Proveedores
    /// </summary>
    public class ProveedoresServiceFacade : IProveedoresServiceFacade
    {
        private readonly IProveedorRepository _proveedorRepository;
        private readonly INotificationManager _notificationManager;
        private readonly ILogger<ProveedorBuilder> _proveedorBuilderLogger;
        
        public ProveedoresServiceFacade(
            IProveedorRepository proveedorRepository,
            INotificationManager notificationManager,
            ILogger<ProveedorBuilder> proveedorBuilderLogger)
        {
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _proveedorBuilderLogger = proveedorBuilderLogger ?? throw new ArgumentNullException(nameof(proveedorBuilderLogger));
        }
        
        /// <inheritdoc />
        public async Task<Proveedor> RegistrarProveedorAsync(
            string nombre, 
            string rut, 
            string direccion, 
            string telefono, 
            string email, 
            string? sitioWeb = null, 
            string? notas = null, 
            CancellationToken cancellationToken = default)
        {
            // Verificar que no exista un proveedor con el mismo RUT
            var existente = await _proveedorRepository.ObtenerPorRutAsync(rut, cancellationToken);
            if (existente != null)
            {
                _notificationManager.AddError($"Ya existe un proveedor con el RUT {rut}", "RUT", "rut");
                throw new InvalidOperationException($"Ya existe un proveedor con el RUT {rut}");
            }
            
            // Usar ProveedorBuilder para crear el proveedor con validaciones robustas
            var builder = new ProveedorBuilder(_notificationManager, _proveedorBuilderLogger);
            
            // Valores predeterminados para una dirección básica
            string ciudad = "Santiago";
            string codigoPostal = "0000000";
            string pais = "Chile";
            
            // Construir el proveedor usando el builder
            var resultado = builder
                .ConNombre(nombre)
                .ConContactoPrincipal(nombre) // Usar el nombre del proveedor como contacto principal
                .ConEmail(email)
                .ConTelefono(telefono)
                .ConDireccion(direccion, ciudad, codigoPostal, pais)
                .ConRUT(rut) // RUT en Chile
                .ConInformacionBancaria(string.Empty) // Valor predeterminado
                .ConDiasCredito(30) // Valor predeterminado: 30 días
                .ConObservaciones(notas ?? string.Empty)
                .Construir();
            
            if (!resultado.Succeeded)
            {
                var errores = string.Join(", ", _notificationManager.GetErrors().Select(e => e.Message));
                throw new InvalidOperationException($"Error al crear el proveedor: {errores}");
            }
            
            var proveedor = resultado.Value;
            
            // Persistir el proveedor
            await _proveedorRepository.AgregarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return proveedor;
        }
        
        /// <inheritdoc />
        public async Task<Proveedor?> ActualizarDatosProveedorAsync(
            Guid proveedorId, 
            string? direccion = null, 
            string? telefono = null, 
            string? email = null, 
            string? sitioWeb = null, 
            string? notas = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return null;
            }
            
            // Actualizar los datos que se proporcionaron
            if (direccion != null)
            {
                proveedor.ActualizarDireccion(direccion);
            }
            
            if (telefono != null)
            {
                proveedor.ActualizarTelefono(telefono);
            }
            
            if (email != null)
            {
                proveedor.ActualizarEmail(email);
            }
            
            if (sitioWeb != null)
            {
                proveedor.ActualizarSitioWeb(sitioWeb);
            }
            
            if (notas != null)
            {
                proveedor.ActualizarNotas(notas);
            }
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return proveedor;
        }
        
        /// <inheritdoc />
        public async Task<Proveedor?> AgregarContactoProveedorAsync(
            Guid proveedorId, 
            string nombre, 
            string cargo, 
            string telefono, 
            string email, 
            bool esPrincipal = false, 
            string? notas = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return null;
            }
            
            // Agregar el contacto
            proveedor.AgregarContacto(nombre, cargo, telefono, email, esPrincipal, notas);
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return proveedor;
        }
        
        /// <inheritdoc />
        public async Task<bool> DesactivarProveedorAsync(
            Guid proveedorId, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return false;
            }
            
            // Desactivar el proveedor
            proveedor.Desactivar(motivo);
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> ActivarProveedorAsync(
            Guid proveedorId, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return false;
            }
            
            // Activar el proveedor
            proveedor.Activar();
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresPorTipoProductoAsync(
            string tipoProducto, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default)
        {
            // Obtener proveedores que ofrecen el tipo de producto
            var proveedores = await _proveedorRepository.ObtenerPorTipoProductoAsync(tipoProducto, cancellationToken);
            
            // Filtrar por activos si se requiere
            if (soloActivos)
            {
                proveedores = proveedores.Where(p => p.Activo).ToList();
            }
            
            return proveedores;
        }
        
        /// <summary>
        /// Registra un nuevo proveedor con información avanzada usando ProveedorBuilder
        /// </summary>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="nombreContacto">Nombre del contacto principal</param>
        /// <param name="email">Email del proveedor</param>
        /// <param name="telefono">Teléfono del proveedor</param>
        /// <param name="direccion">Dirección completa</param>
        /// <param name="ciudad">Ciudad</param>
        /// <param name="codigoPostal">Código postal</param>
        /// <param name="pais">País (default: Chile)</param>
        /// <param name="rfc">RFC del proveedor</param>
        /// <param name="informacionBancaria">Información bancaria (opcional)</param>
        /// <param name="diasCredito">Días de crédito (default: 30)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Result con el proveedor registrado o errores de validación</returns>
        public async Task<Result<Proveedor>> RegistrarProveedorAvanzadoAsync(
            string nombre,
            string nombreContacto,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string codigoPostal,
            string pais = "Chile",
            string? rfc = null,
            string? informacionBancaria = null,
            int diasCredito = 30,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            // Verificar que no exista un proveedor con el mismo RFC/RUT
            if (!string.IsNullOrEmpty(rfc))
            {
                var existente = await _proveedorRepository.ObtenerPorRutAsync(rfc, cancellationToken);
                if (existente != null)
                {
                    _notificationManager.AddError($"Ya existe un proveedor con el RFC/RUT {rfc}", "RFC", "rfc");
                    return Result.Failure<Proveedor>("Proveedor duplicado");
                }
            }
            
            // Usar ProveedorBuilder para crear el proveedor con validaciones robustas
            var builder = new ProveedorBuilder(_notificationManager, _proveedorBuilderLogger);
            
            // Construir el proveedor usando el builder
            var builderResult = builder
                .ConNombre(nombre)
                .ConContactoPrincipal(nombreContacto)
                .ConEmail(email)
                .ConTelefono(telefono)
                .ConDireccion(direccion, ciudad, codigoPostal, pais);
            
            // Agregar RFC si se proporcionó
            if (!string.IsNullOrEmpty(rfc))
            {
                builderResult = builderResult.ConRUT(rfc);
            }
            
            // Agregar información bancaria si se proporcionó
            if (!string.IsNullOrEmpty(informacionBancaria))
            {
                builderResult = builderResult.ConInformacionBancaria(informacionBancaria);
            }
            
            // Establecer días de crédito
            builderResult = builderResult.ConDiasCredito(diasCredito);
            
            // Agregar observaciones si se proporcionaron
            if (!string.IsNullOrEmpty(observaciones))
            {
                builderResult = builderResult.ConObservaciones(observaciones);
            }
            
            // Construir el proveedor
            var resultado = builderResult.Construir();
            
            if (!resultado.Succeeded)
            {
                return Result.Failure<Proveedor>("Error en validaciones de construcción");
            }
            
            var proveedor = resultado.Value;
            
            try
            {
                // Persistir el proveedor
                await _proveedorRepository.AgregarAsync(proveedor);
                await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(proveedor);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al persistir el proveedor: {ex.Message}", "Persistencia", "repository");
                return Result.Failure<Proveedor>("Error al guardar el proveedor");
            }
        }
    }
} 