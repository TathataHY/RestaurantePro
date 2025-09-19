using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestaurantePro.Mobile.Core.Core.Attributes;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authorization;
using System.Reflection;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Core.Services.Authorization;

/// <summary>
/// Tests unitarios para el validador de autorización
/// Verifica la validación de atributos de autorización en clases y métodos
/// </summary>
public class AuthorizationValidatorTests
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly AuthorizationValidator _validator;

    public AuthorizationValidatorTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _validator = new AuthorizationValidator(
            _mockAuthorizationService.Object,
            NullLogger<AuthorizationValidator>.Instance);
    }

    #region Clases de Test con Atributos

    [RequirePermission(AppPermission.CrearComandas)]
    private class TestViewModelWithPermission
    {
        public void SomeMethod() { }
    }

    [RequireFeature(AppFeature.GestionComandas)]
    private class TestViewModelWithFeature
    {
        public void SomeMethod() { }
    }

    [RequireRole("Mesero")]
    private class TestViewModelWithRole
    {
        public void SomeMethod() { }
    }

    [RequirePermission(AppPermission.CrearComandas, AppPermission.VerComandas, RequireAllPermissions = true)]
    private class TestViewModelWithAllPermissions
    {
        public void SomeMethod() { }
    }

    private class TestViewModelWithMethods
    {
        [RequirePermission(AppPermission.GenerarFacturas)]
        public void MethodWithPermission() { }

        [RequireFeature(AppFeature.Caja)]
        public void MethodWithFeature() { }

        [RequireRole("Cajero")]
        public void MethodWithRole() { }

        public void MethodWithoutAttributes() { }
    }

    private class TestViewModelNoAttributes
    {
        public void SomeMethod() { }
    }

    #endregion

    #region Tests de Validación de Clases

    [Fact]
    public async Task ValidateClassAsync_WithPermissionAttribute_UserHasPermission_ShouldReturnAllowed()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(It.IsAny<AppPermission[]>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithPermission));

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateClassAsync_WithPermissionAttribute_UserLacksPermission_ShouldReturnDenied()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(It.IsAny<AppPermission[]>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithPermission));

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Contains("No tiene los permisos requeridos", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateClassAsync_WithFeatureAttribute_UserCanAccess_ShouldReturnAllowed()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.CanAccessFeatureAsync(AppFeature.GestionComandas))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithFeature));

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateClassAsync_WithFeatureAttribute_UserCannotAccess_ShouldReturnDenied()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.CanAccessFeatureAsync(AppFeature.GestionComandas))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithFeature));

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Contains("No tiene acceso a la funcionalidad", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateClassAsync_WithRoleAttribute_UserHasRole_ShouldReturnAllowed()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Mesero"))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithRole));

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateClassAsync_WithRoleAttribute_UserLacksRole_ShouldReturnDenied()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Mesero"))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithRole));

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Contains("No tiene los roles requeridos", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateClassAsync_WithAllPermissionsRequired_UserHasAll_ShouldReturnAllowed()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasAllPermissionsAsync(It.IsAny<AppPermission[]>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithAllPermissions));

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateClassAsync_WithAllPermissionsRequired_UserLacksSome_ShouldReturnDenied()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasAllPermissionsAsync(It.IsAny<AppPermission[]>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithAllPermissions));

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateClassAsync_NoAttributes_ShouldReturnAllowed()
    {
        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelNoAttributes));

        // Assert
        Assert.True(result.IsAllowed);
    }

    #endregion

    #region Tests de Validación de Métodos

    [Fact]
    public async Task ValidateMethodAsync_WithPermissionAttribute_UserHasPermission_ShouldReturnAllowed()
    {
        // Arrange
        var method = typeof(TestViewModelWithMethods).GetMethod("MethodWithPermission");
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(It.IsAny<AppPermission[]>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateMethodAsync(method!);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateMethodAsync_WithFeatureAttribute_UserCanAccess_ShouldReturnAllowed()
    {
        // Arrange
        var method = typeof(TestViewModelWithMethods).GetMethod("MethodWithFeature");
        _mockAuthorizationService.Setup(x => x.CanAccessFeatureAsync(AppFeature.Caja))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateMethodAsync(method!);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateMethodAsync_WithRoleAttribute_UserHasRole_ShouldReturnAllowed()
    {
        // Arrange
        var method = typeof(TestViewModelWithMethods).GetMethod("MethodWithRole");
        _mockAuthorizationService.Setup(x => x.HasRoleAsync("Cajero"))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateMethodAsync(method!);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateMethodAsync_NoAttributes_ShouldReturnAllowed()
    {
        // Arrange
        var method = typeof(TestViewModelWithMethods).GetMethod("MethodWithoutAttributes");

        // Act
        var result = await _validator.ValidateMethodAsync(method!);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateMethodAsync_WithPermissionAttribute_UserLacksPermission_ShouldReturnDenied()
    {
        // Arrange
        var method = typeof(TestViewModelWithMethods).GetMethod("MethodWithPermission");
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(It.IsAny<AppPermission[]>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateMethodAsync(method!);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Contains("No tiene los permisos requeridos", result.ErrorMessage);
    }

    #endregion

    #region Tests de Error Handling

    [Fact]
    public async Task ValidateClassAsync_AuthServiceThrows_ShouldReturnDenied()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(It.IsAny<AppPermission[]>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _validator.ValidateClassAsync(typeof(TestViewModelWithPermission));

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal("Error interno de autorización", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateMethodAsync_AuthServiceThrows_ShouldReturnDenied()
    {
        // Arrange
        var method = typeof(TestViewModelWithMethods).GetMethod("MethodWithPermission");
        _mockAuthorizationService.Setup(x => x.HasAnyPermissionAsync(It.IsAny<AppPermission[]>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _validator.ValidateMethodAsync(method!);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal("Error interno de autorización", result.ErrorMessage);
    }

    #endregion
}
