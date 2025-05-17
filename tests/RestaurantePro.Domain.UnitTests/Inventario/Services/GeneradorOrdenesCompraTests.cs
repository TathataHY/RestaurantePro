namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class GeneradorOrdenesCompraTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly GeneradorOrdenesCompra _generador;
        private readonly DateTime _fechaActual = new DateTime(2023, 10, 15);

        public GeneradorOrdenesCompraTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            _generador = new GeneradorOrdenesCompra(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object
            );
        }
        
        [Fact]
        public async Task GenerarOrdenesCompraAutomaticas_SinIngredientesConStockBajo_RetornaListaVacia()
        {
            // Arrange
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync(CancellationToken.None))
                .ReturnsAsync(new List<Ingrediente?>());
                
            // Act
            var resultado = await _generador.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            Assert.Empty(resultado);
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()), Times.Never);
        }
    }
} 
