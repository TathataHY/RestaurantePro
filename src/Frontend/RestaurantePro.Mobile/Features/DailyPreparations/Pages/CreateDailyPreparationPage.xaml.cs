using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

namespace RestaurantePro.Mobile.Features.DailyPreparations.Pages;

public partial class CreateDailyPreparationPage : ContentPage
{
    public CreateDailyPreparationPage(CreateDailyPreparationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}


