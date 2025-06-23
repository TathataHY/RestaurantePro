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
            Logger.LogInformation("🧹 Iniciando limpieza de base de datos...");
            
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
        var timestamp = DateTimeOffset.UtcNow.Ticks.ToString()[^8..];
        var emailUnico = email ?? $"test_{timestamp}@example.com";
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
        var sufijo = Guid.NewGuid().ToString("N")[..8] + DateTimeOffset.UtcNow.Ticks.ToString()[^4..];
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
        string observaciones = "Factura de prueba")
    {
        // Crear cliente si no se proporciona
        if (!clienteId.HasValue)
        {
            var cliente = await CrearClientePrueba(nombreCliente, "factura@test.com");
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
            clienteId.Value,
            null, // identificacionFiscal
            null, // direccionCliente
            comandasIds,
            observaciones,
            DateTime.Now);

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
        TipoFactura tipoFactura = TipoFactura.Normal)
    {
        // Crear cliente si no se proporciona
        if (!clienteId.HasValue)
        {
            var cliente = await CrearClientePrueba("Cliente Detalles Test", "detalles@test.com");
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
        var factura = await CrearFacturaPrueba(clienteId, comandasIds, numeroFactura, tipoFactura);

        // Agregar detalles de factura usando los productos de la comanda
        var comandaConItems = await DbContext.Comandas
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => comandasIds!.Contains(c.Id));
            
        if (comandaConItems != null && comandaConItems.Items.Any())
        {
            // Usar los items de la comanda para crear los detalles de la factura
            foreach (var item in comandaConItems.Items)
            {
                // Obtener el producto para conseguir el nombre
                var producto = await DbContext.Productos.FindAsync(item.ProductoId);
                var nombreProducto = producto?.Nombre ?? "Producto Sin Nombre";
                
                var detalle = DetalleFactura.Crear(
                    factura.Id,
                    item.ProductoId,
                    nombreProducto,
                    item.Cantidad,
                    item.PrecioUnitario,
                    16.0m, // 16% IVA
                    0.0m); // Sin descuento

                DbContext.Set<DetalleFactura>().Add(detalle);
            }
            
            await DbContext.SaveChangesAsync();
            
            // Recalcular el total de la factura
            await RecalcularTotalFactura(factura);
            
            Logger.LogInformation("✅ Detalles de factura agregados: {CantidadDetalles} productos", comandaConItems.Items.Count);
        }
        else
        {
            // Si no hay items en la comanda, crear productos por defecto
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
            
            // Recalcular el total de la factura
            await RecalcularTotalFactura(factura);

            Logger.LogInformation("✅ Detalles de factura agregados: {CantidadDetalles} productos por defecto", 2);
        }

        return factura;
    }

    /// <summary>
    /// Recalcula el total de una factura basado en sus detalles
    /// </summary>
    private async Task RecalcularTotalFactura(Factura factura)
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
            
            // Usar reflection para establecer el total (ya que es privado)
            var totalProperty = typeof(Factura).GetProperty("Total");
            if (totalProperty != null)
            {
                totalProperty.SetValue(factura, total);
                await DbContext.SaveChangesAsync();
                Logger.LogInformation("✅ Total de factura recalculado: {Total:C}", total);
            }
        }
    }
} 