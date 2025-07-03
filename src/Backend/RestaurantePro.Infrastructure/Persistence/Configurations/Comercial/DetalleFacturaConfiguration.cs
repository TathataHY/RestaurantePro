using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial
{
    public class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFactura>
    {
        public void Configure(EntityTypeBuilder<DetalleFactura> builder)
        {
            builder.ToTable("DetalleFactura", "Comercial");
            // Relación explícita con Factura
            builder.HasOne<Factura>()
                .WithMany(f => f.Detalles)
                .HasForeignKey("FacturaId")
                .OnDelete(DeleteBehavior.Cascade);
            // Puedes agregar más configuraciones aquí si es necesario
        }
    }
} 