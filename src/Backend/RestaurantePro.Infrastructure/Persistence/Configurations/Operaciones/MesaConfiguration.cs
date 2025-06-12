using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;

/// <summary>
/// Configuración de la entidad Mesa para Entity Framework Core
/// </summary>
public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("Mesas", "Operaciones");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.Numero)
            .IsRequired();
            
        builder.Property(p => p.Capacidad)
            .IsRequired();
            
        builder.Property(p => p.Ubicacion)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Configurar fechas
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.FechaActualizacion);
        
        // Configurar índices
        builder.HasIndex(p => p.Numero)
            .HasDatabaseName("IX_Mesas_Numero")
            .IsUnique();
            
        builder.HasIndex(p => p.Estado)
            .HasDatabaseName("IX_Mesas_Estado");
            
        builder.HasIndex(p => p.Ubicacion)
            .HasDatabaseName("IX_Mesas_Ubicacion");
    }
} 