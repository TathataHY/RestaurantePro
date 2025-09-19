using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authorization;
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
        private bool canMarcarDisponible;

        [ObservableProperty]
        private bool canEliminarPreparacion;

        [ObservableProperty]
        private bool canEditarPreparacion;

        [ObservableProperty]
        private bool canCrearPreparacion;

        public DailyPreparationsViewModel(
            IDailyPreparationsService dailyPreparationsService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAuthorizationService authorizationService,
            IAuthorizationValidator authorizationValidator,
            AuthorizationUIHelper authorizationUIHelper,
            ILogger<DailyPreparationsViewModel> logger) 
            : base(authorizationService, authorizationValidator, authorizationUIHelper, dialogService, logger)
        {
            _dailyPreparationsService = dailyPreparationsService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        #region 🔐 Autorización

        /// <summary>
        /// Inicialización con autorización - configurar permisos y cargar datos
        /// </summary>
        protected override async Task OnAuthorizedInitializeAsync()
        {
            await ConfigurarPermisosUIAsync();
            
            // Cargar datos iniciales después de configurar permisos
            await LoadPreparacionesDiariasCommand.ExecuteAsync(null);
            await LoadEstadisticasCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Configurar permisos de UI según el rol del usuario
        /// </summary>
        private async Task ConfigurarPermisosUIAsync()
        {
            try
            {
                // 🔐 Verificar permisos para preparaciones diarias
                CanMarcarDisponible = await HasPermissionAsync(AppPermission.CompletarPreparaciones);
                CanEliminarPreparacion = await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
                CanEditarPreparacion = await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
                CanCrearPreparacion = await HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);

                System.Diagnostics.Debug.WriteLine("🔐 [DailyPrepVM] === PERMISOS PREPARACIONES ===");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanMarcarDisponible: {CanMarcarDisponible}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanEliminarPreparacion: {CanEliminarPreparacion}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanEditarPreparacion: {CanEditarPreparacion}");
                System.Diagnostics.Debug.WriteLine($"🔐 [DailyPrepVM] CanCrearPreparacion: {CanCrearPreparacion}");
                System.Diagnostics.Debug.WriteLine("🔐 [DailyPrepVM] === FIN PERMISOS ===");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al configurar permisos UI en DailyPreparationsViewModel");
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
                    PreparacionesDiarias.Clear();
                    foreach (var preparacion in result.Data)
                    {
                        PreparacionesDiarias.Add(preparacion);
                    }
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
            if (preparacion == null) return;

            try
            {
                var confirmacion = await _dialogService.ShowConfirmationAsync(
                    "Eliminar Preparación",
                    $"¿Está seguro de eliminar '{preparacion.NombreProducto}'?");

                if (!confirmacion)
                    return;

                var result = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(preparacion.Id);
                
                if (result.Succeeded)
                {
                    await _dialogService.ShowSuccessAsync("Preparación eliminada exitosamente");
                    await LoadPreparacionesDiariasCommand.ExecuteAsync(null);
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
            if (preparacion == null) return;
            await _navigationService.NavigateToAsync("editar-preparacion-diaria", new Dictionary<string, object>
            {
                { "id", preparacion.Id }
            });
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