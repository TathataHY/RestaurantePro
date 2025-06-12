using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial;

/// <summary>
/// Configuración de la entidad TarjetaFidelización para Entity Framework Core
/// </summary>
public class TarjetaFidelizacionConfiguration : IEntityTypeConfiguration<TarjetaFidelizacion>
{
    public void Configure(EntityTypeBuilder<TarjetaFidelizacion> builder)
    {
        builder.ToTable("TarjetasFidelizacion", "Comercial");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(p => p.ClienteId)
            .IsRequired();
        
        builder.Property(p => p.FechaExpiracion)
            .IsRequired();
        
        builder.Property(p => p.Nivel)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        
        builder.Property(p => p.PuntosAcumulados)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(p => p.PuntosCanjeados)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        
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
        builder.HasIndex(p => p.Codigo)
            .HasDatabaseName("IX_TarjetasFidelizacion_Codigo")
            .IsUnique();
        
        builder.HasIndex(p => p.ClienteId)
            .HasDatabaseName("IX_TarjetasFidelizacion_ClienteId")
            .IsUnique();
        
        builder.HasIndex(p => new { p.Estado, p.Nivel })
            .HasDatabaseName("IX_TarjetasFidelizacion_EstadoNivel");
    }
} 