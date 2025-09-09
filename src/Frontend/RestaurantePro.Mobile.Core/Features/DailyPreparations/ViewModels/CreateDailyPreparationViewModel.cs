using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

public partial class CreateDailyPreparationViewModel : ObservableObject
{
    private readonly IDailyPreparationsService _dailyPreparationsService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _productoIdText = string.Empty;

    [ObservableProperty]
    private int _cantidad = 1;

    [ObservableProperty]
    private DateTime _fechaVencimiento = DateTime.Today.AddHours(8);

    [ObservableProperty]
    private string _observaciones = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public CreateDailyPreparationViewModel(
        IDailyPreparationsService dailyPreparationsService,
        IAuthService authService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _dailyPreparationsService = dailyPreparationsService;
        _authService = authService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task CrearPreparacionAsync()
    {
        if (IsBusy) return;

        if (!Guid.TryParse(ProductoIdText, out var productoId))
        {
            await _dialogService.ShowErrorAsync("ProductoId inválido. Usa un GUID válido.");
            return;
        }
        if (Cantidad <= 0)
        {
            await _dialogService.ShowErrorAsync("La cantidad debe ser mayor que 0.");
            return;
        }

        IsBusy = true;
        try
        {
            var userIdStr = await _authService.GetUserIdAsync();
            var chefId = Guid.TryParse(userIdStr, out var chefGuid) ? chefGuid : Guid.Empty;

            var cmd = new CrearPreparacionDiariaCommand
            {
                ProductoId = productoId,
                Cantidad = Cantidad,
                ChefId = chefId,
                FechaVencimiento = FechaVencimiento,
                Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones
            };

            var result = await _dailyPreparationsService.CrearPreparacionDiariaAsync(cmd);
            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Preparación creada correctamente.");
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "No se pudo crear la preparación.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al crear preparación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await _navigationService.GoBackAsync();
    }
}


