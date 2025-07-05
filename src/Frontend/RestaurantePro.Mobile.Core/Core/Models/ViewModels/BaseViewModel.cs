using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RestaurantePro.Mobile.Core.Models.ViewModels;

/// <summary>
/// ViewModel base para todas las vistas - V1 Fundamental
/// </summary>
public partial class BaseViewModel : ObservableObject
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
    /// Muestra un mensaje de error
    /// </summary>
    protected virtual async Task ShowErrorAsync(string message)
    {
        ErrorMessage = message;
        HasError = true;
        
        // En una implementación real, aquí mostraríamos el diálogo
        // Por ahora, simplemente establecemos las propiedades
        await Task.Delay(100); // Simular operación asíncrona
    }

    /// <summary>
    /// Limpia el estado de error
    /// </summary>
    [RelayCommand]
    protected virtual void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Método para manejar la navegación hacia atrás
    /// </summary>
    [RelayCommand]
    protected virtual async Task GoBackAsync()
    {
        // En una implementación real, aquí navegaríamos hacia atrás
        await Task.CompletedTask;
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

            ClearError();
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