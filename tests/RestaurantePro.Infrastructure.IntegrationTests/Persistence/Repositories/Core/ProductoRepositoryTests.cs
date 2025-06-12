using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Entities;
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

        public ProductoRepositoryTests()
        {
            _repository = new ProductoRepository(DbContext);
        }

        protected override void SeedDatabase()
        {
            // Crear una categoría para pruebas
            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = "Categoría de Prueba",
                Descripcion = "Descripción de categoría para pruebas",
                Activo = true
            };

            // Crear productos para pruebas
            var productos = new[]
            {
                new Producto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 1",
                    Descripcion = "Descripción del producto 1",
                    Precio = 100.00m,
                    CategoriaId = categoria.Id,
                    Activo = true
                },
                new Producto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 2",
                    Descripcion = "Descripción del producto 2",
                    Precio = 200.00m,
                    CategoriaId = categoria.Id,
                    Activo = true
                },
                new Producto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 3",
                    Descripcion = "Descripción del producto 3",
                    Precio = 300.00m,
                    CategoriaId = categoria.Id,
                    Activo = false  // Producto inactivo
                }
            };

            DbContext.Categorias.Add(categoria);
            DbContext.Productos.AddRange(productos);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodosLosProductos()
        {
            // Act
            var productos = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(3, productos.Count);
        }

        [Fact]
        public async Task GetProductosPorCategoriaAsync_DebeRetornarSoloProductosActivosDeLaCategoria()
        {
            // Arrange
            var categoriaId = DbContext.Categorias.First().Id;

            // Act
            var productos = await _repository.GetProductosPorCategoriaAsync(categoriaId);

            // Assert
            Assert.Equal(2, productos.Count());
            Assert.All(productos, p => Assert.True(p.Activo));
            Assert.All(productos, p => Assert.Equal(categoriaId, p.CategoriaId));
        }

        [Fact]
        public async Task BuscarProductosAsync_DebeRetornarProductosQueCoincidanConElTermino()
        {
            // Act
            var productos = await _repository.BuscarProductosAsync("producto");

            // Assert
            Assert.Equal(3, productos.Count());
            Assert.All(productos, p => Assert.Contains("producto", p.Nombre.ToLower()));
        }

        [Fact]
        public async Task ExisteProductoConNombreAsync_DebeRetornarTrueSiExisteProductoConElNombre()
        {
            // Act
            var existe = await _repository.ExisteProductoConNombreAsync("Producto 1");

            // Assert
            Assert.True(existe);
        }

        [Fact]
        public async Task ExisteProductoConNombreAsync_DebeRetornarFalseSiNoExisteProductoConElNombre()
        {
            // Act
            var existe = await _repository.ExisteProductoConNombreAsync("Producto Inexistente");

            // Assert
            Assert.False(existe);
        }

        [Fact]
        public async Task ExisteProductoConNombreAsync_DebeExcluirProductoConIdEspecificado()
        {
            // Arrange
            var producto = await DbContext.Productos.FirstAsync(p => p.Nombre == "Producto 1");

            // Act
            var existe = await _repository.ExisteProductoConNombreAsync("Producto 1", producto.Id);

            // Assert
            Assert.False(existe);
        }
    }
} 