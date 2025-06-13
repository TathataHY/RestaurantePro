using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    public class RecetaConfiguration : IEntityTypeConfiguration<Receta>
    {
        public void Configure(EntityTypeBuilder<Receta> builder)
        {
            builder.ToTable("Recetas");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ProductoId)
                .IsRequired();

            builder.Property(r => r.Preparacion)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnName("Instrucciones");

            builder.Property(r => r.TiempoPreparacionMinutos)
                .IsRequired();

            // Relaciones
            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<Receta>(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con ingredientes
            builder.HasMany(r => r.Ingredientes)
                .WithOne()
                .HasForeignKey("RecetaId")
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(r => r.ProductoId).IsUnique();
            
            // Query Filters para soft delete
            builder.HasQueryFilter(r => !r.EstaEliminado);
        }
    }

    public class IngredienteRecetaConfiguration : IEntityTypeConfiguration<IngredienteReceta>
    {
        public void Configure(EntityTypeBuilder<IngredienteReceta> builder)
        {
            builder.ToTable("IngredientesRecetas");

            builder.HasKey(ir => ir.IngredienteId);

            builder.Property("RecetaId")
                .IsRequired();

            builder.Property(ir => ir.IngredienteId)
                .IsRequired();

            builder.Property(ir => ir.Cantidad)
                .IsRequired();

            builder.Property(ir => ir.UnidadMedida)
                .IsRequired()
                .HasMaxLength(50);

            // Índices
            builder.HasIndex("RecetaId");
            builder.HasIndex(new[] { "IngredienteId", "RecetaId" }).IsUnique();
        }
    }
} 