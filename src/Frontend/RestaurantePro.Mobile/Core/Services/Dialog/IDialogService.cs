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
} 