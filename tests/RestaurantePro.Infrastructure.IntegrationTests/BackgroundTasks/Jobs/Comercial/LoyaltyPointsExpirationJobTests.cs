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
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Comercial
{
    [Collection("DatabaseCollection")]
    public class LoyaltyPointsExpirationJobTests : IntegrationTestBase
    {
        private readonly Mock<IEmailService> _mockEmailService;

        public LoyaltyPointsExpirationJobTests(DatabaseFixture fixture) : base(fixture)
        {
            _mockEmailService = new Mock<IEmailService>();
        }

        [Fact]
        public async Task ExecuteAsync_WithExpiredPoints_ShouldSetPointsToZeroAndNotify()
        {
            // Arrange
            var clienteRepo = ServiceProvider.GetRequiredService<IClienteRepository>();
            var tarjetaRepo = ServiceProvider.GetRequiredService<ITarjetaFidelizacionRepository>();
            var unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cliente = Cliente.Crear(
                Guid.NewGuid(),
                ClienteNombre.Crear("Integration", "Test"),
                Email.Create("test-expiracion@test.com"),
                PhoneNumber.Create("+15551234567"),
                DateTime.Now.AddYears(-25)
            );

            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "123-EXP-TEST");
            tarjeta.Activar();
            tarjeta.AgregarPuntos(200, "Carga inicial para test");
            
            var fechaExpiracionProperty = typeof(TarjetaFidelizacion).GetProperty("FechaExpiracionPuntos");
            if (fechaExpiracionProperty != null)
            {
                fechaExpiracionProperty.SetValue(tarjeta, DateTime.UtcNow);
            }

            await clienteRepo.AgregarAsync(cliente);
            await tarjetaRepo.AgregarAsync(tarjeta);
            await unitOfWork.SaveChangesAsync();

            // Act
            using (var scope = ServiceProvider.CreateScope())
            {
                var scopedOptions = new LoyaltyPointsExpirationOptions { AutomaticExpiration = true, SendExpirationNotifications = false };
                
                var job = new LoyaltyPointsExpirationJob(
                    scope.ServiceProvider.GetRequiredService<ILogger<LoyaltyPointsExpirationJob>>(),
                    scope.ServiceProvider.GetRequiredService<ITarjetaFidelizacionRepository>(),
                    scope.ServiceProvider.GetRequiredService<IClienteRepository>(),
                    _mockEmailService.Object,
                    scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
                    Options.Create(scopedOptions)
                );
                
                await job.ExecuteAsync(CancellationToken.None);
            }

            // Assert
            ClearTracker();
            
            var tarjetaDb = await ServiceProvider.GetRequiredService<ITarjetaFidelizacionRepository>()
                .ObtenerPorIdAsync(tarjeta.Id);

            tarjetaDb.Should().NotBeNull();
            tarjetaDb.PuntosDisponibles.Should().Be(0);

            _mockEmailService.Verify(
                s => s.SendEmailAsync(
                    "test-expiracion@test.com", // Usar el email correcto
                    It.Is<string>(subj => subj.Contains("Tus puntos de fidelización han expirado")),
                    It.IsAny<string>()
                ),
                Times.Once
            );
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