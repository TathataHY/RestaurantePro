using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;

/// <summary>
/// Configuración de la entidad Reservación para Entity Framework Core
/// </summary>
public class ReservacionConfiguration : IEntityTypeConfiguration<Reservacion>
{
    public void Configure(EntityTypeBuilder<Reservacion> builder)
    {
        builder.ToTable("Reservaciones", "Operaciones");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.ClienteId)
            .IsRequired();
            
        builder.Property(p => p.MesaId)
            .IsRequired();
            
        builder.Property(p => p.Fecha)
            .IsRequired();
            
        builder.Property(p => p.Hora)
            .IsRequired();
            
        builder.Property(p => p.DuracionEstimada)
            .IsRequired();
            
        builder.Property(p => p.CantidadPersonas)
            .IsRequired();
            
        builder.Property(p => p.Telefono)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(p => p.Observaciones)
            .HasMaxLength(500);
            
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.MotivoCancelacion)
            .HasMaxLength(200);
            
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Configurar fechas
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.FechaActualizacion);
        
        // Configurar índices
        builder.HasIndex(p => new { p.Fecha, p.Hora })
            .HasDatabaseName("IX_Reservaciones_FechaHora");
            
        builder.HasIndex(p => p.Estado)
            .HasDatabaseName("IX_Reservaciones_Estado");
            
        builder.HasIndex(p => p.Telefono)
            .HasDatabaseName("IX_Reservaciones_Telefono");
            
        builder.HasIndex(p => p.Email)
            .HasDatabaseName("IX_Reservaciones_Email");
            
        builder.HasIndex(p => p.ClienteId)
            .HasDatabaseName("IX_Reservaciones_ClienteId");
            
        builder.HasIndex(p => p.MesaId)
            .HasDatabaseName("IX_Reservaciones_MesaId");
            
        // Configurar navegaciones
        builder.Navigation(p => p.Mesa).AutoInclude();
        builder.Navigation(p => p.Cliente).AutoInclude();
    }
} 