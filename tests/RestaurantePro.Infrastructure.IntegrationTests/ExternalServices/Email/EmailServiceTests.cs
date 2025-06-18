using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.ExternalServices.Email;
using RestaurantePro.Infrastructure.ExternalServices.Email.Models;
using System.Threading.Tasks;
using FluentAssertions;
using System.IO;
using System;
using System.Linq;

namespace RestaurantePro.Infrastructure.IntegrationTests.ExternalServices.Email
{
    public class EmailServiceTests : IDisposable
    {
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly string _pickupDirectory;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _pickupDirectory = Path.Combine(Path.GetTempPath(), "restaurante-pro-emails", Guid.NewGuid().ToString());

            if (!Directory.Exists(_pickupDirectory))
            {
                Directory.CreateDirectory(_pickupDirectory);
            }

            _emailSettings = Options.Create(new EmailSettings
            {
                ServidorSMTP = "localhost", // No se usará, pero es requerido
                PuertoSMTP = 25,            // No se usará
                UsarSSL = false,
                Usuario = "testuser",
                Password = "testpassword",
                DireccionRemitente = "no-reply@test.com",
                NombreRemitente = "Sistema RestaurantePro",
                PickupDirectory = _pickupDirectory // Directorio para guardar los correos
            });
        }

        [Fact]
        public async Task SendEmailAsync_DebeCrearArchivoEml_ConContenidoCorrecto()
        {
            // Arrange
            // Sobrescribir SmtpClient para usar PickupDirectory en lugar de Network
            var emailSettingsWithPickup = _emailSettings.Value;
            var service = new EmailServiceForTest(Options.Create(emailSettingsWithPickup), _mockLogger.Object);
            
            var to = "destinatario@test.com";
            var subject = "Asunto de prueba";
            var body = "Este es el cuerpo del correo de prueba.";

            // Act
            var result = await service.SendEmailAsync(to, subject, body);

            // Assert
            result.Should().BeTrue();

            var emailFile = Directory.GetFiles(_pickupDirectory).FirstOrDefault();
            emailFile.Should().NotBeNull();

            var emailContent = await File.ReadAllTextAsync(emailFile);
            emailContent.Should().Contain($"To: {to}");
            emailContent.Should().Contain($"From: \"{_emailSettings.Value.NombreRemitente}\" <{_emailSettings.Value.DireccionRemitente}>");
            emailContent.Should().Contain($"Subject: {subject}");
            emailContent.Should().Contain(body);
        }

        public void Dispose()
        {
            if (Directory.Exists(_pickupDirectory))
            {
                Directory.Delete(_pickupDirectory, true);
            }
            GC.SuppressFinalize(this);
        }
    }

    // Clase de ayuda para poder sobreescribir el método de creación de SmtpClient
    public class EmailServiceForTest : EmailService
    {
        private readonly string _pickupDirectory;

        public EmailServiceForTest(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger) 
            : base(emailSettings, logger)
        {
            _pickupDirectory = emailSettings.Value.PickupDirectory ?? Path.GetTempPath();
        }

        protected override System.Net.Mail.SmtpClient CrearClienteSMTP()
        {
            return new System.Net.Mail.SmtpClient
            {
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = _pickupDirectory
            };
        }
    }
} 