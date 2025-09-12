using System.Collections.ObjectModel;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Implementación de prueba para INotificationManager
/// </summary>
public class TestNotificationManager : INotificationManager
{
    private INotification _currentNotification;

    public TestNotificationManager()
    {
        _currentNotification = new Notification();
    }

    public INotification CurrentNotification => _currentNotification;

    public bool HasErrors => _currentNotification.HasErrors;

    public ReadOnlyCollection<Error> GetErrors()
    {
        return _currentNotification.Errors;
    }

    public void AddError(string errorMessage, string? errorCode = null, string? propertyName = null)
    {
        _currentNotification.AddError(errorMessage, errorCode, propertyName);
    }

    public void AddErrors(IEnumerable<Error> errors)
    {
        _currentNotification.AddErrors(errors);
    }

    public void AddErrors(INotification notification)
    {
        _currentNotification.AddErrors(notification);
    }

    public void AddErrorsFromResult(Result result)
    {
        if (!result.Succeeded)
        {
            var errors = result.Errors?.Select(e => new Error(e)) ?? new[] { new Error(result.Error ?? "Error desconocido") };
            _currentNotification.AddErrors(errors);
        }
    }

    public void ClearErrors()
    {
        _currentNotification.ClearErrors();
    }

    public INotification CreateNewNotification()
    {
        _currentNotification = new Notification();
        return _currentNotification;
    }

    public Result ToResult()
    {
        return _currentNotification.ToResult();
    }

    public Result<T> ToResult<T>(T value)
    {
        return _currentNotification.ToResult(value);
    }

    public void AddInformation(string message, string? code = null)
    {
        // En la implementación de prueba, no hacemos nada con la información
        // En una implementación real, se podría almacenar para logging o debugging
    }
}
