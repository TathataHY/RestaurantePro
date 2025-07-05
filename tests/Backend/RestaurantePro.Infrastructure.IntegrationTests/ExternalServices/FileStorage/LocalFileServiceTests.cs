using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RestaurantePro.Infrastructure.ExternalServices.FileStorage;
using System.Threading.Tasks;
using System.IO;
using System;
using RestaurantePro.Application.Common.Interfaces;
using System.Text;

namespace RestaurantePro.Infrastructure.IntegrationTests.ExternalServices.FileStorage
{
    public class LocalFileServiceTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly LocalFileService _fileService;
        private readonly Mock<ILogger<LocalFileService>> _mockLogger;
        private readonly IConfiguration _configuration;

        public LocalFileServiceTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "RestauranteProTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            var inMemorySettings = new Dictionary<string, string> {
                {"FileStorage:LocalPath", _testDirectory},
                {"FileStorage:BaseUrl", "/test-files"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _mockLogger = new Mock<ILogger<LocalFileService>>();
            _fileService = new LocalFileService(_configuration, _mockLogger.Object);
        }

        [Fact]
        public async Task SubirArchivoAsync_DebeGuardarArchivoYDevolverUrlCorrecta()
        {
            // Arrange
            var archivo = new DatosArchivo
            {
                NombreArchivo = "test.txt",
                Contenido = Encoding.UTF8.GetBytes("Hello World"),
                TipoMime = "text/plain"
            };
            var rutaSubida = "documentos/usuarios";

            // Act
            var result = await _fileService.SubirArchivoAsync(archivo, rutaSubida);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNullOrEmpty();
            result.Value.Should().Contain(rutaSubida);
            result.Value.Should().Contain(archivo.NombreArchivo);

            var rutaFisica = Path.Combine(_testDirectory, rutaSubida, archivo.NombreArchivo);
            File.Exists(rutaFisica).Should().BeTrue();
            var content = await File.ReadAllTextAsync(rutaFisica);
            content.Should().Be("Hello World");
        }

        [Fact]
        public async Task DescargarArchivoAsync_CuandoArchivoExiste_DebeDevolverDatosCorrectos()
        {
            // Arrange
            var rutaSubida = "imagenes/productos";
            var nombreArchivo = "producto.jpg";
            var contenidoArchivo = new byte[] { 1, 2, 3 };
            var rutaFisicaDirectorio = Path.Combine(_testDirectory, rutaSubida);
            Directory.CreateDirectory(rutaFisicaDirectorio);
            var rutaFisicaArchivo = Path.Combine(rutaFisicaDirectorio, nombreArchivo);
            await File.WriteAllBytesAsync(rutaFisicaArchivo, contenidoArchivo);
            
            var url = $"/test-files/{rutaSubida}/{nombreArchivo}";

            // Act
            var result = await _fileService.DescargarArchivoAsync(url);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Contenido.Should().BeEquivalentTo(contenidoArchivo);
            result.Value.NombreArchivo.Should().Be(nombreArchivo);
            result.Value.TipoMime.Should().Be("image/jpeg");
        }
        
        [Fact]
        public async Task DescargarArchivoAsync_CuandoArchivoNoExiste_DebeFallar()
        {
            // Arrange
            var url = "/test-files/documentos/inexistente.pdf";

            // Act
            var result = await _fileService.DescargarArchivoAsync(url);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task EliminarArchivoAsync_CuandoArchivoExiste_DebeEliminarloYDevolverTrue()
        {
            // Arrange
            var rutaFisica = Path.Combine(_testDirectory, "a_eliminar.txt");
            await File.WriteAllTextAsync(rutaFisica, "temp");
            var url = "/test-files/a_eliminar.txt";

            // Act
            var result = await _fileService.EliminarArchivoAsync(url);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            File.Exists(rutaFisica).Should().BeFalse();
        }
        
        [Fact]
        public async Task EliminarArchivoAsync_CuandoArchivoNoExiste_DebeDevolverFalse()
        {
            // Arrange
            var url = "/test-files/no_existe.txt";

            // Act
            var result = await _fileService.EliminarArchivoAsync(url);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
    }
} 