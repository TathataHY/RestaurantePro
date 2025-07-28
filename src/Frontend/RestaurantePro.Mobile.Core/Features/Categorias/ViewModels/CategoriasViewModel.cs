using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Categorias;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Categorias.ViewModels;

/// <summary>
/// ViewModel para gestión de categorías de productos
/// </summary>
public partial class CategoriasViewModel : BaseViewModel
{
    private readonly ICategoriasService _categoriasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<CategoriaProductoDto> categorias = new();

    [ObservableProperty]
    private CategoriaProductoDto? categoriaSeleccionada;

    [ObservableProperty]
    private string filtroBusqueda = string.Empty;

    [ObservableProperty]
    private bool mostrarSoloActivas = true;

    [ObservableProperty]
    private bool estaRefrescando;

    [ObservableProperty]
    private int totalCategorias;

    [ObservableProperty]
    private int categoriasActivas;

    [ObservableProperty]
    private int categoriasInactivas;

    #endregion

    #region Constructor

    public CategoriasViewModel(
        ICategoriasService categoriasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _categoriasService = categoriasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        Title = "Categorías de Productos";
    }

    #endregion

    #region Comandos

    /// <summary>
    /// Cargar categorías al aparecer la página
    /// </summary>
    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        if (IsBusy) return;
        
        await ExecuteAsync(async () =>
        {
            await CargarCategoriasAsync();
        });
    }

    /// <summary>
    /// Cargar categorías
    /// </summary>
    [RelayCommand]
    public async Task CargarCategoriasAsync()
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            EstaRefrescando = true;
            
            var response = await _categoriasService.ObtenerCategoriasAsync(mostrarSoloActivas);
            
            if (response.Success && response.Data != null)
            {
                Categorias.Clear();
                foreach (var categoria in response.Data)
                {
                    Categorias.Add(categoria);
                }
                
                ActualizarEstadisticas();
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar categorías");
            }
        }, showLoading: false);
        
        EstaRefrescando = false;
    }

    /// <summary>
    /// Seleccionar categoría
    /// </summary>
    [RelayCommand]
    public async Task SeleccionarCategoriaAsync(CategoriaProductoDto categoria)
    {
        if (categoria == null) return;

        CategoriaSeleccionada = categoria;
        
        // Navegar a productos de la categoría
        await _navigationService.NavigateToAsync("productos", new Dictionary<string, object>
        {
            { "categoriaId", categoria.Id },
            { "categoriaNombre", categoria.Nombre }
        });
    }

    /// <summary>
    /// Buscar categorías
    /// </summary>
    [RelayCommand]
    public async Task BuscarCategoriasAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(FiltroBusqueda)) return;

        await ExecuteAsync(async () =>
        {
            var response = await _categoriasService.BuscarCategoriasAsync(FiltroBusqueda);
            
            if (response.Success && response.Data != null)
            {
                Categorias.Clear();
                foreach (var categoria in response.Data)
                {
                    Categorias.Add(categoria);
                }
                
                ActualizarEstadisticas();
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al buscar categorías");
            }
        });
    }

    /// <summary>
    /// Refrescar categorías
    /// </summary>
    [RelayCommand]
    public async Task RefrescarAsync()
    {
        await CargarCategoriasAsync();
    }

    /// <summary>
    /// Cambiar filtro de categorías activas
    /// </summary>
    [RelayCommand]
    public async Task CambiarFiltroActivasAsync()
    {
        await CargarCategoriasAsync();
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Actualizar estadísticas de categorías
    /// </summary>
    private void ActualizarEstadisticas()
    {
        TotalCategorias = Categorias.Count;
        CategoriasActivas = Categorias.Count(c => c.Activa);
        CategoriasInactivas = Categorias.Count(c => !c.Activa);
    }

    #endregion
} 