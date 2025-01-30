using Microsoft.Extensions.DependencyInjection;
using System;

namespace RestaurantePro.App.Services
{
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;

        public static void Init(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }
    }
}