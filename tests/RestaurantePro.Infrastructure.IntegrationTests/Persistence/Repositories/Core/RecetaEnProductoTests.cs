using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class RecetaEnProductoTests : IntegrationTestBase, IAsyncLifetime
    {
        private Guid _productoId;
        private Guid _recetaId;

        public RecetaEnProductoTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await SeedRecetaEnProductoAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedRecetaEnProductoAsync()
        {
            var producto = Producto.Crear("Hamburguesa Test", "Carne y queso", new PrecioProducto(12.5m), Guid.NewGuid(), "Comida Rápida");
            
            var receta = Receta.Crear(producto.Id, "Cocinar la carne y montar.", 15);
            receta.AgregarIngrediente(Guid.NewGuid(), "Carne de Res", 150, UnidadMedida.Gramos);
            receta.AgregarIngrediente(Guid.NewGuid(), "Queso Cheddar", 2, UnidadMedida.Unidades);

            producto.Recetas.Add(receta);
            
            DbContext.Productos.Add(producto);
            await DbContext.SaveChangesAsync();
            
            _productoId = producto.Id;
            _recetaId = receta.Id;
            
            // Limpiar el tracker para asegurar que la proxima consulta traiga de la DB
            ClearTracker();
        }

        [Fact]
        public async Task AgregarReceta_AProductoExistente_DebeGuardarseCorrectamente()
        {
            // Arrange
            var producto = await DbContext.Productos.FindAsync(_productoId);
            var nuevaReceta = Receta.Crear(_productoId, "Nueva preparación", 10);

            // Act
            producto!.Recetas.Add(nuevaReceta);
            await DbContext.SaveChangesAsync();
            ClearTracker();

            // Assert
            var productoDesdeDb = await DbContext.Productos.Include(p => p.Recetas).FirstAsync(p => p.Id == _productoId);
            productoDesdeDb.Recetas.Should().HaveCount(2);
            productoDesdeDb.Recetas.Should().Contain(r => r.Id == nuevaReceta.Id);
        }

        [Fact]
        public async Task ActualizarReceta_DesdeProducto_DebeGuardarseCorrectamente()
        {
            // Arrange
            var producto = await DbContext.Productos.Include(p => p.Recetas).FirstAsync(p => p.Id == _productoId);
            var recetaAActualizar = producto.Recetas.First(r => r.Id == _recetaId);
            
            // Act
            recetaAActualizar.ActualizarPreparacion("Nuevas instrucciones de preparación");
            await DbContext.SaveChangesAsync();
            ClearTracker();

            // Assert
            var productoDesdeDb = await DbContext.Productos.Include(p => p.Recetas).FirstAsync(p => p.Id == _productoId);
            var recetaActualizada = productoDesdeDb.Recetas.First(r => r.Id == _recetaId);
            recetaActualizada.Preparacion.Should().Be("Nuevas instrucciones de preparación");
        }

        [Fact]
        public async Task EliminarReceta_DesdeProducto_DebeEliminarseCorrectamente()
        {
            // Arrange
            var producto = await DbContext.Productos.Include(p => p.Recetas).FirstAsync(p => p.Id == _productoId);
            var recetaAEliminar = producto.Recetas.First(r => r.Id == _recetaId);

            // Act
            producto.Recetas.Remove(recetaAEliminar);
            await DbContext.SaveChangesAsync();
            ClearTracker();

            // Assert
            var productoDesdeDb = await DbContext.Productos.Include(p => p.Recetas).FirstAsync(p => p.Id == _productoId);
            productoDesdeDb.Recetas.Should().BeEmpty();
        }
    }
} 