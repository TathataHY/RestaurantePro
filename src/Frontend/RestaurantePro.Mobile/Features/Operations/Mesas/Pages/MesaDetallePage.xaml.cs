using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Mesas.Pages;

/// <summary>
/// Página de detalle para una mesa específica
/// </summary>
public partial class MesaDetallePage : ContentPage, IQueryAttributable
{
    private readonly MesaDetalleViewModel _viewModel;

    public MesaDetallePage(MesaDetalleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Implementación de IQueryAttributable para recibir parámetros de navegación
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mesaId", out var mesaIdObj) && 
            Guid.TryParse(mesaIdObj?.ToString(), out var mesaIdParsed))
        {
            _viewModel.MesaId = mesaIdParsed;
        }
    }

    /// <summary>
    /// Se ejecuta cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    /// <summary>
    /// Se ejecuta cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
    }

    /// <summary>
    /// Maneja el tap en una comanda activa
    /// </summary>
    private async void OnComandaTapped(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OnComandaTapped ejecutado");
            
            // El sender es el Frame, no el TapGestureRecognizer
            if (sender is Frame frame && frame.BindingContext is RestaurantePro.Mobile.Core.Models.DTOs.ComandaDto comanda)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Comanda encontrada: {comanda.Id}");
                await _viewModel.VerComandaCommand.ExecuteAsync(comanda);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Sender es: {sender?.GetType().Name}, BindingContext es: {((sender as Frame)?.BindingContext)?.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Error en OnComandaTapped: {ex.Message}");
        }
    }
} 