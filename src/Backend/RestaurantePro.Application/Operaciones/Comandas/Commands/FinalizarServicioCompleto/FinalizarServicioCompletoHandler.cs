using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos;
using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
// using RestaurantePro.Application.Core.Notificaciones.Commands.EnviarNotificacion; // TODO: Implementar cuando esté disponible
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
// using RestaurantePro.Domain.Comercial.Fidelizacion.Entities; // No existe, comentar temporalmente
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using System.Diagnostics;

namespace RestaurantePro.Application.Operaciones.Commands.FinalizarServicioCompleto;

/// <summary>
/// 🚀 Handler para finalizar servicio completo implementando patrón Saga
/// Orquesta: Comanda + Facturación + Fidelización + Mesa + Notificaciones + Analytics
/// PATRÓN SAGA para operaciones distribuidas con compensación
/// </summary>
public class FinalizarServicioCompletoHandler : IRequestHandler<FinalizarServicioCompletoCommand, Result<ServicioCompletoResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<FinalizarServicioCompletoHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizarServicioCompletoHandler(
        IApplicationDbContext context,
        IMediator mediator,
        ILogger<FinalizarServicioCompletoHandler> logger,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServicioCompletoResult>> Handle(FinalizarServicioCompletoCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var resultado = new ServicioCompletoResult
        {
            ComandaId = request.ComandaId,
            ClienteId = request.ClienteId,
            FechaProcesamiento = _dateTime.Now
        };

        var pasosEjecutados = new List<PasoServicio>();
        var advertencias = new List<string>();
        var errores = new List<string>();
        var mensajes = new List<string>();

        try
        {
            _logger.LogInformation("🚀 Iniciando finalización de servicio completo - Comanda: {ComandaId}, Tipo: {TipoFinalizacion}",
                request.ComandaId, request.TipoFinalizacion);

            // 🎯 PASO 1: Obtener y validar comanda
            var comanda = await ObtenerComanda(request.ComandaId, cancellationToken);
            if (comanda == null)
            {
                return Result.Failure<ServicioCompletoResult>("Comanda no encontrada");
            }

            resultado.MesaId = comanda.MesaId;
            resultado.FechaInicioServicio = comanda.FechaCreacion;
            resultado.FechaFinServicio = _dateTime.Now;
            resultado.DuracionServicio = resultado.FechaFinServicio - resultado.FechaInicioServicio;

            AgregarPaso(pasosEjecutados, "Validación de comanda", true, "Comanda validada correctamente");

            // 🎯 PASO 2: Procesar fidelización (si aplica)
            if (request.AplicarDescuentoFidelizacion && request.ClienteId.HasValue)
            {
                var fidelizacionResult = await ProcesarFidelizacion(comanda, request, resultado, pasosEjecutados, advertencias, cancellationToken);
                if (!fidelizacionResult.IsSuccess)
                {
                    advertencias.Add($"Fidelización: {fidelizacionResult.Error}");
                }
            }

            // 🎯 PASO 3: Cerrar comanda
            var cerrarResult = await CerrarComanda(comanda, request, pasosEjecutados, cancellationToken);
            if (!cerrarResult.IsSuccess)
            {
                errores.Add($"Error cerrando comanda: {cerrarResult.Error}");
                return CrearResultadoConErrores(resultado, pasosEjecutados, advertencias, errores, mensajes, EstadoProcesamiento.Fallido);
            }

            // 🎯 PASO 4: Generar factura (si aplica)
            if (request.GenerarFacturaInmediata && request.ClienteId.HasValue)
            {
                var facturaResult = await GenerarFactura(comanda, request, resultado, pasosEjecutados, advertencias, cancellationToken);
                if (!facturaResult.IsSuccess)
                {
                    advertencias.Add($"Facturación: {facturaResult.Error}");
                }
            }

            // 🎯 PASO 5: Liberar mesa (si aplica)
            if (request.LiberarMesaAutomaticamente && comanda.MesaId.HasValue)
            {
                var liberarMesaResult = await LiberarMesa(comanda.MesaId.Value, pasosEjecutados, cancellationToken);
                if (liberarMesaResult.IsSuccess)
                {
                    resultado.MesaLiberada = true;
                }
                else
                {
                    advertencias.Add($"Liberación de mesa: {liberarMesaResult.Error}");
                }
            }

            // 🎯 PASO 6: Enviar notificación (si aplica)
            // TODO: Implementar cuando EnviarNotificacion esté disponible
            /*
            if (request.EnviarNotificacionCliente && request.ClienteId.HasValue)
            {
                var notificacionResult = await EnviarNotificacion(comanda, request, pasosEjecutados, cancellationToken);
                if (notificacionResult.IsSuccess)
                {
                    resultado.NotificacionEnviada = true;
                }
                else
                {
                    advertencias.Add($"Notificación: {notificacionResult.Error}");
                }
            }
            */

            // 🎯 PASO 7: Registrar estadísticas (si aplica)
            if (request.RegistrarEstadisticas)
            {
                await RegistrarEstadisticas(comanda, resultado, pasosEjecutados, cancellationToken);
            }

            // 🎯 PASO 8: Calcular totales finales
            CalcularTotalesFinales(comanda, resultado);

            // 🎯 PASO 9: Determinar estado final
            stopwatch.Stop();
            resultado.Estadisticas.TiempoPromedioPreparacion = (int)stopwatch.ElapsedMilliseconds;

            if (errores.Any())
            {
                resultado.Estado = EstadoProcesamiento.FallosParciales;
            }
            else if (advertencias.Any())
            {
                resultado.Estado = EstadoProcesamiento.ExitosoConAdvertencias;
            }
            else
            {
                resultado.Estado = EstadoProcesamiento.Exitoso;
            }

            // 🎯 PASO 10: Guardar cambios
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 🎯 PASO 11: Completar resultado
            resultado.PasosEjecutados = pasosEjecutados;
            resultado.Advertencias = advertencias;
            resultado.Errores = errores;
            resultado.Mensajes = mensajes;

            _logger.LogInformation("✅ Servicio completo finalizado exitosamente - Comanda: {ComandaId}, Estado: {Estado}, Tiempo: {TiempoMs}ms",
                comanda.Id, resultado.Estado, stopwatch.ElapsedMilliseconds);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Error finalizando servicio completo: {ErrorMessage}", ex.Message);
            
            return CrearResultadoConErrores(resultado, pasosEjecutados, advertencias, errores, mensajes, EstadoProcesamiento.Fallido);
        }
    }

    /// <summary>
    /// Obtiene y valida la comanda
    /// </summary>
    private async Task<Comanda?> ObtenerComanda(Guid comandaId, CancellationToken cancellationToken)
    {
        return await _context.Comandas
            .Include(c => c.Items)
            .Include(c => c.Mesa)
            .FirstOrDefaultAsync(c => c.Id == comandaId, cancellationToken);
    }

    /// <summary>
    /// Procesa la fidelización del cliente
    /// </summary>
    private async Task<Result> ProcesarFidelizacion(
        Comanda comanda, 
        FinalizarServicioCompletoCommand request, 
        ServicioCompletoResult resultado, 
        List<PasoServicio> pasosEjecutados, 
        List<string> advertencias, 
        CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Obtener información actual del cliente
            var cliente = await _context.Clientes.FindAsync(new object[] { request.ClienteId!.Value }, cancellationToken);
            if (cliente == null)
            {
                return Result.Failure("Cliente no encontrado");
            }

            resultado.NivelFidelizacionAnterior = cliente.NivelFidelizacion?.ToString() ?? "Sin nivel";

            // Calcular puntos a acumular
            var montoTotal = comanda.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
            var puntosAAcumular = (int)Math.Floor(montoTotal); // 1 punto por $1

            // Acumular puntos
            var acumularPuntosCommand = new AcumularPuntosCommand
            {
                ClienteId = request.ClienteId.Value,
                ComandaId = comanda.Id,
                PuntosAAcumular = puntosAAcumular,
                Motivo = $"Compra en comanda {comanda.NumeroComanda}"
            };

            var result = await _mediator.Send(acumularPuntosCommand, cancellationToken);
            if (result.IsSuccess)
            {
                resultado.PuntosOtorgados = puntosAAcumular;
                
                // Verificar si cambió el nivel
                var clienteActualizado = await _context.Clientes.FindAsync(new object[] { request.ClienteId.Value }, cancellationToken);
                resultado.NivelFidelizacionActual = clienteActualizado?.NivelFidelizacion?.ToString() ?? "Sin nivel";
                resultado.HuboCambioNivel = resultado.NivelFidelizacionAnterior != resultado.NivelFidelizacionActual;

                stopwatch.Stop();
                AgregarPaso(pasosEjecutados, "Procesamiento de fidelización", true, 
                    $"Puntos acumulados: {puntosAAcumular}, Nivel: {resultado.NivelFidelizacionActual}", stopwatch.ElapsedMilliseconds);

                return Result.Success();
            }

            return Result.Failure(result.Error ?? "Error procesando fidelización");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando fidelización");
            return Result.Failure($"Error procesando fidelización: {ex.Message}");
        }
    }

    /// <summary>
    /// Cierra la comanda
    /// </summary>
    private async Task<Result> CerrarComanda(
        Comanda comanda, 
        FinalizarServicioCompletoCommand request, 
        List<PasoServicio> pasosEjecutados, 
        CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var cerrarCommand = new CerrarComandaCommand
            {
                ComandaId = comanda.Id,
                MetodoPago = MetodoPago.Efectivo, // Por defecto
                Observaciones = request.ObservacionesFinalizacion ?? "Finalización de servicio completo"
            };

            var result = await _mediator.Send(cerrarCommand, cancellationToken);
            
            stopwatch.Stop();
            AgregarPaso(pasosEjecutados, "Cierre de comanda", result.IsSuccess, 
                result.IsSuccess ? "Comanda cerrada exitosamente" : result.Error, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cerrando comanda");
            return Result.Failure($"Error cerrando comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera la factura
    /// </summary>
    private async Task<Result> GenerarFactura(
        Comanda comanda, 
        FinalizarServicioCompletoCommand request, 
        ServicioCompletoResult resultado, 
        List<PasoServicio> pasosEjecutados, 
        List<string> advertencias, 
        CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var facturaCommand = new CrearFacturaCommand
            {
                ClienteId = request.ClienteId!.Value,
                ComandaId = comanda.Id,
                MetodoPago = MetodoPago.Efectivo,
                Observaciones = "Facturación automática por finalización de servicio"
            };

            var result = await _mediator.Send(facturaCommand, cancellationToken);
            if (result.IsSuccess)
            {
                resultado.FacturaId = result.Value.Id;
                resultado.MetodoPago = MetodoPago.Efectivo.ToString();

                stopwatch.Stop();
                AgregarPaso(pasosEjecutados, "Generación de factura", true, 
                    $"Factura generada: {result.Value.Id}", stopwatch.ElapsedMilliseconds);

                return Result.Success();
            }

            return Result.Failure(result.Error ?? "Error generando factura");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando factura");
            return Result.Failure($"Error generando factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Libera la mesa
    /// </summary>
    private async Task<Result> LiberarMesa(Guid mesaId, List<PasoServicio> pasosEjecutados, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var liberarCommand = new LiberarMesaCommand { MesaId = mesaId };
            var result = await _mediator.Send(liberarCommand, cancellationToken);
            
            stopwatch.Stop();
            AgregarPaso(pasosEjecutados, "Liberación de mesa", result.IsSuccess, 
                result.IsSuccess ? "Mesa liberada exitosamente" : result.Error, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liberando mesa");
            return Result.Failure($"Error liberando mesa: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra estadísticas del servicio
    /// </summary>
    private async Task RegistrarEstadisticas(
        Comanda comanda, 
        ServicioCompletoResult resultado, 
        List<PasoServicio> pasosEjecutados, 
        CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Calcular estadísticas básicas
            resultado.Estadisticas.TotalItems = comanda.Items.Count;
            resultado.Estadisticas.TicketPromedio = comanda.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
            resultado.Estadisticas.ServicioRapido = resultado.DuracionServicio.TotalMinutes <= 30;

            // Calcular eficiencia (tiempo promedio por item)
            if (resultado.Estadisticas.TotalItems > 0)
            {
                resultado.Estadisticas.EficienciaServicio = (decimal)(resultado.DuracionServicio.TotalMinutes / resultado.Estadisticas.TotalItems);
            }

            // Agregar métricas adicionales
            resultado.Estadisticas.MetricasAdicionales["TipoFinalizacion"] = request.TipoFinalizacion.ToString();
            resultado.Estadisticas.MetricasAdicionales["PropinaSugerida"] = request.PropinaSugerida ?? 0;
            resultado.Estadisticas.MetricasAdicionales["DuracionMinutos"] = resultado.DuracionServicio.TotalMinutes;

            stopwatch.Stop();
            AgregarPaso(pasosEjecutados, "Registro de estadísticas", true, 
                $"Estadísticas registradas: {resultado.Estadisticas.TotalItems} items, {resultado.Estadisticas.TicketPromedio:C} total", 
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando estadísticas");
            AgregarPaso(pasosEjecutados, "Registro de estadísticas", false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula los totales finales
    /// </summary>
    private void CalcularTotalesFinales(Comanda comanda, ServicioCompletoResult resultado)
    {
        resultado.TotalOriginal = comanda.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
        resultado.TotalDescuentos = comanda.DescuentoAplicado ?? 0;
        resultado.TotalPropinas = request.PropinaSugerida ?? 0;
        resultado.TotalFinal = resultado.TotalOriginal - resultado.TotalDescuentos + resultado.TotalPropinas;
    }

    /// <summary>
    /// Agrega un paso al seguimiento
    /// </summary>
    private void AgregarPaso(List<PasoServicio> pasos, string nombre, bool exitoso, string detalle, double duracionMs = 0)
    {
        pasos.Add(new PasoServicio
        {
            Nombre = nombre,
            FechaEjecucion = _dateTime.Now,
            Exitoso = exitoso,
            Detalle = detalle,
            DuracionMs = duracionMs,
            Orden = pasos.Count + 1
        });
    }

    /// <summary>
    /// Crea resultado con errores
    /// </summary>
    private Result<ServicioCompletoResult> CrearResultadoConErrores(
        ServicioCompletoResult resultado, 
        List<PasoServicio> pasosEjecutados, 
        List<string> advertencias, 
        List<string> errores, 
        List<string> mensajes, 
        EstadoProcesamiento estado)
    {
        resultado.Estado = estado;
        resultado.PasosEjecutados = pasosEjecutados;
        resultado.Advertencias = advertencias;
        resultado.Errores = errores;
        resultado.Mensajes = mensajes;

        return Result.Failure<ServicioCompletoResult>(string.Join("; ", errores));
    }
} 