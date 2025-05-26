namespace RestaurantePro.Domain.Comercial.Policies
{
    /// <summary>
    /// Implementación de la política de clientes frecuentes que analiza los patrones
    /// de consumo y actualiza los niveles de fidelización automáticamente
    /// </summary>
    public class ClientesFrecuentesPolicy : IClientesFrecuentesPolicy
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IServicioFidelizacion _servicioFidelizacion;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationManager _notificationManager;
        
        // Umbrales para determinar los niveles según la cantidad de visitas
        private const int UMBRAL_PLATA = 10;
        private const int UMBRAL_ORO = 20;
        private const int UMBRAL_PLATINO = 30;
        
        // Umbral para clientes inactivos (días sin visita)
        private const int DIAS_INACTIVIDAD = 90;
        
        // Umbrales para segmentación
        private const int UMBRAL_FRECUENCIA_ALTA = 8;  // Visitas en los últimos 60 días
        private const decimal UMBRAL_TICKET_ALTO = 50.0m;  // Gasto promedio por visita
        private const int UMBRAL_CLIENTE_CRECIENTE = 3;  // Incremento de visitas respecto al periodo anterior
        private const int UMBRAL_CLIENTE_DECRECIENTE = -3;  // Decremento de visitas respecto al periodo anterior
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ClientesFrecuentesPolicy(
            IClienteRepository clienteRepository,
            ITarjetaFidelizacionRepository tarjetaRepository,
            IServicioFidelizacion servicioFidelizacion,
            IDateTimeService dateTimeService,
            INotificationManager notificationManager)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }
        
        /// <inheritdoc />
        public async Task<ResultadoClientesFrecuentesPolicy> EjecutarPolicy(CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
                // Obtener todos los clientes activos con información de visitas
    var clientes = await _clienteRepository.ObtenerClientesActivosConVisitasAsync(90, 3, cancellationToken);
            
            if (!clientes.Any())
            {
                return resultado; // No hay clientes que procesar
            }
            
            // Procesar cada cliente
            foreach (var cliente in clientes)
            {
                try
                {
                    // Obtener la tarjeta de fidelización del cliente
                    var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, cancellationToken);
                    
                    // Determinar el nivel que debería tener según sus visitas
                    NivelFidelizacion nivelSegunVisitas = DeterminarNivelSegunVisitas(cliente.CantidadVisitas);
                    
                    // Si no tiene tarjeta, crear una
                    if (tarjeta == null)
                    {
                        tarjeta = TarjetaFidelizacion.Crear(
                            cliente.Id,
                            $"TF-{_dateTimeService.Now:yyyyMMdd}-{cliente.Id.ToString().Substring(0, 8)}"
                        );
                        tarjeta.Activar();
                        
                        // Si el nivel por defecto no es el que debería tener, actualizarlo
                        if (tarjeta.NivelFidelizacion != nivelSegunVisitas)
                        {
                            tarjeta.ActualizarNivel(nivelSegunVisitas);
                        }
                        
                        await _tarjetaRepository.AgregarAsync(tarjeta, cancellationToken);
                        resultado.TarjetasCreadas.Add(tarjeta.Id);
                    }
                    else
                    {
                        // Actualizar el nivel de la tarjeta si es necesario
                        tarjeta.ActualizarNivel(nivelSegunVisitas);
                        await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);
                    }
                    
                    // Agregar el cliente a la lista de actualizados
                    resultado.ClientesActualizados.Add(cliente.Id);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other clients
                    Console.WriteLine($"Error al procesar cliente {cliente.Id}: {ex.Message}");
                }
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<ResultadoClientesFrecuentesPolicy> EjecutarPolicyParaCliente(Guid clienteId, CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
            // Obtener el cliente específico
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            
            if (cliente == null)
            {
                throw new ArgumentException($"No existe un cliente con el ID {clienteId}", nameof(clienteId));
            }
            
            if (!cliente.EstaActivo)
            {
                return resultado; // No procesamos clientes inactivos
            }
            
            try
            {
                // Obtener la tarjeta de fidelización del cliente
                var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, cancellationToken);
                
                // Determinar el nivel que debería tener según sus visitas
                NivelFidelizacion nivelSegunVisitas = DeterminarNivelSegunVisitas(cliente.CantidadVisitas);
                
                // Si no tiene tarjeta, crear una
                if (tarjeta == null)
                {
                    tarjeta = TarjetaFidelizacion.Crear(
                        cliente.Id,
                        $"TF-{_dateTimeService.Now:yyyyMMdd}-{cliente.Id.ToString().Substring(0, 8)}"
                    );
                    tarjeta.Activar();
                    
                    // Si el nivel por defecto no es el que debería tener, actualizarlo
                    if (tarjeta.NivelFidelizacion != nivelSegunVisitas)
                    {
                        tarjeta.ActualizarNivel(nivelSegunVisitas);
                    }
                    
                    await _tarjetaRepository.AgregarAsync(tarjeta, cancellationToken);
                    resultado.TarjetasCreadas.Add(tarjeta.Id);
                }
                else
                {
                    // Actualizar el nivel de la tarjeta si es necesario
                    tarjeta.ActualizarNivel(nivelSegunVisitas);
                    await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);
                }
                
                // Agregar el cliente a la lista de actualizados
                resultado.ClientesActualizados.Add(cliente.Id);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error al procesar cliente {cliente.Id}: {ex.Message}");
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<ResultadoClientesFrecuentesPolicy> EjecutarSegmentacionClientes(CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
            // Obtener todos los clientes activos con su historial de visitas
            var fechaInicio = _dateTimeService.Now.AddDays(-90);
            var fechaFin = _dateTimeService.Now;
            var clientes = await _clienteRepository.ObtenerClientesConHistorialVisitasAsync(fechaInicio, fechaFin, cancellationToken);
            
            if (!clientes.Any())
                return resultado;
                
            // Inicializar conteo de segmentos
            foreach (SegmentoCliente segmento in Enum.GetValues(typeof(SegmentoCliente)))
            {
                resultado.ConteoSegmentos[segmento] = 0;
            }
            
            // Procesar cada cliente para determinar su segmento
            foreach (var cliente in clientes)
            {
                var segmentoAnterior = cliente.Segmento;
                var nuevoSegmento = DeterminarSegmentoCliente(cliente);
                
                // Solo actualizar si el segmento ha cambiado
                if (segmentoAnterior != nuevoSegmento)
                {
                    cliente.ActualizarSegmento(nuevoSegmento);
                    await _clienteRepository.ActualizarAsync(cliente, cancellationToken);
                    resultado.ClientesSegmentados.Add(cliente.Id);
                }
                
                // Actualizar conteo de segmentos
                resultado.ConteoSegmentos[nuevoSegmento]++;
            }
            
            return resultado;
        }
        
        /// <summary>
        /// Determina el nivel de fidelización que corresponde según la cantidad de visitas
        /// </summary>
        private NivelFidelizacion DeterminarNivelSegunVisitas(int cantidadVisitas)
        {
            if (cantidadVisitas >= UMBRAL_PLATINO)
            {
                return NivelFidelizacion.Platino;
            }
            else if (cantidadVisitas >= UMBRAL_ORO)
            {
                return NivelFidelizacion.Oro;
            }
            else if (cantidadVisitas >= UMBRAL_PLATA)
            {
                return NivelFidelizacion.Plata;
            }
            else
            {
                return NivelFidelizacion.Basico;
            }
        }
        
        /// <summary>
        /// Determina el segmento al que pertenece un cliente según su comportamiento de consumo
        /// </summary>
        /// <param name="cliente">Cliente a analizar</param>
        /// <returns>Segmento al que pertenece el cliente</returns>
        private SegmentoCliente DeterminarSegmentoCliente(Cliente cliente)
        {
            // Si el cliente no tiene datos suficientes para segmentar
            if (cliente.CantidadVisitas == 0)
                return SegmentoCliente.SinClasificar;
                
            // Simulación: en implementación real estos datos vendrían de repositorios
            // Esto sería reemplazado por consultas a la base de datos real
            var visitasUltimos60Dias = SimularVisitasRecientes(cliente, 60);
            var visitasPeriodoAnterior = SimularVisitasRecientes(cliente, 120, 60);
            var gastoPromedio = SimularGastoPromedio(cliente);
            var diasDesdeUltimaVisita = SimularDiasDesdeUltimaVisita(cliente);
            
            // Verificar criterios por prioridad
            
            // 1. Cliente inactivo
            if (diasDesdeUltimaVisita >= DIAS_INACTIVIDAD)
                return SegmentoCliente.Inactivo;
                
            // 2. Cliente Premium - alta frecuencia y alto ticket
            if (visitasUltimos60Dias >= UMBRAL_FRECUENCIA_ALTA && gastoPromedio >= UMBRAL_TICKET_ALTO)
                return SegmentoCliente.Premium;
                
            // 3. Detectar crecimiento o decrecimiento
            var variacionVisitas = visitasUltimos60Dias - visitasPeriodoAnterior;
            
            if (variacionVisitas >= UMBRAL_CLIENTE_CRECIENTE)
                return SegmentoCliente.Creciente;
                
            if (variacionVisitas <= UMBRAL_CLIENTE_DECRECIENTE)
                return SegmentoCliente.Decreciente;
                
            // 4. Alta frecuencia o alto ticket
            if (visitasUltimos60Dias >= UMBRAL_FRECUENCIA_ALTA)
                return SegmentoCliente.FrecuenciaAlta;
                
            if (gastoPromedio >= UMBRAL_TICKET_ALTO)
                return SegmentoCliente.TicketAlto;
                
            // Cliente sin clasificación específica
            return SegmentoCliente.SinClasificar;
        }
        
        // Métodos de simulación - en implementación real se reemplazarían por consultas a repositorios
        
        private int SimularVisitasRecientes(Cliente cliente, int dias, int diasAntes = 0)
        {
            // En implementación real: consulta a repositorio de comandas
            // Aquí simulamos basado en puntos y segmento del cliente
            
            // Obtener nivel de fidelización del cliente a través del repositorio
            var tarjeta = _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, CancellationToken.None).Result;
            var nivel = tarjeta?.NivelFidelizacion ?? NivelFidelizacion.Basico;
            
            // Fórmula simulada: visitas proporcionales a nivel y puntos
            var factorNivel = nivel switch
            {
                NivelFidelizacion.Platino => 0.8,
                NivelFidelizacion.Oro => 0.6,
                NivelFidelizacion.Plata => 0.4,
                _ => 0.2
            };
            
            // Simulación simple: más días = más visitas, más puntos = más visitas
            var basePuntos = Math.Min(cliente.PuntosAcumulados / 50, 20);
            
            // Aplicar factor de decaimiento por días anteriores
            var factorDecaimiento = diasAntes > 0 ? 0.5 : 1.0;
            
            return (int)(basePuntos * factorNivel * factorDecaimiento * dias / 30);
        }
        
        private decimal SimularGastoPromedio(Cliente cliente)
        {
            // En implementación real: consulta a repositorio de comandas
            // Aquí simulamos basado en nivel de fidelización
            
            // Obtener nivel de fidelización del cliente a través del repositorio
            var tarjeta = _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, CancellationToken.None).Result;
            var nivel = tarjeta?.NivelFidelizacion ?? NivelFidelizacion.Basico;
            
            return nivel switch
            {
                NivelFidelizacion.Platino => 75.0m,
                NivelFidelizacion.Oro => 50.0m,
                NivelFidelizacion.Plata => 30.0m,
                _ => 20.0m
            };
        }
        
        private int SimularDiasDesdeUltimaVisita(Cliente cliente)
        {
            // En implementación real: consulta a repositorio de comandas
            // Aquí simulamos inverso al nivel: niveles altos = visita reciente
            
            // Obtener nivel de fidelización del cliente a través del repositorio
            var tarjeta = _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, CancellationToken.None).Result;
            var nivel = tarjeta?.NivelFidelizacion ?? NivelFidelizacion.Basico;
            
            return nivel switch
            {
                NivelFidelizacion.Platino => 7,
                NivelFidelizacion.Oro => 14,
                NivelFidelizacion.Plata => 30,
                _ => 60
            };
        }
        
        /// <inheritdoc />
        public Task<ResultadoClientesFrecuentesPolicy> EjecutarAsync(int diasHistorial = 90, CancellationToken cancellationToken = default)
        {
            // Este método es simplemente un alias de EjecutarPolicy
            // pero podría usar el parámetro diasHistorial para personalizar la consulta en el futuro
            return EjecutarPolicy(cancellationToken);
        }

        /// <summary>
        /// Determina el segmento de un cliente basado en su historial de compras
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Resultado con el segmento del cliente</returns>
        public async Task<Result<SegmentoCliente>> DeterminarSegmentoClienteAsync(Guid clienteId)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío");
            
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<SegmentoCliente>(SegmentoCliente.Inactivo);
            
            // Obtener el cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId);
            if (cliente == null)
                return Result.Failure<SegmentoCliente>("No se encontró el cliente especificado");
            
            // Obtener historial de compras
            var comprasRecientes = await _clienteRepository.ObtenerFacturasRecientesAsync(
                clienteId, 
                _dateTimeService.Now.AddMonths(-6), 
                _dateTimeService.Now);
            
            // Si no hay compras recientes, el cliente está inactivo
            if (comprasRecientes == null || !comprasRecientes.Any())
                return Result.Success(SegmentoCliente.Inactivo);
            
            // Calcular métricas del cliente
            var frecuenciaCompra = comprasRecientes.Count();
            var ticketPromedio = comprasRecientes.Average(c => c.Total);
            var tendencia = CalcularTendenciaCompra(comprasRecientes);
            
            // Determinar segmento basado en las métricas
            var segmento = DeterminarSegmento(frecuenciaCompra, ticketPromedio, tendencia);
            
            // Actualizar segmento del cliente si ha cambiado
            if (cliente.Segmento != segmento)
            {
                var segmentoAnterior = cliente.Segmento;
                cliente.ActualizarSegmento(segmento);
                await _clienteRepository.GuardarAsync(cliente);
            }
            
            return Result.Success(segmento);
        }
        
        /// <summary>
        /// Genera recomendaciones para un cliente basadas en su segmento
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Resultado con las recomendaciones para el cliente</returns>
        public async Task<Result<List<Recomendacion>>> GenerarRecomendacionesAsync(Guid clienteId)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(clienteId != Guid.Empty, "El ID del cliente no puede estar vacío");
            
            if (_notificationManager.HasErrors)
                return _notificationManager.ToResult<List<Recomendacion>>(new List<Recomendacion>());
            
            // Obtener el segmento del cliente
            var segmentoResult = await DeterminarSegmentoClienteAsync(clienteId);
            if (!segmentoResult.Succeeded)
                return Result.Failure<List<Recomendacion>>(segmentoResult.Error ?? "Error al determinar el segmento del cliente");
            
            var segmento = segmentoResult.Value;
            
            // Generar recomendaciones según el segmento
            var recomendaciones = new List<Recomendacion>();
            
            switch (segmento)
            {
                case SegmentoCliente.Premium:
                    recomendaciones.Add(new Recomendacion("Invitación a eventos VIP", TipoRecomendacion.Evento));
                    recomendaciones.Add(new Recomendacion("Descuento en productos premium", TipoRecomendacion.Descuento));
                    break;
                    
                case SegmentoCliente.FrecuenciaAlta:
                    recomendaciones.Add(new Recomendacion("Programa de fidelización especial", TipoRecomendacion.Programa));
                    recomendaciones.Add(new Recomendacion("Promoción 2x1 en productos seleccionados", TipoRecomendacion.Promocion));
                    break;
                    
                case SegmentoCliente.TicketAlto:
                    recomendaciones.Add(new Recomendacion("Descuento por volumen de compra", TipoRecomendacion.Descuento));
                    recomendaciones.Add(new Recomendacion("Productos exclusivos", TipoRecomendacion.Producto));
                    break;
                    
                case SegmentoCliente.Creciente:
                    recomendaciones.Add(new Recomendacion("Promoción para aumentar frecuencia", TipoRecomendacion.Promocion));
                    recomendaciones.Add(new Recomendacion("Cupón de descuento para próxima compra", TipoRecomendacion.Cupon));
                    break;
                    
                case SegmentoCliente.Decreciente:
                    recomendaciones.Add(new Recomendacion("Encuesta de satisfacción", TipoRecomendacion.Encuesta));
                    recomendaciones.Add(new Recomendacion("Promoción para reactivación", TipoRecomendacion.Promocion));
                    break;
                    
                case SegmentoCliente.Inactivo:
                    recomendaciones.Add(new Recomendacion("Campaña de reactivación", TipoRecomendacion.Campana));
                    recomendaciones.Add(new Recomendacion("Oferta especial de bienvenida", TipoRecomendacion.Oferta));
                    break;
                    
                default:
                    recomendaciones.Add(new Recomendacion("Promoción general", TipoRecomendacion.Promocion));
                    break;
            }
            
            return Result.Success(recomendaciones);
        }
        
        // Métodos privados de apoyo
        
        private decimal CalcularTendenciaCompra(IEnumerable<Factura> compras)
        {
            // Ordenar compras por fecha
            var comprasOrdenadas = compras.OrderBy(c => c.FechaEmision).ToList();
            
            // Si hay menos de 2 compras, no hay tendencia
            if (comprasOrdenadas.Count < 2)
                return 0;
            
            // Dividir las compras en dos mitades y comparar promedios
            var mitad = comprasOrdenadas.Count / 2;
            var primerasMitad = comprasOrdenadas.Take(mitad);
            var segundasMitad = comprasOrdenadas.Skip(mitad);
            
            var promedioAnterior = primerasMitad.Average(c => c.Total);
            var promedioReciente = segundasMitad.Average(c => c.Total);
            
            // Calcular tendencia como porcentaje de cambio
            if (promedioAnterior > 0)
                return (promedioReciente - promedioAnterior) / promedioAnterior;
            
            return 0;
        }
        
        private SegmentoCliente DeterminarSegmento(int frecuencia, decimal ticketPromedio, decimal tendencia)
        {
            // Cliente Premium: alta frecuencia y alto ticket
            if (frecuencia >= 10 && ticketPromedio >= 100000)
                return SegmentoCliente.Premium;
            
            // Cliente de alta frecuencia
            if (frecuencia >= 10)
                return SegmentoCliente.FrecuenciaAlta;
            
            // Cliente de ticket alto
            if (ticketPromedio >= 100000)
                return SegmentoCliente.TicketAlto;
            
            // Cliente con tendencia creciente
            if (tendencia >= 0.2m)
                return SegmentoCliente.Creciente;
            
            // Cliente con tendencia decreciente
            if (tendencia <= -0.2m)
                return SegmentoCliente.Decreciente;
            
            // Cliente inactivo (poca frecuencia)
            if (frecuencia <= 2)
                return SegmentoCliente.Inactivo;
            
            // Cliente regular
            return SegmentoCliente.Regular;
        }
    }

    /// <summary>
    /// Representa una recomendación para un cliente
    /// </summary>
    public class Recomendacion
    {
        public string Descripcion { get; }
        public TipoRecomendacion Tipo { get; }
        
        public Recomendacion(string descripcion, TipoRecomendacion tipo)
        {
            Descripcion = descripcion;
            Tipo = tipo;
        }
    }

    /// <summary>
    /// Tipos de recomendaciones para clientes
    /// </summary>
    public enum TipoRecomendacion
    {
        Promocion,
        Descuento,
        Producto,
        Evento,
        Programa,
        Cupon,
        Encuesta,
        Campana,
        Oferta
    }
} 