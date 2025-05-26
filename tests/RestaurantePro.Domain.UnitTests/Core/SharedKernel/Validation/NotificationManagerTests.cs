namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Validation;

public class NotificationManagerTests
{
    private readonly NotificationManager _notificationManager;
    
    public NotificationManagerTests()
    {
        _notificationManager = new NotificationManager();
    }
    
    [Fact]
    public void CrearNuevo_DebeRetornarNotificacionSinErrores()
    {
        // Act
        var notification = _notificationManager.CreateNewNotification();
        
        // Assert
        Assert.NotNull(notification);
        Assert.False(notification.HasErrors);
        Assert.Empty(notification.Errors);
    }
    
    [Fact]
    public void AddError_DebeAgregarErrorCorrectamente()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        _notificationManager.AddError("Error de prueba", "ERR001", "Propiedad");
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error de prueba", errors[0].Message);
        Assert.Equal("ERR001", errors[0].Code);
        Assert.Equal("Propiedad", errors[0].PropertyName);
    }
    
    [Fact]
    public void AddErrors_DebeAgregarMultiplesErroresCorrectamente()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        var errors = new List<Error>
        {
            new Error("Error 1", "ERR001", "Prop1"),
            new Error("Error 2", "ERR002", "Prop2")
        };
        
        // Act
        _notificationManager.AddErrors(errors);
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var resultErrors = _notificationManager.GetErrors();
        Assert.Equal(2, resultErrors.Count);
        Assert.Equal("Error 1", resultErrors[0].Message);
        Assert.Equal("Error 2", resultErrors[1].Message);
    }
    
    [Fact]
    public void AddErrorsFromResult_DebeAgregarErroresDesdeResultadoFallido()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        var result = Result.Failure("Error desde Result");
        
        // Act
        _notificationManager.AddErrorsFromResult(result);
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error desde Result", errors[0].Message);
    }
    
    [Fact]
    public void AddErrorsFromResult_DebeAgregarMultiplesErroresDesdeResultadoFallido()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        var errorList = new List<string> { "Error 1", "Error 2" };
        var result = Result.Failure(errorList);
        
        // Act
        _notificationManager.AddErrorsFromResult(result);
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Equal(2, errors.Count);
        Assert.Equal("Error 1", errors[0].Message);
        Assert.Equal("Error 2", errors[1].Message);
    }
    
    [Fact]
    public void ClearErrors_DebeLimpiarTodosLosErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        _notificationManager.AddError("Error de prueba");
        
        // Act
        _notificationManager.ClearErrors();
        
        // Assert
        Assert.False(_notificationManager.HasErrors);
        Assert.Empty(_notificationManager.GetErrors());
    }
    
    [Fact]
    public void ToResult_DebeRetornarResultadoExitosoSiNoHayErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = _notificationManager.ToResult();
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.HasErrors);
    }
    
    [Fact]
    public void ToResult_DebeRetornarResultadoFallidoSiHayErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        _notificationManager.AddError("Error de prueba");
        
        // Act
        var result = _notificationManager.ToResult();
        
        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.HasErrors);
        Assert.Equal("Error de prueba", result.Errors![0]);
    }
    
    [Fact]
    public void ToResultT_DebeRetornarResultadoExitosoConValorSiNoHayErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        var valor = "Test Value";
        
        // Act
        var result = _notificationManager.ToResult(valor);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.HasErrors);
        Assert.Equal(valor, result.Value);
    }
    
    [Fact]
    public void ToResultT_DebeRetornarResultadoFallidoSiHayErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        _notificationManager.AddError("Error de prueba");
        var valor = "Test Value";
        
        // Act
        var result = _notificationManager.ToResult(valor);
        
        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.HasErrors);
        Assert.Equal("Error de prueba", result.Errors![0]);
        Assert.Equal(default, result.Value);
    }
} 