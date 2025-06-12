using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Facturas.Entities;

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
        
        builder.Property(p => p.Numero)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(p => p.ClienteId)
            .IsRequired();
        
        builder.Property(p => p.FechaEmision)
            .IsRequired();
        
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        
        builder.Property(p => p.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.Impuestos)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.Total)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.MetodoPago)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        
        builder.Property(p => p.Observaciones)
            .HasMaxLength(500);
        
        builder.Property(p => p.ComandaId)
            .IsRequired();
        
        builder.Property(p => p.Activo)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Configurar fechas de auditoría
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.CreadoPor)
            .HasMaxLength(36);
            
        builder.Property(p => p.FechaModificacion);
        
        builder.Property(p => p.ModificadoPor)
            .HasMaxLength(36);
        
        // Configurar índices
        builder.HasIndex(p => p.Numero)
            .HasDatabaseName("IX_Facturas_Numero")
            .IsUnique();
        
        builder.HasIndex(p => p.ClienteId)
            .HasDatabaseName("IX_Facturas_ClienteId");
        
        builder.HasIndex(p => p.ComandaId)
            .HasDatabaseName("IX_Facturas_ComandaId")
            .IsUnique();
        
        builder.HasIndex(p => new { p.Estado, p.FechaEmision })
            .HasDatabaseName("IX_Facturas_EstadoFechaEmision");
        
        // Configurar relaciones
        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 