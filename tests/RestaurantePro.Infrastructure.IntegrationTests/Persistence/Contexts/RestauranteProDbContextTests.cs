using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts;

public class RestauranteProDbContextTests
{
    [Fact]
    public async Task DbContext_DebeGuardarProductoCorrectamente()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();
            
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RestauranteProDbContext>();
        
        var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
            
        var dbContext = new RestauranteProDbContext(options, logger);
        
        var productoId = Guid.NewGuid();
        var producto = new Producto
        {
            Id = productoId,
            Nombre = "Producto de Prueba",
            Descripcion = "Descripción del producto para prueba",
            Precio = 100.0m,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            CreadoPor = "Test"
        };
        
        // Act
        dbContext.Productos.Add(producto);
        await dbContext.SaveChangesAsync();
        
        // Assert
        var productoGuardado = await dbContext.Productos.FindAsync(productoId);
        Assert.NotNull(productoGuardado);
        Assert.Equal("Producto de Prueba", productoGuardado.Nombre);
        Assert.Equal(100.0m, productoGuardado.Precio);
    }
    
    [Fact]
    public async Task DbContext_DebeComenzarTransaccionCorrectamente()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();
            
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RestauranteProDbContext>();
        
        var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
            
        var dbContext = new RestauranteProDbContext(options, logger);
        
        // Act & Assert
        using var transaction = await dbContext.BeginTransactionAsync();
        Assert.NotNull(transaction);
    }
} 