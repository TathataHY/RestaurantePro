using RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

namespace RestaurantePro.Mobile.Features.DailyPreparations.Pages;

public partial class EditDailyPreparationPage : ContentPage, IQueryAttributable
{
    private readonly EditDailyPreparationViewModel _vm;

    public EditDailyPreparationPage(EditDailyPreparationViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value))
        {
            if (value is string s && Guid.TryParse(s, out var id))
            {
                await _vm.InitializeAsync(id);
                return;
            }
            if (value is Guid g)
            {
                await _vm.InitializeAsync(g);
            }
        }
    }
}


