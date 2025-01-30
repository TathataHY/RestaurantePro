using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.App.Models;
using RestaurantePro.App.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantePro.App.ViewModels
{
    [QueryProperty(nameof(ComandaId), "comandaId")]
    public partial class AddDetalleViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _comandaId;

        [ObservableProperty]
        private Plato _plato;

        [ObservableProperty]
        private int _cantidad;

        [ObservableProperty]
        private decimal _precioUnitario;

        [ObservableProperty]
        private string _observaciones;

        [ObservableProperty]
        private bool _isCantidadEnabled;

        public ObservableCollection<Plato> Platos { get; } = new ObservableCollection<Plato>();

        public AddDetalleViewModel()
        {
            LoadPlatosCommand.ExecuteAsync(null); // Cargar los platos disponibles al inicializar
        }

        [RelayCommand]
        private async Task AddDetalle()
        {
            var nuevoDetalle = new ComandaDetalle
            {
                ComandaId = ComandaId,
                PlatoId = Plato.Id,
                Cantidad = Cantidad,
                PrecioUnitario = Plato.Precio,
                Subtotal = Cantidad * Plato.Precio,
                Observaciones = Observaciones
            };

            await _databaseService.SaveComandaDetalleAsync(nuevoDetalle);

            // Restar el stock del plato
            Plato.Stock -= Cantidad;
            await _databaseService.SavePlatoAsync(Plato);

            var snackbar = Snackbar.Make("Detalle de comanda agregado", duration: TimeSpan.FromSeconds(3));
            await snackbar.Show();

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task LoadPlatos()
        {
            var platos = await _databaseService.GetPlatosDisponiblesAsync();
            Platos.Clear();
            foreach (var plato in platos)
            {
                if (plato.Stock > 0)
                {
                    Platos.Add(plato);
                }
            }
        }

        partial void OnPlatoChanged(Plato value)
        {
            if (value != null)
            {
                PrecioUnitario = value.Precio;
                Cantidad = 1; // Inicializar la cantidad a 1
                IsCantidadEnabled = true; // Habilitar el Stepper
            }
            else
            {
                IsCantidadEnabled = false; // Deshabilitar el Stepper
            }
        }

        partial void OnCantidadChanged(int value)
        {
            if (Plato != null)
            {
                PrecioUnitario = Plato.Precio * value;
            }
        }
    }
}