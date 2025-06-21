namespace RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Clientes;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.Productos;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Operaciones.Reservaciones;
using RestaurantePro.Domain.Operaciones.Preparaciones;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Core.Notificaciones;
using RestaurantePro.Domain.Proveedores;

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
    /// Limpia todas las tablas de la base de datos usando estrategia robusta para SQLite in-memory
    /// </summary>
    protected virtual async Task LimpiarBaseDeDatos()
    {
        try
        {
            // 🚀 ESTRATEGIA ROBUSTA PARA SQLITE IN-MEMORY: 
            // 1. Usar un nuevo contexto para evitar problemas de tracking
            // 2. Eliminar en orden correcto (dependientes primero)
            // 3. Manejar errores de concurrencia sin fallar los tests
            // 4. Usar transacciones para consistencia
            
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            
            // Desactivar el tracking para evitar problemas de concurrencia
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            // 🗑️ ELIMINAR EN ORDEN CORRECTO (dependientes primero)
            
            // 1. Movimientos de inventario (owned entities dentro de Ingrediente)
            // No se pueden eliminar directamente, se eliminan con el ingrediente
            
            // 2. Items de comandas (dependen de Comanda y Producto)
            if (context.Set<ItemComanda>().Any())
            {
                var itemsComanda = await context.Set<ItemComanda>().ToListAsync();
                context.Set<ItemComanda>().RemoveRange(itemsComanda);
                await GuardarCambiosConRetry(context, "items de comanda");
            }
            
            // 3. Comandas (dependen de Usuario, Cliente, Mesa)
            if (context.Comandas.Any())
            {
                var comandas = await context.Comandas.ToListAsync();
                context.Comandas.RemoveRange(comandas);
                await GuardarCambiosConRetry(context, "comandas");
            }
            
            // 4. Reservaciones (dependen de Cliente, Mesa)
            if (context.Reservaciones.Any())
            {
                var reservaciones = await context.Reservaciones.ToListAsync();
                context.Reservaciones.RemoveRange(reservaciones);
                await GuardarCambiosConRetry(context, "reservaciones");
            }
            
            // 5. Preparaciones (dependen de Comanda)
            if (context.Preparaciones.Any())
            {
                var preparaciones = await context.Preparaciones.ToListAsync();
                context.Preparaciones.RemoveRange(preparaciones);
                await GuardarCambiosConRetry(context, "preparaciones");
            }
            
            // 6. Ingredientes (contienen owned entities de movimientos)
            if (context.Ingredientes.Any())
            {
                var ingredientes = await context.Ingredientes.ToListAsync();
                context.Ingredientes.RemoveRange(ingredientes);
                await GuardarCambiosConRetry(context, "ingredientes");
            }
            
            // 7. Productos (pueden tener relaciones con recetas)
            if (context.Productos.Any())
            {
                var productos = await context.Productos.ToListAsync();
                context.Productos.RemoveRange(productos);
                await GuardarCambiosConRetry(context, "productos");
            }
            
            // 8. Mesas (dependen de Usuario para asignaciones)
            if (context.Mesas.Any())
            {
                var mesas = await context.Mesas.ToListAsync();
                context.Mesas.RemoveRange(mesas);
                await GuardarCambiosConRetry(context, "mesas");
            }
            
            // 9. Clientes (dependen de Usuario para creación)
            if (context.Clientes.Any())
            {
                var clientes = await context.Clientes.ToListAsync();
                context.Clientes.RemoveRange(clientes);
                await GuardarCambiosConRetry(context, "clientes");
            }
            
            // 10. Proveedores (pueden tener contactos)
            if (context.Proveedores.Any())
            {
                var proveedores = await context.Proveedores.ToListAsync();
                context.Proveedores.RemoveRange(proveedores);
                await GuardarCambiosConRetry(context, "proveedores");
            }
            
            // 11. Usuarios (entidad raíz, se elimina al final)
            if (context.Usuarios.Any())
            {
                var usuarios = await context.Usuarios.ToListAsync();
                context.Usuarios.RemoveRange(usuarios);
                await GuardarCambiosConRetry(context, "usuarios");
            }
            
            // 12. Notificaciones (dependen de Usuario)
            if (context.Notificaciones.Any())
            {
                var notificaciones = await context.Notificaciones.ToListAsync();
                context.Notificaciones.RemoveRange(notificaciones);
                await GuardarCambiosConRetry(context, "notificaciones");
            }
            
            // 13. Facturas (dependen de Cliente)
            if (context.Facturas.Any())
            {
                var facturas = await context.Facturas.ToListAsync();
                context.Facturas.RemoveRange(facturas);
                await GuardarCambiosConRetry(context, "facturas");
            }
            
            // 14. Tarjetas de fidelización (dependen de Cliente)
            if (context.TarjetasFidelizacion.Any())
            {
                var tarjetas = await context.TarjetasFidelizacion.ToListAsync();
                context.TarjetasFidelizacion.RemoveRange(tarjetas);
                await GuardarCambiosConRetry(context, "tarjetas de fidelización");
            }
            
            // 15. Promociones
            if (context.Promociones.Any())
            {
                var promociones = await context.Promociones.ToListAsync();
                context.Promociones.RemoveRange(promociones);
                await GuardarCambiosConRetry(context, "promociones");
            }
            
            // 16. Órdenes de compra (dependen de Proveedor)
            if (context.OrdenesCompra.Any())
            {
                var ordenes = await context.OrdenesCompra.ToListAsync();
                context.OrdenesCompra.RemoveRange(ordenes);
                await GuardarCambiosConRetry(context, "órdenes de compra");
            }
            
            Logger.LogInformation("✅ Base de datos limpiada correctamente (SQLite in-memory)");
        }
        catch (Exception ex)
        {
            // 🛡️ NO FALLAR LOS TESTS POR ERRORES DE LIMPIEZA
            Logger.LogWarning(ex, "⚠️ Error al limpiar la base de datos, pero continuando con el test: {Message}", ex.Message);
            
            // Intentar limpieza alternativa más agresiva
            await LimpiezaAlternativa();
        }
    }
    
    /// <summary>
    /// Método alternativo de limpieza más agresivo para casos de error
    /// </summary>
    private async Task LimpiezaAlternativa()
    {
        try
        {
            Logger.LogInformation("🧹 Iniciando limpieza alternativa de BD...");
            
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            
            // Desactivar validaciones y tracking
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            // Limpieza más agresiva usando SQL directo con manejo individual de errores
            var tablas = new[]
            {
                "ItemsComanda",
                "Comandas", 
                "Reservaciones",
                "Preparaciones",
                "Ingredientes",
                "Productos",
                "Mesas",
                "Clientes",
                "Proveedores",
                "Usuarios",
                "Notificaciones",
                "Facturas",
                "TarjetasFidelizacion",
                "Promociones",
                "OrdenesCompra",
                "MovimientosInventario"
            };

            foreach (var tabla in tablas)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync($"DELETE FROM {tabla}");
                    Logger.LogDebug("✅ Limpiada tabla: {Tabla}", tabla);
                }
                catch (Exception ex)
                {
                    // Solo logear el error, no fallar el test
                    Logger.LogDebug("⚠️ No se pudo limpiar tabla {Tabla}: {Message}", tabla, ex.Message);
                }
            }
            
            Logger.LogInformation("✅ Limpieza alternativa completada");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error en limpieza alternativa: {Message}", ex.Message);
            // No lanzar excepción, permitir que el test continúe
        }
    }
    
    /// <summary>
    /// Guarda cambios con retry para manejar errores de concurrencia
    /// </summary>
    private async Task GuardarCambiosConRetry(RestauranteProDbContext context, string entidad, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await context.SaveChangesAsync();
                return;
            }
            catch (DbUpdateConcurrencyException ex) when (i < maxRetries - 1)
            {
                Logger.LogWarning("🔄 Reintento {Retry}/{MaxRetries} para {Entidad}: {Message}", 
                    i + 1, maxRetries, entidad, ex.Message);
                
                // Esperar un poco antes del siguiente intento
                await Task.Delay(50 * (i + 1));
                
                // Refrescar el contexto
                context.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("⚠️ Error al guardar {Entidad}: {Message}", entidad, ex.Message);
                // No reintentar para otros tipos de errores
                break;
            }
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
    protected async Task<Cliente> CrearClientePrueba(string nombre = "Cliente Test", string? email = null)
    {
        var emailUnico = email ?? $"test_{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var clienteNombre = ClienteNombre.Crear("Cliente", "Test");
        var cliente = Cliente.Crear(
            clienteNombre,
            emailUnico,
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
        string? nombreUsuario = null, 
        string nombreCompleto = "Usuario Test", 
        string? email = null,
        RolUsuario rol = RolUsuario.Mesero)
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var usuarioUnico = nombreUsuario ?? $"usuario_{sufijo}";
        var emailUnico = email ?? $"usuario_{sufijo}@test.com";
        var usuario = Usuario.Crear(
            usuarioUnico,
            nombreCompleto,
            emailUnico,
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
        int? numero = null, 
        int capacidad = 4, 
        string ubicacion = "Interior",
        EstadoMesa estado = EstadoMesa.Disponible)
    {
        var numeroUnico = numero ?? new Random().Next(1000, 9999);
        var mesa = Mesa.Crear(
            numeroUnico,
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
        // Crear entidades dependientes si NO se proporcionan los IDs
        var mesero = meseroId.HasValue ? null : await CrearUsuarioPrueba("mesero.test", "Mesero Test", "mesero@test.com", RolUsuario.Mesero);
        var cliente = clienteId.HasValue ? null : await CrearClientePrueba("Cliente Comanda", "cliente@test.com");
        var mesa = mesaId.HasValue ? null : await CrearMesaPrueba(99, 4); // Usar un número de mesa por defecto

        // Usar los IDs proporcionados o los de las entidades creadas
        var meseroIdFinal = meseroId ?? mesero!.Id;
        var clienteIdFinal = clienteId ?? cliente!.Id;
        var mesaIdFinal = mesaId ?? mesa!.Id;

        // Verificar que las entidades referenciadas existen en la BD
        if (meseroId.HasValue)
        {
            var meseroExiste = await DbContext.Usuarios.FindAsync(meseroId.Value);
            if (meseroExiste == null)
            {
                throw new InvalidOperationException($"El mesero con ID {meseroId.Value} no existe en la base de datos");
            }
        }

        if (clienteId.HasValue)
        {
            var clienteExiste = await DbContext.Clientes.FindAsync(clienteId.Value);
            if (clienteExiste == null)
            {
                throw new InvalidOperationException($"El cliente con ID {clienteId.Value} no existe en la base de datos");
            }
        }

        if (mesaId.HasValue)
        {
            var mesaExiste = await DbContext.Mesas.FindAsync(mesaId.Value);
            if (mesaExiste == null)
            {
                throw new InvalidOperationException($"La mesa con ID {mesaId.Value} no existe en la base de datos");
            }
        }

        var comanda = Comanda.Crear(
            meseroIdFinal,
            clienteIdFinal,
            mesaIdFinal,
            observaciones
        );

        DbContext.Comandas.Add(comanda);
        await DbContext.SaveChangesAsync();
        return comanda;
    }

    /// <summary>
    /// Crea un proveedor de prueba en la base de datos
    /// </summary>
    protected async Task<Proveedor> CrearProveedorPrueba(string nombre = "Proveedor Test")
    {
        var proveedor = Proveedor.Crear(
            nombre,
            "Contacto Test",
            "proveedor@test.com",
            "555-0000",
            "Calle Falsa 123",
            "Ciudad Test",
            "12345",
            "País Test",
            "RFC1234567",
            "Cuenta Bancaria Test",
            30
        );
        DbContext.Proveedores.Add(proveedor);
        await DbContext.SaveChangesAsync();
        return proveedor;
    }
} 