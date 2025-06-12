using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Productos.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    /// <summary>
    /// Configuración de la entidad Producto para Entity Framework Core
    /// </summary>
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Descripcion)
                .HasMaxLength(500);

            builder.Property(p => p.Precio)
                .HasPrecision(18, 2)
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
            builder.HasIndex(p => p.Nombre)
                .HasDatabaseName("IX_Productos_Nombre");

            // Query Filters para soft delete
            builder.HasQueryFilter(p => !p.Eliminado);
        }
    }
} 