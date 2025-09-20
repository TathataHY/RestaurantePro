using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Core.Attributes;
using RestaurantePro.Mobile.Core.Services.Dialog;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de preparaciones diarias
    /// </summary>
    public partial class DailyPreparationsViewModel : AuthorizedBaseViewModel
    {
        private readonly IDailyPreparationsService _dailyPreparationsService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<PreparacionDiariaDto> _preparacionesDiarias = new();

        [ObservableProperty]
        private PreparacionDiariaDto? _selectedPreparacion;

        [ObservableProperty]
        private EstadisticasPreparacionesDiariasDto? _estadisticas;

        [ObservableProperty]
        private string _filtroEstado = "Todos";

        [ObservableProperty]
        private bool _mostrarEstadisticas = true;

        [ObservableProperty]
        private bool _mostrarFiltros = false;

        [ObservableProperty]
        private string _textoBusqueda = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        // ===== 🔐 PROPIEDADES DE AUTORIZACIÓN =====
        [ObservableProperty]
        private bool _canMarcarDisponible;

        [ObservableProperty]
        private bool _canEliminarPreparacion;

        [ObservableProperty]
        private bool _canEditarPreparacion;

        [ObservableProperty]
        private bool _canCrearPreparacion;

        // 🔧 PROPIEDADES DIRECTAS PARA XAML (sin RelativeSource)
        public bool CanEditarPreparacionDirect => CanEditarPreparacion;
        public bool CanEliminarPreparacionDirect => CanEliminarPreparacion;
        public bool CanCrearPreparacionDirect => CanCrearPreparacion;

        // 🔧 LOGS ESPECÍFICOS PARA DEBUGGING
    public void LogCurrentPermissions()
    {
        var currentUser = _authService.GetCurrentUserAsync().Result;
        System.Diagnostics.Debug.WriteLine("🔧 [DailyPrepVM] === LOGS DE PERMISOS ACTUALES ===");
        System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Usuario actual: {currentUser?.Email}");
        System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Roles: [{string.Join(", ", currentUser?.Roles ?? new List<string>())}]");
        System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] CanCrearPreparacion: {CanCrearPreparacion}");
        System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] CanEditarPreparacion: {CanEditarPreparacion}");
        System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] CanEliminarPreparacion: {CanEliminarPreparacion}");
        System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] CanMarcarDisponible: {CanMarcarDisponible}");
        System.Diagnostics.Debug.WriteLine("🔧 [DailyPrepVM] === FIN LOGS DE PERMISOS ===");
    }
    
    public async Task<AuthUser?> GetCurrentUserAsync()
    {
        return await _authService.GetCurrentUserAsync();
    }

        public DailyPreparationsViewModel(
            IDailyPreparationsService dailyPreparationsService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAuthService authService,
            IAuthorizationService authorizationService,
            IAuthorizationValidator authorizationValidator,
            AuthorizationUIHelper authorizationUIHelper,
            ILogger<DailyPreparationsViewModel> logger) 
            : base(authorizationService, authorizationValidator, authorizationUIHelper, dialogService, logger)
        {
            _dailyPreparationsService = dailyPreparationsService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _authService = authService;
        }

        #region 🔐 Autorización

        /// <summary>
        /// Inicialización con autorización - configurar permisos y cargar datos
        /// </summary>
        protected override async Task OnAuthorizedInitializeAsync()
        {
            await ConfigurarPermisosUIAsync();
            
            // 🚀 OPTIMIZACIÓN: Cargar datos en paralelo para velocidad máxima
            var preparacionesTask = LoadPreparacionesDiariasCommand.ExecuteAsync(null);
            var estadisticasTask = LoadEstadisticasCommand.ExecuteAsync(null);
            
            // Esperar ambas tareas en paralelo
            await Task.WhenAll(preparacionesTask, estadisticasTask);
            
            System.Diagnostics.Debug.WriteLine($"⚡ [DailyPreparationsViewModel] Carga paralela completada - Preparaciones y estadísticas");
        }

        /// <summary>
        /// Configurar permisos de UI según el rol del usuario
        /// </summary>
        private async Task ConfigurarPermisosUIAsync()
        {
            try
            {
                // 🔍 DEBUG: Verificar usuario y roles ANTES de verificar permisos
                var user = await _authService.GetCurrentUserAsync();
                var userRoles = await AuthorizationService.GetUserRolesAsync();
                var userPermissions = await AuthorizationService.GetUserPermissionsAsync();
                
                System.Diagnostics.Debug.WriteLine("🔐 [DailyPrepVM] === DEBUG USUARIO Y ROLES ===");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] Usuario: {user?.Email}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] Roles del usuario: [{string.Join(", ", userRoles)}]");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] Total permisos: {userPermissions.Count}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] ¿Tiene ActualizarEstadoPreparaciones? {userPermissions.Contains(AppPermission.ActualizarEstadoPreparaciones)}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] ¿Tiene CompletarPreparaciones? {userPermissions.Contains(AppPermission.CompletarPreparaciones)}");

                // 🔐 Verificar permisos para preparaciones diarias
                CanMarcarDisponible = await HasPermissionAsync(AppPermission.CompletarPreparaciones);
                CanEliminarPreparacion = await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
                CanEditarPreparacion = await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
                CanCrearPreparacion = await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
                
                // 🔧 SOLUCIÓN TEMPORAL: Si es administrador pero no tiene permisos, forzarlos
                var isAdmin = userRoles.Contains("Administrador") || 
                              user?.Email?.Contains("admin", StringComparison.OrdinalIgnoreCase) == true;
                
                if (isAdmin && (!CanEliminarPreparacion || !CanEditarPreparacion || !CanCrearPreparacion))
                {
                    System.Diagnostics.Debug.WriteLine("🔧 [DailyPrepVM] Admin detectado sin permisos - Forzando todos los permisos");
                    CanMarcarDisponible = true;
                    CanEliminarPreparacion = true;
                    CanEditarPreparacion = true;
                    CanCrearPreparacion = true;
                }
                
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] === PERMISOS FINALES ===");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanMarcarDisponible: {CanMarcarDisponible}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanEliminarPreparacion: {CanEliminarPreparacion}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanEditarPreparacion: {CanEditarPreparacion}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanCrearPreparacion: {CanCrearPreparacion}");
                System.Diagnostics.Debug.WriteLine("🔐 [DailyPrepVM] === FIN PERMISOS ===");
                
                // 🔧 DEBUGGING ADICIONAL: Verificar si los permisos están siendo aplicados correctamente
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] === DEBUGGING PERMISOS UI ===");
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] ¿Es Mesero? {userRoles.Contains("Mesero")}");
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] ¿Es Cocinero? {userRoles.Contains("Cocinero")}");
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] ¿Es Administrador? {userRoles.Contains("Administrador")}");
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Los botones de editar/eliminar deberían estar visibles: {CanEditarPreparacion && CanEliminarPreparacion}");
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] === FIN DEBUGGING ===");
                
                // 🔧 FORZAR ACTUALIZACIÓN DE UI - MÚLTIPLES FORMAS
                OnPropertyChanged(nameof(CanMarcarDisponible));
                OnPropertyChanged(nameof(CanEliminarPreparacion));
                OnPropertyChanged(nameof(CanEditarPreparacion));
                OnPropertyChanged(nameof(CanCrearPreparacion));
                
                // 🔧 FORZAR ACTUALIZACIÓN ADICIONAL
                await Task.Delay(100); // Pequeña pausa para asegurar que se procese
                OnPropertyChanged(nameof(CanMarcarDisponible));
                OnPropertyChanged(nameof(CanEliminarPreparacion));
                OnPropertyChanged(nameof(CanEditarPreparacion));
                OnPropertyChanged(nameof(CanCrearPreparacion));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al configurar permisos UI en DailyPreparationsViewModel");
                // En caso de error, dar permisos completos al admin
                var user = await _authService.GetCurrentUserAsync();
                if (user?.Email?.Contains("admin", StringComparison.OrdinalIgnoreCase) == true)
                {
                    System.Diagnostics.Debug.WriteLine("🔧 [DailyPrepVM] Error en permisos - Forzando permisos de admin");
                    CanMarcarDisponible = true;
                    CanEliminarPreparacion = true;
                    CanEditarPreparacion = true;
                    CanCrearPreparacion = true;
                }
            }
        }

        #endregion

        #region Commands

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task RefreshAsync()
        {
            if (IsRefreshing) return;

            IsRefreshing = true;
            try
            {
                await LoadPreparacionesDiariasCommand.ExecuteAsync(null);
                await LoadEstadisticasCommand.ExecuteAsync(null);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task LoadPreparacionesDiariasAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();
                
                if (result.Succeeded)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 [DailyPrepVM] Recargando preparaciones - Antes: {PreparacionesDiarias.Count} items");
                    
                    PreparacionesDiarias.Clear();
                    foreach (var preparacion in result.Data)
                    {
                        PreparacionesDiarias.Add(preparacion);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ [DailyPrepVM] Preparaciones recargadas - Después: {PreparacionesDiarias.Count} items");
                    
                    // 🔧 Forzar actualización de UI
                    OnPropertyChanged(nameof(PreparacionesDiarias));
                }
                else
                {
                    await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Error inesperado: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task LoadEstadisticasAsync()
        {
            try
            {
                var result = await _dailyPreparationsService.GetEstadisticasAsync();
                
                if (result.Succeeded)
                {
                    Estadisticas = result.Data;
                }
                else
                {
                    await _dialogService.ShowErrorAsync(result.Error ?? "Error al cargar estadísticas");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Error al cargar estadísticas: {ex.Message}");
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task FiltrarPorEstadoAsync(string estado)
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                Result<List<PreparacionDiariaDto>> result;
                
                if (estado == "Todos")
                {
                    result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();
                }
                else
                {
                    result = await _dailyPreparationsService.GetPreparacionesDiariasPorEstadoAsync(estado);
                }

                if (result.Succeeded)
                {
                    PreparacionesDiarias.Clear();
                    foreach (var preparacion in result.Data)
                    {
                        PreparacionesDiarias.Add(preparacion);
                    }
                    FiltroEstado = estado;
                }
                else
                {
                    await _dialogService.ShowErrorAsync(result.Error ?? "Error al filtrar");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Error al filtrar: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // El consumo ahora se gestiona desde Comandas; se deja intencionalmente deshabilitado aquí
        [RelayCommand(CanExecute = nameof(CanNeverExecute))]
        private Task ConsumirPreparacionAsync(PreparacionDiariaDto preparacion)
            => Task.CompletedTask;

        private bool CanNeverExecute => false;

        [RelayCommand]
        [RequirePermission(AppPermission.CompletarPreparaciones)]
        private async Task MarcarComoDisponibleAsync(PreparacionDiariaDto preparacion)
        {
            if (preparacion == null) return;

            try
            {
                var confirmacion = await _dialogService.ShowConfirmationAsync(
                    "Marcar como Disponible",
                    $"¿Está seguro de marcar '{preparacion.NombreProducto}' como disponible?");

                if (!confirmacion)
                    return;

                var result = await _dailyPreparationsService.MarcarComoDisponibleAsync(preparacion.Id);
                
                if (result.Succeeded)
                {
                    await _dialogService.ShowSuccessAsync("Preparación marcada como disponible");
                    await LoadPreparacionesDiariasCommand.ExecuteAsync(null);
                }
                else
                {
                    await _dialogService.ShowErrorAsync(result.Error ?? "Error al marcar como disponible");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Error al marcar como disponible: {ex.Message}");
            }
        }

        [RelayCommand]
        [RequirePermission(AppPermission.ActualizarEstadoPreparaciones)]
        private async Task EliminarPreparacionAsync(PreparacionDiariaDto preparacion)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] EliminarPreparacionAsync - INICIANDO");
            System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Preparación: {preparacion?.NombreProducto}");
            
            // 🔐 VALIDACIÓN MANUAL DE PERMISOS
            if (!await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones))
            {
                System.Diagnostics.Debug.WriteLine($"🔐❌ [DailyPrepVM] ACCESO DENEGADO - Usuario no tiene permisos para eliminar preparaciones");
                await _dialogService.ShowErrorAsync("No tiene permisos para eliminar preparaciones");
                return;
            }
            
            if (preparacion == null) 
            {
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Preparación es NULL - SALIENDO");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Mostrando confirmación para eliminar: {preparacion.NombreProducto}");
                
                var confirmacion = await _dialogService.ShowConfirmationAsync(
                    "Eliminar Preparación",
                    $"¿Está seguro de eliminar '{preparacion.NombreProducto}'?");

                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Confirmación: {confirmacion}");

                if (!confirmacion)
                    return;

                var result = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(preparacion.Id);
                
                if (result.Succeeded)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ [DailyPrepVM] Preparación eliminada exitosamente, actualizando UI...");
                    
                    // 🚀 OPTIMIZACIÓN: Remover de la colección inmediatamente (UI responsiva)
                    var preparacionAEliminar = PreparacionesDiarias.FirstOrDefault(p => p.Id == preparacion.Id);
                    if (preparacionAEliminar != null)
                    {
                        PreparacionesDiarias.Remove(preparacionAEliminar);
                        System.Diagnostics.Debug.WriteLine($"✅ [DailyPrepVM] Preparación removida de la colección UI");
                    }
                    
                    await _dialogService.ShowSuccessAsync("Preparación eliminada exitosamente");
                    
                    // 🔄 Recargar lista y estadísticas para asegurar sincronización
                    var recargarTask = LoadPreparacionesDiariasCommand.ExecuteAsync(null);
                    var estadisticasTask = LoadEstadisticasCommand.ExecuteAsync(null);
                    
                    await Task.WhenAll(recargarTask, estadisticasTask);
                    
                    System.Diagnostics.Debug.WriteLine($"✅ [DailyPrepVM] Lista y estadísticas recargadas después de eliminar");
                }
                else
                {
                    await _dialogService.ShowErrorAsync(result.Error ?? "Error al eliminar preparación");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Error al eliminar preparación: {ex.Message}");
            }
        }

        [RelayCommand]
        [RequirePermission(AppPermission.ActualizarEstadoPreparaciones)]
        private async Task EditarPreparacionAsync(PreparacionDiariaDto preparacion)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] EditarPreparacionAsync - INICIANDO");
            System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Preparación: {preparacion?.NombreProducto}");
            
            // 🔐 VALIDACIÓN MANUAL DE PERMISOS
            if (!await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones))
            {
                System.Diagnostics.Debug.WriteLine($"🔐❌ [DailyPrepVM] ACCESO DENEGADO - Usuario no tiene permisos para editar preparaciones");
                await _dialogService.ShowErrorAsync("No tiene permisos para editar preparaciones");
                return;
            }
            
            if (preparacion == null) 
            {
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Preparación es NULL - SALIENDO");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Navegando a editar preparación ID: {preparacion.Id}");
                
                await _navigationService.NavigateToAsync("editar-preparacion-diaria", new Dictionary<string, object>
                {
                    { "id", preparacion.Id }
                });
                
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] Navegación completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 [DailyPrepVM] ERROR: {ex.Message}");
                await _dialogService.ShowErrorAsync($"Error al navegar: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task VerDetalleAsync(PreparacionDiariaDto preparacion)
        {
            if (preparacion == null) return;

            // Navegar al detalle de la preparación
            // await _navigationService.NavigateToAsync($"dailypreparationdetail?id={preparacion.Id}");
        }

        [RelayCommand]
        [RequirePermission(AppPermission.ActualizarEstadoPreparaciones)]
        private async Task CrearNuevaPreparacionAsync()
        {
            // 🔐 VALIDACIÓN MANUAL DE PERMISOS
            if (!await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones))
            {
                System.Diagnostics.Debug.WriteLine($"🔐❌ [DailyPrepVM] ACCESO DENEGADO - Usuario no tiene permisos para crear preparaciones");
                await _dialogService.ShowErrorAsync("No tiene permisos para crear preparaciones");
                return;
            }
            
            await _navigationService.NavigateToAsync("crear-preparacion-diaria");
        }

        

        [RelayCommand]
        private void BuscarPreparaciones()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                LoadPreparacionesDiariasCommand.Execute(null);
                return;
            }

            var preparacionesFiltradas = PreparacionesDiarias
                .Where(p => p.NombreProducto.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                           p.NombreChef.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                           p.Observaciones.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();

            PreparacionesDiarias.Clear();
            foreach (var preparacion in preparacionesFiltradas)
            {
                PreparacionesDiarias.Add(preparacion);
            }
        }

        [RelayCommand]
        private void LimpiarBusqueda()
        {
            TextoBusqueda = string.Empty;
            LoadPreparacionesDiariasCommand.Execute(null);
        }

        [RelayCommand]
        private void ToggleEstadisticas()
        {
            MostrarEstadisticas = !MostrarEstadisticas;
        }

        [RelayCommand]
        private void ShowFiltros()
        {
            MostrarFiltros = !MostrarFiltros;
        }

        #endregion
    }
} 