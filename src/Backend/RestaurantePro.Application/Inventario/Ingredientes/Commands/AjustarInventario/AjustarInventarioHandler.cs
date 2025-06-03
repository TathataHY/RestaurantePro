namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AjustarInventario;

/// <summary>
/// Handler para ajustar inventario de ingredientes
/// </summary>
public class AjustarInventarioHandler : IRequestHandler<AjustarInventarioCommand, RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>>
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;
    private readonly IValidacionInventarioService _validacionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunicationService _communicationService;
    private readonly IAlertaStockService _alertaStockService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AjustarInventarioHandler> _logger;

    public AjustarInventarioHandler(
        IIngredienteRepository ingredienteRepository,
        IMovimientoInventarioRepository movimientoRepository,
        IValidacionInventarioService validacionService,
        ICurrentUserService currentUserService,
        ICommunicationService communicationService,
        IAlertaStockService alertaStockService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<AjustarInventarioHandler> logger)
    {
        _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
        _validacionService = validacionService ?? throw new ArgumentNullException(nameof(validacionService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        _alertaStockService = alertaStockService ?? throw new ArgumentNullException(nameof(alertaStockService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Maneja el comando de ajuste de inventario
    /// </summary>
    /// <param name="request">Comando con los datos del ajuste</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del ajuste</returns>
    public async Task<RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>> Handle(AjustarInventarioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando ajuste de inventario para ingrediente {IngredienteId}", request.IngredienteId);

            // Iniciar transacción
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Validar autorización del usuario
                var resultadoAutorizacion = ValidarAutorizacionUsuario(request);
                if (!resultadoAutorizacion.Succeeded)
                {
                    return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>(resultadoAutorizacion.Error ?? "Error de autorización");
                }

                // Obtener el ingrediente
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(request.IngredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>($"Ingrediente no encontrado con ID {request.IngredienteId}");
                }

                // Validar el ajuste
                var resultadoValidacion = await _validacionService.ValidarAjusteInventarioAsync(
                    request.IngredienteId,
                    request.TipoMovimiento,
                    request.Cantidad,
                    ingrediente.Stock);

                if (!resultadoValidacion.EsValido)
                {
                    var errores = string.Join(", ", resultadoValidacion.Errores);
                    return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>($"Validación falló: {errores}");
                }

                // Obtener el motivo del ajuste
                var motivo = !string.IsNullOrWhiteSpace(request.MotivoAjuste) ? request.MotivoAjuste : 
                            !string.IsNullOrWhiteSpace(request.Motivo) ? request.Motivo : "Ajuste manual";

                // Aplicar el ajuste usando los métodos de dominio
                MovimientoInventario movimiento;
                if (request.TipoMovimiento == TipoMovimientoInventario.Ingreso || 
                   request.TipoMovimiento == TipoMovimientoInventario.Incremento)
                {
                    movimiento = ingrediente.IncrementarStock(request.Cantidad, motivo);
                }
                else
                {
                    movimiento = ingrediente.DecrementarStock(request.Cantidad, motivo);
                }

                // Verificar si el ajuste requiere aprobación (cantidades grandes)
                if (RequiereAprobacion(request.Cantidad))
                {
                    await EnviarNotificacionAprobacion(ingrediente, request.Cantidad, motivo, cancellationToken);
                }

                // Verificar alertas de stock crítico
                if (EsStockCritico(ingrediente))
                {
                    await EnviarNotificacionStockCritico(ingrediente, cancellationToken);
                }

                // Actualizar el ingrediente
                await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);

                // Agregar el movimiento al repositorio
                await _movimientoRepository.AgregarAsync(movimiento);

                // Registrar evento de auditoría
                await RegistrarEventoAuditoria(ingrediente, request, movimiento, cancellationToken);

                // Guardar cambios
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Confirmar transacción
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Ajuste de inventario completado. Movimiento ID: {MovimientoId}, Nuevo stock: {NuevoStock}", 
                    movimiento.Id, ingrediente.Stock);
                
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success<bool>(true);
            }
            catch (Exception)
            {
                // Hacer rollback en caso de error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ajustar inventario del ingrediente {IngredienteId}", request.IngredienteId);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>($"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida si el usuario tiene autorización para realizar el ajuste
    /// </summary>
    private RestaurantePro.Domain.Core.SharedKernel.Results.Result ValidarAutorizacionUsuario(AjustarInventarioCommand request)
    {
        try
        {
            // Obtener información del usuario actual
            var userId = _currentUserService.UserId;
            var userRole = _currentUserService.Rol;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Usuario sin autorización para realizar ajustes de inventario");
            }

            // Validar autorización según el rol
            if (!Enum.TryParse<RolUsuario>(userRole, out var rol))
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Usuario sin autorización válida para ajustes de inventario");
            }

            // Validar límites según el rol
            var validacionLimites = ValidarLimitesPorRol(rol, request.Cantidad);
            if (!validacionLimites.Succeeded)
            {
                return validacionLimites;
            }

            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar autorización para usuario {UserId}", request.UsuarioId);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Error en validación de autorización");
        }
    }

    /// <summary>
    /// Valida los límites de cantidad según el rol del usuario
    /// </summary>
    private RestaurantePro.Domain.Core.SharedKernel.Results.Result ValidarLimitesPorRol(RolUsuario rol, decimal cantidad)
    {
        return rol switch
        {
            RolUsuario.Mesero => 
                RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Los meseros no tienen autorización para realizar ajustes de inventario"),
            RolUsuario.Cocinero when cantidad > 100 => 
                RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Los cocineros no pueden ajustar más de 100 unidades. Cantidad excede el límite autorizado"),
            RolUsuario.GerenteInventario when cantidad > 1000 => 
                RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Los gerentes de inventario no pueden ajustar más de 1000 unidades sin autorización"),
            _ => RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success()
        };
    }

    /// <summary>
    /// Determina si un ajuste requiere aprobación basado en la cantidad
    /// </summary>
    private bool RequiereAprobacion(decimal cantidad)
    {
        return cantidad >= 500m; // Cantidades grandes requieren aprobación
    }

    /// <summary>
    /// Determina si el stock del ingrediente está en nivel crítico
    /// </summary>
    private bool EsStockCritico(Ingrediente ingrediente)
    {
        return ingrediente.Stock <= ingrediente.StockMinimo * 1.2m; // 20% por encima del mínimo se considera crítico
    }

    /// <summary>
    /// Envía notificación para ajustes que requieren aprobación
    /// </summary>
    private async Task EnviarNotificacionAprobacion(Ingrediente ingrediente, decimal cantidad, string motivo, CancellationToken cancellationToken)
    {
        var destinatarios = new[] { "gerencia@restaurantepro.com" };
        var asunto = "Ajuste de inventario requiere aprobación";
        var mensaje = $"El ajuste de {cantidad} unidades para {ingrediente.Nombre} requiere aprobación. Motivo: {motivo}";
        
        await _communicationService.EnviarNotificacionAsync(
            destinatarios, 
            asunto, 
            mensaje, 
            TipoComunicacion.Advertencia, 
            cancellationToken);
    }

    /// <summary>
    /// Envía notificación cuando el stock está en nivel crítico
    /// </summary>
    private async Task EnviarNotificacionStockCritico(Ingrediente ingrediente, CancellationToken cancellationToken)
    {
        var destinatarios = new[] { "inventario@restaurantepro.com", "gerencia@restaurantepro.com" };
        var asunto = "⚠️ Stock crítico - requiere reposición inmediata";
        var mensaje = $"Stock crítico para {ingrediente.Nombre}. Stock actual: {ingrediente.Stock}, Mínimo: {ingrediente.StockMinimo}";
        
        await _communicationService.EnviarNotificacionAsync(
            destinatarios, 
            asunto, 
            mensaje, 
            TipoComunicacion.StockBajo, 
            cancellationToken);
    }

    /// <summary>
    /// Registra evento de auditoría para el ajuste de inventario
    /// </summary>
    private async Task RegistrarEventoAuditoria(Ingrediente ingrediente, AjustarInventarioCommand request, MovimientoInventario movimiento, CancellationToken cancellationToken)
    {
        var stockAnterior = request.TipoMovimiento == TipoMovimientoInventario.Incremento 
            ? ingrediente.Stock - request.Cantidad 
            : ingrediente.Stock + request.Cantidad;

        var eventoAuditoria = new EventoAuditoria
        {
            Accion = "AjusteInventario",
            Entidad = "Ingrediente",
            EntidadId = ingrediente.Id,
            UsuarioId = Guid.Parse(_currentUserService.UserId!),
            ValoresAnteriores = $"Stock: {stockAnterior}",
            ValoresNuevos = $"Stock: {ingrediente.Stock}",
            Observaciones = $"Ajuste de {request.TipoMovimiento}: {request.Cantidad} unidades. Motivo: {request.MotivoAjuste}",
            DatosAdicionales = new Dictionary<string, object>
            {
                ["MovimientoId"] = movimiento.Id,
                ["TipoMovimiento"] = request.TipoMovimiento.ToString(),
                ["Cantidad"] = request.Cantidad
            }
        };

        await _auditService.RegistrarEventoAsync(eventoAuditoria, cancellationToken);
    }
} 