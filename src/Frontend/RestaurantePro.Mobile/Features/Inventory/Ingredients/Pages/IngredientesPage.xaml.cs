using RestaurantePro.Mobile.Core.Features.Inventory.Ingredients.ViewModels;

namespace RestaurantePro.Mobile.Features.Inventory.Ingredients.Pages;

/// <summary>
/// Página para gestión de ingredientes
/// </summary>
public partial class IngredientesPage : ContentPage
{
    private readonly IngredientesViewModel _viewModel;

    public IngredientesPage(IngredientesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarIngredientesCommand.ExecuteAsync(null);
    }

    private void OnSoloDisponiblesChanged(object sender, CheckedChangedEventArgs e)
    {
        // El ViewModel maneja la lógica del filtro
        // Este método solo existe para satisfacer el binding del XAML
    }

    private void OnSoloBajoStockChanged(object sender, CheckedChangedEventArgs e)
    {
        // El ViewModel maneja la lógica del filtro
        // Este método solo existe para satisfacer el binding del XAML
    }
} 