using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace RestaurantePro.Infrastructure.Persistence.Configurations.Comercial;

/// <summary>
/// Configuración de la entidad Promocion para Entity Framework Core
/// </summary>
public class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
{
    public void Configure(EntityTypeBuilder<Promocion> builder)
    {
        builder.ToTable("Promociones", "Comercial");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        builder.Property(p => p.Tipo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.ValorDescuento)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.MontoMinimo)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PuntosRequeridos)
            .IsRequired();

        builder.Property(p => p.FechaInicio)
            .IsRequired()
            .HasColumnType("TEXT")
            .HasConversion(
                v => v.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                v => DateTime.ParseExact(v, "yyyy-MM-ddTHH:mm:ss.fffffffZ", null)
            );

        builder.Property(p => p.FechaFin)
            .IsRequired()
            .HasColumnType("TEXT")
            .HasConversion(
                v => v.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                v => DateTime.ParseExact(v, "yyyy-MM-ddTHH:mm:ss.fffffffZ", null)
            );

        builder.Property(p => p.MaximoUsos);

        builder.Property(p => p.VecesUsada)
            .IsRequired();

        builder.Property(p => p.Estado)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.EsAcumulable)
            .IsRequired();

        builder.Property(p => p.DiasValidos);

        builder.Property(p => p.Prioridad)
            .IsRequired();

        builder.Property(p => p.Condiciones)
            .HasMaxLength(1000);

        // Configurar ValueComparer para las colecciones serializadas como JSON
        var guidListComparer = new ValueComparer<List<Guid>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
            c => c != null ? c.ToList() : new List<Guid>()
        );

        var productosProp = builder.Property<List<Guid>>("_productosAplicablesIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
            );
        productosProp.Metadata.SetValueComparer(guidListComparer);

        var categoriasProp = builder.Property<List<Guid>>("_categoriasAplicablesIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
            );
        categoriasProp.Metadata.SetValueComparer(guidListComparer);

        var clientesProp = builder.Property<List<Guid>>("_clientesQueUsaronIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
            );
        clientesProp.Metadata.SetValueComparer(guidListComparer);

        // Configurar índices
        builder.HasIndex(p => p.Codigo)
            .IsUnique();

        builder.HasIndex(p => p.Estado);

        // Comentado: Los índices sobre FechaInicio y FechaFin no son compatibles con SQL Server cuando usan tipo TEXT
        // builder.HasIndex(p => p.FechaInicio);
        // builder.HasIndex(p => p.FechaFin);
    }
} 