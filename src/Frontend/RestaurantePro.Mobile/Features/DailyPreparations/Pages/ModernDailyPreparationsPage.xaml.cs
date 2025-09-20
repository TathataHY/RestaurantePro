using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Features.DailyPreparations.Pages;

public partial class ModernDailyPreparationsPage : ContentPage
{
    private readonly DailyPreparationsViewModel _viewModel;

    public ModernDailyPreparationsPage(DailyPreparationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        SetupFilterTabs();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // 🔐 IMPORTANTE: Inicializar autorización ANTES de cargar datos
        await _viewModel.InitializeWithAuthorizationAsync();
        
        _ = InitializeOnAppearAsync();
    }

    // BottomTabBar deshabilitado en esta vista (usamos tabs nativos del Shell)

    private void SetupFilterTabs()
    {
        FilterTabNavigation.Tabs = new ObservableCollection<string>
        {
            "Todos",
            "Disponibles",
            "Preparando",
            "Agotados"
        };
        FilterTabNavigation.TabSelectedCommand = new Command<int>(OnFilterTabSelected);
    }

    private async Task InitializeOnAppearAsync()
    {
        await Task.Delay(100);
        if (MainFab != null)
        {
            await MainFab.ScaleTo(1, 400, Easing.BounceOut);
        }
        // Los datos ya se cargan en InitializeWithAuthorizationAsync
    }

    // Sin manejador de tabs inferior en esta vista

    private async void OnFilterTabSelected(int tabIndex)
    {
        var tabName = FilterTabNavigation.Tabs[tabIndex];
        await _viewModel.FiltrarPorEstadoCommand.ExecuteAsync(tabName);
    }

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantePro.Mobile.Core.Models.DTOs.PreparacionDiariaDto preparacion)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] OnEditarClicked - Preparación: {preparacion?.NombreProducto}");
            await _viewModel.EditarPreparacionCommand.ExecuteAsync(preparacion);
        }
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantePro.Mobile.Core.Models.DTOs.PreparacionDiariaDto preparacion)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] OnEliminarClicked - Preparación: {preparacion?.NombreProducto}");
            await _viewModel.EliminarPreparacionCommand.ExecuteAsync(preparacion);
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        // Garantizar apagado del refresco incluso si hay errores/red lenta
        try
        {
            await _viewModel.RefreshCommand.ExecuteAsync(null);
        }
        finally
        {
            // Failsafe de seguridad
            await Task.Delay(100);
            RefreshControl.IsRefreshing = false;
        }
    }
}


