using AutoMapper;
using MediatR;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Categorias.Commands.CrearCategoria
{
    public class CrearCategoriaCommandHandler : IRequestHandler<CrearCategoriaCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CrearCategoriaCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> Handle(CrearCategoriaCommand request, CancellationToken cancellationToken)
        {
            var categoria = new Categoria
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                ImagenUrl = request.ImagenUrl,
                Orden = request.Orden,
                FechaCreacion = DateTime.Now
            };
            
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            
            return categoria.Id;
        }
    }
} 