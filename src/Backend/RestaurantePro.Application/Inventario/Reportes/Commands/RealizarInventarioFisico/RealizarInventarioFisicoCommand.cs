using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Commands.RealizarInventarioFisico;

/// <summary>
/// Comando para realizar un inventario físico
/// </summary>
public class RealizarInventarioFisicoCommand : IRequest<Result<ResultadoInventarioFisicoDto>>
{
    /// <summary>
    /// Fecha de realización del inventario
    /// </summary>
    public DateTime FechaInventario { get; set; }
    
    /// <summary>
    /// ID del usuario responsable
    /// </summary>
    public Guid UsuarioId { get; set; }
    
    /// <summary>
    /// Lista de conteos del inventario
    /// </summary>
    public List<ConteoInventarioDto> Conteos { get; set; } = new();
    
    /// <summary>
    /// Observaciones generales del inventario
    /// </summary>
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para conteo de inventario físico
/// </summary>
public class ConteoInventarioDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Cantidad física contada
    /// </summary>
    public decimal CantidadContada { get; set; }

    /// <summary>
    /// Observaciones del conteo
    /// </summary>
    public string? Observaciones { get; set; }
} 