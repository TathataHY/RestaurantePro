using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteClientes;

public class ObtenerReporteClientesHandler : IRequestHandler<ObtenerReporteClientesQuery, Result<ReporteClientesDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerReporteClientesHandler> _logger;

    public ObtenerReporteClientesHandler(IApplicationDbContext context, ILogger<ObtenerReporteClientesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ReporteClientesDto>> Handle(ObtenerReporteClientesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("👥 Generando reporte de clientes");

            // Validaciones de entrada
            if (request.FechaRegistroDesde.HasValue && request.FechaRegistroHasta.HasValue)
            {
                if (request.FechaRegistroDesde.Value > request.FechaRegistroHasta.Value)
                {
                    return Result.Failure<ReporteClientesDto>("La fecha de registro desde debe ser anterior o igual a la fecha hasta");
                }
            }

            if (request.FechaRegistroDesde.HasValue && request.FechaRegistroDesde.Value > DateTime.Now)
            {
                return Result.Failure<ReporteClientesDto>("La fecha de registro desde no puede ser futura");
            }

            // Validar segmento si se proporciona
            if (!string.IsNullOrEmpty(request.Segmento))
            {
                var segmentosValidos = new[] { "Premium", "Regular", "VIP", "Nuevo" };
                if (!segmentosValidos.Contains(request.Segmento, StringComparer.OrdinalIgnoreCase))
                {
                    return Result.Failure<ReporteClientesDto>($"El segmento '{request.Segmento}' no es válido. Segmentos válidos: {string.Join(", ", segmentosValidos)}");
                }
            }

            // Query base de clientes
            var clientesQuery = _context.Clientes.AsQueryable();

            if (request.FechaRegistroDesde.HasValue)
            {
                clientesQuery = clientesQuery.Where(c => c.FechaCreacion >= request.FechaRegistroDesde.Value);
            }

            if (request.FechaRegistroHasta.HasValue)
            {
                clientesQuery = clientesQuery.Where(c => c.FechaCreacion <= request.FechaRegistroHasta.Value);
            }

            // Filtrar por segmento si se proporciona
            if (!string.IsNullOrEmpty(request.Segmento))
            {
                if (Enum.TryParse<SegmentoCliente>(request.Segmento, true, out var segmentoFiltro))
                {
                    clientesQuery = clientesQuery.Where(c => c.Segmento == segmentoFiltro);
                }
                else
                {
                    // Si el segmento no es válido, retornar lista vacía
                    return Result.Success(new ReporteClientesDto
                    {
                        TotalClientes = 0,
                        ClientesActivos = 0,
                        ClientesInactivos = 0
                    });
                }
            }

            // Filtrar por clientes activos si se solicita
            if (request.SoloActivos)
            {
                clientesQuery = clientesQuery.Where(c => c.EstaActivo);
            }

            var clientes = await clientesQuery.ToListAsync(cancellationToken);

            if (!clientes.Any())
            {
                return Result.Success(new ReporteClientesDto
                {
                    TotalClientes = 0,
                    ClientesActivos = 0,
                    ClientesInactivos = 0
                });
            }

            var totalClientes = clientes.Count;
            var clientesActivos = clientes.Count(c => c.EstaActivo);
            var clientesInactivos = clientes.Count(c => !c.EstaActivo);

            // Segmentación por edad
            var clientesPorEdad = clientes
                .GroupBy(c => CalcularGrupoEdad(c.FechaNacimiento))
                .Select(g => new SegmentoEdadDto
                {
                    GrupoEdad = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = totalClientes > 0 ? (double)g.Count() / totalClientes * 100 : 0
                })
                .OrderBy(s => s.GrupoEdad)
                .ToList();

            // Top clientes por puntos
            var topClientesPuntos = clientes
                .OrderByDescending(c => c.PuntosAcumulados)
                .Take(10)
                .Select(c => new ClienteTopPuntosDto
                {
                    ClienteId = c.Id,
                    NombreCompleto = c.Nombre.NombreCompleto,
                    PuntosAcumulados = c.PuntosAcumulados,
                    CantidadVisitas = c.CantidadVisitas
                })
                .ToList();

            // Top clientes por visitas
            var topClientesVisitas = clientes
                .OrderByDescending(c => c.CantidadVisitas)
                .Take(10)
                .Select(c => new ClienteTopVisitasDto
                {
                    ClienteId = c.Id,
                    NombreCompleto = c.Nombre.NombreCompleto,
                    CantidadVisitas = c.CantidadVisitas,
                    PuntosAcumulados = c.PuntosAcumulados
                })
                .ToList();

            // Distribución por segmento
            var distribucionSegmento = clientes
                .GroupBy(c => c.Segmento)
                .Select(g => new SegmentoClienteDto
                {
                    Segmento = g.Key.ToString(),
                    Cantidad = g.Count(),
                    Porcentaje = totalClientes > 0 ? (double)g.Count() / totalClientes * 100 : 0
                })
                .OrderByDescending(s => s.Cantidad)
                .ToList();

            // Nuevos clientes por mes (últimos 12 meses)
            var fechaLimite = DateTime.Now.AddMonths(-12);
            var nuevosClientesPorMes = clientes
                .Where(c => c.FechaCreacion >= fechaLimite)
                .GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
                .Select(g => new NuevosClientesMesDto
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    NombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                    Cantidad = g.Count()
                })
                .OrderBy(n => n.Año)
                .ThenBy(n => n.Mes)
                .ToList();

            var reporte = new ReporteClientesDto
            {
                FechaGeneracion = DateTime.Now,
                TotalClientes = totalClientes,
                ClientesActivos = clientesActivos,
                ClientesInactivos = clientesInactivos,
                PorcentajeActivos = totalClientes > 0 ? (double)clientesActivos / totalClientes * 100 : 0,
                ClientesPorEdad = clientesPorEdad,
                TopClientesPuntos = topClientesPuntos,
                TopClientesVisitas = topClientesVisitas,
                DistribucionSegmento = distribucionSegmento,
                NuevosClientesPorMes = nuevosClientesPorMes
            };

            _logger.LogInformation("✅ Reporte de clientes generado exitosamente. Total: {TotalClientes}, Activos: {ClientesActivos}", 
                totalClientes, clientesActivos);

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de clientes");
            return Result.Failure<ReporteClientesDto>("Error al generar reporte de clientes");
        }
    }

    private string CalcularGrupoEdad(DateTime fechaNacimiento)
    {
        var edad = DateTime.Now.Year - fechaNacimiento.Year;
        if (edad < 18)
        {
            return "Menor de 18 años";
        }
        else if (edad < 30)
        {
            return "De 18 a 29 años";
        }
        else if (edad < 40)
        {
            return "De 30 a 39 años";
        }
        else if (edad < 50)
        {
            return "De 40 a 49 años";
        }
        else if (edad < 60)
        {
            return "De 50 a 59 años";
        }
        else
        {
            return "60 años o más";
        }
    }
} 