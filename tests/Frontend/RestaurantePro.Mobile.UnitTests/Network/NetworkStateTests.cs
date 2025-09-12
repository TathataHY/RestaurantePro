using FluentAssertions;
using Microsoft.Maui.Networking;
using Moq;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Network;

/// <summary>
/// Pruebas unitarias para manejo de estados de red
/// </summary>
public class NetworkStateTests
{
    #region Network Connectivity Tests

    [Fact]
    public void IsOnline_WhenConnected_ShouldReturnTrue()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.Internet);

        // Act
        var result = IsOnline(mockConnectivity.Object);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOnline_WhenDisconnected_ShouldReturnFalse()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.None);

        // Act
        var result = IsOnline(mockConnectivity.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(NetworkAccess.Internet, true)]
    [InlineData(NetworkAccess.Local, true)]
    [InlineData(NetworkAccess.ConstrainedInternet, true)]
    [InlineData(NetworkAccess.None, false)]
    [InlineData(NetworkAccess.Unknown, false)]
    public void IsOnline_WithVariousNetworkAccess_ShouldReturnExpectedResult(NetworkAccess networkAccess, bool expected)
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(networkAccess);

        // Act
        var result = IsOnline(mockConnectivity.Object);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Network Quality Tests

    [Fact]
    public void GetNetworkQuality_WhenInternet_ShouldReturnGood()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.Internet);

        // Act
        var result = GetNetworkQuality(mockConnectivity.Object);

        // Assert
        result.Should().Be("Good");
    }

    [Fact]
    public void GetNetworkQuality_WhenConstrainedInternet_ShouldReturnPoor()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.ConstrainedInternet);

        // Act
        var result = GetNetworkQuality(mockConnectivity.Object);

        // Assert
        result.Should().Be("Poor");
    }

    [Fact]
    public void GetNetworkQuality_WhenNoConnection_ShouldReturnNone()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.None);

        // Act
        var result = GetNetworkQuality(mockConnectivity.Object);

        // Assert
        result.Should().Be("None");
    }

    #endregion

    #region Offline Mode Tests

    [Fact]
    public void ShouldUseOfflineMode_WhenDisconnected_ShouldReturnTrue()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.None);

        // Act
        var result = ShouldUseOfflineMode(mockConnectivity.Object);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldUseOfflineMode_WhenConnected_ShouldReturnFalse()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.Internet);

        // Act
        var result = ShouldUseOfflineMode(mockConnectivity.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldUseOfflineMode_WhenConstrainedInternet_ShouldReturnTrue()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.ConstrainedInternet);

        // Act
        var result = ShouldUseOfflineMode(mockConnectivity.Object);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Network Retry Logic Tests

    [Fact]
    public void ShouldRetry_WhenNetworkError_ShouldReturnTrue()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");
        var retryCount = 0;
        var maxRetries = 3;

        // Act
        var result = ShouldRetry(exception, retryCount, maxRetries);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_WhenMaxRetriesReached_ShouldReturnFalse()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");
        var retryCount = 3;
        var maxRetries = 3;

        // Act
        var result = ShouldRetry(exception, retryCount, maxRetries);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_WhenNonRetryableException_ShouldReturnFalse()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        var retryCount = 0;
        var maxRetries = 3;

        // Act
        var result = ShouldRetry(exception, retryCount, maxRetries);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Network State Management Tests

    [Fact]
    public void NetworkState_WhenCreated_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.Internet);

        // Act
        var networkState = new NetworkState(mockConnectivity.Object);

        // Assert
        networkState.Should().NotBeNull();
        networkState.IsOnline.Should().BeTrue();
    }

    [Fact]
    public void NetworkState_WhenDisconnected_ShouldShowOffline()
    {
        // Arrange
        var mockConnectivity = new Mock<IConnectivity>();
        mockConnectivity.Setup(x => x.NetworkAccess).Returns(NetworkAccess.None);

        // Act
        var networkState = new NetworkState(mockConnectivity.Object);

        // Assert
        networkState.Should().NotBeNull();
        networkState.IsOnline.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static bool IsOnline(IConnectivity connectivity)
    {
        return connectivity.NetworkAccess == NetworkAccess.Internet || 
               connectivity.NetworkAccess == NetworkAccess.Local ||
               connectivity.NetworkAccess == NetworkAccess.ConstrainedInternet;
    }

    private static string GetNetworkQuality(IConnectivity connectivity)
    {
        return connectivity.NetworkAccess switch
        {
            NetworkAccess.Internet => "Good",
            NetworkAccess.Local => "Good",
            NetworkAccess.ConstrainedInternet => "Poor",
            NetworkAccess.None => "None",
            _ => "Unknown"
        };
    }

    private static bool ShouldUseOfflineMode(IConnectivity connectivity)
    {
        return connectivity.NetworkAccess == NetworkAccess.None ||
               connectivity.NetworkAccess == NetworkAccess.ConstrainedInternet;
    }

    private static bool ShouldRetry(Exception exception, int retryCount, int maxRetries)
    {
        if (retryCount >= maxRetries)
            return false;

        return exception switch
        {
            HttpRequestException => true,
            TaskCanceledException => true,
            System.Net.Sockets.SocketException => true,
            _ => false
        };
    }

    #endregion
}

/// <summary>
/// Clase helper para manejo de estados de red
/// </summary>
public class NetworkState
{
    private readonly IConnectivity _connectivity;

    public bool IsOnline { get; private set; }

    public NetworkState(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        IsOnline = IsOnlineCheck(connectivity);
    }

    private static bool IsOnlineCheck(IConnectivity connectivity)
    {
        return connectivity.NetworkAccess == NetworkAccess.Internet || 
               connectivity.NetworkAccess == NetworkAccess.Local ||
               connectivity.NetworkAccess == NetworkAccess.ConstrainedInternet;
    }
}