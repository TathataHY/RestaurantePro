using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Inventory.Ingredients.ViewModels;

/// <summary>
/// ViewModel para gestión de ingredientes
/// </summary>
public partial class IngredientesViewModel : BaseViewModel
{
    private readonly IIngredientesService _ingredientesService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<IngredienteSummaryDto> ingredientes = new();

    [ObservableProperty]
    private IngredienteSummaryDto? ingredienteSeleccionado;

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private bool _soloDisponibles = false;

    [ObservableProperty]
    private bool _soloBajoStock = false;

    [ObservableProperty]
    private int _totalIngredientes;

    [ObservableProperty]
    private int _ingredientesDisponibles;

    [ObservableProperty]
    private int _ingredientesBajoStock;

    #endregion

    public IngredientesViewModel(
        IIngredientesService ingredientesService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _ingredientesService = ingredientesService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    #region Comandos

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        await CargarIngredientesAsync();
        await CargarEstadisticasAsync();
    }

    [RelayCommand]
    private async Task CargarIngredientesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _ingredientesService.ObtenerIngredientesAsync(SoloDisponibles);
            
            if (response.Success && response.Data != null)
            {
                Ingredientes.Clear();
                foreach (var ingrediente in response.Data)
                {
                    Ingredientes.Add(ingrediente);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar ingredientes");
            }
        });
    }

    [RelayCommand]
    private async Task BuscarIngredientesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _ingredientesService.BuscarIngredientesAsync(Busqueda);
            
            if (response.Success && response.Data != null)
            {
                Ingredientes.Clear();
                foreach (var ingrediente in response.Data)
                {
                    Ingredientes.Add(ingrediente);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al buscar ingredientes");
            }
        });
    }

    [RelayCommand]
    private async Task CambiarFiltroDisponiblesAsync()
    {
        await CargarIngredientesAsync();
    }

    [RelayCommand]
    private async Task CambiarFiltroBajoStockAsync()
    {
        await CargarIngredientesAsync();
    }

    [RelayCommand]
    private async Task SeleccionarIngredienteAsync(IngredienteSummaryDto ingrediente)
    {
        if (ingrediente == null) return;

        IngredienteSeleccionado = ingrediente;
        // Navegar a detalle de ingrediente
        await _navigationService.NavigateToAsync("ingredientedetalle", new Dictionary<string, object>
        {
            { "ingredienteId", ingrediente.Id }
        });
    }

    [RelayCommand]
    private async Task VerMovimientosAsync(IngredienteSummaryDto ingrediente)
    {
        if (ingrediente == null) return;

        await ExecuteAsync(async () =>
        {
            var response = await _ingredientesService.ObtenerMovimientosAsync(ingrediente.Id);
            
            if (response.Success && response.Data != null)
            {
                // Navegar a movimientos del ingrediente
                await _navigationService.NavigateToAsync("movimientosingrediente", new Dictionary<string, object>
                {
                    { "ingredienteId", ingrediente.Id },
                    { "movimientos", response.Data }
                });
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar movimientos");
            }
        });
    }

    [RelayCommand]
    private async Task VerBajoStockAsync()
    {
        SoloBajoStock = true;
        await CargarIngredientesAsync();
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        await CargarIngredientesAsync();
        await CargarEstadisticasAsync();
    }

    #endregion

    #region Métodos Privados

    private async Task CargarEstadisticasAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _ingredientesService.ObtenerEstadisticasAsync();
            
            if (response.Success && response.Data != null)
            {
                TotalIngredientes = response.Data.TotalIngredientes;
                IngredientesDisponibles = response.Data.IngredientesDisponibles;
                IngredientesBajoStock = response.Data.IngredientesBajoStock;
            }
        }, showLoading: false);
    }

    #endregion
} 