using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Proveedores
{
    /// <summary>
    /// Configuración para la entidad EvaluacionProveedor
    /// </summary>
    public class EvaluacionProveedorConfiguration : IEntityTypeConfiguration<EvaluacionProveedor>
    {
        /// <summary>
        /// Configura el mapeo de la entidad EvaluacionProveedor
        /// </summary>
        public void Configure(EntityTypeBuilder<EvaluacionProveedor> builder)
        {
            builder.ToTable("EvaluacionesProveedor", "Proveedores");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.ProveedorId)
                .IsRequired();
                
            builder.Property(e => e.EvaluadorId)
                .IsRequired();
                
            builder.Property(e => e.CalificacionGeneral)
                .IsRequired();
                
            builder.Property(e => e.CalificacionCalidad)
                .IsRequired();
                
            builder.Property(e => e.CalificacionPuntualidad)
                .IsRequired();
                
            builder.Property(e => e.CalificacionComunicacion)
                .IsRequired();
                
            builder.Property(e => e.CalificacionPrecios)
                .IsRequired();
                
            builder.Property(e => e.Comentarios)
                .HasMaxLength(1000);
                
            builder.Property(e => e.FechaEvaluacion)
                .IsRequired();
                
            builder.Property(e => e.FechaActualizacion)
                .IsRequired();
                
            builder.Property(e => e.Activa)
                .IsRequired();
                
            // Índices para mejorar el rendimiento de las consultas
            builder.HasIndex(e => e.ProveedorId)
                .HasDatabaseName("IX_EvaluacionesProveedor_ProveedorId");
                
            builder.HasIndex(e => e.EvaluadorId)
                .HasDatabaseName("IX_EvaluacionesProveedor_EvaluadorId");
                
            builder.HasIndex(e => e.FechaEvaluacion)
                .HasDatabaseName("IX_EvaluacionesProveedor_FechaEvaluacion");
                
            builder.HasIndex(e => new { e.ProveedorId, e.Activa })
                .HasDatabaseName("IX_EvaluacionesProveedor_ProveedorId_Activa");
                
            // Relación con Proveedor (opcional para evitar dependencias circulares)
            builder.HasOne<Proveedor>()
                .WithMany()
                .HasForeignKey(e => e.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 