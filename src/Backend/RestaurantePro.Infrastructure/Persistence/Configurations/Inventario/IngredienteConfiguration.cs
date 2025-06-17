using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Inventario
{
    /// <summary>
    /// Configuración para la entidad Ingrediente
    /// </summary>
    public class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
    {
        /// <summary>
        /// Configura el mapeo de la entidad Ingrediente
        /// </summary>
        public void Configure(EntityTypeBuilder<Ingrediente> builder)
        {
            builder.ToTable("Ingredientes", "Inventario");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.Nombre)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(i => i.Codigo)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(i => i.Descripcion)
                .HasMaxLength(500);
                
            builder.Property(i => i.UnidadMedida)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.Stock)
                .IsRequired()
                .HasPrecision(10, 2);
                
            builder.Property(i => i.StockMinimo)
                .IsRequired()
                .HasPrecision(10, 2);
                
            builder.Property(i => i.ProveedorPrincipalId);
            
            builder.Property(i => i.Rotacion)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.Temporada)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.BloqueadoControlCalidad);
            
            builder.Property(i => i.CostoPromedio)
                .HasPrecision(18, 2);
            
            // Índices
            builder.HasIndex(i => i.Nombre)
                .HasDatabaseName("IX_Ingredientes_Nombre");
                
            builder.HasIndex(i => i.Codigo)
                .HasDatabaseName("IX_Ingredientes_Codigo")
                .IsUnique();
                
            builder.HasIndex(i => i.Stock)
                .HasDatabaseName("IX_Ingredientes_Stock");
                
            builder.HasIndex(i => i.ProveedorPrincipalId)
                .HasDatabaseName("IX_Ingredientes_ProveedorPrincipalId");

            // Configurar la colección de movimientos de inventario como una entidad poseída
            builder.OwnsMany(i => i.Movimientos, ownedBuilder =>
            {
                ownedBuilder.ToTable("MovimientosInventario", "Inventario");

                // Clave primaria para la tabla de movimientos (EF Core la necesita)
                ownedBuilder.HasKey("Id");

                ownedBuilder.Property(m => m.Fecha)
                    .IsRequired();

                ownedBuilder.Property(m => m.TipoMovimiento)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                ownedBuilder.Property(m => m.Cantidad)
                    .IsRequired()
                    .HasPrecision(10, 2);

                ownedBuilder.Property(m => m.Motivo)
                    .HasMaxLength(100);

                // Relación de vuelta al ingrediente (clave externa)
                ownedBuilder.WithOwner().HasForeignKey("IngredienteId");
            });
        }
    }
}