using CommunityToolkit.Mvvm.ComponentModel;

namespace RestaurantePro.Mobile.Core.Models.ViewModels;

/// <summary>
/// ViewModel base con funcionalidades comunes - V1 Fundamental
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// Indica si el ViewModel no está ocupado
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Muestra un error al usuario
    /// </summary>
    protected async Task ShowErrorAsync(string message)
    {
        HasError = true;
        ErrorMessage = message;
        
        // En V1 simplemente guardamos el error
        // En V2 se integraría con IDialogService
        await Task.Delay(100); // Placeholder para async
    }

    /// <summary>
    /// Limpia los errores
    /// </summary>
    protected void ClearErrors()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Ejecuta una operación con manejo de loading y errores
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation, bool showLoading = true)
    {
        if (IsBusy) return;

        try
        {
            if (showLoading)
            {
                IsBusy = true;
                IsLoading = true;
            }

            ClearErrors();
            await operation();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            if (showLoading)
            {
                IsBusy = false;
                IsLoading = false;
            }
        }
    }
} 