using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.ViewModels
{
    public partial class ReporteVentaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<ReporteVenta> _reporteVentas;

        public ReporteVentaViewModel()
        {
            ReporteVentas = new ObservableCollection<ReporteVenta>();
        }

        [RelayCommand]
        private async Task LoadReporteVentas()
        {
            var startDate = DateTime.Now.AddDays(-30); // Últimos 30 días
            var endDate = DateTime.Now;

            var reporteVentas = await _databaseService.GetReporteVentasAsync(startDate, endDate);
            ReporteVentas.Clear();
            foreach (var reporte in reporteVentas)
            {
                ReporteVentas.Add(reporte);
            }
        }
    }
}