using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;

public class AnularFacturaCommandHandler : IRequestHandler<AnularFacturaCommand, Result<FacturaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AnularFacturaCommandHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IDelayProvider _delayProvider;

    public AnularFacturaCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AnularFacturaCommandHandler> logger,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        INotificationService notificationService,
        IUsuarioRepository usuarioRepository,
        IDelayProvider delayProvider)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _notificationService = notificationService;
        _usuarioRepository = usuarioRepository;
        _delayProvider = delayProvider;
    }

    public async Task<Result<FacturaDto>> Handle(AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de anulación para factura {FacturaId}", request.FacturaId);
            
            // Obtener la factura de la base de datos
            var factura = await _context.Facturas.FindAsync(new object[] { request.FacturaId }, cancellationToken);
            
            if (factura == null)
            {
                string mensajeError = $"La factura con ID {request.FacturaId} no encontrada";
                _logger.LogWarning(mensajeError);
                var result = Result.Failure<FacturaDto>(mensajeError);
                _logger.LogInformation("🔍 AnularFactura - Devolviendo Result.Failure: IsSuccess={IsSuccess}, Errors={@Errors}", 
                    result.IsSuccess(), result.Errors);
                return result;
            }
            
            // Verificar si la factura ya está anulada
            if (factura.Estado == EstadoFactura.Anulada)
            {
                string mensajeError = "La factura ya se encuentra anulada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<FacturaDto>(mensajeError);
            }
            
            // Validar autorización y permisos
            if (request.RequiereAprobacionGerencia)
            {
                var resultadoValidacion = await ValidarAprobacionGerencia(request, cancellationToken);
                if (!resultadoValidacion.Succeeded)
                {
                    return Result.Failure<FacturaDto>(resultadoValidacion.Error);
                }
            }
            
            // Ejecutar el proceso de anulación
            var resultadoAnulacion = await EjecutarAnulacion(factura, request, cancellationToken);
            if (!resultadoAnulacion.Succeeded)
            {
                return Result.Failure<FacturaDto>(resultadoAnulacion.Error);
            }
            
            // Procesar notificaciones
            await ProcesarNotificaciones(factura, request, cancellationToken);
            
            // Mapear a DTO para la respuesta
            var facturaDto = _mapper.Map<FacturaDto>(factura);
            
            _logger.LogInformation("Anulación de factura {FacturaId} completada exitosamente", request.FacturaId);
            
            return Result.Success(facturaDto);
        }
        catch (Exception ex)
        {
            string mensajeError = $"Ocurrió un error al anular la factura: {ex.Message}";
            _logger.LogError(ex, mensajeError);
            return Result.Failure<FacturaDto>(mensajeError);
        }
    }
    
    private async Task<Result<bool>> ValidarAprobacionGerencia(AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        if (!request.GerenteAprobadorId.HasValue)
        {
            string mensajeError = "Se requiere aprobación de gerencia pero no se especificó un gerente aprobador";
            _logger.LogWarning(mensajeError);
            return Result.Failure<bool>(mensajeError);
        }
        
        // Verificar que el aprobador sea un gerente
        var gerente = await _usuarioRepository.ObtenerPorIdAsync(request.GerenteAprobadorId.Value, cancellationToken);
        
        if (gerente == null || gerente.Rol != "Gerente")
        {
            string mensajeError = "El gerente aprobador especificado no es válido para aprobar la anulación.";
            _logger.LogWarning(mensajeError);
            return Result.Failure<bool>(mensajeError);
        }
        
        return Result.Success(true);
    }

    private async Task<Result<bool>> EjecutarAnulacion(Factura factura, AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Ejecutando anulación de factura {FacturaId}", factura.Id);
            
            // Realizar las operaciones de anulación
            factura.Anular(request.Motivo, _dateTimeService);
            
            // Procesar reversión de inventario si corresponde
            if (request.RevertirInventario)
            {
                _logger.LogInformation("Reversión de inventario para factura {FacturaId}", factura.Id);
                // Implementar lógica de reversión de inventario
            }
            
            // Procesar cancelación de puntos si corresponde
            if (request.CancelarPuntosFidelizacion && factura.ClienteId.HasValue)
            {
                await ProcesarCancelacionPuntosFidelizacion(factura, request);
            }
            
            // Procesar devolución de pagos si corresponde
            if (request.ProcesarDevolucionPago)
            {
                await ProcesarDevolucionPago(factura, request, cancellationToken);
            }
            
            // Generar nota de crédito si corresponde
            if (request.GenerarNotaCredito)
            {
                await GenerarNotaCredito(factura, request, cancellationToken);
            }
            
            // Guardar cambios en la base de datos
            int result = await _context.SaveChangesAsync(cancellationToken);
            
            if (result <= 0)
            {
                string mensajeError = "No se pudieron guardar los cambios en la base de datos";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }
            
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            string mensajeError = $"Error durante la anulación: {ex.Message}";
            _logger.LogError(ex, mensajeError);
            return Result.Failure<bool>(mensajeError);
        }
    }
    
    /// <summary>
    /// Procesa la cancelación de puntos de fidelización otorgados por la factura
    /// </summary>
    private async Task ProcesarCancelacionPuntosFidelizacion(Factura factura, AnularFacturaCommand request)
    {
        _logger.LogInformation("Cancelación de puntos de fidelización para factura {FacturaId}", factura.Id);
        
        // Aquí iría la lógica para cancelar puntos de fidelización
        // Por ejemplo, buscar los puntos otorgados por esta factura y revertirlos
        
        // Simulamos una operación asíncrona
        await _delayProvider.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
    }
    
    /// <summary>
    /// Genera una nota de crédito para la factura anulada
    /// </summary>
    private async Task GenerarNotaCredito(Factura factura, AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generación de nota de crédito para factura {FacturaId}", factura.Id);
        
        // Aquí iría la lógica para generar la nota de crédito
        // Por ejemplo, crear un nuevo documento de tipo NotaCredito con referencia a la factura
        
        // Simulamos una operación asíncrona
        await _delayProvider.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
    }
    
    /// <summary>
    /// Procesa la devolución de pagos para la factura anulada
    /// </summary>
    private async Task ProcesarDevolucionPago(Factura factura, AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Devoluciones de pagos para factura {FacturaId}", factura.Id);
        
        // Aquí iría la lógica para procesar las devoluciones de pago
        // Por ejemplo, crear registros de devolución para cada pago asociado a la factura
        
        // Simulamos una operación asíncrona
        await _delayProvider.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
    }
    
    /// <summary>
    /// Envía notificaciones sobre la anulación de la factura
    /// </summary>
    private async Task ProcesarNotificaciones(Factura factura, AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        if (request.NotificarCliente && factura.ClienteId.HasValue)
        {
            _logger.LogInformation("Enviando notificación al cliente sobre anulación de factura {FacturaId}", factura.Id);
            
            // Implementar envío de notificaciones usando las interfaces disponibles en el proyecto
            // Este código es simplificado para las pruebas unitarias
            await _delayProvider.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            
            var notification = new Notification
            {
                Title = "Factura anulada",
                Message = $"La factura #{factura.NumeroFactura} ha sido anulada.",
                Type = "Email",
                RelatedEntityId = factura.Id
            };
            
            await _notificationService.SendNotificationAsync(notification);
        }
    }
} 