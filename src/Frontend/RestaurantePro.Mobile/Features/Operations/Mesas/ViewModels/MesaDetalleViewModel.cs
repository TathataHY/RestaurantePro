using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Features.Operations.Mesas.ViewModels;

/// <summary>
/// ViewModel para la página de detalle de mesa
/// </summary>
public partial class MesaDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMesasService _mesasService;
    private readonly IComandasService _comandasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private MesaDto mesa;

    [ObservableProperty]
    private ObservableCollection<ComandaDto> comandasActivas;

    [ObservableProperty]
    private Guid mesaId;

    /// <summary>
    /// Constructor para inyección de dependencias
    /// </summary>
    public MesaDetalleViewModel(
        IMesasService mesasService,
        IComandasService comandasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _mesasService = mesasService;
        _comandasService = comandasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        Title = "Detalle de Mesa";
        Mesa = new MesaDto();
        ComandasActivas = new ObservableCollection<ComandaDto>();
    }

    /// <summary>
    /// Implementación de IQueryAttributable para recibir parámetros de navegación
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mesaId", out var mesaIdObj) && 
            Guid.TryParse(mesaIdObj?.ToString(), out var mesaIdParsed))
        {
            MesaId = mesaIdParsed;
        }
    }

    /// <summary>
    /// Propiedades calculadas para la UI
    /// </summary>
    public bool PuedeAsignar => Mesa?.Estado == "disponible";
    public bool PuedeLiberar => Mesa?.Estado == "ocupada";
    public bool TieneComandasActivas => ComandasActivas?.Any() == true;

    /// <summary>
    /// Inicializar el ViewModel con ID de mesa
    /// </summary>
    public async Task InitializeAsync(Guid mesaId)
    {
        MesaId = mesaId;
        await InitializeAsync();
    }

    /// <summary>
    /// Inicializar datos de la mesa
    /// </summary>
    public async Task InitializeAsync()
    {
        if (MesaId == Guid.Empty) return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            await LoadMesaAsync();
            await LoadComandasActivasAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cargar información de la mesa
    /// </summary>
    [RelayCommand]
    private async Task LoadMesaAsync()
    {
        try
        {
            var result = await _mesasService.ObtenerMesaAsync(MesaId);
            
            if (result.Success)
            {
                Mesa = result.Data ?? new MesaDto();
                OnPropertyChanged(nameof(PuedeAsignar));
                OnPropertyChanged(nameof(PuedeLiberar));
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar mesa: {ex.Message}");
        }
    }

    /// <summary>
    /// Cargar comandas activas de la mesa
    /// </summary>
    [RelayCommand]
    private async Task LoadComandasActivasAsync()
    {
        try
        {
            var result = await _comandasService.ObtenerComandasPorMesaAsync(MesaId);
            
            if (result.Success)
            {
                ComandasActivas.Clear();
                var comandasActivas = result.Data?.Where(c => c.Estado != "finalizada" && c.Estado != "cancelada") ?? [];
                foreach (var comanda in comandasActivas)
                {
                    ComandasActivas.Add(comanda);
                }
                OnPropertyChanged(nameof(TieneComandasActivas));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar comandas: {ex.Message}");
        }
    }

    /// <summary>
    /// Asignar mesa a cliente
    /// </summary>
    [RelayCommand]
    private async Task AsignarMesaAsync()
    {
        try
        {
            var nombreCliente = await _dialogService.ShowPromptAsync(
                "Asignar Mesa", 
                "Ingrese el nombre del cliente:", 
                "Confirmar", 
                "Cancelar",
                "Nombre del cliente");

            if (string.IsNullOrWhiteSpace(nombreCliente))
                return;

            var numeroPersonasStr = await _dialogService.ShowPromptAsync(
                "Número de Personas", 
                "¿Cuántas personas?:", 
                "Confirmar", 
                "Cancelar",
                "2");

            if (!int.TryParse(numeroPersonasStr, out var numeroPersonas) || numeroPersonas <= 0)
                numeroPersonas = 2; // Valor por defecto

            IsBusy = true;
            var result = await _mesasService.AsignarMesaAsync(
                MesaId, 
                clienteId: null, // No tenemos clienteId específico
                numeroPersonas: numeroPersonas, 
                observaciones: $"Mesa asignada a {nombreCliente}");

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Mesa asignada correctamente");
                await LoadMesaAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al asignar mesa: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Liberar mesa
    /// </summary>
    [RelayCommand]
    private async Task LiberarMesaAsync()
    {
        try
        {
            var confirmacion = await _dialogService.ShowConfirmAsync(
                "Liberar Mesa", 
                $"¿Está seguro de liberar la Mesa {Mesa.Numero}?");

            if (!confirmacion)
                return;

            IsBusy = true;
            var result = await _mesasService.LiberarMesaAsync(MesaId, "Mesa liberada desde móvil");

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Mesa liberada correctamente");
                await LoadMesaAsync();
                await LoadComandasActivasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al liberar mesa: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cambiar estado de la mesa
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoAsync()
    {
        try
        {
            var opciones = new[] { "disponible", "ocupada", "reservada", "mantenimiento" };
            var estadoSeleccionado = await _dialogService.ShowActionSheetAsync(
                "Cambiar Estado", 
                "Seleccione el nuevo estado:",
                "Cancelar",
                opciones);

            if (string.IsNullOrEmpty(estadoSeleccionado) || estadoSeleccionado == "Cancelar")
                return;

            IsBusy = true;
            var result = await _mesasService.CambiarEstadoMesaAsync(
                MesaId, 
                estadoSeleccionado, 
                motivo: $"Estado cambiado a {estadoSeleccionado}");

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Estado actualizado correctamente");
                await LoadMesaAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cambiar estado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Ver comandas de la mesa
    /// </summary>
    [RelayCommand]
    private async Task VerComandasAsync()
    {
        try
        {
            await _navigationService.NavigateToAsync($"comandas?mesaId={MesaId}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al navegar: {ex.Message}");
        }
    }

    /// <summary>
    /// Limpiar datos al salir
    /// </summary>
    public void Cleanup()
    {
        ComandasActivas.Clear();
        Mesa = new MesaDto();
        HasError = false;
        ErrorMessage = string.Empty;
    }
} 