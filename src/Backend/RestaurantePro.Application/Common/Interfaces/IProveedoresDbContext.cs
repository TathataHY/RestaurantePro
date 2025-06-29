using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Application.Common.Interfaces;

public interface IProveedoresDbContext
{
    DbSet<Proveedor> Proveedores { get; }
    DbSet<ContactoProveedor> ContactosProveedores { get; }
    DbSet<EvaluacionProveedor> EvaluacionesProveedores { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 