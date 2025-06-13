using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;

/// <summary>
/// Configuración de la entidad ItemComanda para Entity Framework Core
/// </summary>
public class ItemComandaConfiguration : IEntityTypeConfiguration<ItemComanda>
{
    public void Configure(EntityTypeBuilder<ItemComanda> builder)
    {
        builder.ToTable("ItemsComanda", "Operaciones");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.ComandaId)
            .IsRequired();
        
        builder.Property(p => p.ProductoId)
            .IsRequired();
        
        builder.Property(p => p.Cantidad)
            .IsRequired();
            
        builder.Property(p => p.PrecioUnitario)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(p => p.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(p => p.Observaciones)
            .HasMaxLength(500);
            
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Configurar fechas
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.FechaActualizacion);
        
        builder.Property(p => p.FechaPreparacion);
        
        builder.Property(p => p.FechaListo);
        
        builder.Property(p => p.FechaEntrega);
        
        builder.Property(p => p.FechaCancelacion);
        
        builder.Property(p => p.MotivoCancelacion)
            .HasMaxLength(200);
        
        // Configurar índices
        builder.HasIndex(p => new { p.ComandaId, p.ProductoId })
            .HasDatabaseName("IX_ItemsComanda_ComandaId_ProductoId");
            
        builder.HasIndex(p => p.Estado)
            .HasDatabaseName("IX_ItemsComanda_Estado");
            
        // Configurar las personalizaciones como un objeto de valor
        builder.OwnsMany(p => p.Personalizaciones, personalización =>
        {
            personalización.ToTable("PersonalizacionesItemComanda", "Operaciones");
            personalización.WithOwner().HasForeignKey("ItemComandaId");
            personalización.HasKey("Id");
            
            personalización.Property(p => p.Accion)
                .IsRequired()
                .HasConversion<string>();
                
            personalización.Property(p => p.IngredienteId)
                .IsRequired();
                
            personalización.Property(p => p.NombreIngrediente)
                .IsRequired()
                .HasMaxLength(100);
                
            personalización.Property(p => p.Cantidad)
                .HasPrecision(10, 2);
                
            personalización.Property(p => p.PrecioAdicional)
                .HasPrecision(18, 2);
                
            personalización.Property(p => p.IngredienteSustitucionId);
            
            personalización.Property(p => p.NombreIngredienteSustitucion)
                .HasMaxLength(100);
        });
    }
} 