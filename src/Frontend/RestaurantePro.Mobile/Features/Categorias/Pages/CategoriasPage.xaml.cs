using RestaurantePro.Mobile.Core.Features.Categorias.ViewModels;

namespace RestaurantePro.Mobile.Features.Categorias.Pages;

/// <summary>
/// Página para mostrar categorías de productos
/// </summary>
public partial class CategoriasPage : ContentPage
{
    private readonly CategoriasViewModel _viewModel;

    public CategoriasPage(CategoriasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
    }

    private void OnMostrarSoloActivasChanged(object sender, CheckedChangedEventArgs e)
    {
        // El ViewModel maneja la lógica del filtro
        // Este método solo existe para satisfacer el binding del XAML
    }

    /// <summary>
    /// Maneja el clic en el botón "Ver Productos"
    /// </summary>
    private async void OnVerProductosClicked(object sender, EventArgs e)
    {
        try
        {
            // Obtener el contexto de datos del botón (la categoría)
            if (sender is Button button && button.BindingContext is RestaurantePro.Mobile.Core.Models.DTOs.CategoriaProductoDto categoria)
            {
                // Llamar al comando del ViewModel
                await _viewModel.SeleccionarCategoriaCommand.ExecuteAsync(categoria);
            }
        }
        catch (Exception ex)
        {
            // Manejar errores de navegación
            await DisplayAlert("Error", $"No se pudo navegar a los productos: {ex.Message}", "OK");
        }
    }
} 