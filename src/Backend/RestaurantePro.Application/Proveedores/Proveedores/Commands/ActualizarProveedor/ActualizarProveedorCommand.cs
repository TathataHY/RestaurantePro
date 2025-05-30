using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using MediatR;

namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;

public class ActualizarProveedorCommand : IRequest<Result<ProveedorDto>>
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public CategoriaProveedor Categoria { get; set; }
    // Comentado hasta encontrar el enum correcto
    // public CondicionPago CondicionPago { get; set; }
    // public CalificacionProveedor Calificacion { get; set; }
    public bool Activo { get; set; } = true;
} 