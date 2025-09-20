using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

namespace RestaurantePro.Mobile.Features.DailyPreparations.Pages;

public partial class CreateDailyPreparationPage : ContentPage
{
    public CreateDailyPreparationPage(CreateDailyPreparationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CreateDailyPreparationViewModel vm)
            {
                // 🔐 IMPORTANTE: Inicializar autorización ANTES de cargar datos
                await vm.InitializeWithAuthorizationAsync();
                
                // 🚀 OPTIMIZACIÓN: Cargar categorías y productos en paralelo
                var categoriasTask = vm.Categorias.Count == 0 ? vm.CargarCategoriasCommand.ExecuteAsync(null) : Task.CompletedTask;
                var productosTask = vm.Productos.Count == 0 ? vm.BuscarProductosCommand.ExecuteAsync(null) : Task.CompletedTask;
                
                // Esperar ambas tareas en paralelo
                await Task.WhenAll(categoriasTask, productosTask);
                
                System.Diagnostics.Debug.WriteLine($"⚡ [CreateDailyPreparationPage] Carga paralela completada");
            }
        }
}


