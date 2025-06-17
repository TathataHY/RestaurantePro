using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System;
using RestaurantePro.Domain.Core.Productos.ValueObjects;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class ProductoRepositoryTests : IntegrationTestBase
    {
        private readonly ProductoRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private Guid _categoriaId1;
        private Guid _categoriaId2;
        private Guid _productoId1;

        public ProductoRepositoryTests()
        {
            _repository = new ProductoRepository(DbContext, Mock.Of<ILogger<ProductoRepository>>());
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        protected override async Task SeedDataAsync()
        {
            _categoriaId1 = Guid.NewGuid();
            _categoriaId2 = Guid.NewGuid();

            var productos = new[]
            {
                Producto.Crear("Hamburguesa", "Carne y queso", new PrecioProducto(12.5m), _categoriaId1, "Comida Rápida"),
                Producto.Crear("Pizza", "Queso y pepperoni", new PrecioProducto(15.0m), _categoriaId1, "Comida Rápida"),
                Producto.Crear("Ensalada", "Lechuga y tomate", new PrecioProducto(8.0m), _categoriaId2, "Saludable")
            };

            productos[2].Desactivar();
            
            _productoId1 = productos[0].Id;

            await DbContext.Productos.AddRangeAsync(productos);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarProducto_CuandoExiste()
        {
            // Act
            var producto = await _repository.ObtenerPorIdAsync(_productoId1);

            // Assert
            producto.Should().NotBeNull();
            producto!.Id.Should().Be(_productoId1);
            producto.Nombre.Should().Be("Hamburguesa");
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarSoloProductosActivos()
        {
            // Act
            var productos = await _repository.ObtenerTodosAsync();

            // Assert
            productos.Should().NotBeNull();
            productos.Should().HaveCount(2);
            productos.All(p => p.EstaActivo).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorCategoriaAsync_DebeRetornarProductosDeEsaCategoria()
        {
            // Act
            var productos = await _repository.ObtenerPorCategoriaAsync(_categoriaId1);

            // Assert
            productos.Should().NotBeNull();
            productos.Should().HaveCount(2);
            productos.All(p => p.CategoriaId == _categoriaId1).Should().BeTrue();
        }

        [Fact]
        public async Task AgregarAsync_DebeAñadirElProductoALaBaseDeDatos()
        {
            // Arrange
            var nuevoProducto = Producto.Crear("Fanta", "Refresco de naranja", new PrecioProducto(2.5m), _categoriaId1, "Comida Rápida");
            
            // Act
            await _repository.AgregarAsync(nuevoProducto, default);
            await _unitOfWork.GuardarCambiosAsync(default);
            
            // Assert
            var productoGuardado = await DbContext.Productos.FindAsync(nuevoProducto.Id);
            productoGuardado.Should().NotBeNull();
            productoGuardado.Nombre.Should().Be("Fanta");
        }

        [Fact]
        public async Task EliminarAsync_DebeMarcarElProductoComoInactivoEnLaBaseDeDatos()
        {
            // Arrange
            var productoActivo = await DbContext.Productos.FirstAsync(p => p.EstaActivo);

            // Act
            await _repository.EliminarAsync(productoActivo.Id, default);
            await _unitOfWork.GuardarCambiosAsync(default);
            
            // Assert
            var productoEliminado = await DbContext.Productos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == productoActivo.Id);
            productoEliminado.Should().NotBeNull();
            productoEliminado.EstaActivo.Should().BeFalse();
        }
    }
} 