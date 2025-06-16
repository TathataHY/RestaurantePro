using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Define el contrato para un servicio de reportes
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Genera un reporte diario de ventas
    /// </summary>
    /// <param name="fecha">Fecha para el reporte</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del reporte generado</returns>
    Task<ReportInfo> GenerarReporteDiarioVentasAsync(DateTime fecha, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera un reporte diario de inventario
    /// </summary>
    /// <param name="fecha">Fecha para el reporte</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del reporte generado</returns>
    Task<ReportInfo> GenerarReporteDiarioInventarioAsync(DateTime fecha, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera un reporte semanal de ventas
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio del período</param>
    /// <param name="fechaFin">Fecha de fin del período</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del reporte generado</returns>
    Task<ReportInfo> GenerarReporteSemanalVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera un reporte mensual de ventas
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio del período</param>
    /// <param name="fechaFin">Fecha de fin del período</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del reporte generado</returns>
    Task<ReportInfo> GenerarReporteMensualVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera un reporte mensual de operaciones
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio del período</param>
    /// <param name="fechaFin">Fecha de fin del período</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del reporte generado</returns>
    Task<ReportInfo> GenerarReporteMensualOperacionesAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
} 