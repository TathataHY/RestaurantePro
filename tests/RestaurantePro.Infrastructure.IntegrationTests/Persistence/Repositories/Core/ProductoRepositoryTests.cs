using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class ProductoRepositoryTests : IntegrationTestBase
    {
        private readonly ProductoRepository _repository;
        private readonly Mock<ILogger<ProductoRepository>> _loggerMock;
        private Guid _categoriaId;

        public ProductoRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<ProductoRepository>>();
            _repository = new ProductoRepository(DbContext, _loggerMock.Object);
        }

        protected override void SeedDatabase()
        {
            var categoria = ProductoCategoria.Crear("Categoría de Prueba", "Descripción", 1);
            _categoriaId = categoria.Id;

            var productos = new[]
            {
                Producto.Crear("Producto 1", "Descripción 1", new PrecioProducto(10m), _categoriaId, categoria.Nombre),
                Producto.Crear("Producto 2", "Descripción 2", new PrecioProducto(20m), _categoriaId, categoria.Nombre),
                Producto.Crear("Producto 3", "Descripción 3", new PrecioProducto(30m), _categoriaId, categoria.Nombre)
            };
            productos[2].Desactivar();

            DbContext.Categorias.Add(categoria);
            DbContext.Productos.AddRange(productos);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task ObtenerTodosAsync_CuandoNoSeFiltraPorActivos_DebeRetornarTodosLosProductos()
        {
            // Act
            var result = await _repository.ObtenerTodosAsync(false);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task ObtenerTodosAsync_CuandoSeFiltraPorActivos_DebeRetornarSoloProductosActivos()
        {
            // Act
            var result = await _repository.ObtenerTodosAsync(true);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.EstaActivo);
        }

        [Fact]
        public async Task ObtenerPorCategoriaAsync_DebeRetornarProductosActivosDeEsaCategoria()
        {
            // Act
            var result = await _repository.ObtenerPorCategoriaAsync(_categoriaId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.CategoriaId == _categoriaId && p.EstaActivo);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_CuandoExiste_DebeRetornarElProducto()
        {
            // Arrange
            var productoExistente = await DbContext.Productos.FirstAsync();

            // Act
            var result = await _repository.ObtenerPorIdAsync(productoExistente.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(productoExistente.Id);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_CuandoNoExiste_DebeRetornarNull()
        {
            // Act
            var result = await _repository.ObtenerPorIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AgregarAsync_DebeAñadirElProductoALaBaseDeDatos()
        {
            // Arrange
            var nuevoProducto = Producto.Crear("Producto Nuevo", "Desc", new PrecioProducto(5m), _categoriaId);
            
            // Act
            await _repository.AgregarAsync(nuevoProducto, default);
            await DbContext.SaveChangesAsync(); // Repository.cs no llama a SaveChanges
            
            // Assert
            var productoGuardado = await DbContext.Productos.FindAsync(nuevoProducto.Id);
            productoGuardado.Should().NotBeNull();
            productoGuardado!.Nombre.Should().Be("Producto Nuevo");
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarElProductoEnLaBaseDeDatos()
        {
            // Arrange
            var productoAActualizar = await DbContext.Productos.FirstAsync();
            var nuevoNombre = "Nombre Actualizado";
            productoAActualizar.Actualizar(nuevoNombre, "Desc Act", new PrecioProducto(15m));

            // Act
            await _repository.ActualizarAsync(productoAActualizar, default);
            await DbContext.SaveChangesAsync(); // Repository.cs no llama a SaveChanges

            // Assert
            var productoActualizado = await DbContext.Productos.FindAsync(productoAActualizar.Id);
            productoActualizado.Should().NotBeNull();
            productoActualizado!.Nombre.Should().Be(nuevoNombre);
        }

        [Fact]
        public async Task EliminarAsync_DebeMarcarElProductoComoInactivoEnLaBaseDeDatos()
        {
            // Arrange
            var productoAEliminar = await DbContext.Productos.FirstAsync(p => p.EstaActivo);

            // Act
            await _repository.EliminarAsync(productoAEliminar.Id, default);
            
            // Assert
            // Tenemos que ignorar los filtros globales para encontrar la entidad marcada como eliminada
            var productoEliminado = await DbContext.Productos.IgnoreQueryFilters()
                                                    .FirstOrDefaultAsync(p => p.Id == productoAEliminar.Id);

            productoEliminado.Should().NotBeNull();
            productoEliminado!.EstaActivo.Should().BeFalse();
        }
    }
} 