using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Inventario.Reportes.Queries.ExportarReporte;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ExportarReporte;

/// <summary>
/// Handler para exportar reportes de inventario
/// </summary>
public class ExportarReporteQueryHandler : IRequestHandler<ExportarReporteQuery, Result<ReporteExportadoDto>>
{
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ExportarReporteQueryHandler> _logger;

    public ExportarReporteQueryHandler(
        IDateTimeService dateTimeService,
        ILogger<ExportarReporteQueryHandler> logger)
    {
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<ReporteExportadoDto>> Handle(
        ExportarReporteQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Exportando reporte de inventario en formato {Formato}", request.Formato);

            // Generar nombre de archivo único
            var timestamp = _dateTimeService.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"reporte_inventario_{timestamp}.{request.Formato.ToLower()}";

            // Simular contenido del reporte (en una implementación real, aquí se generaría el archivo)
            var contenidoArchivo = System.Text.Encoding.UTF8.GetBytes($"Reporte de Inventario - {_dateTimeService.Now:dd/MM/yyyy HH:mm:ss}");

            var resultado = new ReporteExportadoDto
            {
                NombreArchivo = nombreArchivo,
                RutaArchivo = $"/descargas/{nombreArchivo}",
                Formato = request.Formato,
                TamañoBytes = contenidoArchivo.LongLength,
                FechaExportacion = _dateTimeService.Now,
                UrlDescarga = $"/api/descargas/{nombreArchivo}"
            };

            _logger.LogInformation("Reporte exportado exitosamente: {NombreArchivo}", nombreArchivo);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte de inventario");
            return Result.Failure<ReporteExportadoDto>($"Error al exportar reporte: {ex.Message}");
        }
    }
} 