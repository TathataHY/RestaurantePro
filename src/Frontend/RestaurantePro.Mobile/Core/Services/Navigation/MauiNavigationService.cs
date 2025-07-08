using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Core.Services.Navigation
{
    public class MauiNavigationService : NavigationServiceBase
    {
        public override async Task NavigateToAsync(string route)
        {
            await Shell.Current.GoToAsync(route);
        }

        public override async Task NavigateToAsync(string route, IDictionary<string, object> parameters)
        {
            await Shell.Current.GoToAsync(route, parameters);
        }

        public override async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public override async Task GoBackAsync(IDictionary<string, object> parameters)
        {
            await Shell.Current.GoToAsync("..", parameters);
        }

        public override async Task GoToRootAsync()
        {
            await Shell.Current.GoToAsync("//");
        }
    }
} 