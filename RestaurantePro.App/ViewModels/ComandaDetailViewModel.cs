using System.Collections.ObjectModel;
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
    public partial class ComandaDetailViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _comandaId;

        [ObservableProperty]
        private DateTime _fechaHora = DateTime.Now;

        [ObservableProperty]
        private Mesa _mesa;

        [ObservableProperty]
        private EstadoComanda _estado;

        public ObservableCollection<Mesa> Mesas { get; } = new ObservableCollection<Mesa>();
        public ObservableCollection<EstadoComanda> Estados { get; } = new ObservableCollection<EstadoComanda>(Enum.GetValues(typeof(EstadoComanda)).Cast<EstadoComanda>());
        public ObservableCollection<ComandaDetalle> Detalles { get; } = new ObservableCollection<ComandaDetalle>();

        private Mesa _originalMesa;

        public ComandaDetailViewModel()
        {
        }

        [RelayCommand]
        private async Task Save()
        {
            var comanda = new Comanda
            {
                Id = ComandaId,
                FechaHora = FechaHora,
                MesaId = Mesa?.Id ?? 0,
                Estado = Estado
            };

            await _databaseService.SaveComandaAsync(comanda);

            // Actualizar el estado de la mesa
            if (_originalMesa != null && _originalMesa.Id != Mesa.Id)
            {
                _originalMesa.Estado = EstadoMesa.Disponible;
                await _databaseService.SaveMesaAsync(_originalMesa);
            }

            if (Mesa != null)
            {
                Mesa.Estado = EstadoMesa.Ocupada;
                await _databaseService.SaveMesaAsync(Mesa);
            }

            await Shell.Current.GoToAsync("..");
        }

        public async Task LoadComanda(int comandaId)
        {
            var comanda = await _databaseService.GetComandaByIdAsync(comandaId);
            if (comanda != null)
            {
                ComandaId = comanda.Id;
                FechaHora = comanda.FechaHora;
                Estado = comanda.Estado;

                // Agregar la mesa original si no está en la lista de mesas disponibles
                _originalMesa = await _databaseService.GetMesaByIdAsync(comanda.MesaId);
                if (_originalMesa != null && !Mesas.Any(m => m.Id == _originalMesa.Id))
                {
                    Mesas.Add(_originalMesa);
                }

                Mesa = Mesas.FirstOrDefault(m => m.Id == comanda.MesaId);
            }
        }

        [RelayCommand]
        private async Task LoadMesas()
        {
            var mesas = await _databaseService.GetMesasDisponiblesAsync();
            Mesas.Clear();
            foreach (var mesa in mesas)
            {
                Mesas.Add(mesa);
            }
        }
    }
}