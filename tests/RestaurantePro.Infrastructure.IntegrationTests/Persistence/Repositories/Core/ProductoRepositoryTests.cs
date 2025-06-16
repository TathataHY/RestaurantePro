using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class ProductoRepositoryTests : IntegrationTestBase
    {
        private readonly ProductoRepository _repository;
        private readonly Mock<ILogger<ProductoRepository>> _loggerMock;
        private readonly TestDbContextAdapter _dbContextAdapter;

        public ProductoRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<ProductoRepository>>();
            _dbContextAdapter = new TestDbContextAdapter(DbContext);
            _repository = new ProductoRepository(_dbContextAdapter, _loggerMock.Object);
        }

        protected override void SeedDatabase()
        {
            // Crear una categoría para pruebas
            var categoria = ProductoCategoria.Crear(
                "Categoría de Prueba",
                "Descripción de categoría para pruebas",
                1);

            // Crear productos para pruebas
            var productos = new[]
            {
                Producto.Crear(
                    "Producto 1",
                    "Descripción del producto 1",
                    new PrecioProducto(100.00m),
                    categoria.Id,
                    categoria.Nombre
                ),
                Producto.Crear(
                    "Producto 2",
                    "Descripción del producto 2",
                    new PrecioProducto(200.00m),
                    categoria.Id,
                    categoria.Nombre
                ),
                Producto.Crear(
                    "Producto 3",
                    "Descripción del producto 3",
                    new PrecioProducto(300.00m),
                    categoria.Id,
                    categoria.Nombre
                )
            };

            // Desactivar un producto para probar filtros
            productos[2].Desactivar();

            DbContext.Categorias.Add(categoria);
            DbContext.Productos.AddRange(productos);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarTodosLosProductos()
        {
            // Act
            var productos = await _repository.ObtenerTodosAsync(false);

            // Assert
            Assert.Equal(3, productos.Count());
        }

        [Fact]
        public async Task ObtenerProductosPorCategoriaAsync_DebeRetornarSoloProductosActivosDeLaCategoria()
        {
            // Arrange
            var categoriaId = DbContext.Categorias.First().Id;

            // Act
            var productos = await _repository.GetProductosPorCategoriaAsync(categoriaId);

            // Assert
            Assert.Equal(2, productos.Count());
            Assert.All(productos, p => Assert.True(p.EstaActivo));
            Assert.All(productos, p => Assert.Equal(categoriaId, p.CategoriaId));
        }

        [Fact]
        public async Task BuscarProductosAsync_DebeRetornarProductosQueCoincidanConElTermino()
        {
            // Act
            var productos = await _repository.BuscarProductosAsync("producto");

            // Assert
            Assert.Equal(2, productos.Count());
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

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarProductoCorrecto()
        {
            // Arrange
            var productoExistente = await DbContext.Productos.FirstAsync();

            // Act
            var producto = await _repository.ObtenerPorIdAsync(productoExistente.Id);

            // Assert
            Assert.NotNull(producto);
            Assert.Equal(productoExistente.Id, producto.Id);
            Assert.Equal(productoExistente.Nombre, producto.Nombre);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarNullSiNoExiste()
        {
            // Act
            var producto = await _repository.ObtenerPorIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(producto);
        }

        [Fact]
        public async Task AgregarAsync_DebeGuardarNuevoProducto()
        {
            // Arrange
            var categoria = await DbContext.Categorias.FirstAsync();
            var nuevoProducto = Producto.Crear(
                "Producto Nuevo",
                "Descripción",
                new PrecioProducto(50.00m),
                categoria.Id,
                categoria.Nombre
            );

            // Act
            await _repository.AgregarAsync(nuevoProducto);
            var productoGuardado = await DbContext.Productos.FindAsync(nuevoProducto.Id);

            // Assert
            Assert.NotNull(productoGuardado);
            Assert.Equal("Producto Nuevo", productoGuardado.Nombre);
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarProductoExistente()
        {
            // Arrange
            var productoAActualizar = await DbContext.Productos.FirstAsync();
            var nuevoNombre = "Producto Actualizado";
            productoAActualizar.Actualizar(
                nuevoNombre,
                productoAActualizar.Descripcion!,
                productoAActualizar.Precio!
            );

            // Act
            await _repository.ActualizarAsync(productoAActualizar);
            var productoActualizado = await DbContext.Productos.FindAsync(productoAActualizar.Id);

            // Assert
            Assert.NotNull(productoActualizado);
            Assert.Equal(nuevoNombre, productoActualizado.Nombre);
        }
        
        [Fact]
        public async Task EliminarAsync_DebeMarcarProductoComoInactivo()
        {
            // Arrange
            var productoAEliminar = await DbContext.Productos.FirstAsync(p => p.EstaActivo);

            // Act
            await _repository.EliminarAsync(productoAEliminar.Id);
            var productoEliminado = await DbContext.Productos.FindAsync(productoAEliminar.Id);

            // Assert
            Assert.NotNull(productoEliminado);
            Assert.False(productoEliminado.EstaActivo);
        }

        [Fact]
        public async Task ObtenerTodosAsync_DebeRetornarSoloProductosActivos()
        {
            // Act
            var productos = await _repository.ObtenerTodosAsync(true);

            // Assert
            Assert.Equal(2, productos.Count());
            Assert.All(productos, p => Assert.True(p.EstaActivo));
        }

        [Fact]
        public async Task ObtenerProductosMasVendidosAsync_DebeRetornarProductosOrdenadosPorPopularidad()
        {
            // Arrange
            var producto1 = await DbContext.Productos.FirstAsync(p => p.Nombre == "Producto 1");
            var producto2 = await DbContext.Productos.FirstAsync(p => p.Nombre == "Producto 2");
            
            producto1.ActualizarPopularidad(5);
            producto2.ActualizarPopularidad(8);
            await DbContext.SaveChangesAsync();

            // Act
            var productos = await _repository.ObtenerProductosMasVendidosAsync(2);

            // Assert
            Assert.Equal(2, productos.Count());
            Assert.Equal("Producto 2", productos.First().Nombre);
            Assert.Equal("Producto 1", productos.Last().Nombre);
        }

        [Fact]
        public async Task GetProductoConDetallesAsync_DebeRetornarProducto()
        {
            // Arrange
            var productoExistente = await DbContext.Productos.FirstAsync();

            // Act
            var producto = await _repository.GetProductoConDetallesAsync(productoExistente.Id);

            // Assert
            Assert.NotNull(producto);
            Assert.Equal(productoExistente.Id, producto.Id);
        }
    }
} 