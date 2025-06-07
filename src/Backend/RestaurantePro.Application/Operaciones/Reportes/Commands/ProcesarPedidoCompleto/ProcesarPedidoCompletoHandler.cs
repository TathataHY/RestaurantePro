using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;
using RestaurantePro.Application.Comercial.Facturacion.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

/// <summary>
/// Handler para procesar pedidos completos desde comanda hasta facturación
/// Orquesta todo el workflow de procesamiento de pedidos
/// </summary>
public class ProcesarPedidoCompletoHandler : IRequestHandler<ProcesarPedidoCompletoCommand, Result<ProcesarPedidoCompletoDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IFacturaRepository _facturaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcesarPedidoCompletoHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMediator _mediator;
    private readonly IServicioFacturacion _servicioFacturacion;
    private readonly IComercialServiceFacade _comercialServiceFacade;
    private readonly IFacturacionService _facturacionService;
    private readonly IFidelizacionService _fidelizacionService;
    private readonly IMesaService _mesaService;

    public ProcesarPedidoCompletoHandler(
        IComandaRepository comandaRepository,
        IFacturaRepository facturaRepository,
        IClienteRepository clienteRepository,
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ProcesarPedidoCompletoHandler> logger,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService,
        IMediator mediator,
        IServicioFacturacion servicioFacturacion,
        IComercialServiceFacade comercialServiceFacade,
        IFacturacionService facturacionService,
        IFidelizacionService fidelizacionService,
        IMesaService mesaService)
    {
        _comandaRepository = comandaRepository;
        _facturaRepository = facturaRepository;
        _clienteRepository = clienteRepository;
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _dateTimeService = dateTimeService;
        _mediator = mediator;
        _servicioFacturacion = servicioFacturacion;
        _comercialServiceFacade = comercialServiceFacade;
        _facturacionService = facturacionService;
        _fidelizacionService = fidelizacionService;
        _mesaService = mesaService;
    }

    public async Task<Result<ProcesarPedidoCompletoDto>> Handle(ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Iniciando procesamiento completo de pedido - ID Comanda: {ComandaId}", request.ComandaId);
            
            // 1. Obtener comanda existente
            var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, cancellationToken);
            if (comanda == null)
            {
                _logger.LogWarning("❌ No se encontró la comanda con ID: {ComandaId}", request.ComandaId);
                return Result.Failure<ProcesarPedidoCompletoDto>($"No se encontró la comanda con ID: {request.ComandaId}");
            }
            
            // 2. Validar que la comanda esté en estado válido para procesamiento
            if (comanda.Estado != Domain.Operaciones.Comandas.Enums.EstadoComanda.Lista)
            {
                _logger.LogWarning("⚠️ La comanda {ComandaId} no está en estado Lista para procesar. Estado actual: {Estado}", 
                    request.ComandaId, comanda.Estado);
                return Result.Failure<ProcesarPedidoCompletoDto>($"La comanda no está en estado Lista para procesar. Estado actual: {comanda.Estado}");
            }
            
            // 3. Procesar entrega a domicilio si corresponde
            var resultadoEntrega = await ProcesarEntregaDomicilio(comanda, request, cancellationToken);
            if (!resultadoEntrega.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>($"Error al procesar entrega a domicilio: {resultadoEntrega.Error}");
            }
            
            // 4. Procesar pago
            var resultadoPago = await ProcesarPago(comanda, request, cancellationToken);
            if (!resultadoPago.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>($"Error al procesar pago: {resultadoPago.Error}");
            }
            
            // 5. Generar factura
            var resultadoFactura = await GenerarFactura(comanda, request, cancellationToken);
            if (!resultadoFactura.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>($"Error al generar factura: {resultadoFactura.Error}");
            }
            
            // 6. Procesar fidelización (acumular puntos)
            var resultadoFidelizacion = await ProcesarFidelizacion(comanda, resultadoFactura.Value, request, cancellationToken);
            if (!resultadoFidelizacion.Succeeded)
            {
                _logger.LogWarning("⚠️ Error al procesar fidelización: {Error}", resultadoFidelizacion.Error);
                // Continuamos con el proceso a pesar del error en fidelización
            }
            
            // 7. Entregar pedido al cliente
            var resultadoEntrega2 = await EntregarPedidoCliente(comanda, request, cancellationToken);
            if (!resultadoEntrega2.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>($"Error al entregar pedido: {resultadoEntrega2.Error}");
            }
            
            // 8. Liberar mesa si corresponde
            var resultadoMesa = await LiberarMesa(comanda, request, cancellationToken);
            if (!resultadoMesa.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>($"Error al liberar mesa: {resultadoMesa.Error}");
            }
            
            // 9. Finalizar comanda
            var resultadoFinalizar = await FinalizarComanda(comanda, cancellationToken);
            if (!resultadoFinalizar.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>($"Error al finalizar comanda: {resultadoFinalizar.Error}");
            }
            
            // Preparar resultado final
            var resultado = new ProcesarPedidoCompletoDto
            {
                ComandaId = comanda.Id,
                FacturaId = resultadoFactura.Value.Id,
                EntregaId = resultadoEntrega.Value,
                Total = resultadoFactura.Value.Total,
                PuntosAcumulados = resultadoFidelizacion.Value,
                MesaLiberada = resultadoMesa.Value != null,
                MesaId = resultadoMesa.Value?.MesaId,
                FechaHora = DateTime.Now,
                EstadoComanda = comanda.Estado.ToString()
            };
            
            _logger.LogInformation("✅ Procesamiento completo de pedido finalizado exitosamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error general al procesar pedido completo: {Message}", ex.Message);
            return Result.Failure<ProcesarPedidoCompletoDto>($"Error general al procesar pedido: {ex.Message}");
        }
    }

    private async Task<Result<Comanda>> ValidarYObtenerComanda(Guid comandaId, CancellationToken cancellationToken)
    {
        var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
        if (comanda == null)
        {
            _logger.LogWarning("⚠️ Comanda no encontrada: {ComandaId}", comandaId);
            return Result.Failure<Comanda>("La comanda especificada no existe");
        }

        if (comanda.Estado == Domain.Operaciones.Comandas.Enums.EstadoComanda.Cancelada)
        {
            _logger.LogWarning("⚠️ Intento de procesar comanda cancelada: {ComandaId}", comandaId);
            return Result.Failure<Comanda>("No se puede procesar una comanda cancelada");
        }

        if (!comanda.Items.Any())
        {
            _logger.LogWarning("⚠️ Comanda sin items: {ComandaId}", comandaId);
            return Result.Failure<Comanda>("No se puede procesar una comanda sin items");
        }

        return Result.Success(comanda);
    }

    /// <summary>
    /// Procesar el pago de la comanda
    /// </summary>
    private async Task<Result> ProcesarPago(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        if (!request.RequierePago || request.InfoPago == null)
        {
            return Result.Success();
        }

        _logger.LogInformation("💰 Iniciando procesamiento de pago para comanda {ComandaId} - Tipo: {TipoPago}", 
            comanda.Id, request.TipoPago);

        try
        {
            // Procesar pago según el tipo
            if (request.TipoPago == "Tarjeta")
            {
                return await ProcesarPagoTarjeta(comanda, request.InfoPago, cancellationToken);
            }
            else if (request.TipoPago == "Transferencia")
            {
                return await ProcesarPagoTransferencia(comanda, request.InfoPago, cancellationToken);
            }
            else
            {
                // Efectivo u otros medios que no requieren procesamiento electrónico
                _logger.LogInformation("✅ Pago en efectivo registrado para comanda {ComandaId}", comanda.Id);
                return Result.Success();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pago para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error procesando pago: {ex.Message}");
        }
    }

    private async Task<Result> ProcesarPagoTarjeta(Comanda comanda, InfoPagoDto infoPago, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("💳 Procesando pago con tarjeta para comanda {ComandaId}", comanda.Id);
            
            // Validar información de tarjeta
            if (string.IsNullOrEmpty(infoPago.NumeroTarjeta))
            {
                return Result.Failure("El número de tarjeta es requerido");
            }
            
            // Validar número de tarjeta (implementación básica)
            if (!ValidarNumeroTarjeta(infoPago.NumeroTarjeta))
            {
                return Result.Failure("El número de tarjeta no es válido");
            }
            
            // Simular procesamiento con gateway de pago
            await Task.Delay(200, cancellationToken);
            
            _logger.LogInformation("✅ Pago con tarjeta procesado correctamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pago con tarjeta para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error procesando pago con tarjeta: {ex.Message}");
        }
    }
    
    private async Task<Result> ProcesarPagoEfectivo(Comanda comanda, InfoPagoDto infoPago, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("💵 Procesando pago en efectivo para comanda {ComandaId}", comanda.Id);
            
            // Validar que el monto sea mayor a cero
            if (infoPago.MontoTotal <= 0)
            {
                return Result.Failure("El monto debe ser mayor a cero");
            }
            
            // No podemos validar MontoRecibido porque no está en el DTO
            // Podría agregarse en futuras versiones
            
            // Simular procesamiento
            await Task.Delay(100, cancellationToken);
            
            _logger.LogInformation("✅ Pago en efectivo procesado correctamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pago en efectivo para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error procesando pago en efectivo: {ex.Message}");
        }
    }
    
    private async Task<Result> ProcesarPagoDigital(Comanda comanda, InfoPagoDto infoPago, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("💻 Procesando pago digital para comanda {ComandaId}", comanda.Id);
            
            if (string.IsNullOrEmpty(infoPago.ReferenciaPago))
            {
                return Result.Failure("La referencia de pago es requerida para pagos digitales");
            }
            
            // Simular procesamiento
            await Task.Delay(150, cancellationToken);
            
            _logger.LogInformation("✅ Pago digital procesado correctamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pago digital para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error procesando pago digital: {ex.Message}");
        }
    }
    
    private async Task<Result> ProcesarPagoTransferencia(Comanda comanda, InfoPagoDto infoPago, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🏦 Procesando pago por transferencia para comanda {ComandaId}", comanda.Id);
            
            if (string.IsNullOrEmpty(infoPago.ReferenciaPago))
            {
                return Result.Failure("La referencia de transferencia es requerida para pagos por transferencia");
            }
            
            // Simular verificación con banco
            await Task.Delay(300, cancellationToken);
            
            _logger.LogInformation("✅ Pago por transferencia procesado correctamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pago por transferencia para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error procesando pago por transferencia: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Finalizar comanda cambiando su estado
    /// </summary>
    private async Task<Result> FinalizarComanda(Comanda comanda, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Finalizando comanda {ComandaId}", comanda.Id);
            
            // Cambiar estado
            comanda.ActualizarEstado(Domain.Operaciones.Comandas.Enums.EstadoComanda.Finalizada);
            
            // Guardar cambios
            await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅ Comanda {ComandaId} finalizada correctamente", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al finalizar comanda {ComandaId}", comanda.Id);
            return Result.Failure($"Error al finalizar comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Generar factura para la comanda
    /// </summary>
    private async Task<Result<Factura>> GenerarFactura(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🧾 Generando factura para comanda {ComandaId}", comanda.Id);
            
            // Validar datos de facturación si el ClienteId es un Guid vacío (pero no nulo)
            if (request.ClienteId.HasValue && request.ClienteId.Value == Guid.Empty)
            {
                return Result.Failure<Factura>("Se requiere un cliente válido para generar la factura");
            }
            
            // Asegurarse de tener la información de total más actualizada
            if (comanda.Total == null)
            {
                _logger.LogWarning("⚠️ La comanda {ComandaId} no tiene un total calculado", comanda.Id);
                return Result.Failure<Factura>("La comanda no tiene un total calculado");
            }
            
            // Generar la factura
            var resultadoFactura = await _facturacionService.GenerarFacturaAsync(
                comanda.Id,
                request.ClienteId,
                comanda.Total.Total, // Usar la propiedad Total del objeto TotalComanda
                request.Observaciones ?? string.Empty,
                cancellationToken);
            
            if (resultadoFactura.Succeeded)
            {
                _logger.LogInformation("✅ Factura generada exitosamente: {FacturaId} para comanda {ComandaId}", 
                    resultadoFactura.Value.Id, comanda.Id);
                return resultadoFactura;
            }
            else
            {
                _logger.LogWarning("❌ Error al generar factura para comanda {ComandaId}: {Error}", 
                    comanda.Id, resultadoFactura.Error);
                return resultadoFactura;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al generar factura para comanda {ComandaId}", comanda.Id);
            return Result.Failure<Factura>($"Error inesperado al generar factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesar fidelización para el cliente
    /// </summary>
    private async Task<Result<int>> ProcesarFidelizacion(Comanda comanda, Factura factura, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si hay un cliente para procesar fidelización
            if (!factura.ClienteId.HasValue || factura.ClienteId.Value == Guid.Empty)
            {
                _logger.LogInformation("ℹ️ No se procesa fidelización - No hay cliente identificado en comanda {ComandaId}", comanda.Id);
                return Result.Success(0);
            }
            
            _logger.LogInformation("🏆 Procesando fidelización para cliente {ClienteId} en comanda {ComandaId}", 
                factura.ClienteId, comanda.Id);
            
            // Solicitar acumulación de puntos
            var acumularPuntosRequest = new Comercial.Fidelizacion.Interfaces.AcumularPuntosRequest
            {
                ClienteId = factura.ClienteId.Value,
                FacturaId = factura.Id,
                MontoFactura = factura.Total,
                FechaOperacion = DateTime.UtcNow,
                UsuarioId = request.UsuarioId
            };
            
            var resultadoFidelizacion = await _fidelizacionService.AcumularPuntosAsync(acumularPuntosRequest, cancellationToken);
            
            if (resultadoFidelizacion.Succeeded)
            {
                _logger.LogInformation("✅ Cliente {ClienteId} recibió {Puntos} puntos por compra de {MontoFactura}",
                    factura.ClienteId, resultadoFidelizacion.Value, factura.Total);
                return Result.Success(resultadoFidelizacion.Value);
            }
            else
            {
                _logger.LogWarning("⚠️ No se pudieron acumular puntos para cliente {ClienteId}: {Error}",
                    factura.ClienteId, resultadoFidelizacion.Error);
                return Result.Success(0); // No falla el proceso, simplemente no acumula puntos
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al procesar fidelización para cliente {ClienteId} en comanda {ComandaId}",
                factura.ClienteId, comanda.Id);
            return Result.Success(0); // No falla el proceso, simplemente no acumula puntos
        }
    }

    /// <summary>
    /// Procesar entrega a domicilio si corresponde
    /// </summary>
    private async Task<Result<Guid>> ProcesarEntregaDomicilio(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si es un pedido a domicilio
            if (request.TipoServicio != "Domicilio" || string.IsNullOrEmpty(request.DireccionEntrega))
            {
                // No es necesario procesar entrega a domicilio
                return Result.Success(Guid.Empty);
            }

            _logger.LogInformation("🚚 Procesando entrega a domicilio para comanda {ComandaId}", comanda.Id);
            
            // En un sistema real, aquí crearíamos la entrega en un sistema de domicilios
            // Aquí simplemente devolvemos un GUID simulado
            
            // Simular procesamiento
            var entregaId = Guid.NewGuid();
            
            _logger.LogInformation("✅ Entrega a domicilio registrada con ID {EntregaId} para comanda {ComandaId}", 
                entregaId, comanda.Id);
            
            return Result.Success(entregaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al procesar entrega a domicilio para comanda {ComandaId}", comanda.Id);
            return Result.Failure<Guid>($"Error al procesar entrega a domicilio: {ex.Message}");
        }
    }

    /// <summary>
    /// Entregar pedido al cliente (actualizar estado y notificar)
    /// </summary>
    private async Task<Result> EntregarPedidoCliente(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🍽️ Entregando pedido al cliente para comanda {ComandaId}", comanda.Id);
            
            // Si el cliente tiene ID, registrar entrega (simulado)
            if (comanda.ClienteId != Guid.Empty)
            {
                // Aquí iría código para registrar la entrega o enviar notificación
                // En un sistema real, esto podría invocar a un servicio de notificaciones
                _logger.LogInformation("📱 Se registra entrega al cliente {ClienteId}", comanda.ClienteId);
            }
            
            // Actualizar estado de comanda a Entregada
            comanda.ActualizarEstado(Domain.Operaciones.Comandas.Enums.EstadoComanda.Entregada);
            
            // Guardar cambios
            await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅ Pedido entregado correctamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al entregar pedido para comanda {ComandaId}", comanda.Id);
            return Result.Failure($"Error al entregar pedido: {ex.Message}");
        }
    }

    /// <summary>
    /// Liberar mesa si corresponde
    /// </summary>
    private async Task<Result<MesaLiberadaDto>> LiberarMesa(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si hay mesa para liberar
            if (comanda.MesaId == Guid.Empty)
            {
                _logger.LogInformation("ℹ️ No se libera mesa - Comanda {ComandaId} no tiene mesa asignada", comanda.Id);
                return Result.Success<MesaLiberadaDto>(null);
            }
            
            // Si el cliente indica que no quiere liberar la mesa, no la liberamos
            if (!request.LiberarMesa)
            {
                _logger.LogInformation("ℹ️ No se libera mesa por indicación explícita del usuario - Comanda {ComandaId}, Mesa {MesaId}",
                    comanda.Id, comanda.MesaId);
                return Result.Success<MesaLiberadaDto>(null);
            }
            
            _logger.LogInformation("🪑 Liberando mesa {MesaId} para comanda {ComandaId}", 
                comanda.MesaId, comanda.Id);
            
            // Liberar la mesa
            var resultadoLiberarMesa = await _mesaService.LiberarMesaAsync(
                comanda.MesaId,
                request.UsuarioId,
                cancellationToken);
            
            if (!resultadoLiberarMesa.Succeeded)
            {
                _logger.LogWarning("⚠️ Error al liberar mesa {MesaId}: {Error}", 
                    comanda.MesaId, resultadoLiberarMesa.Error);
                return Result.Failure<MesaLiberadaDto>(resultadoLiberarMesa.Error);
            }
            
            _logger.LogInformation("✅ Mesa {MesaId} liberada correctamente", comanda.MesaId);
            
            // Mapear a DTO
            var mesaLiberadaDto = new MesaLiberadaDto
            {
                MesaId = resultadoLiberarMesa.Value.MesaId,
                EstadoMesa = resultadoLiberarMesa.Value.Estado,
                FechaLiberacion = resultadoLiberarMesa.Value.FechaLiberacion
            };
            
            return Result.Success(mesaLiberadaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al liberar mesa para comanda {ComandaId}", comanda.Id);
            return Result.Failure<MesaLiberadaDto>($"Error al liberar mesa: {ex.Message}");
        }
    }

    // Clase Factory para crear instancias de objetos
    public static class Factory
    {
        /// <summary>
        /// Crea una instancia de un objeto utilizando su constructor por defecto o no público
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a crear</typeparam>
        /// <returns>Nueva instancia del objeto</returns>
        public static T Create<T>() where T : class
        {
            // Usamos reflexión para crear la instancia incluso si el constructor es privado/protegido
            return (T)Activator.CreateInstance(typeof(T), nonPublic: true);
        }

        /// <summary>
        /// Crea una instancia de un objeto utilizando reflexión con argumentos
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a crear</typeparam>
        /// <param name="args">Argumentos para el constructor</param>
        /// <returns>Nueva instancia del objeto</returns>
        public static T Create<T>(params object[] args) where T : class
        {
            // Busca constructor con los tipos de parámetros especificados
            Type[] types = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var constructor = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, types, null);

            if (constructor != null)
                return (T)constructor.Invoke(args);

            // Si no encuentra un constructor exacto, intenta con uno genérico
            return (T)Activator.CreateInstance(typeof(T), 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                null, args, null);
        }
    }

    // Método para validar número de tarjeta
    private bool ValidarNumeroTarjeta(string numeroTarjeta)
    {
        // Validación básica: verificar que tenga entre 13 y 19 dígitos y use algoritmo de Luhn
        if (string.IsNullOrWhiteSpace(numeroTarjeta) || numeroTarjeta.Length < 13 || numeroTarjeta.Length > 19)
        {
            return false;
        }

        // Eliminar espacios y guiones
        numeroTarjeta = numeroTarjeta.Replace(" ", "").Replace("-", "");

        // Verificar que solo contenga dígitos
        if (!numeroTarjeta.All(char.IsDigit))
        {
            return false;
        }

        // Para este ejemplo, simplificamos la validación
        // En un caso real, se implementaría el algoritmo de Luhn o se usaría un servicio externo
        
        // Validación simplificada: comprobar prefijos comunes
        if (numeroTarjeta.StartsWith("4") || // Visa
            numeroTarjeta.StartsWith("5") || // MasterCard
            numeroTarjeta.StartsWith("34") || numeroTarjeta.StartsWith("37") || // American Express
            numeroTarjeta.StartsWith("6")) // Discover/Diners
        {
            return true;
        }

        return false;
    }

    // Método para validar pagos por transferencia
    private bool ValidarPagoTransferencia(InfoPagoDto infoPago)
    {
        // Validación básica para transferencias
        if (string.IsNullOrWhiteSpace(infoPago.ReferenciaPago))
        {
            return false;
        }

        // Verificar que la referencia de pago tenga al menos 6 caracteres
        if (infoPago.ReferenciaPago.Length < 6)
        {
            return false;
        }

        // Para transferencias, se podría validar el formato según el banco o servicio
        // En este ejemplo, hacemos una validación simple
        
        // La referencia debe contener al menos un número y una letra
        return infoPago.ReferenciaPago.Any(char.IsDigit) && 
               infoPago.ReferenciaPago.Any(char.IsLetter);
    }

    // Método para simular una factura existente (para pruebas)
    private Factura SimularFacturaExistente(Guid comandaId)
    {
        // Crear una instancia de factura usando el método estático Crear
        var facturaId = Guid.NewGuid();
        var numeroFactura = $"F{DateTime.Now:yyyyMMdd}-{comandaId.ToString().Substring(0, 4)}";
        var fechaEmision = DateTime.Now;
        var nombreCliente = "Cliente Prueba";
        
        // Llamar al método estático de fábrica
        return Factura.Crear(
            numeroFactura: numeroFactura,
            tipoFactura: TipoFactura.Normal,
            nombreCliente: nombreCliente,
            clienteId: null,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: new List<Guid> { comandaId },
            observaciones: "Factura creada para pruebas",
            fechaEmision: fechaEmision);
    }

    private Guid CrearPedido(Comanda comanda, Factura factura, ProcesarPedidoCompletoCommand request)
    {
        // Simulamos crear un pedido para entrega/servicio a domicilio
        var pedidoId = Guid.NewGuid();
        
        _logger.LogInformation("🚚 Creando pedido con id {PedidoId} para comanda {ComandaId} y factura {FacturaId}",
            pedidoId, comanda.Id, factura.Id);

        // Para las pruebas, necesitamos verificar si la dirección y teléfono de entrega existen
        var direccionEntrega = request.DireccionEntrega ?? "Sin dirección de entrega";
        var telefonoEntrega = request.TelefonoEntrega ?? "Sin teléfono de entrega";
        
        _logger.LogInformation("Pedido será entregado en {Direccion}, contacto: {Telefono}",
            direccionEntrega, telefonoEntrega);
        
        return pedidoId;
    }
} 
