namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    using RestaurantePro.Domain.Inventario.Services;
    using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
    using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
    using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
    using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
    using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
    using RestaurantePro.Domain.Proveedores.Entities;
    using RestaurantePro.Domain.Proveedores.Interfaces;
    using RestaurantePro.Domain.Core.SharedKernel.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class VerificadorStockTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private IVerificadorStock _verificadorService;
        
        public VerificadorStockTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
            
            // Inicializar el servicio usando la interfaz
            _verificadorService = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);
        }

        [Fact]
        public async Task VerificarStock_SinIngredientesBajoMinimo_NoDebeGenerarOrdenesCompra()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>();
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(ingredientes);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(CancellationToken.None);

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()), Times.Never);
        }

        [Fact]
        public async Task VerificarStock_ConIngredientesBajoMinimo_DebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente });
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(CancellationToken.None);

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(1);
            result.OrdenesGeneradas.First().ProveedorId.Should().Be(proveedor.Id);
            result.OrdenesGeneradas.First().Items.Should().HaveCount(1);
            result.OrdenesGeneradas.First().Items.First().IngredienteId.Should().Be(ingrediente.Id);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()));
        }

        [Fact]
        public async Task VerificarStock_VariosIngredientesMismoProveedor_DebeGenerarUnaOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor.Id);
            var ingrediente2 = CrearIngrediente(proveedor.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente1, ingrediente2 });
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(CancellationToken.None);

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(1);
            result.OrdenesGeneradas.First().Items.Should().HaveCount(2);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()));
        }

        [Fact]
        public async Task VerificarStock_IngredientesDistintosProveedores_DebeGenerarVariasOrdenesCompra()
        {
            // Arrange
            var proveedor1 = CrearProveedor();
            var proveedor2 = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor1.Id);
            var ingrediente2 = CrearIngrediente(proveedor2.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente1, ingrediente2 });
                
            // Setup para el primer proveedor
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor1);
                
            // Setup para el segundo proveedor
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor2.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor2);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(CancellationToken.None);

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(2);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()));
        }

        [Fact]
        public async Task VerificarStock_ProveedorInactivo_NoDebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor(activo: false);
            var ingrediente = CrearIngrediente(proveedor.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente });
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(CancellationToken.None);

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            result.Errores.Should().HaveCount(1);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()), Times.Never);
        }

        [Fact]
        public async Task VerificarStock_OrdenCompraExistente_NoDebeGenerarNuevaOrden()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);
            var fecha = new DateTime(2023, 1, 1);
            
            var ordenesExistentes = new List<OrdenCompra> { 
                OrdenCompra.Crear(proveedor.Id, "Orden automática", fecha)
            };
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente });
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesExistentes);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(CancellationToken.None);

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            result.OrdenesActualizadas.Should().HaveCount(1);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>()), Times.Never);
            _ordenCompraRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<OrdenCompra>()));
        }

        // Métodos auxiliares para crear objetos de prueba
        private Proveedor CrearProveedor(bool activo = true)
        {
            Proveedor proveedor = Proveedor.Crear(
                "Proveedor Test", 
                "Contacto Test", 
                "contacto@test.com",
                "123456789", 
                "Dirección Test", 
                "Ciudad Test", 
                "12345", 
                "País Test",
                "RFC123456", 
                "Cuenta: 123456789", 
                30);
                
            if (!activo)
            {
                proveedor.Desactivar();
            }
            
            return proveedor;
        }

        private Ingrediente CrearIngrediente(Guid proveedorId)
        {
            var ingrediente = Ingrediente.Crear(
                "Ingrediente" + Guid.NewGuid().ToString().Substring(0, 8),
                "ING-" + Guid.NewGuid().ToString().Substring(0, 5),
                "Descripción ingrediente",
                UnidadMedida.Kilogramo, 
                10.0m,
                0.0m);
                
            // Simulamos que este ingrediente está asociado al proveedor
            ingrediente.AsociarProveedorPrincipal(proveedorId);
            
            return ingrediente;
        }
    }
} 