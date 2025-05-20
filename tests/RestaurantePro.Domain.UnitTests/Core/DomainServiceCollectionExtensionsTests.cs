namespace RestaurantePro.Domain.UnitTests.Core
{
    // Evento de prueba para el test de registro de manejadores
    public class TestEvent : DomainEvent
    {
        public string Message { get; set; }
    }
    
    // Manejador de eventos de prueba para el test de registro de manejadores
    public class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public Task Handle(TestEvent evento, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
    
    public class DomainServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDomainServices_DebeRegistrarServiciosObligatorios()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainServices();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDateTimeService));
            VerificarServiciosRegistrados(services, typeof(IDomainEventDispatcher));
            VerificarServiciosRegistrados(services, typeof(IDomainEventRegistry));
            VerificarServiciosRegistrados(services, typeof(IEventSubscriptionManager));
            VerificarServiciosRegistrados(services, typeof(IClientesFrecuentesPolicy));
            VerificarServiciosRegistrados(services, typeof(IStockBajoPolicy));
            VerificarServiciosRegistrados(services, typeof(IProductoRecomendadoPolicy));
            VerificarServiciosRegistrados(services, typeof(IVisibilidadCategoriasPolicy));
        }
        
        [Fact]
        public void AddDomainServicesForTests_DebeRegistrarServiciosDePrueba()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainServicesForTests();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDateTimeService));
            VerificarServiciosRegistrados(services, typeof(IDomainEventDispatcher));
            
            // Verificar que el IDateTimeService sea una instancia de MockDateTimeService
            var serviceProvider = services.BuildServiceProvider();
            var dateTimeService = serviceProvider.GetService<IDateTimeService>();
            dateTimeService.Should().BeOfType<MockDateTimeService>();
        }
        
        [Fact]
        public void AddDomainEventServices_DebeRegistrarServiciosBasicos()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainEventServices();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDomainEventDispatcher));
        }
        
        [Fact]
        public void AddDomainEventServicesWithRegistry_DebeRegistrarServiciosConRegistro()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainEventServicesWithRegistry();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDomainEventDispatcher));
            VerificarServiciosRegistrados(services, typeof(IDomainEventRegistry));
        }
        
        [Fact]
        public void AddInMemoryDomainEventRegistry_DebeRegistrarRegistroEnMemoria()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddInMemoryDomainEventRegistry();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDomainEventRegistry));
            
            // Verificar que sea InMemoryDomainEventRegistry
            var descriptor = services.First(d => d.ServiceType == typeof(IDomainEventRegistry));
            descriptor.ImplementationType.Should().Be(typeof(RestaurantePro.Domain.Core.Base.Events.Registry.InMemoryDomainEventRegistry));
            descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
        
        [Fact]
        public void AddDomainEventServicesWithSubscriptions_DebeRegistrarServiciosConSuscripciones()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainEventServicesWithSubscriptions();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDomainEventDispatcher));
            VerificarServiciosRegistrados(services, typeof(IEventSubscriptionManager));
            
            // Verificar que IEventSubscriptionManager sea Singleton
            var descriptor = services.First(d => d.ServiceType == typeof(IEventSubscriptionManager));
            descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
        
        [Fact]
        public void AddDomainEventServicesComplete_DebeRegistrarTodosLosServicios()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainEventServicesComplete();
            
            // Assert
            VerificarServiciosRegistrados(services, typeof(IDomainEventDispatcher));
            VerificarServiciosRegistrados(services, typeof(IDomainEventRegistry));
            VerificarServiciosRegistrados(services, typeof(IEventSubscriptionManager));
        }
        
        [Fact]
        public void AddAllDomainEventHandlers_DebeRegistrarHandlersDeEnsamblado()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddAllDomainEventHandlers(typeof(TestEventHandler).Assembly);
            
            // Assert
            // Obtenemos explícitamente el descriptor para nuestro manejador de prueba
            var descriptor = services.FirstOrDefault(d => 
                d.ServiceType.IsGenericType && 
                d.ServiceType.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>) &&
                d.ServiceType.GetGenericArguments()[0] == typeof(TestEvent) &&
                d.ImplementationType == typeof(TestEventHandler));
                
            // Verificamos que se haya registrado correctamente
            descriptor.Should().NotBeNull("porque TestEventHandler debería haberse registrado como IDomainEventHandler<TestEvent>");
            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }
        
        private void VerificarServiciosRegistrados(IServiceCollection services, Type serviceType)
        {
            services.Any(s => s.ServiceType == serviceType).Should().BeTrue(
                $"El servicio {serviceType.Name} debería estar registrado");
        }
    }
} 