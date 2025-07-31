using RestaurantePro.Mobile.Services;

namespace RestaurantePro.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		InitializeTheme();
	}

	private void InitializeTheme()
	{
		try
		{
			// Inicializar el tema al arrancar la aplicación
			var themeService = ThemeService.Instance;
			
			// Aplicar el tema guardado o el tema del sistema
			themeService.ApplyTheme();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error initializing theme: {ex.Message}");
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		
		// Configurar la página inicial como login - V1
		return window;
	}
}