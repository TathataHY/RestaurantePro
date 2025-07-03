using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones
{
    /// <summary>
    /// Configuración para la entidad PreparacionDiaria
    /// </summary>
    public class PreparacionDiariaConfiguration : IEntityTypeConfiguration<PreparacionDiaria>
    {
        /// <summary>
        /// Configura el mapeo de la entidad PreparacionDiaria
        /// </summary>
        public void Configure(EntityTypeBuilder<PreparacionDiaria> builder)
        {
            builder.ToTable("PreparacionesDiarias", "Operaciones");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.FechaPreparacion)
                .IsRequired();
                
            builder.Property(p => p.Estado)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(p => p.Observaciones)
                .HasMaxLength(500);
                
            builder.Property(p => p.ChefId)
                .IsRequired();
                
            builder.Property(p => p.CantidadPreparada)
                .IsRequired();
                
            builder.Property(p => p.CantidadDisponible)
                .IsRequired();
                
            builder.Property(p => p.FechaVencimiento)
                .IsRequired();
                
                    builder.Property(p => p.ProductoId)
            .IsRequired();
                
        // Configurar relaciones
        builder.HasOne<Comanda>()
            .WithMany()
            .HasForeignKey("ComandaId")
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Producto>()
            .WithMany()
            .HasForeignKey("ProductoId")
            .OnDelete(DeleteBehavior.Restrict);
                
        // Índices
        builder.HasIndex(p => p.FechaPreparacion)
            .HasDatabaseName("IX_PreparacionesDiarias_Fecha");
                
            builder.HasIndex(p => p.Estado)
                .HasDatabaseName("IX_PreparacionesDiarias_Estado");
                
            builder.HasIndex(p => p.ChefId)
                .HasDatabaseName("IX_PreparacionesDiarias_ChefId");
        }
    }
}