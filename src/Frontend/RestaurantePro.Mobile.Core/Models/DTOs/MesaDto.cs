using System.ComponentModel;

namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO con información de una mesa
/// </summary>
public class MesaDto : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// ID único de la mesa
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la entidad
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha y hora de la última modificación
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Usuario que creó la entidad
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realizó la última modificación
    /// </summary>
    public string? ModificadoPor { get; set; }

    /// <summary>
    /// Indica si la entidad está activa
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public string Numero { get; set; } = string.Empty;
    
    /// <summary>
    /// Capacidad de la mesa (número de personas)
    /// </summary>
    public int Capacidad { get; set; }
    
    private string _estado = string.Empty;
    
    /// <summary>
    /// Estado actual de la mesa
    /// </summary>
    public string Estado 
    { 
        get => _estado;
        set
        {
            if (_estado != value)
            {
                _estado = value;
                OnPropertyChanged(nameof(Estado));
                OnPropertyChanged(nameof(EstadoDescripcion));
                OnPropertyChanged(nameof(EstadoColor));
                OnPropertyChanged(nameof(Disponible));
                OnPropertyChanged(nameof(Ocupada));
                OnPropertyChanged(nameof(Reservada));
                OnPropertyChanged(nameof(FueraDeServicio));
            }
        }
    }
    
    /// <summary>
    /// ID del cliente asignado (si está ocupada)
    /// </summary>
    public Guid? ClienteId { get; set; }
    
    /// <summary>
    /// Nombre del cliente (si está disponible)
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Ubicación de la mesa
    /// </summary>
    public string Ubicacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Zona o área donde se encuentra la mesa
    /// </summary>
    public string Zona { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de mesa
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Observaciones sobre la mesa
    /// </summary>
    public string Observaciones { get; set; } = string.Empty;
    
    /// <summary>
    /// Hora de la última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }

    /// <summary>
    /// Descripción adicional de la mesa
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Notas especiales sobre la mesa
    /// </summary>
    public string? Notas { get; set; }
    
    /// <summary>
    /// Indica si la mesa tiene ventana
    /// </summary>
    public bool TieneVentana { get; set; }
    
    /// <summary>
    /// Indica si la mesa tiene sofá
    /// </summary>
    public bool TieneSofa { get; set; }
    
    /// <summary>
    /// Indica si la mesa es accesible para personas con discapacidad
    /// </summary>
    public bool EsAccesible { get; set; }
    
    /// <summary>
    /// Indica si la mesa tiene enchufes disponibles
    /// </summary>
    public bool TieneEnchufe { get; set; }

    /// <summary>
    /// Color para mostrar en la UI según el estado
    /// </summary>
    public Microsoft.Maui.Graphics.Color EstadoColor
    {
        get
        {
            var normalized = new string((Estado ?? string.Empty)
                .ToLowerInvariant()
                .Where(char.IsLetter)
                .ToArray());
            return normalized switch
            {
                "disponible" => Microsoft.Maui.Graphics.Color.FromArgb("#4CAF50"), // Verde
                "ocupada" => Microsoft.Maui.Graphics.Color.FromArgb("#F44336"),    // Rojo
                "reservada" => Microsoft.Maui.Graphics.Color.FromArgb("#FF9800"),  // Naranja
                "fueradeservicio" => Microsoft.Maui.Graphics.Color.FromArgb("#9E9E9E"), // Gris
                _ => Microsoft.Maui.Graphics.Color.FromArgb("#607D8B") // Gris azulado por defecto
            };
        }
    }

    /// <summary>
    /// Descripción amigable del estado
    /// </summary>
    public string EstadoDescripcion
    {
        get
        {
            var normalized = new string((Estado ?? string.Empty)
                .ToLowerInvariant()
                .Where(char.IsLetter)
                .ToArray());
            return normalized switch
            {
                "disponible" => "Disponible",
                "ocupada" => "Ocupada",
                "reservada" => "Reservada",
                "fueradeservicio" => "Fuera de Servicio",
                _ => Estado
            };
        }
    }

    /// <summary>
    /// Indica si la mesa está disponible
    /// </summary>
    public bool Disponible
    {
        get
        {
            var normalized = new string((Estado ?? string.Empty).ToLowerInvariant().Where(char.IsLetter).ToArray());
            return normalized == "disponible";
        }
    }

    /// <summary>
    /// Indica si la mesa está ocupada
    /// </summary>
    public bool Ocupada
    {
        get
        {
            var normalized = new string((Estado ?? string.Empty).ToLowerInvariant().Where(char.IsLetter).ToArray());
            return normalized == "ocupada";
        }
    }

    /// <summary>
    /// Indica si la mesa está reservada
    /// </summary>
    public bool Reservada
    {
        get
        {
            var normalized = new string((Estado ?? string.Empty).ToLowerInvariant().Where(char.IsLetter).ToArray());
            return normalized == "reservada";
        }
    }

    /// <summary>
    /// Indica si la mesa está fuera de servicio
    /// </summary>
    public bool FueraDeServicio
    {
        get
        {
            var normalized = new string((Estado ?? string.Empty).ToLowerInvariant().Where(char.IsLetter).ToArray());
            return normalized == "fueradeservicio";
        }
    }

    /// <summary>
    /// Notifica que una propiedad ha cambiado
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

