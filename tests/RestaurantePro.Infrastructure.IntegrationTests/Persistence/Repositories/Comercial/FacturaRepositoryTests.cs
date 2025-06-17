using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class FacturaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IFacturaRepository _repository;
        private IUnitOfWork _unitOfWork;
        private Guid _clienteId1;
        private Guid _comandaId1;
        private Guid _proveedorId1;
        private Guid _meseroId1;
        private Guid _mesaId1;

        public FacturaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IFacturaRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();

            await SeedDependenciesAsync();
            await SeedFacturasAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedDependenciesAsync()
        {
            var mesaRepository = ServiceProvider.GetRequiredService<IMesaRepository>();
            var usuarioRepository = ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var clienteRepository = ServiceProvider.GetRequiredService<IClienteRepository>();
            var proveedorRepository = ServiceProvider.GetRequiredService<IProveedorRepository>();
            var comandaRepository = ServiceProvider.GetRequiredService<IComandaRepository>();
            
            _clienteId1 = Guid.NewGuid();
            _proveedorId1 = Guid.NewGuid();
            _meseroId1 = Guid.NewGuid();
            _mesaId1 = Guid.NewGuid();
            _comandaId1 = Guid.NewGuid();

            var mesa = Mesa.Crear(1, 4, "Interior");
            mesa.SetIdForTesting(_mesaId1);
            await mesaRepository.AgregarAsync(mesa);

            var mesero = Usuario.Crear("mesero1", "Mesero de Prueba", "mesero@test.com", RolUsuario.Mesero);
            mesero.SetIdForTesting(_meseroId1);
            await usuarioRepository.AgregarAsync(mesero);

            var cliente = Cliente.Crear(
                _clienteId1, 
                ClienteNombre.Crear("Cliente", "de Prueba"), 
                Email.Create("cliente@test.com"), 
                PhoneNumber.Create("1234567890"),
                DateTime.Now.AddYears(-25));
            cliente.SetIdForTesting(_clienteId1);
            await clienteRepository.AgregarAsync(cliente);

            var proveedor = Proveedor.Crear("Proveedor de Prueba", "Contacto", "prov@test.com", "0987654321", "Calle Falsa 123", "Ciudad", "12345", "País", "RFC123456", "Banco", 30);
            proveedor.SetIdForTesting(_proveedorId1);
            await proveedorRepository.AgregarAsync(proveedor);

            var comanda = Comanda.Crear(_meseroId1, _clienteId1, _mesaId1);
            comanda.SetIdForTesting(_comandaId1);
            await comandaRepository.AgregarAsync(comanda);

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task SeedFacturasAsync()
        {
            var comandas1 = new List<Guid> { _comandaId1 };

            // Factura 1: Emitida y con pago parcial -> Estado: PagadaParcialmente
            var factura1 = Factura.Crear("F00001", TipoFactura.Venta, "Cliente de Prueba", _clienteId1, "123456789", comandasIds: comandas1);
            factura1.AgregarDetalle(Guid.NewGuid(), "Producto 1", 1, 100, 0.16m);
            factura1.Emitir();
            factura1.RegistrarPago(50, Guid.NewGuid());

            // Factura 2: Emitida y con pago completo -> Estado: Pagada
            var factura2 = Factura.Crear("F00002", TipoFactura.Venta, "Cliente de Prueba", _clienteId1, "123456789");
            factura2.AgregarDetalle(Guid.NewGuid(), "Producto 2", 2, 50, 0.16m);
            factura2.Emitir();
            factura2.RegistrarPago(factura2.Total, Guid.NewGuid());

            // Factura 3: Solo emitida -> Estado: Emitida
            var factura3 = Factura.Crear("F00003", TipoFactura.Venta, "Cliente de Prueba", _clienteId1, "123456789");
            factura3.AgregarDetalle(Guid.NewGuid(), "Producto 3", 1, 200, 0.16m);
            factura3.Emitir();
            
            // Factura 4: Anulada -> no debería aparecer en ObtenerTodos
            var factura4 = Factura.Crear("F00004", TipoFactura.Venta, "Cliente de Prueba", _clienteId1, "123456789");
            factura4.AgregarDetalle(Guid.NewGuid(), "Producto 4", 1, 300, 0.16m);
            factura4.Emitir();
            factura4.Anular("Error en facturación");
            
            // Factura 5: De compra a proveedor, pendiente de pago
            var factura5 = Factura.Crear("FC-P01", TipoFactura.Compra, "Proveedor de Prueba", null, _proveedorId1.ToString());
            factura5.AgregarDetalle(Guid.NewGuid(), "Insumo 1", 10, 20, 0m);
            factura5.Emitir();

            await _repository.AgregarAsync(factura1);
            await _repository.AgregarAsync(factura2);
            await _repository.AgregarAsync(factura3);
            await _repository.AgregarAsync(factura4);
            await _repository.AgregarAsync(factura5);
            await _unitOfWork.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorNumeroAsync_DebeRetornarFactura_CuandoExiste()
        {
            // Act
            var factura = await _repository.ObtenerPorNumeroAsync("F00001");

            // Assert
            factura.Should().NotBeNull();
            factura.NumeroFactura.Should().Be("F00001");
            factura.Detalles.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ObtenerPorComandaAsync_DebeRetornarFacturasAsociadas()
        {
            // Act
            var facturas = await _repository.ObtenerPorComandaAsync(_comandaId1);

            // Assert
            facturas.Should().NotBeNull();
            facturas.Should().ContainSingle(f => f.NumeroFactura == "F00001");
        }

        [Fact]
        public async Task ObtenerPorClienteAsync_DebeRetornarFacturasDelCliente()
        {
            // Act
            var facturas = await _repository.ObtenerPorClienteAsync(_clienteId1);

            // Assert
            facturas.Should().NotBeNull();
            facturas.Should().HaveCount(3); // Se excluye la anulada
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_DebeRetornarFacturasConEstadoCorrecto()
        {
            // Act
            var facturasEmitidas = await _repository.ObtenerPorEstadoAsync(EstadoFactura.Emitida);

            // Assert
            facturasEmitidas.Should().NotBeNull();
            facturasEmitidas.Should().HaveCount(2); // F00003 y FC-P01
            facturasEmitidas.Should().OnlyContain(f => f.Estado == EstadoFactura.Emitida);
        }

        [Fact]
        public async Task ObtenerPorRangoFechasAsync_DebeRetornarFacturasEnRango()
        {
            // Arrange
            var fechaInicio = DateTime.UtcNow.AddDays(-1);
            var fechaFin = DateTime.UtcNow.AddDays(1);

            // Act
            var facturas = await _repository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            // Assert
            facturas.Should().NotBeNull();
            facturas.Should().HaveCount(4); // Se excluye la anulada
        }

        [Fact]
        public async Task ObtenerPendientesPagoAsync_DebeRetornarFacturasNoPagadasCompletamente()
        {
            // Act
            var facturasPendientes = await _repository.ObtenerPendientesPagoAsync();

            // Assert
            facturasPendientes.Should().NotBeNull();
            facturasPendientes.Should().HaveCount(3); // F00001 (parcial), F00003 (emitida), FC-P01 (emitida)
            facturasPendientes.Should().NotContain(f => f.NumeroFactura == "F00002" || f.NumeroFactura == "F00004");
        }
        
        [Theory]
        [InlineData("F", "F00005")]
        [InlineData("TEST-", "TEST-00001")]
        public async Task ObtenerSiguienteNumeroFacturaAsync_DebeGenerarNumeroCorrecto(string prefijo, string numeroEsperado)
        {
            // Act
            var siguienteNumero = await _repository.ObtenerSiguienteNumeroFacturaAsync(prefijo);

            // Assert
            siguienteNumero.Should().Be(numeroEsperado);
        }

        [Fact]
        public async Task ExisteNumeroFacturaAsync_DebeRetornarTrueSiExiste()
        {
            // Act
            var existe = await _repository.ExisteNumeroFacturaAsync("F00001");

            // Assert
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteNumeroFacturaAsync_DebeRetornarFalseSiNoExiste()
        {
            // Act
            var existe = await _repository.ExisteNumeroFacturaAsync("F99999");

            // Assert
            existe.Should().BeFalse();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeIncluirDetalles()
        {
            // Arrange
            var facturaExistente = (await _repository.ObtenerPorNumeroAsync("F00001"))!;
            
            // Act
            var factura = await _repository.ObtenerPorIdAsync(facturaExistente.Id);

            // Assert
            factura.Should().NotBeNull();
            factura!.Detalles.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarTodasLasFacturasActivas()
        {
            // Act
            var facturas = await _repository.ObtenerTodosAsync();

            // Assert
            facturas.Should().NotBeNull();
            facturas.Should().HaveCount(4); // Excluye la factura anulada
            facturas.Should().NotContain(f => f.Estado == EstadoFactura.Anulada);
        }

        [Fact]
        public async Task ActualizarAsync_DebeGuardarCambiosEnLaFactura()
        {
            // Arrange
            var factura = (await _repository.ObtenerPorNumeroAsync("F00003"))!;
            factura.Anular("Motivo de prueba de actualización");

            // Act
            await _repository.ActualizarAsync(factura);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var facturaActualizada = await _repository.ObtenerPorIdAsync(factura.Id);
            facturaActualizada.Should().NotBeNull();
            facturaActualizada!.Estado.Should().Be(EstadoFactura.Anulada);
            facturaActualizada.MotivoAnulacion.Should().Be("Motivo de prueba de actualización");
        }

        [Fact]
        public async Task EliminarAsync_DebeMarcarFacturaComoInactiva()
        {
            // Arrange
            var factura = (await _repository.ObtenerPorNumeroAsync("F00001"))!;
            factura.Should().NotBeNull();
            
            // Act
            await _repository.EliminarPorIdAsync(factura.Id);
            await _unitOfWork.SaveChangesAsync();

            _fixture.ClearTracker();

            // Assert
            var facturaEliminada = await _repository.ObtenerPorIdAsync(factura.Id);
            facturaEliminada.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerFacturasPendientesPorProveedorAsync_DebeRetornarFacturasCorrectas()
        {
            // Act
            var facturasProveedor = await _repository.ObtenerFacturasPendientesPorProveedorAsync(_proveedorId1);

            // Assert
            facturasProveedor.Should().NotBeNull();
            facturasProveedor.Should().ContainSingle();
            facturasProveedor.First().NumeroFactura.Should().Be("FC-P01");
        }
    }
} 