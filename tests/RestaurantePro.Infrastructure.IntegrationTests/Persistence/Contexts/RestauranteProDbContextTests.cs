using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts;

public class TestDbContextTests
{
    private TestDbContext CreateTestContext()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();
            
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            
        // Crear un contexto personalizado para pruebas
        return new TestDbContext(options);
    }

    [Fact]
    public async Task DbContext_DebeGuardarProductoCorrectamente()
    {
        // Arrange
        var dbContext = CreateTestContext();
        
        var categoriaId = Guid.NewGuid();
        var producto = Producto.Crear(
            "Producto de Prueba", 
            "Descripción del producto para prueba", 
            new PrecioProducto(100.0m), 
            categoriaId, 
            "Categoría de Prueba"
        );
        
        // Act
        dbContext.Productos.Add(producto);
        await dbContext.SaveChangesAsync();
        
        // Assert
        var productoGuardado = await dbContext.Productos.FindAsync(producto.Id);
        Assert.NotNull(productoGuardado);
        Assert.Equal("Producto de Prueba", productoGuardado.Nombre);
        Assert.Equal(100.0m, productoGuardado.Precio.Valor);
        
        // Limpiar
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task DbContext_DebeComenzarTransaccionCorrectamente()
    {
        // Arrange
        var dbContext = CreateTestContext();
        
        // Act & Assert
        using var transaction = await dbContext.BeginTransactionAsync();
        Assert.NotNull(transaction);
        
        // Limpiar
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.DisposeAsync();
    }
} 