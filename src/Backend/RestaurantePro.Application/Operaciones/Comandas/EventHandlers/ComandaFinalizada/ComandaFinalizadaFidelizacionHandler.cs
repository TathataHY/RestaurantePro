namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

/// <summary>
/// 🎯 Handler que procesa el evento ComandaFinalizada para acumular puntos de fidelización
/// </summary>
public class ComandaFinalizadaFidelizacionHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IServicioFidelizacion _servicioFidelizacion;
    private readonly ILogger<ComandaFinalizadaFidelizacionHandler> _logger;
    private readonly IMediator _mediator;

    public ComandaFinalizadaFidelizacionHandler(
        IComandaRepository comandaRepository,
        IClienteRepository clienteRepository,
        IServicioFidelizacion servicioFidelizacion,
        ILogger<ComandaFinalizadaFidelizacionHandler> logger,
        IMediator mediator)
    {
        _comandaRepository = comandaRepository;
        _clienteRepository = clienteRepository;
        _servicioFidelizacion = servicioFidelizacion;
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// 🚀 Procesa la finalización de comanda para acumular puntos de fidelización
    /// </summary>
    public async Task Handle(Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🎯 Iniciando procesamiento de fidelización para Comanda {ComandaId} con total {Total:C}", 
            evento.ComandaId, evento.Total);

        try
        {
            // 🔄 Obtener la comanda completa
            var comandaResult = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comandaResult == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada para procesamiento de fidelización: {ComandaId}", evento.ComandaId);
                return;
            }

            var comanda = comandaResult;

            // 2. Verificar si la comanda tiene cliente asociado
            if (comanda.ClienteId == null)
            {
                _logger.LogInformation("ℹ️ Comanda {ComandaId} no tiene cliente asociado, omitiendo fidelización", evento.ComandaId);
                return;
            }

            // 3. Obtener información del cliente
            var clienteResult = await _clienteRepository.ObtenerPorIdAsync(comanda.ClienteId.Value, cancellationToken);
            if (clienteResult == null)
            {
                _logger.LogWarning("⚠️ No se encontró el cliente {ClienteId} para procesar fidelización", comanda.ClienteId);
                return;
            }

            var cliente = clienteResult;

            // 📊 Calcular puntos según el nivel del cliente
            var nivelCliente = NivelFidelizacion.Basico; // Valor por defecto
            var factorMultiplicador = ObtenerFactorMultiplicador(nivelCliente);
            
            var puntosCalculados = CalcularPuntosComanda(evento, factorMultiplicador);

            // 🎯 Aplicar puntos de fidelización usando el servicio de dominio
            var resultadoAplicacion = await _servicioFidelizacion.AcumularPuntosAsync(
                cliente.Id, evento.ComandaId, evento.Total);

            if (resultadoAplicacion != null && resultadoAplicacion.Value > 0)
            {
                _logger.LogInformation("✅ Se acumularon {Puntos} puntos al cliente {ClienteNombre} por Comanda {ComandaId}", 
                    puntosCalculados, cliente.Nombre.NombreCompleto, evento.ComandaId);

                // 🔄 Verificar si hubo cambio de nivel
                var cambioNivel = await VerificarCambioNivelCliente(cliente.Id, cancellationToken);

                // 📊 Registrar estadísticas de fidelización
                await RegistrarEstadisticasFidelizacion(evento, cliente, puntosCalculados, cambioNivel, cancellationToken);

                // 📢 Notificar al cliente sobre puntos y nivel (si cambió)
                await NotificarClienteFidelizacion(cliente, puntosCalculados, cambioNivel, evento, cancellationToken);

                var nombreCliente = cliente.Nombre.NombreCompleto;
                var nivelTexto = nivelCliente.ToString();

                _logger.LogInformation("✅ Procesamiento de fidelización completo - Cliente: {ClienteNombre}, Puntos: {Puntos}, Nivel: {Nivel}",
                    nombreCliente, puntosCalculados, nivelTexto);
            }
            else
            {
                _logger.LogError("💥 Error al acumular puntos para cliente {ClienteId}: {Error}", 
                    cliente.Id, resultadoAplicacion.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al procesar fidelización para Comanda {ComandaId}", evento.ComandaId);
            throw;
        }
    }

    /// <summary>
    /// 🔢 Calcula puntos según el total de la comanda y nivel del cliente
    /// </summary>
    private int CalcularPuntosComanda(Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento, decimal factorMultiplicador)
    {
        // Regla base: 1 punto por cada $10 pesos
        var puntosBase = (int)(evento.Total / 10);

        var puntosCalculados = (int)(puntosBase * factorMultiplicador);

        _logger.LogInformation("🔢 Cálculo puntos: Base={PuntosBase} x Multiplicador={Multiplicador} = {PuntosCalculados} para comanda {ComandaId}", 
            puntosBase, factorMultiplicador, puntosCalculados, evento.ComandaId);

        return puntosCalculados;
    }

    /// <summary>
    /// 🔢 Calcula el factor multiplicador según el nivel del cliente
    /// </summary>
    private decimal ObtenerFactorMultiplicador(NivelFidelizacion nivelCliente)
    {
        return nivelCliente switch
        {
            NivelFidelizacion.Basico => 1.0m,
            NivelFidelizacion.Plata => 1.2m,
            NivelFidelizacion.Oro => 1.5m,
            NivelFidelizacion.Platino => 2.0m,
            _ => 1.0m
        };
    }

    /// <summary>
    /// 🔄 Verifica si el cliente cambió de nivel de fidelización
    /// </summary>
    private async Task<bool> VerificarCambioNivelCliente(Guid clienteId, CancellationToken cancellationToken)
    {
        // TODO: Implementar verificación real de cambio de nivel cuando esté disponible
        // var nivelAnterior = cliente.NivelFidelizacion;
        // var nivelActual = _fidelizacionService.CalcularNivelPorPuntos(cliente.PuntosAcumulados);
        // return nivelAnterior != nivelActual;
        
        return false; // Simplificado para compilación
    }

    /// <summary>
    /// 📊 Registra estadísticas de fidelización para analytics
    /// </summary>
    private async Task RegistrarEstadisticasFidelizacion(
        Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento,
        Cliente cliente,
        int puntosAcumulados,
        bool cambioNivel,
        CancellationToken cancellationToken)
    {
        var estadisticas = new
        {
            FechaHora = DateTime.UtcNow,
            ClienteId = cliente.Id,
            ComandaId = evento.ComandaId,
            PuntosAcumulados = puntosAcumulados,
            NivelFidelizacion = NivelFidelizacion.Basico, // Valor por defecto
            HuboCambioNivel = cambioNivel,
            TotalCompra = evento.Total
        };

        _logger.LogInformation("📊 Estadísticas de fidelización registradas: {@EstadisticasFidelizacion}", estadisticas);
        
        // TODO: Enviar a sistema de analytics
    }

    /// <summary>
    /// 📢 Notifica al cliente sobre puntos y nivel (si cambió)
    /// </summary>
    private async Task NotificarClienteFidelizacion(
        Cliente cliente,
        int puntosCalculados,
        bool cambioNivel,
        Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento,
        CancellationToken cancellationToken)
    {
        var nombreCliente = cliente.Nombre.NombreCompleto;
        var nivelActual = NivelFidelizacion.Basico; // Valor por defecto

        var mensaje = cambioNivel 
            ? $"¡Felicitaciones {nombreCliente}! Has ganado {puntosCalculados} puntos y subiste al nivel {nivelActual}"
            : $"¡Gracias {nombreCliente}! Has ganado {puntosCalculados} puntos de fidelización";

        _logger.LogInformation("📢 Notificación de fidelización: {Mensaje}", mensaje);
        
        // TODO: Enviar notificación real al cliente
    }
} 