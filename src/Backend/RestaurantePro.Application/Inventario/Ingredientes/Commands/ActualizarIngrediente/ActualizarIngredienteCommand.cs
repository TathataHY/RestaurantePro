using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;

public record ActualizarIngredienteCommand : IRequest<Result<IngredienteDto>>
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public decimal StockMinimo { get; init; }
    public RotacionIngrediente Rotacion { get; init; }
    public TemporadaIngrediente Temporada { get; init; }
    public bool BloqueadoControlCalidad { get; init; }
    public decimal CostoPromedio { get; init; }
    public DateTime? FechaExpiracion { get; init; }
    public Guid? ProveedorPrincipalId { get; init; }
} 