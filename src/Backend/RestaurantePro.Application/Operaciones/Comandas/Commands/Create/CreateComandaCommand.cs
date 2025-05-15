using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.Create
{
    public class CreateComandaCommand : IRequest<string>
    {
        public int MesaId { get; set; }
        public int UsuarioId { get; set; }
        public List<ProductoComandaDto> Productos { get; set; } = new List<ProductoComandaDto>();
        public string Notas { get; set; }
    }

    public class ProductoComandaDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string Notas { get; set; }
    }

    public class CreateComandaCommandHandler : IRequestHandler<CreateComandaCommand, string>
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateComandaCommandHandler(
            IComandaRepository comandaRepository,
            IMesaRepository mesaRepository,
            IUnitOfWork unitOfWork)
        {
            _comandaRepository = comandaRepository;
            _mesaRepository = mesaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(CreateComandaCommand request, CancellationToken cancellationToken)
        {
            // Verificar disponibilidad de mesa
            var mesa = await _mesaRepository.GetByIdAsync(request.MesaId);
            if (mesa == null)
            {
                throw new ApplicationException("Mesa no encontrada");
            }

            if (!mesa.EstaDisponible)
            {
                throw new ApplicationException("La mesa no está disponible");
            }

            // Generar número de comanda
            var numeroComanda = GenerarNumeroComanda();

            // Crear comanda
            var comanda = new Comanda
            {
                NumeroComanda = numeroComanda,
                MesaId = request.MesaId,
                UsuarioId = request.UsuarioId,
                FechaCreacion = DateTime.Now,
                Estado = "Pendiente",
                Notas = request.Notas
            };

            // Agregar productos a la comanda
            foreach (var producto in request.Productos)
            {
                comanda.DetallesComanda.Add(new DetalleComanda
                {
                    ProductoId = producto.ProductoId,
                    Cantidad = producto.Cantidad,
                    Notas = producto.Notas,
                    Estado = "Pendiente"
                });
            }

            await _comandaRepository.AddAsync(comanda);
            
            // Actualizar estado de mesa
            mesa.EstaDisponible = false;
            mesa.ComandaActualId = comanda.Id;
            await _mesaRepository.UpdateAsync(mesa);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return numeroComanda;
        }

        private string GenerarNumeroComanda()
        {
            return $"CMD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }
    }
} 