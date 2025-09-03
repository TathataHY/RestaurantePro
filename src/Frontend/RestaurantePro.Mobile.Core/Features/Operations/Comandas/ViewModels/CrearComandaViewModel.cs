using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

public partial class CrearComandaViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly IProductosService _productosService;
    private readonly IMesasService _mesasService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private MesaDto _mesa = new();

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private string _observaciones = string.Empty;

    public CrearComandaViewModel(
        IComandasService comandasService,
        IProductosService productosService,
        IMesasService mesasService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _comandasService = comandasService;
        _productosService = productosService;
        _mesasService = mesasService;
        _navigationService = navigationService;
        _dialogService = dialogService;

        ProductosDisponibles = new ObservableCollection<ProductoCarritoDto>();
        ProductosCarrito = new ObservableCollection<ProductoCarritoDto>();

        // Los comandos se generan automáticamente con [RelayCommand]
    }

    #region Propiedades

    public string MesaInfo => Mesa != null ? $"Mesa {Mesa.Numero} - {Mesa.Ubicacion} (Capacidad: {Mesa.Capacidad})" : string.Empty;

    public bool PuedeCrearComanda => ProductosCarrito.Any() && !IsLoading;

    public decimal TotalCarrito => ProductosCarrito.Sum(p => p.Subtotal);

    public ObservableCollection<ProductoCarritoDto> ProductosDisponibles { get; }
    public ObservableCollection<ProductoCarritoDto> ProductosCarrito { get; }

    #endregion

    #region Comandos

    [RelayCommand]
    private async Task BuscarProductosAsync()
    {
        try
        {
            IsLoading = true;
            
            ApiResponse<List<ProductoDto>> result;
            
            // Si hay texto de búsqueda, usar el método de búsqueda
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                result = await _productosService.BuscarProductosAsync(TextoBusqueda);
            }
            else
            {
                // Si no hay texto de búsqueda, cargar todos los productos paginados (solo activos)
                result = await _productosService.ObtenerProductosPaginadosAsync(1, 100, null, true);
            }
            
            if (result.Success && result.Data != null)
            {
                ProductosDisponibles.Clear();
                
                foreach (var producto in result.Data)
                {
                    var productoCarrito = new ProductoCarritoDto
                    {
                        Id = producto.Id.ToString(),
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        Precio = producto.Precio,
                        Cantidad = 0
                    };
                    
                    ProductosDisponibles.Add(productoCarrito);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al buscar productos: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void IncrementarCantidad(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            producto.Cantidad++;
        }
    }

    [RelayCommand]
    private void DecrementarCantidad(ProductoCarritoDto producto)
    {
        if (producto != null && producto.Cantidad > 0)
        {
            producto.Cantidad--;
        }
    }

    [RelayCommand]
    private void AgregarAlCarrito(ProductoCarritoDto producto)
    {
        if (producto != null && producto.Cantidad > 0)
        {
            // Verificar si el producto ya está en el carrito
            var productoExistente = ProductosCarrito.FirstOrDefault(p => p.Id == producto.Id);
            
            if (productoExistente != null)
            {
                // Si ya existe, incrementar la cantidad
                productoExistente.Cantidad += producto.Cantidad;
            }
            else
            {
                // Si no existe, agregarlo al carrito
                var nuevoProducto = new ProductoCarritoDto
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Cantidad = producto.Cantidad
                };
                
                ProductosCarrito.Add(nuevoProducto);
            }
            
            // Resetear la cantidad en la lista de productos disponibles
            producto.Cantidad = 0;
        }
    }

    [RelayCommand]
    private void IncrementarCantidadCarrito(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            producto.Cantidad++;
        }
    }

    [RelayCommand]
    private void DecrementarCantidadCarrito(ProductoCarritoDto producto)
    {
        if (producto != null && producto.Cantidad > 1)
        {
            producto.Cantidad--;
        }
    }

    [RelayCommand]
    private void EliminarDelCarrito(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            ProductosCarrito.Remove(producto);
        }
    }

    [RelayCommand]
    private async Task CrearComandaAsync()
    {
        try
        {
            if (!PuedeCrearComanda)
            {
                await _dialogService.ShowAlertAsync("Error", "Debe seleccionar al menos un producto");
                return;
            }

            var confirmacion = await _dialogService.ShowConfirmAsync(
                "Confirmar Comanda",
                $"¿Está seguro de crear la comanda para la Mesa {Mesa.Numero} con {ProductosCarrito.Count} productos por un total de ${TotalCarrito:F2}?");
            
            if (!confirmacion)
                return;

            IsLoading = true;

            var comandaRequest = new ComandaModels.CrearComandaRequest
            {
                MesaId = Mesa.Id.ToString(),
                Observaciones = Observaciones,
                Productos = ProductosCarrito.Select(p => new ComandaModels.ProductoComandaRequest
                {
                    ProductoId = p.Id,
                    Cantidad = p.Cantidad,
                    Precio = p.Precio
                }).ToList()
            };

            var result = await _comandasService.CrearComandaAsync(comandaRequest);
            
            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Comanda creada exitosamente");
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message ?? "Error al crear la comanda");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al crear la comanda: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Cancelar",
            "¿Está seguro de cancelar la creación de la comanda? Se perderán todos los productos seleccionados.");
        
        if (confirmacion)
        {
            await _navigationService.GoBackAsync();
        }
    }

    #endregion

    #region Métodos Públicos

    public async Task InitializeAsync(string mesaId)
    {
        try
        {
            IsLoading = true;
            
            if (!string.IsNullOrEmpty(mesaId))
            {
                await CargarMesaAsync(mesaId);
                await BuscarProductosAsync();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al inicializar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Métodos Privados

    private async Task CargarMesaAsync(string mesaId)
    {
        try
        {
            var result = await _mesasService.ObtenerMesaAsync(Guid.Parse(mesaId));
            if (result.Success && result.Data != null)
            {
                Mesa = result.Data;
                // Notificar a la UI que la propiedad Mesa cambió
                OnPropertyChanged(nameof(Mesa));
                OnPropertyChanged(nameof(MesaInfo));
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "No se pudo cargar la información de la mesa");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar la mesa: {ex.Message}");
        }
    }

    #endregion
}
