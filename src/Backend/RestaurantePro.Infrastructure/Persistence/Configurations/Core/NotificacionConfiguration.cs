using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Notificaciones.Entities;

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

            builder.Property(p => p.Estado)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(p => p.UsuarioId)
                .IsRequired();

            builder.Property(p => p.FechaEnvio)
                .IsRequired();

            builder.Property(p => p.FechaLectura);

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
            builder.HasIndex(p => p.UsuarioId)
                .HasDatabaseName("IX_Notificaciones_UsuarioId");

            builder.HasIndex(p => new { p.Estado, p.Tipo })
                .HasDatabaseName("IX_Notificaciones_EstadoTipo");

            // Configurar relaciones
            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query Filters para soft delete
            builder.HasQueryFilter(n => !n.Eliminado);
        }
    }
} 