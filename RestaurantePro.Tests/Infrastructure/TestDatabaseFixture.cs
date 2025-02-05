using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Infrastructure.Data;
using System;
using System.Linq;
using RestaurantePro.Core.Identity;

namespace RestaurantePro.Tests.Infrastructure
{
    public class TestDatabaseFixture : IDisposable
    {
        private const string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=RestauranteProTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
        public RestauranteContext Context { get; private set; }

        public TestDatabaseFixture()
        {
            var options = new DbContextOptionsBuilder<RestauranteContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            Context = new RestauranteContext(options);
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
            SeedTestData();
        }

        private void SeedTestData()
        {
            Context.Mesas.RemoveRange(Context.Mesas);
            Context.Platos.RemoveRange(Context.Platos);
            Context.SaveChanges();

            try
            {
                Context.Set<ApplicationUser>().Add(new ApplicationUser
                {
                    Id = "mesero-test-1",
                    UserName = "mesero1",
                    Nombre = "Mesero Test",
                    Apellido = "Apellido Test",
                    Rol = RolUsuario.Mesero
                });

                Context.Mesas.AddRange(
                    new Mesa { Numero = "1", Estado = EstadoMesa.Disponible },
                    new Mesa { Numero = "2", Estado = EstadoMesa.Disponible }
                );

                Context.Platos.Add(new Plato
                {
                    Nombre = "Plato Test 1",
                    Precio = 25.00m,
                    Disponible = true,
                    Descripcion = "Plato de prueba",
                    Categoria = "Test",
                    Stock = 100,
                    CreatedAt = DateTime.UtcNow
                });

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sembrar datos de prueba: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}