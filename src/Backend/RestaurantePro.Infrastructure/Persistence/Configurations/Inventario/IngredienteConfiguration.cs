using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

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
                
            builder.Property(i => i.StockMaximo)
                .HasPrecision(10, 2);
                
            builder.Property(i => i.CostoUnitario)
                .IsRequired()
                .HasPrecision(18, 2);
                
            builder.Property(i => i.Categoria)
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.FechaVencimiento);
                
            builder.Property(i => i.UbicacionAlmacen)
                .HasMaxLength(100);
                
            builder.Property(i => i.ProveedorId)
                .IsRequired();
                
            builder.Property(i => i.ProveedorPrincipalId)
                .IsRequired();
                
            // Configuración para la colección de movimientos
            builder.OwnsMany(i => i.Movimientos, movimientosBuilder =>
            {
                movimientosBuilder.ToTable("MovimientosInventario", "Inventario");
                
                movimientosBuilder.WithOwner().HasForeignKey("IngredienteId");
                
                movimientosBuilder.HasKey("Id");
                
                movimientosBuilder.Property(m => m.Fecha)
                    .IsRequired();
                    
                movimientosBuilder.Property(m => m.TipoMovimiento)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);
                    
                movimientosBuilder.Property(m => m.Cantidad)
                    .IsRequired()
                    .HasPrecision(10, 2);
                    
                movimientosBuilder.Property(m => m.DocumentoReferencia)
                    .HasMaxLength(100);
                    
                movimientosBuilder.Property(m => m.Observaciones)
                    .HasMaxLength(500);
                    
                movimientosBuilder.Property(m => m.UsuarioId)
                    .IsRequired();
            });
            
            // Configuración para la colección de reservas
            builder.OwnsMany(i => i.Reservas, reservasBuilder =>
            {
                reservasBuilder.ToTable("ReservasIngredientes", "Inventario");
                
                reservasBuilder.WithOwner().HasForeignKey("IngredienteId");
                
                reservasBuilder.HasKey("Id");
                
                reservasBuilder.Property(r => r.ComandaId)
                    .IsRequired();
                    
                reservasBuilder.Property(r => r.Cantidad)
                    .IsRequired()
                    .HasPrecision(10, 2);
                    
                reservasBuilder.Property(r => r.FechaReserva)
                    .IsRequired();
            });
            
            // Índices
            builder.HasIndex(i => i.Nombre)
                .HasDatabaseName("IX_Ingredientes_Nombre");
                
            builder.HasIndex(i => i.Categoria)
                .HasDatabaseName("IX_Ingredientes_Categoria");
                
            builder.HasIndex(i => i.ProveedorId)
                .HasDatabaseName("IX_Ingredientes_ProveedorId");
                
            builder.HasIndex(i => i.Stock)
                .HasDatabaseName("IX_Ingredientes_Stock");
        }
    }
}