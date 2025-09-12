namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Enumeración para estados de reservaciones
/// </summary>
public enum EstadoReservacion
{
    Pendiente = 1,
    Confirmada = 2,
    EnProceso = 3,
    Completada = 4,
    Cancelada = 5,
    NoShow = 6,
    Reagendada = 7
}
