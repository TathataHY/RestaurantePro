using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Navigation;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de preparaciones diarias
    /// </summary>
    public partial class DailyPreparationsViewModel : ObservableObject
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

        public DailyPreparationsViewModel(
            IDailyPreparationsService dailyPreparationsService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _dailyPreparationsService = dailyPreparationsService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        #region Commands

        [RelayCommand]
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

        [RelayCommand]
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

        [RelayCommand]
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

        [RelayCommand]
        private async Task ConsumirPreparacionAsync(PreparacionDiariaDto preparacion)
        {
            if (preparacion == null) return;

            try
            {
                var cantidadStr = await _dialogService.ShowPromptAsync(
                    "Consumir Preparación",
                    $"Ingrese la cantidad a consumir de {preparacion.NombreProducto}",
                    "OK",
                    "Cancelar",
                    "",
                    -1,
                    "1");

                if (string.IsNullOrEmpty(cantidadStr))
                    return;

                if (!int.TryParse(cantidadStr, out int cantidad) || cantidad <= 0)
                {
                    await _dialogService.ShowErrorAsync("La cantidad debe ser un número positivo");
                    return;
                }

                if (cantidad > preparacion.CantidadDisponible)
                {
                    await _dialogService.ShowErrorAsync($"La cantidad no puede ser mayor a {preparacion.CantidadDisponible}");
                    return;
                }

                var result = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(preparacion.Id, cantidad);
                
                if (result.Succeeded)
                {
                    await _dialogService.ShowSuccessAsync("Preparación consumida exitosamente");
                    await LoadPreparacionesDiariasCommand.ExecuteAsync(null);
                }
                else
                {
                    await _dialogService.ShowErrorAsync(result.Error ?? "Error al consumir preparación");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Error al consumir preparación: {ex.Message}");
            }
        }

        [RelayCommand]
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
        private async Task VerDetalleAsync(PreparacionDiariaDto preparacion)
        {
            if (preparacion == null) return;

            // Navegar al detalle de la preparación
            // await _navigationService.NavigateToAsync($"dailypreparationdetail?id={preparacion.Id}");
        }

        [RelayCommand]
        private async Task CrearNuevaPreparacionAsync()
        {
            await _navigationService.NavigateToAsync("crear-preparacion-diaria");
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadPreparacionesDiariasCommand.ExecuteAsync(null);
            await LoadEstadisticasCommand.ExecuteAsync(null);
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