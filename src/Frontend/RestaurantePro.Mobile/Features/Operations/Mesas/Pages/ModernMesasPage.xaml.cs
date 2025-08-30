using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Mesas.Pages;

/// <summary>
/// Página moderna de gestión de mesas - V4 Modernización Visual
/// </summary>
public partial class ModernMesasPage : ContentPage
{
    private MesasViewModel _viewModel;

    public ModernMesasPage(MesasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar animaciones de entrada
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Animaciones de entrada simples
        await Task.Delay(100);
        
        // Animar la página completa
        await this.FadeTo(1, 500);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refrescar datos cuando la página aparece
        if (_viewModel != null)
        {
            _viewModel.RefreshMesasCommand?.Execute(null);
        }
    }
} 