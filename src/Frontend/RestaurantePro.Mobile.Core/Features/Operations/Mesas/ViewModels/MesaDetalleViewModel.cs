using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using System.Collections.ObjectModel;
using System.Linq;

namespace RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

/// <summary>
/// ViewModel para la página de detalle de mesa
/// </summary>
public partial class MesaDetalleViewModel : BaseViewModel
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

    [ObservableProperty]
    private ComandaDto? selectedComanda;
    
    private bool _isInitialized;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

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
        
        // Inicializar comandos
        EntregarComandaCommand = new AsyncRelayCommand<ComandaDto>(EntregarComandaAsync);
        CobrarComandaCommand = new AsyncRelayCommand<ComandaDto>(CobrarComandaAsync);
        VerComandaCommand = new AsyncRelayCommand<ComandaDto>(AbrirComandaDetalleAsync);
    }

    /// <summary>
    /// Propiedades calculadas para la UI
    /// </summary>
    public bool PuedeAsignar => Mesa?.Estado?.ToLowerInvariant() == "disponible";
    public bool PuedeLiberar => Mesa?.Estado?.ToLowerInvariant() == "ocupada";
    public bool TieneComandasActivas => ComandasActivas?.Any() == true;
    
    /// <summary>
    /// Comandas listas para entregar (estado "Lista")
    /// </summary>
    public IEnumerable<ComandaDto> ComandasListasParaEntregar => 
        ComandasActivas?.Where(c => c.Estado?.ToLowerInvariant() == "lista") ?? Enumerable.Empty<ComandaDto>();
    
    /// <summary>
    /// Comandas listas para cobrar (estado "Entregada")
    /// </summary>
    public IEnumerable<ComandaDto> ComandasListasParaCobrar => 
        ComandasActivas?.Where(c => c.Estado?.ToLowerInvariant() == "entregada") ?? Enumerable.Empty<ComandaDto>();
    
    /// <summary>
    /// Indica si hay comandas listas para entregar
    /// </summary>
    public bool TieneComandasListasParaEntregar => ComandasListasParaEntregar.Any();
    
    /// <summary>
    /// Indica si hay comandas listas para cobrar
    /// </summary>
    public bool TieneComandasListasParaCobrar => ComandasListasParaCobrar.Any();

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
    public async Task InitializeAsync(bool force = false)
    {
        await _loadLock.WaitAsync();
        try
        {
            if (MesaId == Guid.Empty) return;
            if (IsBusy) return;
            if (_isInitialized && !force) return;

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
            _isInitialized = true;
            _loadLock.Release();
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Cargando mesa con ID: {MesaId}");
            var result = await _mesasService.ObtenerMesaAsync(MesaId);
            
            if (result.Success)
            {
                Mesa = result.Data ?? new MesaDto();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Mesa cargada - Estado: {Mesa.Estado}, EstadoDescripcion: {Mesa.EstadoDescripcion}, PuedeAsignar: {PuedeAsignar}, PuedeLiberar: {PuedeLiberar}");
                
                // Forzar actualización de todas las propiedades de la mesa
                OnPropertyChanged(nameof(Mesa));
                OnPropertyChanged(nameof(PuedeAsignar));
                OnPropertyChanged(nameof(PuedeLiberar));
                OnPropertyChanged(nameof(TieneComandasActivas));
                
                // Notificar cambios específicos de la mesa
                OnPropertyChanged(nameof(Mesa.Estado));
                OnPropertyChanged(nameof(Mesa.EstadoDescripcion));
                OnPropertyChanged(nameof(Mesa.Numero));
                OnPropertyChanged(nameof(Mesa.Capacidad));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Error al cargar mesa: {result.Message}");
                await _dialogService.ShowAlertAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Excepción al cargar mesa: {ex.Message}");
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
            if (IsBusy) return; // evita solaparse con Initialize
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Cargando comandas para mesa: {MesaId}");
            var result = await _comandasService.ObtenerComandasPorMesaAsync(MesaId);
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Resultado de comandas - Success: {result.Success}, Count: {result.Data?.Count ?? 0}");
            
            if (result.Success)
            {
                ComandasActivas.Clear();
                
                if (result.Data != null)
                {
                    foreach (var comanda in result.Data)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Comanda encontrada - ID: {comanda.Id}, Estado: {comanda.Estado}");
                    }
                    
                    var comandasActivas = result.Data.Where(c => c.Estado != "finalizada" && c.Estado != "cancelada").ToList();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Comandas activas después del filtro: {comandasActivas.Count}");
                    
                    foreach (var comanda in comandasActivas)
                    {
                        ComandasActivas.Add(comanda);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ComandasActivas collection count: {ComandasActivas.Count}");
                OnPropertyChanged(nameof(TieneComandasActivas));
                OnPropertyChanged(nameof(ComandasListasParaEntregar));
                OnPropertyChanged(nameof(ComandasListasParaCobrar));
                OnPropertyChanged(nameof(TieneComandasListasParaEntregar));
                OnPropertyChanged(nameof(TieneComandasListasParaCobrar));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Error al cargar comandas: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Excepción al cargar comandas: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar comandas: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] LiberarMesaAsync iniciado. MesaId={MesaId}, IsBusy={IsBusy}");
            if (IsBusy) return;
            var confirmar = await _dialogService.ShowConfirmAsync(
                "Confirmar",
                "¿Está seguro que desea liberar esta mesa?");

            if (!confirmar) return;

            var motivo = await _dialogService.ShowPromptAsync(
                "Motivo de Liberación",
                "Ingrese el motivo para liberar la mesa:",
                "Confirmar",
                "Cancelar",
                "Finalización del servicio");

            if (motivo == null)
                return; // usuario canceló

            // Si está vacío, usar un motivo por defecto
            if (string.IsNullOrWhiteSpace(motivo))
                motivo = "Finalización del servicio";

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Confirmado liberar. Motivo='{motivo}'");
            IsBusy = true;
            var result = await _mesasService.LiberarMesaAsync(MesaId, motivo);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Resultado LiberarMesaAsync -> Success={result.Success}, Message={result.Message}");

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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Excepción LiberarMesaAsync: {ex}");
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
            // Forzar recarga de la mesa antes del cambio para sincronizar con el backend
            await LoadMesaAsync();
            
            // Obtener estados disponibles (excluyendo el estado actual)
            var estadosDisponibles = new List<string> { "Disponible", "Ocupada", "Reservada", "Mantenimiento" };
            var estadoActual = Mesa?.EstadoDescripcion ?? "Desconocido";
            
            // Filtrar el estado actual de las opciones
            var opciones = estadosDisponibles.Where(e => e != estadoActual).ToArray();
            
            if (opciones.Length == 0)
            {
                await _dialogService.ShowAlertAsync("Información", "La mesa ya está en todos los estados posibles");
                return;
            }

            var nuevoEstado = await _dialogService.ShowActionSheetAsync(
                $"Cambiar Estado (Actual: {estadoActual})",
                "Seleccione el nuevo estado:",
                "Cancelar",
                opciones);

            if (string.IsNullOrWhiteSpace(nuevoEstado) || nuevoEstado == "Cancelar")
                return;

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Cambiando estado de mesa {MesaId} de {estadoActual} a: {nuevoEstado}");

            IsBusy = true;
            var result = await _mesasService.CambiarEstadoMesaAsync(MesaId, nuevoEstado);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Resultado del cambio de estado: Success={result.Success}, Message={result.Message}");

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Estado cambiado de {estadoActual} a {nuevoEstado}");
                
                // Usar directamente la mesa actualizada que retorna el servicio
                if (result.Data != null)
                {
                    Mesa = result.Data;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Mesa actualizada - Estado: {Mesa.Estado}, EstadoDescripcion: {Mesa.EstadoDescripcion}");
                }
                else
                {
                    // Si no hay datos, recargar la mesa
                    await LoadMesaAsync();
                }
                
                // Forzar actualización inmediata de la UI
                // Notificar cambios en todas las propiedades relacionadas
                OnPropertyChanged(nameof(Mesa));
                OnPropertyChanged(nameof(PuedeAsignar));
                OnPropertyChanged(nameof(PuedeLiberar));
                OnPropertyChanged(nameof(TieneComandasActivas));
                
                // Forzar actualización de propiedades específicas de la mesa
                if (Mesa != null)
                {
                    OnPropertyChanged(nameof(Mesa.Estado));
                    OnPropertyChanged(nameof(Mesa.EstadoDescripcion));
                    OnPropertyChanged(nameof(Mesa.EstadoColor));
                    OnPropertyChanged(nameof(Mesa.Disponible));
                    OnPropertyChanged(nameof(Mesa.Ocupada));
                    OnPropertyChanged(nameof(Mesa.Reservada));
                    OnPropertyChanged(nameof(Mesa.FueraDeServicio));
                }
                
                // Pequeña pausa para asegurar que la UI se actualice
                await Task.Delay(100);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Error al cambiar estado: {ex.Message}");
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
        await _navigationService.NavigateToAsync("comandas", new Dictionary<string, object>
        {
            ["mesaId"] = MesaId.ToString()
        });
    }

    /// <summary>
    /// Comando para refrescar datos
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy) return;
        await InitializeAsync(force: true);
    }

    /// <summary>
    /// Crear nueva comanda para la mesa
    /// </summary>
    [RelayCommand]
    private async Task CrearComandaAsync()
    {
        try
        {
                    if (Mesa?.Estado?.ToLowerInvariant() != "ocupada")
        {
            await _dialogService.ShowAlertAsync("Error", "La mesa debe estar ocupada para crear una comanda");
            return;
        }

            await _navigationService.NavigateToAsync("crear-comanda", new Dictionary<string, object>
            {
                ["mesaId"] = MesaId.ToString()
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al crear comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Comando para entregar una comanda
    /// </summary>
    public IAsyncRelayCommand<ComandaDto> EntregarComandaCommand { get; }

    /// <summary>
    /// Entregar una comanda lista al cliente
    /// </summary>
    private async Task EntregarComandaAsync(ComandaDto comanda)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] EntregarComandaAsync llamado con comanda: {comanda?.Id}");
        if (comanda == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar Entrega",
            $"¿Está seguro que desea entregar la comanda #{comanda.Numero} al cliente?");

        if (!confirmacion) return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(
                comanda.Id,
                "entregada",
                "Comanda entregada al cliente");

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.Numero} entregada correctamente");
                await LoadComandasActivasAsync();
                
                // Notificar cambios en las propiedades calculadas
                OnPropertyChanged(nameof(ComandasListasParaEntregar));
                OnPropertyChanged(nameof(ComandasListasParaCobrar));
                OnPropertyChanged(nameof(TieneComandasListasParaEntregar));
                OnPropertyChanged(nameof(TieneComandasListasParaCobrar));
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al entregar la comanda");
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
    /// Comando para cobrar una comanda
    /// </summary>
    public IAsyncRelayCommand<ComandaDto> CobrarComandaCommand { get; }

    /// <summary>
    /// Comando para abrir el detalle de una comanda desde la lista
    /// </summary>
    public IAsyncRelayCommand<ComandaDto> VerComandaCommand { get; }

    /// <summary>
    /// Cobrar una comanda entregada
    /// </summary>
    private async Task CobrarComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar Cobro",
            $"¿Está seguro que desea cobrar la comanda #{comanda.Numero} por ${comanda.Total:F2}?");

        if (!confirmacion) return;

        // Seleccionar método de pago
        var metodoPago = await _dialogService.ShowActionSheetAsync(
            "Método de Pago",
            $"Comanda #{comanda.Numero} - Total: ${comanda.Total:F2}",
            "Cancelar",
            "Efectivo", "Tarjeta", "Transferencia");

        if (string.IsNullOrWhiteSpace(metodoPago) || metodoPago == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(
                comanda.Id,
                "finalizada",
                $"Comanda cobrada - Método: {metodoPago} - Total: ${comanda.Total:F2}");

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.Numero} cobrada correctamente por ${comanda.Total:F2} ({metodoPago})");
                await LoadComandasActivasAsync();
                
                // Notificar cambios en las propiedades calculadas
                OnPropertyChanged(nameof(ComandasListasParaEntregar));
                OnPropertyChanged(nameof(ComandasListasParaCobrar));
                OnPropertyChanged(nameof(TieneComandasListasParaEntregar));
                OnPropertyChanged(nameof(TieneComandasListasParaCobrar));
                OnPropertyChanged(nameof(TieneComandasActivas));
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al cobrar la comanda");
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
    /// Limpiar recursos
    /// </summary>
    public void Cleanup()
    {
        ComandasActivas?.Clear();
        Mesa = new MesaDto();
    }

    partial void OnSelectedComandaChanged(ComandaDto? value)
    {
        // Disparar navegación sin bloquear el hilo del UI
        if (value != null)
        {
            _ = AbrirComandaDetalleAsync(value);
        }
    }

    /// <summary>
    /// Navega al detalle de la comanda y limpia la selección
    /// </summary>
    private async Task AbrirComandaDetalleAsync(ComandaDto? comanda)
    {
        if (comanda == null) return;
        try
        {
            await _navigationService.NavigateToAsync("comanda-detalle", new Dictionary<string, object>
            {
                ["comandaId"] = comanda.Id.ToString()
            });
        }
        finally
        {
            // Limpiar selección para permitir re-taps
            SelectedComanda = null;
        }
    }
} 