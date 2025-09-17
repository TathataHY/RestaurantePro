using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Productos.Pages;

/// <summary>
/// Página de detalle de producto con navegación paramétrica
/// </summary>
public partial class ProductoDetallePage : ContentPage, IQueryAttributable
{
    private readonly ProductoDetalleViewModel _viewModel;

    public ProductoDetallePage(ProductoDetalleViewModel viewModel)
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
        System.Diagnostics.Debug.WriteLine("🔍 [ProductoDetallePage] ApplyQueryAttributes llamado");
        
        if (query.TryGetValue("productoId", out var productoIdObj) && 
            productoIdObj is string productoIdStr &&
            Guid.TryParse(productoIdStr, out var productoId))
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ProductoDetallePage] Cargando producto con ID: {productoId}");
            
            // Cargar el producto cuando aparece la página
            Task.Run(async () =>
            {
                await _viewModel.LoadProductoCommand.ExecuteAsync(productoId);
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("❌ [ProductoDetallePage] No se pudo obtener productoId de los parámetros");
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