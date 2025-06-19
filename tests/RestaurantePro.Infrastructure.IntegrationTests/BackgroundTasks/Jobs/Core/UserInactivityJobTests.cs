using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Core
{
    public class UserInactivityJobTests : IntegrationTestBase, IAsyncLifetime
    {
        private Mock<IEmailService> _emailServiceMock;
        private IUsuarioRepository _usuarioRepository;
        private IUnitOfWork _unitOfWork;
        private ILogger<UserInactivityJob> _logger;
        
        private Usuario _usuarioActivo;
        private Usuario _usuarioInactivo;

        public UserInactivityJobTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            
            _emailServiceMock = new Mock<IEmailService>();
            _usuarioRepository = ServiceProvider.GetRequiredService<IUsuarioRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            _logger = ServiceProvider.GetRequiredService<ILogger<UserInactivityJob>>();

            await SeedDatabaseAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedDatabaseAsync()
        {
            _usuarioActivo = Usuario.Crear("activo", "Usuario Activo", "activo@test.com", RolUsuario.Mesero);
            _usuarioActivo.RegistrarAcceso(DateTime.UtcNow.AddDays(-5));
            _usuarioActivo.ConfirmarCuenta();

            _usuarioInactivo = Usuario.Crear("inactivo", "Usuario Inactivo", "inactivo@test.com", RolUsuario.Mesero);
            _usuarioInactivo.RegistrarAcceso(DateTime.UtcNow.AddDays(-40));
            _usuarioInactivo.ConfirmarCuenta();

            await _usuarioRepository.AgregarAsync(_usuarioActivo);
            await _usuarioRepository.AgregarAsync(_usuarioInactivo);
            await _unitOfWork.SaveChangesAsync();
        }

        [Fact]
        public async Task ExecuteInternalAsync_DebeEnviarCorreoSoloAUsuariosInactivos()
        {
            // Arrange
            var options = Options.Create(new UserInactivityOptions
            {
                InactivityThresholdDays = 30,
                SendReminderEmails = true
            });

            var job = new UserInactivityJob(
                _logger,
                _usuarioRepository,
                _emailServiceMock.Object,
                _unitOfWork,
                options
            );
            
            var jobCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

            // Act
            await job.ExecuteAsync(jobCancellationToken);

            // Assert
            _emailServiceMock.Verify(
                email => email.SendEmailAsync(
                    _usuarioInactivo.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once,
                "Se esperaba que se enviara un correo al usuario inactivo."
            );
            
            _emailServiceMock.Verify(
                email => email.SendEmailAsync(
                    _usuarioActivo.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never,
                "No se esperaba que se enviara un correo al usuario activo."
            );
        }

        [Fact]
        public async Task ExecuteInternalAsync_NoDebeEnviarCorreos_SiOpcionEstaDesactivada()
        {
            // Arrange
            var options = Options.Create(new UserInactivityOptions
            {
                InactivityThresholdDays = 30,
                SendReminderEmails = false
            });

            var job = new UserInactivityJob(
                _logger,
                _usuarioRepository,
                _emailServiceMock.Object,
                _unitOfWork,
                options
            );
            
            var jobCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

            // Act
            await job.ExecuteAsync(jobCancellationToken);

            // Assert
            _emailServiceMock.Verify(
                email => email.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never,
                "No se debería enviar ningún correo si la opción está desactivada."
            );
        }

        [Fact]
        public async Task ExecuteInternalAsync_NoDebeEnviarCorreo_AUsuarioInactivoDesactivado()
        {
            // Arrange
            _usuarioInactivo.Desactivar();
            await _unitOfWork.SaveChangesAsync();
            
            var options = Options.Create(new UserInactivityOptions
            {
                InactivityThresholdDays = 30,
                SendReminderEmails = true
            });

            var job = new UserInactivityJob(
                _logger,
                _usuarioRepository,
                _emailServiceMock.Object,
                _unitOfWork,
                options
            );
            
            var jobCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

            // Act
            await job.ExecuteAsync(jobCancellationToken);

            // Assert
            _emailServiceMock.Verify(
                email => email.SendEmailAsync(
                    _usuarioInactivo.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never,
                "No se debería enviar correo a un usuario que ya está desactivado."
            );
        }
    }
} 