using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class ProductoCategoriaRepositoryTests : IntegrationTestBase
    {
        private readonly IProductoCategoriaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductoCategoriaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
            _repository = ServiceProvider.GetRequiredService<IProductoCategoriaRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarCategoria_CuandoExiste()
        {
            // Arrange
            var categoria1 = ProductoCategoria.Crear("Bebidas", "Gaseosas, jugos y más", 1);
            await _repository.AgregarAsync(categoria1, default);
            await _unitOfWork.SaveChangesAsync(default);

            // Act
            var categoria = await _repository.ObtenerPorIdAsync(categoria1.Id);

            // Assert
            categoria.Should().NotBeNull();
            categoria.Id.Should().Be(categoria1.Id);
        }

        [Fact]
        public async Task ObtenerActivasAsync_DebeRetornarSoloCategoriasActivas()
        {
            // Arrange
            var categoria1 = ProductoCategoria.Crear("Bebidas", "Gaseosas, jugos y más", 1);
            var categoria2 = ProductoCategoria.Crear("Postres", "Dulces y pasteles", 2);
            categoria2.Desactivar();
            await _repository.AgregarAsync(categoria1, default);
            await _repository.AgregarAsync(categoria2, default);
            await _unitOfWork.SaveChangesAsync(default);

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
            // Arrange
            var categoria1 = ProductoCategoria.Crear("Bebidas", "Gaseosas, jugos y más", 1);
            var categoria2 = ProductoCategoria.Crear("Postres", "Dulces y pasteles", 2);
            await _repository.AgregarAsync(categoria1, default);
            await _repository.AgregarAsync(categoria2, default);
            await _unitOfWork.SaveChangesAsync(default);
            
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
            var categoria1 = ProductoCategoria.Crear("Bebidas", "Gaseosas, jugos y más", 1);
            await _repository.AgregarAsync(categoria1, default);
            await _unitOfWork.SaveChangesAsync(default);
            var categoria = await _repository.ObtenerPorIdAsync(categoria1.Id);
            categoria.Should().NotBeNull();

            // Act
            categoria.Actualizar("Bebidas Frias", "Solo bebidas sin alcohol", 3);
            await _repository.ActualizarAsync(categoria, default);
            await _unitOfWork.SaveChangesAsync(default);

            // Assert
            var categoriaActualizada = await _repository.ObtenerPorIdAsync(categoria1.Id);
            categoriaActualizada.Should().NotBeNull();
            categoriaActualizada.Nombre.Should().Be("Bebidas Frias");
            categoriaActualizada.Descripcion.Should().Be("Solo bebidas sin alcohol");
            categoriaActualizada.Orden.Should().Be(3);
        }
    }
} 