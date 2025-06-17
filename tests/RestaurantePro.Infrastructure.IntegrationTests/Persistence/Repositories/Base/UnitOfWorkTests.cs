using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Base
{
    public class UnitOfWorkTests : IntegrationTestBase
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly Guid _categoriaId = Guid.NewGuid();

        public UnitOfWorkTests()
        {
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>() as UnitOfWork;
        }

        [Fact]
        public async Task SaveChangesAsync_DebeGuardarLosCambiosEnLaBaseDeDatos()
        {
            // Arrange
            var nuevoProducto = Producto.Crear(
                "Producto de Prueba",
                "Descripción de prueba",
                new PrecioProducto(10.0m),
                _categoriaId,
                "Bebidas"
            );
            await DbContext.AddAsync(nuevoProducto);

            // Act
            var result = await _unitOfWork.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
            var productoGuardado = await DbContext.FindAsync<Producto>(nuevoProducto.Id);
            productoGuardado.Should().NotBeNull();
            productoGuardado.Nombre.Should().Be("Producto de Prueba");
        }

        [Fact]
        public async Task CommitTransactionAsync_DebeConfirmarLosCambios()
        {
            // Arrange
            var nuevoProducto = Producto.Crear(
                "Producto en Transacción",
                "Descripción",
                new PrecioProducto(20.0m),
                _categoriaId,
                "Plato Fuerte"
            );

            await _unitOfWork.BeginTransactionAsync();
            await DbContext.AddAsync(nuevoProducto);
            await _unitOfWork.SaveChangesAsync();

            // Act
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            var productoGuardado = await DbContext.FindAsync<Producto>(nuevoProducto.Id);
            productoGuardado.Should().NotBeNull();
        }

        [Fact]
        public async Task RollbackTransactionAsync_DebeRevertirLosCambios()
        {
            // Arrange
             var nuevoProducto = Producto.Crear(
                "Producto para Rollback",
                "Descripción",
                new PrecioProducto(30.0m),
                _categoriaId,
                "Postre"
            );

            await _unitOfWork.BeginTransactionAsync();
            await DbContext.AddAsync(nuevoProducto);
            await _unitOfWork.SaveChangesAsync();

            // Act
            await _unitOfWork.RollbackTransactionAsync();

            // Assert
            using var scope = ServiceProvider.CreateScope();
            var scopedDbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var productoGuardado = await scopedDbContext.FindAsync<Producto>(nuevoProducto.Id);
            productoGuardado.Should().BeNull();
        }

        [Fact]
        public async Task EjecutarEnTransaccionAsync_DebeConfirmarSiNoHayErrores()
        {
            // Arrange
            var nuevoProducto = Producto.Crear(
                "Producto en EjecutarEnTransaccion",
                "Descripción",
                new PrecioProducto(40.0m),
                _categoriaId,
                "Entrada"
            );

            // Act
            await _unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                await DbContext.AddAsync(nuevoProducto);
                await _unitOfWork.SaveChangesAsync();
            });

            // Assert
            var productoGuardado = await DbContext.FindAsync<Producto>(nuevoProducto.Id);
            productoGuardado.Should().NotBeNull();
        }

        [Fact]
        public async Task EjecutarEnTransaccionAsync_DebeRevertirSiHayExcepcion()
        {
            // Arrange
            var nuevoProducto = Producto.Crear(
                "Producto para Excepción en Transacción",
                "Descripción",
                new PrecioProducto(50.0m),
                _categoriaId,
                "Bebida"
            );

            // Act
            Func<Task> act = async () => await _unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                await DbContext.AddAsync(nuevoProducto);
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException("Error forzado para rollback");
            });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            using var scope = ServiceProvider.CreateScope();
            var scopedDbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var productoGuardado = await scopedDbContext.FindAsync<Producto>(nuevoProducto.Id);
            productoGuardado.Should().BeNull();
        }
    }
} 