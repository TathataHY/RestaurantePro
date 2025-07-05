using RestaurantePro.Mobile.Features.Operations.Mesas.Pages;
using RestaurantePro.Mobile.Features.Operations.Comandas.Pages;
using RestaurantePro.Mobile.Features.Operations.Productos.Pages;

namespace RestaurantePro.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Registrar rutas para navegación programática
		RegisterRoutes();
	}

	/// <summary>
	/// Registrar rutas para navegación programática
	/// </summary>
	private void RegisterRoutes()
	{
		// Páginas de detalle
		Routing.RegisterRoute("mesa-detalle", typeof(MesaDetallePage));
		Routing.RegisterRoute("comanda-detalle", typeof(ComandaDetallePage));
		Routing.RegisterRoute("producto-detalle", typeof(ProductoDetallePage));
		
		// Futuras páginas de detalle
		// Routing.RegisterRoute("producto-detalle", typeof(ProductoDetallePage));
	}
}
