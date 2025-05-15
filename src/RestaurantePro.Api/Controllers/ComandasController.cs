using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Features.Comandas.Commands.ActualizarComanda;
using RestaurantePro.Application.Features.Comandas.Commands.AgregarProductoComanda;
using RestaurantePro.Application.Features.Comandas.Commands.CrearComanda;
using RestaurantePro.Application.Features.Comandas.Queries.ObtenerComandaPorId;
using RestaurantePro.Application.Features.Comandas.Queries.ObtenerComandas;
using RestaurantePro.Application.Features.Inventario.Dtos;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequiereEmpleado")]
    public class ComandasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IInventarioComandaService _inventarioService;
        private readonly IApplicationDbContext _context;

        public ComandasController(
            IMediator mediator,
            IInventarioComandaService inventarioService,
            IApplicationDbContext context)
        {
            _mediator = mediator;
            _inventarioService = inventarioService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerComandas([FromQuery] ObtenerComandasQuery query)
        {
            var comandas = await _mediator.Send(query);
            return Ok(comandas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerComandaPorId(int id)
        {
            var query = new ObtenerComandaPorIdQuery { Id = id };
            var comanda = await _mediator.Send(query);

            if (comanda == null)
            {
                return NotFound();
            }

            return Ok(comanda);
        }

        [HttpPost]
        public async Task<IActionResult> CrearComanda([FromBody] CrearComandaCommand command)
        {
            // Verificar disponibilidad de ingredientes antes de crear la comanda
            if (command.DetallesComanda != null && command.DetallesComanda.Count > 0)
            {
                // Convertir detalles de la comanda al formato esperado por el servicio
                var detallesProductos = command.DetallesComanda
                    .Select(d => new DetalleProductoDto
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad
                    })
                    .ToList();

                // Verificar disponibilidad
                var faltantes = await _inventarioService.VerificarDisponibilidadIngredientes(detallesProductos);
                
                // Si hay ingredientes faltantes, devolver advertencia
                if (faltantes.Any())
                {
                    // Obtener nombres de ingredientes faltantes
                    var ingredientesIds = faltantes.Keys.ToList();
                    var ingredientes = await _context.Ingredientes
                        .Where(i => ingredientesIds.Contains(i.Id))
                        .ToDictionaryAsync(i => i.Id, i => i.Nombre);
                    
                    var mensajesFaltantes = faltantes
                        .Select(f => $"{ingredientes.GetValueOrDefault(f.Key, "Ingrediente desconocido")}: {f.Value} unidades")
                        .ToList();
                    
                    return BadRequest(new
                    {
                        Error = "Inventario insuficiente para algunos ingredientes",
                        IngredientesFaltantes = mensajesFaltantes
                    });
                }
            }

            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObtenerComandaPorId), new { id }, id);
        }

        [HttpPost("{id}/agregar-producto")]
        public async Task<ActionResult> AgregarProducto(int id, [FromBody] AgregarProductoComandaCommand command)
        {
            if (id != command.ComandaId)
            {
                return BadRequest("El ID de la comanda no coincide");
            }
            
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarComanda(int id, [FromBody] ActualizarComandaCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID no coincide");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/procesar-inventario")]
        [Authorize(Policy = "RequiereGerente")]
        public async Task<IActionResult> ProcesarInventario(int id)
        {
            try
            {
                var resultado = await _inventarioService.ActualizarInventarioPorComanda(id);
                return Ok(new { Message = "Inventario procesado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("mesa/{mesaId}")]
        public async Task<ActionResult> ObtenerComandasPorMesa(int mesaId)
        {
            var query = new ObtenerComandasQuery
            {
                MesaId = mesaId,
                SoloActivas = true
            };
            
            var comandas = await _mediator.Send(query);
            return Ok(comandas);
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult> ObtenerComandasPendientes()
        {
            var query = new ObtenerComandasQuery { Estado = EstadoComanda.Pendiente };
            var comandas = await _mediator.Send(query);
            return Ok(comandas);
        }

        [HttpGet("preparacion")]
        public async Task<ActionResult> ObtenerComandasEnPreparacion()
        {
            var query = new ObtenerComandasQuery { Estado = EstadoComanda.EnPreparacion };
            var comandas = await _mediator.Send(query);
            return Ok(comandas);
        }

        [HttpGet("listas")]
        public async Task<ActionResult> ObtenerComandasListas()
        {
            var query = new ObtenerComandasQuery { Estado = EstadoComanda.Lista };
            var comandas = await _mediator.Send(query);
            return Ok(comandas);
        }
    }
} 