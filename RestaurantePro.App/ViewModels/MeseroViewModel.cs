using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.ViewModels
{
    public partial class MeseroViewModel : BaseViewModel
    {
        public ObservableCollection<Comanda> Comandas { get; } = new ObservableCollection<Comanda>();

        public MeseroViewModel()
        {
            MessagingCenter.Subscribe<SignalRService, (int, EstadoComanda)>(this, "ComandaStatusChanged", 
                async (sender, tuple) =>
            {
                var (comandaId, newStatus) = tuple;
                if (newStatus == EstadoComanda.Lista)
                {
                    await LoadComandasCommand.ExecuteAsync(null);
                }
            });
        }

        public async Task InitializeAsync()
        {
            await LoadComandasCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task LoadComandas()
        {
            var comandas = await _apiService.GetComandasAsync();
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
                comanda.Detalles = await _apiService.GetComandaDetallesAsync(comanda.Id);
                comanda.Total = comanda.Detalles.Sum(d => d.Subtotal);

                comanda.Estado = EstadoComanda.Cancelada;
                await _apiService.SaveComandaAsync(comanda);

                // Actualizar el estado de la mesa a disponible
                var mesa = await _apiService.GetMesaByIdAsync(comanda.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = EstadoMesa.Disponible;
                    await _apiService.SaveMesaAsync(mesa);
                }

                await LoadComandasCommand.ExecuteAsync(null);
            }
        }
    }
}