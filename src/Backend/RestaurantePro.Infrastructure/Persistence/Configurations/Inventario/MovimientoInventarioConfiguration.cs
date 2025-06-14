using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Inventario
{
    /// <summary>
    /// Configuración para la entidad MovimientoInventario
    /// </summary>
    public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
    {
        /// <summary>
        /// Configura el mapeo de la entidad MovimientoInventario
        /// </summary>
        public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
        {
            // Esta configuración complementa la configuración de la entidad Ingrediente
            // ya que MovimientoInventario es una entidad owned por Ingrediente
            
            builder.ToTable("MovimientosInventario", "Inventario");
            
            builder.HasKey("Id");
            
            builder.Property(m => m.Fecha)
                .IsRequired();
                
            builder.Property(m => m.TipoMovimiento)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(m => m.Cantidad)
                .IsRequired()
                .HasPrecision(10, 2);
                
            builder.Property(m => m.DocumentoReferencia)
                .HasMaxLength(100);
                
            builder.Property(m => m.Observaciones)
                .HasMaxLength(500);
                
            builder.Property(m => m.UsuarioId)
                .IsRequired();
                
            // Índices
            builder.HasIndex(m => m.Fecha)
                .HasDatabaseName("IX_MovimientosInventario_Fecha");
                
            builder.HasIndex(m => m.TipoMovimiento)
                .HasDatabaseName("IX_MovimientosInventario_TipoMovimiento");
                
            builder.HasIndex("IngredienteId")
                .HasDatabaseName("IX_MovimientosInventario_IngredienteId");
        }
    }
} 