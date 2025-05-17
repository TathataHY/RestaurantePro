using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Productos.Commands.ActualizarProducto
{
    public class ActualizarProductoCommandHandler : IRequestHandler<ActualizarProductoCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ActualizarProductoCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> Handle(ActualizarProductoCommand request, CancellationToken cancellationToken)
        {
            var producto = await _context.Productos
                .Include(p => p.Ingredientes)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (producto == null)
                return false;

            // Actualizar propiedades básicas
            _mapper.Map(request, producto);
            producto.UltimaModificacion = DateTime.Now;

            // Gestionar los ingredientes
            if (request.Ingredientes != null)
            {
                // Ingredientes a eliminar (los que están en la BD pero no en la solicitud)
                var ingredientesAEliminar = producto.Ingredientes
                    .Where(ip => !request.Ingredientes.Any(i => i.Id.HasValue && i.Id.Value == ip.Id))
                    .ToList();

                foreach (var ingrediente in ingredientesAEliminar)
                {
                    _context.IngredientesProductos.Remove(ingrediente);
                }

                // Actualizar o agregar ingredientes
                foreach (var ingredienteInfo in request.Ingredientes)
                {
                    if (ingredienteInfo.Id.HasValue)
                    {
                        // Actualizar ingrediente existente
                        var ingredienteExistente = producto.Ingredientes.FirstOrDefault(i => i.Id == ingredienteInfo.Id.Value);
                        if (ingredienteExistente != null)
                        {
                            ingredienteExistente.IngredienteId = ingredienteInfo.IngredienteId;
                            ingredienteExistente.Cantidad = ingredienteInfo.Cantidad;
                            ingredienteExistente.Opcional = ingredienteInfo.Opcional;
                        }
                    }
                    else
                    {
                        // Agregar nuevo ingrediente
                        var nuevoIngrediente = new IngredienteProducto
                        {
                            ProductoId = producto.Id,
                            IngredienteId = ingredienteInfo.IngredienteId,
                            Cantidad = ingredienteInfo.Cantidad,
                            Opcional = ingredienteInfo.Opcional
                        };
                        
                        _context.IngredientesProductos.Add(nuevoIngrediente);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
} 