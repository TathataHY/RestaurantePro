namespace RestaurantePro.Api.Models;

public class Mesa
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public EstadoMesa Estado { get; set; }
}


public enum EstadoMesa
{
    Disponible,
    Ocupada,
    Reservada
} 