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
                
                // 🎯 OPTIMIZACIÓN: Cargar categorías PRIMERO (son pocas y críticas para el filtro)
                if (vm.Categorias.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"🗂️ [CreateDailyPreparationPage] Cargando categorías PRIMERO...");
                    await vm.CargarCategoriasCommand.ExecuteAsync(null);
                }
                
                // 📦 DESPUÉS: Cargar productos (pueden ser muchos)
                if (vm.Productos.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"📦 [CreateDailyPreparationPage] Cargando productos DESPUÉS...");
                    await vm.BuscarProductosCommand.ExecuteAsync(null);
                }
                
                System.Diagnostics.Debug.WriteLine($"⚡ [CreateDailyPreparationPage] Carga secuencial completada - Categorías: {vm.Categorias.Count}, Productos: {vm.Productos.Count}");
            }
        }
}


