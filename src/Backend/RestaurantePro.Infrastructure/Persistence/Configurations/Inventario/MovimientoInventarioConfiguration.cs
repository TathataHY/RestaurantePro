using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

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
            builder.ToTable("MovimientosInventario", "Inventario");
            
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.IngredienteId)
                .IsRequired();
                
            builder.Property(m => m.TipoMovimiento)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(m => m.Cantidad)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(m => m.Fecha)
                .IsRequired();
                
            builder.Property(m => m.Motivo)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(m => m.CantidadFinal)
                .HasPrecision(18, 2);
                
            builder.Property(m => m.EstaAplicado)
                .IsRequired()
                .HasDefaultValue(false);
            
            // Índices
            builder.HasIndex(m => m.IngredienteId)
                .HasDatabaseName("IX_MovimientosInventario_IngredienteId");
                
            builder.HasIndex(m => m.Fecha)
                .HasDatabaseName("IX_MovimientosInventario_Fecha");
                
            builder.HasIndex(m => m.TipoMovimiento)
                .HasDatabaseName("IX_MovimientosInventario_TipoMovimiento");
                
            builder.HasIndex(m => m.EstaAplicado)
                .HasDatabaseName("IX_MovimientosInventario_EstaAplicado");

            // Relaciones
            builder.HasOne<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>()
                .WithMany(i => i.Movimientos)
                .HasForeignKey(m => m.IngredienteId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MovimientosInventario_Ingrediente");

            // Configuración de auditoría
            builder.Property(m => m.FechaCreacion)
                .IsRequired();

            builder.Property(m => m.FechaActualizacion);

            builder.Property(m => m.CreatedBy)
                .HasMaxLength(100);

            builder.Property(m => m.LastModifiedBy)
                .HasMaxLength(100);

            builder.Property(m => m.EstaEliminado)
                .IsRequired()
                .HasDefaultValue(false);

            // Configuración de soft delete
            builder.HasQueryFilter(m => !m.EstaEliminado);
        }
    }
} 