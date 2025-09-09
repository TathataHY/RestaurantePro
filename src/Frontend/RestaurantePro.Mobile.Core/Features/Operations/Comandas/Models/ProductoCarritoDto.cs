using System.ComponentModel;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

public class ProductoCarritoDto : INotifyPropertyChanged
{
    private int _cantidad;
    private decimal _precio;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool EsNuevo { get; set; } = true;
    public Guid ItemId { get; set; }
    
    public decimal Precio
    {
        get => _precio;
        set
        {
            _precio = value;
            OnPropertyChanged(nameof(Precio));
            OnPropertyChanged(nameof(Subtotal));
        }
    }
    
    public int Cantidad
    {
        get => _cantidad;
        set
        {
            _cantidad = value;
            OnPropertyChanged(nameof(Cantidad));
            OnPropertyChanged(nameof(Subtotal));
        }
    }
    
    public decimal Subtotal => Precio * Cantidad;

    // Disponibilidad de preparaciones del día (sumatoria de CantidadDisponible)
    // -1 indica que aún no se consultó
    private int _preparacionesDisponiblesHoy = -1;
    public int PreparacionesDisponiblesHoy
    {
        get => _preparacionesDisponiblesHoy;
        set
        {
            _preparacionesDisponiblesHoy = value;
            OnPropertyChanged(nameof(PreparacionesDisponiblesHoy));
            OnPropertyChanged(nameof(TieneDatosPreparacion));
            OnPropertyChanged(nameof(CantidadTomadaDePreparaciones));
            OnPropertyChanged(nameof(CantidadAPreparar));
        }
    }

    public bool TieneDatosPreparacion => PreparacionesDisponiblesHoy >= 0;

    // Derivados para mostrar la "matemática" en la UI
    public int CantidadTomadaDePreparaciones =>
        PreparacionesDisponiblesHoy < 0 ? 0 : Math.Min(Cantidad, PreparacionesDisponiblesHoy);

    public int CantidadAPreparar =>
        PreparacionesDisponiblesHoy < 0 ? 0 : Math.Max(0, Cantidad - PreparacionesDisponiblesHoy);

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(Cantidad))
        {
            // Notificar derivados para refrescar indicadores en UI
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CantidadTomadaDePreparaciones)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CantidadAPreparar))); 
        }
    }
}
