using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Domain.Comercial.Clientes;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Comercial
{
    public class LoyaltyPointsExpirationJobTests : IntegrationTestBase
    {
        private readonly Mock<IEmailService> _mockEmailService;

        public LoyaltyPointsExpirationJobTests(DatabaseFixture fixture) : base(fixture)
        {
            _mockEmailService = new Mock<IEmailService>();
        }

        private LoyaltyPointsExpirationJob CreateJob(LoyaltyPointsExpirationOptions options)
        {
            var optionsWrapper = Options.Create(options);
            var logger = new Mock<ILogger<LoyaltyPointsExpirationJob>>().Object;
            
            // Resolvemos los repositorios y UoW del ServiceProvider de la clase base
            var tarjetaRepo = GetService<ITarjetaFidelizacionRepository>();
            var clienteRepo = GetService<IClienteRepository>();
            var unitOfWork = GetService<IUnitOfWork>();

            return new LoyaltyPointsExpirationJob(
                logger,
                tarjetaRepo,
                clienteRepo,
                _mockEmailService.Object,
                unitOfWork,
                optionsWrapper
            );
        }

        [Fact]
        public async Task ExecuteInternalAsync_CuandoHayPuntosPorExpirar_DebeEnviarAdvertencia()
        {
            // Arrange
            var cliente = Cliente.Crear("Juan", "Perez", "juan.perez@test.com", "123456789");
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "123-456");
            tarjeta.AcumularPuntos(100, "Compra inicial");
            tarjeta.EstablecerFechaExpiracionPuntos(DateTime.UtcNow.AddDays(5));
            
            await AddAsync(cliente);
            await AddAsync(tarjeta);

            var options = new LoyaltyPointsExpirationOptions
            {
                DaysBeforeExpirationForNotification = 7,
                AutomaticExpiration = false,
                SendExpirationWarnings = true
            };
            var job = CreateJob(options);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            _mockEmailService.Verify(
                s => s.SendEmailAsync(
                    cliente.Email, 
                    It.Is<string>(subj => subj.Contains("¡Tus puntos están a punto de expirar!")), 
                    It.IsAny<string>()),
                Times.Once);
                
            var tarjetaDb = await FirstOrDefaultAsync<TarjetaFidelizacion>(t => t.Id == tarjeta.Id);
            tarjetaDb.PuntosDisponibles.Should().Be(100); // Los puntos no deben expirar
        }

        [Fact]
        public async Task ExecuteInternalAsync_CuandoExpiracionEsAutomatica_DebeExpirarPuntosYNotificar()
        {
            // Arrange
            var cliente = Cliente.Crear("Ana", "Gomez", "ana.gomez@test.com", "987654321");
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "654-321");
            tarjeta.AcumularPuntos(200, "Compra grande");
            tarjeta.EstablecerFechaExpiracionPuntos(DateTime.UtcNow.AddDays(3));
            
            await AddAsync(cliente);
            await AddAsync(tarjeta);
            
            var options = new LoyaltyPointsExpirationOptions
            {
                DaysBeforeExpirationForNotification = 5,
                AutomaticExpiration = true,
                SendExpirationNotifications = true
            };
            var job = CreateJob(options);
            
            // Act
            await job.ExecuteAsync(CancellationToken.None);
            
            // Assert
            _mockEmailService.Verify(
                s => s.SendEmailAsync(
                    cliente.Email, 
                    It.Is<string>(subj => subj.Contains("Tus puntos de fidelización han expirado")), 
                    It.IsAny<string>()),
                Times.Once);

            var tarjetaDb = await FirstOrDefaultAsync<TarjetaFidelizacion>(t => t.Id == tarjeta.Id);
            tarjetaDb.PuntosDisponibles.Should().Be(0);
        }
        
        [Fact]
        public async Task ExecuteInternalAsync_CuandoNoHayTarjetas_NoDebeHacerNada()
        {
            // Arrange
            var options = new LoyaltyPointsExpirationOptions();
            var job = CreateJob(options);
            
            // Act
            await job.ExecuteAsync(CancellationToken.None);
            
            // Assert
            _mockEmailService.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
} 