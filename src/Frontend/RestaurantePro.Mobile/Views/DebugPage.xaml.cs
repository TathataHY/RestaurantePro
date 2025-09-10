using System.Text;

namespace RestaurantePro.Mobile.Views;

public partial class DebugPage : ContentPage
{
    private readonly StringBuilder _debugLog = new();
    private readonly Timer _logTimer;

    public DebugPage()
    {
        InitializeComponent();
        
        // Configurar timer para actualizar logs cada segundo
        _logTimer = new Timer(UpdateLogs, null, 1000, 1000);
        
        // Cargar información inicial
        LoadAppInfo();
        AddLog("Página de Debug inicializada");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AddLog("Página de Debug apareciendo");
    }

    private void LoadAppInfo()
    {
        try
        {
            var appInfo = new StringBuilder();
            appInfo.AppendLine($"Versión: {AppInfo.VersionString}");
            appInfo.AppendLine($"Build: {AppInfo.BuildString}");
            appInfo.AppendLine($"Plataforma: {DeviceInfo.Platform}");
            appInfo.AppendLine($"Versión OS: {DeviceInfo.VersionString}");
            appInfo.AppendLine($"Modelo: {DeviceInfo.Model}");
            appInfo.AppendLine($"Fabricante: {DeviceInfo.Manufacturer}");
            appInfo.AppendLine($"Tipo de dispositivo: {DeviceInfo.DeviceType}");
            appInfo.AppendLine($"Pantalla: {DeviceDisplay.MainDisplayInfo.Width}x{DeviceDisplay.MainDisplayInfo.Height}");
            appInfo.AppendLine($"Densidad: {DeviceDisplay.MainDisplayInfo.Density}");
            
            AppInfoLabel.Text = appInfo.ToString();
            AddLog("Información de la aplicación cargada");
        }
        catch (Exception ex)
        {
            AddLog($"Error cargando información: {ex.Message}");
        }
    }

    private void AddLog(string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _debugLog.AppendLine($"[{timestamp}] {message}");
            
            // Mantener solo las últimas 100 líneas
            var lines = _debugLog.ToString().Split('\n');
            if (lines.Length > 100)
            {
                _debugLog.Clear();
                _debugLog.AppendLine(string.Join("\n", lines.Skip(lines.Length - 100)));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error agregando log: {ex.Message}");
        }
    }

    private void UpdateLogs(object? state)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DebugLogLabel.Text = _debugLog.ToString();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error actualizando logs: {ex.Message}");
        }
    }

    private async void OnClearLogsClicked(object sender, EventArgs e)
    {
        try
        {
            _debugLog.Clear();
            AddLog("Logs limpiados");
            await DisplayAlert("Debug", "Logs limpiados", "OK");
        }
        catch (Exception ex)
        {
            AddLog($"Error limpiando logs: {ex.Message}");
        }
    }

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        try
        {
            AddLog("Probando conexión al backend...");
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            
            var baseUrl = "http://10.0.2.2:8080";
            AddLog($"URL de prueba: {baseUrl}");
            
            var response = await client.GetAsync($"{baseUrl}/api/health");
            AddLog($"Respuesta: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                AddLog($"Contenido: {content}");
                await DisplayAlert("Conexión", "Conexión exitosa al backend", "OK");
            }
            else
            {
                AddLog($"Error HTTP: {response.StatusCode}");
                await DisplayAlert("Conexión", $"Error: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            AddLog($"Error probando conexión: {ex.Message}");
            await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        try
        {
            AddLog("Navegando de vuelta al login...");
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            AddLog($"Error navegando: {ex.Message}");
            await DisplayAlert("Error", $"Error navegando: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _logTimer?.Dispose();
    }
}
