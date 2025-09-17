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
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

public partial class CrearComandaViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly IProductosService _productosService;
    private readonly IMesasService _mesasService;
    private readonly IDailyPreparationsService _dailyPreparationsService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly Dictionary<Guid, int> _cantidadesOriginales = new();
    private readonly HashSet<Guid> _productosEliminados = new();
    private readonly HashSet<Guid> _itemsEliminados = new();
    private const int MaxAPrepararPorItem = 10; // Regla de negocio: tope de preparación por ítem
    // "Mesa" (por defecto), "Delivery" o "Para llevar"
    public string TipoSeleccionado { get; set; } = "Mesa";


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
        IDailyPreparationsService dailyPreparationsService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _comandasService = comandasService;
        _productosService = productosService;
        _mesasService = mesasService;
        _dailyPreparationsService = dailyPreparationsService;
        _navigationService = navigationService;
        _dialogService = dialogService;

        ProductosDisponibles = new ObservableCollection<ProductoCarritoDto>();
        ProductosCarrito = new ObservableCollection<ProductoCarritoDto>();
        PreparacionesDelDia = new ObservableCollection<PreparacionDiariaDto>();
        _consumosPreparaciones = new Dictionary<Guid, int>();

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
    public ObservableCollection<PreparacionDiariaDto> PreparacionesDelDia { get; }
    private readonly Dictionary<Guid, int> _consumosPreparaciones;

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
    private async Task LoadPreparacionesDiaAsync()
    {
        try
        {
            var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();
            if (result.Succeeded && result.Data != null)
            {
                PreparacionesDelDia.Clear();
                foreach (var p in result.Data.Where(x => x.CantidadDisponible > 0 && !x.EstaVencida))
                {
                    PreparacionesDelDia.Add(p);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar preparaciones del día: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task IncrementarDesdePreparacionAsync(PreparacionDiariaDto? prep)
    {
        if (prep == null) return;
        if (prep.EstaVencida || prep.CantidadDisponible <= 0)
        {
            await _dialogService.ShowAlertAsync("No disponible", "La preparación no está disponible o no tiene unidades.");
            return;
        }

        // Buscar el producto base en el catálogo disponible
        var prodBase = ProductosDisponibles.FirstOrDefault(x => Guid.TryParse(x.Id, out var gid) && gid == prep.ProductoId);
        if (prodBase == null)
        {
            // Si no está en la lista visible, intentamos obtenerlo del servicio y agregarlo igual
            var prodResp = await _productosService.ObtenerProductoPorIdAsync(prep.ProductoId);
            if (prodResp.Success && prodResp.Data != null)
            {
                prodBase = new ProductoCarritoDto
                {
                    Id = prodResp.Data.Id.ToString(),
                    Nombre = prodResp.Data.Nombre,
                    Descripcion = prodResp.Data.Descripcion,
                    Precio = prodResp.Data.Precio,
                    Cantidad = 0,
                    PreparacionesDisponiblesHoy = -1
                };
                ProductosDisponibles.Add(prodBase);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Producto no disponible", "El producto de la preparación no está disponible.");
                return;
            }
        }

        // Agregar directamente al carrito (o incrementar si ya existe)
        var existente = ProductosCarrito.FirstOrDefault(p => p.Id == prodBase.Id);
        if (existente != null)
        {
            // Validar límite máximo como en IncrementarCantidadCarrito
            var maxTotal = (existente.PreparacionesDisponiblesHoy < 0 ? MaxAPrepararPorItem : existente.PreparacionesDisponiblesHoy + MaxAPrepararPorItem);
            if (existente.Cantidad + 1 > maxTotal)
            {
                await _dialogService.ShowAlertAsync(
                    "Límite alcanzado",
                    $"Máximo permitido por ítem: {maxTotal} (Disp. hoy: {Math.Max(0, existente.PreparacionesDisponiblesHoy)}, A preparar: {MaxAPrepararPorItem}).");
                return;
            }
            existente.Cantidad++;
        }
        else
        {
            var nuevo = new ProductoCarritoDto
            {
                Id = prodBase.Id,
                Nombre = prodBase.Nombre,
                Descripcion = prodBase.Descripcion,
                Precio = prodBase.Precio,
                Cantidad = 1,
                PreparacionesDisponiblesHoy = prodBase.PreparacionesDisponiblesHoy
            };
            ProductosCarrito.Add(nuevo);
        }

        // Registrar consumo tentativo por preparación (se confirmará al crear la comanda)
        if (_consumosPreparaciones.ContainsKey(prep.Id))
            _consumosPreparaciones[prep.Id] += 1;
        else
            _consumosPreparaciones[prep.Id] = 1;

        // Refrescar inmediatamente en la UI creando una nueva instancia con el nuevo disponible
        ActualizarCantidadDisponiblePreparacion(prep, prep.CantidadDisponible - 1);

        OnPropertyChanged(nameof(TotalCarrito));
        OnPropertyChanged(nameof(PuedeGuardar));
    }

    [RelayCommand]
    private async Task IncrementarCantidadAsync(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            // Cargar disponibilidad de preparaciones una sola vez por producto
            if (producto.PreparacionesDisponiblesHoy < 0 && Guid.TryParse(producto.Id, out var prodId))
            {
                var preps = await _dailyPreparationsService.GetPreparacionesDiariasPorProductoAsync(prodId);
                if (preps.Succeeded && preps.Data != null)
                {
                    producto.PreparacionesDisponiblesHoy = preps.Data.Sum(p => p.CantidadDisponible);
                }
                else
                {
                    producto.PreparacionesDisponiblesHoy = 0;
                }
            }

            var maxTotal = (producto.PreparacionesDisponiblesHoy < 0 ? MaxAPrepararPorItem : producto.PreparacionesDisponiblesHoy + MaxAPrepararPorItem);
            if (producto.Cantidad + 1 > maxTotal)
            {
                await _dialogService.ShowAlertAsync(
                    "Límite alcanzado",
                    $"Máximo permitido por ítem: {maxTotal} (Disp. hoy: {Math.Max(0, producto.PreparacionesDisponiblesHoy)}, A preparar: {MaxAPrepararPorItem}).");
                return;
            }

            producto.Cantidad++;

            // Intentar reservar 1 unidad desde preparaciones disponibles para este producto
            if (Guid.TryParse(producto.Id, out var productoId))
            {
                if (ReservarDesdePreparaciones(productoId))
                {
                    // Refrescar indicador local
                    if (producto.PreparacionesDisponiblesHoy >= 0)
                        producto.PreparacionesDisponiblesHoy = Math.Max(0, producto.PreparacionesDisponiblesHoy - 1);
                }
            }
        }
    }

    [RelayCommand]
    private void DecrementarCantidad(ProductoCarritoDto producto)
    {
        if (producto != null && producto.Cantidad > 0)
        {
            producto.Cantidad--;
            // Liberar reserva de preparación si existe para este producto
            if (Guid.TryParse(producto.Id, out var productoId))
            {
                if (LiberarReservaDePreparaciones(productoId))
                {
                    if (producto.PreparacionesDisponiblesHoy >= 0)
                        producto.PreparacionesDisponiblesHoy += 1;
                }
            }
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
                    Cantidad = producto.Cantidad,
                    PreparacionesDisponiblesHoy = producto.PreparacionesDisponiblesHoy
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
    private async Task IncrementarCantidadCarrito(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            // Asegurar disponibilidad cargada también para items del carrito (edición)
            if (producto.PreparacionesDisponiblesHoy < 0 && Guid.TryParse(producto.Id, out var prodId))
            {
                var preps = await _dailyPreparationsService.GetPreparacionesDiariasPorProductoAsync(prodId);
                if (preps.Succeeded && preps.Data != null)
                {
                    producto.PreparacionesDisponiblesHoy = preps.Data.Sum(p => p.CantidadDisponible);
                }
                else
                {
                    producto.PreparacionesDisponiblesHoy = 0;
                }
            }

            var maxTotal = (producto.PreparacionesDisponiblesHoy < 0 ? MaxAPrepararPorItem : producto.PreparacionesDisponiblesHoy + MaxAPrepararPorItem);
            if (producto.Cantidad + 1 > maxTotal)
            {
                await _dialogService.ShowAlertAsync(
                    "Límite alcanzado",
                    $"Máximo permitido por ítem: {maxTotal} (Disp. hoy: {Math.Max(0, producto.PreparacionesDisponiblesHoy)}, A preparar: {MaxAPrepararPorItem}).");
                return;
            }

            producto.Cantidad++;

            // Intentar reservar 1 unidad desde preparaciones disponibles para este producto
            if (Guid.TryParse(producto.Id, out var productoId))
            {
                if (ReservarDesdePreparaciones(productoId))
                {
                    if (producto.PreparacionesDisponiblesHoy >= 0)
                        producto.PreparacionesDisponiblesHoy = Math.Max(0, producto.PreparacionesDisponiblesHoy - 1);
                }
            }
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
            if (Guid.TryParse(producto.Id, out var productoId))
            {
                if (LiberarReservaDePreparaciones(productoId))
                {
                    if (producto.PreparacionesDisponiblesHoy >= 0)
                        producto.PreparacionesDisponiblesHoy += 1;
                }
            }
            OnPropertyChanged(nameof(TotalCarrito));
            OnPropertyChanged(nameof(PuedeGuardar));
        }
    }

    private bool ReservarDesdePreparaciones(Guid productoId)
    {
        // Orden simple: priorizar las que vencen antes
        var prep = PreparacionesDelDia
            .Where(p => p.ProductoId == productoId && p.CantidadDisponible > 0 && !p.EstaVencida)
            .OrderBy(p => p.FechaVencimiento)
            .FirstOrDefault();
        if (prep == null)
            return false;

        // Actualizar contador interno de consumos
        if (_consumosPreparaciones.ContainsKey(prep.Id))
            _consumosPreparaciones[prep.Id] += 1;
        else
            _consumosPreparaciones[prep.Id] = 1;

        // Refrescar item en la colección creando copia
        ActualizarCantidadDisponiblePreparacion(prep, prep.CantidadDisponible - 1);
        return true;
    }

    private bool LiberarReservaDePreparaciones(Guid productoId)
    {
        // Buscar una preparación del mismo producto que tenga consumo registrado
        var prep = PreparacionesDelDia
            .Where(p => p.ProductoId == productoId && _consumosPreparaciones.TryGetValue(p.Id, out var c) && c > 0)
            .OrderByDescending(p => p.FechaVencimiento) // devolver de la que vence más tarde para mantener primeras agotadas
            .FirstOrDefault();
        if (prep == null)
            return false;

        _consumosPreparaciones[prep.Id] -= 1;
        ActualizarCantidadDisponiblePreparacion(prep, prep.CantidadDisponible + 1);
        return true;
    }

    private void ActualizarCantidadDisponiblePreparacion(PreparacionDiariaDto original, int nuevaCantidad)
    {
        var idx = PreparacionesDelDia.IndexOf(original);
        if (idx < 0) return;

        var actualizado = new PreparacionDiariaDto
        {
            Id = original.Id,
            ProductoId = original.ProductoId,
            NombreProducto = original.NombreProducto,
            ChefId = original.ChefId,
            NombreChef = original.NombreChef,
            CantidadPreparada = original.CantidadPreparada,
            CantidadDisponible = nuevaCantidad,
            FechaVencimiento = original.FechaVencimiento,
            Observaciones = original.Observaciones,
            FechaPreparacion = original.FechaPreparacion,
            Estado = original.Estado
        };

        PreparacionesDelDia[idx] = actualizado;
    }

    [RelayCommand]
    private void EliminarDelCarrito(ProductoCarritoDto producto)
    {
        if (producto != null)
        {
            if (!producto.EsNuevo && producto.ItemId != Guid.Empty)
            {
                _itemsEliminados.Add(producto.ItemId);
            }
            else if (!producto.EsNuevo && Guid.TryParse(producto.Id, out var pid))
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

            // Si hubo consumos desde preparaciones, consumir en backend antes de crear la comanda
            if (_consumosPreparaciones.Any())
            {
                foreach (var kvp in _consumosPreparaciones.ToList())
                {
                    var prepId = kvp.Key;
                    var cant = kvp.Value;
                    if (cant <= 0) continue;
                    var consumir = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(prepId, cant, $"Consumo por creación de comanda {DateTime.Now:HH:mm}");
                    if (!consumir.Succeeded)
                    {
                        await _dialogService.ShowAlertAsync("Error", $"No se pudo consumir la preparación {prepId}: {consumir.Error}");
                        IsLoading = false;
                        OnPropertyChanged(nameof(PuedeGuardar));
                        return;
                    }
                }
            }

            if (EsEdicion)
            {
                // 1) Remover items marcados (preferente por itemId)
                foreach (var itemId in _itemsEliminados)
                {
                    if (itemId != Guid.Empty)
                    {
                        var respRemove = await _comandasService.RemoverProductoAsync(ComandaId, itemId);
                        if (!respRemove.Success)
                        {
                            await _dialogService.ShowAlertAsync("Advertencia", respRemove.Message ?? $"No se pudo eliminar el item {itemId}");
                        }
                    }
                }

                // 1b) Remover por productoId (fallback si no hubo itemId)
                foreach (var prodId in _productosEliminados)
                {
                    if (prodId != Guid.Empty)
                    {
                        var item = ProductosCarrito.FirstOrDefault(x => Guid.TryParse(x.Id, out var gid) && gid == prodId);
                        var itemId = item?.ItemId ?? Guid.Empty;
                        var respRemove = await _comandasService.RemoverProductoAsync(ComandaId, itemId);
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
                            var respDel = await _comandasService.RemoverProductoAsync(ComandaId, existente.ItemId);
                            if (!respDel.Success)
                            {
                                await _dialogService.ShowAlertAsync("Advertencia", respDel.Message ?? $"No se pudo eliminar el producto {existente.Nombre}");
                            }
                        }
                        else
                        {
                            var respUpd = await _comandasService.ActualizarCantidadProductoAsync(ComandaId, existente.ItemId, existente.Cantidad);
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
                var esMesa = string.Equals(TipoSeleccionado, "Mesa", StringComparison.OrdinalIgnoreCase);
                var tipoEnviar = esMesa ? "Mesa" : (string.Equals(TipoSeleccionado, "Delivery", StringComparison.OrdinalIgnoreCase) ? "Delivery" : "TakeAway");

                var comandaRequest = new ComandaModels.CrearComandaRequest
                {
                    MeseroId = string.Empty, // Se completará con el UserId del JWT en el servicio
                    MesaId = esMesa ? Mesa.Id.ToString() : null,
                    ClienteId = null,
                    Observaciones = Observaciones,
                    ProductosIniciales = productosComanda,
                    Items = productosComanda,
                    Tipo = tipoEnviar
                };

                var result = await _comandasService.CrearComandaAsync(comandaRequest);
            
                if (result.Success)
                {
                    await _dialogService.ShowAlertAsync("Éxito", "Comanda creada exitosamente");
                    await _navigationService.GoBackAsync();
                }
                else
                {
                    var detalleErrores = result.Errors != null && result.Errors.Any() ? string.Join("\n", result.Errors) : string.Empty;
                    var mensaje = string.IsNullOrWhiteSpace(result.Message) ? "Error al crear la comanda" : result.Message;
                    await _dialogService.ShowAlertAsync("Error", string.IsNullOrWhiteSpace(detalleErrores) ? mensaje : $"{mensaje}\n\n{detalleErrores}");
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
            
            if (!string.IsNullOrWhiteSpace(mesaId) && Guid.TryParse(mesaId, out var _))
            {
                await CargarMesaAsync(mesaId);
                await BuscarProductosAsync();
            }
            else
            {
                // No llegó mesaId válido: habilitar flujo sin mesa (Para llevar / Delivery)
                Mesa = new MesaDto();
                OnPropertyChanged(nameof(Mesa));
                OnPropertyChanged(nameof(MesaInfo));
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
                        EsNuevo = false,
                        ItemId = i.ItemId
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
            if (!Guid.TryParse(mesaId, out var mesaGuid))
            {
                await _dialogService.ShowAlertAsync("Error", "Identificador de mesa inválido");
                return;
            }

            var result = await _mesasService.ObtenerMesaAsync(mesaGuid);
            if (result.Success && result.Data != null)
            {
                Mesa = result.Data;
                // Notificar a la UI que la propiedad Mesa cambió
                OnPropertyChanged(nameof(Mesa));
                OnPropertyChanged(nameof(MesaInfo));
            }
            else
            {
                var detalle = string.IsNullOrWhiteSpace(result.Message) ? string.Join("\n", result.Errors ?? new()) : result.Message;
                await _dialogService.ShowAlertAsync("Error", string.IsNullOrWhiteSpace(detalle) ? "No se pudo cargar la información de la mesa" : detalle);
                // Fallback: permitir seleccionar otra mesa
                await SolicitarSeleccionMesaAsync();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar la mesa: {ex.Message}");
            await SolicitarSeleccionMesaAsync();
        }
    }

    /// <summary>
    /// Si no hay mesa válida, abre un selector simple de mesas disponibles
    /// </summary>
    private async Task SolicitarSeleccionMesaAsync()
    {
        // Traer mesas disponibles
        var mesasResp = await _mesasService.ObtenerMesasDisponiblesAsync();
        if (!mesasResp.Success || mesasResp.Data == null || mesasResp.Data.Count == 0)
        {
            await _dialogService.ShowAlertAsync("Información", "No hay mesas disponibles ahora mismo");
            return;
        }

        var opciones = mesasResp.Data
            .Select(m => ($"Mesa {m.Numero} — {m.Ubicacion} (Cap: {m.Capacidad})", m.Id.ToString()))
            .ToList();

        var labels = opciones.Select(o => o.Item1).ToArray();
        var seleccion = await _dialogService.ShowActionSheetAsync(
            "Seleccionar Mesa",
            "Elija la mesa para la nueva comanda:",
            "Cancelar",
            labels);

        if (string.IsNullOrWhiteSpace(seleccion) || seleccion == "Cancelar")
            return;

        var mesaSeleccionada = opciones.FirstOrDefault(o => o.Item1 == seleccion);
        if (mesaSeleccionada != default)
        {
            await CargarMesaAsync(mesaSeleccionada.Item2);
            await BuscarProductosAsync();
        }
    }

    #endregion
}
