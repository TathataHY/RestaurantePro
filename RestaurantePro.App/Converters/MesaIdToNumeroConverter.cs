using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.Converters
{
    public class MesaIdToNumeroConverter : IValueConverter
    {
        private readonly DatabaseService _databaseService = new DatabaseService();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int mesaId)
            {
                var mesa = _databaseService.GetMesaByIdAsync(mesaId).Result;
                return mesa != null ? $"Mesa: {mesa.Numero}" : "Mesa desconocida";
            }
            return "Mesa desconocida";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}