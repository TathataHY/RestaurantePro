using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    /// <summary>
    /// Configuración de la entidad Notificación para Entity Framework Core
    /// </summary>
    public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
    {
        public void Configure(EntityTypeBuilder<Notificacion> builder)
        {
            builder.ToTable("Notificaciones", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.Titulo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Mensaje)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.Tipo)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(p => p.DestinatarioId)
                .IsRequired();

            builder.Property(p => p.FechaCreacion)
                .IsRequired();

            builder.Property(p => p.FechaLectura);

            builder.Property(p => p.EntidadRelacionadaId);

            // Configurar fechas de auditoría
            builder.Property(p => p.FechaCreacion)
                .IsRequired();

            builder.Property(p => p.FechaActualizacion);

            // Configurar índices
            builder.HasIndex(p => p.DestinatarioId)
                .HasDatabaseName("IX_Notificaciones_DestinatarioId");

            builder.HasIndex(p => p.Tipo)
                .HasDatabaseName("IX_Notificaciones_Tipo");

            // Configurar relaciones
            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(p => p.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query Filters para soft delete
            builder.HasQueryFilter(n => !n.EstaEliminado);
        }
    }
} 