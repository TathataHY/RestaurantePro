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
    /// <summary>
    /// Adaptador para usar TestDbContext como RestauranteProDbContext en pruebas
    /// </summary>
    public class TestDbContextAdapter : RestauranteProDbContext
    {
        private readonly TestDbContext _testDbContext;

        public TestDbContextAdapter(TestDbContext testDbContext) 
            : base(new DbContextOptions<RestauranteProDbContext>(), null)
        {
            _testDbContext = testDbContext;
        }

        public override DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _testDbContext.Set<TEntity>();
        }

        public override int SaveChanges()
        {
            return _testDbContext.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _testDbContext.SaveChangesAsync(cancellationToken);
        }
    }

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
    }
} 