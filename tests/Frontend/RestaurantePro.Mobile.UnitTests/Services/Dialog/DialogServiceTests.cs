using RestaurantePro.Mobile.Core.Services.Dialog;

namespace RestaurantePro.Mobile.UnitTests.Services.Dialog;

public class DialogServiceTests
{
    private readonly DialogService _dialogService;

    public DialogServiceTests()
    {
        _dialogService = new DialogService();
    }

    #region ShowAlertAsync Tests

    [Fact]
    public async Task ShowAlertAsync_WithTitleAndMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Test Alert";
        var message = "This is a test message";

        // Act & Assert
        await _dialogService.ShowAlertAsync(title, message);
        // No exception should be thrown
    }

    [Fact]
    public async Task ShowAlertAsync_WithCustomCancelButton_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Test Alert";
        var message = "This is a test message";
        var cancel = "Close";

        // Act & Assert
        await _dialogService.ShowAlertAsync(title, message, cancel);
        // No exception should be thrown
    }

    [Fact]
    public async Task ShowAlertAsync_WithEmptyStrings_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "";
        var message = "";

        // Act & Assert
        await _dialogService.ShowAlertAsync(title, message);
        // No exception should be thrown
    }

    [Fact]
    public async Task ShowAlertAsync_WithNullStrings_ShouldCompleteSuccessfully()
    {
        // Arrange
        string? title = null;
        string? message = null;

        // Act & Assert
        await _dialogService.ShowAlertAsync(title!, message!);
        // No exception should be thrown
    }

    #endregion

    #region ShowConfirmAsync Tests

    [Fact]
    public async Task ShowConfirmAsync_WithTitleAndMessage_ShouldReturnTrue()
    {
        // Arrange
        var title = "Test Confirm";
        var message = "Are you sure?";

        // Act
        var result = await _dialogService.ShowConfirmAsync(title, message);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShowConfirmAsync_WithCustomButtons_ShouldReturnTrue()
    {
        // Arrange
        var title = "Test Confirm";
        var message = "Are you sure?";
        var accept = "Yes";
        var cancel = "No";

        // Act
        var result = await _dialogService.ShowConfirmAsync(title, message, accept, cancel);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShowConfirmAsync_WithEmptyStrings_ShouldReturnTrue()
    {
        // Arrange
        var title = "";
        var message = "";

        // Act
        var result = await _dialogService.ShowConfirmAsync(title, message);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ShowErrorAsync Tests

    [Fact]
    public async Task ShowErrorAsync_WithMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = "This is an error message";

        // Act & Assert
        await _dialogService.ShowErrorAsync(message);
        // No exception should be thrown
    }

    [Fact]
    public async Task ShowErrorAsync_WithEmptyMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = "";

        // Act & Assert
        await _dialogService.ShowErrorAsync(message);
        // No exception should be thrown
    }

    [Fact]
    public async Task ShowErrorAsync_WithNullMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        string? message = null;

        // Act & Assert
        await _dialogService.ShowErrorAsync(message!);
        // No exception should be thrown
    }

    #endregion

    #region ShowSuccessAsync Tests

    [Fact]
    public async Task ShowSuccessAsync_WithMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = "Operation completed successfully";

        // Act & Assert
        await _dialogService.ShowSuccessAsync(message);
        // No exception should be thrown
    }

    [Fact]
    public async Task ShowSuccessAsync_WithEmptyMessage_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = "";

        // Act & Assert
        await _dialogService.ShowSuccessAsync(message);
        // No exception should be thrown
    }

    #endregion

    #region ShowActionSheetAsync Tests

    [Fact]
    public async Task ShowActionSheetAsync_WithButtons_ShouldReturnCancel()
    {
        // Arrange
        var title = "Test Action Sheet";
        var message = "Choose an option";
        var cancel = "Cancel";
        var buttons = new[] { "Option 1", "Option 2", "Option 3" };

        // Act
        var result = await _dialogService.ShowActionSheetAsync(title, message, cancel, buttons);

        // Assert
        Assert.Equal(cancel, result);
    }

    [Fact]
    public async Task ShowActionSheetAsync_WithNoButtons_ShouldReturnCancel()
    {
        // Arrange
        var title = "Test Action Sheet";
        var message = "Choose an option";
        var cancel = "Cancel";
        var buttons = new string[0];

        // Act
        var result = await _dialogService.ShowActionSheetAsync(title, message, cancel, buttons);

        // Assert
        Assert.Equal(cancel, result);
    }

    [Fact]
    public async Task ShowActionSheetAsync_WithEmptyStrings_ShouldReturnCancel()
    {
        // Arrange
        var title = "";
        var message = "";
        var cancel = "";

        // Act
        var result = await _dialogService.ShowActionSheetAsync(title, message, cancel);

        // Assert
        Assert.Equal(cancel, result);
    }

    #endregion

    #region ShowPromptAsync Tests

    [Fact]
    public async Task ShowPromptAsync_WithTitleAndMessage_ShouldReturnTestInput()
    {
        // Arrange
        var title = "Test Prompt";
        var message = "Enter some text";

        // Act
        var result = await _dialogService.ShowPromptAsync(title, message);

        // Assert
        Assert.Equal("Test Input", result);
    }

    [Fact]
    public async Task ShowPromptAsync_WithAllParameters_ShouldReturnTestInput()
    {
        // Arrange
        var title = "Test Prompt";
        var message = "Enter some text";
        var accept = "OK";
        var cancel = "Cancel";
        var placeholder = "Enter text here";
        var maxLength = 100;
        var initialValue = "Initial";

        // Act
        var result = await _dialogService.ShowPromptAsync(title, message, accept, cancel, placeholder, maxLength, initialValue);

        // Assert
        Assert.Equal("Test Input", result);
    }

    [Fact]
    public async Task ShowPromptAsync_WithEmptyStrings_ShouldReturnTestInput()
    {
        // Arrange
        var title = "";
        var message = "";

        // Act
        var result = await _dialogService.ShowPromptAsync(title, message);

        // Assert
        Assert.Equal("Test Input", result);
    }

    #endregion

    #region ShowConfirmationAsync Tests

    [Fact]
    public async Task ShowConfirmationAsync_WithTitleAndMessage_ShouldReturnTrue()
    {
        // Arrange
        var title = "Test Confirmation";
        var message = "Are you sure?";

        // Act
        var result = await _dialogService.ShowConfirmationAsync(title, message);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithCustomButtons_ShouldReturnTrue()
    {
        // Arrange
        var title = "Test Confirmation";
        var message = "Are you sure?";
        var accept = "Yes";
        var cancel = "No";

        // Act
        var result = await _dialogService.ShowConfirmationAsync(title, message, accept, cancel);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Multiple Operations Tests

    [Fact]
    public async Task MultipleDialogOperations_ShouldCompleteSuccessfully()
    {
        // Arrange
        var title = "Test";
        var message = "Test message";

        // Act & Assert
        await _dialogService.ShowAlertAsync(title, message);
        var confirmResult = await _dialogService.ShowConfirmAsync(title, message);
        await _dialogService.ShowErrorAsync("Error message");
        await _dialogService.ShowSuccessAsync("Success message");
        var actionResult = await _dialogService.ShowActionSheetAsync(title, message, "Cancel", "Option 1");
        var promptResult = await _dialogService.ShowPromptAsync(title, message);
        var confirmationResult = await _dialogService.ShowConfirmationAsync(title, message);

        // Assert
        Assert.True(confirmResult);
        Assert.Equal("Cancel", actionResult);
        Assert.Equal("Test Input", promptResult);
        Assert.True(confirmationResult);
    }

    #endregion
}
