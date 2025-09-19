using RestaurantePro.Mobile.Core.Features.Operations.Cocina.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Cocina.Pages;

public partial class ModernCocinaPage : ContentPage
{
    private readonly ModernCocinaViewModel _viewModel;

    public ModernCocinaPage(ModernCocinaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // 🔐 IMPORTANTE: Inicializar autorización ANTES de cargar datos
        await _viewModel.InitializeWithAuthorizationAsync();
    }
}
