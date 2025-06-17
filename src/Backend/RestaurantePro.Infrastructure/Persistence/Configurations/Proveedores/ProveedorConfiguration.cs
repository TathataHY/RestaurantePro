using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Proveedores.ValueObjects;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Proveedores
{
    /// <summary>
    /// Configuración para la entidad Proveedor
    /// </summary>
    public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        /// <summary>
        /// Configura el mapeo de la entidad Proveedor
        /// </summary>
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder.ToTable("Proveedores", "Proveedores");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.RFC)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(p => p.Direccion)
                .HasMaxLength(200);
                
            builder.Property(p => p.Ciudad)
                .HasMaxLength(100);
                
            builder.Property(p => p.Pais)
                .HasMaxLength(100);
                
            builder.Property(p => p.CodigoPostal)
                .HasMaxLength(10);
                
            builder.Property(p => p.NombreContacto)
                .HasMaxLength(100);
                
            builder.Property(p => p.InformacionBancaria)
                .HasMaxLength(200);
                
            builder.Property(p => p.DiasCredito);
                
            builder.Property(p => p.Activo);
                
            builder.Property(p => p.FechaRegistro);
                
            builder.Property(p => p.UltimaOrden);
                
            builder.Property(p => p.Observaciones)
                .HasMaxLength(500);
                
            // Configuración para Email como Value Object
            builder.OwnsOne(p => p.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(100);
            });
                
            // Configuración para PhoneNumber como Value Object
            builder.OwnsOne(p => p.Telefono, phoneBuilder =>
            {
                phoneBuilder.Property(p => p.Value)
                    .HasColumnName("Telefono")
                    .HasMaxLength(20);
            });
                
            // Relación con ContactoProveedor
            builder.HasMany(p => p.Contactos)
                .WithOne()
                .HasForeignKey("ProveedorId")
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configuración para la colección de Value Objects ProveedorCategoria
            builder.OwnsMany(p => p.Categorias, categoriaBuilder =>
            {
                categoriaBuilder.ToTable("ProveedorCategorias", "Proveedores");
                
                categoriaBuilder.WithOwner().HasForeignKey("ProveedorId");

                // Clave primaria compuesta por ProveedorId y Categoria
                categoriaBuilder.HasKey("ProveedorId", nameof(ProveedorCategoria.Categoria));

                categoriaBuilder.Property(c => c.Categoria)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                
                categoriaBuilder.Property(c => c.PorcentajeDescuento)
                    .HasPrecision(5, 2);

                categoriaBuilder.Property(c => c.EsProveedorPrincipal);
            });
                
            // Índices
            builder.HasIndex(p => p.Nombre)
                .HasDatabaseName("IX_Proveedores_Nombre");
                
            builder.HasIndex(p => p.RFC)
                .HasDatabaseName("IX_Proveedores_RFC")
                .IsUnique();
        }
    }
}