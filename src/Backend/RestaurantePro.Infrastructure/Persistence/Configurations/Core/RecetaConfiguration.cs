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
            builder.ToTable("Recetas", "Core");

            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Id)
                .ValueGeneratedNever();

            builder.Property(r => r.ProductoId)
                .IsRequired();

            builder.Property(r => r.Preparacion)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.TiempoPreparacionMinutos)
                .IsRequired();
            
            builder.OwnsMany(r => r.Ingredientes, ownedBuilder =>
            {
                ownedBuilder.ToTable("IngredientesRecetas", "Core");
                
                ownedBuilder.WithOwner().HasForeignKey("RecetaId");
                
                ownedBuilder.Property(i => i.IngredienteId).IsRequired();
                ownedBuilder.Property(i => i.Nombre).IsRequired().HasMaxLength(100);
                ownedBuilder.Property(i => i.Cantidad).IsRequired().HasColumnType("decimal(18,2)");
                ownedBuilder.Property(i => i.UnidadMedida).IsRequired().HasMaxLength(50).HasConversion<string>();
                ownedBuilder.Property(i => i.EsOpcional).IsRequired();

                ownedBuilder.HasKey("RecetaId", "IngredienteId");
            });

            // Índices
            builder.HasIndex(r => r.ProductoId);
            
            // Query Filters para soft delete
            builder.HasQueryFilter(r => !r.EstaEliminado);
        }
    }
} 