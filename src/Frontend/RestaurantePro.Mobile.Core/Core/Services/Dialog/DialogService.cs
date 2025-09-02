using RestaurantePro.Mobile.Core.Services.Dialog;

namespace RestaurantePro.Mobile.Core.Services.Dialog;

/// <summary>
/// Implementación mock del servicio de diálogos para pruebas
/// </summary>
public class MockDialogService : IDialogService
{
    public List<string> AlertsShown { get; } = new();
    public List<string> ConfirmationsShown { get; } = new();
    public List<string> ErrorsShown { get; } = new();
    public List<string> SuccessesShown { get; } = new();
    public List<string> ActionSheetsShown { get; } = new();
    public List<string> PromptsShown { get; } = new();
    
    public bool NextConfirmationResult { get; set; } = true;
    public string? NextActionSheetResult { get; set; } = null;
    public string? NextPromptResult { get; set; } = null;

    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        AlertsShown.Add($"{title}: {message}");
        await Task.CompletedTask;
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        ConfirmationsShown.Add($"{title}: {message}");
        return NextConfirmationResult;
    }

    public async Task ShowErrorAsync(string message)
    {
        ErrorsShown.Add(message);
        await Task.CompletedTask;
    }

    public async Task ShowSuccessAsync(string message)
    {
        SuccessesShown.Add(message);
        await Task.CompletedTask;
    }

    public async Task<string?> ShowActionSheetAsync(string title, string message, string cancel, params string[] buttons)
    {
        ActionSheetsShown.Add($"{title}: {message}");
        await Task.CompletedTask;
        return NextActionSheetResult ?? cancel;
    }

    public async Task<string?> ShowPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancelar", string placeholder = "", int maxLength = -1, string? initialValue = null)
    {
        PromptsShown.Add($"{title}: {message}");
        await Task.CompletedTask;
        return NextPromptResult;
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        ConfirmationsShown.Add($"{title}: {message}");
        return NextConfirmationResult;
    }
}

/// <summary>
/// Implementación real del servicio de diálogos para producción
/// </summary>
public class DialogService : IDialogService
{
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        // TODO: Implementar alerta real con MAUI - requiere inyección de dependencias
        System.Diagnostics.Debug.WriteLine($"🔔 ALERT: {title} - {message}");
        await Task.CompletedTask;
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        // TODO: Implementar confirmación real con MAUI - requiere inyección de dependencias
        System.Diagnostics.Debug.WriteLine($"❓ CONFIRM: {title} - {message}");
        await Task.CompletedTask;
        return true; // Por defecto retorna true para testing
    }

    public async Task ShowErrorAsync(string message)
    {
        // TODO: Implementar error real con MAUI
        await Task.CompletedTask;
    }

    public async Task ShowSuccessAsync(string message)
    {
        // TODO: Implementar éxito real con MAUI
        await Task.CompletedTask;
    }

    public async Task<string?> ShowActionSheetAsync(string title, string message, string cancel, params string[] buttons)
    {
        // TODO: Implementar action sheet real con MAUI
        return cancel;
    }

    public async Task<string?> ShowPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancelar", string placeholder = "", int maxLength = -1, string? initialValue = null)
    {
        // TODO: Implementar prompt real con MAUI - requiere inyección de dependencias
        System.Diagnostics.Debug.WriteLine($"📝 PROMPT: {title} - {message}");
        await Task.CompletedTask;
        return "Test Input"; // Por defecto retorna un valor para testing
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        // TODO: Implementar confirmación real con MAUI
        return true;
    }
}

 