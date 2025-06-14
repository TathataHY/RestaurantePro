using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Inventario
{
    /// <summary>
    /// Configuración para la entidad ItemOrdenCompra
    /// </summary>
    public class ItemOrdenCompraConfiguration : IEntityTypeConfiguration<ItemOrdenCompra>
    {
        /// <summary>
        /// Configura el mapeo de la entidad ItemOrdenCompra
        /// </summary>
        public void Configure(EntityTypeBuilder<ItemOrdenCompra> builder)
        {
            builder.ToTable("ItemsOrdenCompra", "Inventario");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.OrdenCompraId)
                .IsRequired();
                
            builder.Property(i => i.IngredienteId)
                .IsRequired();
                
            builder.Property(i => i.NombreIngrediente)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(i => i.Cantidad)
                .IsRequired()
                .HasPrecision(10, 2);
                
            builder.Property(i => i.UnidadMedida)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.PrecioUnitario)
                .IsRequired()
                .HasPrecision(18, 2);
                
            builder.Property(i => i.Subtotal)
                .IsRequired()
                .HasPrecision(18, 2);
                
            builder.Property(i => i.CantidadRecibida)
                .HasPrecision(10, 2);
                
            // Índices
            builder.HasIndex(i => i.OrdenCompraId)
                .HasDatabaseName("IX_ItemsOrdenCompra_OrdenCompraId");
                
            builder.HasIndex(i => i.IngredienteId)
                .HasDatabaseName("IX_ItemsOrdenCompra_IngredienteId");
        }
    }
} 