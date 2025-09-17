using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;

/// <summary>
/// Configuración de la entidad Comanda para Entity Framework Core
/// </summary>
public class ComandaConfiguration : IEntityTypeConfiguration<Comanda>
{
    public void Configure(EntityTypeBuilder<Comanda> builder)
    {
        builder.ToTable("Comandas", "Operaciones");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.NumeroComanda)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.MesaId);
            
        builder.Property(p => p.ClienteId);
            
        builder.Property(p => p.MeseroId);
            
        builder.Property(p => p.Observaciones)
            .HasMaxLength(500);

        builder.Property(p => p.NombreEntrega)
            .HasMaxLength(100);

        builder.Property(p => p.DireccionEntrega)
            .HasMaxLength(200);

        builder.Property(p => p.TelefonoEntrega)
            .HasMaxLength(20);
            
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(p => p.FechaActualizacion);
        
        builder.Property(p => p.DescuentoFidelizacion)
            .HasPrecision(18, 2);
        
        // Configurar índices
        builder.HasIndex(p => p.NumeroComanda)
            .HasDatabaseName("IX_Comandas_NumeroComanda")
            .IsUnique();
            
        builder.HasIndex(p => p.MesaId)
            .HasDatabaseName("IX_Comandas_MesaId");
            
        builder.HasIndex(p => p.ClienteId)
            .HasDatabaseName("IX_Comandas_ClienteId");
            
        builder.HasIndex(p => p.MeseroId)
            .HasDatabaseName("IX_Comandas_MeseroId");
            
        builder.HasIndex(p => p.Estado)
            .HasDatabaseName("IX_Comandas_Estado");
        
        // Configurar el Value Object TotalComanda como propiedad poseída
        builder.OwnsOne(p => p.Total, total =>
        {
            total.Property(t => t.Subtotal).HasColumnName("Subtotal").HasPrecision(18, 2);
            total.Property(t => t.Impuestos).HasColumnName("Impuestos").HasPrecision(18, 2);
            total.Property(t => t.Descuento).HasColumnName("Descuento").HasPrecision(18, 2);
            total.Property(t => t.Total).HasColumnName("Total").HasPrecision(18, 2);
        });
        
        // Configurar relaciones
        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.ComandaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Mesa)
            .WithMany()
            .HasForeignKey(c => c.MesaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Mesero)
            .WithMany()
            .HasForeignKey(c => c.MeseroId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Factura)
            .WithMany(f => f.Comandas)
            .HasForeignKey(c => c.FacturaId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Configurar navegaciones
        builder.Navigation(p => p.Mesa).AutoInclude();
        builder.Navigation(p => p.Mesero).AutoInclude();
        builder.Navigation(p => p.Cliente).AutoInclude();
        // Factura se consulta bajo demanda; evitar AutoInclude para no encarecer listados
    }
} 