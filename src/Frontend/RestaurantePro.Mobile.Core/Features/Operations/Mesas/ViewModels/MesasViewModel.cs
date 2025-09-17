using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Authentication;
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
    private readonly IAuthService _authService;
    private readonly SemaphoreSlim _loadingSemaphore = new(1, 1);

    // Buffer para paginado en cliente
    private List<MesaDto> _allMesasBuffer = new();
    private int _currentBufferIndex = 0;
    private const int DefaultPageSize = 12; // Tamaño de página por defecto (12-20 sugerido)
    private int _pageSize = DefaultPageSize;
    
    // Paginación del servidor
    private int _currentPage = 1;
    private int _totalPages = 1;
    private bool _hasMorePages = true;
    private bool _isLoadingMore = false;

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

    [ObservableProperty]
    private ObservableCollection<string> ubicacionesDisponibles = new();

    // Toggle para mostrar/ocultar la sección de “Más filtros”
    [ObservableProperty]
    private bool mostrarMasFiltros;

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
        INavigationService navigationService,
        IAuthService authService)
    {
        _mesasService = mesasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _authService = authService;
        
        Title = "Gestión de Mesas";
    }

    #endregion

    #region Comandos Principales

    /// <summary>
    /// Cargar todas las mesas con filtros aplicados
    /// </summary>
    [RelayCommand]
    public async Task LoadMesasAsync()
    {
        // Verificar que los servicios estén disponibles
        if (_mesasService == null || _authService == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ [MesasViewModel] Servicios no inicializados, esperando...");
            await Task.Delay(500);
            
            if (_mesasService == null || _authService == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ [MesasViewModel] Servicios aún no disponibles");
                return;
            }
        }
        
        // Resetear a la primera página para nueva búsqueda
        _currentPage = 1;
        await LoadMesasAsync(FiltroEstado, FiltroUbicacion, FiltroCapacidadMinima);
    }

    private async Task LoadMesasAsync(string? estado, string? ubicacion, int? capacidadMinima)
    {
        // Esperar a que termine cualquier petición en curso (con timeout para evitar bloqueos)
        var semaphoreAcquired = await _loadingSemaphore.WaitAsync(TimeSpan.FromSeconds(10));
        
        if (!semaphoreAcquired)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ [MesasViewModel] Timeout esperando semáforo, cancelando operación");
            return;
        }
        
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            
            // Resetear paginación para nueva búsqueda (solo si es la primera página)
            if (_currentPage == 1)
            {
                _hasMorePages = true;
            }

            System.Diagnostics.Debug.WriteLine($"🔍 MesasViewModel.LoadMesasAsync - Iniciando carga de mesas (Página {_currentPage})");
            System.Diagnostics.Debug.WriteLine($"🔍 Filtros: Estado={estado}, Ubicacion={ubicacion}, CapacidadMinima={capacidadMinima}");
            
            // Verificar autenticación
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] Usuario autenticado: {isAuthenticated}");

            var response = await _mesasService.ObtenerMesasAsync(
                string.IsNullOrWhiteSpace(estado) ? null : NormalizeEstado(estado),
                string.IsNullOrWhiteSpace(ubicacion) ? null : ubicacion,
                capacidadMinima,
                _currentPage,
                _pageSize);

            if (response.Success)
            {
                var data = response.Data?.Items ?? new List<MesaDto>();
                
                // Actualizar información de paginación
                _totalPages = response.Data?.TotalPages ?? 1;
                _hasMorePages = _currentPage < _totalPages;
                
                System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] Paginación - Página actual: {_currentPage}, Total páginas: {_totalPages}, Más páginas: {_hasMorePages}");

                // Actualizar ubicaciones disponibles (solo en la primera página)
                if (_currentPage == 1)
                {
                    var ubicaciones = data
                        .Select(m => m.Ubicacion)
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(u => u)
                        .ToList();
                    UbicacionesDisponibles.Clear();
                    UbicacionesDisponibles.Add("Todas");
                    foreach (var u in ubicaciones) UbicacionesDisponibles.Add(u);
                }

                // Filtro de búsqueda local por número/ubicación/zona
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var term = SearchText.Trim();
                    data = data.Where(m =>
                        (!string.IsNullOrWhiteSpace(m.Numero) && m.Numero.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(m.Ubicacion) && m.Ubicacion.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(m.Zona) && m.Zona.Contains(term, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                // Ordenar las mesas por número de forma ascendente (1, 2, 3, 4...)
                data = data.OrderBy(m => 
                {
                    // Intentar convertir el número a entero para ordenamiento numérico correcto
                    if (int.TryParse(m.Numero, out var numero))
                        return numero;
                    // Si no se puede convertir, usar ordenamiento alfabético como fallback
                    return int.MaxValue;
                }).ThenBy(m => m.Numero).ToList();

                // Para la primera página, limpiar la colección; para páginas adicionales, agregar
                var mesasAntes = Mesas.Count;
                if (_currentPage == 1)
                {
                    Mesas.Clear();
                    System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] Página 1 - Lista limpiada");
                }
                
                System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] Antes de agregar - Mesas en lista: {Mesas.Count}, Datos a agregar: {data.Count}");
                
                // Agregar solo las mesas que no están ya en la colección (evitar duplicados)
                var agregadas = 0;
                foreach (var mesa in data)
                {
                    if (!Mesas.Any(m => m.Id == mesa.Id))
                    {
                        Mesas.Add(mesa);
                        agregadas++;
                        System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] Agregada mesa {mesa.Numero} (ID: {mesa.Id})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] Mesa duplicada ignorada: {mesa.Numero} (ID: {mesa.Id})");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] Después de agregar - Mesas agregadas: {agregadas}, Total en lista: {Mesas.Count}");
                
                // Si no hay datos en esta página, marcar como sin más páginas
                if (data.Count == 0)
                {
                    _hasMorePages = false;
                    System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] Página {_currentPage} sin datos - Marcando como última página");
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Se cargaron {data.Count} mesas (Página {_currentPage}/{_totalPages}) - Total: {Mesas.Count} - Más páginas: {_hasMorePages}");
            }
            else
            {
                // Si es la primera página, mostrar error completo
                if (_currentPage == 1)
                {
                    await ShowErrorAsync(response.Message ?? "Error al cargar las mesas");
                    await _dialogService.ShowAlertAsync("Error", ErrorMessage);
                }
                else
                {
                    // Si es una página adicional, solo log el error pero mantener la paginación activa
                    System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] Error cargando página {_currentPage}: {response.Message}");
                    
                    // Solo detener si es un error de autenticación o si hemos intentado varias veces
                    if (response.Message?.Contains("401") == true || response.Message?.Contains("Unauthorized") == true)
                    {
                        _hasMorePages = false; // Error de auth, detener
                        System.Diagnostics.Debug.WriteLine($"❌ [MesasViewModel] Error de autenticación, deteniendo paginación");
                    }
                    else
                    {
                        // Para errores de red temporales, mantener la paginación activa
                        System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] Error temporal, manteniendo paginación activa");
                        // No cambiar _hasMorePages, permitir que el usuario intente de nuevo
                    }
                    
                    _currentPage--; // Revertir el incremento de página para reintentar
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR en LoadMesasAsync: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            
            // Si es la primera página, mostrar error completo
            if (_currentPage == 1)
            {
                await ShowErrorAsync($"Error inesperado: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error", ErrorMessage);
            }
            else
            {
                // Si es una página adicional, solo log el error pero mantener paginación para errores temporales
                System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] Error en página {_currentPage}: {ex.Message}");
                
                // Solo detener para errores críticos, no para errores de red temporales
                if (ex is UnauthorizedAccessException || ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
                {
                    _hasMorePages = false; // Error de auth, detener
                    System.Diagnostics.Debug.WriteLine($"❌ [MesasViewModel] Error de autenticación en catch, deteniendo paginación");
                }
                else
                {
                    // Para errores de red/IO temporales, mantener la paginación activa
                    System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] Error temporal en catch, manteniendo paginación activa");
                }
                
                _currentPage--; // Revertir el incremento de página para reintentar
            }
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
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 [MesasViewModel] RefreshMesasAsync - Iniciando refresh manual");
            
            IsRefreshing = true;
            
            // Cancelar cualquier operación de LoadMore en progreso
            _isLoadingMore = false;
            
            // Resetear estado de paginación completamente
            _currentPage = 1;
            _hasMorePages = true;
            
            // Pequeño delay para evitar conflictos con operaciones en curso
            await Task.Delay(100);
            
            await LoadMesasAsync();
            
            System.Diagnostics.Debug.WriteLine("✅ [MesasViewModel] RefreshMesasAsync - Completado exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [MesasViewModel] RefreshMesasAsync - Error: {ex.Message}");
            await ShowErrorAsync($"Error al refrescar: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Cargar siguiente página del servidor (scroll infinito)
    /// </summary>
    [RelayCommand]
    private async Task LoadMoreMesasAsync()
    {
        if (IsBusy || !_hasMorePages || _isLoadingMore || IsRefreshing) 
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] LoadMoreMesasAsync - Saltando: IsBusy={IsBusy}, HasMorePages={_hasMorePages}, IsLoadingMore={_isLoadingMore}, IsRefreshing={IsRefreshing}");
            return;
        }
        
        _isLoadingMore = true;
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [MesasViewModel] LoadMoreMesasAsync - Cargando página {_currentPage + 1}");
            
            // Verificar nuevamente que no se inició un refresh mientras esperábamos
            if (IsRefreshing)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [MesasViewModel] LoadMoreMesasAsync - Refresh detectado, cancelando LoadMore");
                return;
            }
            
            // Incrementar página y cargar
            _currentPage++;
            await LoadMesasAsync(FiltroEstado, FiltroUbicacion, FiltroCapacidadMinima);
        }
        finally
        {
            _isLoadingMore = false;
        }
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

    /// <summary>
    /// Mostrar menú contextual por mesa (⋮ Más)
    /// </summary>
    [RelayCommand]
    private async Task MostrarOpcionesMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var opcion = await _dialogService.ShowActionSheetAsync(
            $"Mesa {mesa.Numero}",
            "Seleccione una opción:",
            "Cancelar",
            "Asignar",
            "Liberar",
            "Cambiar estado",
            "Crear comanda");

        switch (opcion)
        {
            case "Asignar":
                await AsignarMesaAsync(mesa);
                break;
            case "Liberar":
                await LiberarMesaAsync(mesa);
                break;
            case "Cambiar estado":
                await CambiarEstadoMesaAsync(mesa);
                break;
            case "Crear comanda":
                await NavigateToNewComandaAsync(mesa);
                break;
            default:
                break;
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
    /// Alternar sección de "Más filtros"
    /// </summary>
    [RelayCommand]
    private void ToggleMasFiltros()
    {
        MostrarMasFiltros = !MostrarMasFiltros;
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

    private static string? NormalizeEstado(string? estadoUi)
    {
        if (string.IsNullOrWhiteSpace(estadoUi)) return null;
        var e = estadoUi.Trim().ToLowerInvariant();
        return e switch
        {
            "todas" => null,
            "disponible" => "Disponible",
            "disponibles" => "Disponible",
            "ocupada" => "Ocupada",
            "ocupadas" => "Ocupada",
            "reservada" => "Reservada",
            "reservadas" => "Reservada",
            "fuera de servicio" => "Mantenimiento",
            "en limpieza" => "Mantenimiento", // ajustar si existe estado específico
            _ => estadoUi
        };
    }

    private void AppendNextPage()
    {
        if (_allMesasBuffer == null || _allMesasBuffer.Count == 0) return;

        var remaining = _allMesasBuffer.Count - _currentBufferIndex;
        if (remaining <= 0) return;

        var take = Math.Min(_pageSize, remaining);
        var slice = _allMesasBuffer.Skip(_currentBufferIndex).Take(take);
        foreach (var item in slice)
        {
            Mesas.Add(item);
        }
        _currentBufferIndex += take;
    }

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
