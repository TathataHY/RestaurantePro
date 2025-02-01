using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;

namespace RestaurantePro.App.ViewModels
{
    public partial class MeseroViewModel : BaseViewModel
    {
        public ObservableCollection<Comanda> Comandas { get; } = new ObservableCollection<Comanda>();

        public MeseroViewModel()
        {
        }

        [RelayCommand]
        private async Task LoadComandas()
        {
            var comandas = await _databaseService.GetComandasAsync();
            Comandas.Clear();
            foreach (var comanda in comandas)
            {
                if (comanda.Estado == EstadoComanda.Lista)
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
                // Calcular el total de la comanda
                comanda.Detalles = await _databaseService.GetComandaDetallesAsync(comanda.Id);
                comanda.Total = comanda.Detalles.Sum(d => d.Subtotal);

                comanda.Estado = EstadoComanda.Cancelada;
                await _databaseService.SaveComandaAsync(comanda);

                // Actualizar el estado de la mesa a disponible
                var mesa = await _databaseService.GetMesaByIdAsync(comanda.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = EstadoMesa.Disponible;
                    await _databaseService.SaveMesaAsync(mesa);
                }

                await LoadComandas();
            }
        }
    }
}