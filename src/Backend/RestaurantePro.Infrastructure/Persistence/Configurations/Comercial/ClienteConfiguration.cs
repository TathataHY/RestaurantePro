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
        
        // Configurar el valor objeto Nombre como propiedad de navegación poseída
        builder.OwnsOne(p => p.Nombre, nombre =>
        {
            nombre.Property(n => n.Nombre)
                .HasColumnName("Nombres")
                .IsRequired()
                .HasMaxLength(100);
                
            nombre.Property(n => n.Apellido)
                .HasColumnName("Apellidos")
                .IsRequired()
                .HasMaxLength(100);
        });
        
        // Configurar el valor objeto Email como propiedad de navegación poseída
        builder.OwnsOne(p => p.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(150);
        });
        
        // Configurar el valor objeto Telefono como propiedad de navegación poseída
        builder.OwnsOne(p => p.Telefono, telefono =>
        {
            telefono.Property(t => t.Value)
                .HasColumnName("Telefono")
                .IsRequired()
                .HasMaxLength(20);
        });
        
        builder.Property(p => p.FechaNacimiento)
            .IsRequired();
        
        builder.Property(p => p.EstaActivo)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(p => p.PuntosAcumulados)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(p => p.CantidadVisitas)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(p => p.TarjetaFidelizacionPrincipalId);
            
        builder.Property(p => p.Segmento)
            .IsRequired()
            .HasConversion<string>();
        
        // Configurar fechas
        builder.Property(p => p.FechaCreacion)
            .IsRequired();
            
        builder.Property(p => p.FechaActualizacion);
            
        builder.Property(p => p.EstaEliminado)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Configurar índices
        builder.HasIndex("Email_Value")
            .HasDatabaseName("IX_Clientes_Email")
            .IsUnique();
            
        builder.HasIndex("Nombre_Nombres", "Nombre_Apellidos")
            .HasDatabaseName("IX_Clientes_NombreCompleto");
            
        builder.HasIndex("Telefono_Value")
            .HasDatabaseName("IX_Clientes_Telefono");
            
        builder.HasIndex(p => p.TarjetaFidelizacionPrincipalId)
            .HasDatabaseName("IX_Clientes_TarjetaFidelizacionId");
    }
} 