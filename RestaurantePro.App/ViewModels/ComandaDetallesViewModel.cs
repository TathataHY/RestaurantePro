using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;
using RestaurantePro.App.Views;

namespace RestaurantePro.App.ViewModels
{
    [QueryProperty(nameof(ComandaId), "comandaId")]
    public partial class ComandaDetallesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _comandaId;

        [ObservableProperty]
        private decimal _total;

        public ObservableCollection<ComandaDetalle> Detalles { get; } = new ObservableCollection<ComandaDetalle>();

        public ComandaDetallesViewModel()
        {
        }

        [RelayCommand]
        private async Task LoadDetalles()
        {
            var detalles = await _databaseService.GetComandaDetallesAsync(ComandaId);
            Detalles.Clear();
            foreach (var detalle in detalles)
            {
                Detalles.Add(detalle);
            }
            Total = Detalles.Sum(d => d.Subtotal);
        }

        [RelayCommand]
        private async Task AddDetalle()
        {
            await Shell.Current.GoToAsync($"{nameof(AddDetallePage)}?comandaId={ComandaId}");
        }
    }
}