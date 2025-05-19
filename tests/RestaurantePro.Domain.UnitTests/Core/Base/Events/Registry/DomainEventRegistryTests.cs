using RestaurantePro.Domain.Core.Base.Events.Registry;

namespace RestaurantePro.Domain.UnitTests.Core.Base.Events.Registry
{
    public class DomainEventRegistryTests
    {
        [Fact]
        public async Task RegisterAsync_DebeGuardarEvento()
        {
            // Arrange
            var registro = new InMemoryDomainEventRegistry();
            var evento = new EventoPrueba { ProductoId = Guid.NewGuid() };

            // Act
            await registro.RegisterAsync(evento);

            // Assert
            var eventos = await registro.GetEventsByTypeAsync<EventoPrueba>();
            
            eventos.Should().HaveCount(1);
            eventos.First().Should().BeEquivalentTo(evento);
        }

        [Fact]
        public async Task RegisterAllAsync_DebeGuardarMultiplesEventos()
        {
            // Arrange
            var registro = new InMemoryDomainEventRegistry();
            var eventos = new List<DomainEvent>
            {
                new EventoPrueba { ProductoId = Guid.NewGuid() },
                new EventoPrueba { ProductoId = Guid.NewGuid() }
            };

            // Act
            await registro.RegisterAllAsync(eventos);

            // Assert
            var eventosGuardados = await registro.GetEventsByTypeAsync<EventoPrueba>();
            
            eventosGuardados.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetEventsForEntityAsync_DebeRetornarEventosDeUnaEntidad()
        {
            // Arrange
            var registro = new InMemoryDomainEventRegistry();
            var entityId = Guid.NewGuid();
            
            await registro.RegisterAsync(new EventoPrueba { ProductoId = entityId });
            await registro.RegisterAsync(new EventoPrueba { ProductoId = Guid.NewGuid() });
            await registro.RegisterAsync(new EventoPrueba { ProductoId = entityId });

            // Act
            var eventos = await registro.GetEventsForEntityAsync(entityId);

            // Assert
            eventos.Should().HaveCount(2);
            eventos.Should().AllBeOfType<EventoPrueba>();
        }

        [Fact]
        public async Task GetEventsByTypeAsync_DebeRetornarEventosDeUnTipo()
        {
            // Arrange
            var registro = new InMemoryDomainEventRegistry();
            var ahora = DateTime.UtcNow;
            
            var evento1 = new EventoPrueba { ProductoId = Guid.NewGuid() };
            var evento2 = new EventoPrueba { ProductoId = Guid.NewGuid() };
            
            await registro.RegisterAsync(evento1);
            await registro.RegisterAsync(evento2);

            // Act
            var eventos = await registro.GetEventsByTypeAsync<EventoPrueba>();

            // Assert
            eventos.Should().HaveCount(2);
            eventos.Should().Contain(evento1);
            eventos.Should().Contain(evento2);
        }

        [Fact]
        public async Task GetEventsByTypeAsync_ConFiltroFechas_DebeRetornarEventosFiltrados()
        {
            // Arrange - Este test es más conceptual ya que no podemos controlar directamente el timestamp en InMemoryDomainEventRegistry
            var registro = new InMemoryDomainEventRegistry();
            await registro.RegisterAsync(new EventoPrueba { ProductoId = Guid.NewGuid() });
            
            // Esperamos un poco para tener una diferencia de tiempo
            await Task.Delay(10);
            var fechaIntermedia = DateTime.UtcNow;
            await Task.Delay(10);
            
            await registro.RegisterAsync(new EventoPrueba { ProductoId = Guid.NewGuid() });

            // Act - Obtenemos solo eventos después de la fecha intermedia
            var eventosFiltrados = await registro.GetEventsByTypeAsync<EventoPrueba>(fechaIntermedia);

            // Assert
            eventosFiltrados.Should().HaveCount(1);
        }

        [Fact]
        public void Clear_DebeLimpiarTodosLosEventos()
        {
            // Arrange
            var registro = new InMemoryDomainEventRegistry();
            registro.RegisterAsync(new EventoPrueba { ProductoId = Guid.NewGuid() }).Wait();
            registro.RegisterAsync(new EventoPrueba { ProductoId = Guid.NewGuid() }).Wait();

            // Act
            registro.Clear();
            var eventos = registro.GetEventsByTypeAsync<EventoPrueba>().Result;

            // Assert
            eventos.Should().BeEmpty();
        }

        // Clase de evento para pruebas
        private class EventoPrueba : DomainEvent
        {
            public Guid ProductoId { get; set; }
        }
    }
} 