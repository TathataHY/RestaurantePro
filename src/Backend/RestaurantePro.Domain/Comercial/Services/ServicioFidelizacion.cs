namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Servicio para gestionar la fidelización de clientes
    /// </summary>
    public class ServicioFidelizacion : IServicioFidelizacion
    {
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IHistorialPuntosRepository _historialPuntosRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationManager _notificationManager;

        /// <summary>
        /// Constructor del servicio de fidelización
        /// </summary>
        public ServicioFidelizacion(
            ITarjetaFidelizacionRepository tarjetaRepository,
            IClienteRepository clienteRepository,
            IHistorialPuntosRepository historialPuntosRepository,
            IDateTimeService dateTimeService,
            INotificationManager notificationManager)
        {
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _historialPuntosRepository = historialPuntosRepository ?? throw new ArgumentNullException(nameof(historialPuntosRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }

        /// <summary>
        /// Crea una nueva tarjeta de fidelización y la asocia al cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Resultado de la operación con la tarjeta creada</returns>
        public async Task<Result<TarjetaFidelizacion>> CrearTarjetaFidelizacionAsync(Guid clienteId)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", propertyName: "ClienteId");
            
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<TarjetaFidelizacion>(null!);
            
            // Verificar que el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, default);
            if (cliente == null)
                return Result.Failure<TarjetaFidelizacion>("No existe un cliente con el ID especificado");

            // Verificar si ya tiene una tarjeta activa
            try
            {
                var tarjetaExistente = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(clienteId);
                return Result.Failure<TarjetaFidelizacion>($"El cliente ya tiene una tarjeta activa con código {tarjetaExistente.Codigo}");
            }
            catch (KeyNotFoundException)
            {
                // No tiene tarjeta activa, continuar con la creación
            }

            // Generar código único para la tarjeta
            string codigo = GenerarCodigoTarjeta(clienteId);

            // Crear nueva tarjeta
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, codigo);
            
            // Persistir la tarjeta
            await _tarjetaRepository.AgregarAsync(tarjeta);

            // Asociar la tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            await _clienteRepository.ActualizarAsync(cliente);

            return Result.Success(tarjeta);
        }

        /// <summary>
        /// Calcula el descuento aplicable para un cliente según su nivel de fidelización
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Información del descuento aplicable</returns>
        public async Task<Result<decimal>> CalcularDescuentoAsync(Guid clienteId, decimal montoTotal)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", propertyName: "ClienteId")
                .Require(montoTotal > 0, "El monto total debe ser mayor a cero", propertyName: "MontoTotal");
                
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<decimal>(0);
                
            // Obtener el cliente primero
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, default);
            if (cliente == null)
                return Result.Failure<decimal>($"No existe un cliente con el ID {clienteId}");

            // Verificar si el cliente está activo
            if (!cliente.EstaActivo)
                return Result.Failure<decimal>("No se puede calcular descuento para un cliente inactivo");

            // Verificar si el cliente tiene una tarjeta asociada
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                return Result.Success(0m);
                
            // Obtener tarjeta activa del cliente
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value, default, false);
            
            // Si no existe o no está activa, no hay descuento
            if (tarjeta == null)
                return Result.Success(0m);
                
            if (tarjeta.Estado != EstadoTarjeta.Activa)
                return Result.Success(0m);
            
            // Calcular descuento según el nivel
            int porcentajeDescuento = ObtenerPorcentajeDescuentoPorNivel(tarjeta.NivelFidelizacion);
            decimal montoDescuento = montoTotal * (porcentajeDescuento / 100m);
            
            return Result.Success(montoDescuento);
        }

        /// <summary>
        /// Acumula puntos para un cliente basado en el monto de su comanda
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Resultado de la operación con los puntos acumulados</returns>
        public async Task<Result<int>> AcumularPuntosAsync(Guid clienteId, Guid comandaId, decimal montoTotal)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", propertyName: "ClienteId")
                .Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", propertyName: "ComandaId")
                .Require(montoTotal > 0, "El monto total debe ser mayor a cero", propertyName: "MontoTotal");
                
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<int>(0);
                
            // Verificamos primero si el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, default);
            if (cliente == null)
                return Result.Failure<int>("No se encontró el cliente especificado");
            
            // Verificar si el cliente está activo
            if (!cliente.EstaActivo)
                return Result.Failure<int>("No se pueden acumular puntos para un cliente inactivo");

            // Verificar si el cliente tiene una tarjeta asociada
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                return Result.Failure<int>("El cliente no tiene una tarjeta de fidelización asociada");
                
            // Obtener tarjeta por ID
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value, default, false);
            
            // Si no existe o no está activa, no puede acumular puntos
            if (tarjeta == null)
                return Result.Failure<int>("No se encontró la tarjeta asociada al cliente");
                
            if (tarjeta.Estado != EstadoTarjeta.Activa)
                return Result.Failure<int>($"La tarjeta no está activa, estado actual: {tarjeta.Estado}");
            
            // Factor de conversión basado en el nivel
            int factorConversion = ObtenerFactorConversionPorNivel(tarjeta.NivelFidelizacion);
            
            // Acumular puntos directamente en la tarjeta (la tarjeta maneja su historial interno)
            var historial = tarjeta.AgregarPuntosPorCompra(
                montoTotal,
                factorConversion,
                $"Acumulación por comanda {comandaId}"
            );
            
            // Actualizar la tarjeta en el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjeta);
            
            // Actualizar el cliente con los puntos acumulados
            cliente.AgregarPuntos(historial.Puntos);
            await _clienteRepository.ActualizarAsync(cliente);
            
            return Result.Success(historial.Puntos);
        }

        /// <summary>
        /// Canjea puntos de un cliente
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="puntos">Cantidad de puntos a canjear</param>
        /// <param name="concepto">Motivo del canje</param>
        /// <returns>Resultado de la operación con los puntos restantes</returns>
        public async Task<Result<int>> CanjearPuntosAsync(Guid clienteId, int puntos, string concepto)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", propertyName: "ClienteId")
                .Require(puntos > 0, "Los puntos a canjear deben ser mayores a cero", propertyName: "Puntos")
                .RequireNotEmpty(concepto, "El concepto del canje no puede estar vacío", propertyName: "Concepto");
                
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<int>(0);
                
            // Verificamos primero si el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, default);
            if (cliente == null)
                return Result.Failure<int>($"No existe un cliente con el ID {clienteId}");
            
            // Verificar si el cliente está activo
            if (!cliente.EstaActivo)
                return Result.Failure<int>("No se pueden canjear puntos para un cliente inactivo");

            // Verificar si el cliente tiene una tarjeta asociada
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                return Result.Failure<int>("El cliente no tiene una tarjeta de fidelización asociada");
                
            // Obtener tarjeta por ID
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value, default, false);
            
            // Si no existe o no está activa, no puede canjear puntos
            if (tarjeta == null)
                return Result.Failure<int>("No se encontró la tarjeta asociada al cliente");
                
            if (tarjeta.Estado != EstadoTarjeta.Activa)
                return Result.Failure<int>($"La tarjeta no está activa, estado actual: {tarjeta.Estado}");
            
            // Verificar si tiene puntos suficientes
            if (tarjeta.PuntosDisponibles < puntos)
                return Result.Failure<int>($"El cliente no tiene suficientes puntos disponibles. " +
                    $"Disponibles: {tarjeta.PuntosDisponibles}, Solicitados: {puntos}");
                    
            // Canjear puntos directamente en la tarjeta (la tarjeta maneja su historial interno)
            tarjeta.CanjearPuntos(puntos, concepto);
            
            // Actualizar la tarjeta en el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjeta);
            
            return Result.Success(tarjeta.PuntosDisponibles);
        }

        /// <summary>
        /// Obtiene el porcentaje de descuento según el nivel de fidelización
        /// </summary>
        private int ObtenerPorcentajeDescuentoPorNivel(NivelFidelizacion nivel)
        {
            switch (nivel)
            {
                case NivelFidelizacion.Plata:
                    return 5;
                case NivelFidelizacion.Oro:
                    return 10;
                case NivelFidelizacion.Platino:
                    return 15;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Obtiene el factor de conversión según el nivel de fidelización
        /// </summary>
        private int ObtenerFactorConversionPorNivel(NivelFidelizacion nivel)
        {
            // Cuanto menor es el factor, más puntos se obtienen
            switch (nivel)
            {
                case NivelFidelizacion.Plata:
                    return 8; // 100 pesos = 12.5 puntos
                case NivelFidelizacion.Oro:
                    return 5; // 100 pesos = 20 puntos
                case NivelFidelizacion.Platino:
                    return 3; // 100 pesos = 33.3 puntos
                default:
                    return 10; // 100 pesos = 10 puntos
            }
        }

        /// <summary>
        /// Genera un código único para la tarjeta de fidelización
        /// </summary>
        private string GenerarCodigoTarjeta(Guid clienteId)
        {
            // Formato: TF-XXXX-YYYY donde XXXX son dígitos aleatorios y YYYY es un hash del clienteId
            Random random = new Random();
            string randomPart = random.Next(1000, 9999).ToString();
            string hashPart = Math.Abs(clienteId.GetHashCode() % 10000).ToString().PadLeft(4, '0');
            
            return $"TF-{randomPart}-{hashPart}";
        }

        /// <summary>
        /// Calcula el descuento basado en los puntos a utilizar
        /// </summary>
        /// <param name="puntos">Puntos a utilizar</param>
        /// <param name="montoTotal">Monto total (opcional, solo para validaciones)</param>
        /// <returns>Monto del descuento calculado</returns>
        public decimal CalcularDescuentoPorPuntos(int puntos, decimal montoTotal)
        {
            if (puntos <= 0)
                return 0;
            
            // Valor de cada punto en pesos (podría variar según lógica de negocio)
            const decimal VALOR_PUNTO = 0.5m;
            
            // Calcular el descuento
            decimal descuento = puntos * VALOR_PUNTO;
            
            // Verificar que el descuento no exceda límites si hay un monto total
            if (montoTotal > 0)
            {
                // El descuento máximo es el 50% del monto total
                decimal descuentoMaximo = montoTotal * 0.5m;
                if (descuento > descuentoMaximo)
                    descuento = descuentoMaximo;
            }
            
            return descuento;
        }

        /// <summary>
        /// Agrega puntos a la tarjeta de fidelización de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntos">Puntos a agregar</param>
        /// <param name="motivo">Motivo por el que se agregan los puntos</param>
        /// <returns>Resultado de la operación con los puntos totales</returns>
        public async Task<Result<int>> AgregarPuntosAsync(Guid clienteId, int puntos, string motivo)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager
                .Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío", propertyName: "ClienteId")
                .Require(puntos > 0, "Los puntos deben ser mayores a cero", propertyName: "Puntos")
                .RequireNotEmpty(motivo, "El motivo no puede estar vacío", propertyName: "Motivo");
                
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<int>(0);
                
            // Obtener el cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, default);
            if (cliente == null)
                return Result.Failure<int>("No se encontró el cliente especificado");
                
            // Verificar si el cliente tiene tarjeta de fidelización
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                return Result.Failure<int>("El cliente no tiene tarjeta de fidelización");
                
            // Obtener tarjeta por ID
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value, default, false);
            
            // Si no existe, error
            if (tarjeta == null)
                return Result.Failure<int>("No se encontró la tarjeta asociada al cliente");
                
            try
            {
                // Agregar puntos a la tarjeta
                var historial = tarjeta.AgregarPuntos(puntos, motivo);
                
                // Persistir cambios
                await _tarjetaRepository.ActualizarAsync(tarjeta);
                
                // Retornar los puntos disponibles
                return Result.Success(tarjeta.PuntosDisponibles);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>(ex.Message);
            }
        }
    }
} 