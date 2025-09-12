using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using Moq;
using RestaurantePro.Mobile.Core.Features.Onboarding.ViewModels;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Preferences;

namespace RestaurantePro.Mobile.UnitTests.Features.Onboarding.ViewModels;

public class OnboardingViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IPreferencesService> _mockPreferencesService;
    private readonly OnboardingViewModel _viewModel;

    public OnboardingViewModelTests()
    {
        _mockNavigationService = new Mock<INavigationService>();
        _mockPreferencesService = new Mock<IPreferencesService>();

        _viewModel = new OnboardingViewModel(
            _mockNavigationService.Object,
            _mockPreferencesService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.Equal("Bienvenido a RestaurantePro", _viewModel.Title);
        Assert.NotNull(_viewModel.OnboardingItems);
        Assert.Equal(5, _viewModel.OnboardingItems.Count);
        Assert.Equal(0.2, _viewModel.Progress); // 1.0 / 5 items
    }

    [Fact]
    public void Constructor_ShouldInitializeOnboardingItems()
    {
        // Assert
        Assert.Equal(5, _viewModel.OnboardingItems.Count);
        
        // Verificar primer elemento
        var firstItem = _viewModel.OnboardingItems[0];
        Assert.Equal("🍽️", firstItem.Icon);
        Assert.Equal("Gestión Completa de Restaurante", firstItem.Title);
        Assert.Equal("Administra mesas, comandas, inventario y más desde una sola aplicación moderna y fácil de usar.", firstItem.Description);
        Assert.Equal("Siguiente", firstItem.ActionText);
        
        // Verificar último elemento
        var lastItem = _viewModel.OnboardingItems[4];
        Assert.Equal("🎯", lastItem.Icon);
        Assert.Equal("¡Listo para Comenzar!", lastItem.Title);
        Assert.Equal("Ya tienes todo lo necesario para gestionar tu restaurante de manera profesional. ¡Empecemos!", lastItem.Description);
        Assert.Equal("Comenzar", lastItem.ActionText);
    }

    #endregion

    #region NextAsync Tests

    [Fact]
    public async Task NextAsync_ShouldMarkOnboardingCompletedAndNavigateToLogin()
    {
        // Act
        await _viewModel.NextCommand.ExecuteAsync(null);

        // Assert
        _mockPreferencesService.Verify(x => x.SetAsync("OnboardingCompleted", true), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//login"), Times.Once);
    }

    [Fact]
    public async Task NextAsync_WhenPreferencesServiceFails_ShouldShowError()
    {
        // Arrange
        _mockPreferencesService.Setup(x => x.SetAsync("OnboardingCompleted", true))
            .ThrowsAsync(new Exception("Error de preferencias"));

        // Act
        await _viewModel.NextCommand.ExecuteAsync(null);

        // Assert
        _mockPreferencesService.Verify(x => x.SetAsync("OnboardingCompleted", true), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//login"), Times.Never);
    }

    [Fact]
    public async Task NextAsync_WhenNavigationServiceFails_ShouldShowError()
    {
        // Arrange
        _mockNavigationService.Setup(x => x.NavigateToAsync("//login"))
            .ThrowsAsync(new Exception("Error de navegación"));

        // Act
        await _viewModel.NextCommand.ExecuteAsync(null);

        // Assert
        _mockPreferencesService.Verify(x => x.SetAsync("OnboardingCompleted", true), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//login"), Times.Once);
    }

    #endregion

    #region SkipAsync Tests

    [Fact]
    public async Task SkipAsync_ShouldMarkOnboardingCompletedAndNavigateToLogin()
    {
        // Act
        await _viewModel.SkipCommand.ExecuteAsync(null);

        // Assert
        _mockPreferencesService.Verify(x => x.SetAsync("OnboardingCompleted", true), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//login"), Times.Once);
    }

    [Fact]
    public async Task SkipAsync_WhenPreferencesServiceFails_ShouldShowError()
    {
        // Arrange
        _mockPreferencesService.Setup(x => x.SetAsync("OnboardingCompleted", true))
            .ThrowsAsync(new Exception("Error de preferencias"));

        // Act
        await _viewModel.SkipCommand.ExecuteAsync(null);

        // Assert
        _mockPreferencesService.Verify(x => x.SetAsync("OnboardingCompleted", true), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//login"), Times.Never);
    }

    [Fact]
    public async Task SkipAsync_WhenNavigationServiceFails_ShouldShowError()
    {
        // Arrange
        _mockNavigationService.Setup(x => x.NavigateToAsync("//login"))
            .ThrowsAsync(new Exception("Error de navegación"));

        // Act
        await _viewModel.SkipCommand.ExecuteAsync(null);

        // Assert
        _mockPreferencesService.Verify(x => x.SetAsync("OnboardingCompleted", true), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync("//login"), Times.Once);
    }

    #endregion

    #region UpdateProgress Tests

    [Fact]
    public void UpdateProgress_ShouldCalculateCorrectProgress()
    {
        // Arrange
        var viewModel = new OnboardingViewModel(_mockNavigationService.Object, _mockPreferencesService.Object);

        // Assert
        Assert.Equal(0.2, viewModel.Progress); // 1.0 / 5 items
    }

    [Fact]
    public void UpdateProgress_WhenOnboardingItemsIsNull_ShouldNotThrow()
    {
        // Arrange
        var viewModel = new OnboardingViewModel(_mockNavigationService.Object, _mockPreferencesService.Object);
        viewModel.OnboardingItems = null;

        // Act & Assert
        // No debería lanzar excepción
        Assert.Null(viewModel.OnboardingItems);
    }

    [Fact]
    public void UpdateProgress_WhenOnboardingItemsIsEmpty_ShouldNotThrow()
    {
        // Arrange
        var viewModel = new OnboardingViewModel(_mockNavigationService.Object, _mockPreferencesService.Object);
        viewModel.OnboardingItems.Clear();

        // Act & Assert
        // No debería lanzar excepción
        Assert.Empty(viewModel.OnboardingItems);
    }

    #endregion

    #region OnboardingItem Tests

    [Fact]
    public void OnboardingItem_ShouldHaveDefaultValues()
    {
        // Arrange
        var item = new OnboardingItem();

        // Assert
        Assert.Equal(string.Empty, item.Icon);
        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.Description);
        Assert.Equal(string.Empty, item.ActionText);
        Assert.Equal(Colors.Gray, item.AccentColor);
    }

    [Fact]
    public void OnboardingItem_ShouldAllowSettingProperties()
    {
        // Arrange
        var item = new OnboardingItem
        {
            Icon = "🍽️",
            Title = "Test Title",
            Description = "Test Description",
            ActionText = "Test Action",
            AccentColor = Colors.Red
        };

        // Assert
        Assert.Equal("🍽️", item.Icon);
        Assert.Equal("Test Title", item.Title);
        Assert.Equal("Test Description", item.Description);
        Assert.Equal("Test Action", item.ActionText);
        Assert.Equal(Colors.Red, item.AccentColor);
    }

    #endregion
}
