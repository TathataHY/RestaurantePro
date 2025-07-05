using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Proveedores
{
    public class ContactoProveedorRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IContactoProveedorRepository _repository = null!;
        private Guid _proveedorId;
        private Guid _contactoId1;
        private Guid _contactoId2;

        public ContactoProveedorRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IContactoProveedorRepository>();
            await SeedDataAsync();
        }

        private async Task SeedDataAsync()
        {
            var proveedor = Proveedor.Crear(
                "Proveedor para Contactos", "Contacto P", "contactos@proveedor.com", "333333333",
                "Direccion P", "Ciudad P", "3333333", "Pais P", "P3P3P3P3P3P3", "Info Bancaria P", 30);
            
            _proveedorId = proveedor.Id;

            var contacto1 = proveedor.AgregarContacto("Juan Perez", "Gerente", "111222333", "juan.perez@proveedor.com");
            _contactoId1 = contacto1.Id;

            var contacto2 = proveedor.AgregarContacto("Maria Lopez", "Ventas", "444555666", "maria.lopez@proveedor.com");
            _contactoId2 = contacto2.Id;
            
            await DbContext.Proveedores.AddAsync(proveedor);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarContactoExistente()
        {
            // Act
            var contacto = await _repository.ObtenerPorIdAsync(_contactoId1);

            // Assert
            contacto.Should().NotBeNull();
            contacto?.Id.Should().Be(_contactoId1);
            contacto?.Nombre.Should().Be("Juan Perez");
        }

        [Fact]
        public async Task ObtenerPorProveedorAsync_DebeRetornarTodosLosContactosDeUnProveedor()
        {
            // Act
            var contactos = await _repository.ObtenerPorProveedorAsync(_proveedorId);

            // Assert
            contactos.Should().NotBeNull();
            contactos.Should().HaveCount(2);
            contactos.Should().Contain(c => c.Id == _contactoId1);
            contactos.Should().Contain(c => c.Id == _contactoId2);
        }

        [Theory]
        [InlineData("Juan", 1)]
        [InlineData("Lopez", 1)]
        [InlineData("Gerente", 1)]
        [InlineData("Ventas", 1)]
        [InlineData("e", 2)]
        public async Task BuscarPorNombreOCargoAsync_DebeRetornarContactosCoincidentes(string termino, int conteoEsperado)
        {
            // Act
            var contactos = await _repository.BuscarPorNombreOCargoAsync(termino);

            // Assert
            contactos.Should().HaveCount(conteoEsperado);
        }

        [Fact]
        public async Task BuscarPorEmailAsync_DebeRetornarContactoCorrecto()
        {
            // Act
            var contacto = await _repository.BuscarPorEmailAsync("juan.perez@proveedor.com");

            // Assert
            contacto.Should().NotBeNull();
            contacto?.Id.Should().Be(_contactoId1);
        }

        [Fact]
        public async Task BuscarPorTelefonoAsync_DebeRetornarContactosCorrectos()
        {
            // Act
            var contactos = await _repository.BuscarPorTelefonoAsync("111");

            // Assert
            contactos.Should().ContainSingle();
            contactos.First().Id.Should().Be(_contactoId1);
        }

        [Fact]
        public async Task AgregarAsync_DebeGuardarNuevoContacto()
        {
            // Arrange
            var proveedor = await DbContext.Proveedores.FindAsync(_proveedorId);
            var nuevoContacto = proveedor.AgregarContacto("Carlos Sanchez", "Soporte", "777888999", "carlos.sanchez@proveedor.com");

            // Act
            await _repository.AgregarAsync(nuevoContacto);
            
            // Assert
            var contactoGuardado = await DbContext.Set<ContactoProveedor>().FindAsync(nuevoContacto.Id);
            contactoGuardado.Should().NotBeNull();
            contactoGuardado?.Nombre.Should().Be("Carlos Sanchez");
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarContactoExistente()
        {
            // Arrange
            var contacto = await DbContext.Set<ContactoProveedor>().FindAsync(_contactoId1);
            contacto.ActualizarInformacion("Juan Perez Modificado", "Gerente General", "111222334", "juan.perez.mod@proveedor.com");

            // Act
            await _repository.ActualizarAsync(contacto);

            // Assert
            var contactoActualizado = await DbContext.Set<ContactoProveedor>().FindAsync(_contactoId1);
            contactoActualizado?.Nombre.Should().Be("Juan Perez Modificado");
        }

        [Fact]
        public async Task EliminarAsync_DebeRemoverContacto()
        {
            // Arrange
            var contacto = await DbContext.Set<ContactoProveedor>().FindAsync(_contactoId1);

            // Act
            await _repository.EliminarAsync(contacto);

            // Assert
            var contactoEliminado = await DbContext.Set<ContactoProveedor>().FindAsync(_contactoId1);
            contactoEliminado.Should().NotBeNull();
            contactoEliminado.EstaEliminado.Should().BeTrue();
        }

        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }
    }
} 