using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

namespace RestaurantePro.Mobile.Features.DailyPreparations.Pages;

public partial class CreateDailyPreparationPage : ContentPage
{
    public CreateDailyPreparationPage(CreateDailyPreparationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CreateDailyPreparationViewModel vm)
        {
            if (vm.Categorias.Count == 0)
            {
                await vm.CargarCategoriasCommand.ExecuteAsync(null);
            }
            if (vm.Productos.Count == 0)
            {
                await vm.BuscarProductosCommand.ExecuteAsync(null);
            }
        }
    }
}


