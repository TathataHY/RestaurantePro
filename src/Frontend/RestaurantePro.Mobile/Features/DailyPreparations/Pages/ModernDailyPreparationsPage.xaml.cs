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
}


