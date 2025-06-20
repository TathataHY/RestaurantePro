namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Clase base para todos los tests de integración de la API.
/// Proporciona infraestructura común para testing HTTP con base de datos en memoria.
/// </summary>
public abstract class ApiIntegrationTestBase : IAsyncLifetime, IDisposable
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;
    protected readonly IServiceScope ServiceScope;
    protected readonly RestauranteProDbContext DbContext;
    protected readonly ILogger Logger;

    protected ApiIntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        ServiceScope = factory.Services.CreateScope();
        DbContext = ServiceScope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        Logger = ServiceScope.ServiceProvider.GetRequiredService<ILogger<ApiIntegrationTestBase>>();
    }

    /// <summary>
    /// Configuración inicial antes de cada test
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Limpiar la base de datos antes de cada test
        await LimpiarBaseDeDatos();
        
        // Configurar datos base necesarios para los tests
        await ConfigurarDatosBase();
    }

    /// <summary>
    /// Limpieza después de cada test
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await LimpiarBaseDeDatos();
        ServiceScope?.Dispose();
    }

    /// <summary>
    /// Limpieza síncrona para IDisposable
    /// </summary>
    public virtual void Dispose()
    {
        ServiceScope?.Dispose();
        HttpClient?.Dispose();
    }

    /// <summary>
    /// Limpia todas las tablas de la base de datos usando estrategia compatible con InMemory
    /// </summary>
    protected virtual async Task LimpiarBaseDeDatos()
    {
        try
        {
            // 🚀 ESTRATEGIA COMPATIBLE CON INMEMORY: Limpiar usando RemoveRange
            
            // Limpiar entidades principales (orden importante por las FK)
            if (DbContext.Productos.Any())
            {
                DbContext.Productos.RemoveRange(DbContext.Productos.ToList());
            }
            
            if (DbContext.Clientes.Any())
            {
                DbContext.Clientes.RemoveRange(DbContext.Clientes.ToList());
            }
            
            // TODO: Agregar otras entidades cuando se implementen
            // if (DbContext.Comandas.Any())
            // {
            //     DbContext.Comandas.RemoveRange(DbContext.Comandas.ToList());
            // }
            
            await DbContext.SaveChangesAsync();
            Logger.LogInformation("Base de datos limpiada correctamente (InMemory)");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al limpiar la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Configura datos base necesarios para los tests (roles, configuraciones, etc.)
    /// </summary>
    protected virtual async Task ConfigurarDatosBase()
    {
        // Este método puede ser sobrescrito por tests específicos
        // que necesiten datos base particulares
        await Task.CompletedTask;
    }

    /// <summary>
    /// Ejecuta una petición HTTP y deserializa la respuesta a ApiResponse<T>
    /// </summary>
    protected async Task<ApiResponse<T>> ExecuteAndDeserializeAsync<T>(
        Func<HttpClient, Task<HttpResponseMessage>> httpCall)
    {
        var response = await httpCall(HttpClient);
        var jsonContent = await response.Content.ReadAsStringAsync();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, options)!;
    }

    /// <summary>
    /// Verifica que una respuesta HTTP sea exitosa y contenga los datos esperados
    /// </summary>
    protected static void VerificarRespuestaExitosa<T>(
        HttpResponseMessage response, 
        ApiResponse<T> apiResponse, 
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        response.StatusCode.Should().Be(expectedStatusCode);
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Verifica que una respuesta HTTP sea de error y contenga los errores esperados
    /// </summary>
    protected static void VerificarRespuestaError<T>(
        HttpResponseMessage response,
        ApiResponse<T> apiResponse,
        HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
    {
        response.StatusCode.Should().Be(expectedStatusCode);
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Crea un cliente de prueba en la base de datos
    /// </summary>
    protected async Task<Cliente> CrearClientePrueba(string nombre = "Cliente Test", string email = "test@example.com")
    {
        var clienteNombre = ClienteNombre.Crear("Cliente", "Test");
        var cliente = Cliente.Crear(
            clienteNombre,
            email,
            "555-1234",
            DateTime.Now.AddYears(-25)
        );

        DbContext.Clientes.Add(cliente);
        await DbContext.SaveChangesAsync();
        return cliente;
    }

    /// <summary>
    /// Crea un producto de prueba en la base de datos
    /// </summary>
    protected async Task<Producto> CrearProductoPrueba(string nombre = "Producto Test", decimal precio = 100.00m)
    {
        var producto = Producto.Crear(
            nombre,
            "Descripción de prueba",
            new PrecioProducto(precio),
            Guid.NewGuid(),
            "Categoria Test"
        );

        DbContext.Productos.Add(producto);
        await DbContext.SaveChangesAsync();
        return producto;
    }
} 