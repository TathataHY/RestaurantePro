using System;

namespace RestaurantePro.Application.Common.Models;

/// <summary>
/// Información sobre un reporte generado
/// </summary>
public class ReportInfo
{
    /// <summary>
    /// Identificador único del reporte
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Nombre del reporte
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de reporte (Diario, Semanal, Mensual, etc.)
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de inicio del período del reporte
    /// </summary>
    public DateTime FechaInicio { get; set; }
    
    /// <summary>
    /// Fecha de fin del período del reporte
    /// </summary>
    public DateTime FechaFin { get; set; }
    
    /// <summary>
    /// Fecha de generación del reporte
    /// </summary>
    public DateTime FechaGeneracion { get; set; }
    
    /// <summary>
    /// Formato del reporte (PDF, Excel, CSV, etc.)
    /// </summary>
    public string Formato { get; set; } = string.Empty;
    
    /// <summary>
    /// Ruta de almacenamiento del reporte
    /// </summary>
    public string RutaArchivo { get; set; } = string.Empty;
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long TamanoBytes { get; set; }
    
    /// <summary>
    /// Indica si el reporte está disponible para descarga
    /// </summary>
    public bool EstaDisponible { get; set; }
    
    /// <summary>
    /// Usuario que generó el reporte
    /// </summary>
    public string GeneradoPor { get; set; } = string.Empty;
} 