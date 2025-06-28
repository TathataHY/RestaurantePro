using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Commands.ExportarInventario;

public class ExportarInventarioCommandHandler : IRequestHandler<ExportarInventarioCommand, Result<ReporteExportadoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ExportarInventarioCommandHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<ReporteExportadoDto>> Handle(
        ExportarInventarioCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ingredientes = await _context.Ingredientes
                .Where(i => i.EstaActivo)
                .ToListAsync(cancellationToken);

            // Simular generación de archivo
            var contenidoArchivo = GenerarContenidoArchivo(ingredientes, request.Formato);
            var nombreArchivo = $"Inventario_{_dateTimeService.Now:yyyyMMdd_HHmmss}.{request.Formato.ToLower()}";

            var resultado = new ReporteExportadoDto
            {
                NombreArchivo = nombreArchivo,
                RutaArchivo = $"/descargas/{nombreArchivo}",
                Formato = request.Formato,
                TamañoBytes = contenidoArchivo.Length,
                FechaExportacion = _dateTimeService.Now,
                UrlDescarga = $"/api/descargas/{nombreArchivo}"
            };

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result.Failure<ReporteExportadoDto>($"Error al exportar inventario: {ex.Message}");
        }
    }

    private byte[] GenerarContenidoArchivo(List<Ingrediente> ingredientes, string formato)
    {
        // Simulación simple de generación de contenido
        var contenido = $"Reporte de Inventario - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
        contenido += $"Total de ingredientes: {ingredientes.Count}\n";
        contenido += $"Formato: {formato}\n";
        
        foreach (var ingrediente in ingredientes.Take(5)) // Solo primeros 5 para simulación
        {
            contenido += $"- {ingrediente.Nombre}: {ingrediente.Stock} {ingrediente.UnidadMedida}\n";
        }

        return System.Text.Encoding.UTF8.GetBytes(contenido);
    }

    private string ObtenerTipoContenido(string formato)
    {
        return formato.ToUpper() switch
        {
            "PDF" => "application/pdf",
            "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "CSV" => "text/csv",
            "JSON" => "application/json",
            _ => "application/octet-stream"
        };
    }
} 