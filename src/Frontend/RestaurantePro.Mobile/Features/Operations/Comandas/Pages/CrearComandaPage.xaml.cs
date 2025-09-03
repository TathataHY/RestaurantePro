using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.Pages;

public partial class CrearComandaPage : ContentPage
{
    public CrearComandaPage(CrearComandaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CrearComandaViewModel viewModel)
        {
            // Obtener el ID de la mesa desde los parámetros de query
            var query = Shell.Current.CurrentState.Location.Query;
            var mesaId = System.Web.HttpUtility.ParseQueryString(query)["mesaId"];
            
            if (!string.IsNullOrEmpty(mesaId))
            {
                await viewModel.InitializeAsync(mesaId);
            }
        }
    }
}
