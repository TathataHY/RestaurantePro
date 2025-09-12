using RestaurantePro.Mobile.Core.Services.Notifications;

namespace RestaurantePro.Mobile.UnitTests.Services.Notifications;

/// <summary>
/// Implementación mock del servicio de notificaciones para pruebas
/// </summary>
public class MockNotificationService : INotificationService
{
    public List<string> ToastMessages { get; } = new();
    public List<int> ToastDurations { get; } = new();
    public List<int> VibrationDurations { get; } = new();

    public async Task ShowToastAsync(string message, int durationMs = 2000)
    {
        ToastMessages.Add(message);
        ToastDurations.Add(durationMs);
        await Task.CompletedTask;
    }

    public async Task VibrateAsync(int milliseconds = 100)
    {
        VibrationDurations.Add(milliseconds);
        await Task.CompletedTask;
    }
}

public class NotificationServiceTests
{
    private readonly MockNotificationService _notificationService;

    public NotificationServiceTests()
    {
        _notificationService = new MockNotificationService();
    }

    #region ShowToastAsync Tests

    [Fact]
    public async Task ShowToastAsync_WithMessage_ShouldRecordMessage()
    {
        // Arrange
        var message = "Test toast message";

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Equal(message, _notificationService.ToastMessages[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithCustomDuration_ShouldRecordDuration()
    {
        // Arrange
        var message = "Test toast message";
        var duration = 3000;

        // Act
        await _notificationService.ShowToastAsync(message, duration);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(duration, _notificationService.ToastDurations[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithDefaultDuration_ShouldUseDefault()
    {
        // Arrange
        var message = "Test toast message";

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(2000, _notificationService.ToastDurations[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithEmptyMessage_ShouldRecordEmptyMessage()
    {
        // Arrange
        var message = "";

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Equal(message, _notificationService.ToastMessages[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithNullMessage_ShouldRecordNullMessage()
    {
        // Arrange
        string? message = null;

        // Act
        await _notificationService.ShowToastAsync(message!);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Null(_notificationService.ToastMessages[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithLongMessage_ShouldRecordMessage()
    {
        // Arrange
        var message = "This is a very long message that should be displayed in a toast notification to test the service functionality";

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Equal(message, _notificationService.ToastMessages[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithSpecialCharacters_ShouldRecordMessage()
    {
        // Arrange
        var message = "¡Hola! ¿Cómo estás? @#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Equal(message, _notificationService.ToastMessages[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithShortDuration_ShouldRecordDuration()
    {
        // Arrange
        var message = "Short duration toast";
        var duration = 500;

        // Act
        await _notificationService.ShowToastAsync(message, duration);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(duration, _notificationService.ToastDurations[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithLongDuration_ShouldRecordDuration()
    {
        // Arrange
        var message = "Long duration toast";
        var duration = 5000;

        // Act
        await _notificationService.ShowToastAsync(message, duration);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(duration, _notificationService.ToastDurations[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithZeroDuration_ShouldRecordDuration()
    {
        // Arrange
        var message = "Zero duration toast";
        var duration = 0;

        // Act
        await _notificationService.ShowToastAsync(message, duration);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(duration, _notificationService.ToastDurations[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithNegativeDuration_ShouldRecordDuration()
    {
        // Arrange
        var message = "Negative duration toast";
        var duration = -100;

        // Act
        await _notificationService.ShowToastAsync(message, duration);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(duration, _notificationService.ToastDurations[0]);
    }

    #endregion

    #region VibrateAsync Tests

    [Fact]
    public async Task VibrateAsync_WithDefaultDuration_ShouldRecordDuration()
    {
        // Act
        await _notificationService.VibrateAsync();

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(100, _notificationService.VibrationDurations[0]);
    }

    [Fact]
    public async Task VibrateAsync_WithCustomDuration_ShouldRecordDuration()
    {
        // Arrange
        var duration = 500;

        // Act
        await _notificationService.VibrateAsync(duration);

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(duration, _notificationService.VibrationDurations[0]);
    }

    [Fact]
    public async Task VibrateAsync_WithShortDuration_ShouldRecordDuration()
    {
        // Arrange
        var duration = 50;

        // Act
        await _notificationService.VibrateAsync(duration);

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(duration, _notificationService.VibrationDurations[0]);
    }

    [Fact]
    public async Task VibrateAsync_WithLongDuration_ShouldRecordDuration()
    {
        // Arrange
        var duration = 2000;

        // Act
        await _notificationService.VibrateAsync(duration);

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(duration, _notificationService.VibrationDurations[0]);
    }

    [Fact]
    public async Task VibrateAsync_WithZeroDuration_ShouldRecordDuration()
    {
        // Arrange
        var duration = 0;

        // Act
        await _notificationService.VibrateAsync(duration);

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(duration, _notificationService.VibrationDurations[0]);
    }

    [Fact]
    public async Task VibrateAsync_WithNegativeDuration_ShouldRecordDuration()
    {
        // Arrange
        var duration = -50;

        // Act
        await _notificationService.VibrateAsync(duration);

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(duration, _notificationService.VibrationDurations[0]);
    }

    #endregion

    #region Multiple Operations Tests

    [Fact]
    public async Task MultipleToastOperations_ShouldRecordAllMessages()
    {
        // Arrange
        var messages = new[] { "First message", "Second message", "Third message" };

        // Act
        foreach (var message in messages)
        {
            await _notificationService.ShowToastAsync(message);
        }

        // Assert
        Assert.Equal(3, _notificationService.ToastMessages.Count);
        Assert.Equal(messages, _notificationService.ToastMessages);
    }

    [Fact]
    public async Task MultipleVibrationOperations_ShouldRecordAllDurations()
    {
        // Arrange
        var durations = new[] { 100, 200, 300 };

        // Act
        foreach (var duration in durations)
        {
            await _notificationService.VibrateAsync(duration);
        }

        // Assert
        Assert.Equal(3, _notificationService.VibrationDurations.Count);
        Assert.Equal(durations, _notificationService.VibrationDurations);
    }

    [Fact]
    public async Task MixedOperations_ShouldRecordAllOperations()
    {
        // Act
        await _notificationService.ShowToastAsync("Test message 1", 1000);
        await _notificationService.VibrateAsync(200);
        await _notificationService.ShowToastAsync("Test message 2", 2000);
        await _notificationService.VibrateAsync(300);

        // Assert
        Assert.Equal(2, _notificationService.ToastMessages.Count);
        Assert.Equal(2, _notificationService.ToastDurations.Count);
        Assert.Equal(2, _notificationService.VibrationDurations.Count);
        
        Assert.Equal("Test message 1", _notificationService.ToastMessages[0]);
        Assert.Equal("Test message 2", _notificationService.ToastMessages[1]);
        Assert.Equal(1000, _notificationService.ToastDurations[0]);
        Assert.Equal(2000, _notificationService.ToastDurations[1]);
        Assert.Equal(200, _notificationService.VibrationDurations[0]);
        Assert.Equal(300, _notificationService.VibrationDurations[1]);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task ShowToastAsync_WithVeryLongMessage_ShouldRecordMessage()
    {
        // Arrange
        var message = new string('A', 1000); // 1000 caracteres

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Equal(message, _notificationService.ToastMessages[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithVeryLongDuration_ShouldRecordDuration()
    {
        // Arrange
        var message = "Very long duration";
        var duration = int.MaxValue;

        // Act
        await _notificationService.ShowToastAsync(message, duration);

        // Assert
        Assert.Single(_notificationService.ToastDurations);
        Assert.Equal(duration, _notificationService.ToastDurations[0]);
    }

    [Fact]
    public async Task VibrateAsync_WithVeryLongDuration_ShouldRecordDuration()
    {
        // Arrange
        var duration = int.MaxValue;

        // Act
        await _notificationService.VibrateAsync(duration);

        // Assert
        Assert.Single(_notificationService.VibrationDurations);
        Assert.Equal(duration, _notificationService.VibrationDurations[0]);
    }

    [Fact]
    public async Task ShowToastAsync_WithUnicodeCharacters_ShouldRecordMessage()
    {
        // Arrange
        var message = "🚀 ¡Hola! 🌟 Café ☕ 中文 🎉";

        // Act
        await _notificationService.ShowToastAsync(message);

        // Assert
        Assert.Single(_notificationService.ToastMessages);
        Assert.Equal(message, _notificationService.ToastMessages[0]);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task ConcurrentToastOperations_ShouldRecordAllMessages()
    {
        // Arrange
        var tasks = new List<Task>();
        var messages = new[] { "Message 1", "Message 2", "Message 3", "Message 4", "Message 5" };

        // Act
        foreach (var message in messages)
        {
            tasks.Add(_notificationService.ShowToastAsync(message));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, _notificationService.ToastMessages.Count);
        Assert.Contains("Message 1", _notificationService.ToastMessages);
        Assert.Contains("Message 2", _notificationService.ToastMessages);
        Assert.Contains("Message 3", _notificationService.ToastMessages);
        Assert.Contains("Message 4", _notificationService.ToastMessages);
        Assert.Contains("Message 5", _notificationService.ToastMessages);
    }

    [Fact]
    public async Task ConcurrentVibrationOperations_ShouldRecordAllDurations()
    {
        // Arrange
        var tasks = new List<Task>();
        var durations = new[] { 100, 200, 300, 400, 500 };

        // Act
        foreach (var duration in durations)
        {
            tasks.Add(_notificationService.VibrateAsync(duration));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, _notificationService.VibrationDurations.Count);
        Assert.Contains(100, _notificationService.VibrationDurations);
        Assert.Contains(200, _notificationService.VibrationDurations);
        Assert.Contains(300, _notificationService.VibrationDurations);
        Assert.Contains(400, _notificationService.VibrationDurations);
        Assert.Contains(500, _notificationService.VibrationDurations);
    }

    #endregion
}
