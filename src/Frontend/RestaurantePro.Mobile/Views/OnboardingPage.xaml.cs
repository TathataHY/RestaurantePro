using RestaurantePro.Mobile.Core.Features.Onboarding.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de Onboarding moderna - V4 Modernización Visual
/// </summary>
public partial class OnboardingPage : ContentPage
{
    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 