using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantePro.App.ViewModels
{
    [QueryProperty(nameof(MesaId), "mesaId")]
    public partial class MesaDetailViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _mesaId;

        [ObservableProperty]
        private string _numero;

        [ObservableProperty]
        private int _capacidad;

        [ObservableProperty]
        private EstadoMesa _estado;

        public ObservableCollection<EstadoMesa> Estados { get; } = new ObservableCollection<EstadoMesa>(Enum.GetValues(typeof(EstadoMesa)).Cast<EstadoMesa>());

        public MesaDetailViewModel()
        {
        }

        [RelayCommand]
        private async Task Save()
        {
            var mesa = new Mesa
            {
                Id = MesaId,
                Numero = Numero,
                Capacidad = Capacidad,
                Estado = Estado
            };

            await _apiService.SaveMesaAsync(mesa);
            await Shell.Current.GoToAsync("..");
        }

        public async void LoadMesa(int mesaId)
        {
            if (mesaId != 0)
            {
                var mesa = await _apiService.GetMesaByIdAsync(mesaId);
                if (mesa != null)
                {
                    MesaId = mesa.Id;
                    Numero = mesa.Numero;
                    Capacidad = mesa.Capacidad;
                    Estado = mesa.Estado;
                }
            }
        }
    }
}