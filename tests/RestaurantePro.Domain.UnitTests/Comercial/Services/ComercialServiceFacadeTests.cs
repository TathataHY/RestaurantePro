using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;

namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    /// <summary>
    /// Clases de datos necesarias para las pruebas
    /// </summary>
    public record DatosFacturacion(string Nombre, string NumeroDocumento, string Direccion, TipoContribuyente TipoContribuyente);
    
    public record DatosCliente(string Nombre, string Apellido, string Email, string Telefono);
    
    /// <summary>
    /// Enumeración para los tipos de contribuyentes
    /// </summary>
    public enum TipoContribuyente
    {
        NoDefinido = 0,
        PersonaNatural = 1,
        PersonaJuridica = 2
    }

    /// <summary>
    /// Pruebas unitarias para ComercialServiceFacade
    /// </summary>
    public class ComercialServiceFacadeTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IClientesFrecuentesPolicy> _clientesFrecuentesPolicyMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly NotificationManager _notificationManager;
        private readonly ComercialServiceFacade _sut;

        public ComercialServiceFacadeTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _clientesFrecuentesPolicyMock = new Mock<IClientesFrecuentesPolicy>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _notificationManager = new NotificationManager();
            
            _sut = new ComercialServiceFacade(
                _clienteRepositoryMock.Object,
                _clientesFrecuentesPolicyMock.Object,
                _servicioFidelizacionMock.Object,
                _notificationManager);
        }
        
        // Test mínimo para verificar que la compilación funciona
        [Fact]
        public async Task Test_Dummy()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            // Configure simple repository response
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente)null);
            
            // Act & Assert - No hacemos nada real, solo verificamos que compile
            await Task.CompletedTask;
        }
    }
} 