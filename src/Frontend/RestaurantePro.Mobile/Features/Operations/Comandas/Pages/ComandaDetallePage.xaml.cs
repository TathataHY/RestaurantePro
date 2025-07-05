using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.Pages;

/// <summary>
/// Página de detalle de comanda con navegación paramétrica
/// </summary>
public partial class ComandaDetallePage : ContentPage, IQueryAttributable
{
    private readonly ComandaDetalleViewModel _viewModel;

    public ComandaDetallePage(ComandaDetalleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Aplicar parámetros de consulta recibidos durante la navegación
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("comandaId", out var comandaIdObj) && 
            comandaIdObj is string comandaIdStr &&
            Guid.TryParse(comandaIdStr, out var comandaId))
        {
            // Cargar la comanda cuando aparece la página
            Task.Run(async () =>
            {
                await _viewModel.LoadComandaCommand.ExecuteAsync(comandaId);
            });
        }
    }

    /// <summary>
    /// Limpiar datos cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
    }
} 