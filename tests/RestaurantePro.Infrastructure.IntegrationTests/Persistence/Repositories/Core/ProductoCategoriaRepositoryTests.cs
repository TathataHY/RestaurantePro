using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class ProductoCategoriaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IProductoCategoriaRepository _repository;
        private IUnitOfWork _unitOfWork;
        private Guid _categoriaId1;

        public async Task InitializeAsync()
        {
            _repository = ServiceProvider.GetRequiredService<IProductoCategoriaRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var categoria1 = ProductoCategoria.Crear("Bebidas", "Gaseosas, jugos y más", 1);
            var categoria2 = ProductoCategoria.Crear("Postres", "Dulces y pasteles", 2);
            categoria2.Desactivar();

            _categoriaId1 = categoria1.Id;

            await DbContext.Set<ProductoCategoria>().AddRangeAsync(categoria1, categoria2);
            await DbContext.SaveChangesAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarCategoria_CuandoExiste()
        {
            // Act
            var categoria = await _repository.ObtenerPorIdAsync(_categoriaId1);

            // Assert
            categoria.Should().NotBeNull();
            categoria.Id.Should().Be(_categoriaId1);
        }

        [Fact]
        public async Task ObtenerActivasAsync_DebeRetornarSoloCategoriasActivas()
        {
            // Act
            var categorias = await _repository.ObtenerActivasAsync();

            // Assert
            categorias.Should().NotBeNull();
            categorias.Should().HaveCount(1);
            categorias.First().Nombre.Should().Be("Bebidas");
        }

        [Fact]
        public async Task ObtenerTodasAsync_DebeRetornarTodasLasCategorias()
        {
            // Act
            var categorias = await _repository.ObtenerTodasAsync();

            // Assert
            categorias.Should().NotBeNull();
            categorias.Should().HaveCount(2);
        }

        [Fact]
        public async Task AgregarAsync_DebeAñadirLaCategoriaALaBaseDeDatos()
        {
            // Arrange
            var nuevaCategoria = ProductoCategoria.Crear("Entradas", "Aperitivos para empezar", 0);
            
            // Act
            await _repository.AgregarAsync(nuevaCategoria, default);
            await _unitOfWork.SaveChangesAsync(default);

            // Assert
            var categoriaGuardada = await _repository.ObtenerPorIdAsync(nuevaCategoria.Id);
            categoriaGuardada.Should().NotBeNull();
            categoriaGuardada.Nombre.Should().Be("Entradas");
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarLaCategoriaEnLaBaseDeDatos()
        {
            // Arrange
            var categoria = await _repository.ObtenerPorIdAsync(_categoriaId1);
            categoria.Should().NotBeNull();

            // Act
            categoria.Actualizar("Bebidas Frias", "Solo bebidas sin alcohol", 3);
            await _repository.ActualizarAsync(categoria, default);
            await _unitOfWork.SaveChangesAsync(default);

            // Assert
            var categoriaActualizada = await _repository.ObtenerPorIdAsync(_categoriaId1);
            categoriaActualizada.Should().NotBeNull();
            categoriaActualizada.Nombre.Should().Be("Bebidas Frias");
            categoriaActualizada.Descripcion.Should().Be("Solo bebidas sin alcohol");
            categoriaActualizada.Orden.Should().Be(3);
        }
    }
} 