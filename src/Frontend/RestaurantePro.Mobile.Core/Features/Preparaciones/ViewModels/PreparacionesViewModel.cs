using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Inventory;

namespace RestaurantePro.Mobile.Core.Features.Preparaciones.ViewModels;

/// <summary>
/// ViewModel para gestión de preparaciones de cocina
/// </summary>
public partial class PreparacionesViewModel : BaseViewModel
{
    private readonly IPreparacionesService _preparacionesService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<PreparacionDto> preparaciones;

    [ObservableProperty]
    private PreparacionDto? preparacionSeleccionada;

    [ObservableProperty]
    private string filtroEstado = string.Empty;

    [ObservableProperty]
    private string filtroBusqueda = string.Empty;

    [ObservableProperty]
    private bool mostrarSoloPendientes = true;

    [ObservableProperty]
    private bool mostrarSoloEnPreparacion = false;

    [ObservableProperty]
    private bool mostrarSoloCompletadas = false;

    [ObservableProperty]
    private int paginaActual = 1;

    [ObservableProperty]
    private bool hayMasPreparaciones = true;

    [ObservableProperty]
    private bool estaRefrescando = false;

    public PreparacionesViewModel(
        IPreparacionesService preparacionesService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _preparacionesService = preparacionesService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        Preparaciones = new ObservableCollection<PreparacionDto>();
        
        Title = "Preparaciones";
    }

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        if (IsBusy) return;
        
        IsBusy = true;
        try
        {
            await CargarPreparacionesAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar preparaciones: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OnDisappearingAsync()
    {
        PreparacionSeleccionada = null;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarPreparacionesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var filtros = CrearFiltros();
            var resultado = await _preparacionesService.ObtenerPreparacionesAsync(MostrarSoloPendientes);

            if (resultado.Succeeded)
            {
                if (PaginaActual == 1)
                {
                    Preparaciones.Clear();
                }

                foreach (var preparacion in resultado.Data)
                {
                    Preparaciones.Add(preparacion);
                }

                HayMasPreparaciones = false; // Por ahora, asumimos que no hay paginación
            }
            else
            {
                await _dialogService.ShowErrorAsync(resultado.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar preparaciones: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CargarMasPreparacionesAsync()
    {
        if (IsBusy || !HayMasPreparaciones) return;

        PaginaActual++;
        await CargarPreparacionesAsync();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task RefrescarPreparacionesAsync()
    {
        if (EstaRefrescando) return;

        EstaRefrescando = true;
        try
        {
            PaginaActual = 1;
            await CargarPreparacionesAsync();
        }
        finally
        {
            EstaRefrescando = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task AplicarFiltrosAsync()
    {
        PaginaActual = 1;
        await CargarPreparacionesAsync();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task LimpiarFiltrosAsync()
    {
        FiltroEstado = string.Empty;
        FiltroBusqueda = string.Empty;
        MostrarSoloPendientes = true;
        MostrarSoloEnPreparacion = false;
        MostrarSoloCompletadas = false;
        
        PaginaActual = 1;
        await CargarPreparacionesAsync();
    }

    [RelayCommand]
    private async Task SeleccionarPreparacionAsync(PreparacionDto preparacion)
    {
        if (preparacion == null) return;

        PreparacionSeleccionada = preparacion;
        await _navigationService.NavigateToAsync("preparaciondetalle", new Dictionary<string, object>
        {
            { "preparacionId", preparacion.Id }
        });
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task IniciarPreparacionAsync(PreparacionDto preparacion)
    {
        if (preparacion == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            $"¿Desea iniciar la preparación de {preparacion.NombreProducto}?",
            "Iniciar Preparación");

        if (!confirmacion) return;

        IsBusy = true;
        try
        {
            var dto = new IniciarPreparacionDto
            {
                PreparacionId = preparacion.Id,
                Observaciones = "Iniciado desde móvil"
            };

            var result = await _preparacionesService.IniciarPreparacionAsync(preparacion.Id, dto);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Preparación iniciada exitosamente");
                await RefrescarPreparacionesAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al iniciar preparación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CompletarPreparacionAsync(PreparacionDto preparacion)
    {
        if (preparacion == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            $"¿Desea marcar como completada la preparación de {preparacion.NombreProducto}?",
            "Completar Preparación");

        if (!confirmacion) return;

        IsBusy = true;
        try
        {
            var result = await _preparacionesService.CompletarPreparacionAsync(preparacion.Id);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Preparación completada exitosamente");
                await RefrescarPreparacionesAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al completar preparación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CancelarPreparacionAsync(PreparacionDto preparacion)
    {
        if (preparacion == null) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Motivo de la cancelación:",
            "Cancelar Preparación",
            "Cancelar",
            "Confirmar");

        if (string.IsNullOrWhiteSpace(motivo)) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            $"¿Está seguro de cancelar la preparación de {preparacion.NombreProducto}?",
            "Confirmar Cancelación");

        if (!confirmacion) return;

        IsBusy = true;
        try
        {
            var dto = new CancelarPreparacionDto
            {
                PreparacionId = preparacion.Id,
                MotivoCancelacion = motivo,
                Observaciones = "Cancelado desde móvil"
            };

            var result = await _preparacionesService.CancelarPreparacionAsync(preparacion.Id, dto);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Preparación cancelada exitosamente");
                await RefrescarPreparacionesAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cancelar preparación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CargarColaPreparacionesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _preparacionesService.ObtenerColaPreparacionesAsync();

            if (result.Succeeded)
            {
                Preparaciones.Clear();
                foreach (var preparacion in result.Data)
                {
                    Preparaciones.Add(preparacion);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar cola de preparaciones: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CargarPreparacionesPendientesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _preparacionesService.ObtenerPreparacionesPorEstadoAsync("Pendiente");

            if (result.Succeeded)
            {
                Preparaciones.Clear();
                foreach (var preparacion in result.Data)
                {
                    Preparaciones.Add(preparacion);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar preparaciones pendientes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CargarPreparacionesEnPreparacionAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _preparacionesService.ObtenerPreparacionesPorEstadoAsync("EnPreparacion");

            if (result.Succeeded)
            {
                Preparaciones.Clear();
                foreach (var preparacion in result.Data)
                {
                    Preparaciones.Add(preparacion);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar preparaciones en preparación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CargarPreparacionesCompletadasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _preparacionesService.ObtenerPreparacionesPorEstadoAsync("Completada");

            if (result.Succeeded)
            {
                Preparaciones.Clear();
                foreach (var preparacion in result.Data)
                {
                    Preparaciones.Add(preparacion);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar preparaciones completadas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CrearPreparacionAsync()
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

    [RelayCommand]
    public async Task VerPreparacionAsync(PreparacionDto? preparacion)
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

    private FiltroPreparacionesDto CrearFiltros()
    {
        var filtros = new FiltroPreparacionesDto();

        if (!string.IsNullOrWhiteSpace(FiltroBusqueda))
        {
            filtros.SearchTerm = FiltroBusqueda;
        }

        if (!string.IsNullOrWhiteSpace(FiltroEstado))
        {
            filtros.Estado = FiltroEstado;
        }

        if (MostrarSoloPendientes)
        {
            filtros.Estado = "Pendiente";
        }
        else if (MostrarSoloEnPreparacion)
        {
            filtros.Estado = "EnPreparacion";
        }
        else if (MostrarSoloCompletadas)
        {
            filtros.Estado = "Completada";
        }

        return filtros;
    }
} 