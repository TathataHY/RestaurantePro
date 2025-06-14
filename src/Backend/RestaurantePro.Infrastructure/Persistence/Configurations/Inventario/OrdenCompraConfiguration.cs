using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Inventario
{
    /// <summary>
    /// Configuración para la entidad OrdenCompra
    /// </summary>
    public class OrdenCompraConfiguration : IEntityTypeConfiguration<OrdenCompra>
    {
        /// <summary>
        /// Configura el mapeo de la entidad OrdenCompra
        /// </summary>
        public void Configure(EntityTypeBuilder<OrdenCompra> builder)
        {
            builder.ToTable("OrdenesCompra", "Inventario");
            
            builder.HasKey(o => o.Id);
            
            builder.Property(o => o.Numero)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(o => o.FechaEmision)
                .IsRequired();
                
            builder.Property(o => o.FechaEntregaEstimada)
                .IsRequired();
                
            builder.Property(o => o.FechaEntregaReal);
                
            builder.Property(o => o.Estado)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(o => o.ProveedorId)
                .IsRequired();
                
            builder.Property(o => o.Total)
                .IsRequired()
                .HasPrecision(18, 2);
                
            builder.Property(o => o.Observaciones)
                .HasMaxLength(500);
                
            builder.Property(o => o.UsuarioId)
                .IsRequired();
                
            // Configuración para la colección de items
            builder.OwnsMany(o => o.Items, itemsBuilder =>
            {
                itemsBuilder.ToTable("ItemsOrdenCompra", "Inventario");
                
                itemsBuilder.WithOwner().HasForeignKey("OrdenCompraId");
                
                itemsBuilder.HasKey("Id");
                
                itemsBuilder.Property(i => i.IngredienteId)
                    .IsRequired();
                    
                itemsBuilder.Property(i => i.Cantidad)
                    .IsRequired()
                    .HasPrecision(10, 2);
                    
                itemsBuilder.Property(i => i.PrecioUnitario)
                    .IsRequired()
                    .HasPrecision(18, 2);
                    
                itemsBuilder.Property(i => i.Subtotal)
                    .IsRequired()
                    .HasPrecision(18, 2);
                    
                itemsBuilder.Property(i => i.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);
                    
                itemsBuilder.Property(i => i.Notas)
                    .HasMaxLength(500);
            });
            
            // Índices
            builder.HasIndex(o => o.Numero)
                .HasDatabaseName("IX_OrdenesCompra_Numero")
                .IsUnique();
                
            builder.HasIndex(o => o.FechaEmision)
                .HasDatabaseName("IX_OrdenesCompra_FechaEmision");
                
            builder.HasIndex(o => o.Estado)
                .HasDatabaseName("IX_OrdenesCompra_Estado");
                
            builder.HasIndex(o => o.ProveedorId)
                .HasDatabaseName("IX_OrdenesCompra_ProveedorId");
        }
    }
}