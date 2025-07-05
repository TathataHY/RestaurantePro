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
    
    public bool NextConfirmationResult { get; set; } = true;

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
}

 