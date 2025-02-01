using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.ViewModels
{
    public partial class CocinaViewModel : BaseViewModel
    {
        public ObservableCollection<Comanda> Comandas { get; } = new ObservableCollection<Comanda>();

        public CocinaViewModel()
        {
        }

        [RelayCommand]
        private async Task LoadComandas()
        {
            var comandas = await _databaseService.GetComandasAsync();
            Comandas.Clear();
            foreach (var comanda in comandas)
            {
                if (comanda.Estado == EstadoComanda.Pendiente || comanda.Estado == EstadoComanda.EnPreparacion)
                {
                    Comandas.Add(comanda);
                }
            }
        }

        [RelayCommand]
        private async Task UpdateEstado(Comanda comanda)
        {
            if (comanda != null)
            {
                if (comanda.Estado == EstadoComanda.Pendiente)
                {
                    comanda.Estado = EstadoComanda.EnPreparacion;
                }
                else if (comanda.Estado == EstadoComanda.EnPreparacion)
                {
                    comanda.Estado = EstadoComanda.Lista;
                }

                await _databaseService.SaveComandaAsync(comanda);
                await LoadComandas();
            }
        }
    }
}