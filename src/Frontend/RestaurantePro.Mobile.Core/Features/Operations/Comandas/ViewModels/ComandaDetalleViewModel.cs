using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Core.Attributes;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Core.Helpers;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

/// <summary>
/// ViewModel para la página de detalle de comanda
/// Maneja la visualización y edición de una comanda específica
/// </summary>
public partial class ComandaDetalleViewModel : AuthorizedBaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public ComandaDetalleViewModel(
        IComandasService comandasService,
        INavigationService navigationService,
        IDialogService dialogService,
        IAuthorizationService authorizationService,
        IAuthorizationValidator authorizationValidator,
        AuthorizationUIHelper uiHelper,
        ILogger<ComandaDetalleViewModel> logger)
        : base(authorizationService, authorizationValidator, uiHelper, dialogService, logger)
    {
        _comandasService = comandasService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        
        Items = new ObservableCollection<ComandaProductoDto>();
        Title = "Detalle de Comanda";
        
        // Inicializar con comanda vacía para evitar nulls
        Comanda = new ComandaDto();
    }

    /// <summary>
    /// Configurar permisos UI según el rol del usuario actual
    /// </summary>
    private async Task ConfigurarPermisosUIAsync()
    {
        try
        {
            // Configurar permisos específicos
            CanEditComanda = await HasPermissionAsync(AppPermission.ModificarComandas);
            CanChangeEstado = await HasPermissionAsync(AppPermission.ModificarComandas);
            CanFinalizeComanda = await HasPermissionAsync(AppPermission.FinalizarComandas);
            CanCancelComanda = await HasPermissionAsync(AppPermission.CancelarComandas);

            // Log para debug
            var userRoles = await AuthorizationService.GetUserRolesAsync();
            System.Diagnostics.Debug.WriteLine($"🔐 [ComandaDetalleVM] Usuario: {string.Join(", ", userRoles)}");
            System.Diagnostics.Debug.WriteLine($"🔐 [ComandaDetalleVM] Editar: {CanEditComanda}, Cambiar Estado: {CanChangeEstado}");
            System.Diagnostics.Debug.WriteLine($"🔐 [ComandaDetalleVM] Finalizar: {CanFinalizeComanda}, Cancelar: {CanCancelComanda}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error configurando permisos UI en ComandaDetalleViewModel");
        }
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

    [ObservableProperty]
    private string finalizarButtonText = "Finalizar";

    [ObservableProperty]
    private bool showCambiarEstado = true;

    // Propiedades de Autorización UI
    [ObservableProperty]
    private bool canEditComanda = false;

    [ObservableProperty]
    private bool canChangeEstado = false;

    [ObservableProperty]
    private bool canFinalizeComanda = false;

    [ObservableProperty]
    private bool canCancelComanda = false;

    /// <summary>
    /// Indica si la comanda es nueva (estado Creada)
    /// </summary>
    public bool EsNueva => Comanda?.PuedeSerEditada == true;

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
                Title = $"Comanda {Comanda.NumeroDisplay}";
                Observaciones = Comanda.Observaciones ?? string.Empty;

                // Preferir Items si el backend los devuelve; si no, usar Productos
                Items.Clear();
                var lista = (Comanda.Items != null && Comanda.Items.Any()) ? Comanda.Items : Comanda.Productos;
                foreach (var p in lista)
                {
                    Items.Add(p);
                }
                ActualizarEstados();
                CalcularTotales();
                
                // Configurar permisos de autorización
                await ConfigurarPermisosUIAsync();
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
        // Redirigir a la sección de edición de comanda
        if (Comanda?.Id == null || Comanda.Id == Guid.Empty) return;
        await _navigationService.NavigateToAsync($"editar-comanda?comandaId={Comanda.Id}");
    }

    /// <summary>
    /// Eliminar un item de la comanda
    /// </summary>
    [RelayCommand]
    private async Task EliminarItemAsync(ComandaProductoDto? item)
    {
        // Redirigir a la sección de edición de comanda
        if (Comanda?.Id == null || Comanda.Id == Guid.Empty) return;
        await _navigationService.NavigateToAsync($"editar-comanda?comandaId={Comanda.Id}");
    }

    /// <summary>
    /// Agregar nuevo producto a la comanda
    /// </summary>
    [RelayCommand]
    [RequirePermission(AppPermission.ModificarComandas)]
    private async Task AgregarProductoAsync()
    {
        if (!IsEditable) return;

        await _navigationService.NavigateToAsync($"productos?comandaId={Comanda.Id}");
    }

    /// <summary>
    /// Cambiar el estado de la comanda
    /// </summary>
    [RelayCommand]
    [RequirePermission(AppPermission.ModificarComandas)]
    private async Task CambiarEstadoAsync()
    {
        if (Comanda?.Id == null) return;

        var estadoActual = Comanda.Estado ?? string.Empty;
        var estadoActualLower = estadoActual.ToLowerInvariant();

        // Restricción: solo el mesero cambia de "Lista" a "Entregada".
        if (!estadoActualLower.Contains("lista"))
        {
            await _dialogService.ShowAlertAsync(
                "Información",
                "Este cambio de estado lo realiza Cocina. Solo puedes marcar 'Entregada' cuando la comanda esté 'Lista'.");
            return;
        }

        var siguienteEstado = "Entregada";

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
                // Refrescar desde API para reflejar de inmediato (cache ya invalidada en backend)
                await LoadComandaAsync(Comanda.Id);
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
    /// Finalizar la comanda - Administradores y Meseros
    /// </summary>
    [RelayCommand]
    [RequirePermission(AppPermission.FinalizarComandas)]
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
                // Recargar los datos de la comanda para obtener el estado actualizado
                await LoadComandaAsync(Comanda.Id);
                
                await _dialogService.ShowAlertAsync("Éxito", "Comanda finalizada correctamente");
                
                // Opcional: navegar de vuelta después de un breve delay
                // await Task.Delay(1000);
                // await _navigationService.GoBackAsync();
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
    /// Cancelar la comanda - Solo Administradores
    /// </summary>
    [RelayCommand]
    [RequirePermission(AppPermission.CancelarComandas)]
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
                // Recargar los datos de la comanda para obtener el estado actualizado
                await LoadComandaAsync(Comanda.Id);
                
                await _dialogService.ShowAlertAsync("Éxito", "Comanda cancelada correctamente");
                
                // Opcional: navegar de vuelta después de un breve delay
                // await Task.Delay(1000);
                // await _navigationService.GoBackAsync();
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
    [RequirePermission(AppPermission.ModificarComandas)]
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

    /// <summary>
    /// Editar comanda (navegar a página de edición)
    /// </summary>
    [RelayCommand]
    [RequirePermission(AppPermission.ModificarComandas)]
    private async Task EditarComandaAsync()
    {
        if (Comanda?.Id == null) return;

        try
        {
            // Navegar a la página de edición de comanda
            await _navigationService.NavigateToAsync("editar-comanda", new Dictionary<string, object>
            {
                ["comandaId"] = Comanda.Id.ToString()
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al editar comanda: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Actualizar estados de disponibilidad de acciones
    /// </summary>
    private void ActualizarEstados()
    {
        if (Comanda == null) return;

        // Usar las propiedades del DTO del frontend
        var estado = Comanda.Estado?.ToLowerInvariant() ?? string.Empty;
        var estadoTexto = Comanda.EstadoTexto?.ToLowerInvariant() ?? string.Empty;
        var estadoNormalizado = !string.IsNullOrWhiteSpace(estado) ? estado : estadoTexto;

        // Editable solo si puede ser editada (Creada o Pendiente)
        IsEditable = Comanda.PuedeSerEditada;

        // Mostrar botón de finalizar solo en Lista o Entregada (NO en finalizada)
        CanFinalize = (estadoNormalizado == "lista" || estadoNormalizado == "entregada") 
                     && estadoNormalizado != "finalizada" 
                     && estadoNormalizado != "cancelada";
        FinalizarButtonText = estadoNormalizado == "entregada" ? "Cobrar" : "Finalizar";

        // Mostrar "Cambiar Estado" SOLO cuando está Lista (mesero marca 'Entregada')
        ShowCambiarEstado = estadoNormalizado == "lista";

        // Cancelar permitido solo en Creada o EnProceso (no en finalizadas o canceladas)
        CanCancel = (estadoNormalizado == "creada" || estadoNormalizado == "enproceso" || estadoNormalizado == "pendiente") 
                   && estadoNormalizado != "finalizada" 
                   && estadoNormalizado != "cancelada";

        // Notificar cambio en EsNueva para actualizar la UI
        OnPropertyChanged(nameof(EsNueva));
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