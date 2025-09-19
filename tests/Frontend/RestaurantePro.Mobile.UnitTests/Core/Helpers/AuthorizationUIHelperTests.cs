using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authorization;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Core.Helpers;

/// <summary>
/// Tests unitarios para el helper de autorización UI
/// Verifica la lógica de visibilidad y habilitación de elementos UI
/// </summary>
public class AuthorizationUIHelperTests
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly AuthorizationUIHelper _uiHelper;

    public AuthorizationUIHelperTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _uiHelper = new AuthorizationUIHelper(
            _mockAuthorizationService.Object,
            NullLogger<AuthorizationUIHelper>.Instance);
    }

    #region Tests de Visibilidad por Permisos

    [Fact]
    public async Task IsVisibleAsync_UserHasPermission_ShouldReturnTrue()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasPermissionAsync(AppPermission.CrearComandas))
            .ReturnsAsync(true);

        // Act
        var result = await _uiHelper.IsVisibleAsync(AppPermission.CrearComandas);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVisibleAsync_UserLacksPermission_ShouldReturnFalse()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasPermissionAsync(AppPermission.CrearComandas))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleAsync(AppPermission.CrearComandas);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsVisibleWithAnyPermissionAsync_UserHasOnePermission_ShouldReturnTrue()
    {
        // Arrange
        var permissions = new[] { AppPermission.CrearComandas, AppPermission.GenerarFacturas };
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(permissions))
            .ReturnsAsync(true);

        // Act
        var result = await _uiHelper.IsVisibleWithAnyPermissionAsync(permissions);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVisibleWithAnyPermissionAsync_UserLacksAllPermissions_ShouldReturnFalse()
    {
        // Arrange
        var permissions = new[] { AppPermission.CrearComandas, AppPermission.GenerarFacturas };
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(permissions))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleWithAnyPermissionAsync(permissions);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsVisibleWithAllPermissionsAsync_UserHasAllPermissions_ShouldReturnTrue()
    {
        // Arrange
        var permissions = new[] { AppPermission.VerComandas, AppPermission.ModificarComandas };
        _mockAuthorizationService.Setup(x => x.HasAllPermissionsAsync(permissions))
            .ReturnsAsync(true);

        // Act
        var result = await _uiHelper.IsVisibleWithAllPermissionsAsync(permissions);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVisibleWithAllPermissionsAsync_UserLacksSomePermissions_ShouldReturnFalse()
    {
        // Arrange
        var permissions = new[] { AppPermission.VerComandas, AppPermission.ModificarComandas };
        _mockAuthorizationService.Setup(x => x.HasAllPermissionsAsync(permissions))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleWithAllPermissionsAsync(permissions);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Tests de Visibilidad por Funcionalidades

    [Fact]
    public async Task IsVisibleAsync_UserCanAccessFeature_ShouldReturnTrue()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.CanAccessFeatureAsync(AppFeature.GestionComandas))
            .ReturnsAsync(true);

        // Act
        var result = await _uiHelper.IsVisibleAsync(AppFeature.GestionComandas);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVisibleAsync_UserCannotAccessFeature_ShouldReturnFalse()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.CanAccessFeatureAsync(AppFeature.GestionComandas))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleAsync(AppFeature.GestionComandas);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Tests de Visibilidad por Roles

    [Fact]
    public async Task IsVisibleForRoleAsync_UserHasRole_ShouldReturnTrue()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Mesero"))
            .ReturnsAsync(true);

        // Act
        var result = await _uiHelper.IsVisibleForRoleAsync("Mesero");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVisibleForRoleAsync_UserLacksRole_ShouldReturnFalse()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Mesero"))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleForRoleAsync("Mesero");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsVisibleForAnyRoleAsync_UserHasOneRole_ShouldReturnTrue()
    {
        // Arrange
        var roles = new[] { "Mesero", "Cajero" };
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Mesero"))
            .ReturnsAsync(true);
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Cajero"))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleForAnyRoleAsync(roles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVisibleForAnyRoleAsync_UserLacksAllRoles_ShouldReturnFalse()
    {
        // Arrange
        var roles = new[] { "Mesero", "Cajero" };
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Mesero"))
            .ReturnsAsync(false);
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Cajero"))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsVisibleForAnyRoleAsync(roles);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Tests de Habilitación (IsEnabled)

    [Fact]
    public async Task IsEnabledAsync_Permission_ShouldDelegateToIsVisible()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasPermissionAsync(AppPermission.GenerarFacturas))
            .ReturnsAsync(true);

        // Act
        var result = await _uiHelper.IsEnabledAsync(AppPermission.GenerarFacturas);

        // Assert
        Assert.True(result);
        _mockAuthorizationService.Verify(x => x.HasPermissionAsync(AppPermission.GenerarFacturas), Times.Once);
    }

    [Fact]
    public async Task IsEnabledAsync_Feature_ShouldDelegateToIsVisible()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.CanAccessFeatureAsync(AppFeature.Caja))
            .ReturnsAsync(false);

        // Act
        var result = await _uiHelper.IsEnabledAsync(AppFeature.Caja);

        // Assert
        Assert.False(result);
        _mockAuthorizationService.Verify(x => x.CanAccessFeatureAsync(AppFeature.Caja), Times.Once);
    }

    #endregion

    #region Tests de Mensajes de Acceso Denegado

    [Fact]
    public void GetAccessDeniedMessage_Permission_ShouldReturnAppropriateMessage()
    {
        // Act & Assert
        var createMessage = _uiHelper.GetAccessDeniedMessage(AppPermission.CrearComandas);
        Assert.Equal("No tiene permisos para crear comandas", createMessage);

        var updateMessage = _uiHelper.GetAccessDeniedMessage(AppPermission.ActualizarEstadoPreparaciones);
        Assert.Equal("No tiene permisos para actualizar preparaciones", updateMessage);

        var paymentMessage = _uiHelper.GetAccessDeniedMessage(AppPermission.ProcesarPagos);
        Assert.Equal("No tiene permisos para procesar pagos", paymentMessage);
    }

    [Fact]
    public void GetAccessDeniedMessage_Feature_ShouldReturnAppropriateMessage()
    {
        // Act & Assert
        var comandasMessage = _uiHelper.GetAccessDeniedMessage(AppFeature.GestionComandas);
        Assert.Equal("No tiene acceso a la gestión de comandas", comandasMessage);

        var cocinaMessage = _uiHelper.GetAccessDeniedMessage(AppFeature.Cocina);
        Assert.Equal("No tiene acceso a las funciones de cocina", cocinaMessage);

        var supervisionMessage = _uiHelper.GetAccessDeniedMessage(AppFeature.Supervision);
        Assert.Equal("No tiene acceso a las funciones de supervisión", supervisionMessage);
    }

    [Fact]
    public void GetAccessDeniedMessage_UnknownPermission_ShouldReturnGenericMessage()
    {
        // Arrange
        var unknownPermission = (AppPermission)999;

        // Act
        var message = _uiHelper.GetAccessDeniedMessage(unknownPermission);

        // Assert
        Assert.Contains("No tiene permisos para realizar esta acción", message);
    }

    [Fact]
    public void GetAccessDeniedMessage_UnknownFeature_ShouldReturnGenericMessage()
    {
        // Arrange
        var unknownFeature = (AppFeature)999;

        // Act
        var message = _uiHelper.GetAccessDeniedMessage(unknownFeature);

        // Assert
        Assert.Contains("No tiene acceso a esta funcionalidad", message);
    }

    #endregion

    #region Tests de Error Handling

    [Fact]
    public async Task IsVisibleAsync_AuthServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasPermissionAsync(It.IsAny<AppPermission>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _uiHelper.IsVisibleAsync(AppPermission.CrearComandas);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsVisibleForRoleAsync_AuthServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasRoleAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _uiHelper.IsVisibleForRoleAsync("Mesero");

        // Assert
        Assert.False(result);
    }

    #endregion
}
