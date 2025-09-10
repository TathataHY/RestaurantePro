namespace RestaurantePro.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Configurar manejo global de excepciones
		SetupGlobalExceptionHandling();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		return window;
	}

	private void SetupGlobalExceptionHandling()
	{
		// Manejar excepciones no controladas
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		
		// Manejar excepciones de tareas
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
		
		// Manejar excepciones de UI
		Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
		{
			Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
			{
				// Esto se ejecutará en el hilo de UI
			});
		});
	}

	private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		var exception = e.ExceptionObject as Exception;
		LogException("UnhandledException", exception);
		ShowExceptionDialog("Error Crítico", exception);
	}

	private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	{
		LogException("UnobservedTaskException", e.Exception);
		ShowExceptionDialog("Error de Tarea", e.Exception);
		e.SetObserved(); // Marcar como observado para evitar que termine la aplicación
	}

	private void LogException(string source, Exception? exception)
	{
		try
		{
			var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {exception?.Message}\n" +
							$"Stack Trace: {exception?.StackTrace}\n" +
							$"Inner Exception: {exception?.InnerException?.Message}";
			
			System.Diagnostics.Debug.WriteLine(logMessage);
			
			// También escribir a un archivo si es posible
			WriteToDebugFile(logMessage);
		}
		catch
		{
			// Si falla el logging, al menos mostrar en consola
			System.Diagnostics.Debug.WriteLine($"Error logging exception: {exception?.Message}");
		}
	}

	private void WriteToDebugFile(string message)
	{
		try
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var filePath = Path.Combine(documentsPath, "RestaurantePro_Debug.log");
			File.AppendAllText(filePath, message + "\n\n");
		}
		catch
		{
			// Ignorar si no se puede escribir al archivo
		}
	}

	private void ShowExceptionDialog(string title, Exception? exception)
	{
		try
		{
			// Mostrar un diálogo simple con el error
			var message = $"Error: {exception?.Message}\n\n" +
						 $"Tipo: {exception?.GetType().Name}\n\n" +
						 $"Stack Trace:\n{exception?.StackTrace}";
			
			// Usar DisplayAlert si está disponible
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					if (Current?.MainPage != null)
					{
						await Current.MainPage.DisplayAlert(title, message, "OK");
					}
				}
				catch
				{
					// Si no se puede mostrar el diálogo, al menos loguear
					System.Diagnostics.Debug.WriteLine($"No se pudo mostrar diálogo: {message}");
				}
			});
		}
		catch
		{
			// Si todo falla, al menos loguear
			System.Diagnostics.Debug.WriteLine($"Error mostrando diálogo: {exception?.Message}");
		}
	}
}