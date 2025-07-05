using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;
using Xunit;
using System.Reflection;
using System.Linq.Expressions;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Inventario
{
    public class ExpirationCheckJobTests
    {
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<ExpirationCheckJob>> _loggerMock;
        private readonly ExpirationCheckJob _job;

        public ExpirationCheckJobTests()
        {
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ExpirationCheckJob>>();

            _job = new ExpirationCheckJob(
                _loggerMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_CuandoHayItemsAExpirar_DebeLoggearWarning()
        {
            // Arrange
            var productoAExpirar = Producto.Crear("Producto A", "Desc A", new PrecioProducto(10.0m), Guid.NewGuid(), "Bebidas");
            productoAExpirar.ActualizarFechaExpiracion(DateTime.UtcNow.AddDays(3));
            var productos = new List<Producto> { productoAExpirar };

            var ingredienteAExpirar = Ingrediente.Crear("Ingrediente X", "IX-001", "Desc X", UnidadMedida.Kilogramo, 5, 1);
            ingredienteAExpirar.ActualizarFechaExpiracion(DateTime.UtcNow.AddDays(5));
            var ingredientes = new List<Ingrediente> { ingredienteAExpirar };

            _productoRepositoryMock.Setup(r => r.BuscarAsync(It.IsAny<Expression<Func<Producto, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos);
            _ingredienteRepositoryMock.Setup(r => r.BuscarAsync(It.IsAny<Expression<Func<Ingrediente, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes);

            // Act
            await _job.ExecuteAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("está próximo a expirar")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_CuandoNoHayItemsAExpirar_NoDebeLoggearWarning()
        {
            // Arrange
            _productoRepositoryMock.Setup(r => r.BuscarAsync(It.IsAny<Expression<Func<Producto, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Producto>());
            
            _ingredienteRepositoryMock.Setup(r => r.BuscarAsync(It.IsAny<Expression<Func<Ingrediente, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Ingrediente>());

            // Act
            await _job.ExecuteAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("está próximo a expirar")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
            
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    public static class BackgroundJobBaseTestExtensions
    {
        public static Task TestExecuteAsync(this BackgroundJobBase job, CancellationToken cancellationToken)
        {
            var method = typeof(BackgroundJobBase).GetMethod("ExecuteInternalAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method.Invoke(job, new object[] { cancellationToken });
        }
    }
} 