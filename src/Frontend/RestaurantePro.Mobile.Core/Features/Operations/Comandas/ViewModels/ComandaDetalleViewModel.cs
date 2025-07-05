using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

/// <summary>
/// ViewModel para la página de detalle de comanda
/// Maneja la visualización y edición de una comanda específica
/// </summary>
public partial class ComandaDetalleViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public ComandaDetalleViewModel(
        IComandasService comandasService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _comandasService = comandasService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        
        Items = new ObservableCollection<ComandaProductoDto>();
        Title = "Detalle de Comanda";
        
        // Inicializar con comanda vacía para evitar nulls
        Comanda = new ComandaDto();
    }

    #region Properties

    [ObservableProperty]
    private ComandaDto comanda;

    [ObservableProperty]
    private ObservableCollection<ComandaProductoDto> items;

    [ObservableProperty]
    private ComandaProductoDto? selectedItem;

    [ObservableProperty]
    private bool isEditable;

    [ObservableProperty]
    private bool canFinalize;

    [ObservableProperty]
    private bool canCancel;

    [ObservableProperty]
    private string observaciones = string.Empty;

    [ObservableProperty]
    private decimal totalActual;

    [ObservableProperty]
    private int totalItems;

    #endregion

    #region Commands

    /// <summary>
    /// Cargar la comanda con sus detalles
    /// </summary>
    [RelayCommand]
    private async Task LoadComandaAsync(Guid comandaId)
    {
        if (comandaId == Guid.Empty) return;

        IsBusy = true;
        try
        {
            var result = await _comandasService.ObtenerComandaPorIdAsync(comandaId);
            
            if (result.Success && result.Data != null)
            {
                Comanda = result.Data;
                Title = $"Comanda {Comanda.Numero}";
                Observaciones = Comanda.Observaciones ?? string.Empty;
                
                await LoadItemsComandaAsync();
                ActualizarEstados();
                CalcularTotales();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message ?? "No se pudo cargar la comanda");
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
    /// Cargar los items de la comanda
    /// </summary>
    [RelayCommand]
    private async Task LoadItemsComandaAsync()
    {
        if (Comanda?.Id == null) return;

        try
        {
            // Usar los productos de la comanda (simulando que vienen del servicio)
            if (Comanda.Productos != null)
            {
                Items.Clear();
                foreach (var producto in Comanda.Productos)
                {
                    Items.Add(producto);
                }
                
                CalcularTotales();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar items: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualizar cantidad de un item
    /// </summary>
    [RelayCommand]
    private async Task ActualizarCantidadAsync(ComandaProductoDto? item)
    {
        if (item == null || !IsEditable) return;

        var cantidadStr = await _dialogService.ShowPromptAsync(
            "Actualizar Cantidad",
            $"Cantidad actual: {item.Cantidad}",
            item.Cantidad.ToString());

        if (string.IsNullOrWhiteSpace(cantidadStr)) return;

        if (int.TryParse(cantidadStr, out var nuevaCantidad) && nuevaCantidad > 0)
        {
            IsBusy = true;
            try
            {
                var result = await _comandasService.ActualizarCantidadProductoAsync(
                    Comanda.Id, item.ProductoId, nuevaCantidad);

                if (result.Success)
                {
                    item.Cantidad = nuevaCantidad;
                    CalcularTotales();
                    
                    await _dialogService.ShowAlertAsync("Éxito", "Cantidad actualizada correctamente");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Error", result.Message ?? "No se pudo actualizar la cantidad");
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
        else
        {
            await _dialogService.ShowAlertAsync("Error", "Cantidad inválida");
        }
    }

    /// <summary>
    /// Eliminar un item de la comanda
    /// </summary>
    [RelayCommand]
    private async Task EliminarItemAsync(ComandaProductoDto? item)
    {
        if (item == null || !IsEditable) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmar",
            $"¿Eliminar {item.Nombre} de la comanda?");

        if (!confirmar) return;

        // Simular eliminación local por ahora
        Items.Remove(item);
        CalcularTotales();
        
        await _dialogService.ShowAlertAsync("Éxito", "Producto eliminado correctamente");
    }

    /// <summary>
    /// Agregar nuevo producto a la comanda
    /// </summary>
    [RelayCommand]
    private async Task AgregarProductoAsync()
    {
        if (!IsEditable) return;

        await _navigationService.NavigateToAsync($"productos?comandaId={Comanda.Id}");
    }

    /// <summary>
    /// Cambiar el estado de la comanda
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoAsync()
    {
        if (Comanda?.Id == null) return;

        var estadoActual = Comanda.Estado;
        var siguienteEstado = estadoActual switch
        {
            "Pendiente" => "Preparando",
            "Preparando" => "Lista",
            "Lista" => "Entregada",
            _ => "Pendiente"
        };

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Cambiar Estado",
            $"¿Cambiar estado de '{estadoActual}' a '{siguienteEstado}'?");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            var result = await _comandasService.CambiarEstadoComandaAsync(Comanda.Id, siguienteEstado);

            if (result.Success)
            {
                Comanda.Estado = siguienteEstado;
                ActualizarEstados();
                
                await _dialogService.ShowAlertAsync("Éxito", $"Estado cambiado a '{siguienteEstado}'");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message ?? "No se pudo cambiar el estado");
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
    /// Finalizar la comanda
    /// </summary>
    [RelayCommand]
    private async Task FinalizarComandaAsync()
    {
        if (!CanFinalize) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Finalizar Comanda",
            $"¿Finalizar la comanda {Comanda.Numero}? Esta acción no se puede deshacer.");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            var result = await _comandasService.FinalizarComandaAsync(Comanda.Id, "Efectivo");

            if (result.Success)
            {
                Comanda.Estado = "Finalizada";
                ActualizarEstados();
                
                await _dialogService.ShowAlertAsync("Éxito", "Comanda finalizada correctamente");
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message ?? "No se pudo finalizar la comanda");
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
    /// Cancelar la comanda
    /// </summary>
    [RelayCommand]
    private async Task CancelarComandaAsync()
    {
        if (!CanCancel) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Cancelar Comanda",
            "Ingrese el motivo de cancelación:");

        if (string.IsNullOrWhiteSpace(motivo)) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmar Cancelación",
            $"¿Cancelar la comanda {Comanda.Numero}? Esta acción no se puede deshacer.");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            var result = await _comandasService.CancelarComandaAsync(Comanda.Id, motivo);

            if (result.Success)
            {
                Comanda.Estado = "Cancelada";
                ActualizarEstados();
                
                await _dialogService.ShowAlertAsync("Éxito", "Comanda cancelada correctamente");
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message ?? "No se pudo cancelar la comanda");
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
    /// Actualizar observaciones de la comanda
    /// </summary>
    [RelayCommand]
    private async Task ActualizarObservacionesAsync()
    {
        if (!IsEditable) return;

        var nuevasObservaciones = await _dialogService.ShowPromptAsync(
            "Observaciones",
            "Ingrese observaciones para la comanda:",
            Observaciones);

        if (nuevasObservaciones == null) return; // Usuario canceló

        // Simulamos actualización local
        Observaciones = nuevasObservaciones;
        Comanda.Observaciones = nuevasObservaciones;
        
        await _dialogService.ShowAlertAsync("Éxito", "Observaciones actualizadas correctamente");
    }

    /// <summary>
    /// Refrescar datos de la comanda
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (Comanda?.Id != null)
        {
            await LoadComandaAsync(Comanda.Id);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Actualizar estados de disponibilidad de acciones
    /// </summary>
    private void ActualizarEstados()
    {
        IsEditable = Comanda?.Estado == "Pendiente" || Comanda?.Estado == "Preparando";
        CanFinalize = Comanda?.Estado == "Lista";
        CanCancel = Comanda?.Estado == "Pendiente" || Comanda?.Estado == "Preparando";
    }

    /// <summary>
    /// Calcular totales de la comanda
    /// </summary>
    private void CalcularTotales()
    {
        TotalItems = Items?.Sum(i => i.Cantidad) ?? 0;
        TotalActual = Items?.Sum(i => i.PrecioTotal) ?? 0;
        
        // Actualizar el total en la comanda
        if (Comanda != null)
        {
            Comanda.Total = TotalActual;
        }
    }

    /// <summary>
    /// Limpiar datos al navegar fuera de la página
    /// </summary>
    public void Cleanup()
    {
        Items?.Clear();
        Comanda = new ComandaDto();
        SelectedItem = null;
        Observaciones = string.Empty;
        TotalActual = 0;
        TotalItems = 0;
        IsEditable = false;
        CanFinalize = false;
        CanCancel = false;
    }

    #endregion
} 