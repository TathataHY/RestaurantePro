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
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;

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
    protected readonly IDateTimeService DateTimeService;

    private static readonly HashSet<string> _emailsGeneradosEnEjecucion = new();

    protected ApiIntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        ServiceScope = factory.Services.CreateScope();
        DbContext = ServiceScope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        Logger = ServiceScope.ServiceProvider.GetRequiredService<ILogger<ApiIntegrationTestBase>>();
        DateTimeService = ServiceScope.ServiceProvider.GetRequiredService<IDateTimeService>();
    }

    /// <summary>
    /// Configuración inicial antes de cada test
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Configurar autenticación por defecto
        ConfigurarAutenticacionPorDefecto();
        
        // Limpiar completamente la base de datos antes de cada test
        await LimpiarBaseDeDatosCompletamente();
        
        // Configurar datos base si es necesario
        await ConfigurarDatosBase();
        
        Logger.LogInformation("✅ Test inicializado correctamente");
    }

    /// <summary>
    /// Configura autenticación automática con rol de Administrador para todos los tests
    /// </summary>
    protected virtual void ConfigurarAutenticacionPorDefecto()
    {
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "AuthenticatedUser");
    }

    /// <summary>
    /// Configura autenticación con un rol específico para el test
    /// </summary>
    protected virtual void ConfigurarAutenticacionConRol(string rol)
    {
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", $"AuthenticatedUser-{rol}");
    }

    /// <summary>
    /// Configura autenticación con un usuario específico que pase el ID del usuario en el header de autorización.
    /// </summary>
    protected virtual void ConfigurarAutenticacionConUsuario(Guid usuarioId, string rol = "Administrador")
    {
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", $"User_{usuarioId}_{rol}");
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
    /// Crea un nuevo contexto de base de datos para evitar problemas de tracking
    /// Útil para verificar cambios en la BD después de operaciones
    /// </summary>
    protected RestauranteProDbContext CreateNewDbContext()
    {
        var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        // Configurar para evitar tracking
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        return context;
    }

    /// <summary>
    /// Limpia todas las tablas de la base de datos usando estrategia robusta para SQLite in-memory
    /// </summary>
    protected virtual async Task LimpiarBaseDeDatos()
    {
        try
        {
            Logger.LogInformation("�� Iniciando limpieza de base de datos...");
            
            // Desactivar detección de cambios para mejorar rendimiento
            DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            
            // Limpiar en orden específico para evitar problemas de FK
            await EliminarEntidadesSafely(DbContext, DbContext.Facturas, "Facturas");
            await EliminarEntidadesSafely(DbContext, DbContext.ItemsComanda, "ItemsComanda");
            await EliminarEntidadesSafely(DbContext, DbContext.Comandas, "Comandas");
            await EliminarEntidadesSafely(DbContext, DbContext.Reservaciones, "Reservaciones");
            await EliminarEntidadesSafely(DbContext, DbContext.Mesas, "Mesas");
            await EliminarEntidadesSafely(DbContext, DbContext.Clientes, "Clientes");
            await EliminarEntidadesSafely(DbContext, DbContext.Usuarios, "Usuarios");
            await EliminarEntidadesSafely(DbContext, DbContext.Productos, "Productos");
            await EliminarEntidadesSafely(DbContext, DbContext.Ingredientes, "Ingredientes");
            await EliminarEntidadesSafely(DbContext, DbContext.Proveedores, "Proveedores");
            await EliminarEntidadesSafely(DbContext, DbContext.Notificaciones, "Notificaciones");
            
            // Guardar cambios
            await GuardarCambiosConRetry(DbContext, "Limpieza general");
            
            // Reactivar detección de cambios
            DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            
            Logger.LogInformation("✅ Base de datos limpiada correctamente");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error durante la limpieza de base de datos");
            await LimpiezaAlternativa();
        }
    }
    
    /// <summary>
    /// Limpieza más agresiva que elimina y recrea la base de datos
    /// </summary>
    protected virtual async Task LimpiarBaseDeDatosCompletamente()
    {
        try
        {
            Logger.LogInformation("🧹 Iniciando limpieza completa de base de datos...");
            
            // Cerrar conexión actual
            await DbContext.Database.CloseConnectionAsync();
            
            // Eliminar y recrear la base de datos
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            
            Logger.LogInformation("✅ Base de datos recreada completamente");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error durante la limpieza completa de base de datos");
            // Fallback a limpieza normal
            await LimpiarBaseDeDatos();
        }
    }
    
    /// <summary>
    /// Elimina entidades de forma segura con manejo de errores individual
    /// </summary>
    private async Task EliminarEntidadesSafely<T>(RestauranteProDbContext context, DbSet<T> dbSet, string nombreEntidad) where T : class
    {
        try
        {
            if (dbSet.Any())
            {
                var entidades = await dbSet.ToListAsync();
                dbSet.RemoveRange(entidades);
                await context.SaveChangesAsync();
                Logger.LogDebug("✅ Eliminadas {Count} entidades de {Entidad}", entidades.Count, nombreEntidad);
            }
        }
        catch (Exception ex)
        {
            // Solo logear el error, no fallar el test
            Logger.LogDebug("⚠️ No se pudieron eliminar entidades de {Entidad}: {Message}", nombreEntidad, ex.Message);
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
        // Generar un email siempre válido sin patrones repetitivos
        var emailValido = GenerarEmailValido(email);
        var clienteNombre = ClienteNombre.Crear("Cliente", "Test");
        var telefono = "+1234567890";
        
        var cliente = Cliente.Crear(clienteNombre, emailValido, telefono, DateTime.Now.AddYears(-25));

        await DbContext.Clientes.AddAsync(cliente);
        await DbContext.SaveChangesAsync();
        
        return cliente;
    }

    /// <summary>
    /// Genera un email válido sin patrones repetitivos, incluso si se pasa un email explícito
    /// </summary>
    protected static string GenerarEmailValido(string? emailBase = null)
    {
        var random = new Random();
        var palabras = new[] { "usuario", "cliente", "test", "demo", "admin", "user", "guest", "visitor", "member", "customer" };
        var sufijos = new[] { "2024", "2025", "test", "demo", "dev", "qa", "prod", "stage", "local", "temp" };
        var dominios = new[] { "testmail.com", "example.com", "test.com", "demo.com", "local.com" };
        
        string ObtenerSufijoDiferente(string palabra)
        {
            var sufijosValidos = sufijos.Where(s => !s.Equals(palabra, StringComparison.OrdinalIgnoreCase)).ToArray();
            return sufijosValidos[random.Next(sufijosValidos.Length)];
        }
        
        string GenerarEmail()
        {
            var palabra = palabras[random.Next(palabras.Length)];
            var sufijo = ObtenerSufijoDiferente(palabra);
            var dominio = dominios[random.Next(dominios.Length)];
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000;
            var guid = Guid.NewGuid().ToString("N")[..6];
            
            return $"{palabra}.{sufijo}.{timestamp}{guid}@{dominio}";
        }
        
        // Usar Email.CreateForTesting() que está diseñado específicamente para tests
        // y omite las validaciones estrictas de patrones repetitivos
        for (int i = 0; i < 50; i++) // Máximo 50 intentos (por seguridad extrema)
        {
            var email = GenerarEmail();
            
            // Verificar que no esté duplicado en esta ejecución
            if (_emailsGeneradosEnEjecucion.Contains(email))
                continue;
                
            // Usar CreateForTesting() que es más permisivo para tests
            if (Email.TryCreateForTesting(email, out _))
            {
                _emailsGeneradosEnEjecucion.Add(email);
                return email;
            }
        }
        
        // Fallback: generar un email simple y único
        var fallbackEmail = $"test.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.{Guid.NewGuid():N}@test.com";
        _emailsGeneradosEnEjecucion.Add(fallbackEmail);
        return fallbackEmail;
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
        string nombreUsuario = "test.user",
        string nombreCompleto = null,
        string email = null,
        RolUsuario rol = RolUsuario.Mesero)
    {
        // Generar un nombre de usuario único para evitar conflictos de constraint UNIQUE
        var nombreUsuarioUnico = $"{nombreUsuario}.{Guid.NewGuid():N}";
        var nombreCompletoFinal = nombreCompleto ?? $"Usuario {nombreUsuarioUnico}";
        var emailFinal = email ?? $"{nombreUsuarioUnico}@test.com";
        
        // Usar el factory method de la entidad Usuario
        var usuario = Usuario.Crear(
            nombreUsuarioUnico,
            nombreCompletoFinal,
            emailFinal,
            rol
        );
        
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
        // Si se proporciona un número específico, usarlo directamente
        // Solo generar un número único si no se proporciona número
        int numeroFinal;
        if (numero.HasValue)
        {
            numeroFinal = numero.Value;
        }
        else
        {
            // Generar un número único más simple y seguro
            var random = new Random();
            var numeroBase = random.Next(1000, 9999);
            var sufijo = random.Next(1000, 9999);
            numeroFinal = numeroBase * 10000 + sufijo; // Máximo: 99999999
        }
        var mesa = Mesa.Crear(
            numeroFinal,
            capacidad,
            ubicacion
        );

        // Establecer el estado si es diferente al por defecto
        if (estado != EstadoMesa.Disponible)
        {
            switch (estado)
            {
                case EstadoMesa.Ocupada:
                    mesa.MarcarComoOcupada();
                    break;
                case EstadoMesa.Reservada:
                    mesa.MarcarComoReservada();
                    break;
                case EstadoMesa.FueraDeServicio:
                    mesa.MarcarComoFueraDeServicio("Test automatizado");
                    break;
            }
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
        return await CrearComandaPrueba(meseroId: meseroId, clienteId: clienteId, mesaId: mesaId, observaciones: observaciones, estado: EstadoComanda.Creada);
    }

    /// <summary>
    /// Crea una comanda de prueba en la base de datos con estado específico
    /// </summary>
    protected async Task<Comanda> CrearComandaPrueba(
        Guid? meseroId = null,
        Guid? clienteId = null,
        Guid? mesaId = null,
        string observaciones = "Comanda de prueba",
        EstadoComanda estado = EstadoComanda.Creada)
    {
        // Crear entidades dependientes si NO se proporcionan los IDs
        var mesero = meseroId.HasValue ? null : await CrearUsuarioPrueba("mesero.test", "Mesero Test", null, RolUsuario.Mesero);
        var cliente = clienteId.HasValue ? null : await CrearClientePrueba($"ClienteCmd_{Guid.NewGuid().ToString("N")[..8]}", $"cmd_{Guid.NewGuid().ToString("N")[..8]}@test.com");
        var mesa = mesaId.HasValue ? null : await CrearMesaPrueba(null, 4); // Usar número generado automáticamente

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

        // Cambiar el estado si es diferente al por defecto
        if (estado != EstadoComanda.Creada)
        {
            // Usar reflection para cambiar el estado ya que es una propiedad privada
            var estadoProperty = typeof(Comanda).GetProperty("Estado");
            if (estadoProperty != null)
            {
                estadoProperty.SetValue(comanda, estado);
            }
        }

        DbContext.Comandas.Add(comanda);
        await DbContext.SaveChangesAsync();
        return comanda;
    }

    /// <summary>
    /// Crea un detalle de comanda de prueba en la base de datos
    /// </summary>
    protected async Task<ItemComanda> CrearDetalleComandaPrueba(
        Guid comandaId,
        Guid productoId,
        int cantidad = 1,
        string observaciones = "Detalle de prueba")
    {
        // Verificar que la comanda existe
        var comanda = await DbContext.Comandas.FindAsync(comandaId);
        if (comanda == null)
        {
            throw new InvalidOperationException($"La comanda con ID {comandaId} no existe en la base de datos");
        }

        // Verificar que el producto existe
        var producto = await DbContext.Productos.FindAsync(productoId);
        if (producto == null)
        {
            throw new InvalidOperationException($"El producto con ID {productoId} no existe en la base de datos");
        }

        // Verificar que el producto tiene nombre y precio válidos
        if (string.IsNullOrEmpty(producto.Nombre))
        {
            throw new InvalidOperationException($"El producto con ID {productoId} no tiene un nombre válido");
        }

        if (producto.Precio == null)
        {
            throw new InvalidOperationException($"El producto con ID {productoId} no tiene un precio válido");
        }

        var detalle = ItemComanda.Crear(
            comandaId,
            productoId,
            producto.Nombre,
            cantidad,
            producto.Precio.Valor,
            observaciones
        );

        DbContext.ItemsComanda.Add(detalle);
        await DbContext.SaveChangesAsync();
        return detalle;
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

    /// <summary>
    /// Crea una factura de prueba en la base de datos
    /// </summary>
    protected async Task<Factura> CrearFacturaPrueba(
        Guid? clienteId = null,
        List<Guid>? comandasIds = null,
        string numeroFactura = null,
        TipoFactura tipoFactura = TipoFactura.Normal,
        string nombreCliente = "Cliente Factura Test",
        string observaciones = "Factura de prueba",
        DateTime? fechaCreacion = null)
    {
        // Crear cliente si no se proporciona
        if (!clienteId.HasValue)
        {
            var sufijo = Guid.NewGuid().ToString("N")[..8];
            var emailUnico = $"cliente.factura.{sufijo}@test.com";
            var cliente = await CrearClientePrueba(nombreCliente, emailUnico);
            clienteId = cliente.Id;
        }

        // Crear comanda si no se proporciona
        if (comandasIds == null || !comandasIds.Any())
        {
            var comanda = await CrearComandaPrueba(meseroId: null, clienteId: clienteId.Value, mesaId: null, observaciones: "Comanda de prueba");
            comandasIds = new List<Guid> { comanda.Id };
        }

        // Generar número de factura si no se proporciona
        if (string.IsNullOrEmpty(numeroFactura))
        {
            numeroFactura = $"FAC-TEST-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // Crear la factura usando el factory method
        var factura = Factura.Crear(
            numeroFactura,
            tipoFactura,
            nombreCliente,
            clienteId,
            null, // identificacionFiscal
            null, // direccionCliente
            comandasIds,
            observaciones,
            fechaCreacion ?? DateTimeService.Now);

        // Guardar en la base de datos
        DbContext.Facturas.Add(factura);
        await DbContext.SaveChangesAsync();

        Logger.LogInformation("✅ Factura de prueba creada: {NumeroFactura} (ID: {Id})", factura.NumeroFactura, factura.Id);

        return factura;
    }

    /// <summary>
    /// Crea una factura con detalles de productos
    /// </summary>
    protected async Task<Factura> CrearFacturaConDetallesPrueba(
        Guid? clienteId = null,
        List<Guid>? comandasIds = null,
        string numeroFactura = null,
        TipoFactura tipoFactura = TipoFactura.Normal,
        DateTime? fechaCreacion = null)
    {
        // Crear cliente si no se proporciona
        if (!clienteId.HasValue)
        {
            var sufijo = Guid.NewGuid().ToString("N")[..8];
            var emailUnico = $"cliente.detalles.{sufijo}@test.com";
            var cliente = await CrearClientePrueba("Cliente Detalles Test", emailUnico);
            clienteId = cliente.Id;
        }

        // Crear comanda con productos si no se proporciona
        if (comandasIds == null || !comandasIds.Any())
        {
            var comanda = await CrearComandaPrueba(meseroId: null, clienteId: clienteId.Value, mesaId: null, observaciones: "Comanda de prueba");
            // Agregar productos a la comanda
            var producto1 = await CrearProductoPrueba("Producto 1", 100.00m);
            var producto2 = await CrearProductoPrueba("Producto 2", 150.00m);
            await CrearDetalleComandaPrueba(comanda.Id, producto1.Id, 2);
            await CrearDetalleComandaPrueba(comanda.Id, producto2.Id, 1);
            comandasIds = new List<Guid> { comanda.Id };
        }

        // Generar número de factura si no se proporciona
        if (string.IsNullOrEmpty(numeroFactura))
        {
            numeroFactura = $"FAC-DET-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // Crear la factura
        var factura = await CrearFacturaPrueba(clienteId, comandasIds, numeroFactura, tipoFactura, "Cliente Factura Test", "Factura de prueba", fechaCreacion);

        // Si no hay detalles, agregarlos
        var detalles = await DbContext.Set<DetalleFactura>().Where(d => d.FacturaId == factura.Id).ToListAsync();
        if (detalles == null || !detalles.Any())
        {
            var producto1 = await CrearProductoPrueba("Producto Factura 1", 100.00m);
            var producto2 = await CrearProductoPrueba("Producto Factura 2", 150.00m);
            
            var detalle1 = DetalleFactura.Crear(
                factura.Id,
                producto1.Id,
                producto1.Nombre,
                2,
                producto1.Precio!.Valor,
                16.0m, // 16% IVA
                0.0m); // Sin descuento

            var detalle2 = DetalleFactura.Crear(
                factura.Id,
                producto2.Id,
                producto2.Nombre,
                1,
                producto2.Precio!.Valor,
                16.0m, // 16% IVA
                0.0m); // Sin descuento

            DbContext.Set<DetalleFactura>().AddRange(detalle1, detalle2);
            await DbContext.SaveChangesAsync();
            await RecalcularTotalFactura(factura);
            Logger.LogInformation("✅ Detalles de factura agregados: {CantidadDetalles} productos por defecto", 2);
        }

        return factura;
    }

    /// <summary>
    /// Recalcula el total de una factura basado en sus detalles
    /// </summary>
    protected async Task RecalcularTotalFactura(Factura factura)
    {
        var detalles = await DbContext.Set<DetalleFactura>()
            .Where(d => d.FacturaId == factura.Id)
            .ToListAsync();
            
        if (detalles.Any())
        {
            var subtotal = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            var impuestos = detalles.Sum(d => (d.Cantidad * d.PrecioUnitario) * (d.PorcentajeImpuesto / 100));
            var descuentos = detalles.Sum(d => (d.Cantidad * d.PrecioUnitario) * (d.PorcentajeDescuento / 100));
            var total = subtotal + impuestos - descuentos;
            
            // Usar reflection para establecer todas las propiedades (ya que son privadas)
            var subtotalProperty = typeof(Factura).GetProperty("Subtotal");
            var impuestosProperty = typeof(Factura).GetProperty("TotalImpuestos");
            var descuentosProperty = typeof(Factura).GetProperty("TotalDescuentos");
            var totalProperty = typeof(Factura).GetProperty("Total");
            
            if (subtotalProperty != null)
                subtotalProperty.SetValue(factura, subtotal);
            if (impuestosProperty != null)
                impuestosProperty.SetValue(factura, impuestos);
            if (descuentosProperty != null)
                descuentosProperty.SetValue(factura, descuentos);
            if (totalProperty != null)
                totalProperty.SetValue(factura, total);
                
            await DbContext.SaveChangesAsync();
            Logger.LogInformation("✅ Total de factura recalculado: Subtotal={Subtotal:C}, Impuestos={Impuestos:C}, Descuentos={Descuentos:C}, Total={Total:C}", 
                subtotal, impuestos, descuentos, total);
        }
    }
} 