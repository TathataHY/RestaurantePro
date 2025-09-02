using RestaurantePro.Mobile.Core.Services.Dialog;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Implementación real del servicio de diálogos para MAUI
/// </summary>
public class DialogService : IDialogService
{
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowAlertAsync: {ex.Message}");
        }
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowConfirmAsync: {ex.Message}");
            return false;
        }
    }

    public async Task ShowErrorAsync(string message)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowErrorAsync: {ex.Message}");
        }
    }

    public async Task ShowSuccessAsync(string message)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowSuccessAsync: {ex.Message}");
        }
    }

    public async Task<string?> ShowActionSheetAsync(string title, string message, string cancel, params string[] buttons)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayActionSheet(title, cancel, null, buttons);
            }
            return cancel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowActionSheetAsync: {ex.Message}");
            return cancel;
        }
    }

    public async Task<string?> ShowPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancelar", string placeholder = "", int maxLength = -1, string? initialValue = null)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                var result = await Application.Current.MainPage.DisplayPromptAsync(
                    title, 
                    message, 
                    accept, 
                    cancel, 
                    placeholder, 
                    maxLength, 
                    Keyboard.Default, 
                    initialValue);
                return result;
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowPromptAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Sí", string cancel = "No")
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en ShowConfirmationAsync: {ex.Message}");
            return false;
        }
    }
}
