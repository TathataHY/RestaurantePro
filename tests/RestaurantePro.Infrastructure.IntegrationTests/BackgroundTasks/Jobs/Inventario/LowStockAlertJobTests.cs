using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;
using Xunit;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Inventario;

public class LowStockAlertJobTests
{
    private readonly IIngredienteRepository _ingredienteRepositoryMock;
    private readonly IUsuarioRepository _usuarioRepositoryMock;
    private readonly INotificationService _notificationServiceMock;
    private readonly ILogger<LowStockAlertJob> _loggerMock;

    public LowStockAlertJobTests()
    {
        _ingredienteRepositoryMock = Substitute.For<IIngredienteRepository>();
        _usuarioRepositoryMock = Substitute.For<IUsuarioRepository>();
        _notificationServiceMock = Substitute.For<INotificationService>();
        _loggerMock = Substitute.For<ILogger<LowStockAlertJob>>();
    }

    [Fact]
    public async Task ExecuteAsync_NoLowStockIngredients_ShouldLogAndNotCreateNotifications()
    {
        // Arrange
        _ingredienteRepositoryMock.GetLowStockIngredientsAsync(Arg.Any<int>()).Returns(new List<Ingrediente>());

        var job = new LowStockAlertJob(
            _ingredienteRepositoryMock,
            _usuarioRepositoryMock,
            _notificationServiceMock,
            _loggerMock
        );

        // Act
        await job.ExecuteAsync(null, null, null);

        // Assert
        _loggerMock.Received(1).LogInformation("Iniciando job de alerta de bajo stock...");
        _loggerMock.Received(1).LogInformation("No hay ingredientes con bajo stock.");
        await _notificationServiceMock.DidNotReceiveWithAnyArgs().CreateNotificationAsync(default, default, default);
    }

    [Fact]
    public async Task ExecuteAsync_WithLowStockIngredients_ShouldCreateNotifications()
    {
        // Arrange
        var lowStockIngredients = new List<Ingrediente>
        {
            new Ingrediente { Id = Guid.NewGuid(), Nombre = "Tomates", StockActual = 5, StockMinimo = 10 },
            new Ingrediente { Id = Guid.NewGuid(), Nombre = "Cebollas", StockActual = 2, StockMinimo = 5 }
        };

        var usersToNotify = new List<Usuario>
        {
            new Usuario { Id = "user-1", Rol = RolUsuario.Administrador },
            new Usuario { Id = "user-2", Rol = RolUsuario.Gerente }
        };

        _ingredienteRepositoryMock.GetLowStockIngredientsAsync(Arg.Any<int>()).Returns(lowStockIngredients);
        _usuarioRepositoryMock.GetUsersByRoleAsync(Arg.Is<RolUsuario[]>(roles => roles.Contains(RolUsuario.Administrador) && roles.Contains(RolUsuario.Gerente)))
            .Returns(usersToNotify);

        var job = new LowStockAlertJob(
            _ingredienteRepositoryMock,
            _usuarioRepositoryMock,
            _notificationServiceMock,
            _loggerMock
        );

        // Act
        await job.ExecuteAsync(null, null, null);

        // Assert
        _loggerMock.Received(1).LogInformation("Iniciando job de alerta de bajo stock...");
        _loggerMock.Received(1).LogWarning("Se encontraron {Count} ingredientes con bajo stock.", lowStockIngredients.Count);

        await _notificationServiceMock.Received(usersToNotify.Count).CreateNotificationAsync(
            Arg.Any<string>(),
            "Alerta de Bajo Stock",
            Arg.Is<string>(msg => msg.Contains("Los siguientes ingredientes tienen bajo stock:"))
        );
    }
} 