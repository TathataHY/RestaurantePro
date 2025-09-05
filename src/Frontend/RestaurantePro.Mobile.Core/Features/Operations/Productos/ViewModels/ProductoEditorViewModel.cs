using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;

/// <summary>
/// ViewModel para crear/editar productos
/// </summary>
public partial class ProductoEditorViewModel : BaseViewModel
{
    private readonly IProductosService _productosService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string descripcion = string.Empty;

    [ObservableProperty]
    private decimal precio;

    [ObservableProperty]
    private Guid categoriaId;

    [ObservableProperty]
    private bool activo = true;

    [ObservableProperty]
    private bool esEdicion;

    [ObservableProperty]
    private string primaryButtonText = "Crear";

    [ObservableProperty]
    private ObservableCollection<CategoriaProductoDto> categorias = new();

    public ProductoEditorViewModel(
        IProductosService productosService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _productosService = productosService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        Title = "Producto";
    }

    [RelayCommand]
    public async Task CargarCategoriasAsync()
    {
        try
        {
            var result = await _productosService.ObtenerCategoriasAsync();
            if (result.Success && result.Data != null)
            {
                Categorias.Clear();
                foreach (var c in result.Data)
                {
                    Categorias.Add(c);
                }
            }
        }
        catch
        {
            // Ignorar; mostramos formulario igualmente
        }
    }

    [RelayCommand]
    public async Task CargarParaEdicionAsync(Guid productoId)
    {
        if (productoId == Guid.Empty) return;

        try
        {
            IsBusy = true;
            var result = await _productosService.ObtenerProductoPorIdAsync(productoId);
            if (result.Success && result.Data != null)
            {
                var p = result.Data;
                Id = p.Id;
                Nombre = p.Nombre;
                Descripcion = p.Descripcion;
                Precio = p.Precio;
                CategoriaId = p.CategoriaId;
                Activo = p.Activo;
                EsEdicion = true;
                Title = "Editar producto";
                PrimaryButtonText = "Guardar";
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Message ?? "No se pudo cargar el producto");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            _ = _dialogService.ShowAlertAsync("Validación", "El nombre es obligatorio");
            return false;
        }
        if (Precio <= 0)
        {
            _ = _dialogService.ShowAlertAsync("Validación", "El precio debe ser mayor a 0");
            return false;
        }
        if (CategoriaId == Guid.Empty)
        {
            _ = _dialogService.ShowAlertAsync("Validación", "Selecciona una categoría");
            return false;
        }
        return true;
    }

    [RelayCommand]
    public async Task GuardarAsync()
    {
        if (!Validar()) return;

        try
        {
            IsBusy = true;
            if (EsEdicion)
            {
                var request = new ActualizarProductoRequest
                {
                    Id = Id,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    Precio = Precio,
                    CategoriaId = CategoriaId,
                    Activo = Activo
                };
                var result = await _productosService.ActualizarProductoAsync(request);
                if (!result.Success)
                {
                    await _dialogService.ShowErrorAsync(result.Message ?? "No se pudo actualizar el producto");
                    return;
                }
            }
            else
            {
                var request = new CrearProductoRequest
                {
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    Precio = Precio,
                    CategoriaId = CategoriaId,
                    Activo = Activo
                };
                var result = await _productosService.CrearProductoAsync(request);
                if (!result.Success)
                {
                    await _dialogService.ShowErrorAsync(result.Message ?? "No se pudo crear el producto");
                    return;
                }
            }

            await _dialogService.ShowSuccessAsync("Producto guardado");
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al guardar: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CancelarAsync()
    {
        await _navigationService.GoBackAsync();
    }
}


