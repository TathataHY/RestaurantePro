using Microsoft.Extensions.Logging.Abstractions;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Authorization;
using Xunit;
using Moq;

namespace RestaurantePro.Mobile.UnitTests.Core.Services.Authorization;

/// <summary>
/// Tests unitarios para el servicio de autorización
/// Verifica el mapeo correcto de roles a permisos según la documentación del proyecto
/// </summary>
public class AuthorizationServiceTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthorizationService _authorizationService;

    public AuthorizationServiceTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _authorizationService = new AuthorizationService(
            _mockAuthService.Object,
            NullLogger<AuthorizationService>.Instance);
    }

    #region Tests de Roles - Mesero

    [Fact]
    public async Task Mesero_ShouldHave_ComandaPermissions()
    {
        // Arrange
        var meseroUser = new AuthUser
        {
            Id = 1,
            Email = "mesero@test.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Roles = new List<string> { "Mesero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(meseroUser);

        // Act & Assert - Permisos de comandas (funcionalidad principal del mesero)
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CrearComandas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.VerComandas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ModificarComandas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CerrarComandas));
    }

    [Fact]
    public async Task Mesero_ShouldHave_MesaPermissions()
    {
        // Arrange
        var meseroUser = new AuthUser
        {
            Id = 1,
            Email = "mesero@test.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Roles = new List<string> { "Mesero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(meseroUser);

        // Act & Assert - Permisos de mesas (funcionalidad principal del mesero)
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.VerEstadoMesas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CambiarEstadoMesas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.AsignarMesas));
    }

    [Fact]
    public async Task Mesero_ShouldNOTHave_CocinaPermissions()
    {
        // Arrange
        var meseroUser = new AuthUser
        {
            Id = 1,
            Email = "mesero@test.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Roles = new List<string> { "Mesero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(meseroUser);

        // Act & Assert - NO debe tener permisos de cocina
        Assert.False(await _authorizationService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones));
        Assert.False(await _authorizationService.HasPermissionAsync(AppPermission.CompletarPreparaciones));
    }

    #endregion

    #region Tests de Roles - Cocinero

    [Fact]
    public async Task Cocinero_ShouldHave_PreparacionPermissions()
    {
        // Arrange
        var cocineroUser = new AuthUser
        {
            Id = 2,
            Email = "cocinero@test.com",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            Roles = new List<string> { "Cocinero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cocineroUser);

        // Act & Assert - Permisos de preparación (funcionalidad principal del cocinero)
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.VerPreparacionesPendientes));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CompletarPreparaciones));
    }

    [Fact]
    public async Task Cocinero_ShouldHave_InventarioPermissions()
    {
        // Arrange
        var cocineroUser = new AuthUser
        {
            Id = 2,
            Email = "cocinero@test.com",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            Roles = new List<string> { "Cocinero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cocineroUser);

        // Act & Assert - Permisos de inventario (para verificar ingredientes)
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ConsultarDisponibilidadIngredientes));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.VerAlertasInventario));
    }

    [Fact]
    public async Task Cocinero_ShouldNOTHave_FacturacionPermissions()
    {
        // Arrange
        var cocineroUser = new AuthUser
        {
            Id = 2,
            Email = "cocinero@test.com",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            Roles = new List<string> { "Cocinero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cocineroUser);

        // Act & Assert - NO debe tener permisos de facturación
        Assert.False(await _authorizationService.HasPermissionAsync(AppPermission.GenerarFacturas));
        Assert.False(await _authorizationService.HasPermissionAsync(AppPermission.ProcesarPagos));
    }

    #endregion

    #region Tests de Roles - Cajero

    [Fact]
    public async Task Cajero_ShouldHave_FacturacionPermissions()
    {
        // Arrange
        var cajeroUser = new AuthUser
        {
            Id = 3,
            Email = "cajero@test.com",
            Nombre = "Ana",
            Apellido = "López",
            Roles = new List<string> { "Cajero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cajeroUser);

        // Act & Assert - Permisos de facturación (funcionalidad principal del cajero)
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.GenerarFacturas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ProcesarPagos));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.AplicarPromociones));
    }

    [Fact]
    public async Task Cajero_ShouldHave_ClientePermissions()
    {
        // Arrange
        var cajeroUser = new AuthUser
        {
            Id = 3,
            Email = "cajero@test.com",
            Nombre = "Ana",
            Apellido = "López",
            Roles = new List<string> { "Cajero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cajeroUser);

        // Act & Assert - Permisos de clientes (para facturación)
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ConsultarClientes));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.UsarTarjetasFidelizacion));
    }

    [Fact]
    public async Task Cajero_ShouldNOTHave_MesaManagementPermissions()
    {
        // Arrange
        var cajeroUser = new AuthUser
        {
            Id = 3,
            Email = "cajero@test.com",
            Nombre = "Ana",
            Apellido = "López",
            Roles = new List<string> { "Cajero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cajeroUser);

        // Act & Assert - NO debe tener permisos de gestión de mesas
        Assert.False(await _authorizationService.HasPermissionAsync(AppPermission.CambiarEstadoMesas));
        Assert.False(await _authorizationService.HasPermissionAsync(AppPermission.AsignarMesas));
    }

    #endregion

    #region Tests de Roles - Gerente

    [Fact]
    public async Task Gerente_ShouldHave_AllOperationalPermissions()
    {
        // Arrange
        var gerenteUser = new AuthUser
        {
            Id = 4,
            Email = "gerente@test.com",
            Nombre = "Luis",
            Apellido = "García",
            Roles = new List<string> { "Gerente" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(gerenteUser);

        // Act & Assert - Debe tener acceso a todas las funcionalidades operativas
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CrearComandas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.GenerarFacturas));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.SupervisarOperaciones));
    }

    [Fact]
    public async Task Gerente_ShouldHave_SupervisionPermissions()
    {
        // Arrange
        var gerenteUser = new AuthUser
        {
            Id = 4,
            Email = "gerente@test.com",
            Nombre = "Luis",
            Apellido = "García",
            Roles = new List<string> { "Gerente" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(gerenteUser);

        // Act & Assert - Permisos específicos de supervisión
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.VerReportesBasicos));
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.GestionarPersonalTurno));
    }

    #endregion

    #region Tests de Funcionalidades

    [Fact]
    public async Task Mesero_CanAccess_GestionComandas()
    {
        // Arrange
        var meseroUser = new AuthUser
        {
            Id = 1,
            Email = "mesero@test.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Roles = new List<string> { "Mesero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(meseroUser);

        // Act & Assert
        Assert.True(await _authorizationService.CanAccessFeatureAsync(AppFeature.GestionComandas));
        Assert.True(await _authorizationService.CanAccessFeatureAsync(AppFeature.GestionMesas));
    }

    [Fact]
    public async Task Cocinero_CanAccess_Cocina()
    {
        // Arrange
        var cocineroUser = new AuthUser
        {
            Id = 2,
            Email = "cocinero@test.com",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            Roles = new List<string> { "Cocinero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cocineroUser);

        // Act & Assert
        Assert.True(await _authorizationService.CanAccessFeatureAsync(AppFeature.Cocina));
        Assert.True(await _authorizationService.CanAccessFeatureAsync(AppFeature.ConsultaInventario));
    }

    [Fact]
    public async Task Cajero_CanAccess_Caja()
    {
        // Arrange
        var cajeroUser = new AuthUser
        {
            Id = 3,
            Email = "cajero@test.com",
            Nombre = "Ana",
            Apellido = "López",
            Roles = new List<string> { "Cajero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cajeroUser);

        // Act & Assert
        Assert.True(await _authorizationService.CanAccessFeatureAsync(AppFeature.Caja));
        Assert.True(await _authorizationService.CanAccessFeatureAsync(AppFeature.AtencionCliente));
    }

    [Fact]
    public async Task Cocinero_CanNOTAccess_Supervision()
    {
        // Arrange
        var cocineroUser = new AuthUser
        {
            Id = 2,
            Email = "cocinero@test.com",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            Roles = new List<string> { "Cocinero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(cocineroUser);

        // Act & Assert
        Assert.False(await _authorizationService.CanAccessFeatureAsync(AppFeature.Supervision));
    }

    #endregion

    #region Tests de Múltiples Roles

    [Fact]
    public async Task User_WithMultipleRoles_ShouldHave_CombinedPermissions()
    {
        // Arrange
        var multiRoleUser = new AuthUser
        {
            Id = 5,
            Email = "multi@test.com",
            Nombre = "María",
            Apellido = "Sánchez",
            Roles = new List<string> { "Mesero", "Cajero" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(multiRoleUser);

        // Act & Assert - Debe tener permisos de ambos roles
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CrearComandas)); // Mesero
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.GenerarFacturas)); // Cajero
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.CambiarEstadoMesas)); // Mesero
        Assert.True(await _authorizationService.HasPermissionAsync(AppPermission.ProcesarPagos)); // Cajero
    }

    #endregion

    #region Tests de Error Handling

    [Fact]
    public async Task AuthService_ReturnsNull_ShouldReturnEmptyPermissions()
    {
        // Arrange
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync((AuthUser?)null);

        // Act
        var permissions = await _authorizationService.GetUserPermissionsAsync();

        // Assert
        Assert.Empty(permissions);
    }

    [Fact]
    public async Task User_WithUnknownRole_ShouldHaveOnlyBasicPermissions()
    {
        // Arrange
        var unknownRoleUser = new AuthUser
        {
            Id = 6,
            Email = "unknown@test.com",
            Nombre = "Test",
            Apellido = "User",
            Roles = new List<string> { "RolInexistente" }
        };
        
        _mockAuthService.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(unknownRoleUser);

        // Act
        var permissions = await _authorizationService.GetUserPermissionsAsync();

        // Assert
        Assert.Empty(permissions); // No debería tener permisos con rol inexistente
    }

    #endregion
}
