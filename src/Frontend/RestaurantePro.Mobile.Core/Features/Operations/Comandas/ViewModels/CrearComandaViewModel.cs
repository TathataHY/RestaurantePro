using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Models.Common;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

public partial class CrearComandaViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly IProductosService _productosService;
    private readonly IMesasService _mesasService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly Dictionary<Guid, int> _cantidadesOriginales = new();
    private readonly HashSet<Guid> _productosEliminados = new();

    [ObservableProperty]
    private MesaDto _mesa = new();

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private string _observaciones = string.Empty;

    [ObservableProperty]
    private bool _esEdicion;

    [ObservableProperty]
    private Guid _comandaId;

    partial void OnEsEdicionChanged(bool value)
    {
        OnPropertyChanged(nameof(TituloPagina));
        OnPropertyChanged(nameof(TextoBotonPrimario));
        OnPropertyChanged(nameof(PuedeGuardar));
    }

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

    public string TituloPagina => EsEdicion ? "Editando Comanda" : "Nueva Comanda";
    public string TextoBotonPrimario => EsEdicion ? "Guardar Cambios" : "Crear";

    public bool PuedeCrearComanda => ProductosCarrito.Any() && !IsLoading;
    public bool PuedeGuardar => (EsEdicion ? true : ProductosCarrito.Any()) && !IsLoading;

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
            OnPropertyChanged(nameof(PuedeGuardar));
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
            
            // Notificar cambios en las propiedades calculadas
            OnPropertyChanged(nameof(TotalCarrito));
            OnPropertyChanged(nameof(PuedeCrearComanda));
            OnPropertyChanged(nameof(PuedeGuardar));
        }
    }

    [RelayCommand]
    private void IncrementarCantidadCarrito(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            producto.Cantidad++;
            OnPropertyChanged(nameof(TotalCarrito));
            OnPropertyChanged(nameof(PuedeGuardar));
        }
    }

    [RelayCommand]
    private void DecrementarCantidadCarrito(ProductoCarritoDto producto)
    {
        if (producto != null && producto.Cantidad > 1)
        {
            producto.Cantidad--;
            OnPropertyChanged(nameof(TotalCarrito));
            OnPropertyChanged(nameof(PuedeGuardar));
        }
    }

    [RelayCommand]
    private void EliminarDelCarrito(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            if (!producto.EsNuevo && Guid.TryParse(producto.Id, out var pid))
            {
                _productosEliminados.Add(pid);
            }
            ProductosCarrito.Remove(producto);
            OnPropertyChanged(nameof(TotalCarrito));
            OnPropertyChanged(nameof(PuedeCrearComanda));
            OnPropertyChanged(nameof(PuedeGuardar));
        }
    }

    [RelayCommand]
    private async Task CrearComandaAsync()
    {
        try
        {
            if (!EsEdicion && !PuedeCrearComanda)
            {
                await _dialogService.ShowAlertAsync("Error", "Debe seleccionar al menos un producto");
                return;
            }

            if (EsEdicion && !ProductosCarrito.Any())
            {
                await _dialogService.ShowAlertAsync("Sin cambios", "No hay productos para guardar");
                return;
            }

            var confirmacion = await _dialogService.ShowConfirmAsync(
                EsEdicion ? "Confirmar Cambios" : "Confirmar Comanda",
                EsEdicion 
                    ? $"¿Guardar cambios en la comanda por un total de ${TotalCarrito:F2}?"
                    : $"¿Crear la comanda para la Mesa {Mesa.Numero} con {ProductosCarrito.Count} productos por un total de ${TotalCarrito:F2}?");
            
            if (!confirmacion)
                return;

            IsLoading = true;
            OnPropertyChanged(nameof(PuedeGuardar));

            // Crear la lista de productos para la comanda
            var productosComanda = ProductosCarrito.Select(p => new ComandaModels.ProductoComandaRequest
            {
                ProductoId = p.Id,
                Cantidad = p.Cantidad,
                Precio = p.Precio
            }).ToList();

            if (EsEdicion)
            {
                // 1) Remover productos marcados
                foreach (var prodId in _productosEliminados)
                {
                    if (prodId != Guid.Empty)
                    {
                        var respRemove = await _comandasService.RemoverProductoAsync(ComandaId, prodId);
                        if (!respRemove.Success)
                        {
                            await _dialogService.ShowAlertAsync("Advertencia", respRemove.Message ?? $"No se pudo eliminar el producto {prodId}");
                        }
                    }
                }

                // 2) Actualizar cantidades de existentes
                foreach (var existente in ProductosCarrito.Where(pc => !pc.EsNuevo))
                {
                    if (!Guid.TryParse(existente.Id, out var pid)) continue;
                    if (_cantidadesOriginales.TryGetValue(pid, out var cantOriginal) && existente.Cantidad != cantOriginal)
                    {
                        if (existente.Cantidad <= 0)
                        {
                            var respDel = await _comandasService.RemoverProductoAsync(ComandaId, pid);
                            if (!respDel.Success)
                            {
                                await _dialogService.ShowAlertAsync("Advertencia", respDel.Message ?? $"No se pudo eliminar el producto {existente.Nombre}");
                            }
                        }
                        else
                            {
                            var respUpd = await _comandasService.ActualizarCantidadProductoAsync(ComandaId, pid, existente.Cantidad);
                            if (!respUpd.Success)
                            {
                                await _dialogService.ShowAlertAsync("Advertencia", respUpd.Message ?? $"No se pudo actualizar cantidad para {existente.Nombre}");
                            }
                        }
                    }
                }

                // 3) Agregar productos nuevos
                var productosNuevos = ProductosCarrito
                    .Where(pc => pc.EsNuevo)
                    .Select(pc => new ComandaProductoRequest
                    {
                        ProductoId = Guid.Parse(pc.Id),
                        Cantidad = pc.Cantidad
                    })
                    .ToList();

                ApiResponse<ComandaDto>? resultUpdate = ApiResponse<ComandaDto>.SuccessResponse(null!, "Sin nuevos productos");
                if (productosNuevos.Any())
                {
                    resultUpdate = await _comandasService.AgregarProductosAsync(ComandaId, productosNuevos);
                    if (!resultUpdate.Success)
                    {
                        await _dialogService.ShowAlertAsync("Advertencia", resultUpdate.Message ?? "La comanda se actualizó, pero no se pudieron agregar productos");
                    }
                }
                if (resultUpdate.Success)
                {
                    await _dialogService.ShowAlertAsync("Éxito", "Cambios guardados");
                    // Notificar a la lista de comandas para refrescar inmediatamente
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(Messages.ComandaActualizada));
                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Error", resultUpdate.Message ?? "Error al guardar cambios");
                }
                return;
            }
            else
            {
                var comandaRequest = new ComandaModels.CrearComandaRequest
                {
                    MeseroId = "11111111-1111-1111-1111-111111111111", // Usuario administrador por defecto
                    MesaId = Mesa.Id.ToString(),
                    ClienteId = null, // Opcional
                    Observaciones = Observaciones,
                    ProductosIniciales = productosComanda,
                    Items = productosComanda // El backend parece esperar ambos campos
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
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al crear la comanda: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(PuedeGuardar));
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
            OnPropertyChanged(nameof(PuedeGuardar));
        }
    }

    public async Task InitializeEdicionAsync(string comandaId)
    {
        try
        {
            IsLoading = true;
            EsEdicion = true;
            if (Guid.TryParse(comandaId, out var id))
            {
                ComandaId = id;
            }

            var result = await _comandasService.ObtenerComandaPorIdAsync(ComandaId);
            if (result.Success && result.Data != null)
            {
                // Cargar mesa e items actuales como carrito
                Mesa = result.Data.Mesa ?? new MesaDto { Id = result.Data.MesaId, Numero = result.Data.NumeroMesa.ToString(), Ubicacion = string.Empty, Capacidad = 0 };
                ProductosCarrito.Clear();
                var items = result.Data.Items.Any() ? result.Data.Items : result.Data.Productos;
                _cantidadesOriginales.Clear();
                foreach (var i in items)
                {
                    ProductosCarrito.Add(new ProductoCarritoDto
                    {
                        Id = i.ProductoId.ToString(),
                        Nombre = string.IsNullOrWhiteSpace(i.Nombre) ? i.NombreDisplay : i.Nombre,
                        Descripcion = i.Descripcion,
                        Precio = i.PrecioUnitario > 0 ? i.PrecioUnitario : i.PrecioFinal / Math.Max(1, i.Cantidad),
                        Cantidad = i.Cantidad,
                        EsNuevo = false
                    });
                    _cantidadesOriginales[i.ProductoId] = i.Cantidad;
                }
                OnPropertyChanged(nameof(Mesa));
                OnPropertyChanged(nameof(MesaInfo));
                OnPropertyChanged(nameof(TotalCarrito));
                OnPropertyChanged(nameof(PuedeCrearComanda));
                OnPropertyChanged(nameof(TituloPagina));
                OnPropertyChanged(nameof(TextoBotonPrimario));
                OnPropertyChanged(nameof(PuedeGuardar));

                // Cargar también catálogo para permitir agregar más productos en edición
                await BuscarProductosAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message ?? "No se pudo cargar la comanda");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al inicializar edición: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(PuedeGuardar));
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
