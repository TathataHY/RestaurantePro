namespace RestaurantePro.Core.Enums
{
    public enum EstadoComanda
    {
        Pendiente,      // Recién creada
        EnPreparacion,  // Cocinero la está preparando
        Lista,          // Lista para entregar
        Entregada,      // Entregada al cliente
        Cancelada,      // Pagada y finalizada
        Anulada         // Cancelada sin pago
    }
}