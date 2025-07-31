using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Dialog;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Inventory.Preparaciones.ViewModels;

public partial class PreparacionesViewModel : ObservableObject
{
    private readonly IPreparacionesService _preparacionesService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<PreparacionDto> _preparaciones = new();

    [ObservableProperty]
    private ObservableCollection<PreparacionDto> _preparacionesFiltradas = new();

    [ObservableProperty]
    private string _terminoBusqueda = string.Empty;

    [ObservableProperty]
    private bool _soloDisponibles = true;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private EstadisticasPreparacionesDto? _estadisticas;

    [ObservableProperty]
    private string _categoriaSeleccionada = "Todas";

    public List<string> Categorias { get; } = new() { "Todas", "Entradas", "Platos Principales", "Postres", "Bebidas" };



    public PreparacionesViewModel(IPreparacionesService preparacionesService, IDialogService dialogService)
    {
        _preparacionesService = preparacionesService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task CargarPreparacionesAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _preparacionesService.ObtenerPreparacionesAsync(SoloDisponibles);
            
            if (response.Success)
            {
                Preparaciones.Clear();
                foreach (var preparacion in response.Data)
                {
                    Preparaciones.Add(preparacion);
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
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar preparaciones: {ex.Message}");
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
            var response = await _preparacionesService.ObtenerEstadisticasAsync();
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
    private async Task BuscarPreparacionesAsync()
    {
        if (string.IsNullOrWhiteSpace(TerminoBusqueda))
        {
            AplicarFiltros();
            return;
        }

        try
        {
            IsLoading = true;
            var response = await _preparacionesService.BuscarPreparacionesAsync(TerminoBusqueda);
            
            if (response.Success)
            {
                PreparacionesFiltradas.Clear();
                foreach (var preparacion in response.Data)
                {
                    PreparacionesFiltradas.Add(preparacion);
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al buscar preparaciones: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CambiarDisponibilidadAsync(PreparacionDto preparacion)
    {
        try
        {
            var response = await _preparacionesService.CambiarDisponibilidadAsync(preparacion.Id, !preparacion.Disponible);
            
            if (response.Success)
            {
                preparacion.Disponible = !preparacion.Disponible;
                await _dialogService.ShowAlertAsync("Éxito", "Disponibilidad actualizada correctamente");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cambiar disponibilidad: {ex.Message}");
        }
    }

    partial void OnTerminoBusquedaChanged(string value)
    {
        AplicarFiltros();
    }

    partial void OnSoloDisponiblesChanged(bool value)
    {
        _ = CargarPreparacionesAsync();
    }

    partial void OnCategoriaSeleccionadaChanged(string value)
    {
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        PreparacionesFiltradas.Clear();
        
        var preparacionesFiltradas = Preparaciones.AsEnumerable();

        // Filtrar por categoría
        if (CategoriaSeleccionada != "Todas")
        {
            preparacionesFiltradas = preparacionesFiltradas.Where(p => p.Categoria == CategoriaSeleccionada);
        }

        // Filtrar por término de búsqueda
        if (!string.IsNullOrWhiteSpace(TerminoBusqueda))
        {
            preparacionesFiltradas = preparacionesFiltradas.Where(p => 
                p.Nombre.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                p.Descripcion.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var preparacion in preparacionesFiltradas)
        {
            PreparacionesFiltradas.Add(preparacion);
        }
    }

    /// <summary>
    /// Refrescar preparaciones
    /// </summary>
    [RelayCommand]
    private async Task RefreshPreparacionesAsync()
    {
        await CargarPreparacionesAsync();
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
    /// Cargar preparaciones urgentes (temporalmente deshabilitado)
    /// </summary>
    [RelayCommand]
    private async Task LoadPreparacionesUrgentesAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La carga de preparaciones urgentes no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Crear nueva preparación
    /// </summary>
    [RelayCommand]
    private async Task CrearPreparacionAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La creación de preparaciones no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Ver preparación
    /// </summary>
    [RelayCommand]
    private async Task VerPreparacionAsync(PreparacionDto? preparacion)
    {
        if (preparacion == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Detalles de Preparación", 
                $"Nombre: {preparacion.Nombre}\nCategoría: {preparacion.Categoria}\nDisponible: {(preparacion.Disponible ? "Sí" : "No")}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al ver preparación: {ex.Message}");
        }
    }

    /// <summary>
    /// Iniciar preparación
    /// </summary>
    [RelayCommand]
    private async Task IniciarPreparacionAsync(PreparacionDto? preparacion)
    {
        if (preparacion == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La iniciación de preparaciones no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Completar preparación
    /// </summary>
    [RelayCommand]
    private async Task CompletarPreparacionAsync(PreparacionDto? preparacion)
    {
        if (preparacion == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La completación de preparaciones no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }
} 