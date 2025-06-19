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
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Inventario;

public class LowStockAlertJobTests
{
    private readonly IIngredienteRepository _ingredienteRepositoryMock;
    private readonly IUsuarioRepository _usuarioRepositoryMock;
    private readonly INotificationService _notificationServiceMock;
    private readonly IEmailService _emailServiceMock;
    private readonly ILogger<LowStockAlertJob> _loggerMock;
    private readonly IOptions<LowStockAlertOptions> _optionsMock;

    public LowStockAlertJobTests()
    {
        _ingredienteRepositoryMock = Substitute.For<IIngredienteRepository>();
        _usuarioRepositoryMock = Substitute.For<IUsuarioRepository>();
        _notificationServiceMock = Substitute.For<INotificationService>();
        _emailServiceMock = Substitute.For<IEmailService>();
        _loggerMock = Substitute.For<ILogger<LowStockAlertJob>>();
        _optionsMock = Options.Create(new LowStockAlertOptions { SendEmailAlerts = true, SendSystemNotifications = true });
    }

    private LowStockAlertJob CreateJob()
    {
        return new LowStockAlertJob(
            _loggerMock,
            _ingredienteRepositoryMock,
            _emailServiceMock,
            _notificationServiceMock,
            _usuarioRepositoryMock,
            _optionsMock
        );
    }

    [Fact]
    public async Task ExecuteInternalAsync_NoLowStockIngredients_ShouldLogAndDoNothingElse()
    {
        // Arrange
        _ingredienteRepositoryMock.ObtenerConStockBajoAsync(Arg.Any<CancellationToken>()).Returns(new List<Ingrediente>());
        var job = CreateJob();

        // Act
        await job.DoWork(new CancellationToken());

        // Assert
        _loggerMock.Received(1).LogInformation("Verificando ingredientes con stock bajo");
        _loggerMock.Received(1).LogInformation("No se encontraron ingredientes con stock bajo");
        await _notificationServiceMock.DidNotReceiveWithAnyArgs().EnviarNotificacionAsync(default, default, default, default);
        await _emailServiceMock.DidNotReceiveWithAnyArgs().SendEmailAsync(default, default, default);
    }

    [Fact]
    public async Task ExecuteInternalAsync_WithLowStockIngredients_ShouldSendNotificationsAndEmails()
    {
        // Arrange
        var ingredientes = new List<Ingrediente>
        {
            Ingrediente.Crear("Tomates", "ING-001", "Tomates Rojos", UnidadMedida.Kilogramo, 10, 5),
            Ingrediente.Crear("Cebollas", "ING-002", "Cebollas Blancas", UnidadMedida.Kilogramo, 5, 1)
        };
        
        var usuarios = new List<Usuario>
        {
            Usuario.Crear("inventario_user", "Usuario de Inventario", "inventario@test.com", RolUsuario.EncargadoInventario)
        };

        _ingredienteRepositoryMock.ObtenerConStockBajoAsync(Arg.Any<CancellationToken>()).Returns(ingredientes);
        _usuarioRepositoryMock.ObtenerPorRolAsync(RolUsuario.EncargadoInventario, Arg.Any<CancellationToken>()).Returns(usuarios);

        var job = CreateJob();

        // Act
        await job.DoWork(new CancellationToken());

        // Assert
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains($"Se encontraron {ingredientes.Count} ingredientes con stock bajo")),
            null,
            Arg.Any<Func<object, Exception, string>>());
        
        // Verificar notificación de sistema
        await _notificationServiceMock.Received(1).EnviarNotificacionAsync(
            usuarios[0].Id, 
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<string>());

        // Verificar alerta de email
        await _emailServiceMock.Received(1).SendEmailAsync(
            usuarios[0].Email,
            Arg.Any<string>(),
            Arg.Is<string>(html => html.Contains(ingredientes[0].Nombre) && html.Contains(ingredientes[1].Nombre))
        );
    }
} 