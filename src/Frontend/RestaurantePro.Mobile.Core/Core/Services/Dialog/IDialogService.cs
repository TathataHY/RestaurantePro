namespace RestaurantePro.Mobile.Core.Services.Dialog;

/// <summary>
/// Servicio de diálogos y alertas - V1 Fundamental
/// </summary>
public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> ShowConfirmAsync(string title, string message, string accept = "Sí", string cancel = "No");
    Task ShowErrorAsync(string message);
    Task ShowSuccessAsync(string message);
    Task<string?> ShowActionSheetAsync(string title, string message, string cancel, params string[] buttons);
    Task<string?> ShowPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancelar", string placeholder = "", int maxLength = -1, string? initialValue = null);
    Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Sí", string cancel = "No");
} 