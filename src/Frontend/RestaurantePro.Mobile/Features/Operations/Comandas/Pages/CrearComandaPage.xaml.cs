using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.Pages;

/// <summary>
/// Página para crear una nueva comanda
/// </summary>
public partial class CrearComandaPage : ContentPage, IQueryAttributable
{
    private readonly CrearComandaViewModel _viewModel;

    public CrearComandaPage(CrearComandaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Implementación de IQueryAttributable para recibir parámetros de navegación
    /// </summary>
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mesaId", out var mesaIdObj) && 
            !string.IsNullOrEmpty(mesaIdObj?.ToString()))
        {
            var mesaId = mesaIdObj.ToString();
            await _viewModel.InitializeAsync(mesaId);
        }
    }

    /// <summary>
    /// Se ejecuta cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // La inicialización se maneja en ApplyQueryAttributes
    }
}
