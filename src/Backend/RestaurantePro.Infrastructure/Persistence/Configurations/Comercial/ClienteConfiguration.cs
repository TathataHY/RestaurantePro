using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial;

/// <summary>
/// Configuración de la entidad Cliente para Entity Framework Core
/// </summary>
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes", "Comercial");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
        
        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Apellidos)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(150);
        
        builder.Property(p => p.Telefono)
            .HasMaxLength(20);
        
        builder.Property(p => p.Direccion)
            .HasMaxLength(200);
            
        builder.Property(p => p.CodigoPostal)
            .HasMaxLength(10);
            
        builder.Property(p => p.Ciudad)
            .HasMaxLength(100);
            
        builder.Property(p => p.Pais)
            .HasMaxLength(100);
        
        builder.Property(p => p.FechaNacimiento);
        
        builder.Property(p => p.Activo)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Configurar fechas de auditoría
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.CreadoPor)
            .HasMaxLength(36);
            
        builder.Property(p => p.FechaModificacion);
        
        builder.Property(p => p.ModificadoPor)
            .HasMaxLength(36);
        
        // Configurar índices
        builder.HasIndex(p => p.Email)
            .HasDatabaseName("IX_Clientes_Email")
            .IsUnique();
        
        builder.HasIndex(p => new { p.Nombre, p.Apellidos })
            .HasDatabaseName("IX_Clientes_NombreApellidos");
            
        builder.HasIndex(p => p.Telefono)
            .HasDatabaseName("IX_Clientes_Telefono");
        
        // Configurar relaciones
        builder.HasOne(p => p.TarjetaFidelizacion)
            .WithOne()
            .HasForeignKey<TarjetaFidelizacion>(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 