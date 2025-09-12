using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class NotificacionesPageTests : TestContext
{
    private readonly Mock<INotificacionesApiService> _notificacionesApiMock;

    public NotificacionesPageTests()
    {
        _notificacionesApiMock = new Mock<INotificacionesApiService>();
        
        Services.AddSingleton(_notificacionesApiMock.Object);
        Services.AddSingleton<INotificacionesApiService>(_notificacionesApiMock.Object);
        Services.AddSingleton<TestNavigationManager>();
        
        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("console.error", _ => true);
        JSInterop.SetupVoid("alert", _ => true);
    }

    [Fact]
    public void NotificacionesPage_ShouldRender()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public async Task NotificacionesPage_ShouldLoadNotificaciones()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new NotificacionDto
            {
                Id = Guid.NewGuid(),
                Titulo = "Nueva orden recibida",
                Mensaje = "Se ha recibido una nueva orden en la mesa 5",
                Tipo = "Informativa",
                FechaCreacion = DateTime.Now,
                EstaLeida = false,
                EntidadRelacionadaId = Guid.NewGuid()
            }
        };

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();
        component.WaitForAssertion(() => component.FindAll(".card").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task NotificacionesPage_ShouldDisplayNotificaciones()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new NotificacionDto
            {
                Id = Guid.NewGuid(),
                Titulo = "Nueva orden recibida",
                Mensaje = "Se ha recibido una nueva orden en la mesa 5",
                Tipo = "Informativa",
                FechaCreacion = DateTime.Now,
                EstaLeida = false,
                EntidadRelacionadaId = Guid.NewGuid()
            },
            new NotificacionDto
            {
                Id = Guid.NewGuid(),
                Titulo = "Stock bajo",
                Mensaje = "El ingrediente tomate tiene stock bajo",
                Tipo = "Advertencia",
                FechaCreacion = DateTime.Now.AddHours(-1),
                EstaLeida = true,
                EntidadRelacionadaId = null
            }
        };

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();
        component.WaitForAssertion(() => component.FindAll(".card").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveActionButtons()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert - Buscar por texto en lugar de onclick
        component.FindAll("button").Should().HaveCountGreaterThan(0);
        var buttons = component.FindAll("button");
        buttons.Should().Contain(b => b.TextContent.Contains("Marcar todas como leidas"));
        buttons.Should().Contain(b => b.TextContent.Contains("Nueva Notificacion"));
    }

    [Fact]
    public void NotificacionesPage_ShouldDisplayStatistics()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll(".row.mb-4").Should().HaveCount(1);
        component.FindAll(".row.mb-4 .col-md-3").Should().HaveCount(4); // 4 tarjetas de estadísticas
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveCorrectPageTitle()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert - Buscar el título en el h3
        component.Find("h3").TextContent.Trim().Should().Be("🔔 Notificaciones");
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveFilterControls()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll("select").Should().HaveCount(2); // 2 selectores de filtro
        component.FindAll("select").Should().Contain(s => s.GetAttribute("class") != null && s.GetAttribute("class")!.Contains("form-select"));
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveNewNotificationButton()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var newButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Nueva Notificacion"));
        newButton.Should().NotBeNull();
        newButton!.TextContent.Should().Contain("Nueva Notificacion");
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveMarkAllAsReadButton()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert - Buscar por texto en lugar de onclick
        var buttons = component.FindAll("button");
        var markAllButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Marcar todas como leidas"));
        markAllButton.Should().NotBeNull();
        markAllButton!.TextContent.Should().Contain("Marcar todas como leidas");
    }

    [Fact]
    public void NotificacionesPage_ShouldHandleEmptyState()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void NotificacionesPage_ShouldHandleErrorState()
    {
        // Arrange
        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveStatisticsCards()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll(".row.mb-4").Should().HaveCount(1);
        component.FindAll(".row.mb-4 .col-md-3").Should().HaveCount(4); // 4 tarjetas de estadísticas
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveFilterSelects()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll("select").Should().HaveCount(2); // 2 selectores de filtro
        component.FindAll("select").Should().Contain(s => s.GetAttribute("class") != null && s.GetAttribute("class")!.Contains("form-select"));
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveCorrectDescription()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.Find("p.text-muted").TextContent.Trim().Should().Be("Centro de notificaciones del sistema.");
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveStatisticsLabels()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll(".card-text").Should().HaveCount(4); // 4 etiquetas de estadísticas
        component.FindAll(".card-text").Should().Contain(t => t.TextContent.Contains("Total"));
        component.FindAll(".card-text").Should().Contain(t => t.TextContent.Contains("No Leidas"));
        component.FindAll(".card-text").Should().Contain(t => t.TextContent.Contains("Leidas"));
        component.FindAll(".card-text").Should().Contain(t => t.TextContent.Contains("Hoy"));
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveFilterOptions()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll("option").Should().HaveCountGreaterThan(0);
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Todos los tipos"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Todas"));
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveNotificationCards()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new NotificacionDto
            {
                Id = Guid.NewGuid(),
                Titulo = "Nueva orden recibida",
                Mensaje = "Se ha recibido una nueva orden en la mesa 5",
                Tipo = "Informativa",
                FechaCreacion = DateTime.Now,
                EstaLeida = false,
                EntidadRelacionadaId = Guid.NewGuid()
            }
        };

        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync(notificaciones);

        // Act
        var component = RenderComponent<Notificaciones>();
        component.WaitForAssertion(() => component.FindAll(".card").Count.Should().BeGreaterThan(0));

        // Assert
        component.FindAll(".card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void NotificacionesPage_ShouldHaveLoadingState()
    {
        // Arrange
        _notificacionesApiMock.Setup(x => x.ObtenerNotificacionesAsync())
            .ReturnsAsync((List<NotificacionDto>?)null);

        // Act
        var component = RenderComponent<Notificaciones>();

        // Assert
        component.FindAll(".spinner-border").Should().HaveCountGreaterThan(0);
        component.FindAll("p").Should().Contain(p => p.TextContent.Contains("Cargando notificaciones..."));
    }
}
