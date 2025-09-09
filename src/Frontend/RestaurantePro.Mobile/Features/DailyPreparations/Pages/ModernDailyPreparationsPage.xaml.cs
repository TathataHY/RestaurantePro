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

    protected override void OnAppearing()
    {
        base.OnAppearing();
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
        await _viewModel.LoadPreparacionesDiariasCommand.ExecuteAsync(null);
        await _viewModel.LoadEstadisticasCommand.ExecuteAsync(null);
    }

    // Sin manejador de tabs inferior en esta vista

    private async void OnFilterTabSelected(int tabIndex)
    {
        var tabName = FilterTabNavigation.Tabs[tabIndex];
        await _viewModel.FiltrarPorEstadoCommand.ExecuteAsync(tabName);
    }

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is RestaurantePro.Mobile.Core.Models.DTOs.PreparacionDiariaDto prep)
        {
            await _viewModel.EditarPreparacionCommand.ExecuteAsync(prep);
        }
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is RestaurantePro.Mobile.Core.Models.DTOs.PreparacionDiariaDto prep)
        {
            await _viewModel.EliminarPreparacionCommand.ExecuteAsync(prep);
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


