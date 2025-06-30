using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial
{
    public class HistorialPuntosConfiguration : IEntityTypeConfiguration<HistorialPuntos>
    {
        public void Configure(EntityTypeBuilder<HistorialPuntos> builder)
        {
            builder.ToTable("HistorialPuntos", "Comercial");
            // Puedes agregar más configuraciones aquí si es necesario
        }
    }
} 