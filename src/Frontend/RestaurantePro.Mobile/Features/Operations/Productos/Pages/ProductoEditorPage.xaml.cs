using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Features.Operations.Productos.Pages;

public partial class ProductoEditorPage : ContentPage, IQueryAttributable
{
    private readonly ProductoEditorViewModel _viewModel;

    public ProductoEditorPage(ProductoEditorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Cargar categorías al abrir
        _ = _viewModel.CargarCategoriasCommand.ExecuteAsync(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // productoId opcional; si llega, es edición
        if (query.TryGetValue("productoId", out var idObj) && idObj is string idStr && Guid.TryParse(idStr, out var id))
        {
            _ = _viewModel.CargarParaEdicionCommand.ExecuteAsync(id);
        }
        else
        {
            // modo crear
            _viewModel.EsEdicion = false;
            _viewModel.Title = "Crear producto";
            _viewModel.PrimaryButtonText = "Crear";
        }
    }

    private void Picker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is CategoriaProductoDto categoria)
        {
            _viewModel.CategoriaId = categoria.Id;
        }
    }
}


