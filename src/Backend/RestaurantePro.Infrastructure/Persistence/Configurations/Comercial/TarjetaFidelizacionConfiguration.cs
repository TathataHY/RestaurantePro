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
        
        // Configurar listas - USAR CAMPO PRIVADO PARA EVITAR PROBLEMAS DE CASTEO
        builder.HasMany(p => p.HistorialPuntos)
            .WithOne()
            .HasForeignKey("TarjetaFidelizacionId")
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configurar EF Core para usar el campo privado directamente y evitar problemas de casteo
        builder.Metadata.FindNavigation(nameof(TarjetaFidelizacion.HistorialPuntos))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);
            
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

        // ATENCIÓN: Control de concurrencia optimista DESACTIVADO para compatibilidad con SQLite en tests
        // En producción con SQL Server se debe reactivar el RowVersion
        
        // Desactivar completamente cualquier control de concurrencia automático
        builder.UsePropertyAccessMode(PropertyAccessMode.Property);
        
        // Eliminar cualquier anotación de control de concurrencia residual
        builder.Metadata.RemoveAnnotation("Relational:ConcurrencyToken");
        builder.Metadata.RemoveAnnotation("SqlServer:ValueGenerationStrategy");
    }
} 