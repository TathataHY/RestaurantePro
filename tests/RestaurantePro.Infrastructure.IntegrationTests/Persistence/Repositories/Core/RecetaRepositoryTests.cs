using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Core
{
    public class RecetaRepositoryTests : IntegrationTestBase
    {
        private readonly RecetaRepository _recetaRepository;
        private Guid _productoId1;
        private Guid _recetaId1;
        private Guid _ingredienteId1;
        private Guid _ingredienteId2;
        private readonly PropertyInfo? propInfo = typeof(EntityBase).GetProperty(nameof(EntityBase.Id));

        public RecetaRepositoryTests()
        {
            _recetaRepository = new RecetaRepository(DbContext, Mock.Of<ILogger<RecetaRepository>>());
        }

        protected override async Task SeedDataAsync()
        {
            _productoId1 = Guid.NewGuid();
            _ingredienteId1 = Guid.NewGuid();
            _ingredienteId2 = Guid.NewGuid();

            var producto = Producto.Crear("Hamburguesa", "Carne y queso", new PrecioProducto(12.5m), Guid.NewGuid(), "Comida Rápida");
            // Sobrescribimos el ID para poder usarlo en los tests
            propInfo?.SetValue(producto, _productoId1);


            var receta = Receta.Crear(producto.Id, "Cocinar la carne y montar.", 15);
            receta.AgregarIngrediente(_ingredienteId1, "Carne de Res", 150, UnidadMedida.Gramos);
            receta.AgregarIngrediente(_ingredienteId2, "Queso Cheddar", 2, UnidadMedida.Unidades);

            _recetaId1 = receta.Id;

            producto.Recetas.Add(receta);
            DbContext.Productos.Add(producto);
            // EF Core se encarga de guardar la receta por la relación de navegación
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarRecetaConIngredientes_CuandoExiste()
        {
            // Act
            var receta = await _recetaRepository.ObtenerPorIdAsync(_recetaId1);

            // Assert
            receta.Should().NotBeNull();
            receta!.Id.Should().Be(_recetaId1);
            receta.Ingredientes.Should().HaveCount(2);
            receta.Ingredientes.First(i => i.IngredienteId == _ingredienteId1).Nombre.Should().Be("Carne de Res");
        }
        
        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarNull_CuandoNoExiste()
        {
            // Act
            var receta = await _recetaRepository.ObtenerPorIdAsync(Guid.NewGuid());

            // Assert
            receta.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorProductoIdAsync_DebeRetornarRecetaCorrecta_CuandoExiste()
        {
            // Act
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(_productoId1);

            // Assert
            receta.Should().NotBeNull();
            receta!.ProductoId.Should().Be(_productoId1);
        }

        [Fact]
        public async Task AgregarAsync_DebeAñadirNuevaReceta()
        {
            // Arrange
            var producto2Id = Guid.NewGuid();
            var producto2 = Producto.Crear("Pizza", "Pizza de peperoni", new PrecioProducto(15m), Guid.NewGuid(), "Italiana");
            propInfo?.SetValue(producto2, producto2Id);
            DbContext.Productos.Add(producto2);
            await DbContext.SaveChangesAsync();
            
            var nuevaReceta = Receta.Crear(producto2.Id, "Hornear a 200 grados", 25);
            
            // Act
            await _recetaRepository.AgregarAsync(nuevaReceta);
            await DbContext.SaveChangesAsync();

            // Assert
            var recetaEnDb = await _recetaRepository.ObtenerPorIdAsync(nuevaReceta.Id);
            recetaEnDb.Should().NotBeNull();
            recetaEnDb!.Preparacion.Should().Be("Hornear a 200 grados");
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
            var productoConReceta = await DbContext.Productos
                .Include(p => p.Recetas)
                .FirstOrDefaultAsync(p => p.Id == _productoId1);
            
            var recetaActualizada = productoConReceta?.Recetas.FirstOrDefault(r => r.Id == receta.Id);

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