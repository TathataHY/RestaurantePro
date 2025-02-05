using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.ViewModels
{
    public partial class CocinaViewModel : BaseViewModel
    {
        public ObservableCollection<ComandaDto> ComandasPendientes { get; } = new ObservableCollection<ComandaDto>();

        public CocinaViewModel()
        {
            ComandasPendientes = new ObservableCollection<ComandaDto>();

            MessagingCenter.Subscribe<SignalRService, ComandaDto>(this, "ComandaCreated", async (sender, comanda) =>
            {
                await LoadComandasPendientes();
            });

            MessagingCenter.Subscribe<SignalRService, (int, EstadoComanda)>(this, "ComandaStatusChanged", 
                async (sender, tuple) =>
            {
                var (comandaId, newStatus) = tuple;
                await LoadComandasPendientes();
            });
        }

        public async Task InitializeAsync()
        {
            await _signalRService.StartAsync();
            await _signalRService.JoinGroupAsync("Cocinero");
            await LoadComandasPendientes();
        }

        private async Task LoadComandasPendientes()
        {
            var comandas = await _apiService.GetComandasPendientesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ComandasPendientes.Clear();
                foreach (var comanda in comandas)
                {
                    ComandasPendientes.Add(comanda);
                }
            });
        }

        [RelayCommand]
        private async Task LoadComandas()
        {
            var comandas = await _apiService.GetComandasAsync();
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

                await _apiService.SaveComandaAsync(comanda);
                await LoadComandasCommand.ExecuteAsync(null);
            }
        }
    }
}