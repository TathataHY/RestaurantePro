using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Mobile.Core.Services
{
    /// <summary>
    /// Implementación del servicio de preparaciones diarias en mobile
    /// </summary>
    public class DailyPreparationsService : IDailyPreparationsService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<DailyPreparationsService> _logger;

        public DailyPreparationsService(IApiService apiService, IAuthService authService, ILogger<DailyPreparationsService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las preparaciones diarias (para gestión)
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasAsync()
        {
            try
            {
                _logger.LogInformation("📋 Obteniendo preparaciones diarias (todas para gestión)");
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.GetAsync<List<PreparacionDiariaDto>>("api/operaciones/preparaciones-diarias", token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparaciones diarias obtenidas: {Count} preparaciones", response.Data?.Count ?? 0);
                    return Result<List<PreparacionDiariaDto>>.Success(response.Data ?? new List<PreparacionDiariaDto>());
                }
                
                _logger.LogWarning("⚠️ Error al obtener preparaciones diarias: {Error}", response.Error);
                return Result<List<PreparacionDiariaDto>>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparaciones diarias");
                return Result<List<PreparacionDiariaDto>>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene preparaciones diarias para la página móvil (rango operativo de 5 días)
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasOperativasAsync(DateTime? fecha = null, int limite = 50)
        {
            try
            {
                _logger.LogInformation("📱 Obteniendo preparaciones diarias operativas - Fecha: {Fecha}, Límite: {Limite}", 
                    fecha?.ToString("yyyy-MM-dd") ?? "HOY", limite);
                
                var token = await _authService.GetTokenAsync();
                var url = $"api/operaciones/preparaciones-diarias/menu-del-dia?limite={limite}";
                
                if (fecha.HasValue)
                {
                    url += $"&fecha={fecha.Value:yyyy-MM-dd}";
                }
                
                var response = await _apiService.GetAsync<List<PreparacionDiariaDto>>(url, token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparaciones diarias operativas obtenidas: {Count} preparaciones", response.Data?.Count ?? 0);
                    return Result<List<PreparacionDiariaDto>>.Success(response.Data ?? new List<PreparacionDiariaDto>());
                }
                
                _logger.LogWarning("⚠️ Error al obtener preparaciones diarias operativas: {Error}", response.Error);
                return Result<List<PreparacionDiariaDto>>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparaciones diarias operativas");
                return Result<List<PreparacionDiariaDto>>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el menú del día (solo preparaciones disponibles, de hoy, no vencidas)
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> GetMenuDelDiaAsync(DateTime? fecha = null, int limite = 10)
        {
            try
            {
                _logger.LogInformation("🍽️ Obteniendo menú del día - Fecha: {Fecha}, Límite: {Limite}", 
                    fecha?.ToString("yyyy-MM-dd") ?? "HOY", limite);
                
                var token = await _authService.GetTokenAsync();
                var url = $"api/operaciones/preparaciones-diarias/menu-del-dia?limite={limite}";
                
                if (fecha.HasValue)
                {
                    url += $"&fecha={fecha.Value:yyyy-MM-dd}";
                }
                
                var response = await _apiService.GetAsync<List<PreparacionDiariaDto>>(url, token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Menú del día obtenido: {Count} preparaciones disponibles", response.Data?.Count ?? 0);
                    return Result<List<PreparacionDiariaDto>>.Success(response.Data ?? new List<PreparacionDiariaDto>());
                }
                
                _logger.LogWarning("⚠️ Error al obtener menú del día: {Error}", response.Error);
                return Result<List<PreparacionDiariaDto>>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener menú del día");
                return Result<List<PreparacionDiariaDto>>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene una preparación diaria específica por ID
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> GetPreparacionDiariaAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo preparación diaria: {Id}", id);
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.GetAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparación diaria obtenida: {Id}", id);
                    return Result<PreparacionDiariaDto>.Success(response.Data);
                }
                
                _logger.LogWarning("⚠️ Error al obtener preparación diaria: {Error}", response.Error);
                return Result<PreparacionDiariaDto>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparación diaria: {Id}", id);
                return Result<PreparacionDiariaDto>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea una nueva preparación diaria
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> CrearPreparacionDiariaAsync(CrearPreparacionDiariaCommand command)
        {
            try
            {
                _logger.LogInformation("➕ Creando preparación diaria - Producto: {ProductoId}, Cantidad: {Cantidad}", 
                    command.ProductoId, command.Cantidad);
                
                var json = JsonSerializer.Serialize(command);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.PostAsync<PreparacionDiariaDto>("api/operaciones/preparaciones-diarias", command, token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparación diaria creada: {Id}", response.Data?.Id);
                    return Result<PreparacionDiariaDto>.Success(response.Data);
                }
                
                _logger.LogWarning("⚠️ Error al crear preparación diaria: {Error}", response.Error);
                return Result<PreparacionDiariaDto>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear preparación diaria");
                return Result<PreparacionDiariaDto>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza una preparación diaria existente
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> ActualizarPreparacionDiariaAsync(Guid id, ActualizarPreparacionDiariaCommand command)
        {
            try
            {
                _logger.LogInformation("✏️ Actualizando preparación diaria: {Id}", id);
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.PutAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", command, token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparación diaria actualizada: {Id}", id);
                    return Result<PreparacionDiariaDto>.Success(response.Data);
                }
                
                _logger.LogWarning("⚠️ Error al actualizar preparación diaria: {Error}", response.Error);
                return Result<PreparacionDiariaDto>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar preparación diaria: {Id}", id);
                return Result<PreparacionDiariaDto>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una preparación diaria
        /// </summary>
        public async Task<Result> EliminarPreparacionDiariaAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("🗑️ Eliminando preparación diaria: {Id}", id);
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.DeleteAsync($"api/operaciones/preparaciones-diarias/{id}", token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparación diaria eliminada: {Id}", id);
                    return Result.Success();
                }
                
                _logger.LogWarning("⚠️ Error al eliminar preparación diaria: {Error}", response.Error);
                return Result.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar preparación diaria: {Id}", id);
                return Result.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Consume una cantidad específica de una preparación diaria
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> ConsumirPreparacionDiariaAsync(Guid id, int cantidad, string? observaciones = null)
        {
            try
            {
                _logger.LogInformation("🍽️ Consumiendo preparación diaria: {Id}, Cantidad: {Cantidad}", id, cantidad);
                
                var token = await _authService.GetTokenAsync();
                var command = new { Cantidad = cantidad, Observaciones = observaciones };
                var response = await _apiService.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/consumir", command, token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparación diaria consumida: {Id}, Cantidad restante: {Restante}", 
                        id, response.Data?.CantidadDisponible);
                    return Result<PreparacionDiariaDto>.Success(response.Data);
                }
                
                _logger.LogWarning("⚠️ Error al consumir preparación diaria: {Error}", response.Error);
                return Result<PreparacionDiariaDto>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consumir preparación diaria: {Id}", id);
                return Result<PreparacionDiariaDto>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Marca una preparación diaria como disponible
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> MarcarComoDisponibleAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("🟢 Marcando preparación diaria como disponible: {Id}", id);
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/disponible", new object(), token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparación diaria marcada como disponible: {Id}", id);
                    return Result<PreparacionDiariaDto>.Success(response.Data);
                }
                
                _logger.LogWarning("⚠️ Error al marcar como disponible: {Error}", response.Error);
                return Result<PreparacionDiariaDto>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al marcar como disponible: {Id}", id);
                return Result<PreparacionDiariaDto>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene preparaciones diarias por estado
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasPorEstadoAsync(string estado)
        {
            try
            {
                _logger.LogInformation("📋 Obteniendo preparaciones diarias por estado: {Estado}", estado);
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.GetAsync<List<PreparacionDiariaDto>>($"api/operaciones/preparaciones-diarias/por-estado?estado={estado}", token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparaciones diarias por estado obtenidas: {Count} preparaciones", response.Data?.Count ?? 0);
                    return Result<List<PreparacionDiariaDto>>.Success(response.Data ?? new List<PreparacionDiariaDto>());
                }
                
                _logger.LogWarning("⚠️ Error al obtener preparaciones por estado: {Error}", response.Error);
                return Result<List<PreparacionDiariaDto>>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparaciones por estado: {Estado}", estado);
                return Result<List<PreparacionDiariaDto>>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene preparaciones diarias por producto
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> GetPreparacionesDiariasPorProductoAsync(Guid productoId)
        {
            try
            {
                _logger.LogInformation("🏷️ Obteniendo preparaciones diarias por producto: {ProductoId}", productoId);
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.GetAsync<List<PreparacionDiariaDto>>($"api/operaciones/preparaciones-diarias/por-producto/{productoId}", token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Preparaciones diarias por producto obtenidas: {Count} preparaciones", response.Data?.Count ?? 0);
                    return Result<List<PreparacionDiariaDto>>.Success(response.Data ?? new List<PreparacionDiariaDto>());
                }
                
                _logger.LogWarning("⚠️ Error al obtener preparaciones por producto: {Error}", response.Error);
                return Result<List<PreparacionDiariaDto>>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparaciones por producto: {ProductoId}", productoId);
                return Result<List<PreparacionDiariaDto>>.Failure($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de preparaciones diarias
        /// </summary>
        public async Task<Result<EstadisticasPreparacionesDiariasDto>> GetEstadisticasAsync()
        {
            try
            {
                _logger.LogInformation("📊 Obteniendo estadísticas de preparaciones diarias");
                
                var token = await _authService.GetTokenAsync();
                var response = await _apiService.GetAsync<EstadisticasPreparacionesDiariasDto>("api/operaciones/preparaciones-diarias/estadisticas", token);
                
                if (response.Succeeded)
                {
                    _logger.LogInformation("✅ Estadísticas obtenidas: {TotalPreparaciones} preparaciones", response.Data?.TotalPreparaciones);
                    return Result<EstadisticasPreparacionesDiariasDto>.Success(response.Data);
                }
                
                _logger.LogWarning("⚠️ Error al obtener estadísticas: {Error}", response.Error);
                return Result<EstadisticasPreparacionesDiariasDto>.Failure(response.Error ?? "Error desconocido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas");
                return Result<EstadisticasPreparacionesDiariasDto>.Failure($"Error: {ex.Message}");
            }
        }
    }
} 