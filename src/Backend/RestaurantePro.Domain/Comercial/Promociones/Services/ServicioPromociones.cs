namespace RestaurantePro.Domain.Comercial.Promociones.Services
{
    /// <summary>
    /// Implementación del servicio de dominio para gestionar promociones
    /// </summary>
    public class ServicioPromociones : IServicioPromociones
    {
        private readonly IPromocionRepository _promocionRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor del servicio de promociones
        /// </summary>
        public ServicioPromociones(
            IPromocionRepository promocionRepository,
            IClienteRepository clienteRepository,
            ITarjetaFidelizacionRepository tarjetaRepository,
            IDateTimeService dateTimeService)
        {
            _promocionRepository = promocionRepository ?? throw new ArgumentNullException(nameof(promocionRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <summary>
        /// Obtiene las promociones aplicables para un cliente y monto específicos
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="monto">Monto de la compra</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de promociones aplicables</returns>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesAplicablesAsync(
            Guid clienteId, 
            decimal monto, 
            CancellationToken cancellationToken = default)
        {
            // Obtener cliente y sus datos
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente == null)
            {
                return Enumerable.Empty<Promocion>();
            }
            
            // Obtener todas las promociones activas
            var promocionesActivas = await _promocionRepository.ObtenerPromocionesActivasAsync(cancellationToken);
            
            // Filtrar por las que son aplicables al cliente y monto
            var puntosDisponibles = cliente.TarjetaFidelizacion?.PuntosActuales ?? 0;
            var fechaActual = _dateTimeService.Now;
            
            var especificacion = new PromocionValidaParaClienteSpecification(
                clienteId, 
                puntosDisponibles, 
                monto, 
                fechaActual);
                
            return promocionesActivas.Where(p => especificacion.IsSatisfiedBy(p));
        }
        
        /// <summary>
        /// Aplica una promoción específica a una comanda
        /// </summary>
        /// <param name="promocionId">ID de la promoción</param>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="montoOriginal">Monto original de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Monto del descuento aplicado, o 0 si no es aplicable</returns>
        public async Task<decimal> AplicarPromocionAsync(
            Guid promocionId, 
            Guid clienteId, 
            Guid comandaId, 
            decimal montoOriginal, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la promoción
            var promocion = await _promocionRepository.ObtenerPorIdAsync(promocionId, cancellationToken);
            if (promocion == null || promocion.Estado != EstadoPromocion.Activa)
            {
                return 0;
            }
            
            // Verificar que la promoción está vigente
            if (!promocion.EstaVigente())
            {
                return 0;
            }
            
            // Verificar monto mínimo
            if (montoOriginal < promocion.MontoMinimo)
            {
                return 0;
            }
            
            // Si es promoción de tipo canje de puntos, verificar puntos del cliente
            if (promocion.Tipo == TipoPromocion.CanjePuntos)
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente == null || cliente.TarjetaFidelizacion == null || 
                    cliente.TarjetaFidelizacion.PuntosActuales < promocion.PuntosRequeridos)
                {
                    return 0;
                }
                
                // Restar puntos al cliente
                await _clienteRepository.RestarPuntosAsync(
                    clienteId, 
                    promocion.PuntosRequeridos, 
                    cancellationToken);
            }
            
            // Calcular descuento
            var descuento = promocion.CalcularDescuento(montoOriginal);
            
            // Registrar uso de la promoción
            promocion.RegistrarUso(clienteId, comandaId, descuento);
            await _promocionRepository.ActualizarAsync(promocion, cancellationToken);
            
            return descuento;
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesValidasParaClienteAsync(
            Guid clienteId,
            decimal montoTotal,
            CancellationToken cancellationToken = default)
        {
            // Obtener el cliente y sus puntos disponibles
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente == null)
                return Enumerable.Empty<Promocion>();
                
            // Obtener los puntos disponibles (de tarjeta o del cliente)
            int puntosDisponibles = 0;
            if (cliente.TieneTarjetaFidelizacion() && cliente.TarjetaFidelizacionPrincipalId.HasValue)
            {
                var tarjeta = await _promocionRepository.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId.Value, cancellationToken);
                puntosDisponibles = tarjeta?.PuntosDisponibles ?? 0;
            }
            else
            {
                puntosDisponibles = cliente.ObtenerPuntosFidelizacionDisponibles();
            }
            
            // Obtener promociones activas
            var promocionesActivas = await _promocionRepository.ObtenerPromocionesActivasAsync(cancellationToken);
            
            // Aplicar especificación para validar promociones para este cliente y monto
            var fechaActual = _dateTimeService.Now;
            var spec = new PromocionValidaParaClienteSpecification(clienteId, puntosDisponibles, montoTotal, fechaActual);
            
            return promocionesActivas.Where(p => spec.IsSatisfiedBy(p)).ToList();
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesParaProductoAsync(
            Guid productoId,
            Guid? categoriaId = null,
            CancellationToken cancellationToken = default)
        {
            // Obtener todas las promociones activas
            var promocionesActivas = await _promocionRepository.ObtenerPromocionesActivasAsync(cancellationToken);
            
            // Aplicar especificación para filtrar por producto y categoría
            var specProducto = new PromocionParaProductoSpecification(productoId, categoriaId);
            var specActiva = new PromocionActivaSpecification(_dateTimeService.Now);
            
            // Combinar especificaciones
            var spec = specActiva.And(specProducto);
            
            return promocionesActivas.Where(p => spec.IsSatisfiedBy(p)).ToList();
        }
        
        /// <inheritdoc/>
        public async Task<decimal> CalcularDescuentoTotalAsync(
            IEnumerable<Guid> promocionesIds,
            decimal montoOriginal,
            IEnumerable<Guid> productosIds,
            CancellationToken cancellationToken = default)
        {
            if (montoOriginal <= 0)
                return 0;
                
            var promocionesIdsList = promocionesIds.ToList();
            if (!promocionesIdsList.Any())
                return 0;
                
            // Obtener las promociones solicitadas
            var promociones = new List<Promocion>();
            foreach (var id in promocionesIdsList)
            {
                var promocion = await _promocionRepository.ObtenerPorIdAsync(id, cancellationToken);
                if (promocion != null && promocion.EstaVigente())
                    promociones.Add(promocion);
            }
            
            // Verificar compatibilidad
            if (!await ValidarCompatibilidadPromocionesAsync(promocionesIdsList, cancellationToken))
                throw new InvalidOperationException("Las promociones seleccionadas no son compatibles entre sí");
                
            // Calcular descuento total
            decimal descuentoTotal = 0;
            
            // Primero aplicamos las promociones sobre el total
            var promocionesTotal = promociones.Where(p => p.Tipo == Enums.TipoPromocion.PorcentajeTotal 
                                                       || p.Tipo == Enums.TipoPromocion.MontoFijoTotal);
            foreach (var promocion in promocionesTotal)
            {
                descuentoTotal += promocion.CalcularDescuento(montoOriginal);
            }
            
            // Limitar el descuento al monto original
            descuentoTotal = Math.Min(descuentoTotal, montoOriginal);
            
            return descuentoTotal;
        }
        
        /// <inheritdoc/>
        public async Task<bool> RegistrarUsoPromocionAsync(
            Guid promocionId,
            Guid clienteId,
            Guid comandaId,
            decimal montoAplicado,
            CancellationToken cancellationToken = default)
        {
            var promocion = await _promocionRepository.ObtenerPorIdAsync(promocionId, cancellationToken);
            if (promocion == null)
                return false;
                
            // Verificar si la promoción está activa
            if (!promocion.EstaVigente())
                return false;
                
            try
            {
                // Registrar el uso de la promoción
                promocion.RegistrarUso(clienteId, comandaId, montoAplicado);
                
                // Guardar los cambios
                await _promocionRepository.ActualizarAsync(promocion, cancellationToken);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> ValidarCompatibilidadPromocionesAsync(
            IEnumerable<Guid> promocionesIds,
            CancellationToken cancellationToken = default)
        {
            var promocionesIdsList = promocionesIds.ToList();
            
            // Si hay 0 o 1 promociones, son compatibles
            if (promocionesIdsList.Count <= 1)
                return true;
                
            // Obtener las promociones
            var promociones = new List<Promocion>();
            foreach (var id in promocionesIdsList)
            {
                var promocion = await _promocionRepository.ObtenerPorIdAsync(id, cancellationToken);
                if (promocion != null)
                    promociones.Add(promocion);
            }
            
            // Verificar si hay alguna promoción no acumulable
            if (promociones.Any(p => !p.EsAcumulable))
            {
                // Si hay más de una promoción no acumulable, no son compatibles
                if (promociones.Count(p => !p.EsAcumulable) > 1)
                    return false;
                    
                // Si hay una promoción no acumulable, solo puede estar ella sola
                if (promociones.Count > 1)
                    return false;
            }
            
            // Verificar si hay promociones del mismo tipo
            var tiposPorcentajeTotal = promociones.Count(p => p.Tipo == Enums.TipoPromocion.PorcentajeTotal);
            var tiposMontoFijoTotal = promociones.Count(p => p.Tipo == Enums.TipoPromocion.MontoFijoTotal);
            
            // No permitir más de una promoción del mismo tipo para descuentos globales
            if (tiposPorcentajeTotal > 1 || tiposMontoFijoTotal > 1)
                return false;
                
            return true;
        }
    }
} 