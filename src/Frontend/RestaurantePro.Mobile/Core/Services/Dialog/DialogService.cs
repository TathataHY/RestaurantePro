namespace RestaurantePro.Mobile.Core.Services.Dialog;

/// <summary>
/// Implementación del servicio de diálogos - V1
/// </summary>
public class DialogService : IDialogService
{
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }

    public async Task ShowErrorAsync(string message)
    {
        await ShowAlertAsync("Error", message, "OK");
    }

    public async Task ShowSuccessAsync(string message)
    {
        await ShowAlertAsync("Éxito", message, "OK");
    }
} 