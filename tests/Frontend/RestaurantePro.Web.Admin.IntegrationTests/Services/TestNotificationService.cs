using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Implementación de prueba para INotificationService
/// </summary>
public class TestNotificationService : INotificationService
{
    public Task<bool> EnviarNotificacionAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Info")
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<bool> EnviarNotificacionMasivaAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo = "Info")
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<bool> EnviarNotificacionPushAsync(Guid usuarioId, string titulo, string mensaje)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<bool> MarcarComoLeidaAsync(Guid notificacionId, Guid usuarioId)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }

    public Task<int> ObtenerNotificacionesNoLeidasAsync(Guid usuarioId)
    {
        // Implementación de prueba - devuelve 0
        return Task.FromResult(0);
    }

    public Task<bool> SendNotificationAsync(Notification notification)
    {
        // Implementación de prueba - siempre devuelve true
        return Task.FromResult(true);
    }
}
