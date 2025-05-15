using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.ViewModels;
using RestaurantePro.Mobile.Features.Comercial.Clientes.Models;

namespace RestaurantePro.Mobile.Features.Comercial.Clientes.ViewModels
{
    public class ClientesListViewModel : BaseViewModel
    {
        private readonly IClientesApiClient _clientesApiClient;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        
        private ObservableCollection<ClienteModel> _clientes;
        public ObservableCollection<ClienteModel> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }
        
        private bool _isEmptyList;
        public bool IsEmptyList
        {
            get => _isEmptyList;
            set => SetProperty(ref _isEmptyList, value);
        }
        
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    // Filtrar resultados al cambiar el texto de búsqueda
                    FilterResults();
                }
            }
        }
        
        public ICommand LoadClientsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ViewClientDetailCommand { get; }
        public ICommand AddClientCommand { get; }
        
        public ClientesListViewModel(
            IClientesApiClient clientesApiClient,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _clientesApiClient = clientesApiClient;
            _navigationService = navigationService;
            _dialogService = dialogService;
            
            Title = "Clientes";
            Clientes = new ObservableCollection<ClienteModel>();
            
            LoadClientsCommand = new Command(async () => await LoadClientsAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            SearchCommand = new Command(FilterResults);
            ViewClientDetailCommand = new Command<int>(async (id) => await ViewClientDetailAsync(id));
            AddClientCommand = new Command(async () => await AddClientAsync());
        }
        
        public override async Task InitializeAsync(System.Collections.Generic.IDictionary<string, object> parameters)
        {
            await LoadClientsAsync();
        }
        
        public override async Task OnAppearingAsync()
        {
            await RefreshAsync();
        }
        
        private async Task LoadClientsAsync()
        {
            if (IsBusy)
                return;
            
            try
            {
                IsBusy = true;
                
                // Obtener clientes desde la API
                var clientes = await _clientesApiClient.GetAllClientesAsync();
                
                Clientes.Clear();
                
                // Convertir a modelos de cliente y agregar a la colección
                foreach (var cliente in clientes)
                {
                    Clientes.Add(cliente);
                }
                
                // Actualizar estado de lista vacía
                IsEmptyList = Clientes.Count == 0;
            }
            catch (ApiException ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudieron cargar los clientes: {ex.Message}", "Aceptar");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}", "Aceptar");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }
        
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadClientsAsync();
        }
        
        private void FilterResults()
        {
            if (IsBusy)
                return;
            
            // Aquí implementaríamos el filtrado de clientes basado en SearchQuery
            // Por ahora, simplemente recargamos todos los clientes
            // (En una implementación real, filtrarías en la lista existente o solicitarías
            // un nuevo conjunto de datos filtrados de la API)
            Task.Run(async () => await LoadClientsAsync());
        }
        
        private async Task ViewClientDetailAsync(int id)
        {
            await _navigationService.NavigateToAsync("ClienteDetailPage", new { Id = id });
        }
        
        private async Task AddClientAsync()
        {
            await _navigationService.NavigateToAsync("ClienteAddPage");
        }
    }
} 