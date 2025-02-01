using System.Collections.Generic;

namespace RestaurantePro.Api.Models;

public class Plato
{
    public Plato()
    {
        ComandaDetalles = new HashSet<ComandaDetalle>();
    }

    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Disponible { get; set; }
    public string Categoria { get; set; }

    public ICollection<ComandaDetalle> ComandaDetalles { get; set; }
} 