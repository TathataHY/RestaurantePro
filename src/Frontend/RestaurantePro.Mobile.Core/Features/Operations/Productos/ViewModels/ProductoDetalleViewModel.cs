using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;

/// <summary>
/// ViewModel para la página de detalle de producto
/// </summary>
public partial class ProductoDetalleViewModel : BaseViewModel
{
    private readonly IProductosService _productosService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public ProductoDetalleViewModel(
        IProductosService productosService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _productosService = productosService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        
        Title = "Detalle de Producto";
        
        // Inicializar valores por defecto
        CantidadParaComanda = 1;
        Categorias = new List<CategoriaProductoDto>();
    }

    // ========================================
    // PROPIEDADES OBSERVABLES
    // ========================================

    [ObservableProperty]
    private ProductoDto? producto;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string descripcionEditada = string.Empty;

    [ObservableProperty]
    private decimal precioEditado;

    [ObservableProperty]
    private CategoriaProductoDto? categoriaSeleccionada;

    [ObservableProperty]
    private List<CategoriaProductoDto> categorias;

    [ObservableProperty]
    private int cantidadParaComanda;

    [ObservableProperty]
    private string observacionesComanda = string.Empty;

    // ========================================
    // PROPIEDADES CALCULADAS
    // ========================================

    /// <summary>
    /// Indica si se puede editar el producto
    /// </summary>
    public bool CanEdit => Producto != null && !IsBusy;

    /// <summary>
    /// Indica si se puede agregar a comanda
    /// </summary>
    public bool CanAddToComanda => Producto?.PuedeAgregarAComanda == true && !IsBusy;

    /// <summary>
    /// Indica si se puede eliminar el producto
    /// </summary>
    public bool CanDelete => Producto != null && !IsBusy;

    // ========================================
    // COMANDOS
    // ========================================

    /// <summary>
    /// Cargar producto por ID
    /// </summary>
    [RelayCommand]
    private async Task LoadProductoAsync(Guid productoId)
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _productosService.ObtenerProductoPorIdAsync(productoId);

            if (result.Success && result.Data != null)
            {
                Producto = result.Data;
                
                // Actualizar título con información del producto
                Title = $"Producto: {Producto?.Nombre} - {Producto?.CategoriaNombre}";
                
                // Cargar categorías para edición
                await LoadCategoriasAsync();
                
                // Actualizar estados
                ActualizarEstados();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", 
                    result.Message ?? "No se pudo cargar el producto");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cargar categorías disponibles
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriasAsync()
    {
        try
        {
            var result = await _productosService.ObtenerCategoriasAsync();
            
            if (result.Success && result.Data != null)
            {
                Categorias.Clear();
                foreach (var categoria in result.Data)
                {
                    Categorias.Add(categoria);
                }
                
                // Seleccionar la categoría actual del producto
                CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Nombre == Producto.CategoriaNombre);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar categorías: {ex.Message}");
        }
    }

    /// <summary>
    /// Iniciar edición del producto
    /// </summary>
    [RelayCommand]
    private void StartEdit()
    {
        if (!CanEdit) return;

        IsEditing = true;
        DescripcionEditada = Producto.Descripcion ?? string.Empty;
        PrecioEditado = Producto.Precio;
    }

    /// <summary>
    /// Cancelar edición
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        DescripcionEditada = Producto.Descripcion ?? string.Empty;
        PrecioEditado = Producto.Precio;
    }

    /// <summary>
    /// Guardar cambios del producto (simulado - no implementado en servicio)
    /// </summary>
    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (!IsEditing) return;

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(DescripcionEditada))
        {
            await _dialogService.ShowAlertAsync("Error", "La descripción no puede estar vacía");
            return;
        }

        if (PrecioEditado <= 0)
        {
            await _dialogService.ShowAlertAsync("Error", "El precio debe ser mayor a cero");
            return;
        }

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Guardar Cambios",
            "¿Guardar los cambios realizados al producto?");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            // Simular actualización (no implementado en servicio)
            await Task.Delay(500);

            // Actualizar producto local (simulado)
            if (Producto != null)
            {
                Producto.Descripcion = DescripcionEditada;
                Producto.Precio = PrecioEditado;
            }

            IsEditing = false;
            
            await _dialogService.ShowAlertAsync("Éxito", "Producto actualizado correctamente");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cambiar disponibilidad del producto (simulado - no implementado en servicio)
    /// </summary>
    [RelayCommand]
    private async Task ToggleDisponibilidadAsync()
    {
        if (Producto == null) return;

        var nuevoEstado = !Producto.PuedeAgregarAComanda;
        var accion = nuevoEstado ? "habilitar" : "deshabilitar";

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Cambiar Disponibilidad",
            $"¿{accion} el producto '{Producto.Nombre}'?");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            // Simular cambio de disponibilidad (no implementado en servicio)
            await Task.Delay(500);

            // Actualizar estado local (simulado)
            // Nota: En un caso real, esto vendría del servidor
            await _dialogService.ShowAlertAsync("Éxito", 
                nuevoEstado ? "Producto habilitado" : "Producto deshabilitado");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Agregar producto a una comanda
    /// </summary>
    [RelayCommand]
    private async Task AgregarAComandaAsync()
    {
        if (!CanAddToComanda) return;

        // Validar cantidad
        if (CantidadParaComanda <= 0)
        {
            await _dialogService.ShowAlertAsync("Error", "La cantidad debe ser mayor a cero");
            return;
        }

        // Aquí podrías mostrar una lista de comandas abiertas para seleccionar
        // Por simplicidad, simulamos la selección de una comanda
        var confirmar = await _dialogService.ShowConfirmAsync(
            "Agregar a Comanda",
            $"¿Agregar {CantidadParaComanda} unidad(es) de '{Producto.Nombre}' a una comanda activa?");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            // Simular agregado exitoso
            await Task.Delay(500); // Simular operación

            await _dialogService.ShowAlertAsync("Éxito", 
                $"Producto agregado a comanda:\n" +
                $"• {Producto.Nombre}\n" +
                $"• Cantidad: {CantidadParaComanda}\n" +
                $"• Total: {Producto.Precio * CantidadParaComanda:C}");

            // Limpiar campos
            CantidadParaComanda = 1;
            ObservacionesComanda = string.Empty;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navegar a la lista de comandas para agregar este producto
    /// </summary>
    [RelayCommand]
    private async Task SeleccionarComandaAsync()
    {
        if (!CanAddToComanda) return;

        await _dialogService.ShowAlertAsync("Información", 
            "Esta funcionalidad navegaría a la lista de comandas activas para seleccionar una comanda específica.");
    }

    /// <summary>
    /// Ver historial del producto
    /// </summary>
    [RelayCommand]
    private async Task VerHistorialAsync()
    {
        if (Producto == null) return;

        await _dialogService.ShowAlertAsync("Historial del Producto", 
            $"📊 Historial de '{Producto.Nombre}':\n\n" +
            $"• Veces ordenado: {Producto.Popularidad} veces\n" +
            $"• Popularidad: {Producto.PopularidadDescripcion}\n" +
            $"• Última modificación: {Producto.FechaModificacion:dd/MM/yyyy HH:mm}\n" +
            $"• Tiempo promedio de preparación: {Producto.TiempoPreparacion} min\n" +
            $"• Estado actual: {Producto.EstadoDisponibilidad}");
    }

    /// <summary>
    /// Eliminar producto (simulado - no implementado en servicio)
    /// </summary>
    [RelayCommand]
    private async Task EliminarProductoAsync()
    {
        if (!CanDelete) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Eliminar Producto",
            $"¿Está seguro de eliminar el producto '{Producto.Nombre}'?\n\n" +
            "Esta acción no se puede deshacer.");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            // Simular eliminación (no implementado en servicio)
            await Task.Delay(500);

            await _dialogService.ShowAlertAsync("Éxito", "Producto eliminado correctamente");
            
            // Navegar de vuelta
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Refrescar datos del producto
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (Producto == null || IsBusy) return;

        IsBusy = true;
        try
        {
            // Recargar producto
            await LoadProductoAsync(Producto.Id);
            
            await _dialogService.ShowAlertAsync("Éxito", "Datos actualizados correctamente");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al refrescar: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ========================================
    // MÉTODOS PRIVADOS
    // ========================================

    /// <summary>
    /// Actualizar estados calculados
    /// </summary>
    private void ActualizarEstados()
    {
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanAddToComanda));
        OnPropertyChanged(nameof(CanDelete));
    }

    /// <summary>
    /// Limpiar recursos al salir de la página
    /// </summary>
    public void Cleanup()
    {
        Producto = null;
        IsEditing = false;
        Categorias.Clear();
        CantidadParaComanda = 1;
        ObservacionesComanda = string.Empty;
    }
} 