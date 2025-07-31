using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RestaurantePro.Mobile.ViewModels;

/// <summary>
/// ViewModel para la página de perfil - V4
/// </summary>
public class PerfilViewModel : INotifyPropertyChanged
{
    public PerfilViewModel()
    {
        
        // Inicializar comandos
        EditarPerfilCommand = new Command(async () => await EditarPerfilAsync());
        CambiarContrasenaCommand = new Command(async () => await CambiarContrasenaAsync());
        ConfigurarNotificacionesCommand = new Command(async () => await ConfigurarNotificacionesAsync());
        VerHistorialCommand = new Command(async () => await VerHistorialAsync());
        ExportarDatosCommand = new Command(async () => await ExportarDatosAsync());
        CerrarSesionCommand = new Command(async () => await CerrarSesionAsync());
        
        // Cargar datos del perfil
        LoadProfileData();
    }

    #region Propiedades del Perfil

    private string _nombreCompleto = "Juan Pérez";
    public string NombreCompleto
    {
        get => _nombreCompleto;
        set
        {
            if (_nombreCompleto != value)
            {
                _nombreCompleto = value;
                OnPropertyChanged();
            }
        }
    }

    private string _email = "juan.perez@restaurantepro.com";
    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged();
            }
        }
    }

    private string _telefono = "+34 600 123 456";
    public string Telefono
    {
        get => _telefono;
        set
        {
            if (_telefono != value)
            {
                _telefono = value;
                OnPropertyChanged();
            }
        }
    }

    private DateTime _fechaNacimiento = new DateTime(1990, 5, 15);
    public DateTime FechaNacimiento
    {
        get => _fechaNacimiento;
        set
        {
            if (_fechaNacimiento != value)
            {
                _fechaNacimiento = value;
                OnPropertyChanged();
            }
        }
    }

    private string _rol = "Camarero Senior";
    public string Rol
    {
        get => _rol;
        set
        {
            if (_rol != value)
            {
                _rol = value;
                OnPropertyChanged();
            }
        }
    }

    private string _departamento = "Sala";
    public string Departamento
    {
        get => _departamento;
        set
        {
            if (_departamento != value)
            {
                _departamento = value;
                OnPropertyChanged();
            }
        }
    }

    private DateTime _fechaIngreso = new DateTime(2020, 3, 1);
    public DateTime FechaIngreso
    {
        get => _fechaIngreso;
        set
        {
            if (_fechaIngreso != value)
            {
                _fechaIngreso = value;
                OnPropertyChanged();
            }
        }
    }

    private string _estado = "Activo";
    public string Estado
    {
        get => _estado;
        set
        {
            if (_estado != value)
            {
                _estado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstadoColor));
            }
        }
    }

    public Color EstadoColor => Estado switch
    {
        "Activo" => Colors.Green,
        "Inactivo" => Colors.Red,
        "Vacaciones" => Colors.Orange,
        _ => Colors.Gray
    };

    #endregion

    #region Estadísticas

    private int _comandasAtendidas = 1247;
    public int ComandasAtendidas
    {
        get => _comandasAtendidas;
        set
        {
            if (_comandasAtendidas != value)
            {
                _comandasAtendidas = value;
                OnPropertyChanged();
            }
        }
    }

    private int _horasTrabajadas = 1840;
    public int HorasTrabajadas
    {
        get => _horasTrabajadas;
        set
        {
            if (_horasTrabajadas != value)
            {
                _horasTrabajadas = value;
                OnPropertyChanged();
            }
        }
    }

    private double _calificacion = 4.8;
    public double Calificacion
    {
        get => _calificacion;
        set
        {
            if (_calificacion != value)
            {
                _calificacion = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Configuración de Privacidad

    private bool _autenticacionDosFactores = false;
    public bool AutenticacionDosFactores
    {
        get => _autenticacionDosFactores;
        set
        {
            if (_autenticacionDosFactores != value)
            {
                _autenticacionDosFactores = value;
                OnPropertyChanged();
                SavePrivacySettings();
            }
        }
    }

    private bool _sincronizacionNube = true;
    public bool SincronizacionNube
    {
        get => _sincronizacionNube;
        set
        {
            if (_sincronizacionNube != value)
            {
                _sincronizacionNube = value;
                OnPropertyChanged();
                SavePrivacySettings();
            }
        }
    }

    private bool _notificacionesSeguridad = true;
    public bool NotificacionesSeguridad
    {
        get => _notificacionesSeguridad;
        set
        {
            if (_notificacionesSeguridad != value)
            {
                _notificacionesSeguridad = value;
                OnPropertyChanged();
                SavePrivacySettings();
            }
        }
    }

    #endregion

    #region Comandos

    public ICommand EditarPerfilCommand { get; }
    public ICommand CambiarContrasenaCommand { get; }
    public ICommand ConfigurarNotificacionesCommand { get; }
    public ICommand VerHistorialCommand { get; }
    public ICommand ExportarDatosCommand { get; }
    public ICommand CerrarSesionCommand { get; }

    #endregion

    #region Métodos Privados

    private void LoadProfileData()
    {
        try
        {
            // Aquí se cargarían los datos reales del perfil desde el servicio
            // Por ahora usamos datos de ejemplo
            System.Diagnostics.Debug.WriteLine("Cargando datos del perfil...");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile data: {ex.Message}");
        }
    }

    private void SavePrivacySettings()
    {
        try
        {
            // Guardar configuración de privacidad
            System.Diagnostics.Debug.WriteLine("Guardando configuración de privacidad...");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving privacy settings: {ex.Message}");
        }
    }

    private async Task EditarPerfilAsync()
    {
        try
        {
            await Application.Current.MainPage.DisplayAlert("Editar Perfil", 
                "Funcionalidad de edición de perfil en desarrollo.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error editing profile: {ex.Message}");
        }
    }

    private async Task CambiarContrasenaAsync()
    {
        try
        {
            await Application.Current.MainPage.DisplayAlert("Cambiar Contraseña", 
                "Funcionalidad de cambio de contraseña en desarrollo.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error changing password: {ex.Message}");
        }
    }

    private async Task ConfigurarNotificacionesAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//configuracion");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to notifications: {ex.Message}");
        }
    }

    private async Task VerHistorialAsync()
    {
        try
        {
            await Application.Current.MainPage.DisplayAlert("Ver Historial", 
                "Funcionalidad de historial en desarrollo.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error viewing history: {ex.Message}");
        }
    }

    private async Task ExportarDatosAsync()
    {
        try
        {
            await Application.Current.MainPage.DisplayAlert("Exportar Datos", 
                "Funcionalidad de exportación en desarrollo.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting data: {ex.Message}");
        }
    }

    private async Task CerrarSesionAsync()
    {
        try
        {
            var confirmacion = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión", 
                "¿Estás seguro de que quieres cerrar sesión?", "Sí", "No");
            
            if (confirmacion)
            {
                // Aquí se implementaría la lógica de cierre de sesión
                await Shell.Current.GoToAsync("//login");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error logging out: {ex.Message}");
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        // Limpiar recursos si es necesario
    }

    #endregion
} 