using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using System.ComponentModel;

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
        
        // Suscribirse a cambios en el ViewModel para controlar visibilidad
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
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
    /// Controlar visibilidad de información de entrega cuando cambian las propiedades del ViewModel
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.Comanda))
        {
            UpdateDeliveryInfoVisibility();
        }
    }

    /// <summary>
    /// Actualizar visibilidad del frame de información de entrega
    /// </summary>
    private void UpdateDeliveryInfoVisibility()
    {
        var esDelivery = _viewModel.Comanda?.Tipo == "Delivery" || _viewModel.Comanda?.Tipo == "2";
        DeliveryInfoFrame.IsVisible = esDelivery;
        System.Diagnostics.Debug.WriteLine($"🔍 [ComandaDetallePage] UpdateDeliveryInfoVisibility - Tipo: {_viewModel.Comanda?.Tipo}, EsDelivery: {esDelivery}");
    }

    /// <summary>
    /// Limpiar datos cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.Cleanup();
    }
} 