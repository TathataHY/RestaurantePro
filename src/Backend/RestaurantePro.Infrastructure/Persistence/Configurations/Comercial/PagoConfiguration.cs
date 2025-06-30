using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Pagos.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos", "Comercial");
            // Aquí puedes agregar más configuraciones si es necesario
        }
    }
} 