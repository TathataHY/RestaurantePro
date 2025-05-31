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
        private readonly ILogger<ComercialServiceFacade> _logger;
        
        public ComercialServiceFacade(
            IClienteRepository clienteRepository,
            IClientesFrecuentesPolicy clientesFrecuentesPolicy,
            IServicioFidelizacion servicioFidelizacion,
            INotificationManager notificationManager,
            ILogger<ComercialServiceFacade> logger)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _clientesFrecuentesPolicy = clientesFrecuentesPolicy ?? throw new ArgumentNullException(nameof(clientesFrecuentesPolicy));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Result<Cliente?>> ObtenerClientePorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstEmpty(id, nameof(id));
                
                var cliente = await _clienteRepository.ObtenerPorIdAsync(id, cancellationToken);
                return Result.Success<Cliente?>(cliente);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al obtener cliente {ClienteId}: {Message}", id, ex.Message);
                return ex.ToResult<Cliente?>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al obtener cliente: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<Cliente?>(message);
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
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre));
                Guard.AgainstNullOrWhiteSpace(apellidos, nameof(apellidos));
                Guard.AgainstNullOrWhiteSpace(email, nameof(email));
                
                // Verificar si ya existe un cliente con ese email
                var clienteExistente = await _clienteRepository.ObtenerPorEmailAsync(email, cancellationToken);
                if (clienteExistente != null)
                {
                    _notificationManager.AddError("Ya existe un cliente registrado con este email", "CLIENTE_DUPLICADO", "Email");
                    return _notificationManager.ToResult<Cliente>(default!);
                }
                
                // Crear ValueObjects
                var clienteNombre = ClienteNombre.Crear(nombre, apellidos);
                var emailVO = Email.Create(email);
                var telefonoVO = !string.IsNullOrWhiteSpace(telefono) 
                    ? PhoneNumber.Create(telefono) 
                    : PhoneNumber.Create("000000000"); // Teléfono por defecto
                
                // Crear cliente (asumiendo fecha de nacimiento por defecto - esto debería ser parámetro)
                var fechaNacimiento = DateTime.Now.AddYears(-25); // Por defecto 25 años
                var cliente = Cliente.Crear(Guid.NewGuid(), clienteNombre, emailVO, telefonoVO, fechaNacimiento);
                
                // Persistir el cliente
                await _clienteRepository.AgregarAsync(cliente);
                
                // Crear tarjeta de fidelización usando el servicio especializado
                var resultadoTarjeta = await _servicioFidelizacion.CrearTarjetaFidelizacionAsync(cliente.Id);
                if (!resultadoTarjeta.Succeeded)
                {
                    _logger.LogWarning("No se pudo crear tarjeta de fidelización para cliente {ClienteId}: {Errores}", 
                        cliente.Id, string.Join(", ", resultadoTarjeta.Errors));
                    // Continuar sin tarjeta - no es crítico
                }
                
                _logger.LogInformation("Cliente {ClienteId} registrado exitosamente con email {Email}", 
                    cliente.Id, email);
                
                return Result.Success(cliente);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al registrar cliente: {Message}", ex.Message);
                return ex.ToResult<Cliente>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al registrar cliente: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<Cliente>(message);
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
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstEmpty(clienteId, nameof(clienteId));
                
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "CLIENTE_NO_ENCONTRADO", "ClienteId");
                    return _notificationManager.ToResult<Cliente?>(default);
                }
                
                // Actualizar información de contacto si se proporcionó email o teléfono
                if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(telefono))
                {
                    var emailActual = cliente.Email;
                    var telefonoActual = cliente.Telefono;
                    
                    if (!string.IsNullOrEmpty(email) && email != emailActual.Value)
                    {
                        emailActual = Email.Create(email);
                    }
                    
                    if (!string.IsNullOrEmpty(telefono) && telefono != telefonoActual.Value)
                    {
                        telefonoActual = PhoneNumber.Create(telefono);
                    }
                    
                    cliente.ActualizarInformacionContacto(emailActual, telefonoActual);
                }
                
                // Persistir cambios
                await _clienteRepository.ActualizarAsync(cliente);
                
                _logger.LogInformation("Cliente {ClienteId} actualizado exitosamente", clienteId);
                
                return Result.Success<Cliente?>(cliente);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al actualizar cliente {ClienteId}: {Message}", clienteId, ex.Message);
                return ex.ToResult<Cliente?>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al actualizar cliente: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<Cliente?>(message);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AsignarPuntosClienteAsync(
            Guid clienteId, 
            int puntos, 
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstEmpty(clienteId, nameof(clienteId));
                Guard.AgainstNegativeOrZero(puntos, nameof(puntos));
                Guard.AgainstEmpty(comandaId, nameof(comandaId));
                
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "CLIENTE_NO_ENCONTRADO", "ClienteId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Usar el servicio de fidelización para agregar puntos
                var resultado = await _servicioFidelizacion.AgregarPuntosAsync(clienteId, puntos, $"Comanda {comandaId}");
                
                if (!resultado.Succeeded)
                {
                    _logger.LogWarning("Error al asignar puntos al cliente {ClienteId}: {Errores}", 
                        clienteId, string.Join(", ", resultado.Errors));
                    return Result.Failure<bool>(resultado.Errors);
                }
                
                _logger.LogInformation("Asignados {Puntos} puntos al cliente {ClienteId} por comanda {ComandaId}", 
                    puntos, clienteId, comandaId);
                
                return Result.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al asignar puntos: {Message}", ex.Message);
                return ex.ToResult<bool>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al asignar puntos: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<bool>(message);
            }
        }

        /// <inheritdoc />
        public async Task<Result<decimal>> CalcularDescuentoPuntosFidelizacionAsync(
            Guid clienteId, 
            decimal montoTotal, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstEmpty(clienteId, nameof(clienteId));
                Guard.AgainstNegative(montoTotal, nameof(montoTotal));
                
                return await _servicioFidelizacion.CalcularDescuentoAsync(clienteId, montoTotal);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al calcular descuento: {Message}", ex.Message);
                return ex.ToResult<decimal>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al calcular descuento: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<decimal>(message);
            }
        }

        /// <inheritdoc />
        public async Task<Result<decimal>> CanjearPuntosPorDescuentoAsync(
            Guid clienteId, 
            int puntosAUtilizar, 
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstEmpty(clienteId, nameof(clienteId));
                Guard.AgainstNegativeOrZero(puntosAUtilizar, nameof(puntosAUtilizar));
                Guard.AgainstEmpty(comandaId, nameof(comandaId));
                
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "CLIENTE_NO_ENCONTRADO", "ClienteId");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                if (!cliente.EsElegibleParaDescuento(puntosAUtilizar))
                {
                    _notificationManager.AddError(
                        $"El cliente no tiene puntos suficientes. Disponibles: {cliente.ObtenerPuntosDisponibles()}, Solicitados: {puntosAUtilizar}",
                        "PUNTOS_INSUFICIENTES",
                        "PuntosAUtilizar");
                    return _notificationManager.ToResult<decimal>(0);
                }
                
                // Calcular descuento
                var descuento = _servicioFidelizacion.CalcularDescuentoPorPuntos(puntosAUtilizar, 0);
                
                // Restar los puntos del cliente
                cliente.RestarPuntos(puntosAUtilizar, $"Canje por descuento en comanda {comandaId}");
                
                // Persistir cambios
                await _clienteRepository.ActualizarAsync(cliente);
                
                _logger.LogInformation("Canjeados {Puntos} puntos del cliente {ClienteId} por descuento de {Descuento:C}", 
                    puntosAUtilizar, clienteId, descuento);
                
                return Result.Success(descuento);
            }
            catch (ClienteInactivoException ex)
            {
                _logger.LogWarning(ex, "Cliente inactivo en canje de puntos");
                return ex.ToResult<decimal>();
            }
            catch (BusinessRuleViolationException ex)
            {
                _logger.LogWarning(ex, "Violación de regla de negocio en canje de puntos");
                return ex.ToResult<decimal>();
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al canjear puntos: {Message}", ex.Message);
                return ex.ToResult<decimal>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al canjear puntos: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<decimal>(message);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Dictionary<Guid, SegmentoCliente>>> EjecutarPoliticaClientesFrecuentesAsync(
            IEnumerable<Guid>? clienteIds = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();
                
                // Si se proporcionaron clientes específicos, validar cada uno individualmente
                if (clienteIds != null && clienteIds.Any())
                {
                    var resultadosSegmentos = new Dictionary<Guid, SegmentoCliente>();
                    
                    foreach (var clienteId in clienteIds)
                    {
                        var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                        if (cliente != null)
                        {
                            resultadosSegmentos[clienteId] = cliente.Segmento;
                        }
                    }
                    
                    return Result.Success(resultadosSegmentos);
                }
                
                // Ejecutar segmentación para todos los clientes
                var resultado = await _clientesFrecuentesPolicy.EjecutarSegmentacionClientes(cancellationToken);
                
                // Convertir el resultado a un diccionario de segmentos por cliente
                var segmentosPorCliente = new Dictionary<Guid, SegmentoCliente>();
                foreach (var clienteId in resultado.ClientesSegmentados)
                {
                    var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                    if (cliente != null)
                    {
                        segmentosPorCliente[clienteId] = cliente.Segmento;
                    }
                }
                
                return Result.Success(segmentosPorCliente);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio en política de clientes frecuentes: {Message}", ex.Message);
                return ex.ToResult<Dictionary<Guid, SegmentoCliente>>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado en política de clientes frecuentes: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<Dictionary<Guid, SegmentoCliente>>(message);
            }
        }

        /// <summary>
        /// Procesa un cliente inactivo para reactivación con validaciones de negocio complejas.
        /// Incluye verificaciones de historial y elegibilidad.
        /// </summary>
        /// <param name="clienteId">ID del cliente a reactivar</param>
        /// <param name="motivoReactivacion">Motivo de la reactivación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        public async Task<Result> ProcesarReactivacionClienteAsync(Guid clienteId, string motivoReactivacion, CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();

                Guard.AgainstEmpty(clienteId, nameof(clienteId));
                Guard.AgainstNullOrWhiteSpace(motivoReactivacion, nameof(motivoReactivacion));

                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "CLIENTE_NO_ENCONTRADO", "ClienteId");
                    return _notificationManager.ToResult();
                }

                // Verificar que el cliente esté inactivo
                if (cliente.EstaActivo)
                {
                    _notificationManager.AddError("El cliente ya está activo", "CLIENTE_YA_ACTIVO", "Estado");
                    return _notificationManager.ToResult();
                }

                // Validaciones de negocio específicas para reactivación
                if (cliente.CantidadVisitas == 0)
                {
                    _notificationManager.AddError("No se puede reactivar un cliente sin historial de visitas", "SIN_HISTORIAL", "CantidadVisitas");
                }

                if (cliente.PuntosAcumulados > 0)
                {
                    _notificationManager.AddError("Cliente inactivo no puede tener puntos acumulados", "PUNTOS_EN_INACTIVO", "PuntosAcumulados");
                }

                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult();
                }

                // Procesar reactivación
                cliente.Reactivar();
                await _clienteRepository.ActualizarAsync(cliente);

                _logger.LogInformation("Cliente {ClienteId} reactivado exitosamente. Motivo: {Motivo}", 
                    clienteId, motivoReactivacion);

                return Result.Success();
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al reactivar cliente {ClienteId}: {Message}", 
                    clienteId, ex.Message);
                return ex.ToResult();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al reactivar cliente: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure(message);
            }
        }

        /// <summary>
        /// Transfiere puntos entre clientes con validaciones exhaustivas.
        /// Incluye verificación de eligibilidad y límites de transferencia.
        /// </summary>
        /// <param name="clienteOrigenId">ID del cliente que transfiere puntos</param>
        /// <param name="clienteDestinoId">ID del cliente que recibe puntos</param>
        /// <param name="puntos">Cantidad de puntos a transferir</param>
        /// <param name="motivo">Motivo de la transferencia</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación con detalles de la transferencia</returns>
        public async Task<Result<TransferenciaResult>> TransferirPuntosAsync(
            Guid clienteOrigenId,
            Guid clienteDestinoId,
            int puntos,
            string motivo,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();

                // Validaciones de entrada
                Guard.AgainstEmpty(clienteOrigenId, nameof(clienteOrigenId));
                Guard.AgainstEmpty(clienteDestinoId, nameof(clienteDestinoId));
                Guard.AgainstNegativeOrZero(puntos, nameof(puntos));
                Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo));

                // Obtener clientes
                var clienteOrigen = await _clienteRepository.ObtenerPorIdAsync(clienteOrigenId, cancellationToken);
                var clienteDestino = await _clienteRepository.ObtenerPorIdAsync(clienteDestinoId, cancellationToken);

                if (clienteOrigen == null)
                {
                    _notificationManager.AddError("No se encontró el cliente origen", "CLIENTE_ORIGEN_NO_ENCONTRADO", "ClienteOrigen");
                }

                if (clienteDestino == null)
                {
                    _notificationManager.AddError("No se encontró el cliente destino", "CLIENTE_DESTINO_NO_ENCONTRADO", "ClienteDestino");
                }

                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult<TransferenciaResult>(default!);
                }

                // Validaciones de negocio específicas
                if (!clienteOrigen!.EstaActivo)
                {
                    _notificationManager.AddError("El cliente origen debe estar activo", "CLIENTE_ORIGEN_INACTIVO", "ClienteOrigen");
                }

                if (!clienteDestino!.EstaActivo)
                {
                    _notificationManager.AddError("El cliente destino debe estar activo", "CLIENTE_DESTINO_INACTIVO", "ClienteDestino");
                }

                if (clienteOrigenId == clienteDestinoId)
                {
                    _notificationManager.AddError("No se puede transferir puntos al mismo cliente", "TRANSFERENCIA_MISMO_CLIENTE", "Transferencia");
                }

                if (!clienteOrigen.EsElegibleParaDescuento(puntos))
                {
                    _notificationManager.AddError(
                        $"El cliente origen no tiene puntos suficientes. Disponibles: {clienteOrigen.ObtenerPuntosDisponibles()}, Solicitados: {puntos}",
                        "PUNTOS_INSUFICIENTES",
                        "Puntos");
                }

                // Límite de transferencia (ej: máximo 1000 puntos por transferencia)
                const int LIMITE_TRANSFERENCIA = 1000;
                if (puntos > LIMITE_TRANSFERENCIA)
                {
                    _notificationManager.AddError(
                        $"No se pueden transferir más de {LIMITE_TRANSFERENCIA} puntos en una sola operación",
                        "LIMITE_EXCEDIDO",
                        "Puntos");
                }

                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult<TransferenciaResult>(default!);
                }

                // Ejecutar transferencia
                var puntosOrigenAntes = clienteOrigen.ObtenerPuntosDisponibles();
                var puntosDestinoAntes = clienteDestino.ObtenerPuntosDisponibles();

                clienteOrigen.RestarPuntos(puntos, $"Transferencia a cliente {clienteDestinoId}: {motivo}");
                clienteDestino.AgregarPuntos(puntos);

                // Persistir cambios
                await _clienteRepository.ActualizarAsync(clienteOrigen);
                await _clienteRepository.ActualizarAsync(clienteDestino);

                var resultado = new TransferenciaResult
                {
                    ClienteOrigenId = clienteOrigenId,
                    ClienteDestinoId = clienteDestinoId,
                    PuntosTransferidos = puntos,
                    PuntosOrigenAntes = puntosOrigenAntes,
                    PuntosOrigenDespues = clienteOrigen.ObtenerPuntosDisponibles(),
                    PuntosDestinoAntes = puntosDestinoAntes,
                    PuntosDestinoDespues = clienteDestino.ObtenerPuntosDisponibles(),
                    Motivo = motivo,
                    FechaTransferencia = DateTime.UtcNow
                };

                _logger.LogInformation("Transferencia exitosa: {Puntos} puntos de {OrigenId} a {DestinoId}. Motivo: {Motivo}",
                    puntos, clienteOrigenId, clienteDestinoId, motivo);

                return Result.Success(resultado);
            }
            catch (ClienteInactivoException ex)
            {
                _logger.LogWarning(ex, "Cliente inactivo en transferencia de puntos");
                return Result.Failure<TransferenciaResult>(ex.Message);
            }
            catch (BusinessRuleViolationException ex)
            {
                _logger.LogWarning(ex, "Violación de regla de negocio en transferencia de puntos");
                return Result.Failure<TransferenciaResult>(ex.Message);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio en transferencia de puntos: {Message}", ex.Message);
                return Result.Failure<TransferenciaResult>(ex.Message);
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado en transferencia de puntos: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<TransferenciaResult>(message);
            }
        }

        /// <summary>
        /// Valida múltiples clientes en lote y retorna un resumen de resultados.
        /// </summary>
        /// <param name="clienteIds">IDs de clientes a validar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con resumen de validaciones</returns>
        public async Task<Result<ValidacionLoteResult>> ValidarClientesEnLoteAsync(IEnumerable<Guid> clienteIds, CancellationToken cancellationToken = default)
        {
            try
            {
                Guard.AgainstNull(clienteIds, nameof(clienteIds));

                var listaClienteIds = clienteIds.ToList();
                if (!listaClienteIds.Any())
                {
                    return Result.Failure<ValidacionLoteResult>("No se proporcionaron clientes para validar");
                }

                var resultado = new ValidacionLoteResult();
                
                foreach (var clienteId in listaClienteIds)
                {
                    try
                    {
                        var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                        if (cliente == null)
                        {
                            resultado.ClientesInvalidos[clienteId] = "Cliente no encontrado";
                            continue;
                        }

                        // Validar cliente individual
                        if (cliente.EstaActivo)
                        {
                            resultado.ClientesValidos.Add(clienteId);
                        }
                        else
                        {
                            resultado.ClientesInvalidos.Add(clienteId, "Cliente inactivo");
                        }

                        // Validar invariantes implícitamente
                        var puntos = cliente.ObtenerPuntosDisponibles();
                        if (puntos < 0)
                        {
                            resultado.ClientesInvalidos[clienteId] = "Puntos negativos detectados";
                        }
                    }
                    catch (DomainException ex)
                    {
                        resultado.ClientesInvalidos[clienteId] = ex.Message;
                        _logger.LogWarning("Error de dominio en validación de cliente {ClienteId}: {Message}", 
                            clienteId, ex.Message);
                    }
                }

                resultado.TotalProcesados = listaClienteIds.Count;
                resultado.FechaProcesamiento = DateTime.UtcNow;

                _logger.LogInformation("Validación en lote completada: {Total} clientes, {Validos} válidos, {Invalidos} inválidos",
                    resultado.TotalProcesados, resultado.ClientesValidos.Count, resultado.ClientesInvalidos.Count);

                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado en validación en lote: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<ValidacionLoteResult>(message);
            }
        }

        /// <inheritdoc />
        public async Task<Result<int>> AcumularPuntosPorCompraAsync(
            Guid clienteId, 
            decimal montoCompra, 
            Guid? comandaId = null, 
            string concepto = "Compra", 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _notificationManager.CreateNewNotification();
                
                Guard.AgainstEmpty(clienteId, nameof(clienteId));
                Guard.AgainstNegative(montoCompra, nameof(montoCompra));
                Guard.AgainstNullOrWhiteSpace(concepto, nameof(concepto));

                // Obtener el cliente
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null)
                {
                    _notificationManager.AddError($"No se encontró el cliente con ID {clienteId}", "CLIENTE_NO_ENCONTRADO", "ClienteId");
                    return _notificationManager.ToResult<int>(0);
                }

                // Calcular puntos según reglas de negocio
                // Regla básica: 1 punto por cada $1000 de compra, mínimo 1 punto por compra
                var puntosCalculados = Math.Max(1, (int)(montoCompra / 1000));
                
                // Aplicar multiplicadores según segmento del cliente (simplificado)
                // Nota: En una implementación real, Cliente debería tener una propiedad SegmentoCliente
                var multiplicador = 1.0m; // Por defecto para clientes regulares
                
                // TODO: Implementar lógica de segmentación cuando Cliente tenga la propiedad SegmentoCliente
                // var multiplicador = cliente.SegmentoCliente switch
                // {
                //     SegmentoCliente.Premium => 1.5m,
                //     SegmentoCliente.FrecuenciaAlta => 1.2m,
                //     SegmentoCliente.TicketAlto => 1.3m,
                //     _ => 1.0m
                // };
                
                var puntosFinales = (int)(puntosCalculados * multiplicador);

                // Usar el servicio de fidelización para acumular los puntos
                // Crear un ID temporal para la comanda si no se proporcionó
                var comandaIdParaAcumulacion = comandaId ?? Guid.NewGuid();
                
                var resultadoAcumulacion = await _servicioFidelizacion.AcumularPuntosAsync(
                    clienteId, 
                    comandaIdParaAcumulacion, 
                    montoCompra); // Usar el monto original para el cálculo interno del servicio

                if (!resultadoAcumulacion.Succeeded)
                {
                    _logger.LogWarning("No se pudieron acumular puntos para cliente {ClienteId}: {Errores}", 
                        clienteId, string.Join(", ", resultadoAcumulacion.Errors));
                    return Result.Failure<int>($"Error al acumular puntos: {string.Join(", ", resultadoAcumulacion.Errors)}");
                }

                _logger.LogInformation(
                    "Puntos acumulados exitosamente - Cliente: {ClienteId}, Monto: {Monto:C}, Puntos: {Puntos}, Concepto: {Concepto}",
                    clienteId, montoCompra, puntosFinales, concepto);

                return Result.Success(puntosFinales);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al acumular puntos para cliente {ClienteId}: {Message}", clienteId, ex.Message);
                return ex.ToResult<int>();
            }
            catch (Exception ex)
            {
                var message = $"Error inesperado al acumular puntos para cliente {clienteId}: {ex.Message}";
                _logger.LogError(ex, message);
                return Result.Failure<int>(message);
            }
        }
    }

    /// <summary>
    /// Resultado de una operación de transferencia de puntos
    /// </summary>
    public record TransferenciaResult
    {
        public Guid ClienteOrigenId { get; init; }
        public Guid ClienteDestinoId { get; init; }
        public int PuntosTransferidos { get; init; }
        public int PuntosOrigenAntes { get; init; }
        public int PuntosOrigenDespues { get; init; }
        public int PuntosDestinoAntes { get; init; }
        public int PuntosDestinoDespues { get; init; }
        public string Motivo { get; init; } = string.Empty;
        public DateTime FechaTransferencia { get; init; }
    }

    /// <summary>
    /// Resultado de validación en lote de clientes
    /// </summary>
    public record ValidacionLoteResult
    {
        public List<Guid> ClientesValidos { get; init; } = new();
        public Dictionary<Guid, string> ClientesInvalidos { get; init; } = new();
        public int TotalProcesados { get; set; }
        public DateTime FechaProcesamiento { get; set; }
        
        public bool TodosValidos => ClientesInvalidos.Count == 0;
        public double PorcentajeExito => TotalProcesados > 0 ? (double)ClientesValidos.Count / TotalProcesados * 100 : 0;
    }
} 