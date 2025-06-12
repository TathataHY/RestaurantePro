using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Usuarios.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    /// <summary>
    /// Configuración de la entidad Usuario para Entity Framework Core
    /// </summary>
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Apellido)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Telefono)
                .HasMaxLength(20);

            builder.Property(p => p.Rol)
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
            builder.HasIndex(p => p.Email)
                .HasDatabaseName("IX_Usuarios_Email")
                .IsUnique();

            builder.HasIndex(p => new { p.Nombre, p.Apellido })
                .HasDatabaseName("IX_Usuarios_NombreApellido");
        }
    }
} 