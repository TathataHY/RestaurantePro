using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Comercial
{
    public class LoyaltyPointsExpirationJobTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly TestDbContext _dbContext;
        private readonly Mock<IEmailService> _mockEmailService;

        public LoyaltyPointsExpirationJobTests()
        {
            _mockEmailService = new Mock<IEmailService>();

            var services = new ServiceCollection();

            // Configurar EF Core In-Memory Database
            var dbName = Guid.NewGuid().ToString();
            var inMemoryOptions = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            services.AddSingleton(inMemoryOptions);

            services.AddDbContext<RestauranteProDbContext>();
            services.AddDbContext<TestDbContext>();

            services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDbContext>());

            // Registrar Repositorios y UnitOfWork
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar Mocks y Servicios
            services.AddSingleton(_mockEmailService.Object);
            services.AddSingleton<ILogger<LoyaltyPointsExpirationJob>>(new Mock<ILogger<LoyaltyPointsExpirationJob>>().Object);
            services.AddSingleton<ILogger<TarjetaFidelizacionRepository>>(new Mock<ILogger<TarjetaFidelizacionRepository>>().Object);
            services.AddSingleton<ILogger<ClienteRepository>>(new Mock<ILogger<ClienteRepository>>().Object);
            services.AddSingleton<ILogger<UnitOfWork>>(new Mock<ILogger<UnitOfWork>>().Object);
            services.AddSingleton<ILogger<RestauranteProDbContext>>(new Mock<ILogger<RestauranteProDbContext>>().Object);
            services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();


            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<TestDbContext>();
        }
        
        private LoyaltyPointsExpirationJob CreateJob(LoyaltyPointsExpirationOptions options)
        {
            var scope = _serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;

            return new LoyaltyPointsExpirationJob(
                sp.GetRequiredService<ILogger<LoyaltyPointsExpirationJob>>(),
                sp.GetRequiredService<ITarjetaFidelizacionRepository>(),
                sp.GetRequiredService<IClienteRepository>(),
                _mockEmailService.Object,
                sp.GetRequiredService<IUnitOfWork>(),
                Options.Create(options)
            );
        }

        [Fact]
        public async Task ExecuteAsync_WithExpiredPoints_ShouldSetPointsToZeroAndNotify()
        {
            // Arrange
            var clienteRepo = _serviceProvider.GetRequiredService<IClienteRepository>();
            var tarjetaRepo = _serviceProvider.GetRequiredService<ITarjetaFidelizacionRepository>();
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            var email = "test-expiracion@test.com";
            var cliente = Cliente.Crear(
                Guid.NewGuid(), 
                ClienteNombre.Crear("Integration", "Test"),
                Email.Create(email), 
                PhoneNumber.Create("+15551234567"), // Número de teléfono válido
                DateTime.Now.AddYears(-25)
            );
            
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "123-EXP");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(200, "Test");
            
            // MODIFICACIÓN: Usar reflexión para establecer fecha de expiración para la prueba
            tarjeta.GetType().GetProperty("FechaExpiracion").SetValue(tarjeta, DateTime.UtcNow.AddDays(-1), null);

            await clienteRepo.AgregarAsync(cliente);
            await tarjetaRepo.AgregarAsync(tarjeta);
            await _dbContext.SaveChangesAsync();

            var options = new LoyaltyPointsExpirationOptions
            {
                AutomaticExpiration = true,
                SendExpirationNotifications = true,
                DaysBeforeExpirationForNotification = 5
            };
            var job = CreateJob(options);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var tarjetaDb = await tarjetaRepo.ObtenerPorIdAsync(tarjeta.Id);
            tarjetaDb.PuntosDisponibles.Should().Be(0);

            _mockEmailService.Verify(
                s => s.SendEmailAsync(
                    email, // Usar el email correcto
                    It.Is<string>(subj => subj.Contains("Tus puntos de fidelización han expirado")),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _serviceProvider?.Dispose();
        }
    }

    public class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
        {
            return Task.CompletedTask;
        }

        public Task Dispatch(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DispatchAll(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
} 