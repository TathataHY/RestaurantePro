using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Models.Common;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;
using RestaurantePro.Mobile.Core.Services.Realtime;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Preferences;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

/// <summary>
/// ViewModel para gestión de comandas - Funcionalidad operativa principal
/// </summary>
public partial class ComandasViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IMesasService _mesasService;
    private readonly IComandaRealtimeService _realtimeService;
    private readonly INotificationService _notificationService;
    private readonly IPreferencesService _preferencesService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<ComandaDto> comandas = new();

    [ObservableProperty]
    private ComandaDto? selectedComanda;

    [ObservableProperty]
    private EstadisticasComandasDto? estadisticas;

    [ObservableProperty]
    private string filtroEstado = string.Empty;

    [ObservableProperty]
    private DateTime? filtroFecha;

    [ObservableProperty]
    private Guid? filtroMesaId;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool soloActivas = true;

    #endregion

    #region Estados Disponibles

    public List<string> EstadosDisponibles => new()
    {
        "Todos",
        "Pendiente",
        "En Preparación", 
        "Lista",
        "Entregada",
        "Cancelada",
        "Finalizada"
    };

    #endregion

    #region Constructor

    public ComandasViewModel(
        IComandasService comandasService,
        IDialogService dialogService,
        INavigationService navigationService,
        IMesasService mesasService,
        INotificationService notificationService,
        IComandaRealtimeService realtimeService,
        IPreferencesService preferencesService)
    {
        _comandasService = comandasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _mesasService = mesasService;
        _notificationService = notificationService;
        _realtimeService = realtimeService;
        _preferencesService = preferencesService;
        
        Title = "Gestión de Comandas";

        // Restaurar preferencias de filtros antes de cargar datos
        RestaurarPreferencias();

        // Cargar datos iniciales
        _ = LoadComandasAsync();
        _ = LoadEstadisticasAsync();

        // Suscribir a eventos realtime y arrancar
        _realtimeService.OnNuevaComanda += async () =>
        {
            await _notificationService.VibrateAsync(60);
            await _notificationService.ShowToastAsync("Nueva comanda (tiempo real)");
            await RefreshComandasCommand.ExecuteAsync(null);
        };
        _realtimeService.OnComandaActualizada += async () =>
        {
            await RefreshComandasCommand.ExecuteAsync(null);
        };
        _ = _realtimeService.StartAsync();

        // Escuchar mensaje de actualización de comanda para refrescar al volver de edición
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<string>>(this, (r, m) =>
        {
            if (m.Value == Messages.ComandaActualizada)
            {
                _ = LoadComandasAsync();
            }
        });
    }

    #endregion

    #region Comandos Principales

    /// <summary>
    /// Cargar todas las comandas con filtros aplicados
    /// </summary>
    [RelayCommand]
    private async Task LoadComandasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _comandasService.BuscarComandasAsync(
                estado: string.IsNullOrWhiteSpace(FiltroEstado) || FiltroEstado == "Todos" ? null : FiltroEstado,
                mesaId: FiltroMesaId,
                fechaDesde: FiltroFecha,
                fechaHasta: FiltroFecha?.AddDays(1),
                clienteNombre: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);

            if (response.Success)
            {
                var comandasFiltradas = response.Data ?? new List<ComandaDto>();

                // Por defecto ("Todos") mostrar estados operativos hasta Entregada.
                // Si el usuario eligió un estado específico, el backend ya filtró.
                if (string.IsNullOrWhiteSpace(FiltroEstado) || FiltroEstado == "Todos")
                {
                    if (SoloActivas)
                    {
                        // Incluir estados activos operativos: Creada/Pendiente, En preparación/En proceso, Lista y Entregada
                        var estadosOperativos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {
                            "Creada", "Pendiente",
                            "En Preparación", "Preparando", "En Proceso", "EnProceso",
                            "Lista", "Entregada"
                        };
                        comandasFiltradas = comandasFiltradas
                            .Where(c =>
                            {
                                var estado = c.Estado ?? string.Empty;
                                var estadoTexto = c.EstadoTexto ?? string.Empty;
                                return estadosOperativos.Contains(estado) || estadosOperativos.Contains(estadoTexto);
                            })
                            .ToList();
                    }
                }

                Comandas.Clear();
                foreach (var comanda in comandasFiltradas)
                {
                    Comandas.Add(comanda);
                }
            }
            else
            {
                ErrorMessage = response.Message ?? "Error al cargar las comandas";
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error inesperado: {ex.Message}";
            await _dialogService.ShowErrorAsync(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Refrescar la lista de comandas
    /// </summary>
    [RelayCommand]
    private async Task RefreshComandasAsync()
    {
        IsRefreshing = true;
        await LoadComandasAsync();
    }

    /// <summary>
    /// Cargar estadísticas de comandas
    /// </summary>
    [RelayCommand]
    public async Task LoadEstadisticasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.ObtenerEstadisticasAsync();

            if (response.Success && response.Data != null)
            {
                Estadisticas = response.Data;
            }
            else
            {
                // Fallback seguro a cero para que la UI muestre siempre la banda
                Estadisticas = new EstadisticasComandasDto();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar estadísticas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Comandos de Operaciones

    /// <summary>
    /// Crear una nueva comanda
    /// </summary>
    [RelayCommand]
    private async Task CrearComandaAsync()
    {
        try
        {
            // 1) Elegir tipo de comanda
            var tipoSeleccion = await _dialogService.ShowActionSheetAsync(
                "Nueva comanda",
                "Seleccione el tipo de comanda",
                "Cancelar",
                new[] { "Mesa", "Para llevar", "Delivery" });

            if (string.IsNullOrWhiteSpace(tipoSeleccion) || tipoSeleccion == "Cancelar")
                return;

            var esMesa = tipoSeleccion.Equals("Mesa", StringComparison.OrdinalIgnoreCase);

            if (esMesa)
            {
                // 2) Traer mesas disponibles
                var mesasResult = await _mesasService.ObtenerMesasDisponiblesAsync();
                if (!mesasResult.Success)
                {
                    await _dialogService.ShowAlertAsync("Error", mesasResult.Message ?? "No se pudieron cargar las mesas disponibles");
                    return;
                }

                var mesas = mesasResult.Data ?? new List<MesaDto>();
                if (!mesas.Any())
                {
                    await _dialogService.ShowAlertAsync("Sin Mesas", "No hay mesas disponibles en este momento");
                    return;
                }

                // 3) Mostrar selector de mesa
                var opciones = mesas
                    .Select(m => ($"Mesa {m.Numero} — {m.Ubicacion} (Cap: {m.Capacidad})", m.Id))
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
                if (mesaSeleccionada == default)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo identificar la mesa seleccionada");
                    return;
                }

                // 4) Navegar con mesa y tipo
                await _navigationService.NavigateToAsync("crear-comanda", new Dictionary<string, object>
                {
                    ["mesaId"] = mesaSeleccionada.Item2.ToString()
                });
            }
            else
            {
                // 2) Para llevar / Delivery → navegar sin mesa; el ViewModel ya soporta iniciar sin mesa
                await _navigationService.NavigateToAsync("crear-comanda", new Dictionary<string, object>());
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al iniciar nueva comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Ver detalles de una comanda
    /// </summary>
    [RelayCommand]
    private async Task VerDetalleAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await NavigateToComandaDetailAsync(comanda);
    }

    /// <summary>
    /// Editar una comanda existente
    /// </summary>
    [RelayCommand]
    private async Task EditarAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        // Verificar si la comanda es editable
        var estado = comanda.Estado?.ToLowerInvariant() ?? string.Empty;
        var estadoTexto = comanda.EstadoTexto?.ToLowerInvariant() ?? string.Empty;
        var estadoNormalizado = !string.IsNullOrWhiteSpace(estado) ? estado : estadoTexto;
        
        var esFinalizada = estadoNormalizado == "finalizada";
        var esCancelada = estadoNormalizado == "cancelada";
        
        if (esFinalizada || esCancelada)
        {
            await _dialogService.ShowAlertAsync("No Editable", 
                "No se puede editar una comanda finalizada o cancelada. Use 'Ver' para consultar los detalles.");
            return;
        }

        // Ir al mismo formulario en modo edición
        await _navigationService.NavigateToAsync("editar-comanda", new Dictionary<string, object>
        {
            ["comandaId"] = comanda.Id.ToString()
        });
    }

    /// <summary>
    /// Cambiar estado de una comanda
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var estadosPermitidos = ObtenerEstadosPermitidos(comanda.Estado);
        
        if (!estadosPermitidos.Any())
        {
            await _dialogService.ShowAlertAsync("Información", 
                "No hay cambios de estado disponibles para esta comanda");
            return;
        }

        var nuevoEstado = await _dialogService.ShowActionSheetAsync(
            "Cambiar Estado",
            "Seleccione el nuevo estado:",
            "Cancelar",
            estadosPermitidos.ToArray());

        if (string.IsNullOrWhiteSpace(nuevoEstado) || nuevoEstado == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(comanda.Id, nuevoEstado);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Estado cambiado a {nuevoEstado}");
                await LoadComandasAsync();
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
    /// Finalizar una comanda
    /// </summary>
    [RelayCommand]
    private async Task FinalizarComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar",
            $"¿Está seguro que desea finalizar la comanda #{comanda.Numero}?");

        if (!confirmacion) return;

        var metodoPago = await _dialogService.ShowActionSheetAsync(
            "Método de Pago",
            "Seleccione el método de pago:",
            "Cancelar",
            "Efectivo", "Tarjeta", "Transferencia");

        if (string.IsNullOrWhiteSpace(metodoPago) || metodoPago == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.FinalizarComandaAsync(
                comanda.Id, 
                metodoPago, 
                $"Finalizada - Total: {comanda.Total:C}");

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.Numero} finalizada correctamente");
                await LoadComandasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al finalizar la comanda");
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
    /// Cancelar una comanda
    /// </summary>
    [RelayCommand]
    private async Task CancelarComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar Cancelación",
            $"¿Está seguro que desea cancelar la comanda #{comanda.Numero}?");

        if (!confirmacion) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Motivo de Cancelación",
            "Ingrese el motivo de la cancelación:");

        if (string.IsNullOrWhiteSpace(motivo))
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CancelarComandaAsync(comanda.Id, motivo);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.Numero} cancelada correctamente");
                await LoadComandasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al cancelar la comanda");
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
    /// Mostrar/ocultar filtros
    /// </summary>
    [RelayCommand]
    private void MostrarFiltrosCommand()
    {
        // Esta funcionalidad se maneja desde la UI
        // Aquí solo notificamos el cambio si es necesario
    }

    #endregion

    #region Comandos de Filtros

    /// <summary>
    /// Aplicar filtros de búsqueda
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadComandasAsync();
    }

    /// <summary>
    /// Buscar comanda específica
    /// </summary>
    [RelayCommand]
    private async Task BuscarComandaAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await _dialogService.ShowAlertAsync("Información", "Ingrese un término de búsqueda");
            return;
        }

        // Primero intentar buscar por cliente
        await LoadComandasAsync();
        
        // Si no se encontraron resultados y el texto parece ser un número, buscar por número de comanda
        if (!Comandas.Any())
        {
            // Buscar en todas las comandas y filtrar por número (int o código display)
            var response = await _comandasService.BuscarComandasAsync(estado: null);
            if (response.Success && response.Data != null)
            {
                var term = SearchText.Trim();
                var encontrados = response.Data.Where(c =>
                    (!string.IsNullOrWhiteSpace(c.NumeroDisplay) && c.NumeroDisplay.Contains(term, StringComparison.OrdinalIgnoreCase))
                    || c.Numero.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(c.ClienteNombre) && c.ClienteNombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (encontrados.Any())
                {
                    Comandas.Clear();
                    foreach (var c in encontrados)
                        Comandas.Add(c);
                }
            }
        }
    }

    /// <summary>
    /// Limpiar todos los filtros
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        FiltroEstado = string.Empty;
        FiltroFecha = null;
        FiltroMesaId = null;
        SearchText = string.Empty;
        
        await LoadComandasAsync();
    }

    /// <summary>
    /// Toggle entre solo activas y todas
    /// </summary>
    [RelayCommand]
    private async Task ToggleActivasAsync()
    {
        SoloActivas = !SoloActivas;
        await LoadComandasAsync();
    }

    #endregion

    #region Comandos de Navegación

    /// <summary>
    /// Navegar a la página de detalle de comanda
    /// </summary>
    [RelayCommand]
    private async Task NavigateToComandaDetailAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await _navigationService.NavigateToAsync("comanda-detalle", new Dictionary<string, object>
        {
            ["comandaId"] = comanda.Id.ToString()
        });
    }

    /// <summary>
    /// Navegar a agregar productos a la comanda
    /// </summary>
    [RelayCommand]
    private async Task NavigateToAgregarProductosAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await _navigationService.NavigateToAsync("///productos", new Dictionary<string, object>
        {
            ["comandaId"] = comanda.Id.ToString()
        });
    }

    #endregion

    #region Métodos de Utilidad

    /// <summary>
    /// Obtener estados permitidos según el estado actual
    /// </summary>
    private List<string> ObtenerEstadosPermitidos(string estadoActual)
    {
        return estadoActual?.ToLower() switch
        {
            "pendiente" => new List<string> { "En Preparación", "Cancelada" },
            "en preparación" => new List<string> { "Lista", "Cancelada" },
            "lista" => new List<string> { "Entregada" },
            "entregada" => new List<string> { "Finalizada" },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Obtener color basado en el estado de la comanda
    /// </summary>
    public string GetEstadoColor(string estado)
    {
        return estado?.ToLower() switch
        {
            "pendiente" => "Orange",
            "en preparación" => "Blue", 
            "lista" => "Green",
            "entregada" => "Purple",
            "finalizada" => "Gray",
            "cancelada" => "Red",
            _ => "Black"
        };
    }

    /// <summary>
    /// Obtener icono basado en el estado de la comanda
    /// </summary>
    public string GetEstadoIcon(string estado)
    {
        return estado?.ToLower() switch
        {
            "pendiente" => "⏳",
            "en preparación" => "👨‍🍳",
            "lista" => "✅",
            "entregada" => "🍽️",
            "finalizada" => "✔️",
            "cancelada" => "❌",
            _ => "?"
        };
    }

    /// <summary>
    /// Restaurar preferencias de filtros del usuario
    /// </summary>
    private void RestaurarPreferencias()
    {
        try
        {
            FiltroEstado = _preferencesService.Get<string>("Comandas.FiltroEstado", string.Empty);
            SoloActivas = _preferencesService.Get<bool>("Comandas.SoloActivas", true);
            SearchText = _preferencesService.Get<string>("Comandas.SearchText", string.Empty);
            var fechaTicks = _preferencesService.Get<long>("Comandas.FiltroFechaTicks", -1);
            FiltroFecha = fechaTicks > 0 ? new DateTime(fechaTicks) : null;
            var mesaIdStr = _preferencesService.Get<string>("Comandas.FiltroMesaId", string.Empty);
            FiltroMesaId = Guid.TryParse(mesaIdStr, out var parsed) ? parsed : null;
        }
        catch
        {
            // Ignorar errores al restaurar preferencias
        }
    }

    // Guardar cambios de filtros en preferencias
    partial void OnFiltroEstadoChanged(string value)
    {
        _ = _preferencesService.SetAsync("Comandas.FiltroEstado", value ?? string.Empty);
    }

    partial void OnSoloActivasChanged(bool value)
    {
        _ = _preferencesService.SetAsync("Comandas.SoloActivas", value);
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = _preferencesService.SetAsync("Comandas.SearchText", value ?? string.Empty);
    }

    partial void OnFiltroFechaChanged(DateTime? value)
    {
        var ticks = value?.Ticks ?? -1;
        _ = _preferencesService.SetAsync("Comandas.FiltroFechaTicks", ticks);
    }

    partial void OnFiltroMesaIdChanged(Guid? value)
    {
        _ = _preferencesService.SetAsync("Comandas.FiltroMesaId", value?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Mostrar menú de opciones para una comanda (acciones secundarias)
    /// </summary>
    [RelayCommand]
    private async Task MostrarOpcionesComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        // Determinar opciones disponibles según el estado de la comanda
        var opciones = new List<string>();
        var estado = comanda.Estado?.ToLowerInvariant() ?? string.Empty;
        var estadoTexto = comanda.EstadoTexto?.ToLowerInvariant() ?? string.Empty;
        var estadoNormalizado = !string.IsNullOrWhiteSpace(estado) ? estado : estadoTexto;
        
        var esFinalizada = estadoNormalizado == "finalizada";
        var esCancelada = estadoNormalizado == "cancelada";
        var esEditable = !esFinalizada && !esCancelada && comanda.PuedeSerEditada;

        if (esEditable)
        {
            // Comandas editables: mostrar opciones de edición
            opciones.Add("Editar");
            opciones.Add("Agregar Productos");
        }
        
        // Todas las comandas pueden ser vistas
        opciones.Add("Ver");

        if (!opciones.Any())
        {
            await _dialogService.ShowAlertAsync("Información", "No hay acciones disponibles para esta comanda");
            return;
        }

        var seleccion = await _dialogService.ShowActionSheetAsync(
            $"Comanda #{comanda.NumeroDisplay}",
            "Seleccione una acción:",
            "Cerrar",
            opciones.ToArray());

        switch (seleccion)
        {
            case "Editar":
                await EditarCommand.ExecuteAsync(comanda);
                break;
            case "Ver":
                await VerDetalleCommand.ExecuteAsync(comanda);
                break;
            case "Agregar Productos":
                await NavigateToAgregarProductosCommand.ExecuteAsync(comanda);
                break;
            default:
                break;
        }
    }

    #endregion
} 