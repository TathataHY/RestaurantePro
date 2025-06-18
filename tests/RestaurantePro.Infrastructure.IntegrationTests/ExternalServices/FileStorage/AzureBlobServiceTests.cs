using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RestaurantePro.Infrastructure.ExternalServices.FileStorage;
using System.Threading.Tasks;
using System;
using RestaurantePro.Application.Common.Interfaces;
using System.Text;

namespace RestaurantePro.Infrastructure.IntegrationTests.ExternalServices.FileStorage
{
    public class AzureBlobServiceTests
    {
        private readonly AzureBlobService _fileService;
        private readonly Mock<ILogger<AzureBlobService>> _mockLogger;
        private readonly IConfiguration _configuration;

        public AzureBlobServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"FileStorage:Azure:ConnectionString", "UseDevelopmentStorage=true"},
                {"FileStorage:Azure:ContainerName", "test-container"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _mockLogger = new Mock<ILogger<AzureBlobService>>();
            _fileService = new AzureBlobService(_configuration, _mockLogger.Object);
        }

        [Fact]
        public async Task SubirArchivoAsync_DebeDevolverUrlSimulada()
        {
            // Arrange
            var archivo = new DatosArchivo
            {
                NombreArchivo = "test.txt",
                Contenido = Encoding.UTF8.GetBytes("Hello Azure"),
                TipoMime = "text/plain"
            };
            var rutaSubida = "documentos-azure";

            // Act
            var result = await _fileService.SubirArchivoAsync(archivo, rutaSubida);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Contain(rutaSubida);
            result.Value.Should().Contain(archivo.NombreArchivo);
        }

        [Fact]
        public async Task DescargarArchivoAsync_DebeDevolverDatosSimulados()
        {
            // Arrange
            var url = "https://storage.azure.com/test-container/documentos-azure/test.txt";

            // Act
            var result = await _fileService.DescargarArchivoAsync(url);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.NombreArchivo.Should().Be("test.txt");
            result.Value.Contenido.Should().NotBeEmpty();
        }

        [Fact]
        public async Task EliminarArchivoAsync_DebeDevolverTrue()
        {
            // Arrange
            var url = "https://storage.azure.com/test-container/documentos-azure/test.txt";
            
            // Act
            var result = await _fileService.EliminarArchivoAsync(url);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
        
        [Fact]
        public async Task ObtenerUrlTemporalAsync_DebeDevolverUrlSimulada()
        {
            // Arrange
            var url = "https://storage.azure.com/test-container/documentos-azure/test.txt";
            
            // Act
            var result = await _fileService.ObtenerUrlTemporalAsync(url, TimeSpan.FromMinutes(5));

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNullOrEmpty();
            result.Value.Should().Contain(url);
        }
    }
} 