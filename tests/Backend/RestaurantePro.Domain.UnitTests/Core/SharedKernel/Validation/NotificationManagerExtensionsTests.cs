namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Validation;

public class NotificationManagerExtensionsTests
{
    private readonly NotificationManager _notificationManager;
    
    public NotificationManagerExtensionsTests()
    {
        _notificationManager = new NotificationManager();
    }
    
    [Fact]
    public void Require_NoDebeAgregarError_CuandoCondicionEsVerdadera()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = _notificationManager.Require(true, "Este error no debería agregarse");
        
        // Assert
        Assert.False(_notificationManager.HasErrors);
        Assert.Empty(_notificationManager.GetErrors());
        Assert.Same(_notificationManager, result); // Verifica que devuelve la misma instancia
    }
    
    [Fact]
    public void Require_DebeAgregarError_CuandoCondicionEsFalsa()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = _notificationManager.Require(false, "Error de prueba", "ERR001", "Propiedad");
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error de prueba", errors[0].Message);
        Assert.Equal("ERR001", errors[0].Code);
        Assert.Equal("Propiedad", errors[0].PropertyName);
        Assert.Same(_notificationManager, result); // Verifica que devuelve la misma instancia
    }
    
    [Fact]
    public void RequireNotNull_NoDebeAgregarError_CuandoValorNoEsNulo()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = _notificationManager.RequireNotNull("valor", "Este error no debería agregarse");
        
        // Assert
        Assert.False(_notificationManager.HasErrors);
        Assert.Empty(_notificationManager.GetErrors());
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public void RequireNotNull_DebeAgregarError_CuandoValorEsNulo()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        string? valor = null;
        
        // Act
        var result = _notificationManager.RequireNotNull(valor, "Error de prueba", "ERR001", "Propiedad");
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error de prueba", errors[0].Message);
        Assert.Equal("ERR001", errors[0].Code);
        Assert.Equal("Propiedad", errors[0].PropertyName);
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public void RequireNotEmpty_NoDebeAgregarError_CuandoCadenaNoEsVacia()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = _notificationManager.RequireNotEmpty("valor", "Este error no debería agregarse");
        
        // Assert
        Assert.False(_notificationManager.HasErrors);
        Assert.Empty(_notificationManager.GetErrors());
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public void RequireNotEmpty_DebeAgregarError_CuandoCadenaEsVacia()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act - Probar con cadena vacía
        var result1 = _notificationManager.RequireNotEmpty("", "Error cadena vacía", "ERR001", "Propiedad");
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors1 = _notificationManager.GetErrors();
        Assert.Single(errors1);
        Assert.Equal("Error cadena vacía", errors1[0].Message);
        
        // Limpiar errores
        _notificationManager.ClearErrors();
        
        // Act - Probar con cadena nula
        string? valor = null;
        var result2 = _notificationManager.RequireNotEmpty(valor, "Error cadena nula", "ERR002", "Propiedad");
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors2 = _notificationManager.GetErrors();
        Assert.Single(errors2);
        Assert.Equal("Error cadena nula", errors2[0].Message);
    }
    
    [Fact]
    public void RequireSuccess_NoDebeAgregarError_CuandoResultadoEsExitoso()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        var result = Result.Success();
        
        // Act
        var managerResult = _notificationManager.RequireSuccess(result);
        
        // Assert
        Assert.False(_notificationManager.HasErrors);
        Assert.Empty(_notificationManager.GetErrors());
        Assert.Same(_notificationManager, managerResult);
    }
    
    [Fact]
    public void RequireSuccess_DebeAgregarError_CuandoResultadoEsFallido()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        var result = Result.Failure("Error desde Result");
        
        // Act
        var managerResult = _notificationManager.RequireSuccess(result);
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error desde Result", errors[0].Message);
        Assert.Same(_notificationManager, managerResult);
    }
    
    [Fact]
    public void OnSuccess_DebeEjecutarAccion_CuandoNoHayErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        bool accionEjecutada = false;
        
        // Act
        var result = _notificationManager.OnSuccess(() => accionEjecutada = true);
        
        // Assert
        Assert.True(accionEjecutada);
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public void OnSuccess_NoDebeEjecutarAccion_CuandoHayErrores()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        _notificationManager.AddError("Error previo");
        bool accionEjecutada = false;
        
        // Act
        var result = _notificationManager.OnSuccess(() => accionEjecutada = true);
        
        // Assert
        Assert.False(accionEjecutada);
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public void OnSuccess_DebeEjecutarFuncion_YAgregarErrores_CuandoFuncionDevuelveResultadoFallido()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = _notificationManager.OnSuccess(() => Result.Failure("Error desde función"));
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error desde función", errors[0].Message);
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public async Task OnSuccessAsync_DebeEjecutarFuncionAsincrona_YAgregarErrores_CuandoFuncionDevuelveResultadoFallido()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        
        // Act
        var result = await _notificationManager.OnSuccessAsync(async () => {
            await Task.Delay(10); // Simular operación asíncrona
            return Result.Failure("Error asíncrono");
        });
        
        // Assert
        Assert.True(_notificationManager.HasErrors);
        var errors = _notificationManager.GetErrors();
        Assert.Single(errors);
        Assert.Equal("Error asíncrono", errors[0].Message);
        Assert.Same(_notificationManager, result);
    }
    
    [Fact]
    public async Task OnSuccessAsync_NoDebeEjecutarFuncionAsincrona_CuandoHayErroresPrevios()
    {
        // Arrange
        _notificationManager.CreateNewNotification();
        _notificationManager.AddError("Error previo");
        bool funcionEjecutada = false;
        
        // Act
        var result = await _notificationManager.OnSuccessAsync(async () => {
            funcionEjecutada = true;
            await Task.Delay(10);
            return Result.Success();
        });
        
        // Assert
        Assert.False(funcionEjecutada);
        Assert.Same(_notificationManager, result);
    }
} 