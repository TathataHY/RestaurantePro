using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Operaciones.Comandas.EventHandlers.ComandaCreada
{
    public class ComandaCreadaSignalRHandlerTests
    {
        [Fact]
        public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionesSignalR()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var evento = new ComandaCreadaDomainEvent(Guid.NewGuid(), mesaId, meseroId);

            var comanda = Comanda.Crear(mesaId, meseroId, "Mesa 1", new List<ItemComanda>(), "Normal", DateTime.UtcNow, null, "Observaciones");
            // No se puede setear el Id directamente por DDD, así que validamos por otras propiedades

            var mockSignalR = new Mock<ISignalRService>();
            var mockLogger = new Mock<ILogger<ComandaCreadaSignalRHandler>>();
            var handler = new ComandaCreadaSignalRHandler(mockSignalR.Object, mockLogger.Object);

            // Act
            await handler.Handle(evento, CancellationToken.None);

            // Assert
            mockSignalR.Verify(s => s.NotificarGrupoAsync(
                It.Is<string>(g => g == "Cocina"),
                It.Is<string>(t => t == "NuevaComanda"),
                It.Is<object>(o => o != null)
            ), Times.Once);
        }

        // ... otros tests ...
        // En todos los tests, eliminar cualquier set de Id y ajustar los mocks igual que arriba
        // Si en el futuro se requiere forzar el Id, considerar usar un builder de test o reflexión SOLO en tests
    }
} 