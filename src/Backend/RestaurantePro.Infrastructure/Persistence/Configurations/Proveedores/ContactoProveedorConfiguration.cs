using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Proveedores
{
    /// <summary>
    /// Configuración para la entidad ContactoProveedor
    /// </summary>
    public class ContactoProveedorConfiguration : IEntityTypeConfiguration<ContactoProveedor>
    {
        /// <summary>
        /// Configura el mapeo de la entidad ContactoProveedor
        /// </summary>
        public void Configure(EntityTypeBuilder<ContactoProveedor> builder)
        {
            builder.ToTable("ContactosProveedores", "Proveedores");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.Apellidos)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.Cargo)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.Departamento)
                .HasMaxLength(100);
                
            // Configuración para Email como Value Object
            builder.OwnsOne(c => c.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(100);
            });
            
            // Configuración para PhoneNumber como Value Object
            builder.OwnsOne(c => c.PhoneNumber, phoneBuilder =>
            {
                phoneBuilder.Property(p => p.Value)
                    .HasColumnName("Telefono")
                    .HasMaxLength(20);
            });
                
            builder.Property(c => c.TelefonoMovil)
                .HasMaxLength(20);
                
            builder.Property(c => c.Extension)
                .HasMaxLength(10);
                
            builder.Property(c => c.EmailSecundario)
                .HasMaxLength(100);
                
            builder.Property(c => c.Notas)
                .HasMaxLength(500);
                
            // Índices
            builder.HasIndex(c => new { c.Nombre, c.Apellidos })
                .HasDatabaseName("IX_ContactosProveedores_NombreCompleto");
                
            builder.HasIndex("ProveedorId")
                .HasDatabaseName("IX_ContactosProveedores_ProveedorId");
        }
    }
} 