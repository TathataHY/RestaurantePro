using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Enums;

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
    private readonly ILogger<AjustarInventarioHandler> _logger;

    public AjustarInventarioHandler(
        IIngredienteRepository ingredienteRepository,
        IMovimientoInventarioRepository movimientoRepository,
        IValidacionInventarioService validacionService,
        ICurrentUserService currentUserService,
        ILogger<AjustarInventarioHandler> logger)
    {
        _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
        _validacionService = validacionService ?? throw new ArgumentNullException(nameof(validacionService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>($"Ingrediente con ID {request.IngredienteId} no encontrado");
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

            // Actualizar el ingrediente
            await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);

            // Agregar el movimiento al repositorio
            await _movimientoRepository.AgregarAsync(movimiento);

            _logger.LogInformation("Ajuste de inventario completado. Movimiento ID: {MovimientoId}, Nuevo stock: {NuevoStock}", 
                movimiento.Id, ingrediente.Stock);
            
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success<bool>(true);
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
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Usuario no autenticado");
            }

            // Validar autorización según el rol
            if (!Enum.TryParse<RolUsuario>(userRole, out var rol))
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Rol de usuario no válido");
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
            RolUsuario.Cocinero when cantidad > 100 => 
                RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Los cocineros no pueden ajustar más de 100 unidades. Cantidad excede el límite autorizado"),
            RolUsuario.GerenteInventario when cantidad > 1000 => 
                RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure("Los gerentes de inventario no pueden ajustar más de 1000 unidades sin autorización"),
            _ => RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success()
        };
    }
} 