using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Dialog;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Dialog;

/// <summary>
/// Pruebas de integración básicas para el servicio de diálogos
/// </summary>
public class DialogServiceIntegrationTests
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<DialogServiceIntegrationTests> _logger;

    public DialogServiceIntegrationTests()
    {
        _logger = NullLogger<DialogServiceIntegrationTests>.Instance;
        _dialogService = new MockDialogService();
    }

    [Fact]
    public async Task ShowAlertAsync_WithValidParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Información";
        var message = "Operación completada exitosamente";

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _dialogService.ShowAlertAsync(title, message));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowAlertAsync_WithCustomCancelButton_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Confirmación";
        var message = "¿Desea continuar?";
        var cancelButton = "Cancelar";

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _dialogService.ShowAlertAsync(title, message, cancelButton));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowConfirmAsync_WithValidParameters_ShouldReturnExpectedResult()
    {
        // Arrange
        var title = "Confirmación";
        var message = "¿Está seguro de eliminar este elemento?";
        var mockService = (MockDialogService)_dialogService;
        mockService.NextConfirmationResult = true;

        // Act
        var result = await _dialogService.ShowConfirmAsync(title, message);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShowConfirmAsync_WithCustomButtons_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Confirmación";
        var message = "¿Desea guardar los cambios?";
        var acceptButton = "Guardar";
        var cancelButton = "No guardar";
        var mockService = (MockDialogService)_dialogService;
        mockService.NextConfirmationResult = false;

        // Act
        var result = await _dialogService.ShowConfirmAsync(title, message, acceptButton, cancelButton);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShowErrorAsync_WithValidMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var errorMessage = "Ha ocurrido un error inesperado";

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _dialogService.ShowErrorAsync(errorMessage));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowSuccessAsync_WithValidMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var successMessage = "Operación completada con éxito";

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _dialogService.ShowSuccessAsync(successMessage));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ShowActionSheetAsync_WithValidParameters_ShouldReturnExpectedResult()
    {
        // Arrange
        var title = "Opciones";
        var message = "Seleccione una opción";
        var cancelButton = "Cancelar";
        var buttons = new[] { "Opción 1", "Opción 2", "Opción 3" };
        var mockService = (MockDialogService)_dialogService;
        mockService.NextActionSheetResult = "Opción 2";

        // Act
        var result = await _dialogService.ShowActionSheetAsync(title, message, cancelButton, buttons);

        // Assert
        Assert.Equal("Opción 2", result);
    }

    [Fact]
    public async Task ShowActionSheetAsync_WithNoButtons_ShouldReturnCancelButton()
    {
        // Arrange
        var title = "Opciones";
        var message = "No hay opciones disponibles";
        var cancelButton = "Cancelar";

        // Act
        var result = await _dialogService.ShowActionSheetAsync(title, message, cancelButton);

        // Assert
        Assert.Equal(cancelButton, result);
    }

    [Fact]
    public async Task ShowPromptAsync_WithValidParameters_ShouldReturnExpectedResult()
    {
        // Arrange
        var title = "Entrada de datos";
        var message = "Ingrese su nombre";
        var mockService = (MockDialogService)_dialogService;
        mockService.NextPromptResult = "Juan Pérez";

        // Act
        var result = await _dialogService.ShowPromptAsync(title, message);

        // Assert
        Assert.Equal("Juan Pérez", result);
    }

    [Fact]
    public async Task ShowPromptAsync_WithCustomParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Configuración";
        var message = "Ingrese el valor máximo";
        var acceptButton = "Aceptar";
        var cancelButton = "Cancelar";
        var placeholder = "Ej: 100";
        var maxLength = 10;
        var initialValue = "50";

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _dialogService.ShowPromptAsync(title, message, acceptButton, cancelButton, placeholder, maxLength, initialValue));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task DialogFlow_ShouldHandleMultipleOperations()
    {
        // Arrange
        var mockService = (MockDialogService)_dialogService;

        // Act
        await _dialogService.ShowAlertAsync("Info", "Iniciando operación");
        var confirmResult = await _dialogService.ShowConfirmAsync("Confirmar", "¿Continuar?");
        await _dialogService.ShowSuccessAsync("Operación completada");

        // Assert
        Assert.Single(mockService.AlertsShown);
        Assert.Single(mockService.ConfirmationsShown);
        Assert.Single(mockService.SuccessesShown);
        Assert.Contains("Info: Iniciando operación", mockService.AlertsShown);
        Assert.Contains("Confirmar: ¿Continuar?", mockService.ConfirmationsShown);
        Assert.Contains("Operación completada", mockService.SuccessesShown);
    }
} 