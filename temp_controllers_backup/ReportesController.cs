using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteOcupacion;
using RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteReservaciones;
using RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteVentas;
using System;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequiereGerente")]
    public class ReportesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("ventas")]
        public async Task<IActionResult> ObtenerReporteVentas([FromQuery] ObtenerReporteVentasQuery query)
        {
            var reporte = await _mediator.Send(query);
            return Ok(reporte);
        }

        [HttpGet("ocupacion")]
        public async Task<IActionResult> ObtenerReporteOcupacion([FromQuery] ObtenerReporteOcupacionQuery query)
        {
            var reporte = await _mediator.Send(query);
            return Ok(reporte);
        }

        [HttpGet("reservaciones")]
        public async Task<IActionResult> ObtenerReporteReservaciones([FromQuery] ObtenerReporteReservacionesQuery query)
        {
            var reporte = await _mediator.Send(query);
            return Ok(reporte);
        }
        
        [HttpGet("dashboard")]
        public async Task<IActionResult> ObtenerDashboard([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            // Definir fechas por defecto si no se proporcionan
            var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            var fin = fechaFin ?? DateTime.Now;
            
            // Consultar los tres reportes en paralelo
            var reporteVentasTask = _mediator.Send(new ObtenerReporteVentasQuery 
            { 
                FechaInicio = inicio, 
                FechaFin = fin 
            });
            
            var reporteOcupacionTask = _mediator.Send(new ObtenerReporteOcupacionQuery 
            { 
                FechaInicio = inicio, 
                FechaFin = fin 
            });
            
            var reporteReservacionesTask = _mediator.Send(new ObtenerReporteReservacionesQuery 
            { 
                FechaInicio = inicio, 
                FechaFin = fin 
            });
            
            // Esperar a que se completen todas las tareas
            await Task.WhenAll(reporteVentasTask, reporteOcupacionTask, reporteReservacionesTask);
            
            // Construir el resultado combinado
            var dashboard = new
            {
                FechaInicio = inicio,
                FechaFin = fin,
                Ventas = reporteVentasTask.Result,
                Ocupacion = reporteOcupacionTask.Result,
                Reservaciones = reporteReservacionesTask.Result
            };
            
            return Ok(dashboard);
        }
    }
} 