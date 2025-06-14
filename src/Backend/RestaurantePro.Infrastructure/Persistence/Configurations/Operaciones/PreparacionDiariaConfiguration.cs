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
            
            builder.Property(p => p.Fecha)
                .IsRequired();
                
            builder.Property(p => p.Estado)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(p => p.Observaciones)
                .HasMaxLength(500);
                
            builder.Property(p => p.ResponsableId)
                .IsRequired();
                
            builder.Property(p => p.CantidadTotal)
                .IsRequired()
                .HasPrecision(10, 2);
                
            builder.Property(p => p.CostoTotal)
                .IsRequired()
                .HasPrecision(18, 2);
                
            // Configuración para la colección de elementos de preparación
            builder.OwnsMany(p => p.Elementos, elementsBuilder =>
            {
                elementsBuilder.ToTable("ElementosPreparacion", "Operaciones");
                
                elementsBuilder.WithOwner().HasForeignKey("PreparacionDiariaId");
                
                elementsBuilder.HasKey("Id");
                
                elementsBuilder.Property(e => e.ProductoId)
                    .IsRequired();
                    
                elementsBuilder.Property(e => e.Cantidad)
                    .IsRequired()
                    .HasPrecision(10, 2);
                    
                elementsBuilder.Property(e => e.CostoUnitario)
                    .IsRequired()
                    .HasPrecision(18, 2);
                    
                elementsBuilder.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);
                    
                elementsBuilder.Property(e => e.Notas)
                    .HasMaxLength(500);
            });
            
            // Índices
            builder.HasIndex(p => p.Fecha)
                .HasDatabaseName("IX_PreparacionesDiarias_Fecha");
                
            builder.HasIndex(p => p.Estado)
                .HasDatabaseName("IX_PreparacionesDiarias_Estado");
                
            builder.HasIndex(p => p.ResponsableId)
                .HasDatabaseName("IX_PreparacionesDiarias_ResponsableId");
        }
    }
} 