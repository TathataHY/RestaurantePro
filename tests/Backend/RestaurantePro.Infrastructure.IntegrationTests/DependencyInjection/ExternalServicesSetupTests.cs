using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Infrastructure.ExternalServices.Email;
using RestaurantePro.Infrastructure.ExternalServices.FileStorage;
using RestaurantePro.Infrastructure.ExternalServices.Payment;
using RestaurantePro.Infrastructure.ExternalServices.SMS;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using RestaurantePro.Infrastructure.ExternalServices.Email.Models;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class ExternalServicesSetupTests
    {
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        private void Setup(Dictionary<string, string> config = null)
        {
            var services = new ServiceCollection();
            
            var baseConfig = new Dictionary<string, string>
            {
                { "Email:ServidorSMTP", "localhost" },
                { "Email:PuertoSMTP", "25" },
                { "Email:Usuario", "test" },
                { "Email:Password", "test" },
                { "Email:DireccionRemitente", "test@test.com" },
                { "Email:NombreRemitente", "Test" }
            };

            if (config != null)
            {
                foreach (var entry in config)
                {
                    baseConfig[entry.Key] = entry.Value;
                }
            }

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(baseConfig)
                .Build();

            services.AddSingleton(_configuration);
            services.AddLogging();
            
            // Mock de dependencias necesarias por los servicios
            services.AddSingleton(Substitute.For<IDelayProvider>());
            services.Configure<EmailSettings>(_configuration.GetSection("Email"));

            services.AddExternalServices(_configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddExternalServices_DefaultConfiguration_ShouldRegisterDefaultServices()
        {
            // Arrange
            Setup();

            // Act
            var emailService = _serviceProvider.GetService<IEmailService>();
            var fileStorageService = _serviceProvider.GetService<IFileStorageService>();
            var paymentService = _serviceProvider.GetService<IPaymentService>();
            var smsService = _serviceProvider.GetService<ISMSService>();

            // Assert
            Assert.NotNull(emailService);
            Assert.IsType<EmailService>(emailService);

            Assert.NotNull(fileStorageService);
            Assert.IsType<LocalFileService>(fileStorageService);

            Assert.NotNull(paymentService);
            Assert.IsType<StripeService>(paymentService);

            Assert.NotNull(smsService);
            Assert.IsType<TwilioService>(smsService);
        }

        [Fact]
        public void AddExternalServices_WithAzureProvider_ShouldRegisterAzureBlobService()
        {
            // Arrange
            var config = new Dictionary<string, string>
            {
                { "FileStorage:Provider", "azure" },
                { "FileStorage:Azure:ConnectionString", "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;EndpointSuffix=core.windows.net" }
            };
            Setup(config);

            // Act
            var fileStorageService = _serviceProvider.GetService<IFileStorageService>();

            // Assert
            Assert.NotNull(fileStorageService);
            Assert.IsType<AzureBlobService>(fileStorageService);
        }

        [Fact]
        public void AddExternalServices_WithPayPalProvider_ShouldRegisterPayPalService()
        {
            // Arrange
            var config = new Dictionary<string, string>
            {
                { "Payment:Provider", "paypal" }
            };
            Setup(config);

            // Act
            var paymentService = _serviceProvider.GetService<IPaymentService>();

            // Assert
            Assert.NotNull(paymentService);
            Assert.IsType<PayPalService>(paymentService);
        }
    }
} 