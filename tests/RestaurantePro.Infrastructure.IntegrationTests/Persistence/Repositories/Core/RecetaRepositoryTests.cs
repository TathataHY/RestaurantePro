using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class RecetaRepositoryTests : IntegrationTestBase
    {
        private readonly RecetaRepository _recetaRepository;
        private readonly Mock<ILogger<RecetaRepository>> _loggerMock;

        private Guid _productoId1;
        private Guid _productoId2;
        private Guid _recetaId1;

        public RecetaRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<RecetaRepository>>();
            _recetaRepository = new RecetaRepository(DbContext, _loggerMock.Object);
        }

        protected override void SeedDatabase()
        {
            base.SeedDatabase();

            var producto1 = Producto.Crear("Pizza Margarita", "Pizza clásica", new PrecioProducto(12.50m), Guid.NewGuid());
            var producto2 = Producto.Crear("Hamburguesa", "Hamburguesa de la casa", new PrecioProducto(10.00m), Guid.NewGuid());
            DbContext.Set<Producto>().AddRange(producto1, producto2);
            _productoId1 = producto1.Id;
            _productoId2 = producto2.Id;


            var receta1 = Receta.Crear(_productoId1, "1. Mezclar ingredientes. 2. Hornear.", 25);
            receta1.AgregarIngrediente(Guid.NewGuid(), "Masa de pizza", 1, UnidadMedida.Unidad);
            receta1.AgregarIngrediente(Guid.NewGuid(), "Salsa de tomate", 200, UnidadMedida.Gramos);
            receta1.AgregarIngrediente(Guid.NewGuid(), "Queso Mozzarella", 250, UnidadMedida.Gramos);
            _recetaId1 = receta1.Id;
            
            var receta2 = Receta.Crear(_productoId2, "1. Freir la carne. 2. Montar.", 15);
            
            DbContext.Set<Receta>().AddRange(receta1, receta2);
            DbContext.SaveChanges();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarRecetaConIngredientes()
        {
            // Act
            var receta = await _recetaRepository.ObtenerPorIdAsync(_recetaId1);

            // Assert
            receta.Should().NotBeNull();
            receta!.Id.Should().Be(_recetaId1);
            receta.Ingredientes.Should().HaveCount(3);
            receta.Ingredientes.First().Nombre.Should().Be("Masa de pizza");
        }
        
        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarNullSiNoExiste()
        {
            // Act
            var receta = await _recetaRepository.ObtenerPorIdAsync(Guid.NewGuid());

            // Assert
            receta.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorProductoIdAsync_DebeRetornarRecetaCorrecta()
        {
            // Act
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(_productoId1);

            // Assert
            receta.Should().NotBeNull();
            receta!.ProductoId.Should().Be(_productoId1);
        }

        [Fact]
        public async Task ObtenerRecetaIngredientesAsync_DebeRetornarIngredientes()
        {
            // Act
            var ingredientes = await _recetaRepository.ObtenerRecetaIngredientesAsync(_recetaId1);

            // Assert
            ingredientes.Should().NotBeNull();
            ingredientes.Should().HaveCount(3);
            ingredientes.First().Nombre.Should().Be("Masa de pizza");
        }

        [Fact]
        public async Task AgregarAsync_DebeAñadirNuevaReceta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var nuevaReceta = Receta.Crear(productoId, "Instrucciones", 10);
            
            // Act
            await _recetaRepository.AgregarAsync(nuevaReceta);
            await DbContext.SaveChangesAsync();

            // Assert
            var recetaEnDb = await _recetaRepository.ObtenerPorIdAsync(nuevaReceta.Id);
            recetaEnDb.Should().NotBeNull();
            recetaEnDb!.Preparacion.Should().Be("Instrucciones");
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarReceta()
        {
            // Arrange
            var receta = await _recetaRepository.ObtenerPorIdAsync(_recetaId1);
            receta!.ActualizarPreparacion("Nuevas instrucciones de preparación");
            
            // Act
            await _recetaRepository.ActualizarAsync(receta);
            await DbContext.SaveChangesAsync();

            // Assert
            var recetaActualizada = await _recetaRepository.ObtenerPorIdAsync(_recetaId1);
            recetaActualizada!.Preparacion.Should().Be("Nuevas instrucciones de preparación");
        }


        [Fact]
        public async Task EliminarAsync_DebeMarcarRecetaComoInactiva()
        {
            // Arrange
            var receta = await _recetaRepository.ObtenerPorIdAsync(_recetaId1);
            
            // Act
            await _recetaRepository.EliminarAsync(receta!);
            await DbContext.SaveChangesAsync();

            // Assert
            var result = await _recetaRepository.ObtenerPorIdAsync(receta!.Id);
            result.Should().BeNull(); // El repo base filtra por Activo=true
        }
    }
} 