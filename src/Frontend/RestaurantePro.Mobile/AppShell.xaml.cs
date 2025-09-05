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
		// Limpiar rutas existentes para evitar duplicados
		Routing.UnRegisterRoute("mesa-detalle");
		Routing.UnRegisterRoute("comanda-detalle");
		Routing.UnRegisterRoute("producto-detalle");
		Routing.UnRegisterRoute("crear-comanda");
		Routing.UnRegisterRoute("editar-comanda");
		
		// Páginas de detalle
		Routing.RegisterRoute("mesa-detalle", typeof(MesaDetallePage));
		Routing.RegisterRoute("comanda-detalle", typeof(ComandaDetallePage));
		Routing.RegisterRoute("producto-detalle", typeof(ProductoDetallePage));
		
		// Páginas de creación
		Routing.RegisterRoute("crear-comanda", typeof(CrearComandaPage));
		Routing.RegisterRoute("editar-comanda", typeof(CrearComandaPage));

		// Editor de productos (crear/editar)
		Routing.UnRegisterRoute("producto-crear");
		Routing.UnRegisterRoute("producto-editar");
		Routing.RegisterRoute("producto-crear", typeof(ProductoEditorPage));
		Routing.RegisterRoute("producto-editar", typeof(ProductoEditorPage));
	}
}
