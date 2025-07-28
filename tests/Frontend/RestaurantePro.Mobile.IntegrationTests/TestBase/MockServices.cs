using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Notifications;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

public class MockEmailService : IEmailService
{
    public Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, string? from = null)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, List<string>? cc = null, List<string>? bcc = null)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, string? from = null, List<string>? cc = null, List<string>? bcc = null)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body)
    {
        return Task.FromResult(true);
    }
}

public class MockSignalRService : ISignalRService
{
    public Task NotificarNuevaComandaAsync(NuevaComandaNotificationDto notification)
    {
        return Task.CompletedTask;
    }

    public Task NotificarActualizacionComandaAsync(Guid comandaId, string estado, string? observaciones = null)
    {
        return Task.CompletedTask;
    }

    public Task NotificarPreparacionListaAsync(Guid preparacionId, string productoNombre)
    {
        return Task.CompletedTask;
    }

    public Task NotificarMesaDisponibleAsync(int mesaId, string ubicacion)
    {
        return Task.CompletedTask;
    }

    public Task NotificarNuevaReservacionAsync(Guid reservacionId, string clienteNombre, DateTime fecha)
    {
        return Task.CompletedTask;
    }

    public Task NotificarCambioInventarioAsync(string productoNombre, int cantidadAnterior, int cantidadNueva)
    {
        return Task.CompletedTask;
    }

    public Task NotificarAlertaStockAsync(string productoNombre, int stockActual, int stockMinimo)
    {
        return Task.CompletedTask;
    }

    public Task NotificarNuevoClienteAsync(Guid clienteId, string clienteNombre)
    {
        return Task.CompletedTask;
    }

    public Task NotificarNuevaFacturaAsync(Guid facturaId, decimal monto, string clienteNombre)
    {
        return Task.CompletedTask;
    }

    public Task NotificarErrorSistemaAsync(string error, string contexto)
    {
        return Task.CompletedTask;
    }

    public Task NotificarEventoSistemaAsync(string evento, object datos)
    {
        return Task.CompletedTask;
    }

    public Task NotificarUsuarioAsync(string userId, string metodo, object mensaje)
    {
        return Task.CompletedTask;
    }

    public Task NotificarGrupoAsync(string grupo, string metodo, object mensaje)
    {
        return Task.CompletedTask;
    }

    public Task EnviarNotificacionAUsuarioAsync(Guid userId, string titulo, string mensaje, string tipo)
    {
        return Task.CompletedTask;
    }

    public Task EnviarNotificacionAUsuariosAsync(List<Guid> userIds, string titulo, string mensaje, string tipo)
    {
        return Task.CompletedTask;
    }

    public Task EnviarNotificacionARolAsync(string rol, string titulo, string mensaje, string tipo)
    {
        return Task.CompletedTask;
    }

    public Task EnviarNotificacionGlobalAsync(string titulo, string mensaje, string tipo)
    {
        return Task.CompletedTask;
    }

    public Task ActualizarEstadoMesaAsync(Guid mesaId, string estado, object datos)
    {
        return Task.CompletedTask;
    }

    public Task ActualizarEstadoComandaAsync(Guid comandaId, string estado, object datos)
    {
        return Task.CompletedTask;
    }

    public Task EnviarAlertaInventarioAsync(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo)
    {
        return Task.CompletedTask;
    }

    public Task<List<Guid>> ObtenerUsuariosConectadosAsync()
    {
        return Task.FromResult(new List<Guid>());
    }

    public Task<bool> UsuarioEstaConectadoAsync(Guid userId)
    {
        return Task.FromResult(false);
    }
}

public class MockDelayProvider : IDelayProvider
{
    public Task DelayAsync(TimeSpan delay)
    {
        return Task.CompletedTask;
    }

    public Task DelayAsync(int millisecondsDelay)
    {
        return Task.CompletedTask;
    }

    public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class MockFileStorageService : IFileStorageService
{
    public Task<string> SaveFileAsync(byte[] fileData, string fileName, string folderPath = "")
    {
        return Task.FromResult($"mock_path/{fileName}");
    }

    public Task<byte[]> GetFileAsync(string filePath)
    {
        return Task.FromResult(new byte[] { 1, 2, 3, 4 });
    }

    public Task DeleteFileAsync(string filePath)
    {
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(true);
    }

    public Task<string> GetFileUrlAsync(string filePath)
    {
        return Task.FromResult($"https://mock-storage.com/{filePath}");
    }

    public Task<List<string>> GetFilesInFolderAsync(string folderPath)
    {
        return Task.FromResult(new List<string> { "file1.txt", "file2.pdf" });
    }

    public Task<long> GetFileSizeAsync(string filePath)
    {
        return Task.FromResult(1024L);
    }

    public Task<string> GetFileExtensionAsync(string filePath)
    {
        return Task.FromResult(".txt");
    }

    public Task<string> GetFileNameAsync(string filePath)
    {
        return Task.FromResult("mock_file.txt");
    }

    public Task<string> GetFileNameWithoutExtensionAsync(string filePath)
    {
        return Task.FromResult("mock_file");
    }

    public Task<Result<string>> SubirArchivoAsync(DatosArchivo datosArchivo, string carpetaDestino, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Success($"mock_path/{datosArchivo.NombreArchivo}"));
    }

    public Task<Result<DatosArchivo>> DescargarArchivoAsync(string rutaArchivo, CancellationToken cancellationToken = default)
    {
        var datosArchivo = new DatosArchivo
        {
            NombreArchivo = "mock_file.txt",
            Contenido = new byte[] { 1, 2, 3, 4 },
            TipoMime = "text/plain"
        };
        return Task.FromResult(Result<DatosArchivo>.Success(datosArchivo));
    }

    public Task<Result<bool>> EliminarArchivoAsync(string rutaArchivo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<string>> ObtenerUrlTemporalAsync(string rutaArchivo, TimeSpan duracion, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Success($"https://mock-storage.com/temp/{rutaArchivo}"));
    }
}

public class MockSMSService : RestaurantePro.Application.Common.Interfaces.ISMSService
{
    public Task<bool> SendSMSAsync(string phoneNumber, string message)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendSMSWithTrackingAsync(string phoneNumber, string message, Guid? clienteId = null, string? tipoNotificacion = null)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message)
    {
        return Task.FromResult(true);
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        return true;
    }

    public Task<string> GetDeliveryStatusAsync(string messageId)
    {
        return Task.FromResult("Delivered");
    }
} 