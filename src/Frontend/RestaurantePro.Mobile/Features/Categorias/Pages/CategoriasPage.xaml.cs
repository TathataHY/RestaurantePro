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
} 