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

        /// <summary>
        /// Constructor del servicio de fidelización
        /// </summary>
        public ServicioFidelizacion(
            ITarjetaFidelizacionRepository tarjetaRepository,
            IClienteRepository clienteRepository,
            IHistorialPuntosRepository historialPuntosRepository,
            IDateTimeService dateTimeService)
        {
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _historialPuntosRepository = historialPuntosRepository ?? throw new ArgumentNullException(nameof(historialPuntosRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <summary>
        /// Crea una nueva tarjeta de fidelización y la asocia al cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>La tarjeta creada</returns>
        public async Task<TarjetaFidelizacion> CrearTarjetaFidelizacionAsync(Guid clienteId)
        {
            // Verificar que el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId);
            if (cliente == null)
                throw new InvalidOperationException($"No existe un cliente con el ID {clienteId}");

            // Verificar si ya tiene una tarjeta activa
            var tarjetaExistente = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(clienteId);
            if (tarjetaExistente != null)
                throw new InvalidOperationException($"El cliente ya tiene una tarjeta activa con código {tarjetaExistente.Codigo}");

            // Generar código único para la tarjeta
            string codigo = GenerarCodigoTarjeta(clienteId);

            // Crear nueva tarjeta
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, codigo);
            
            // Persistir la tarjeta
            await _tarjetaRepository.AgregarAsync(tarjeta);

            // Asociar la tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            await _clienteRepository.ActualizarAsync(cliente);

            return tarjeta;
        }

        /// <summary>
        /// Calcula el descuento aplicable para un cliente según su nivel de fidelización
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Información del descuento aplicable</returns>
        public async Task<ResultadoDescuento> CalcularDescuentoAsync(Guid clienteId, decimal montoTotal)
        {
            // Obtener el cliente primero
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId);
            if (cliente == null)
                return new ResultadoDescuento(0, 0);

            // Verificar si el cliente está activo
            if (!cliente.EstaActivo)
                return new ResultadoDescuento(0, 0);

            // Verificar si el cliente tiene una tarjeta asociada
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                return new ResultadoDescuento(0, 0);
                
            // Obtener tarjeta activa del cliente
            var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(clienteId);
            
            // Si no existe o no está activa, no hay descuento
            if (tarjeta == null || tarjeta.Estado != EstadoTarjeta.Activa)
                return new ResultadoDescuento(0, 0);
            
            // Calcular descuento según el nivel
            int porcentajeDescuento = ObtenerPorcentajeDescuentoPorNivel(tarjeta.NivelFidelizacion);
            decimal montoDescuento = montoTotal * (porcentajeDescuento / 100m);
            
            return new ResultadoDescuento(porcentajeDescuento, montoDescuento);
        }

        /// <summary>
        /// Acumula puntos para un cliente basado en el monto de su comanda
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task AcumularPuntosAsync(Guid clienteId, Guid comandaId, decimal montoTotal)
        {
            // Verificamos primero si el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId);
            if (cliente == null)
                throw new InvalidOperationException($"No existe un cliente con el ID {clienteId}");
            
            // Verificar si el cliente está activo
            if (!cliente.EstaActivo)
                throw new InvalidOperationException("No se pueden acumular puntos para un cliente inactivo");

            // Verificar si el cliente tiene una tarjeta asociada
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                throw new InvalidOperationException("El cliente no tiene una tarjeta de fidelización asociada");
                
            // Obtener tarjeta por ID
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value);
            
            // Si no existe o no está activa, no puede acumular puntos
            if (tarjeta == null)
                throw new InvalidOperationException("No se encontró la tarjeta asociada al cliente");
                
            if (tarjeta.Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException($"La tarjeta no está activa, estado actual: {tarjeta.Estado}");
            
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
        }

        /// <summary>
        /// Canjea puntos de un cliente
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="puntos">Cantidad de puntos a canjear</param>
        /// <param name="concepto">Motivo del canje</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task CanjearPuntosAsync(Guid clienteId, int puntos, string concepto)
        {
            // Verificamos primero si el cliente existe
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId);
            if (cliente == null)
                throw new InvalidOperationException($"No existe un cliente con el ID {clienteId}");
            
            // Verificar si el cliente está activo
            if (!cliente.EstaActivo)
                throw new InvalidOperationException("No se pueden canjear puntos para un cliente inactivo");

            // Verificar si el cliente tiene una tarjeta asociada
            if (!cliente.TarjetaFidelizacionPrincipalId.HasValue)
                throw new InvalidOperationException("El cliente no tiene una tarjeta de fidelización asociada");
                
            // Obtener tarjeta por ID
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value);
            
            // Si no existe o no está activa, no puede canjear puntos
            if (tarjeta == null)
                throw new InvalidOperationException("No se encontró la tarjeta asociada al cliente");
                
            if (tarjeta.Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException($"La tarjeta no está activa, estado actual: {tarjeta.Estado}");
            
            // Canjear puntos directamente en la tarjeta (la tarjeta maneja su historial interno)
            tarjeta.CanjearPuntos(puntos, concepto);
            
            // Actualizar la tarjeta en el repositorio
            await _tarjetaRepository.ActualizarAsync(tarjeta);
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
    }

    /// <summary>
    /// Representa el resultado de un cálculo de descuento
    /// </summary>
    public class ResultadoDescuento
    {
        /// <summary>
        /// Porcentaje de descuento aplicado
        /// </summary>
        public int PorcentajeDescuento { get; }
        
        /// <summary>
        /// Monto del descuento calculado
        /// </summary>
        public decimal MontoDescuento { get; }
        
        public ResultadoDescuento(int porcentajeDescuento, decimal montoDescuento)
        {
            PorcentajeDescuento = porcentajeDescuento;
            MontoDescuento = montoDescuento;
        }
    }
} 