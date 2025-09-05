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

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
