using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Operations.Cocina.ViewModels;

/// <summary>
/// ViewModel para la pantalla de cocina - Gestión de comandas desde cocina
/// </summary>
public partial class ModernCocinaViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly IDialogService _dialogService;
    private readonly RestaurantePro.Mobile.Core.Services.Notifications.INotificationService _notificationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<ComandaDto> comandas = new();

    [ObservableProperty]
    private ObservableCollection<ComandaDto> comandasFiltradas = new();

    [ObservableProperty]
    private EstadisticasCocinaDto? estadisticas;

    [ObservableProperty]
    private string filtroEstado = "Todas";

    [ObservableProperty]
    private bool isRefreshing;

    #endregion

    #region Estados Disponibles

    public List<string> EstadosDisponibles => new()
    {
        "Todas",
        "Creada",
        "En Proceso", 
        "Lista"
    };

    #endregion

    #region Constructor

    public ModernCocinaViewModel(
        IComandasService comandasService,
        IDialogService dialogService,
        RestaurantePro.Mobile.Core.Services.Notifications.INotificationService notificationService)
    {
        _comandasService = comandasService;
        _dialogService = dialogService;
        _notificationService = notificationService;
        
        Title = "Cocina";
        
        // Cargar datos iniciales
        _ = LoadComandasAsync();
        _ = LoadEstadisticasAsync();
    }

    #endregion

    #region Comandos Principales

    /// <summary>
    /// Cargar comandas para cocina
    /// </summary>
    [RelayCommand]
    private async Task LoadComandasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        IsRefreshing = true;
        ErrorMessage = string.Empty;

        try
        {
            // Cargar comandas que cocina puede manejar (Creada, EnProceso, Lista)
            // Usar el método que funciona correctamente (el mismo que usa el detalle de mesa)
            var response = await _comandasService.ObtenerComandasActivasAsync();

            if (response.Success)
            {
                var comandasFiltradas = response.Data ?? new List<ComandaDto>();
                
                // Filtrar solo comandas que cocina puede manejar
                comandasFiltradas = comandasFiltradas.Where(c => 
                    c.Estado == "Creada" || c.Estado == "EnProceso" || c.Estado == "Lista").ToList();

                // Detectar nuevas comandas para notificar
                var nuevosIds = comandasFiltradas
                    .Select(c => c.Id)
                    .Except(Comandas.Select(c => c.Id))
                    .ToList();

                Comandas.Clear();
                foreach (var comanda in comandasFiltradas)
                {
                    Comandas.Add(comanda);
                }
                
                AplicarFiltros();

                if (nuevosIds.Any())
                {
                    await _notificationService.VibrateAsync(120);
                    await _notificationService.ShowToastAsync(
                        nuevosIds.Count == 1 ? "Nueva comanda recibida" : $"{nuevosIds.Count} nuevas comandas");
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al cargar comandas");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Cargar estadísticas de cocina
    /// </summary>
    [RelayCommand]
    private async Task LoadEstadisticasAsync()
    {
        try
        {
            // Por ahora, calcular estadísticas localmente
            // TODO: Implementar endpoint específico para estadísticas de cocina
            Estadisticas = new EstadisticasCocinaDto
            {
                ComandasCreadas = Comandas.Count(c => c.Estado == "Creada"),
                ComandasEnProceso = Comandas.Count(c => c.Estado == "EnProceso"),
                ComandasListas = Comandas.Count(c => c.Estado == "Lista"),
                TiempoPromedio = CalcularTiempoPromedio()
            };
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar estadísticas: {ex.Message}");
        }
    }

    /// <summary>
    /// Tomar comanda para preparar (Creada → EnProceso)
    /// </summary>
    [RelayCommand]
    private async Task TomarComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Tomar Comanda",
            $"¿Está seguro que desea tomar la comanda #{comanda.NumeroDisplay} para preparar?");

        if (!confirmacion) return;

        IsBusy = true;
        var shouldRefresh = false;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(
                comanda.Id, 
                "enproceso", 
                "Tomada por cocina para preparar");

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.NumeroDisplay} tomada para preparar");
                shouldRefresh = true;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al tomar la comanda");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            if (shouldRefresh)
            {
                await RefreshComandasCommand.ExecuteAsync(null);
                await LoadEstadisticasCommand.ExecuteAsync(null);
            }
        }
    }

    /// <summary>
    /// Marcar comanda como lista (EnProceso → Lista)
    /// </summary>
    [RelayCommand]
    private async Task MarcarListaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Marcar como Lista",
            $"¿Está seguro que la comanda #{comanda.NumeroDisplay} está lista para entregar?");

        if (!confirmacion) return;

        IsBusy = true;
        var shouldRefresh = false;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(
                comanda.Id, 
                "lista", 
                "Comanda lista para entregar");

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.NumeroDisplay} marcada como lista");
                shouldRefresh = true;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al marcar como lista");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            if (shouldRefresh)
            {
                await RefreshComandasCommand.ExecuteAsync(null);
            }
        }
    }

    /// <summary>
    /// Ver detalle de comanda
    /// </summary>
    [RelayCommand]
    private async Task VerDetalleAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        try
        {
            var detalle = $"Comanda #{comanda.NumeroDisplay}\n" +
                         $"Mesa: {comanda.MesaNumeroDisplay}\n" +
                         $"Estado: {comanda.Estado}\n" +
                         $"Total: {comanda.Total:C}\n" +
                         $"Productos: {comanda.ProductosResumen}";

            if (!string.IsNullOrWhiteSpace(comanda.Observaciones))
            {
                detalle += $"\n\nObservaciones:\n{comanda.Observaciones}";
            }

            await _dialogService.ShowAlertAsync("Detalle de Comanda", detalle);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al mostrar detalle: {ex.Message}");
        }
    }

    /// <summary>
    /// Refrescar comandas
    /// </summary>
    [RelayCommand]
    private async Task RefreshComandasAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadComandasAsync();
            await LoadEstadisticasAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Aplicar filtros a las comandas
    /// </summary>
    private void AplicarFiltros()
    {
        ComandasFiltradas.Clear();
        
        var comandasFiltradas = Comandas.AsEnumerable();

        // Filtrar por estado
        if (FiltroEstado != "Todas")
        {
            comandasFiltradas = comandasFiltradas.Where(c => c.Estado == FiltroEstado);
        }

        // Ordenar por fecha de creación (más antiguas primero)
        comandasFiltradas = comandasFiltradas.OrderBy(c => c.FechaCreacion);

        foreach (var comanda in comandasFiltradas)
        {
            ComandasFiltradas.Add(comanda);
        }
    }

    /// <summary>
    /// Calcular tiempo promedio de preparación
    /// </summary>
    private int CalcularTiempoPromedio()
    {
        // Por ahora retornar un valor fijo
        // TODO: Implementar cálculo real basado en historial
        return 15;
    }

    #endregion

    #region Event Handlers

    partial void OnFiltroEstadoChanged(string value)
    {
        AplicarFiltros();
    }

    #endregion
}

/// <summary>
/// DTO para estadísticas de cocina
/// </summary>
public class EstadisticasCocinaDto
{
    public int ComandasCreadas { get; set; }
    public int ComandasEnProceso { get; set; }
    public int ComandasListas { get; set; }
    public int TiempoPromedio { get; set; }
}
