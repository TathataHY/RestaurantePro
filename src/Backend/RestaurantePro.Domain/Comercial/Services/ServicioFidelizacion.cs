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
        /// Calcula el descuento aplicable para un cliente según su nivel de fidelización
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="montoTotal">Monto total de la comanda</param>
        /// <returns>Información del descuento aplicable</returns>
        public async Task<ResultadoDescuento> CalcularDescuentoAsync(Guid clienteId, decimal montoTotal)
        {
            // Obtener tarjeta activa del cliente
            var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(clienteId);
            
            // Si no tiene tarjeta activa, no hay descuento
            if (tarjeta == null)
            {
                return new ResultadoDescuento(0, 0);
            }
            
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
            {
                throw new InvalidOperationException($"No existe un cliente con el ID {clienteId}");
            }
            
            // Obtener tarjeta activa del cliente
            var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(clienteId);
            
            // Si no tiene tarjeta activa, no acumula puntos
            if (tarjeta == null)
            {
                throw new InvalidOperationException("El cliente no tiene una tarjeta activa para acumular puntos");
            }
            
            // Calcular puntos a acumular (10% del monto)
            int puntos = CalcularPuntosAcumular(montoTotal);
            
            // Agregar puntos a la tarjeta
            tarjeta.AgregarPuntos(puntos);
            await _tarjetaRepository.ActualizarAsync(tarjeta);
            
            // Registrar en el historial usando el método de fábrica
            var historial = HistorialPuntos.CrearRegistroPorCompra(
                tarjeta.Id,
                montoTotal,
                CalcularFactorConversion(),
                $"Acumulación por comanda {comandaId}"
            );
            
            await _historialPuntosRepository.AgregarAsync(historial);
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
            // Obtener tarjeta activa del cliente
            var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(clienteId);
            
            // Si no tiene tarjeta activa, no puede canjear
            if (tarjeta == null)
            {
                throw new InvalidOperationException("El cliente no tiene una tarjeta activa para canjear puntos");
            }
            
            // Verificar si tiene suficientes puntos
            if (tarjeta.PuntosAcumulados < puntos)
            {
                throw new InvalidOperationException($"Puntos insuficientes. Disponibles: {tarjeta.PuntosAcumulados}, Solicitados: {puntos}");
            }
            
            // Canjear puntos
            tarjeta.CanjearPuntos(puntos, concepto);
            await _tarjetaRepository.ActualizarAsync(tarjeta);
            
            // Registrar en el historial usando el método de fábrica
            var historial = HistorialPuntos.CrearRegistroCanjeados(
                tarjeta.Id,
                puntos,
                concepto
            );
            
            await _historialPuntosRepository.AgregarAsync(historial);
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
        /// Calcula los puntos a acumular basado en el monto
        /// </summary>
        private int CalcularPuntosAcumular(decimal monto)
        {
            // 10% del monto para convertir a puntos
            return (int)(monto * 0.1m);
        }

        /// <summary>
        /// Calcula el factor de conversión para los puntos
        /// </summary>
        private int CalcularFactorConversion()
        {
            // Asumimos 10 unidades monetarias = 1 punto
            return 10;
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