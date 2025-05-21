namespace RestaurantePro.Domain.Core.Base.Interfaces
{
     /// <summary>
    /// Interfaz de marcado para raíces de agregado
    /// 
    /// Esta interfaz no tiene métodos ni propiedades, solo sirve para identificar las entidades
    /// que son raíces de agregado en el dominio. Permite distinguir entre entidades regulares
    /// y entidades que son punto de entrada a un agregado completo.
    /// 
    /// Las raíces de agregado son las únicas entidades que pueden ser referenciadas desde
    /// fuera del agregado, y son responsables de mantener la consistencia del agregado.
    /// </summary>
    public interface IAggregateRoot
    {
        // Esta es una interfaz marcadora (marker interface)
        // No contiene miembros, solo sirve para identificar entidades que son raíz de un agregado
    }
}
