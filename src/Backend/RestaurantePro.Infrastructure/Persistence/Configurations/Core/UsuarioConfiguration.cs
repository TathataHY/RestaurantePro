using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Core
{
    /// <summary>
    /// Configuración de la entidad Usuario para Entity Framework Core
    /// </summary>
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.NombreUsuario)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.NombreCompleto)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Rol)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Estado)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (EstadoUsuario)System.Enum.Parse(typeof(EstadoUsuario), v));

            // Configurar fechas de auditoría
            builder.Property(p => p.FechaCreacion)
                .IsRequired();

            builder.Property(p => p.FechaActualizacion);

            // Configurar índices
            builder.HasIndex(p => p.Email)
                .HasDatabaseName("IX_Usuarios_Email")
                .IsUnique();

            builder.HasIndex(p => p.NombreUsuario)
                .HasDatabaseName("IX_Usuarios_NombreUsuario")
                .IsUnique();

            // Configurar propiedades adicionales
            builder.Property(p => p.TipoUsuario)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (TipoUsuario)System.Enum.Parse(typeof(TipoUsuario), v));

            builder.Property(p => p.IdentityId)
                .HasMaxLength(128);

            builder.Property(p => p.PasswordHash)
                .HasMaxLength(256);

            builder.Property(p => p.Salt)
                .HasMaxLength(128);

            builder.Property(p => p.NivelAcceso)
                .IsRequired();

            builder.Property(p => p.Departamento)
                .HasMaxLength(100);

            builder.Property(p => p.Posicion)
                .HasMaxLength(100);

            builder.Property(p => p.Identificacion)
                .HasMaxLength(50);

            builder.Property(p => p.MotivoBloqueo)
                .HasMaxLength(500);

            // Configurar colección de roles
            var rolListComparer = new ValueComparer<List<RolUsuario>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                c => c != null ? c.ToList() : new List<RolUsuario>()
            );
            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                c => c != null ? c.ToList() : new List<string>()
            );

            var rolesProp = builder.Property<List<RolUsuario>>("_roles")
                .HasColumnName("Roles")
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                         .Select(r => Enum.Parse<RolUsuario>(r))
                         .ToList()
                );
            rolesProp.Metadata.SetValueComparer(rolListComparer);

            var permisosProp = builder.Property<List<string>>("_permisos")
                .HasColumnName("Permisos")
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList()
                );
            permisosProp.Metadata.SetValueComparer(stringListComparer);

            // Relaciones
            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(u => u.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query Filter para soft delete
            builder.HasQueryFilter(p => !p.EstaEliminado);
        }
    }
} 