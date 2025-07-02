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
        
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.NivelFidelizacion)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.FechaEmision)
            .IsRequired();
            
        builder.Property(p => p.FechaActivacion);
        
        builder.Property(p => p.FechaExpiracion);
        
        builder.Property(p => p.PuntosAcumulados)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(p => p.PuntosDisponibles)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(p => p.MultiplicadorPuntos)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(1.0m);
            
        builder.Property(p => p.LimiteMensual);
        
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Configurar fechas
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.FechaActualizacion);
        
        // Configurar listas
        builder.HasMany(p => p.HistorialPuntos)
            .WithOne()
            .HasForeignKey("TarjetaFidelizacionId")
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configurar colección de etiquetas
        builder.Property<string>("EtiquetasSerializadas")
            .HasColumnName("Etiquetas")
            .HasMaxLength(500);
            
        // Configurar conversión para etiquetas
        builder.Metadata.FindNavigation(nameof(TarjetaFidelizacion.Etiquetas))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        // Configurar índices
        builder.HasIndex(p => p.Codigo)
            .HasDatabaseName("IX_TarjetasFidelizacion_Codigo")
            .IsUnique();
        
        builder.HasIndex(p => p.ClienteId)
            .HasDatabaseName("IX_TarjetasFidelizacion_ClienteId");
        
        builder.HasIndex(p => new { p.Estado, p.NivelFidelizacion })
            .HasDatabaseName("IX_TarjetasFidelizacion_EstadoNivel");
            
        // Configurar query filter para soft delete usando la propiedad heredada de EntityBase
        builder.HasQueryFilter(t => !t.EstaEliminado);
        
        // Deshabilitar concurrencia optimista explícitamente para evitar problemas con SQLite
        builder.Property(p => p.Id).IsConcurrencyToken(false);
        builder.Property(p => p.PuntosAcumulados).IsConcurrencyToken(false);
        builder.Property(p => p.PuntosDisponibles).IsConcurrencyToken(false);
        builder.Property(p => p.Estado).IsConcurrencyToken(false);
        builder.Property(p => p.NivelFidelizacion).IsConcurrencyToken(false);
    }
} 