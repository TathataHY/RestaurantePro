using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Preferences;

namespace RestaurantePro.Mobile.Core.Features.Onboarding.ViewModels;

/// <summary>
/// ViewModel para la página de Onboarding - V4 Modernización Visual
/// </summary>
public partial class OnboardingViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IPreferencesService _preferencesService;

    [ObservableProperty]
    private ObservableCollection<OnboardingItem> onboardingItems;

    [ObservableProperty]
    private double progress;

    public OnboardingViewModel(INavigationService navigationService, IPreferencesService preferencesService)
    {
        _navigationService = navigationService;
        _preferencesService = preferencesService;
        Title = "Bienvenido a RestaurantePro";
        
        InitializeOnboardingItems();
    }

    /// <summary>
    /// Inicializa los elementos del onboarding
    /// </summary>
    private void InitializeOnboardingItems()
    {
        OnboardingItems = new ObservableCollection<OnboardingItem>
        {
            new OnboardingItem
            {
                Icon = "🍽️",
                Title = "Gestión Completa de Restaurante",
                Description = "Administra mesas, comandas, inventario y más desde una sola aplicación moderna y fácil de usar.",
                ActionText = "Siguiente",
                AccentColor = Color.FromHex("#FF6B35")
            },
            new OnboardingItem
            {
                Icon = "📱",
                Title = "Interfaz Moderna y Responsive",
                Description = "Diseño actualizado con la versión 4.0 que ofrece una experiencia visual profesional y atractiva.",
                ActionText = "Siguiente",
                AccentColor = Color.FromHex("#3498DB")
            },
            new OnboardingItem
            {
                Icon = "⚡",
                Title = "Operaciones Rápidas",
                Description = "Crea comandas, gestiona mesas y procesa pagos de manera eficiente con nuestra interfaz optimizada.",
                ActionText = "Siguiente",
                AccentColor = Color.FromHex("#2ECC71")
            },
            new OnboardingItem
            {
                Icon = "📊",
                Title = "Reportes en Tiempo Real",
                Description = "Monitorea el rendimiento de tu restaurante con reportes detallados y análisis en tiempo real.",
                ActionText = "Siguiente",
                AccentColor = Color.FromHex("#9B59B6")
            },
            new OnboardingItem
            {
                Icon = "🎯",
                Title = "¡Listo para Comenzar!",
                Description = "Ya tienes todo lo necesario para gestionar tu restaurante de manera profesional. ¡Empecemos!",
                ActionText = "Comenzar",
                AccentColor = Color.FromHex("#F39C12")
            }
        };

        UpdateProgress();
    }

    /// <summary>
    /// Comando para ir a la siguiente página del onboarding
    /// </summary>
    [RelayCommand]
    private async Task NextAsync()
    {
        try
        {
            // Marcar onboarding como completado
            await _preferencesService.SetAsync("OnboardingCompleted", true);
            
            // Navegar al login
            await _navigationService.NavigateToAsync("//login");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error al completar onboarding: {ex.Message}");
        }
    }

    /// <summary>
    /// Comando para saltar el onboarding
    /// </summary>
    [RelayCommand]
    private async Task SkipAsync()
    {
        try
        {
            // Marcar onboarding como completado
            await _preferencesService.SetAsync("OnboardingCompleted", true);
            
            // Navegar al login
            await _navigationService.NavigateToAsync("//login");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error al saltar onboarding: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza el progreso del onboarding
    /// </summary>
    private void UpdateProgress()
    {
        if (OnboardingItems?.Count > 0)
        {
            Progress = 1.0 / OnboardingItems.Count;
        }
    }
}

/// <summary>
/// Modelo para los elementos del onboarding
/// </summary>
public class OnboardingItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public Color AccentColor { get; set; } = Colors.Gray;
} 