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

            builder.OwnsOne(p => p.Precio, priceBuilder => {
                priceBuilder.Property(p => p.Valor)
                    .HasColumnName("Precio")
                    .HasPrecision(18, 2)
                    .IsRequired();
            });

            builder.Property(p => p.CategoriaId)
                .IsRequired();

            builder.Property(p => p.CategoriaNombre)
                .HasMaxLength(100);

            builder.Property(p => p.Popularidad)
                .HasDefaultValue(0);

            builder.Property(p => p.EstaActivo)
                .IsRequired()
                .HasDefaultValue(true);

            // Relaciones
            builder.HasMany(p => p.Recetas)
                .WithOne()
                .HasForeignKey("ProductoId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar fechas de auditoría
            builder.Property(p => p.FechaCreacion)
                .IsRequired();

            builder.Property(p => p.FechaActualizacion);

            // Configurar índices
            builder.HasIndex(p => p.Nombre)
                .HasDatabaseName("IX_Productos_Nombre");

            builder.HasIndex(p => new { p.CategoriaId, p.EstaActivo })
                .HasDatabaseName("IX_Productos_Categoria_Activo");

            // Query Filters para soft delete
            builder.HasQueryFilter(p => !p.EstaEliminado);
        }
    }
} 