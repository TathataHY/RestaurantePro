namespace RestaurantePro.Domain.UnitTests.Core.Base
{
    // Evento de dominio de prueba - lo movemos fuera de la clase de tests para hacerlo público
    public class TestDomainEvent : DomainEvent
    {
        public Guid EntityId { get; }

        public TestDomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }
    }

    public class DomainEventDispatcherTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock;
        private readonly DomainEventDispatcher _dispatcher;

        public DomainEventDispatcherTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _eventRegistryMock = new Mock<IDomainEventRegistry>();
            _dispatcher = new DomainEventDispatcher(_serviceProviderMock.Object, _eventRegistryMock.Object);
        }

        [Fact]
        public async Task Dispatch_ConEventoValido_DebeInvocarHandlers()
        {
            // Arrange
            var evento = new TestDomainEvent(Guid.NewGuid());
            var handler1 = new Mock<IDomainEventHandler<TestDomainEvent>>();
            var handler2 = new Mock<IDomainEventHandler<TestDomainEvent>>();

            var handlers = new List<IDomainEventHandler<TestDomainEvent>> 
            { 
                handler1.Object,
                handler2.Object
            };

            // Configuración correcta para evitar el uso de métodos de extensión
            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
                .Returns(handlers);

            // Act
            await _dispatcher.Dispatch(evento);

            // Assert
            handler1.Verify(h => h.Handle(evento, It.IsAny<CancellationToken>()), Times.Once);
            handler2.Verify(h => h.Handle(evento, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Dispatch_SinHandlers_DebeRegistrarEnLog()
        {
            // Arrange
            var evento = new TestDomainEvent(Guid.NewGuid());
            
            // Simular que no hay handlers registrados
            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
                .Returns(Enumerable.Empty<IDomainEventHandler<TestDomainEvent>>());

            // Act
            await _dispatcher.Dispatch(evento);

            // Assert
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    evento, 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Dispatch_ConHandlerQueArrojaExcepcion_DebeRegistrarErrorYContinuar()
        {
            // Arrange
            var evento = new TestDomainEvent(Guid.NewGuid());
            
            var handler1 = new Mock<IDomainEventHandler<TestDomainEvent>>();
            var handler2 = new Mock<IDomainEventHandler<TestDomainEvent>>();

            // El primer handler arrojará una excepción
            handler1
                .Setup(h => h.Handle(evento, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error de prueba"));

            var handlers = new List<IDomainEventHandler<TestDomainEvent>> 
            { 
                handler1.Object,
                handler2.Object
            };

            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
                .Returns(handlers);

            // Act
            await _dispatcher.Dispatch(evento);

            // Assert
            handler1.Verify(h => h.Handle(evento, It.IsAny<CancellationToken>()), Times.Once);
            handler2.Verify(h => h.Handle(evento, It.IsAny<CancellationToken>()), Times.Once);
            
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    evento, 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DispatchAll_ConVariosEventos_DebeInvocarDispatchParaCadaUno()
        {
            // Arrange
            var evento1 = new TestDomainEvent(Guid.NewGuid());
            var evento2 = new TestDomainEvent(Guid.NewGuid());
            var eventos = new List<DomainEvent> { evento1, evento2 };

            var handler = new Mock<IDomainEventHandler<TestDomainEvent>>();
            var handlers = new List<IDomainEventHandler<TestDomainEvent>> { handler.Object };

            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
                .Returns(handlers);

            // Act
            await _dispatcher.DispatchAll(eventos);

            // Assert
            handler.Verify(h => h.Handle(evento1, It.IsAny<CancellationToken>()), Times.Once);
            handler.Verify(h => h.Handle(evento2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 