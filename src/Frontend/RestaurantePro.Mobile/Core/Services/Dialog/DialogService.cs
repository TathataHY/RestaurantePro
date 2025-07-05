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

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Confirmar", string cancel = "Cancelar")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }

    public async Task ShowErrorAsync(string message)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
        }
    }

    public async Task ShowSuccessAsync(string message)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Éxito", message, "OK");
        }
    }

    public async Task<string?> ShowActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
        }
        return null;
    }

    public async Task<string?> ShowPromptAsync(string title, string message, string placeholder = "", string accept = "OK", string cancel = "Cancelar")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayPromptAsync(title, message, accept, cancel, placeholder);
        }
        return null;
    }
} 