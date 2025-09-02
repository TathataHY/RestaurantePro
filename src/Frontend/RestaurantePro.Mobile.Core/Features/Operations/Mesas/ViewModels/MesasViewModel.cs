using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

/// <summary>
/// ViewModel para gestión de mesas - Funcionalidad operativa principal
/// </summary>
public partial class MesasViewModel : BaseViewModel
{
    private readonly IMesasService _mesasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly SemaphoreSlim _loadingSemaphore = new(1, 1);

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<MesaDto> mesas = new();

    [ObservableProperty]
    private MesaDto? selectedMesa;

    [ObservableProperty]
    private EstadoMesasDto? estadoMesas;

    [ObservableProperty]
    private string filtroEstado = string.Empty;

    [ObservableProperty]
    private string filtroUbicacion = string.Empty;

    [ObservableProperty]
    private int? filtroCapacidadMinima;

    [ObservableProperty]
    private string filtroCapacidad = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor sin parámetros requerido por XAML
    /// </summary>
    public MesasViewModel()
    {
        Title = "Gestión de Mesas";
    }

    /// <summary>
    /// Constructor principal con dependencias
    /// </summary>
    public MesasViewModel(
        IMesasService mesasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _mesasService = mesasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        Title = "Gestión de Mesas";
        
        // Cargar datos iniciales
        _ = LoadMesasAsync();
    }

    #endregion

    #region Comandos Principales

    /// <summary>
    /// Cargar todas las mesas con filtros aplicados
    /// </summary>
    [RelayCommand]
    public async Task LoadMesasAsync()
    {
        await LoadMesasAsync(FiltroEstado, FiltroUbicacion, FiltroCapacidadMinima);
    }

    private async Task LoadMesasAsync(string? estado, string? ubicacion, int? capacidadMinima)
    {
        // Esperar a que termine cualquier petición en curso
        await _loadingSemaphore.WaitAsync();
        
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($"🔍 MesasViewModel.LoadMesasAsync - Iniciando carga de mesas");
            System.Diagnostics.Debug.WriteLine($"🔍 Filtros: Estado={estado}, Ubicacion={ubicacion}, CapacidadMinima={capacidadMinima}");

            var response = await _mesasService.ObtenerMesasAsync(
                string.IsNullOrWhiteSpace(estado) ? null : estado,
                string.IsNullOrWhiteSpace(ubicacion) ? null : ubicacion,
                capacidadMinima);

            if (response.Success)
            {
                Mesas.Clear();
                foreach (var mesa in response.Data ?? new List<MesaDto>())
                {
                    Mesas.Add(mesa);
                }
                System.Diagnostics.Debug.WriteLine($"✅ Se cargaron {response.Data?.Count ?? 0} mesas");
            }
            else
            {
                await ShowErrorAsync(response.Message ?? "Error al cargar las mesas");
                await _dialogService.ShowAlertAsync("Error", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error inesperado: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", ErrorMessage);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
            _loadingSemaphore.Release();
        }
    }

    /// <summary>
    /// Refrescar la lista de mesas
    /// </summary>
    [RelayCommand]
    private async Task RefreshMesasAsync()
    {
        IsRefreshing = true;
        await LoadMesasAsync();
    }

    /// <summary>
    /// Cargar estadísticas de ocupación
    /// </summary>
    [RelayCommand]
    public async Task LoadEstadisticasAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔍 MesasViewModel.LoadEstadisticasAsync - Iniciando");

        // Esperar a que termine cualquier petición en curso
        await _loadingSemaphore.WaitAsync();
        
        try
        {
            IsBusy = true;
            System.Diagnostics.Debug.WriteLine("🔍 Llamando a _mesasService.ObtenerEstadoOcupacionAsync()");
            var response = await _mesasService.ObtenerEstadoOcupacionAsync();

            System.Diagnostics.Debug.WriteLine($"🔍 Respuesta: Success={response.Success}, Data={response.Data != null}");

            if (response.Success)
            {
                EstadoMesas = response.Data;
                System.Diagnostics.Debug.WriteLine($"✅ Estadísticas cargadas: TotalMesas={EstadoMesas?.TotalMesas}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {response.Message}");
                await _dialogService.ShowAlertAsync("Error", "No se pudieron cargar las estadísticas");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Excepción: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar estadísticas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            _loadingSemaphore.Release();
        }
    }

    #endregion

    #region Comandos de Operaciones

    /// <summary>
    /// Asignar una mesa a un cliente
    /// </summary>
    [RelayCommand]
    private async Task AsignarMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var clienteId = await _dialogService.ShowPromptAsync(
            "Asignar Mesa",
            $"Ingrese el ID del cliente para la mesa {mesa.Numero}:");

        if (string.IsNullOrWhiteSpace(clienteId))
            return;

        if (!Guid.TryParse(clienteId, out var clienteGuid))
        {
            await _dialogService.ShowAlertAsync("Error", "ID de cliente inválido");
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _mesasService.AsignarMesaAsync(mesa.Id, clienteGuid);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Mesa {mesa.Numero} asignada correctamente");
                await LoadMesasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al asignar la mesa");
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
    /// Liberar una mesa
    /// </summary>
    [RelayCommand]
    private async Task LiberarMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar",
            $"¿Está seguro que desea liberar la mesa {mesa.Numero}?");

        if (!confirmacion) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Motivo de Liberación",
            "Ingrese el motivo para liberar la mesa:");

        if (string.IsNullOrWhiteSpace(motivo))
            return;

        IsBusy = true;

        try
        {
            var response = await _mesasService.LiberarMesaAsync(mesa.Id, motivo);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Mesa {mesa.Numero} liberada correctamente");
                await LoadMesasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al liberar la mesa");
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
    /// Cambiar estado de una mesa
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var nuevoEstado = await _dialogService.ShowActionSheetAsync(
            "Cambiar Estado",
            "Seleccione el nuevo estado:",
            "Cancelar",
            "Disponible", "Ocupada", "Reservada", "Mantenimiento");

        if (string.IsNullOrWhiteSpace(nuevoEstado) || nuevoEstado == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _mesasService.CambiarEstadoMesaAsync(mesa.Id, nuevoEstado);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Estado de mesa {mesa.Numero} cambiado a {nuevoEstado}");
                await LoadMesasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al cambiar el estado");
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
    /// Buscar la mejor mesa disponible
    /// </summary>
    [RelayCommand]
    private async Task BuscarMejorMesaAsync()
    {
        var capacidadStr = await _dialogService.ShowPromptAsync(
            "Buscar Mesa",
            "Ingrese la capacidad requerida:");

        if (string.IsNullOrWhiteSpace(capacidadStr))
            return;

        if (!int.TryParse(capacidadStr, out var capacidad) || capacidad <= 0)
        {
            await _dialogService.ShowAlertAsync("Error", "Capacidad inválida");
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _mesasService.BuscarMejorMesaAsync(capacidad);

            if (response.Success && response.Data != null)
            {
                var mesa = response.Data;
                var mensaje = $"Mesa sugerida: {mesa.Numero}\nCapacidad: {mesa.Capacidad}\nUbicación: {mesa.Zona}";
                
                var asignar = await _dialogService.ShowConfirmAsync(
                    "Mesa Encontrada",
                    mensaje + "\n\n¿Desea asignar esta mesa?");

                if (asignar)
                {
                    await AsignarMesaAsync(mesa);
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Información", "No se encontró una mesa disponible con esa capacidad");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al buscar mesa: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Comandos de Filtros

    /// <summary>
    /// Aplicar filtros de búsqueda
    /// </summary>
        [RelayCommand]
        private async Task ApplyFiltersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 ApplyFiltersAsync - FiltroEstado original: '{FiltroEstado}'");
                
                // Procesar filtro de estado
                string? estadoFiltro = null;
                if (!string.IsNullOrWhiteSpace(FiltroEstado) && FiltroEstado != "Todas")
                {
                    // Mapear los valores del frontend a los valores del backend
                    estadoFiltro = FiltroEstado switch
                    {
                        "Disponibles" => "Disponible",
                        "Ocupadas" => "Ocupada", 
                        "Reservadas" => "Reservada",
                        "Fuera de servicio" => "FueraDeServicio",
                        "En limpieza" => "EnLimpieza",
                        _ => FiltroEstado
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"🔍 ApplyFiltersAsync - Estado mapeado: '{estadoFiltro}'");
                }

                // Procesar filtro de capacidad
                int? capacidadMinima = null;
                if (!string.IsNullOrWhiteSpace(FiltroCapacidad) && FiltroCapacidad != "Todas")
                {
                    var capacidadStr = FiltroCapacidad.Replace(" personas", "").Replace("+", "");
                    if (int.TryParse(capacidadStr, out var capacidad))
                    {
                        capacidadMinima = capacidad;
                    }
                }

                // NO actualizar FiltroEstado aquí - mantener el valor del frontend para el Picker
                // Solo actualizar FiltroCapacidadMinima para el procesamiento interno
                FiltroCapacidadMinima = capacidadMinima;

            System.Diagnostics.Debug.WriteLine($"🔍 Aplicando filtros: Estado={estadoFiltro}, CapacidadMinima={capacidadMinima}");

            // Llamar a LoadMesasAsync con los valores mapeados del backend
            await LoadMesasAsync(estadoFiltro, FiltroUbicacion, capacidadMinima);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error aplicando filtros: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", $"Error al aplicar filtros: {ex.Message}");
        }
    }

    /// <summary>
    /// Limpiar todos los filtros
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        FiltroEstado = string.Empty;
        FiltroUbicacion = string.Empty;
        FiltroCapacidadMinima = null;
        FiltroCapacidad = string.Empty;
        SearchText = string.Empty;
        
        await LoadMesasAsync();
    }

    #endregion

    #region Comandos de Navegación

    /// <summary>
    /// Navegar a la página de detalle de mesa
    /// </summary>
    [RelayCommand]
    private async Task NavigateToMesaDetailAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        await _navigationService.NavigateToAsync("mesa-detalle", new Dictionary<string, object>
        {
            ["mesaId"] = mesa.Id.ToString()
        });
    }

    /// <summary>
    /// Navegar a crear nueva comanda para la mesa
    /// </summary>
    [RelayCommand]
    private async Task NavigateToNewComandaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        await _navigationService.NavigateToAsync("comandas", new Dictionary<string, object>
        {
            ["mesaId"] = mesa.Id.ToString()
        });
    }

    #endregion

    #region Comandos Adicionales

    /// <summary>
    /// Crear nueva mesa
    /// </summary>
    [RelayCommand]
    private async Task CreateMesaAsync()
    {
        try
        {
            // Solicitar datos de la nueva mesa
            var numeroStr = await _dialogService.ShowPromptAsync(
                "Nueva Mesa", 
                "Ingrese el número de la mesa:");
            
            if (string.IsNullOrWhiteSpace(numeroStr))
                return;

            if (!int.TryParse(numeroStr, out var numero) || numero <= 0)
            {
                await _dialogService.ShowAlertAsync("Error", "Número de mesa inválido");
                return;
            }

            var capacidadStr = await _dialogService.ShowPromptAsync(
                "Nueva Mesa", 
                "Ingrese la capacidad de la mesa:");
            
            if (string.IsNullOrWhiteSpace(capacidadStr))
                return;

            if (!int.TryParse(capacidadStr, out var capacidad) || capacidad <= 0)
            {
                await _dialogService.ShowAlertAsync("Error", "Capacidad inválida");
                return;
            }

            var ubicacion = await _dialogService.ShowPromptAsync(
                "Nueva Mesa", 
                "Ingrese la ubicación de la mesa (opcional):");

            // Por ahora solo mostramos un mensaje de confirmación
            // En el futuro aquí se haría la llamada a la API para crear la mesa
            var mensaje = $"Mesa {numero} creada:\n" +
                         $"• Capacidad: {capacidad} personas\n" +
                         $"• Ubicación: {ubicacion ?? "No especificada"}\n" +
                         $"• Estado: Disponible";

            await _dialogService.ShowAlertAsync("Mesa Creada", mensaje);
            
            // Recargar la lista de mesas
            await LoadMesasAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al crear mesa: {ex.Message}");
        }
    }

    /// <summary>
    /// Cargar estadísticas de ocupación (comando público)
    /// </summary>
    [RelayCommand]
    public async Task LoadEstadisticasCommandAsync()
    {
        try
        {
            await LoadEstadisticasAsync();
            
            if (EstadoMesas != null)
            {
                var mensaje = $"📊 Estadísticas de Mesas:\n\n" +
                             $"• Total de mesas: {EstadoMesas.TotalMesas}\n" +
                             $"• Disponibles: {EstadoMesas.MesasDisponibles.Count}\n" +
                             $"• Ocupadas: {EstadoMesas.MesasOcupadas.Count}\n" +
                             $"• Reservadas: {EstadoMesas.MesasReservadas.Count}\n\n" +
                             $"• Ocupación actual: {EstadoMesas.Estadisticas.PorcentajeOcupacion:F1}%";

                await _dialogService.ShowAlertAsync("Estadísticas de Mesas", mensaje);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Información", "No se pudieron cargar las estadísticas");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar estadísticas: {ex.Message}");
        }
    }

    #endregion

    #region Métodos de Utilidad

    /// <summary>
    /// Obtener color basado en el estado de la mesa
    /// </summary>
    public string GetEstadoColor(string estado)
    {
        return estado?.ToLower() switch
        {
            "disponible" => "Green",
            "ocupada" => "Red",
            "reservada" => "Orange",
            "mantenimiento" => "Gray",
            _ => "Black"
        };
    }

    /// <summary>
    /// Obtener icono basado en el estado de la mesa
    /// </summary>
    public string GetEstadoIcon(string estado)
    {
        return estado?.ToLower() switch
        {
            "disponible" => "✓",
            "ocupada" => "●",
            "reservada" => "⏰",
            "mantenimiento" => "🔧",
            _ => "?"
        };
    }

    #endregion
} 
