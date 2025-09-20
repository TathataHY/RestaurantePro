using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Features.DailyPreparations.Pages;

public partial class ModernDailyPreparationsPage : ContentPage
{
    private readonly DailyPreparationsViewModel _viewModel;

    public ModernDailyPreparationsPage(DailyPreparationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        SetupFilterTabs();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // 🔐 IMPORTANTE: Inicializar autorización ANTES de cargar datos
        await _viewModel.InitializeWithAuthorizationAsync();
        
        // 🔧 DEBUGGING: Verificar valores de autorización después de la inicialización
        System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] === DEBUGGING UI BINDING ===");
        System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] CanEditarPreparacion: {_viewModel.CanEditarPreparacion}");
        System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] CanEliminarPreparacion: {_viewModel.CanEliminarPreparacion}");
        System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] CanCrearPreparacion: {_viewModel.CanCrearPreparacion}");
        System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] === FIN DEBUGGING UI ===");
        
        // 🔧 LLAMAR AL MÉTODO DE LOGS DEL VIEWMODEL
        _viewModel.LogCurrentPermissions();
        
        // 🔧 OCULTAR BOTONES DIRECTAMENTE SI NO TIENE PERMISOS
        await OcultarBotonesSiNoTienePermisos();
        
        _ = InitializeOnAppearAsync();
    }

    // BottomTabBar deshabilitado en esta vista (usamos tabs nativos del Shell)

    private void SetupFilterTabs()
    {
        FilterTabNavigation.Tabs = new ObservableCollection<string>
        {
            "Todos",
            "Disponibles",
            "Preparando",
            "Agotados"
        };
        FilterTabNavigation.TabSelectedCommand = new Command<int>(OnFilterTabSelected);
    }

    private async Task InitializeOnAppearAsync()
    {
        await Task.Delay(100);
        if (MainFab != null)
        {
            await MainFab.ScaleTo(1, 400, Easing.BounceOut);
        }
        // Los datos ya se cargan en InitializeWithAuthorizationAsync
    }

    // Sin manejador de tabs inferior en esta vista

    private async void OnFilterTabSelected(int tabIndex)
    {
        var tabName = FilterTabNavigation.Tabs[tabIndex];
        await _viewModel.FiltrarPorEstadoCommand.ExecuteAsync(tabName);
    }

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantePro.Mobile.Core.Models.DTOs.PreparacionDiariaDto preparacion)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] OnEditarClicked - Preparación: {preparacion?.NombreProducto}");
            await _viewModel.EditarPreparacionCommand.ExecuteAsync(preparacion);
        }
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantePro.Mobile.Core.Models.DTOs.PreparacionDiariaDto preparacion)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] OnEliminarClicked - Preparación: {preparacion?.NombreProducto}");
            await _viewModel.EliminarPreparacionCommand.ExecuteAsync(preparacion);
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        // Garantizar apagado del refresco incluso si hay errores/red lenta
        try
        {
            await _viewModel.RefreshCommand.ExecuteAsync(null);
        }
        finally
        {
            // Failsafe de seguridad
            await Task.Delay(100);
            RefreshControl.IsRefreshing = false;
        }
    }
    
    private async Task OcultarBotonesSiNoTienePermisos()
    {
        try
        {
            // 🔧 OBTENER USUARIO ACTUAL
            var currentUser = await _viewModel.GetCurrentUserAsync();
            
            if (currentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] Usuario no encontrado, ocultando todos los botones");
                OcultarTodosLosBotones();
                return;
            }
            
            // 🔧 VERIFICAR SI ES MESERO
            bool esMesero = currentUser.Roles.Contains("Mesero");
            
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] Usuario: {currentUser.Email}");
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] Es Mesero: {esMesero}");
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] Roles: [{string.Join(", ", currentUser.Roles)}]");
            
            if (esMesero)
            {
                System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] ES MESERO - Ocultando botones de editar/eliminar/crear");
                OcultarBotonesParaMesero();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] NO ES MESERO - Mostrando botones según permisos");
                MostrarBotonesSegunPermisos();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] Error al verificar permisos: {ex.Message}");
            // En caso de error, ocultar todos los botones por seguridad
            OcultarTodosLosBotones();
        }
    }
    
    private void OcultarBotonesParaMesero()
    {
        // 🔧 OCULTAR FLOATING ACTION BUTTON
        if (MainFab != null)
        {
            MainFab.IsVisible = false;
            System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] Floating Action Button ocultado");
        }
        
        // 🔧 OCULTAR BOTÓN DE AGREGAR EN EL HEADER (si existe)
        var botonAgregarHeader = this.FindByName("BotonAgregarHeader") as Button;
        if (botonAgregarHeader != null)
        {
            botonAgregarHeader.IsVisible = false;
            System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] Botón agregar header ocultado");
        }
        
        // 🔧 OCULTAR BOTONES DE EDICIÓN/ELIMINACIÓN EN CADA ITEM
        OcultarBotonesEnItems();
    }
    
    private void OcultarBotonesEnItems()
    {
        // 🔧 BUSCAR TODOS LOS BOTONES DE EDICIÓN Y ELIMINACIÓN EN LA LISTA
        if (RefreshControl?.Content is CollectionView collectionView)
        {
            foreach (var item in collectionView.ItemsSource)
            {
                // Los botones ya están ocultos por IsVisible="False" en XAML
                // Solo necesitamos asegurarnos de que permanezcan ocultos
            }
        }
        System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] Botones de edición/eliminación ocultos para Mesero");
    }
    
    private void MostrarBotonesSegunPermisos()
    {
        // 🔧 MOSTRAR BOTONES SEGÚN PERMISOS DEL VIEWMODEL
        if (MainFab != null)
        {
            MainFab.IsVisible = _viewModel.CanCrearPreparacion;
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] Floating Action Button visible: {MainFab.IsVisible}");
        }
        
        var botonAgregarHeader = this.FindByName("BotonAgregarHeader") as Button;
        if (botonAgregarHeader != null)
        {
            botonAgregarHeader.IsVisible = _viewModel.CanCrearPreparacion;
            System.Diagnostics.Debug.WriteLine($"🔧 [ModernDailyPrepPage] Botón agregar header visible: {botonAgregarHeader.IsVisible}");
        }
    }
    
    private void OcultarTodosLosBotones()
    {
        // 🔧 OCULTAR TODOS LOS BOTONES POR SEGURIDAD
        if (MainFab != null) MainFab.IsVisible = false;
        
        var botonAgregarHeader = this.FindByName("BotonAgregarHeader") as Button;
        if (botonAgregarHeader != null) botonAgregarHeader.IsVisible = false;
        
        System.Diagnostics.Debug.WriteLine("🔧 [ModernDailyPrepPage] Todos los botones ocultados por seguridad");
    }
}


