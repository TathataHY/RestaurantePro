namespace RestaurantePro.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		
		// Configurar la página inicial como login - V1
		return window;
	}
}