using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProductoComanda
{
    /// <summary>
    /// Manejador para el comando AgregarProductoComandaCommand
    /// </summary>
    public class AgregarProductoComandaCommandHandler : IRequestHandler<AgregarProductoComandaCommand, Result<ComandaDto>>
    {
        private readonly IMapper _mapper;
        private readonly IComandaRepository _comandaRepository;
        private readonly IInventarioComandaService _inventarioService;

        public AgregarProductoComandaCommandHandler(
            IMapper mapper,
            IComandaRepository comandaRepository,
            IInventarioComandaService inventarioService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _inventarioService = inventarioService ?? throw new ArgumentNullException(nameof(inventarioService));
        }

        public async Task<Result<ComandaDto>> Handle(AgregarProductoComandaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
                
                if (comanda == null)
                {
                    return Result<ComandaDto>.Failure($"No se encontró la comanda con ID {request.ComandaId}");
                }
                
                // Verificar disponibilidad de ingredientes
                var disponibilidad = await _inventarioService.VerificarDisponibilidadProductoAsync(
                    request.ProductoId, request.Cantidad, cancellationToken);
                
                if (!disponibilidad.EstaDisponible)
                {
                    return Result<ComandaDto>.Failure($"No hay suficiente stock para el producto: {disponibilidad.MensajeError}");
                }
                
                // Agregar producto a la comanda
                comanda.AgregarProducto(
                    request.ProductoId,
                    request.Cantidad,
                    request.PrecioUnitario,
                    request.Observaciones);
                
                // Guardar cambios
                await _comandaRepository.UpdateAsync(comanda, cancellationToken);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                // Actualizar inventario
                await _inventarioService.ActualizarInventarioAsync(
                    request.ProductoId, request.Cantidad, comanda.Id, cancellationToken);
                
                // Mapear a DTO
                var comandaDto = _mapper.Map<ComandaDto>(comanda);
                
                return Result<ComandaDto>.Success(comandaDto);
            }
            catch (InvalidOperationException ex)
            {
                return Result<ComandaDto>.Failure($"Error de operación: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<ComandaDto>.Failure($"Error al agregar producto a la comanda: {ex.Message}");
            }
        }
    }
} 