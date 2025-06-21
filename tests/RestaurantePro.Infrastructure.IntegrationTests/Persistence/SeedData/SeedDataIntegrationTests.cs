using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using RestaurantePro.Infrastructure.Persistence.SeedData.Demo;
using RestaurantePro.Infrastructure.Persistence.SeedData.Testing;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

public class SeedDataIntegrationTests : IntegrationTestBase
{
    private ILogger<SeedDataIntegrationTests> _logger = null!;

    public SeedDataIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
        // El logger se inicializará en InitializeAsync
    }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _logger = ServiceProvider.GetRequiredService<ILogger<SeedDataIntegrationTests>>();
    }

    [Fact]
    public async Task SeedCriticalData_DebeCrearDatosCriticosCorrectamente()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Limpiar base de datos
        await LimpiarBaseDeDatos(context);

        // Act
        var rolesSeeder = new RolesSeeder();
        var usuarioSeeder = new UsuarioAdminSeeder();
        var estadosSeeder = new EstadosSeeder();
        var unidadesSeeder = new UnidadesMedidaSeeder();
        var configuracionSeeder = new ConfiguracionSeeder();
        var permisosSeeder = new PermisosSeeder();

        await rolesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await usuarioSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await estadosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await unidadesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await configuracionSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await permisosSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        await VerificarDatosCriticos(context);
    }

    [Fact]
    public async Task SeedDemoData_DebeCrearDatosDemoCorrectamente()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Limpiar y crear datos críticos primero
        await LimpiarBaseDeDatos(context);
        
        // Ejecutar seeders críticos primero
        var rolesSeeder = new RolesSeeder();
        var usuarioSeeder = new UsuarioAdminSeeder();
        var categoriasSeeder = new ProductoCategoriasSeeder();
        await rolesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await usuarioSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await categoriasSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // Act
        // Ejecutar seeders demo específicos
        var productosSeeder = new ProductosSeeder();
        var ingredientesSeeder = new IngredientesSeeder();
        var clientesSeeder = new ClientesSeeder();
        var mesasSeeder = new MesasSeeder();
        var proveedoresSeeder = new ProveedoresSeeder();
        var escenariosSeeder = new EscenariosDemoSeeder();

        await productosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await ingredientesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await clientesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await mesasSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await proveedoresSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await escenariosSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // Assert
        await VerificarDatosDemo(context);
    }

    [Fact]
    public async Task SeedTestingData_DebeCrearDatosTestingCorrectamente()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Limpiar y crear datos críticos primero
        await LimpiarBaseDeDatos(context);
        var seedRunner = scope.ServiceProvider.GetRequiredService<SeedDataRunner>();
        await seedRunner.RunCriticalOnlyAsync();

        // Act
        var testingSeeder = new DatosPruebasIntegracion();
        await testingSeeder.SeedAsync(context, _logger);

        // Assert
        await VerificarDatosTesting(context);
    }

    [Fact]
    public async Task SeedAllData_DebeCrearTodosLosDatosCorrectamente()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Limpiar base de datos
        await LimpiarBaseDeDatos(context);

        // Act - Ejecutar todos los seeders
        await EjecutarTodosLosSeeders(context);

        // Assert
        await VerificarTodosLosDatos(context);
    }

    [Fact]
    public async Task SeedData_DebeSerIdempotente()
    {
        // Arrange
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Limpiar base de datos
        await LimpiarBaseDeDatos(context);

        // Act - Ejecutar dos veces
        await EjecutarTodosLosSeeders(context);
        var countPrimeraEjecucion = await ContarRegistros(context);
        
        await EjecutarTodosLosSeeders(context);
        var countSegundaEjecucion = await ContarRegistros(context);

        // Assert - Los conteos deben ser iguales (idempotente)
        Assert.Equal(countPrimeraEjecucion.Usuarios, countSegundaEjecucion.Usuarios);
        Assert.Equal(countPrimeraEjecucion.Productos, countSegundaEjecucion.Productos);
        Assert.Equal(countPrimeraEjecucion.Clientes, countSegundaEjecucion.Clientes);
        Assert.Equal(countPrimeraEjecucion.Mesas, countSegundaEjecucion.Mesas);
        Assert.Equal(countPrimeraEjecucion.Proveedores, countSegundaEjecucion.Proveedores);
        Assert.Equal(countPrimeraEjecucion.Ingredientes, countSegundaEjecucion.Ingredientes);
    }

    private async Task LimpiarBaseDeDatos(RestauranteProDbContext context)
    {
        // Limpiar en orden inverso de dependencias usando EF Core
        context.Mesas.RemoveRange(context.Mesas);
        context.Clientes.RemoveRange(context.Clientes);
        context.ContactosProveedor.RemoveRange(context.ContactosProveedor);
        context.Proveedores.RemoveRange(context.Proveedores);
        context.Ingredientes.RemoveRange(context.Ingredientes);
        context.Productos.RemoveRange(context.Productos);
        context.Usuarios.RemoveRange(context.Usuarios);
        
        await context.SaveChangesAsync();
    }

    private async Task VerificarDatosCriticos(RestauranteProDbContext context)
    {
        // Verificar que se crearon los datos críticos mínimos
        var usuariosCount = await context.Usuarios.CountAsync();
        Assert.True(usuariosCount >= 1, "Debe existir al menos el usuario administrador");

        var adminUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@restaurantepro.com");
        Assert.NotNull(adminUser);
        Assert.Equal("Administrador del Sistema", adminUser.NombreCompleto);
    }

    private async Task VerificarDatosDemo(RestauranteProDbContext context)
    {
        // Verificar productos demo
        var productosCount = await context.Productos.CountAsync();
        Assert.True(productosCount >= 25, $"Debe haber al menos 25 productos demo, encontrados: {productosCount}");

        // Verificar ingredientes demo
        var ingredientesCount = await context.Ingredientes.CountAsync();
        Assert.True(ingredientesCount >= 30, $"Debe haber al menos 30 ingredientes demo, encontrados: {ingredientesCount}");

        // Verificar clientes demo
        var clientesCount = await context.Clientes.CountAsync();
        Assert.True(clientesCount >= 10, $"Debe haber al menos 10 clientes demo, encontrados: {clientesCount}");

        // Verificar mesas demo (28 de MesasSeeder + 15 de EscenariosDemoSeeder = 43)
        var mesasCount = await context.Mesas.CountAsync();
        Assert.True(mesasCount >= 43, $"Debe haber al menos 43 mesas demo, encontradas: {mesasCount}");

        // Verificar proveedores demo
        var proveedoresCount = await context.Proveedores.CountAsync();
        Assert.True(proveedoresCount >= 5, $"Debe haber al menos 5 proveedores demo, encontrados: {proveedoresCount}");
    }

    private async Task VerificarDatosTesting(RestauranteProDbContext context)
    {
        // Verificar datos específicos de testing
        var usuariosTestCount = await context.Usuarios
            .Where(u => u.Email.Contains("testing.com") || u.Email.Contains("integration"))
            .CountAsync();
        Assert.True(usuariosTestCount >= 4, $"Debe haber al menos 4 usuarios de testing, encontrados: {usuariosTestCount}");

        var clientesTestCount = await context.Clientes
            .Where(c => c.Email.Value.Contains("testing.com") || c.Email.Value.Contains("integration"))
            .CountAsync();
        Assert.True(clientesTestCount >= 3, $"Debe haber al menos 3 clientes de testing, encontrados: {clientesTestCount}");

        var mesasTestCount = await context.Mesas
            .Where(m => m.Ubicacion.Contains("Testing") || m.Ubicacion.Contains("Integration"))
            .CountAsync();
        Assert.True(mesasTestCount >= 4, $"Debe haber al menos 4 mesas de testing, encontradas: {mesasTestCount}");
    }

    private async Task VerificarTodosLosDatos(RestauranteProDbContext context)
    {
        // Verificar que todos los tipos de datos están presentes
        await VerificarDatosCriticos(context);
        await VerificarDatosDemo(context);
        await VerificarDatosTesting(context);

        // Verificar totales esperados
        var totalUsuarios = await context.Usuarios.CountAsync();
        Assert.True(totalUsuarios >= 10, $"Total de usuarios debe ser >= 10, encontrados: {totalUsuarios}");

        var totalProductos = await context.Productos.CountAsync();
        Assert.True(totalProductos >= 25, $"Total de productos debe ser >= 25, encontrados: {totalProductos}");

        var totalClientes = await context.Clientes.CountAsync();
        Assert.True(totalClientes >= 13, $"Total de clientes debe ser >= 13, encontrados: {totalClientes}");

        var totalMesas = await context.Mesas.CountAsync();
        Assert.True(totalMesas >= 47, $"Total de mesas debe ser >= 47, encontradas: {totalMesas}"); // 43 demo + 4 testing
    }

    private async Task EjecutarTodosLosSeeders(RestauranteProDbContext context)
    {
        // 1. Seeders críticos
        var rolesSeeder = new RolesSeeder();
        var usuarioSeeder = new UsuarioAdminSeeder();
        var estadosSeeder = new EstadosSeeder();
        var unidadesSeeder = new UnidadesMedidaSeeder();
        var configuracionSeeder = new ConfiguracionSeeder();
        var permisosSeeder = new PermisosSeeder();
        var categoriasSeeder = new ProductoCategoriasSeeder();

        await rolesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await usuarioSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await estadosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await unidadesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await configuracionSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await permisosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await categoriasSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // 2. Seeders demo
        var productosSeeder = new ProductosSeeder();
        var ingredientesSeeder = new IngredientesSeeder();
        var clientesSeeder = new ClientesSeeder();
        var mesasSeeder = new MesasSeeder();
        var proveedoresSeeder = new ProveedoresSeeder();
        var escenariosSeeder = new EscenariosDemoSeeder();

        await productosSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await ingredientesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await clientesSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await mesasSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await proveedoresSeeder.SeedAsync(context, _logger, CancellationToken.None);
        await escenariosSeeder.SeedAsync(context, _logger, CancellationToken.None);

        // 3. Seeders testing
        var testingSeeder = new DatosPruebasIntegracion();
        await testingSeeder.SeedAsync(context, _logger, CancellationToken.None);
    }

    private async Task<ConteoRegistros> ContarRegistros(RestauranteProDbContext context)
    {
        return new ConteoRegistros
        {
            Usuarios = await context.Usuarios.CountAsync(),
            Productos = await context.Productos.CountAsync(),
            Clientes = await context.Clientes.CountAsync(),
            Mesas = await context.Mesas.CountAsync(),
            Proveedores = await context.Proveedores.CountAsync(),
            Ingredientes = await context.Ingredientes.CountAsync()
        };
    }

    private record ConteoRegistros
    {
        public int Usuarios { get; init; }
        public int Productos { get; init; }
        public int Clientes { get; init; }
        public int Mesas { get; init; }
        public int Proveedores { get; init; }
        public int Ingredientes { get; init; }
    }
} 