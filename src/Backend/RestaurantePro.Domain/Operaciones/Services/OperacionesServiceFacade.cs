using RestaurantePro.Domain.Operaciones.Comandas.Builders;
using RestaurantePro.Domain.Operaciones.Reservaciones.Builders;

namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Operaciones
    /// </summary>
    public class OperacionesServiceFacade : IOperacionesServiceFacade
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly INotificationManager _notificationManager;
        private readonly ILogger<ComandaBuilder> _comandaBuilderLogger;
        private readonly ILogger<ReservacionBuilder> _reservacionBuilderLogger;
        
        public OperacionesServiceFacade(
            IComandaRepository comandaRepository,
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IProductoRepository productoRepository,
            INotificationManager notificationManager,
            ILogger<ComandaBuilder> comandaBuilderLogger,
            ILogger<ReservacionBuilder> reservacionBuilderLogger)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _comandaBuilderLogger = comandaBuilderLogger ?? throw new ArgumentNullException(nameof(comandaBuilderLogger));
            _reservacionBuilderLogger = reservacionBuilderLogger ?? throw new ArgumentNullException(nameof(reservacionBuilderLogger));
        }
        
        #region Comandas

        /// <inheritdoc />
        public async Task<Result<Comanda>> CrearNuevaComandaAsync(
            Guid? clienteId, 
            Guid? mesaId, 
            Guid meseroId, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(meseroId != Guid.Empty, "El ID del mesero no puede estar vacío", "MeseroId");
            
            if (mesaId.HasValue)
            {
                _notificationManager.Require(mesaId.Value != Guid.Empty, "El ID de la mesa no puede estar vacío", "MesaId");
            }
            
            if (clienteId.HasValue)
            {
                _notificationManager.Require(clienteId.Value != Guid.Empty, "El ID del cliente no puede estar vacío", "ClienteId");
            }
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // Usar ComandaBuilder para crear la comanda con validaciones robustas
                var builder = new ComandaBuilder(_notificationManager, _comandaBuilderLogger);
                
                builder.ConMesero(meseroId);
                
                if (clienteId.HasValue)
                {
                    builder.ConCliente(clienteId.Value);
                }
                
                if (mesaId.HasValue)
                {
                    builder.EnMesa(mesaId.Value);
                }
                
                if (!string.IsNullOrWhiteSpace(observaciones))
                {
                    builder.ConObservaciones(observaciones);
                }
                
                // Construir la comanda
                var resultadoComanda = builder.Construir();
                if (!resultadoComanda.Succeeded)
                {
                    return resultadoComanda; // Ya tiene los errores del builder
                }
                
                var comanda = resultadoComanda.Value!;
                
                // Persistir la comanda
                await _comandaRepository.AgregarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear comanda: {ex.Message}", "CrearComanda");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Comanda>> AgregarProductoAComandaAsync(
            Guid comandaId, 
            Guid productoId, 
            int cantidad, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor a cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Obtener el producto
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                if (producto == null)
                {
                    _notificationManager.AddError($"No se encontró el producto con ID {productoId}", "ProductoId");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Agregar el producto a la comanda
                comanda.AgregarItem(productoId, producto.Nombre, cantidad, producto.Precio.Valor, observaciones);
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar producto a comanda: {ex.Message}", "AgregarProducto");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AgregarPersonalizacionExtraAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            decimal cantidad, 
            decimal precioAdicional = 0, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(itemId != Guid.Empty, "El ID del ítem no puede estar vacío", "ItemId");
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngrediente), "El nombre del ingrediente no puede estar vacío", "NombreIngrediente");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor a cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Agregar personalización de tipo "extra"
                var personalizado = comanda.AgregarPersonalizacionExtra(
                    itemId, 
                    ingredienteId, 
                    nombreIngrediente, 
                    cantidad, 
                    precioAdicional);
                
                if (!personalizado)
                {
                    _notificationManager.AddError($"No se pudo agregar la personalización al ítem con ID {itemId}", "ItemId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar personalización: {ex.Message}", "AgregarPersonalizacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AgregarPersonalizacionQuitarAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(itemId != Guid.Empty, "El ID del ítem no puede estar vacío", "ItemId");
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngrediente), "El nombre del ingrediente no puede estar vacío", "NombreIngrediente");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Agregar personalización de tipo "quitar"
                var personalizado = comanda.AgregarPersonalizacionQuitar(
                    itemId, 
                    ingredienteId, 
                    nombreIngrediente);
                
                if (!personalizado)
                {
                    _notificationManager.AddError($"No se pudo agregar la personalización al ítem con ID {itemId}", "ItemId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar personalización: {ex.Message}", "AgregarPersonalizacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AgregarPersonalizacionSustituirAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            Guid ingredienteSustitucionId, 
            string nombreIngredienteSustitucion, 
            decimal cantidad = 1, 
            decimal precioAdicional = 0, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(itemId != Guid.Empty, "El ID del ítem no puede estar vacío", "ItemId");
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngrediente), "El nombre del ingrediente no puede estar vacío", "NombreIngrediente");
            _notificationManager.Require(ingredienteSustitucionId != Guid.Empty, "El ID del ingrediente de sustitución no puede estar vacío", "IngredienteSustitucionId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreIngredienteSustitucion), "El nombre del ingrediente de sustitución no puede estar vacío", "NombreIngredienteSustitucion");
            _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor a cero", "Cantidad");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Agregar personalización de tipo "sustituir"
                var personalizado = comanda.AgregarPersonalizacionSustituir(
                    itemId, 
                    ingredienteId, 
                    nombreIngrediente, 
                    ingredienteSustitucionId, 
                    nombreIngredienteSustitucion, 
                    cantidad, 
                    precioAdicional);
                
                if (!personalizado)
                {
                    _notificationManager.AddError($"No se pudo agregar la personalización al ítem con ID {itemId}", "ItemId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al agregar personalización: {ex.Message}", "AgregarPersonalizacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ActualizarEstadoComandaAsync(
            Guid comandaId, 
            EstadoComanda nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(Enum.IsDefined(typeof(EstadoComanda), nuevoEstado), $"El estado '{nuevoEstado}' no es válido", "NuevoEstado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Actualizar estado según el tipo
                try
                {
                    switch (nuevoEstado)
                    {
                        case EstadoComanda.EnProceso:
                            comanda.MarcarEnPreparacion();
                            break;
                        case EstadoComanda.Lista:
                            comanda.MarcarLista();
                            break;
                        case EstadoComanda.Entregada:
                            comanda.MarcarEntregada();
                            break;
                        case EstadoComanda.Finalizada:
                            comanda.MarcarPagada();
                            break;
                        case EstadoComanda.Cancelada:
                            comanda.Cancelar("Cancelada desde servicio de operaciones");
                            break;
                        default:
                            _notificationManager.AddError($"No se puede actualizar al estado {nuevoEstado}", "NuevoEstado");
                            return _notificationManager.ToResult<bool>(false);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "ActualizarEstado");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar estado de comanda: {ex.Message}", "ActualizarEstadoComanda");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AplicarDescuentoComandaAsync(
            Guid comandaId, 
            decimal montoDescuento, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(montoDescuento > 0, "El monto del descuento debe ser mayor a cero", "MontoDescuento");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(motivo), "El motivo del descuento no puede estar vacío", "Motivo");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                try
                {
                    // Aplicar descuento
                    comanda.AplicarDescuento(montoDescuento, motivo);
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "AplicarDescuento");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _comandaRepository.ActualizarAsync(comanda);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al aplicar descuento a comanda: {ex.Message}", "AplicarDescuentoComanda");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        #endregion
        
        #region Reservaciones

        /// <inheritdoc />
        public async Task<Result<Reservacion>> CrearReservacionAsync(
            Guid clienteId, 
            DateTime fecha, 
            int cantidadPersonas, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Buscar mesa disponible primero
                var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                    fecha.Date, 
                    fecha.TimeOfDay, 
                    cantidadPersonas, 
                    90, // Duración predeterminada en minutos
                    cancellationToken);
                
                if (!mesasDisponibles.Any())
                {
                    _notificationManager.AddError("No hay mesas disponibles para la fecha y cantidad de personas especificadas", "Disponibilidad");
                    return _notificationManager.ToResult<Reservacion>(null);
                }
                
                // Seleccionar la primera mesa disponible
                var mesaId = mesasDisponibles.First();
                
                // Usar ReservacionBuilder para crear la reservación con validaciones robustas
                var builder = new ReservacionBuilder(_notificationManager, _reservacionBuilderLogger);
                
                var resultadoReservacion = builder
                    .ParaCliente(clienteId)
                    .ParaMesa(mesaId)
                    .ParaFecha(fecha.Date)
                    .AHora(fecha.TimeOfDay)
                    .ConDuracion(TimeSpan.FromMinutes(90)) // Duración predeterminada
                    .ParaPersonas(cantidadPersonas)
                    .ConTelefono("000-000-0000") // Teléfono temporal (deberá actualizarse)
                    .ConEmail("temp@restaurant.com") // Email temporal (deberá actualizarse)
                    .ConObservaciones(observaciones)
                    .Construir();
                
                if (!resultadoReservacion.Succeeded)
                {
                    return resultadoReservacion; // Ya tiene los errores del builder
                }
                
                var reservacion = resultadoReservacion.Value!;
                
                // Persistir la reservación
                await _reservacionRepository.AgregarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(reservacion);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear reservación: {ex.Message}", "CrearReservacion");
                return _notificationManager.ToResult<Reservacion>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Reservacion>> ObtenerReservacionAsync(Guid reservacionId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Reservacion>(null);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<Reservacion>(null);
                }
                
                return Result.Success(reservacion);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener la reservación: {ex.Message}", "ObtenerReservacion");
                return _notificationManager.ToResult<Reservacion>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AsignarMesaAReservacionAsync(
            Guid reservacionId, 
            Guid mesaId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            _notificationManager.Require(mesaId != Guid.Empty, "El ID de la mesa no puede estar vacío", "MesaId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar que la mesa exista
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _notificationManager.AddError($"No se encontró la mesa con ID {mesaId}", "MesaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar disponibilidad
                var disponible = await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                    mesaId,
                    reservacion.FechaReservacion,
                    reservacion.FechaReservacion.TimeOfDay,
                    (int)reservacion.DuracionEstimada.TotalMinutes,
                    cancellationToken);
                
                if (!disponible)
                {
                    _notificationManager.AddError("La mesa seleccionada no está disponible en el horario de la reservación", "Disponibilidad");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Asignar mesa
                reservacion.CambiarMesa(mesaId);
                
                // Persistir cambios
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al asignar mesa a reservación: {ex.Message}", "AsignarMesa");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ActualizarEstadoReservacionAsync(
            Guid reservacionId, 
            EstadoReservacion nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            _notificationManager.Require(Enum.IsDefined(typeof(EstadoReservacion), nuevoEstado), $"El estado '{nuevoEstado}' no es válido", "NuevoEstado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                try
                {
                    // Actualizar estado según el tipo
                    switch (nuevoEstado)
                    {
                        case EstadoReservacion.Confirmada:
                            reservacion.Confirmar();
                            break;
                        case EstadoReservacion.Completada:
                            reservacion.Completar();
                            break;
                        case EstadoReservacion.Cancelada:
                            reservacion.Cancelar("Cancelada desde servicio de operaciones");
                            break;
                        case EstadoReservacion.NoShow:
                            reservacion.MarcarNoAsistio();
                            break;
                        default:
                            _notificationManager.AddError($"No se puede actualizar al estado {nuevoEstado}", "NuevoEstado");
                            return _notificationManager.ToResult<bool>(false);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "ActualizarEstado");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Persistir cambios
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar estado de reservación: {ex.Message}", "ActualizarEstadoReservacion");
                return _notificationManager.ToResult<bool>(false);
            }
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<Guid>>> VerificarDisponibilidadMesasAsync(
            DateTime fecha, 
            int cantidadPersonas, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(fecha > DateTime.Now, "La fecha debe ser posterior a la fecha actual", "Fecha");
            _notificationManager.Require(cantidadPersonas > 0, "La cantidad de personas debe ser mayor a cero", "CantidadPersonas");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<IEnumerable<Guid>>(Array.Empty<Guid>());
            }
            
            try
            {
                // Obtener mesas disponibles
                var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                    fecha.Date,
                    fecha.TimeOfDay,
                    cantidadPersonas,
                    90, // Duración predeterminada en minutos
                    cancellationToken);
                
                return Result.Success(mesasDisponibles);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al verificar disponibilidad de mesas: {ex.Message}", "VerificarDisponibilidad");
                return _notificationManager.ToResult<IEnumerable<Guid>>(Array.Empty<Guid>());
            }
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<Reservacion>>> ObtenerReservacionesPorRangoFechasAsync(
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(fechaInicio <= fechaFin, "La fecha de inicio debe ser anterior o igual a la fecha fin", "FechaInicio");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<IEnumerable<Reservacion>>(Array.Empty<Reservacion>());
            }
            
            try
            {
                var reservaciones = await _reservacionRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, cancellationToken);
                return Result.Success(reservaciones);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al obtener reservaciones: {ex.Message}", "ObtenerReservaciones");
                return _notificationManager.ToResult<IEnumerable<Reservacion>>(Array.Empty<Reservacion>());
            }
        }

        /// <inheritdoc />
        public async Task<Result<Comanda>> ConvertirReservacionAComandaAsync(
            Guid reservacionId, 
            Guid meseroId, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(reservacionId != Guid.Empty, "El ID de la reservación no puede estar vacío", "ReservacionId");
            _notificationManager.Require(meseroId != Guid.Empty, "El ID del mesero no puede estar vacío", "MeseroId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Comanda>(null);
            }
            
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
                if (reservacion == null)
                {
                    _notificationManager.AddError($"No se encontró la reservación con ID {reservacionId}", "ReservacionId");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Verificar que la reservación esté confirmada
                if (reservacion.Estado != EstadoReservacion.Confirmada)
                {
                    _notificationManager.AddError("Solo se pueden convertir a comanda las reservaciones confirmadas", "Estado");
                    return _notificationManager.ToResult<Comanda>(null);
                }
                
                // Usar ComandaBuilder para crear la comanda con validaciones robustas
                var builder = new ComandaBuilder(_notificationManager, _comandaBuilderLogger);
                
                var observacionesComanda = $"Comanda generada desde reservación #{reservacionId}";
                
                var resultadoComanda = builder
                    .ConMesero(meseroId)
                    .ConCliente(reservacion.ClienteId)
                    .EnMesa(reservacion.MesaId)
                    .ConObservaciones(observacionesComanda)
                    .Construir();
                
                if (!resultadoComanda.Succeeded)
                {
                    return resultadoComanda; // Ya tiene los errores del builder
                }
                
                var comanda = resultadoComanda.Value!;
                
                // Marcar la reservación como completada
                reservacion.Completar();
                
                // Persistir cambios
                await _comandaRepository.AgregarAsync(comanda);
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                return Result.Success(comanda);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al convertir reservación a comanda: {ex.Message}", "ConvertirReservacion");
                return _notificationManager.ToResult<Comanda>(null);
            }
        }

        #endregion
    }
} 