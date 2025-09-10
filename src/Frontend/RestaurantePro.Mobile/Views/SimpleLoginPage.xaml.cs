using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de Login simplificada para pruebas UI
/// </summary>
public partial class SimpleLoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public SimpleLoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Mostrar botón de debug solo en modo DEBUG
#if DEBUG
        DebugButton.IsVisible = true;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            System.Diagnostics.Debug.WriteLine("[SimpleLoginPage] OnAppearing iniciado");
            
            // Verificar estado de autenticación al aparecer
            if (_viewModel.CheckAuthStatusCommand.CanExecute(null))
            {
                System.Diagnostics.Debug.WriteLine("[SimpleLoginPage] Ejecutando CheckAuthStatusCommand");
                await _viewModel.CheckAuthStatusCommand.ExecuteAsync(null);
            }
            
            System.Diagnostics.Debug.WriteLine("[SimpleLoginPage] OnAppearing completado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimpleLoginPage] Error en OnAppearing: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar la página: {ex.Message}", "OK");
        }
    }

    private async void OnDebugClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SimpleLoginPage] Navegando a página de debug");
            await Shell.Current.GoToAsync("//debug");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimpleLoginPage] Error navegando a debug: {ex.Message}");
            await DisplayAlert("Error", $"Error navegando a debug: {ex.Message}", "OK");
        }
    }
} 