using RestaurantePro.Domain.Core.Base.Events;

namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente;

public class IngredienteActualizado : DomainEvent
{
    public Guid IngredienteId { get; }
    public string Nombre { get; }

    public IngredienteActualizado(Guid ingredienteId, string nombre)
    {
        IngredienteId = ingredienteId;
        Nombre = nombre;
    }
} 