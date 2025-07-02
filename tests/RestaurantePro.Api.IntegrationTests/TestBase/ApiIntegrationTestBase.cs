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
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using ProductoCategoria = RestaurantePro.Domain.Core.Productos.Entities.ProductoCategoria;

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
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "AuthenticatedUser-Administrador");
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
            Logger.LogInformation("🧹 Iniciando limpieza de base de datos...");
            
            // Desactivar detección de cambios para mejorar rendimiento
            DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            
            // 🔧 LIMPIEZA COMPLETA Y ROBUSTA: ELIMINAR TODAS LAS ENTIDADES EN ORDEN CORRECTO
            // Primero las entidades que dependen de otras
            await EliminarEntidadesSafely(DbContext, DbContext.ItemsComanda, "ItemsComanda");
            await EliminarEntidadesSafely(DbContext, DbContext.Facturas, "Facturas");
            await EliminarEntidadesSafely(DbContext, DbContext.Comandas, "Comandas");
            await EliminarEntidadesSafely(DbContext, DbContext.Reservaciones, "Reservaciones");
            await EliminarEntidadesSafely(DbContext, DbContext.TarjetasFidelizacion, "TarjetasFidelizacion");
            await EliminarEntidadesSafely(DbContext, DbContext.Mesas, "Mesas");
            await EliminarEntidadesSafely(DbContext, DbContext.Clientes, "Clientes");
            await EliminarEntidadesSafely(DbContext, DbContext.Usuarios, "Usuarios");
            await EliminarEntidadesSafely(DbContext, DbContext.Productos, "Productos");
            await EliminarEntidadesSafely(DbContext, DbContext.Ingredientes, "Ingredientes");
            await EliminarEntidadesSafely(DbContext, DbContext.Proveedores, "Proveedores");
            await EliminarEntidadesSafely(DbContext, DbContext.Notificaciones, "Notificaciones");
            await EliminarEntidadesSafely(DbContext, DbContext.OrdenesCompra, "OrdenesCompra");
            
            // 🔧 LIMPIAR CUALQUIER OTRA TABLA QUE PUEDA EXISTIR
            try
            {
                // Limpiar tablas del sistema usando SQL directo válido para SQLite
                // Solo limpiar tablas que realmente existen
                var tablasSistema = new[]
                {
                    "AspNetUserTokens",
                    "AspNetUserRoles", 
                    "AspNetUserLogins",
                    "AspNetUserClaims",
                    "AspNetRoleClaims",
                    "AspNetUsers",
                    "AspNetRoles"
                };

                foreach (var tabla in tablasSistema)
                {
                    try
                    {
                        await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {tabla}");
                        Logger.LogDebug("✅ Limpiada tabla del sistema: {Tabla}", tabla);
                    }
                    catch (Exception ex)
                    {
                        // Solo logear, no fallar si la tabla no existe
                        Logger.LogDebug("⚠️ No se pudo limpiar tabla del sistema {Tabla}: {Message}", tabla, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("⚠️ Error limpiando tablas del sistema: {Error}", ex.Message);
            }
            
            // Guardar cambios
            await GuardarCambiosConRetry(DbContext, "Limpieza general");
            
            // Reactivar detección de cambios
            DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            
            // 🔧 LIMPIAR CACHE DE EMAILS GENERADOS
            _emailsGeneradosEnEjecucion.Clear();
            
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
            
            // 🔧 CERRAR CONEXIÓN ACTUAL Y RECREAR COMPLETAMENTE
            await DbContext.Database.CloseConnectionAsync();
            
            // Eliminar y recrear la base de datos
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            
            // 🔧 VERIFICAR QUE LAS TABLAS SE CREARON CORRECTAMENTE
            var tables = DbContext.Database.SqlQueryRaw<string>(
                "SELECT name FROM sqlite_master WHERE type='table'").ToList();
            
            Logger.LogInformation($"🔧 Tablas recreadas: {string.Join(", ", tables)}");
            
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
                var count = await dbSet.CountAsync();
                dbSet.RemoveRange(dbSet);
                Logger.LogInformation($"🗑️ Eliminadas {count} entidades de {nombreEntidad}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"⚠️ Error eliminando {nombreEntidad}: {ex.Message}");
            // Continuar con otras entidades
        }
    }
    
    /// <summary>
    /// Elimina entidades usando SQL raw para casos especiales
    /// </summary>
    private async Task EliminarEntidadesSafely(RestauranteProDbContext context, IQueryable<object> query, string nombreEntidad)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM {nombreEntidad}");
            Logger.LogInformation($"🗑️ Eliminadas entidades de {nombreEntidad} usando SQL");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"⚠️ Error eliminando {nombreEntidad}: {ex.Message}");
            // Continuar con otras entidades
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
        // 🔧 EJECUTAR SEEDERS CRÍTICOS PARA QUE LOS TESTS FUNCIONEN
        try
        {
            Logger.LogInformation("🔧 Iniciando configuración de datos base...");
            
            var seedDataRunner = ServiceScope.ServiceProvider.GetRequiredService<SeedDataRunner>();
            Logger.LogInformation("✅ SeedDataRunner obtenido correctamente");
            
            // Ejecutar solo seeders críticos (roles, permisos, etc.)
            await seedDataRunner.RunCriticalOnlyAsync();
            Logger.LogInformation("✅ SeedDataRunner.RunCriticalOnlyAsync() ejecutado correctamente");
            
            Logger.LogInformation("✅ Datos base configurados correctamente");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error configurando datos base: {Error}", ex.Message);
            // No fallar el test si hay problemas con seeders
        }
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
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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
    /// Crea un cliente de prueba con datos únicos
    /// </summary>
    protected async Task<Cliente> CrearClientePrueba(string nombre = null, string apellido = null, string? email = null, string telefono = null, DateTime? fechaNacimiento = null)
    {
        // 🔧 GENERAR DATOS ÚNICOS PARA EVITAR CONFLICTOS
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var nombreFinal = nombre ?? $"Cliente_{timestamp}_{guid.Substring(0, 6)}";
        var apellidoFinal = apellido ?? $"Test_{guid.Substring(4, 4)}";
        var emailFinal = email ?? GenerarEmailValido();
        
        // 🔧 GENERAR NÚMERO DE TELÉFONO MÓVIL CHILENO VÁLIDO (+569XXXXXXXX)
        var random = new Random();
        var telefonoFinal = telefono ?? $"+569{random.Next(10000000, 99999999)}";
        
        var fechaNacimientoFinal = fechaNacimiento ?? DateTime.Today.AddYears(-25);

        // 🔧 CREAR CLIENTE USANDO LA FIRMA CORRECTA
        var clienteNombre = ClienteNombre.Crear(nombreFinal, apellidoFinal);
        var cliente = Cliente.Crear(clienteNombre, emailFinal, telefonoFinal, fechaNacimientoFinal);

        DbContext.Clientes.Add(cliente);
        await GuardarCambiosConRetry(DbContext, "Cliente");
        
        return cliente;
    }

    /// <summary>
    /// Genera un email válido y único para tests
    /// </summary>
    protected static string GenerarEmailValido(string? emailBase = null)
    {
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        // 🔧 GENERAR EMAIL ÚNICO CON TIMESTAMP Y GUID
        var emailUnico = emailBase ?? $"test_{timestamp}_{guid.Substring(0, 8)}@test.com";
        
        // 🔧 VERIFICAR QUE NO SE HA USADO EN ESTA EJECUCIÓN
        if (_emailsGeneradosEnEjecucion.Contains(emailUnico))
        {
            // Si ya existe, agregar más aleatoriedad
            var extraGuid = Guid.NewGuid().ToString("N");
            emailUnico = $"test_{timestamp}_{guid.Substring(0, 4)}_{extraGuid.Substring(0, 4)}@test.com";
        }
        
        // 🔧 AGREGAR AL CACHE DE EMAILS GENERADOS
        _emailsGeneradosEnEjecucion.Add(emailUnico);
        
        return emailUnico;
    }

    /// <summary>
    /// Crea un producto de prueba con datos únicos
    /// </summary>
    protected async Task<Producto> CrearProductoPrueba(string nombre = "Producto Test", decimal precio = 100.00m)
    {
        // 🔧 GENERAR DATOS ÚNICOS PARA EVITAR CONFLICTOS
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var nombreFinal = nombre == "Producto Test" ? $"Producto_{timestamp}_{guid.Substring(0, 6)}" : nombre;
        
        var producto = Producto.Crear(
            nombreFinal,
            $"Descripción del {nombreFinal}",
            new PrecioProducto(precio),
            Guid.NewGuid(), // categoriaId
            "Plato Principal" // categoriaNombre
        );

        DbContext.Productos.Add(producto);
        await GuardarCambiosConRetry(DbContext, "Producto");
        
        return producto;
    }

    /// <summary>
    /// Crea un usuario de prueba con datos únicos
    /// </summary>
    protected async Task<Usuario> CrearUsuarioPrueba(
        string nombreUsuario = null,
        string nombreCompleto = null,
        string email = null,
        RolUsuario rol = RolUsuario.Mesero)
    {
        // 🔧 GENERAR DATOS ÚNICOS PARA EVITAR CONFLICTOS
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var nombreUsuarioFinal = nombreUsuario ?? $"user_{timestamp}_{guid.Substring(0, 6)}";
        var nombreCompletoFinal = nombreCompleto ?? $"Usuario Test {guid.Substring(0, 4)}";
        var emailFinal = email ?? GenerarEmailValido();
        
        var usuario = Usuario.Crear(
            nombreUsuarioFinal,
            nombreCompletoFinal,
            emailFinal,
            rol
        );
        
        DbContext.Usuarios.Add(usuario);
        await GuardarCambiosConRetry(DbContext, "Usuario");
        
        return usuario;
    }

    /// <summary>
    /// Crea una mesa de prueba con datos únicos
    /// </summary>
    protected async Task<Mesa> CrearMesaPrueba(
        int? numero = null, 
        int capacidad = 4, 
        string ubicacion = "Interior",
        EstadoMesa estado = EstadoMesa.Disponible)
    {
        // 🔧 GENERAR DATOS ÚNICOS PARA EVITAR CONFLICTOS
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var numeroFinal = numero ?? (int)(timestamp % 9999) + 1;
        
        // 🔧 SOLO AGREGAR SUFIJO ALEATORIO SI NO SE ESPECIFICA UNA UBICACIÓN ESPECÍFICA
        var ubicacionFinal = string.IsNullOrWhiteSpace(ubicacion) 
            ? $"Ubicacion_{guid.Substring(0, 8)}"
            : ubicacion; // Usar la ubicación exacta que se pasa
        
        var mesa = Mesa.Crear(
            numeroFinal,
            capacidad,
            ubicacionFinal
        );

        // Cambiar el estado si es distinto de Disponible
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
                    mesa.MarcarComoFueraDeServicio("Test");
                    break;
            }
        }

        DbContext.Mesas.Add(mesa);
        await GuardarCambiosConRetry(DbContext, "Mesa");
        
        return mesa;
    }

    /// <summary>
    /// Crea un ingrediente de prueba con datos únicos
    /// </summary>
    protected async Task<Ingrediente> CrearIngredientePrueba(
        string nombre = null,
        string codigo = null,
        decimal stockInicial = 10,
        decimal stockMinimo = 5,
        decimal costoPromedio = 4.0m)
    {
        // 🔧 GENERAR DATOS ÚNICOS PARA EVITAR CONFLICTOS
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var nombreFinal = nombre ?? $"Ingrediente_{timestamp}_{guid.Substring(0, 6)}";
        var codigoFinal = codigo ?? $"ING_{timestamp}_{guid.Substring(0, 4)}";
        
        var ingrediente = Ingrediente.Crear(
            nombreFinal,
            codigoFinal,
            $"Descripción del {nombreFinal}",
            UnidadMedida.Kilogramo,
            stockMinimo,
            stockInicial
        );

        // Establecer costo promedio si es mayor a 0
        if (costoPromedio > 0)
        {
            ingrediente.ActualizarCostoPromedio(costoPromedio);
        }

        DbContext.Ingredientes.Add(ingrediente);
        await GuardarCambiosConRetry(DbContext, "Ingrediente");
        
        return ingrediente;
    }

    /// <summary>
    /// Crea una comanda de prueba en la base de datos
    /// </summary>
    protected async Task<Comanda> CrearComandaPrueba(
        Guid? meseroId = null,
        Guid? clienteId = null,
        Guid? mesaId = null,
        string observaciones = "Comanda de prueba",
        DateTime? fechaCreacion = null)
    {
        var mesero = meseroId.HasValue ? await DbContext.Usuarios.FindAsync(meseroId.Value) : null;
        if (mesero == null)
        {
            mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        }
        var cliente = clienteId.HasValue ? await DbContext.Clientes.FindAsync(clienteId.Value) : null;
        if (cliente == null)
        {
            cliente = await CrearClientePrueba();
        }
        var mesa = mesaId.HasValue ? await DbContext.Mesas.FindAsync(mesaId.Value) : null;
        if (mesa == null)
        {
            mesa = await CrearMesaPrueba();
        }
        
        var fechaFinal = fechaCreacion ?? DateTime.Now;
        var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(
            mesero.Id,
            fechaFinal,
            cliente.Id,
            mesa.Id,
            observaciones
        );
        DbContext.Comandas.Add(comanda);
        await GuardarCambiosConRetry(DbContext, "Comanda");
        Logger.LogInformation($"✅ Comanda creada: {comanda.Id} - Mesero: {mesero.Id} - Cliente: {cliente.Id} - Mesa: {mesa.Id}");
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
    /// Crea un proveedor de prueba con datos únicos
    /// </summary>
    protected async Task<Proveedor> CrearProveedorPrueba(string nombre = null)
    {
        // 🔧 GENERAR DATOS ÚNICOS PARA EVITAR CONFLICTOS
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var nombreFinal = nombre ?? $"Proveedor_{timestamp}_{guid.Substring(0, 6)}";
        var emailFinal = GenerarEmailValido();
        
        // 🔧 GENERAR NÚMERO DE TELÉFONO MÓVIL CHILENO VÁLIDO (+569XXXXXXXX)
        var random = new Random();
        var telefonoFinal = $"+569{random.Next(10000000, 99999999)}";

        var proveedor = Proveedor.Crear(
            nombreFinal,
            nombreFinal, // nombreContacto
            emailFinal,
            telefonoFinal,
            $"Dirección {guid.Substring(0, 8)}",
            $"Ciudad {guid.Substring(0, 5)}",
            $"{guid.Substring(0, 5)}",
            "Chile",
            $"RFC{guid.Substring(0, 8)}",
            $"CuentaBancaria{guid.Substring(0, 8)}",
            30 // diasCredito
        );

        DbContext.Proveedores.Add(proveedor);
        await GuardarCambiosConRetry(DbContext, "Proveedor");
        
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
        string nombreCliente = null,
        string observaciones = "Factura de prueba",
        DateTime? fechaCreacion = null)
    {
        var guid = Guid.NewGuid().ToString("N");
        var cliente = clienteId.HasValue ? await DbContext.Clientes.FindAsync(clienteId.Value) : null;
        if (cliente == null)
        {
            cliente = await CrearClientePrueba();
        }
        var comandas = new List<Comanda>();
        if (comandasIds != null && comandasIds.Any())
        {
            foreach (var comandaId in comandasIds)
            {
                var comanda = await DbContext.Comandas.FindAsync(comandaId);
                if (comanda == null)
                {
                    comanda = await CrearComandaPrueba(clienteId: cliente.Id);
                }
                comandas.Add(comanda);
            }
        }
        else
        {
            var comanda = await CrearComandaPrueba(clienteId: cliente.Id);
            comandas.Add(comanda);
        }
        var numeroFacturaFinal = numeroFactura ?? $"FAC-{guid}";
        var fechaCreacionFinal = fechaCreacion ?? DateTime.UtcNow;
        var factura = Factura.Crear(
            numeroFacturaFinal,
            tipoFactura,
            cliente.Nombre.ToString(),
            cliente.Id,
            null, // identificacionFiscal
            null, // direccionCliente
            comandas.Select(c => c.Id).ToList(),
            observaciones,
            fechaCreacionFinal
        );
        DbContext.Facturas.Add(factura);
        await GuardarCambiosConRetry(DbContext, "Factura");
        Logger.LogInformation($"✅ Factura creada: {factura.Id} - Número: {factura.NumeroFactura} - Cliente: {cliente.Id}");
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
            // Buscar la(s) comanda(s) asociada(s) para obtener el descuento
            var comandas = await DbContext.Comandas.Where(c => comandasIds.Contains(c.Id)).ToListAsync();
            decimal porcentajeDescuento = 0.0m;
            if (comandas.Count == 1)
            {
                porcentajeDescuento = comandas[0].DescuentoFidelizacion ?? 0.0m;
            }
            // Si hay más de una comanda, podrías promediar o tomar el mayor, aquí tomamos el mayor
            else if (comandas.Count > 1)
            {
                porcentajeDescuento = comandas.Max(c => c.DescuentoFidelizacion ?? 0.0m);
            }

            var producto1 = await CrearProductoPrueba("Producto Factura 1", 100.00m);
            var producto2 = await CrearProductoPrueba("Producto Factura 2", 150.00m);
            
            var detalle1 = DetalleFactura.Crear(
                factura.Id,
                producto1.Id,
                producto1.Nombre,
                2,
                producto1.Precio!.Valor,
                16.0m, // 16% IVA
                porcentajeDescuento); // Aplica descuento de la comanda

            var detalle2 = DetalleFactura.Crear(
                factura.Id,
                producto2.Id,
                producto2.Nombre,
                1,
                producto2.Precio!.Valor,
                16.0m, // 16% IVA
                porcentajeDescuento); // Aplica descuento de la comanda

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

    /// <summary>
    /// Crea una tarjeta de fidelización de prueba en la base de datos
    /// </summary>
    protected async Task<TarjetaFidelizacion> CrearTarjetaFidelizacionPrueba(
        Guid? clienteId = null,
        string codigo = null)
    {
        var guid = Guid.NewGuid().ToString("N");
        var cliente = clienteId.HasValue ? await DbContext.Clientes.FindAsync(clienteId.Value) : null;
        if (cliente == null)
        {
            cliente = await CrearClientePrueba();
        }
        var codigoFinal = codigo ?? $"TARJ-{guid}";
        var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, codigoFinal);
        DbContext.TarjetasFidelizacion.Add(tarjeta);
        await GuardarCambiosConRetry(DbContext, "TarjetaFidelizacion");
        Logger.LogInformation($"✅ Tarjeta de fidelización creada: {tarjeta.Id} - Código: {tarjeta.Codigo} - Cliente: {cliente.Id}");
        return tarjeta;
    }
}

/// <summary>
/// Métodos de extensión para HttpClient en tests de integración
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Método de extensión para enviar JSON en requests DELETE
    /// </summary>
    public static async Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        return await client.SendAsync(request);
    }
} 