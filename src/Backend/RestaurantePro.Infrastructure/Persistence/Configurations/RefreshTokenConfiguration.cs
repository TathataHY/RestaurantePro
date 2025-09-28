using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Infrastructure.Identity.Models;

namespace RestaurantePro.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad RefreshToken para Entity Framework
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Configurar la clave primaria
        builder.HasKey(rt => rt.Id);

        // Configurar la longitud del token
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(450); // Suficiente para un GUID

        // Configurar el UserId como índice para consultas rápidas
        builder.HasIndex(rt => rt.UserId);

        // Configurar el Token como índice único
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        // Configurar fechas
        builder.Property(rt => rt.ExpiryTime)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        // Configurar campos opcionales
        builder.Property(rt => rt.DeviceId)
            .HasMaxLength(100);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(200);

        // Configurar la relación con IdentityApplicationUser
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el usuario, se eliminan sus tokens

        // Configurar el nombre de la tabla
        builder.ToTable("RefreshTokens");

        // Configurar comentarios para la tabla
        builder.HasComment("Tokens de renovación para autenticación en múltiples dispositivos");
    }
}
