using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using IdentityApplicationUser = RestaurantePro.Infrastructure.Identity.Models.ApplicationUser;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

public interface ISeedDataService
{
    Task SeedAsync();
}

public class TestSeedDataService : ISeedDataService
{
    private readonly RestauranteProDbContext _context;
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public TestSeedDataService(
        RestauranteProDbContext context,
        UserManager<IdentityApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine("🔧 INICIANDO SEED DE DATOS PARA TESTS...");
        
        try
        {
            // 🔧 LIMPIEZA COMPLETA DE LA BASE DE DATOS
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Base de datos limpiada y recreada");
            
            // 🔧 SEMBRAR ROLES PRIMERO
            await SeedRolesAsync();
            Console.WriteLine("✅ Roles creados");
            
            // 🔧 SEMBRAR USUARIOS DESPUÉS
            await SeedUsersAsync();
            Console.WriteLine("✅ Usuarios creados");
            
            // 🔧 SEMBRAR DATOS DE PRUEBA USANDO BUILDERS DEL DOMINIO
            await SeedTestDataWithBuildersAsync();
            Console.WriteLine("✅ Datos de prueba creados con builders");
            
            // 🔧 GUARDAR CAMBIOS
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Seed de datos completado");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR EN SEED DE DATOS: {ex.Message}");
            throw; 
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Mesero", "Cocinero", "Cajero" };
        
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper(),
                    Description = $"Rol de {roleName}"
                };
                await _roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        // 🔧 CREAR USUARIO ADMIN
        var adminUser = new IdentityApplicationUser
        {
            UserName = "admin@restaurantepro.com",
            Email = "admin@restaurantepro.com",
            Nombre = "Administrador",
            Apellidos = "Sistema",
            Activo = true,
            EmailConfirmed = true,
            FotoPerfil = "",
            RefreshToken = ""
        };

        // 🔧 VERIFICAR SI EL USUARIO YA EXISTE
        var existingAdmin = await _userManager.FindByEmailAsync(adminUser.Email);
        if (existingAdmin == null)
        {
            var result = await _userManager.CreateAsync(adminUser, "AdminRestaurante123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"✅ Usuario admin creado: {adminUser.Email}");
            }
            else
            {
                Console.WriteLine($"❌ Error creando usuario admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"✅ Usuario admin ya existe: {existingAdmin.Email}");
        }

        // 🔧 CREAR USUARIO MESERO
        var meseroUser = new IdentityApplicationUser
        {
            UserName = "mesero@restaurantepro.com",
            Email = "mesero@restaurantepro.com",
            Nombre = "Mesero",
            Apellidos = "Test",
            Activo = true,
            EmailConfirmed = true,
            FotoPerfil = "",
            RefreshToken = ""
        };

        // 🔧 VERIFICAR SI EL USUARIO YA EXISTE
        var existingMesero = await _userManager.FindByEmailAsync(meseroUser.Email);
        if (existingMesero == null)
        {
            var result = await _userManager.CreateAsync(meseroUser, "Mesero123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(meseroUser, "Mesero");
                Console.WriteLine($"✅ Usuario mesero creado: {meseroUser.Email}");
            }
            else
            {
                Console.WriteLine($"❌ Error creando usuario mesero: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"✅ Usuario mesero ya existe: {existingMesero.Email}");
        }
    }

    private async Task SeedTestDataWithBuildersAsync()
    {
        try
        {
            Console.WriteLine("🔧 CREANDO DATOS DE PRUEBA CON BUILDERS DEL DOMINIO...");

            // 🔧 CREAR MESAS USANDO EL MÉTODO DE FÁBRICA
            var mesas = new[]
            {
                RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(1, 4, "Interior"),
                RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(2, 6, "Interior"),
                RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(3, 2, "Terraza"),
                RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(4, 8, "VIP"),
                RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(5, 4, "Interior")
            };

            // 🔧 AGREGAR MESAS AL CONTEXTO
            foreach (var mesa in mesas)
            {
                _context.Mesas.Add(mesa);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Mesas creadas usando factory method");

            // 🔧 CREAR CATEGORÍAS DE PRODUCTOS USANDO EL MÉTODO DE FÁBRICA
            var categorias = new[]
            {
                RestaurantePro.Domain.Core.Productos.Entities.ProductoCategoria.Crear("Platos Principales", "Platos principales del menú", 1),
                RestaurantePro.Domain.Core.Productos.Entities.ProductoCategoria.Crear("Entradas", "Entradas y aperitivos", 2),
                RestaurantePro.Domain.Core.Productos.Entities.ProductoCategoria.Crear("Postres", "Postres y dulces", 3),
                RestaurantePro.Domain.Core.Productos.Entities.ProductoCategoria.Crear("Bebidas", "Bebidas y refrescos", 4)
            };

            foreach (var categoria in categorias)
            {
                _context.ProductoCategorias.Add(categoria);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Categorías de productos creadas usando factory method");

            // 🔧 CREAR PRODUCTOS USANDO EL MÉTODO DE FÁBRICA
            var productos = new[]
            {
                RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                    "Pasta Carbonara", 
                    "Pasta con salsa carbonara", 
                    new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(15.99m), 
                    categorias[0].Id),
                RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                    "Ensalada César", 
                    "Ensalada fresca estilo César", 
                    new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(8.99m), 
                    categorias[1].Id),
                RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                    "Tiramisú", 
                    "Postre italiano clásico", 
                    new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(6.50m), 
                    categorias[2].Id),
                RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                    "Café Americano", 
                    "Café negro americano", 
                    new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(2.50m), 
                    categorias[3].Id)
            };

            foreach (var producto in productos)
            {
                _context.Productos.Add(producto);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Productos creados usando factory method");

            // 🔧 CREAR CLIENTES USANDO EL MÉTODO DE FÁBRICA
            var clientes = new[]
            {
                RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                    RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Juan", "Pérez"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.Email.Create("juan@example.com"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.PhoneNumber.Create("+56912345678"),
                    new DateTime(1990, 5, 15)),
                RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                    RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("María", "García"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.Email.Create("maria@example.com"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.PhoneNumber.Create("+56987654321"),
                    new DateTime(1985, 8, 22)),
                RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                    RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Carlos", "López"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.Email.Create("carlos@example.com"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.PhoneNumber.Create("+56955566677"),
                    new DateTime(1992, 3, 10)),
                RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                    RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Ana", "Martínez"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.Email.Create("ana@example.com"),
                    RestaurantePro.Domain.Core.SharedKernel.ValueObjects.PhoneNumber.Create("+56911122233"),
                    new DateTime(1988, 12, 5))
            };

            foreach (var cliente in clientes)
            {
                _context.Clientes.Add(cliente);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Clientes creados usando factory method");
            
            // 🔧 VERIFICAR QUE LOS CLIENTES SE GUARDARON CORRECTAMENTE
            var clientesGuardados = await _context.Clientes.ToListAsync();
            Console.WriteLine($"🔧 Clientes en la base de datos después del guardado: {clientesGuardados.Count}");
            foreach (var cliente in clientesGuardados)
            {
                Console.WriteLine($"  - {cliente.Id}: {cliente.Nombre.NombreCompleto} ({cliente.Email.Value})");
            }

            // 🔧 CREAR TARJETAS DE FIDELIZACIÓN USANDO EL MÉTODO DE FÁBRICA
            var tarjetas = new[]
            {
                RestaurantePro.Domain.Comercial.Clientes.Entities.TarjetaFidelizacion.Crear(clientes[0].Id, "1234567890123456"),
                RestaurantePro.Domain.Comercial.Clientes.Entities.TarjetaFidelizacion.Crear(clientes[1].Id, "2345678901234567"),
                RestaurantePro.Domain.Comercial.Clientes.Entities.TarjetaFidelizacion.Crear(clientes[2].Id, "3456789012345678")
            };

            foreach (var tarjeta in tarjetas)
            {
                _context.TarjetasFidelizacion.Add(tarjeta);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Tarjetas de fidelización creadas usando factory method");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR CREANDO DATOS DE PRUEBA: {ex.Message}");
            // No lanzar excepción para no fallar el seed completo
        }
    }




}