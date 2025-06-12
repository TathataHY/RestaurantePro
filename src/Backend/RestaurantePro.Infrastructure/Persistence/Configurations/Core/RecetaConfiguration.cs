using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Recetas.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    public class RecetaConfiguration : IEntityTypeConfiguration<Receta>
    {
        public void Configure(EntityTypeBuilder<Receta> builder)
        {
            builder.ToTable("Recetas");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Descripcion)
                .HasMaxLength(500);

            builder.Property(r => r.Instrucciones)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.TiempoPreparacionMinutos)
                .IsRequired();

            builder.Property(r => r.Raciones)
                .IsRequired();

            builder.Property(r => r.ProductoId)
                .IsRequired();

            // Relaciones
            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<Receta>(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con ingredientes
            builder.HasMany(r => r.Ingredientes)
                .WithOne(i => i.Receta)
                .HasForeignKey(i => i.RecetaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(r => r.Nombre);
            builder.HasIndex(r => r.ProductoId).IsUnique();
            
            // Query Filters para soft delete
            builder.HasQueryFilter(r => !r.Eliminado);
        }
    }

    public class IngredienteRecetaConfiguration : IEntityTypeConfiguration<IngredienteReceta>
    {
        public void Configure(EntityTypeBuilder<IngredienteReceta> builder)
        {
            builder.ToTable("IngredientesRecetas");

            builder.HasKey(ir => ir.Id);

            builder.Property(ir => ir.RecetaId)
                .IsRequired();

            builder.Property(ir => ir.IngredienteId)
                .IsRequired();

            builder.Property(ir => ir.Cantidad)
                .IsRequired();

            builder.Property(ir => ir.UnidadMedida)
                .IsRequired()
                .HasMaxLength(50);

            // Relaciones
            builder.HasOne(ir => ir.Receta)
                .WithMany(r => r.Ingredientes)
                .HasForeignKey(ir => ir.RecetaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Ingrediente>()
                .WithMany()
                .HasForeignKey(ir => ir.IngredienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(ir => new { ir.RecetaId, ir.IngredienteId }).IsUnique();
        }
    }
} 