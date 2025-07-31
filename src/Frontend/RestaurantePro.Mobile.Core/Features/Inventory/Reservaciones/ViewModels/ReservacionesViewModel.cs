using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Dialog;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Inventory.Reservaciones.ViewModels;

public partial class ReservacionesViewModel : ObservableObject
{
    private readonly IReservacionesService _reservacionesService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<ReservacionDto> _reservaciones = new();

    [ObservableProperty]
    private ObservableCollection<ReservacionDto> _reservacionesFiltradas = new();

    [ObservableProperty]
    private string _terminoBusqueda = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private EstadisticasReservacionesDto? _estadisticas;

    [ObservableProperty]
    private string _estadoFiltro = "Todas";

    [ObservableProperty]
    private DateTime _fechaSeleccionada = DateTime.Today;

    public List<string> Estados { get; } = new() { "Todas", "Confirmada", "Pendiente", "Cancelada", "Completada" };



    public ReservacionesViewModel(IReservacionesService reservacionesService, IDialogService dialogService)
    {
        _reservacionesService = reservacionesService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task CargarReservacionesAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _reservacionesService.ObtenerReservacionesAsync();
            
            if (response.Success)
            {
                Reservaciones.Clear();
                foreach (var reservacion in response.Data)
                {
                    Reservaciones.Add(reservacion);
                }
                AplicarFiltros();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar reservaciones: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CargarReservacionesHoyAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _reservacionesService.ObtenerReservacionesHoyAsync();
            
            if (response.Success)
            {
                Reservaciones.Clear();
                foreach (var reservacion in response.Data)
                {
                    Reservaciones.Add(reservacion);
                }
                AplicarFiltros();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar reservaciones de hoy: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CargarEstadisticasAsync()
    {
        try
        {
            var response = await _reservacionesService.ObtenerEstadisticasAsync();
            if (response.Success)
            {
                Estadisticas = response.Data;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar estadísticas: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BuscarReservacionesAsync()
    {
        if (string.IsNullOrWhiteSpace(TerminoBusqueda))
        {
            AplicarFiltros();
            return;
        }

        try
        {
            IsLoading = true;
            var response = await _reservacionesService.BuscarReservacionesAsync(TerminoBusqueda);
            
            if (response.Success)
            {
                ReservacionesFiltradas.Clear();
                foreach (var reservacion in response.Data)
                {
                    ReservacionesFiltradas.Add(reservacion);
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al buscar reservaciones: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync(ReservacionDto reservacion)
    {
        try
        {
            var estadosDisponibles = new[] { "Confirmada", "Pendiente", "Cancelada", "Completada" };
            var nuevoEstado = await _dialogService.ShowActionSheetAsync("Cambiar Estado", "Cancelar", null, estadosDisponibles);
            
            if (!string.IsNullOrEmpty(nuevoEstado) && nuevoEstado != "Cancelar")
            {
                var response = await _reservacionesService.CambiarEstadoReservacionAsync(reservacion.Id, nuevoEstado);
                
                if (response.Success)
                {
                    reservacion.Estado = nuevoEstado;
                    await _dialogService.ShowAlertAsync("Éxito", "Estado actualizado correctamente");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Error", response.Message);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cambiar estado: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AsignarMesaAsync(ReservacionDto reservacion)
    {
        try
        {
            var mesa = await _dialogService.ShowPromptAsync("Asignar Mesa", "Ingrese el número de mesa:", "Asignar", "Cancelar", "Mesa");
            
            if (!string.IsNullOrEmpty(mesa) && mesa != "Cancelar")
            {
                var response = await _reservacionesService.AsignarMesaAsync(reservacion.Id, mesa);
                
                if (response.Success)
                {
                    reservacion.MesaAsignada = mesa;
                    await _dialogService.ShowAlertAsync("Éxito", "Mesa asignada correctamente");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Error", response.Message);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al asignar mesa: {ex.Message}");
        }
    }

    partial void OnTerminoBusquedaChanged(string value)
    {
        AplicarFiltros();
    }

    partial void OnEstadoFiltroChanged(string value)
    {
        AplicarFiltros();
    }

    partial void OnFechaSeleccionadaChanged(DateTime value)
    {
        _ = CargarReservacionesAsync();
    }

    private void AplicarFiltros()
    {
        ReservacionesFiltradas.Clear();
        
        var reservacionesFiltradas = Reservaciones.AsEnumerable();

        // Filtrar por estado
        if (EstadoFiltro != "Todas")
        {
            reservacionesFiltradas = reservacionesFiltradas.Where(r => r.Estado == EstadoFiltro);
        }

        // Filtrar por término de búsqueda
        if (!string.IsNullOrWhiteSpace(TerminoBusqueda))
        {
            reservacionesFiltradas = reservacionesFiltradas.Where(r => 
                r.NombreCliente.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                r.Telefono.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                r.Email.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var reservacion in reservacionesFiltradas)
        {
            ReservacionesFiltradas.Add(reservacion);
        }
    }

    /// <summary>
    /// Refrescar reservaciones
    /// </summary>
    [RelayCommand]
    private async Task RefreshReservacionesAsync()
    {
        await CargarReservacionesAsync();
    }

    /// <summary>
    /// Cargar estadísticas
    /// </summary>
    [RelayCommand]
    private async Task LoadEstadisticasAsync()
    {
        await CargarEstadisticasAsync();
    }

    /// <summary>
    /// Ver calendario
    /// </summary>
    [RelayCommand]
    private async Task VerCalendarioAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La vista de calendario no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Crear nueva reservación
    /// </summary>
    [RelayCommand]
    private async Task CrearReservacionAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La creación de reservaciones no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Ver reservación
    /// </summary>
    [RelayCommand]
    private async Task VerReservacionAsync(ReservacionDto? reservacion)
    {
        if (reservacion == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Detalles de Reservación", 
                $"Cliente: {reservacion.NombreCliente}\nFecha: {reservacion.FechaReservacion:dd/MM/yyyy}\nHora: {reservacion.HoraReservacion:HH:mm}\nPersonas: {reservacion.NumeroPersonas}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al ver reservación: {ex.Message}");
        }
    }

    /// <summary>
    /// Editar reservación
    /// </summary>
    [RelayCommand]
    private async Task EditarReservacionAsync(ReservacionDto? reservacion)
    {
        if (reservacion == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La edición de reservaciones no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Confirmar reservación
    /// </summary>
    [RelayCommand]
    private async Task ConfirmarReservacionAsync(ReservacionDto? reservacion)
    {
        if (reservacion == null) return;

        try
        {
            await CambiarEstadoAsync(reservacion);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al confirmar reservación: {ex.Message}");
        }
    }
} 