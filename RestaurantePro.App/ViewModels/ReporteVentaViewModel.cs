using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.ViewModels
{
    public partial class ReporteVentaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Today;

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Today;

        [ObservableProperty]
        private decimal _totalVentas;

        public ObservableCollection<Venta> Ventas { get; } = new ObservableCollection<Venta>();

        public ReporteVentaViewModel()
        {
        }

        [RelayCommand]
        private async Task GenerarReporte()
        {
            Ventas.Clear();
            var ventas = await _apiService.GetVentasAsync(FechaInicio, FechaFin);
            decimal total = 0;

            foreach (var venta in ventas)
            {
                Ventas.Add(venta);
                total += venta.Monto;
            }

            TotalVentas = total;
        }
    }
}