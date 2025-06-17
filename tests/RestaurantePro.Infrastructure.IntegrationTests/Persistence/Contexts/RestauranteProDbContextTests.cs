using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts;

public class TestDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task DbContext_DebeGuardarProductoCorrectamente()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var producto = Producto.Crear(
            "Producto de Prueba", 
            "Descripción del producto para prueba", 
            new PrecioProducto(100.0m), 
            categoriaId, 
            "Categoría de Prueba"
        );
        
        // Act
        DbContext.Productos.Add(producto);
        await DbContext.SaveChangesAsync();
        
        // Assert
        var productoGuardado = await DbContext.Productos.FindAsync(producto.Id);
        productoGuardado.Should().NotBeNull();
        productoGuardado.Nombre.Should().Be("Producto de Prueba");
        productoGuardado.Precio.Valor.Should().Be(100.0m);
    }
    
    [Fact]
    public async Task DbContext_DebeComenzarTransaccionCorrectamente()
    {
        // Arrange
        // El DbContext ya está disponible desde la clase base
        
        // Act & Assert
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        transaction.Should().NotBeNull();
    }
} 