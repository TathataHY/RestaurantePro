namespace RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

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

    /// <summary>
    /// Crea un usuario de prueba en la base de datos
    /// </summary>
    protected async Task<Usuario> CrearUsuarioPrueba(
        string nombreUsuario = "usuario.test", 
        string nombreCompleto = "Usuario Test", 
        string email = "usuario@test.com",
        RolUsuario rol = RolUsuario.Mesero)
    {
        var usuario = Usuario.Crear(
            nombreUsuario,
            nombreCompleto,
            email,
            rol
        );

        // Confirmar la cuenta para que esté activo
        usuario.ConfirmarCuenta();

        DbContext.Usuarios.Add(usuario);
        await DbContext.SaveChangesAsync();
        return usuario;
    }

    /// <summary>
    /// Crea una mesa de prueba en la base de datos
    /// </summary>
    protected async Task<Mesa> CrearMesaPrueba(
        int numero = 1, 
        int capacidad = 4, 
        string ubicacion = "Interior",
        EstadoMesa estado = EstadoMesa.Disponible)
    {
        var mesa = Mesa.Crear(
            numero,
            capacidad,
            ubicacion
        );

        // Establecer el estado si es diferente al por defecto
        if (estado != EstadoMesa.Disponible)
        {
            // Aquí deberías usar el método correspondiente para cambiar el estado
            // Por ahora, asumimos que se crea en estado Disponible
        }

        DbContext.Mesas.Add(mesa);
        await DbContext.SaveChangesAsync();
        return mesa;
    }

    /// <summary>
    /// Crea un ingrediente de prueba en la base de datos
    /// </summary>
    protected async Task<Ingrediente> CrearIngredientePrueba(
        string nombre = "Tomate",
        string codigo = "TOM-001",
        decimal stockInicial = 10,
        decimal stockMinimo = 5)
    {
        var ingrediente = Ingrediente.Crear(
            nombre,
            codigo,
            "Ingrediente de prueba",
            UnidadMedida.Unidad,
            stockMinimo,
            stockInicial
        );

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        return ingrediente;
    }

    /// <summary>
    /// Crea una comanda de prueba en la base de datos
    /// </summary>
    protected async Task<Comanda> CrearComandaPrueba(
        Guid? meseroId = null,
        Guid? clienteId = null,
        Guid? mesaId = null,
        string observaciones = "Comanda de prueba")
    {
        // Crear entidades dependientes si no se proporcionan
        var mesero = meseroId.HasValue ? null : await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = clienteId.HasValue ? null : await CrearClientePrueba("Cliente Comanda", "cliente@test.com");
        var mesa = mesaId.HasValue ? null : await CrearMesaPrueba(99, 4); // Usar un número de mesa por defecto

        var comanda = Comanda.Crear(
            meseroId ?? mesero!.Id,
            clienteId ?? cliente!.Id,
            mesaId ?? mesa!.Id,
            observaciones
        );

        DbContext.Comandas.Add(comanda);
        await DbContext.SaveChangesAsync();
        return comanda;
    }
} 