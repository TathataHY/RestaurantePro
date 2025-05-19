namespace RestaurantePro.Domain.UnitTests.Core.Base.Events.Subscription
{
    public class TestEvent : DomainEvent
    {
        public string TestMessage { get; }
        
        public TestEvent(Guid entityId, string testMessage) : base()
        {
            EntityId = entityId;
            TestMessage = testMessage;
            BoundedContext = "Test";
        }
    }
    
    public class AnotherTestEvent : DomainEvent
    {
        public int Value { get; }
        
        public AnotherTestEvent(Guid entityId, int value) : base()
        {
            EntityId = entityId;
            Value = value;
            BoundedContext = "AnotherTest";
        }
    }
    
    public class EventSubscriptionManagerTests
    {
        [Fact]
        public async Task Subscribe_WhenEventMatches_ShouldNotifySubscriber()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var testEntityId = Guid.NewGuid();
            var testEvent = new TestEvent(testEntityId, "Test message");
            var handlerCalled = false;
            
            // Act
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                handlerCalled = true;
                Assert.Equal("Test message", ((TestEvent)evento).TestMessage);
                await Task.CompletedTask;
            });
            
            await subscriptionManager.NotifySubscribersAsync(testEvent);
            
            // Assert
            Assert.True(handlerCalled, "El manejador no fue llamado");
        }
        
        [Fact]
        public async Task Subscribe_WhenEventDoesNotMatch_ShouldNotNotifySubscriber()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var testEntityId = Guid.NewGuid();
            var testEvent = new AnotherTestEvent(testEntityId, 42);
            var handlerCalled = false;
            
            // Act
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                handlerCalled = true;
                await Task.CompletedTask;
            });
            
            await subscriptionManager.NotifySubscribersAsync(testEvent);
            
            // Assert
            Assert.False(handlerCalled, "El manejador no debería haber sido llamado");
        }
        
        [Fact]
        public async Task Subscribe_WithEntityFilter_ShouldOnlyNotifyForMatchingEntity()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var entity1 = Guid.NewGuid();
            var entity2 = Guid.NewGuid();
            var event1 = new TestEvent(entity1, "Entity 1");
            var event2 = new TestEvent(entity2, "Entity 2");
            var messagesReceived = new List<string>();
            
            // Act
            subscriptionManager.Subscribe<TestEvent>(entity1, async (evento, ct) => 
            {
                messagesReceived.Add(((TestEvent)evento).TestMessage);
                await Task.CompletedTask;
            });
            
            await subscriptionManager.NotifySubscribersAsync(event1); // Debería recibirse
            await subscriptionManager.NotifySubscribersAsync(event2); // No debería recibirse
            
            // Assert
            Assert.Single(messagesReceived);
            Assert.Equal("Entity 1", messagesReceived[0]);
        }
        
        [Fact]
        public async Task Subscribe_WithBoundedContextFilter_ShouldOnlyNotifyForMatchingContext()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var entity = Guid.NewGuid();
            var event1 = new TestEvent(entity, "Test message");
            var event2 = new AnotherTestEvent(entity, 42);
            var messagesReceived = new List<string>();
            
            // Act
            subscriptionManager.Subscribe<DomainEvent>("Test", async (evento, ct) => 
            {
                messagesReceived.Add(evento.GetType().Name);
                await Task.CompletedTask;
            });
            
            await subscriptionManager.NotifySubscribersAsync(event1); // Debería recibirse
            await subscriptionManager.NotifySubscribersAsync(event2); // No debería recibirse
            
            // Assert
            Assert.Single(messagesReceived);
            Assert.Equal(nameof(TestEvent), messagesReceived[0]);
        }
        
        [Fact]
        public async Task Unsubscribe_ShouldRemoveSubscription()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var testEntityId = Guid.NewGuid();
            var testEvent = new TestEvent(testEntityId, "Test message");
            var handlerCalled = false;
            
            // Act
            var subscriptionId = subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                handlerCalled = true;
                await Task.CompletedTask;
            });
            
            var unsubscribeResult = subscriptionManager.Unsubscribe(subscriptionId);
            await subscriptionManager.NotifySubscribersAsync(testEvent);
            
            // Assert
            Assert.True(unsubscribeResult, "La suscripción no se canceló correctamente");
            Assert.False(handlerCalled, "El manejador no debería haber sido llamado después de cancelar la suscripción");
        }
        
        [Fact]
        public async Task NotifySubscribersAsync_WithMultipleSubscribers_ShouldNotifyAll()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var testEntityId = Guid.NewGuid();
            var testEvent = new TestEvent(testEntityId, "Test message");
            var handler1Called = false;
            var handler2Called = false;
            
            // Act
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                handler1Called = true;
                await Task.CompletedTask;
            });
            
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                handler2Called = true;
                await Task.CompletedTask;
            });
            
            await subscriptionManager.NotifySubscribersAsync(testEvent);
            
            // Assert
            Assert.True(handler1Called, "El primer manejador no fue llamado");
            Assert.True(handler2Called, "El segundo manejador no fue llamado");
        }
        
        [Fact]
        public async Task NotifySubscribersAsync_WithExceptionInHandler_ShouldContinueWithOtherHandlers()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var testEntityId = Guid.NewGuid();
            var testEvent = new TestEvent(testEntityId, "Test message");
            var secondHandlerCalled = false;
            
            // Act
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                await Task.CompletedTask;
                throw new Exception("Error simulado en manejador");
            });
            
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                secondHandlerCalled = true;
                await Task.CompletedTask;
            });
            
            // No debería lanzar excepción
            await subscriptionManager.NotifySubscribersAsync(testEvent);
            
            // Assert
            Assert.True(secondHandlerCalled, "El segundo manejador debería haber sido llamado a pesar del error en el primero");
        }
        
        [Fact]
        public async Task NotifySubscribersAsync_WithMultipleEvents_ShouldNotifyForEachEvent()
        {
            // Arrange
            var subscriptionManager = new EventSubscriptionManager();
            var entity = Guid.NewGuid();
            var event1 = new TestEvent(entity, "Event 1");
            var event2 = new TestEvent(entity, "Event 2");
            var messagesReceived = new List<string>();
            
            // Act
            subscriptionManager.Subscribe<TestEvent>(async (evento, ct) => 
            {
                messagesReceived.Add(((TestEvent)evento).TestMessage);
                await Task.CompletedTask;
            });
            
            await subscriptionManager.NotifySubscribersAsync(new List<DomainEvent> { event1, event2 });
            
            // Assert
            Assert.Equal(2, messagesReceived.Count);
            Assert.Contains("Event 1", messagesReceived);
            Assert.Contains("Event 2", messagesReceived);
        }
    }
} 