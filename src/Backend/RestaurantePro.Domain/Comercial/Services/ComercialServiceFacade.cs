namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Comercial
    /// </summary>
    public class ComercialServiceFacade : IComercialServiceFacade
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IClientesFrecuentesPolicy _clientesFrecuentesPolicy;
        private readonly IServicioFidelizacion _servicioFidelizacion;
        private readonly INotificationManager _notificationManager;
        
        public ComercialServiceFacade(
            IClienteRepository clienteRepository,
            IClientesFrecuentesPolicy clientesFrecuentesPolicy,
            IServicioFidelizacion servicioFidelizacion,
            INotificationManager notificationManager)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _clientesFrecuentesPolicy = clientesFrecuentesPolicy ?? throw new ArgumentNullException(nameof(clientesFrecuentesPolicy));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }

        /// <inheritdoc />
        public async Task<Result<Cliente?>> ObtenerClientePorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(id != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Cliente?>(null);
            }
            
            try
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(id, cancellationToken);
                return Result.Success<Cliente?>(cliente);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener cliente: {ex.Message}", "ObtenerCliente");
                return _notificationManager.ToResult<Cliente?>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Cliente>> RegistrarNuevoClienteConTarjetaAsync(
            string nombre, 
            string apellidos, 
            string email, 
            string? telefono = null, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombre), "El nombre no puede estar vacío", "Nombre");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(apellidos), "Los apellidos no pueden estar vacíos", "Apellidos");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(email), "El email no puede estar vacío", "Email");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Cliente>(null);
            }
            
            try
            {
                // Crear un nuevo cliente
                var clienteNombre = ClienteNombre.Crear(nombre, apellidos);
                var cliente = Cliente.Crear(clienteNombre, email, telefono);
                
                // Crear y asignar tarjeta de fidelización
                cliente.CrearTarjetaFidelizacion();
                
                // Persistir el cliente
                await _clienteRepository.AgregarAsync(cliente);
                await _clienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(cliente);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al registrar cliente: {ex.Message}", "RegistrarCliente");
                return _notificationManager.ToResult<Cliente>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Cliente?>> ActualizarDatosClienteAsync(
            Guid clienteId, 
            string? nombre = null, 
            string? apellidos = null, 
            string? email = null, 
            string? telefono = null, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Cliente?>(null);
            }
            
            try
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "ClienteId");
                    return _notificationManager.ToResult<Cliente?>(null);
                }
                
                // Actualizar nombre si se proporcionó
                if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellidos))
                {
                    try
                    {
                        var nuevoNombre = ClienteNombre.Crear(nombre, apellidos);
                        cliente.ActualizarNombre(nuevoNombre);
                    }
                    catch (Exception ex)
                    {
                        _notificationManager.AddError($"Error al actualizar nombre: {ex.Message}", "Nombre");
                    }
                }
                
                // Actualizar email si se proporcionó
                if (!string.IsNullOrEmpty(email))
                {
                    try
                    {
                        cliente.ActualizarEmail(email);
                    }
                    catch (Exception ex)
                    {
                        _notificationManager.AddError($"Error al actualizar email: {ex.Message}", "Email");
                    }
                }
                
                // Actualizar teléfono si se proporcionó
                if (!string.IsNullOrEmpty(telefono))
                {
                    try
                    {
                        cliente.ActualizarTelefono(telefono);
                    }
                    catch (Exception ex)
                    {
                        _notificationManager.AddError($"Error al actualizar teléfono: {ex.Message}", "Telefono");
                    }
                }
                
                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult<Cliente?>(null);
                }
                
                // Persistir cambios
                await _clienteRepository.ActualizarAsync(cliente);
                await _clienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success<Cliente?>(cliente);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar cliente: {ex.Message}", "ActualizarCliente");
                return _notificationManager.ToResult<Cliente?>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AsignarPuntosClienteAsync(
            Guid clienteId, 
            int puntos, 
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            _notificationManager.Require(puntos > 0, "La cantidad de puntos debe ser mayor a cero", "Puntos");
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "ClienteId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                if (!cliente.TieneTarjetaFidelizacion())
                {
                    _notificationManager.AddError("El cliente no tiene tarjeta de fidelización", "TarjetaFidelizacion");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                cliente.AgregarPuntosFidelizacion(puntos, comandaId.ToString());
                
                await _clienteRepository.ActualizarAsync(cliente);
                await _clienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al asignar puntos: {ex.Message}", "AsignarPuntos");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<decimal>> CalcularDescuentoPuntosFidelizacionAsync(
            Guid clienteId, 
            decimal montoTotal, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            _notificationManager.Require(montoTotal >= 0, "El monto total no puede ser negativo", "MontoTotal");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<decimal>(0);
            }
            
            try
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "ClienteId");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                if (!cliente.TieneTarjetaFidelizacion())
                {
                    _notificationManager.AddError("El cliente no tiene tarjeta de fidelización", "TarjetaFidelizacion");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                var puntos = cliente.ObtenerPuntosFidelizacionDisponibles();
                var descuento = _servicioFidelizacion.CalcularDescuentoPorPuntos(puntos, montoTotal);
                
                return Result.Success(descuento);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al calcular descuento: {ex.Message}", "CalcularDescuento");
                return _notificationManager.ToResult<decimal>(0);
            }
        }

        /// <inheritdoc />
        public async Task<Result<decimal>> CanjearPuntosPorDescuentoAsync(
            Guid clienteId, 
            int puntosAUtilizar, 
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            _notificationManager.Require(puntosAUtilizar > 0, "La cantidad de puntos debe ser mayor a cero", "PuntosAUtilizar");
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<decimal>(0);
            }
            
            try
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "ClienteId");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                if (!cliente.TieneTarjetaFidelizacion())
                {
                    _notificationManager.AddError("El cliente no tiene tarjeta de fidelización", "TarjetaFidelizacion");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                var puntosDisponibles = cliente.ObtenerPuntosFidelizacionDisponibles();
                if (puntosAUtilizar > puntosDisponibles)
                {
                    _notificationManager.AddError($"El cliente solo tiene {puntosDisponibles} puntos disponibles", "PuntosDisponibles");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                // Calcular descuento
                var descuento = _servicioFidelizacion.CalcularDescuentoPorPuntos(puntosAUtilizar, 0);
                
                // Registrar el uso de puntos
                cliente.UsarPuntosFidelizacion(puntosAUtilizar, $"Descuento en comanda {comandaId}");
                
                // Persistir cambios
                await _clienteRepository.ActualizarAsync(cliente);
                await _clienteRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(descuento);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al canjear puntos: {ex.Message}", "CanjearPuntos");
                return _notificationManager.ToResult<decimal>(0);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Dictionary<Guid, SegmentoCliente>>> EjecutarPoliticaClientesFrecuentesAsync(
            IEnumerable<Guid>? clienteIds = null, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Obtener clientes a procesar
                var clientes = clienteIds != null
                    ? await _clienteRepository.ObtenerClientesPorIdsAsync(clienteIds, cancellationToken)
                    : await _clienteRepository.ObtenerTodosConHistorialVisitasAsync(90, cancellationToken);
                
                if (!clientes.Any())
                {
                    _notificationManager.AddError("No se encontraron clientes para procesar", "Clientes");
                    return _notificationManager.ToResult<Dictionary<Guid, SegmentoCliente>>(new Dictionary<Guid, SegmentoCliente>());
                }
                
                // Ejecutar política
                var resultado = await _clientesFrecuentesPolicy.EjecutarAsync(90, cancellationToken);
                
                // Construir diccionario de resultados
                var resultadoClientes = new Dictionary<Guid, SegmentoCliente>();
                
                // Agregar todos los clientes actualizados
                foreach (var clienteId in resultado.ClientesActualizados)
                {
                    var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                    if (cliente != null)
                    {
                        resultadoClientes[clienteId] = cliente.Segmento;
                    }
                }
                
                return Result.Success(resultadoClientes);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al ejecutar política de clientes frecuentes: {ex.Message}", "ClientesFrecuentes");
                return _notificationManager.ToResult<Dictionary<Guid, SegmentoCliente>>(new Dictionary<Guid, SegmentoCliente>());
            }
        }
    }
} 