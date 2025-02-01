using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Models;

namespace RestaurantePro.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Comanda> Comandas { get; set; }
    public DbSet<ComandaDetalle> ComandaDetalles { get; set; }
    public DbSet<Mesa> Mesas { get; set; }
    public DbSet<Plato> Platos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ComandaDetalle>()
            .HasOne(cd => cd.Comanda)
            .WithMany(c => c.Detalles)
            .HasForeignKey(cd => cd.ComandaId);

        modelBuilder.Entity<ComandaDetalle>()
            .HasOne(cd => cd.Plato)
            .WithMany(p => p.ComandaDetalles)
            .HasForeignKey(cd => cd.PlatoId);
    }
}
