using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Commands.RealizarInventarioFisico;

public class RealizarInventarioFisicoCommandHandler : IRequestHandler<RealizarInventarioFisicoCommand, Result<ResultadoInventarioFisicoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public RealizarInventarioFisicoCommandHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<ResultadoInventarioFisicoDto>> Handle(
        RealizarInventarioFisicoCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fechaInventario = request.FechaInventario;
            var diferencias = new List<DiferenciaInventarioDto>();
            var totalDiferencias = 0;
            var valorTotalDiferencias = 0m;

            foreach (var conteo in request.Conteos)
            {
                var ingrediente = await _context.Ingredientes
                    .FirstOrDefaultAsync(i => i.Id == conteo.IngredienteId, cancellationToken);

                if (ingrediente == null)
                {
                    return Result.Failure<ResultadoInventarioFisicoDto>($"Ingrediente con ID {conteo.IngredienteId} no encontrado");
                }

                var diferencia = conteo.CantidadContada - ingrediente.Stock;
                var valorDiferencia = diferencia * ingrediente.CostoPromedio;

                if (Math.Abs(diferencia) > 0.01m) // Tolerancia para diferencias mínimas
                {
                    diferencias.Add(new DiferenciaInventarioDto
                    {
                        IngredienteId = 0, // TODO: Usar ID real del ingrediente cuando esté disponible
                        NombreIngrediente = ingrediente.Nombre,
                        StockSistema = ingrediente.Stock,
                        StockContado = conteo.CantidadContada,
                        Diferencia = diferencia,
                        ValorDiferencia = Math.Abs(valorDiferencia),
                        UnidadMedida = ingrediente.UnidadMedida.ToString(),
                        Observaciones = conteo.Observaciones
                    });

                    totalDiferencias++;
                    valorTotalDiferencias += Math.Abs(valorDiferencia);
                }
            }

            var resultado = new ResultadoInventarioFisicoDto
            {
                InventarioId = 0,
                FechaRealizacion = fechaInventario,
                Responsable = request.UsuarioId.ToString(),
                Diferencias = diferencias,
                ValorTotalDiferencias = valorTotalDiferencias,
                TotalItemsContados = request.Conteos.Count,
                ItemsConDiferencias = totalDiferencias,
                Estado = "Finalizado"
            };

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result.Failure<ResultadoInventarioFisicoDto>($"Error al realizar inventario físico: {ex.Message}");
        }
    }
} 