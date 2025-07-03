using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

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
                
            builder.Property(i => i.Codigo)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(i => i.Descripcion)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(i => i.UnidadMedida)
                .IsRequired()
                .HasConversion<string>();
                
            builder.Property(i => i.Stock)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(i => i.StockMinimo)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(i => i.ProveedorPrincipalId);
            
            builder.Property(i => i.Rotacion)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.Temporada)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(i => i.BloqueadoControlCalidad);
            
            builder.Property(i => i.CostoPromedio)
                .HasPrecision(18, 2);
            
            // Configuración de RowVersion - comentada temporalmente para tests
            // builder.Property(i => i.RowVersion)
            //     .HasColumnName("RowVersion")
            //     .HasDefaultValue(new byte[8]); // Valor por defecto para SQLite
            // // NO se marca como IsRowVersion() ni IsConcurrencyToken() para evitar concurrencia optimista
            
            // Índices
            builder.HasIndex(i => i.Nombre)
                .HasDatabaseName("IX_Ingredientes_Nombre");
                
            builder.HasIndex(i => i.Codigo)
                .HasDatabaseName("IX_Ingredientes_Codigo")
                .IsUnique();
                
            builder.HasIndex(i => i.Stock)
                .HasDatabaseName("IX_Ingredientes_Stock");
                
            builder.HasIndex(i => i.ProveedorPrincipalId)
                .HasDatabaseName("IX_Ingredientes_ProveedorPrincipalId");

            // Relaciones
            builder.HasOne(i => i.ProveedorPrincipal)
                .WithMany()
                .HasForeignKey(i => i.ProveedorPrincipalId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(i => i.Movimientos)
                .WithOne()
                .HasForeignKey("IngredienteId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de auditoría
            builder.Property(i => i.FechaCreacion)
                .IsRequired();

            builder.Property(i => i.FechaActualizacion);

            builder.Property(i => i.CreatedBy)
                .HasMaxLength(100);

            builder.Property(i => i.LastModifiedBy)
                .HasMaxLength(100);

            builder.Property(i => i.EstaActivo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(i => i.EstaEliminado)
                .IsRequired()
                .HasDefaultValue(false);

            // Configuración de soft delete
            builder.HasQueryFilter(i => !i.EstaEliminado);
        }
    }
}