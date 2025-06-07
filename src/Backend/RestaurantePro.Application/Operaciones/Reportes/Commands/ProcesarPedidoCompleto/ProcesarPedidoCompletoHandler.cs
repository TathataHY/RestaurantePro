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
            
            // 2. Validar que la comanda esté en estado válido para procesar (EnProceso)
            if (comanda.Estado != Domain.Operaciones.Comandas.Enums.EstadoComanda.EnProceso)
            {
                _logger.LogWarning("❌ La comanda {ComandaId} no está en estado válido para procesar. Estado actual: {Estado}", 
                    request.ComandaId, comanda.Estado);
                return Result.Failure<ProcesarPedidoCompletoDto>($"La comanda debe estar en estado EnProceso para procesarla. Estado actual: {comanda.Estado}");
            }
            
            // 3. Procesar el pago (si es requerido)
            var pagoResult = await ProcesarPago(comanda, request, cancellationToken);
            if (!pagoResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(pagoResult.Error ?? "Error al procesar el pago");
            }
            
            // 4. Generar facturación
            var facturaResult = await GenerarFactura(comanda, request, cancellationToken);
            if (!facturaResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(facturaResult.Error ?? "Error al procesar la facturación");
            }
            
            var factura = facturaResult.Value;
            
            // 5. Procesar fidelización (acumulación de puntos)
            var fidelizacionResult = await ProcesarFidelizacion(comanda, factura, request, cancellationToken);
            if (!fidelizacionResult.Succeeded)
            {
                return Result.Failure<ProcesarPedidoCompletoDto>(fidelizacionResult.Error ?? "Error al procesar la fidelización");
            }
            
            var puntosAcumulados = fidelizacionResult.Value;
            
            // 6. Cambiar estado de la comanda a Finalizada
            await FinalizarComanda(comanda, cancellationToken);
            
            // 7. Liberar mesa (si aplica)
            var mesaLiberadaResult = await LiberarMesa(comanda, request, cancellationToken);
            if (!mesaLiberadaResult.Succeeded)
            {
                _logger.LogWarning("⚠️ No se pudo liberar la mesa: {Error}", mesaLiberadaResult.Error);
                // No fallamos todo el proceso si no se pudo liberar la mesa
            }
            
            // 8. Crear el DTO de respuesta
            var resultado = new ProcesarPedidoCompletoDto
            {
                ComandaId = comanda.Id,
                FacturaId = factura.Id,
                Total = factura.Total,
                PuntosAcumulados = puntosAcumulados,
                MesaLiberada = mesaLiberadaResult.Value != null,
                MesaId = comanda.MesaId,
                FechaHora = DateTime.Now,
                EstadoComanda = comanda.Estado.ToString()
            };
            
            if (mesaLiberadaResult.Value != null)
            {
                resultado.MesaLiberadaDto = mesaLiberadaResult.Value;
            }
            
            _logger.LogInformation("✅ Procesamiento de pedido completado exitosamente - Comanda: {ComandaId}, Factura: {FacturaId}", 
                comanda.Id, factura.Id);
            
            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al procesar pedido completo: {Error}", ex.Message);
            return Result.Failure<ProcesarPedidoCompletoDto>($"Error al procesar pedido: {ex.Message}");
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
            else if (request.TipoPago == "Efectivo")
            {
                return await ProcesarPagoEfectivo(comanda, request.InfoPago, cancellationToken);
            }
            else if (request.TipoPago == "Digital")
            {
                return await ProcesarPagoDigital(comanda, request.InfoPago, cancellationToken);
            }
            else if (request.TipoPago == "Transferencia")
            {
                return await ProcesarPagoTransferencia(comanda, request.InfoPago, cancellationToken);
            }
            else
            {
                _logger.LogWarning("❌ Tipo de pago no soportado: {TipoPago}", request.TipoPago);
                return Result.Failure($"Tipo de pago no soportado: {request.TipoPago}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando pago: {Error}", ex.Message);
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
    
    private async Task<Result> FinalizarComanda(Comanda comanda, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Finalizando comanda {ComandaId}", comanda.Id);
            
            // Cambiar estado de la comanda a Finalizada
            comanda.ActualizarEstado(Domain.Operaciones.Comandas.Enums.EstadoComanda.Finalizada);
            await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
            
            _logger.LogInformation("✅ Comanda {ComandaId} finalizada correctamente", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finalizando comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error finalizando comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Generar factura para la comanda
    /// </summary>
    private async Task<Result<Factura>> GenerarFactura(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📝 Generando factura para comanda {ComandaId}", comanda.Id);
            
            // Validar datos de facturación
            if (request.ClienteId == Guid.Empty)
            {
                return Result.Failure<Factura>("Se requiere un cliente válido para generar la factura");
            }
            
            // Obtener cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                return Result.Failure<Factura>($"No se encontró el cliente con ID {request.ClienteId}");
            }
            
            // Crear la factura usando el servicio de facturación
            var facturaRequest = new FacturaRequest
            {
                ComandaId = comanda.Id,
                ClienteId = cliente.Id,
                UsuarioId = request.UsuarioId,
                Observaciones = request.Observaciones
            };
            
            var facturaResult = await _facturacionService.CrearFacturaAsync(facturaRequest, cancellationToken);
            if (!facturaResult.IsSuccess)
            {
                return Result.Failure<Factura>(facturaResult.Error);
            }
            
            _logger.LogInformation("✅ Factura generada correctamente para comanda {ComandaId}", comanda.Id);
            
            return Result.Success(facturaResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando factura para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure<Factura>($"Error generando factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesar fidelización para el cliente
    /// </summary>
    private async Task<Result<int>> ProcesarFidelizacion(Comanda comanda, Factura factura, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🏆 Procesando fidelización para cliente {ClienteId} en comanda {ComandaId}", 
                factura.ClienteId, comanda.Id);
            
            // Solicitar acumulación de puntos
            var acumularRequest = new AcumularPuntosRequest
            {
                ClienteId = factura.ClienteId,
                FacturaId = factura.Id,
                MontoFactura = factura.Total,
                FechaOperacion = DateTime.Now,
                UsuarioId = request.UsuarioId
            };
            
            var puntosResult = await _fidelizacionService.AcumularPuntosAsync(acumularRequest, cancellationToken);
            if (!puntosResult.IsSuccess)
            {
                _logger.LogWarning("⚠️ No se pudieron acumular puntos: {Error}", puntosResult.Error);
                return Result.Success(0); // Continuamos el proceso aunque falle la fidelización
            }
            
            _logger.LogInformation("✅ Se acumularon {Puntos} puntos para el cliente {ClienteId}", 
                puntosResult.Value, factura.ClienteId);
            
            return Result.Success(puntosResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando fidelización para comanda {ComandaId}: {Message}", 
                comanda.Id, ex.Message);
            return Result.Success(0); // No interrumpimos el proceso principal por errores en fidelización
        }
    }

    /// <summary>
    /// Liberar mesa de la comanda
    /// </summary>
    private async Task<Result<MesaLiberadaDto>> LiberarMesa(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Si no se requiere liberar la mesa, saltamos este paso
            if (!request.LiberarMesa)
            {
                _logger.LogInformation("ℹ️ No se requiere liberar la mesa para comanda {ComandaId}", comanda.Id);
                return Result.Success(new MesaLiberadaDto
                {
                    MesaId = comanda.MesaId.GetValueOrDefault(),
                    EstadoMesa = "No Liberada"
                    // No establecemos FechaLiberacion para que tome su valor por defecto
                });
            }
            
            if (!comanda.MesaId.HasValue || comanda.MesaId.Value == Guid.Empty)
            {
                _logger.LogWarning("⚠️ La comanda {ComandaId} no tiene una mesa asignada", comanda.Id);
                return Result.Success(new MesaLiberadaDto
                {
                    MesaId = Guid.Empty,
                    EstadoMesa = "Sin Mesa"
                    // No establecemos FechaLiberacion para que tome su valor por defecto
                });
            }
            
            _logger.LogInformation("🪑 Liberando mesa {MesaId} para comanda {ComandaId}", comanda.MesaId, comanda.Id);
            
            // Obtener la mesa
            var mesaId = comanda.MesaId.Value;
            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId, cancellationToken);
            if (mesa == null)
            {
                return Result.Failure<MesaLiberadaDto>($"No se encontró la mesa con ID {mesaId}");
            }
            
            // Liberar la mesa con el servicio
            var liberarResult = await _mesaService.LiberarMesaAsync(mesa.Id, request.UsuarioId, cancellationToken);
            if (!liberarResult.IsSuccess)
            {
                return Result.Failure<MesaLiberadaDto>(liberarResult.Error);
            }
            
            _logger.LogInformation("✅ Mesa {MesaId} liberada correctamente", mesa.Id);
            
            return Result.Success(new MesaLiberadaDto
            {
                MesaId = mesa.Id,
                EstadoMesa = "Disponible",
                FechaLiberacion = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error liberando mesa para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure<MesaLiberadaDto>($"Error liberando mesa: {ex.Message}");
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

    /// <summary>
    /// Procesar entrega a domicilio si aplica
    /// </summary>
    private async Task<Result> ProcesarEntregaDomicilio(Comanda comanda, ProcesarPedidoCompletoCommand request, CancellationToken cancellationToken)
    {
        // Verificar si es una comanda para llevar o a domicilio usando la enumeración directamente 
        // en lugar de la propiedad que no existe
        TipoServicioComanda tipoServicio = TipoServicioComanda.Local; // Valor por defecto
        
        // Aquí podríamos determinar el tipo de servicio de alguna manera (por ejemplo, desde un campo en request)
        // o desde alguna otra fuente, ya que la propiedad TipoServicio no existe en Comanda
        if (request.TipoServicio == "Domicilio")
        {
            tipoServicio = TipoServicioComanda.Domicilio;
        }
        else if (request.TipoServicio == "ParaLlevar")
        {
            tipoServicio = TipoServicioComanda.ParaLlevar;
        }
        
        if (tipoServicio != TipoServicioComanda.Domicilio && 
            tipoServicio != TipoServicioComanda.ParaLlevar)
        {
            return Result.Success();
        }
        
        try
        {
            _logger.LogInformation("🚚 Procesando entrega a domicilio/para llevar para comanda {ComandaId}", comanda.Id);
            
            // Aquí verificaríamos si hay información de entrega
            // Por ahora, solo validamos que la comanda esté finalizada
            if (comanda.Estado != EstadoComanda.Finalizada)
            {
                return Result.Failure("La comanda debe estar finalizada para procesar la entrega");
            }
            
            // TODO: Implementar lógica de entrega a domicilio cuando se agregue el módulo
            
            _logger.LogInformation("✅ Entrega procesada para comanda {ComandaId}", comanda.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando entrega para comanda {ComandaId}: {Message}", comanda.Id, ex.Message);
            return Result.Failure($"Error procesando entrega: {ex.Message}");
        }
    }
} 