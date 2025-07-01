using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;

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
            
            builder.Property(o => o.FechaEmision)
                .IsRequired();
                
            builder.Property(o => o.FechaEntregaEstimada)
                .IsRequired();
                
            builder.Property(o => o.FechaEnvio);
                
            builder.Property(o => o.FechaRecepcion);
                
            builder.Property(o => o.FechaCancelacion);
                
            builder.Property(o => o.Estado)
                .IsRequired()
                .HasConversion<int>();
                
            builder.Property(o => o.ProveedorId)
                .IsRequired();
                
            builder.Property(o => o.Total)
                .IsRequired()
                .HasPrecision(18, 2);
                
            builder.Property(o => o.Observaciones)
                .HasMaxLength(500);
                
            builder.Property(o => o.ObservacionesRecepcion)
                .HasMaxLength(500)
                .IsRequired(false);
                
            builder.Property(o => o.MotivoCancelacion)
                .HasMaxLength(500)
                .IsRequired(false);
                
            // Configuración de RowVersion para concurrencia optimista
            builder.Property(o => o.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
                
            // Índices
            builder.HasIndex(o => o.FechaEmision)
                .HasDatabaseName("IX_OrdenesCompra_FechaEmision");
                
            builder.HasIndex(o => o.Estado)
                .HasDatabaseName("IX_OrdenesCompra_Estado");
                
            builder.HasIndex(o => o.ProveedorId)
                .HasDatabaseName("IX_OrdenesCompra_ProveedorId");
                
            // Configurar relación con ItemOrdenCompra, especificando el backing field
            // para la propiedad de navegación 'Items'.
            var navigation = builder.Metadata.FindNavigation(nameof(OrdenCompra.Items));
            navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey("OrdenCompraId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}