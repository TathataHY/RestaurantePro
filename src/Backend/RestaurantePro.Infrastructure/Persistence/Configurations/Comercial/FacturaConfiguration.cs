using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial;

/// <summary>
/// Configuración de la entidad Factura para Entity Framework Core
/// </summary>
public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas", "Comercial");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.NumeroFactura)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(p => p.TipoFactura)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.FechaEmision)
            .IsRequired();
            
        builder.Property(p => p.FechaVencimiento);
        
        builder.Property(p => p.FechaPago);
        
        builder.Property(p => p.ClienteId);
        
        builder.Property(p => p.NombreCliente)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(p => p.IdentificacionFiscal)
            .HasMaxLength(50);
            
        builder.Property(p => p.DireccionCliente)
            .HasMaxLength(250);
        
        builder.Property(p => p.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.TotalImpuestos)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(p => p.TotalDescuentos)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.Total)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(p => p.TotalPagado)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(p => p.MotivoAnulacion)
            .HasMaxLength(250);
        
        builder.Property(p => p.Observaciones)
            .HasMaxLength(500);
            
        // Configurar ComandasIds como un valor convertible
        builder.Property(p => p.ComandasIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => Guid.Parse(id))
                    .ToList());
        
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Configurar fechas
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.FechaActualizacion);
        
        // Configurar relaciones
        builder.HasMany(p => p.Detalles)
            .WithOne()
            .HasForeignKey("FacturaId")
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configurar navegaciones
        builder.Navigation(p => p.Cliente).AutoInclude();
        builder.Navigation(p => p.Comandas).AutoInclude();
        builder.Navigation(p => p.Pagos).AutoInclude();
        
        // Configurar índices
        builder.HasIndex(p => p.NumeroFactura)
            .HasDatabaseName("IX_Facturas_NumeroFactura")
            .IsUnique();
        
        builder.HasIndex(p => p.ClienteId)
            .HasDatabaseName("IX_Facturas_ClienteId");
        
        builder.HasIndex(p => new { p.Estado, p.FechaEmision })
            .HasDatabaseName("IX_Facturas_EstadoFechaEmision");
    }
} 