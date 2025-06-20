using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Demo;
using RestaurantePro.Infrastructure.Persistence.SeedData.Testing;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

public class IndividualSeedersTests : IntegrationTestBase
{
    private ILogger<IndividualSeedersTests> _logger = null!;

    public IndividualSeedersTests(DatabaseFixture fixture) : base(fixture)
    {
        // El logger se inicializará en InitializeAsync
    }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _logger = ServiceProvider.GetRequiredService<ILogger<IndividualSeedersTests>>();
    }

    #region Critical Seeders Tests

    [Fact]
    public async Task UsuarioAdminSeeder_DebeCrearUsuarioAdministrador()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new UsuarioAdminSeeder();
        
        await LimpiarUsuarios(context);

        // Act
        await seeder.SeedAsync(context, _logger);

        // Assert
        var adminUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@restaurantepro.com");
        Assert.NotNull(adminUser);
        Assert.Equal("Administrador del Sistema", adminUser.NombreCompleto);
        Assert.True(adminUser.Estado == RestaurantePro.Domain.Core.Usuarios.Enums.EstadoUsuario.Activo);
    }

    #endregion

    #region Demo Seeders Tests

    [Fact]
    public async Task ProductosSeeder_DebeCrear25ProductosDemo()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new ProductosSeeder();
        
        await LimpiarProductos(context);

        // Ejecutar dependencias necesarias
        var categoriasSeeder = new ProductoCategoriasSeeder();
        await categoriasSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        var productosCount = await context.Productos.CountAsync();
        Assert.True(productosCount > 0, $"Se esperaban productos pero se encontraron {productosCount}");

        // Verificar algunos productos específicos
        var hamburguesa = await context.Productos.FirstOrDefaultAsync(p => p.Nombre.Contains("Hamburguesa"));
        Assert.NotNull(hamburguesa);
        Assert.True(hamburguesa.Precio.Valor > 0);
    }

    [Fact]
    public async Task IngredientesSeeder_DebeCrear30IngredientesDemo()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new IngredientesSeeder();
        
        await LimpiarIngredientes(context);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        var ingredientesCount = await context.Ingredientes.CountAsync();
        Assert.True(ingredientesCount > 0, $"Se esperaban ingredientes pero se encontraron {ingredientesCount}");

        // Verificar algunos ingredientes específicos
        var pollo = await context.Ingredientes.FirstOrDefaultAsync(i => i.Nombre.Contains("Pollo"));
        Assert.NotNull(pollo);
        Assert.True(pollo.Stock > 0);
    }

    [Fact]
    public async Task ClientesSeeder_DebeCrear10ClientesDemo()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new ClientesSeeder();
        
        await LimpiarClientes(context);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        var clientesCount = await context.Clientes.CountAsync();
        Assert.Equal(10, clientesCount);

        // Verificar segmentos de clientes
        var clientePremium = await context.Clientes.FirstOrDefaultAsync(c => c.Segmento.ToString().Contains("Premium"));
        Assert.NotNull(clientePremium);
    }

    [Fact]
    public async Task MesasSeeder_DebeCrear29MesasDemo()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new MesasSeeder();
        
        await LimpiarMesas(context);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        var mesasCount = await context.Mesas.CountAsync();
        Assert.True(mesasCount > 0, $"Se esperaban mesas pero se encontraron {mesasCount}");

        // Verificar distribución por áreas
        var mesasInterior = await context.Mesas.CountAsync(m => m.Ubicacion == "Interior");
        var mesasTerraza = await context.Mesas.CountAsync(m => m.Ubicacion == "Terraza");
        var mesasVIP = await context.Mesas.CountAsync(m => m.Ubicacion == "VIP");

        Assert.True(mesasInterior > 0);
        Assert.True(mesasTerraza > 0);
        Assert.True(mesasVIP > 0);
    }

    [Fact]
    public async Task ProveedoresSeeder_DebeCrear5ProveedoresDemo()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new ProveedoresSeeder();
        
        await LimpiarProveedores(context);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        var proveedoresCount = await context.Proveedores.CountAsync();
        Assert.True(proveedoresCount > 0, $"Se esperaban proveedores pero se encontraron {proveedoresCount}");

        // Verificar que tienen contactos
        var proveedorConContacto = await context.Proveedores
            .Include(p => p.Contactos)
            .FirstOrDefaultAsync(p => p.Contactos.Any());
        Assert.NotNull(proveedorConContacto);
    }

    [Fact]
    public async Task EscenariosDemoSeeder_DebeCrearEscenarioCompleto()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new EscenariosDemoSeeder();
        
        await LimpiarTodo(context);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        // Verificar que se crearon datos de todos los tipos
        var usuariosCount = await context.Usuarios.CountAsync();
        var productosCount = await context.Productos.CountAsync();
        var ingredientesCount = await context.Ingredientes.CountAsync();
        var clientesCount = await context.Clientes.CountAsync();
        var proveedoresCount = await context.Proveedores.CountAsync();
        var mesasCount = await context.Mesas.CountAsync();

        Assert.True(usuariosCount >= 5, $"Usuarios: {usuariosCount}");
        Assert.True(productosCount >= 10, $"Productos: {productosCount}");
        Assert.True(ingredientesCount >= 6, $"Ingredientes: {ingredientesCount}");
        Assert.True(clientesCount >= 5, $"Clientes: {clientesCount}");
        Assert.True(proveedoresCount >= 4, $"Proveedores: {proveedoresCount}");
        Assert.True(mesasCount >= 15, $"Mesas: {mesasCount}");
    }

    #endregion

    #region Testing Seeders Tests

    [Fact]
    public async Task DatosPruebasIntegracion_DebeCrearDatosParaIntegracion()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var seeder = new DatosPruebasIntegracion();
        
        await LimpiarTodo(context);

        // Act
        await seeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        // Verificar usuarios de integración
        var usuariosTest = await context.Usuarios
            .Where(u => u.Email.Contains("integration"))
            .CountAsync();
        Assert.True(usuariosTest >= 4, $"Usuarios de integración: {usuariosTest}");

        // Verificar clientes de integración
        var clientesTest = await context.Clientes
            .Where(c => c.Email.Value.Contains("integration"))
            .CountAsync();
        Assert.True(clientesTest >= 3, $"Clientes de integración: {clientesTest}");

        // Verificar mesas de testing
        var mesasTest = await context.Mesas
            .Where(m => m.Ubicacion.Contains("Testing") || m.Ubicacion.Contains("Integration"))
            .CountAsync();
        Assert.True(mesasTest >= 4, $"Mesas de testing: {mesasTest}");
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task Seeders_DebenSerIdempotentes()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var productosSeeder = new ProductosSeeder();
        
        await LimpiarProductos(context);

        // Ejecutar dependencias necesarias
        var categoriasSeeder = new ProductoCategoriasSeeder();
        await categoriasSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // Act - Ejecutar dos veces
        await productosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        var countPrimero = await context.Productos.CountAsync();
        
        await productosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        var countSegundo = await context.Productos.CountAsync();

        // Assert
        Assert.Equal(countPrimero, countSegundo);
        Assert.True(countSegundo > 0, $"Se esperaban productos pero se encontraron {countSegundo}"); // Debe mantener la cantidad de productos
    }

    #endregion

    #region Helper Methods

    private async Task LimpiarUsuarios(RestauranteProDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Usuarios");
        await context.SaveChangesAsync();
    }

    private async Task LimpiarProductos(RestauranteProDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Productos");
        await context.SaveChangesAsync();
    }

    private async Task LimpiarIngredientes(RestauranteProDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Ingredientes");
        await context.SaveChangesAsync();
    }

    private async Task LimpiarClientes(RestauranteProDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Clientes");
        await context.SaveChangesAsync();
    }

    private async Task LimpiarMesas(RestauranteProDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Mesas");
        await context.SaveChangesAsync();
    }

    private async Task LimpiarProveedores(RestauranteProDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ContactosProveedores");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Proveedores");
        await context.SaveChangesAsync();
    }

    private async Task LimpiarTodo(RestauranteProDbContext context)
    {
        // Orden inverso de dependencias
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Mesas");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Clientes");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ContactosProveedores");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Proveedores");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Ingredientes");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Productos");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Usuarios");
        await context.SaveChangesAsync();
    }

    #endregion
} 