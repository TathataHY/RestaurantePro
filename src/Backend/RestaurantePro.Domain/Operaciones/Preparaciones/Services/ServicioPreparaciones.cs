using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Events;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Services;

/// <summary>
/// Implementación del servicio de preparaciones diarias
/// </summary>
public class ServicioPreparaciones : IServicioPreparaciones
{
    private readonly ILogger<ServicioPreparaciones> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly IDateTimeService _dateTimeService;
    private readonly IPreparacionRepository _preparacionRepository;

    public ServicioPreparaciones(
        ILogger<ServicioPreparaciones> logger,
        INotificationManager notificationManager,
        IDateTimeService dateTimeService,
        IPreparacionRepository preparacionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        _preparacionRepository = preparacionRepository ?? throw new ArgumentNullException(nameof(preparacionRepository));
    }

    /// <summary>
    /// Prepara un producto en una cantidad específica
    /// </summary>
    public async Task<Result<PreparacionDiaria>> PrepararProductoAsync(
        Guid productoId, 
        int cantidad, 
        Guid chefId,
        DateTime fechaVencimiento,
        string? observaciones = null)
    {
        _logger.LogInformation("Iniciando preparación de producto {ProductoId}", productoId);

        _notificationManager.CreateNewNotification();

        // Validaciones básicas
        _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
        _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor que cero", "Cantidad");
        _notificationManager.Require(chefId != Guid.Empty, "El ID del chef no puede estar vacío", "ChefId");
        _notificationManager.Require(fechaVencimiento > _dateTimeService.Now, "La fecha de vencimiento debe ser futura", "FechaVencimiento");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult<PreparacionDiaria>(null);
        }

        try
        {
            // Crear la preparación
            var preparacion = PreparacionDiaria.Crear(
                productoId,
                cantidad,
                chefId,
                fechaVencimiento,
                observaciones,
                _dateTimeService.Now);

            // Guardar en el repositorio
            await _preparacionRepository.AgregarAsync(preparacion);
            await _preparacionRepository.GuardarCambiosAsync();

            _logger.LogInformation("Preparación creada exitosamente: {PreparacionId}", preparacion.Id);

            return Result<PreparacionDiaria>.Success(preparacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar producto {ProductoId}", productoId);
            _notificationManager.AddError($"Error al preparar producto: {ex.Message}", "PrepararProducto");
            return _notificationManager.ToResult<PreparacionDiaria>(null);
        }
    }

    /// <summary>
    /// Verifica si hay suficiente cantidad preparada de un producto
    /// </summary>
    public async Task<Result<bool>> VerificarDisponibilidadAsync(
        Guid productoId, 
        int cantidadRequerida,
        Guid? preparacionId = null)
    {
        _logger.LogDebug("Verificando disponibilidad de producto {ProductoId}", productoId);

        _notificationManager.CreateNewNotification();
        _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
        _notificationManager.Require(cantidadRequerida > 0, "La cantidad requerida debe ser mayor que cero", "CantidadRequerida");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult<bool>(false);
        }

        try
        {
            // Obtener preparaciones disponibles del producto
            var preparaciones = await _preparacionRepository.ObtenerPreparacionesDisponiblesPorProductoAsync(productoId);
            
            // Si se especificó una preparación específica, filtrar solo esa
            if (preparacionId.HasValue)
            {
                preparaciones = preparaciones.Where(p => p.Id == preparacionId.Value).ToList();
                if (!preparaciones.Any())
                {
                    _logger.LogWarning("No se encontró la preparación específica {PreparacionId}", preparacionId.Value);
                    return Result<bool>.Success(false);
                }
            }
            
            // Sumar cantidad disponible total
            int cantidadDisponible = 0;
            foreach (var preparacion in preparaciones)
            {
                if (preparacion.Estado == EstadoPreparacion.Disponible || 
                    preparacion.Estado == EstadoPreparacion.PorVencer)
                {
                    cantidadDisponible += preparacion.CantidadDisponible;
                }
            }
            
            // Verificar si hay suficiente cantidad
            bool hayDisponibilidad = cantidadDisponible >= cantidadRequerida;
            
            _logger.LogDebug("Verificación completada para producto {ProductoId} - Disponible: {Disponible}, Cantidad: {Cantidad}/{Requerida}", 
                productoId, hayDisponibilidad, cantidadDisponible, cantidadRequerida);
            
            return Result<bool>.Success(hayDisponibilidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductoId}", productoId);
            _notificationManager.AddError($"Error al verificar disponibilidad: {ex.Message}", "VerificarDisponibilidad");
            return _notificationManager.ToResult<bool>(false);
        }
    }

    /// <summary>
    /// Consume una cantidad específica de un producto preparado
    /// </summary>
    public async Task<Result> ConsumirPreparacionAsync(Guid productoId, int cantidad)
    {
        _logger.LogInformation("Consumiendo preparación - Producto: {ProductoId}", productoId);

        _notificationManager.CreateNewNotification();
        _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");
        _notificationManager.Require(cantidad > 0, "La cantidad debe ser mayor que cero", "Cantidad");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult();
        }

        try
        {
            // Obtener preparaciones disponibles del producto ordenadas por fecha de vencimiento (FIFO)
            var preparaciones = (await _preparacionRepository.ObtenerPreparacionesDisponiblesPorProductoAsync(productoId))
                .OrderBy(p => p.FechaVencimiento)
                .ThenBy(p => p.FechaPreparacion)
                .ToList();
            
            // Verificar si hay suficientes preparaciones
            int cantidadDisponible = preparaciones.Sum(p => p.CantidadDisponible);
            if (cantidadDisponible < cantidad)
            {
                _logger.LogWarning("No hay suficiente cantidad disponible. Disponible: {Disponible}, Solicitada: {Solicitada}", 
                    cantidadDisponible, cantidad);
                _notificationManager.AddError($"No hay suficiente cantidad disponible. Disponible: {cantidadDisponible}, Solicitada: {cantidad}", "ConsumirPreparacion");
                return _notificationManager.ToResult();
            }
            
            // Consumir de cada preparación hasta completar la cantidad requerida
            int cantidadRestante = cantidad;
            foreach (var preparacion in preparaciones)
            {
                if (cantidadRestante <= 0) break;
                
                // Determinar cuánto consumir de esta preparación
                int cantidadAConsumir = Math.Min(cantidadRestante, preparacion.CantidadDisponible);
                
                // Consumir cantidad
                try 
                {
                    preparacion.ConsumirCantidad(cantidadAConsumir);
                    
                    // Actualizar preparación en repositorio
                    await _preparacionRepository.ActualizarAsync(preparacion);
                    
                    // Reducir cantidad restante
                    cantidadRestante -= cantidadAConsumir;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error al consumir preparación {PreparacionId}: {Error}", 
                        preparacion.Id, ex.Message);
                    continue; // Intentar con la siguiente preparación
                }
            }
            
            // Guardar cambios
            await _preparacionRepository.GuardarCambiosAsync();
            
            if (cantidadRestante > 0)
            {
                _logger.LogWarning("No se pudo consumir toda la cantidad solicitada. Restante: {Restante}", cantidadRestante);
                _notificationManager.AddError($"Solo se consumieron {cantidad - cantidadRestante} de {cantidad} unidades", "ConsumirPreparacion");
                return _notificationManager.ToResult();
            }
            
            _logger.LogInformation("Preparación consumida exitosamente - Producto: {ProductoId}, Cantidad: {Cantidad}", productoId, cantidad);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consumir preparación del producto {ProductoId}", productoId);
            _notificationManager.AddError($"Error al consumir preparación: {ex.Message}", "ConsumirPreparacion");
            return _notificationManager.ToResult();
        }
    }

    /// <summary>
    /// Obtiene todas las preparaciones del día actual
    /// </summary>
    public async Task<Result<List<PreparacionDiaria>>> ObtenerPreparacionesDelDiaAsync()
    {
        _logger.LogDebug("Obteniendo preparaciones del día");

        try
        {
            var preparaciones = await _preparacionRepository.ObtenerPreparacionesDelDiaAsync();
            var listaPreparaciones = preparaciones.ToList();
            
            _logger.LogDebug("Se encontraron {Count} preparaciones del día", listaPreparaciones.Count);
            return Result<List<PreparacionDiaria>>.Success(listaPreparaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener preparaciones del día");
            _notificationManager.AddError($"Error al obtener preparaciones: {ex.Message}", "ObtenerPreparaciones");
            return _notificationManager.ToResult<List<PreparacionDiaria>>(new List<PreparacionDiaria>());
        }
    }

    /// <summary>
    /// Obtiene las preparaciones de un producto específico
    /// </summary>
    public async Task<Result<List<PreparacionDiaria>>> ObtenerPreparacionesPorProductoAsync(Guid productoId)
    {
        _logger.LogDebug("Obteniendo preparaciones del producto {ProductoId}", productoId);

        _notificationManager.CreateNewNotification();
        _notificationManager.Require(productoId != Guid.Empty, "El ID del producto no puede estar vacío", "ProductoId");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult<List<PreparacionDiaria>>(new List<PreparacionDiaria>());
        }

        try
        {
            var preparaciones = await _preparacionRepository.ObtenerPreparacionesDisponiblesPorProductoAsync(productoId);
            var listaPreparaciones = preparaciones.ToList();
            
            _logger.LogDebug("Se encontraron {Count} preparaciones para producto {ProductoId}", 
                listaPreparaciones.Count, productoId);
            
            return Result<List<PreparacionDiaria>>.Success(listaPreparaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener preparaciones del producto {ProductoId}", productoId);
            _notificationManager.AddError($"Error al obtener preparaciones: {ex.Message}", "ObtenerPreparaciones");
            return _notificationManager.ToResult<List<PreparacionDiaria>>(new List<PreparacionDiaria>());
        }
    }

    /// <summary>
    /// Marca las preparaciones vencidas automáticamente
    /// </summary>
    public async Task<Result<int>> MarcarVencidasAsync()
    {
        _logger.LogInformation("Iniciando proceso de marcado de preparaciones vencidas");

        try
        {
            var fechaActual = _dateTimeService.Now;
            
            // Obtener preparaciones disponibles y por vencer
            var preparaciones = await _preparacionRepository.ObtenerPorEstadoAsync(EstadoPreparacion.Disponible);
            var preparacionesPorVencer = await _preparacionRepository.ObtenerPorEstadoAsync(EstadoPreparacion.PorVencer);
            
            var todasPreparaciones = preparaciones.Concat(preparacionesPorVencer).ToList();
            int preparacionesMarcadas = 0;
            
            // Revisar cada preparación
            foreach (var preparacion in todasPreparaciones)
            {
                if (preparacion.FechaVencimiento <= fechaActual)
                {
                    preparacion.MarcarComoVencida();
                    await _preparacionRepository.ActualizarAsync(preparacion);
                    preparacionesMarcadas++;
                }
            }
            
            // Guardar cambios
            if (preparacionesMarcadas > 0)
            {
                await _preparacionRepository.GuardarCambiosAsync();
            }
            
            _logger.LogInformation("Proceso completado: {Count} preparaciones marcadas como vencidas", preparacionesMarcadas);
            return Result<int>.Success(preparacionesMarcadas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar preparaciones vencidas");
            _notificationManager.AddError($"Error al marcar preparaciones vencidas: {ex.Message}", "MarcarVencidas");
            return _notificationManager.ToResult<int>(0);
        }
    }

    /// <summary>
    /// Obtiene las preparaciones que están por vencer
    /// </summary>
    public async Task<Result<List<PreparacionDiaria>>> ObtenerPreparacionesPorVencerAsync(int horasAnticipacion = 2)
    {
        _logger.LogDebug("Obteniendo preparaciones por vencer en {Horas} horas", horasAnticipacion);

        _notificationManager.CreateNewNotification();
        _notificationManager.Require(horasAnticipacion >= 0, "Las horas de anticipación deben ser un valor positivo", "HorasAnticipacion");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult<List<PreparacionDiaria>>(new List<PreparacionDiaria>());
        }

        try
        {
            var preparaciones = await _preparacionRepository.ObtenerPorVencerAsync(horasAnticipacion);
            var listaPreparaciones = preparaciones.ToList();
            
            _logger.LogDebug("Se encontraron {Count} preparaciones por vencer", listaPreparaciones.Count);
            return Result<List<PreparacionDiaria>>.Success(listaPreparaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener preparaciones por vencer");
            _notificationManager.AddError($"Error al obtener preparaciones por vencer: {ex.Message}", "ObtenerPreparacionesPorVencer");
            return _notificationManager.ToResult<List<PreparacionDiaria>>(new List<PreparacionDiaria>());
        }
    }

    /// <summary>
    /// Marca una preparación como disponible para consumo
    /// </summary>
    public async Task<Result> MarcarComoDisponibleAsync(Guid preparacionId)
    {
        _logger.LogInformation("Marcando preparación como disponible: {PreparacionId}", preparacionId);

        _notificationManager.CreateNewNotification();
        _notificationManager.Require(preparacionId != Guid.Empty, "El ID de la preparación no puede estar vacío", "PreparacionId");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult();
        }

        try
        {
            // Obtener la preparación
            var preparacion = await _preparacionRepository.ObtenerPorIdAsync(preparacionId);
            if (preparacion == null)
            {
                _logger.LogWarning("No se encontró la preparación con ID {PreparacionId}", preparacionId);
                _notificationManager.AddError($"No se encontró la preparación con ID {preparacionId}", "MarcarComoDisponible");
                return _notificationManager.ToResult();
            }
            
            // Marcar como disponible
            preparacion.MarcarComoDisponible();
            
            // Actualizar y guardar
            await _preparacionRepository.ActualizarAsync(preparacion);
            await _preparacionRepository.GuardarCambiosAsync();
            
            _logger.LogInformation("Preparación {PreparacionId} marcada como disponible exitosamente", preparacionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar como disponible la preparación {PreparacionId}", preparacionId);
            _notificationManager.AddError($"Error al marcar como disponible: {ex.Message}", "MarcarComoDisponible");
            return _notificationManager.ToResult();
        }
    }

    /// <summary>
    /// Agrega cantidad adicional a una preparación existente
    /// </summary>
    public async Task<Result> AgregarCantidadAsync(Guid preparacionId, int cantidadAdicional)
    {
        _logger.LogInformation("Agregando cantidad a preparación - ID: {PreparacionId}, Cantidad: {Cantidad}", 
            preparacionId, cantidadAdicional);

        _notificationManager.CreateNewNotification();
        _notificationManager.Require(preparacionId != Guid.Empty, "El ID de la preparación no puede estar vacío", "PreparacionId");
        _notificationManager.Require(cantidadAdicional > 0, "La cantidad adicional debe ser mayor que cero", "CantidadAdicional");

        if (_notificationManager.HasErrors)
        {
            return _notificationManager.ToResult();
        }

        try
        {
            // Obtener la preparación
            var preparacion = await _preparacionRepository.ObtenerPorIdAsync(preparacionId);
            if (preparacion == null)
            {
                _logger.LogWarning("No se encontró la preparación con ID {PreparacionId}", preparacionId);
                _notificationManager.AddError($"No se encontró la preparación con ID {preparacionId}", "AgregarCantidad");
                return _notificationManager.ToResult();
            }
            
            // Agregar cantidad
            preparacion.AgregarCantidad(cantidadAdicional);
            
            // Actualizar y guardar
            await _preparacionRepository.ActualizarAsync(preparacion);
            await _preparacionRepository.GuardarCambiosAsync();
            
            _logger.LogInformation("Se agregó cantidad a la preparación {PreparacionId} exitosamente", preparacionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar cantidad a la preparación {PreparacionId}", preparacionId);
            _notificationManager.AddError($"Error al agregar cantidad: {ex.Message}", "AgregarCantidad");
            return _notificationManager.ToResult();
        }
    }

    /// <summary>
    /// Obtiene estadísticas de preparaciones del día
    /// </summary>
    public async Task<Result<EstadisticasPreparaciones>> ObtenerEstadisticasDelDiaAsync()
    {
        _logger.LogDebug("Obteniendo estadísticas de preparaciones del día");

        try
        {
            var estadisticas = await _preparacionRepository.ObtenerEstadisticasDelDiaAsync();
            
            _logger.LogDebug("Estadísticas obtenidas: {TotalPreparaciones} preparaciones", estadisticas.TotalPreparaciones);
            return Result<EstadisticasPreparaciones>.Success(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de preparaciones");
            _notificationManager.AddError($"Error al obtener estadísticas: {ex.Message}", "ObtenerEstadisticas");
            return _notificationManager.ToResult<EstadisticasPreparaciones>(new EstadisticasPreparaciones());
        }
    }
} 



