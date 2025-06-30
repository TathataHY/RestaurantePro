using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Core.Productos.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    public class ProductoCategoriaConfiguration : IEntityTypeConfiguration<ProductoCategoria>
    {
        public void Configure(EntityTypeBuilder<ProductoCategoria> builder)
        {
            builder.ToTable("ProductoCategorias", "Core");
            // Puedes agregar más configuraciones aquí si es necesario
        }
    }
} 