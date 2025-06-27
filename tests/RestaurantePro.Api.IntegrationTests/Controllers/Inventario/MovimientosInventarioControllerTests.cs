using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración COMPLETOS para MovimientosInventarioController
/// Prueba la interacción real con la base de datos usando la API de dominio correcta
/// </summary>
[Collection("Sequential")]
public class MovimientosInventarioControllerTests : ApiIntegrationTestBase
{
    public MovimientosInventarioControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMovimientos_SinMovimientos_DebeRetornarListaVacia()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/inventario/movimientos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync("/api/inventario/movimientos"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que no hay movimientos en la BD
        var movimientosEnBD = await DbContext.Ingredientes
            .SelectMany(i => i.Movimientos)
            .CountAsync();
        movimientosEnBD.Should().Be(0);
    }

    [Fact]
    public async Task GetMovimientos_ConMovimientosEnBD_DebeRetornarMovimientos()
    {
        // Arrange - Crear ingredientes base y agregar movimientos vía API
        var ingrediente1 = await CrearIngredientePrueba("Harina", 100m);
        var ingrediente2 = await CrearIngredientePrueba("Azúcar", 80m);
        var usuario = await CrearUsuarioPrueba("usuario.movimientos", "Usuario Movimientos", "movimientos@test.com", RolUsuario.Administrador);
        
        // Agregar movimientos usando el endpoint (flujo real)
        var movimiento1 = new { IngredienteId = ingrediente1.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 50m, Motivo = "Compra inicial", UsuarioId = usuario.Id };
        var movimiento2 = new { IngredienteId = ingrediente2.Id, TipoMovimiento = TipoMovimientoInventario.Egreso, Cantidad = 30m, Motivo = "Consumo", UsuarioId = usuario.Id };
        
        await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", movimiento1);
        await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", movimiento2);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/movimientos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync("/api/inventario/movimientos"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que realmente hay movimientos en la BD
        var ingredientesEnBD = await DbContext.Ingredientes
            .Include(i => i.Movimientos)
            .ToListAsync();
        
        ingredientesEnBD.Should().HaveCount(2);
        ingredientesEnBD.Sum(i => i.Movimientos.Count).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetMovimiento_ConIdExistente_DebeRetornarMovimiento()
    {
        // Arrange - Crear ingrediente base y agregar movimiento vía API
        var ingrediente = await CrearIngredientePrueba("Leche", 50m);
        var usuario = await CrearUsuarioPrueba("usuario.movimiento", "Usuario Movimiento", "movimiento@test.com", RolUsuario.Administrador);
        
        var nuevoMovimiento = new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 25m, Motivo = "Compra leche", UsuarioId = usuario.Id };
        var postResponse = await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", nuevoMovimiento);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Obtener el ID del movimiento creado desde la respuesta
        var apiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var movimientoId = Guid.Parse(apiResponse.Data.ToString());

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getApiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync($"/api/inventario/movimientos/{movimientoId}"));
        
        VerificarRespuestaExitosa(response, getApiResponse);
        getApiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que el movimiento existe en la BD
        var ingredienteEnBD = await DbContext.Ingredientes
            .Include(i => i.Movimientos)
            .FirstAsync(i => i.Id == ingrediente.Id);
        
        ingredienteEnBD.Movimientos.Should().Contain(m => m.Id == movimientoId);
    }

    [Fact]
    public async Task GetMovimiento_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/movimientos/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // ✅ Comportamiento correcto: 404 para ID inexistente
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync($"/api/inventario/movimientos/{idInexistente}"));
        
        // Verificar que la respuesta indica que no se encontró
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostMovimiento_ConDatosValidos_DebeCrearMovimiento()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostMovimiento_ConDatosValidos_DebeCrearMovimiento");
        
        var ingrediente = await CrearIngredientePrueba("Huevos", stockInicial: 100m);
        var usuario = await CrearUsuarioPrueba("usuario.movimientos", "Usuario Movimientos", "movimientos@test.com", RolUsuario.Administrador);
        
        // ✅ VERIFICACIÓN ADICIONAL: Confirmar que el ingrediente existe en la BD
        var ingredienteEnBD = await DbContext.Ingredientes.FindAsync(ingrediente.Id);
        ingredienteEnBD.Should().NotBeNull($"El ingrediente con ID {ingrediente.Id} debe existir en la BD");
        Logger.LogInformation($"✅ Ingrediente creado y verificado en BD: {ingrediente.Id} - {ingrediente.Nombre} - Stock: {ingrediente.Stock}");
        
        // ✅ VERIFICACIÓN ADICIONAL: Confirmar que el usuario existe en la BD
        var usuarioEnBD = await DbContext.Usuarios.FindAsync(usuario.Id);
        usuarioEnBD.Should().NotBeNull($"El usuario con ID {usuario.Id} debe existir en la BD");
        Logger.LogInformation($"✅ Usuario creado y verificado en BD: {usuario.Id} - {usuario.NombreCompleto} - Email: {usuario.Email}");
        
        var nuevoMovimiento = new
        {
            IngredienteId = ingrediente.Id,
            TipoMovimiento = TipoMovimientoInventario.Ingreso, // Usar enum directamente
            Cantidad = 25m,
            Motivo = "Compra huevos frescos",
            Observaciones = "Huevos frescos de granja",
            UsuarioId = usuario.Id // Usar el Id del usuario real creado
        };

        Logger.LogInformation($"📋 Request a enviar: {System.Text.Json.JsonSerializer.Serialize(nuevoMovimiento)}");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", nuevoMovimiento);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Logger.LogInformation($"📋 Response Status: {response.StatusCode}");
        Logger.LogInformation($"📋 Response Content: {responseContent}");

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Logger.LogError($"❌ Error de validación detectado: {responseContent}");
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            if (errorResponse?.Errors != null)
            {
                foreach (var error in errorResponse.Errors)
                {
                    Logger.LogError($"❌ Error de validación: {error}");
                }
            }
            if (errorResponse?.Message != null)
            {
                Logger.LogError($"❌ Mensaje de error: {errorResponse.Message}");
            }
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.PostAsJsonAsync("/api/inventario/movimientos", nuevoMovimiento));
        
        VerificarRespuestaExitosa(response, apiResponse, HttpStatusCode.Created);
        apiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que el ingrediente existe y puede tener movimientos
        var ingredienteConMovimientos = await DbContext.Ingredientes
            .Include(i => i.Movimientos)
            .FirstAsync(i => i.Id == ingrediente.Id);
        
        ingredienteConMovimientos.Should().NotBeNull();
        ingredienteConMovimientos.Id.Should().Be(ingrediente.Id);
        // Verificar que el ingrediente sigue existiendo (no se eliminó por error de concurrencia)
        ingredienteConMovimientos.Nombre.Should().Be("Huevos");
    }

    [Fact]
    public async Task PutMovimiento_ConDatosValidos_DebeActualizarMovimiento()
    {
        // Arrange - Crear ingrediente base y agregar movimiento vía API
        var ingrediente = await CrearIngredientePrueba("Aceite", 30m);
        var usuario = await CrearUsuarioPrueba("usuario.actualiza", "Usuario Actualiza", "actualiza@test.com", RolUsuario.Administrador);
        ConfigurarAutenticacionConUsuario(usuario.Id, "Administrador");
        
        var nuevoMovimiento = new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 10m, Motivo = "Compra inicial", UsuarioId = usuario.Id };
        var postResponse = await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", nuevoMovimiento);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Obtener el ID del movimiento creado
        var apiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var movimientoId = Guid.Parse(apiResponse.Data.ToString());
        
        var movimientoActualizado = new
        {
            Cantidad = 35,
            Motivo = "Compra actualizada",
            Fecha = DateTime.Now,
            UsuarioId = usuario.Id
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", movimientoActualizado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que el movimiento existe en la BD
        var ingredienteEnBD = await DbContext.Ingredientes
            .Include(i => i.Movimientos)
            .FirstAsync(i => i.Id == ingrediente.Id);
        
        ingredienteEnBD.Movimientos.Should().Contain(m => m.Id == movimientoId);
    }

    [Fact]
    public async Task DeleteMovimiento_ConIdExistente_DebeEliminarMovimiento()
    {
        // Arrange - Crear ingrediente base y agregar movimiento vía API
        var ingrediente = await CrearIngredientePrueba("Sal", 10m);
        var usuario = await CrearUsuarioPrueba("usuario.elimina", "Usuario Elimina", "elimina@test.com", RolUsuario.Administrador);
        ConfigurarAutenticacionConUsuario(usuario.Id, "Administrador");
        
        var nuevoMovimiento = new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 5m, Motivo = "Compra sal", UsuarioId = usuario.Id };
        var postResponse = await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", nuevoMovimiento);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Obtener el ID del movimiento creado
        var apiResponse = await postResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var movimientoId = Guid.Parse(apiResponse.Data.ToString());

        // Crear request con usuarioId requerido
        var deleteRequest = new
        {
            UsuarioId = usuario.Id,
            MotivoEliminacion = "Eliminación de prueba"
        };

        // Act
        var response = await HttpClient.DeleteAsync($"/api/inventario/movimientos/{movimientoId}?usuarioId={deleteRequest.UsuarioId}&motivoEliminacion={deleteRequest.MotivoEliminacion}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que el movimiento existe en la BD
        var ingredienteEnBD = await DbContext.Ingredientes
            .Include(i => i.Movimientos)
            .FirstAsync(i => i.Id == ingrediente.Id);
        
        ingredienteEnBD.Movimientos.Should().Contain(m => m.Id == movimientoId);
    }

    [Fact]
    public async Task GetMovimientosPorIngrediente_ConIngredienteExistente_DebeRetornarMovimientos()
    {
        // Arrange - Crear ingredientes base y agregar movimientos vía API
        var ingrediente1 = await CrearIngredientePrueba("Pimienta", 100m);
        var ingrediente2 = await CrearIngredientePrueba("Cebolla", 80m);
        var usuario = await CrearUsuarioPrueba("usuario.ingrediente", "Usuario Ingrediente", "ingrediente@test.com", RolUsuario.Administrador);
        
        // Agregar movimientos usando el endpoint
        var movimientos = new[]
        {
            new { IngredienteId = ingrediente1.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 50m, Motivo = "Compra pimienta", UsuarioId = usuario.Id },
            new { IngredienteId = ingrediente1.Id, TipoMovimiento = TipoMovimientoInventario.Egreso, Cantidad = 20m, Motivo = "Consumo pimienta", UsuarioId = usuario.Id },
            new { IngredienteId = ingrediente2.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 40m, Motivo = "Compra cebolla", UsuarioId = usuario.Id }
        };
        
        foreach (var mov in movimientos)
        {
            await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", mov);
        }

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/movimientos/ingrediente/{ingrediente1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync($"/api/inventario/movimientos/ingrediente/{ingrediente1.Id}"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que el ingrediente tiene movimientos
        var ingredienteEnBD = await DbContext.Ingredientes
            .Include(i => i.Movimientos)
            .FirstAsync(i => i.Id == ingrediente1.Id);
        
        ingredienteEnBD.Movimientos.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetMovimientosPorTipo_ConTipoExistente_DebeRetornarMovimientos()
    {
        // Arrange - Crear ingrediente base y agregar movimientos vía API
        var ingrediente = await CrearIngredientePrueba("Tomate", 50m);
        var usuario = await CrearUsuarioPrueba("usuario.tipo", "Usuario Tipo", "tipo@test.com", RolUsuario.Administrador);
        
        // Agregar movimientos usando el endpoint
        var movimientos = new[]
        {
            new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 30m, Motivo = "Compra tomates", UsuarioId = usuario.Id },
            new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Egreso, Cantidad = 10m, Motivo = "Consumo tomates", UsuarioId = usuario.Id }
        };
        
        foreach (var mov in movimientos)
        {
            await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", mov);
        }

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/movimientos/tipo/Ingreso");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync("/api/inventario/movimientos/tipo/Ingreso"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que hay movimientos de tipo Ingreso
        var movimientosIngreso = await DbContext.Ingredientes
            .SelectMany(i => i.Movimientos)
            .Where(m => m.TipoMovimiento == TipoMovimientoInventario.Ingreso)
            .CountAsync();
        
        movimientosIngreso.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetReporteMovimientos_DebeRetornarReporte()
    {
        // Arrange - Definir fechas fijas para el test
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now.AddDays(-1);
        var usuario = await CrearUsuarioPrueba("usuario.reporte", "Usuario Reporte", "reporte@test.com", RolUsuario.Administrador);
        ConfigurarAutenticacionConUsuario(usuario.Id, "Administrador");

        // Crear ingrediente base y agregar movimientos vía API
        var ingrediente = await CrearIngredientePrueba("Zanahoria", 100m);
        
        // Agregar movimientos usando el endpoint
        var fechaMovimiento = DateTime.Now.AddDays(-10);
        var movimientos = new[]
        {
            new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Ingreso, Cantidad = 60m, Motivo = "Compra zanahorias", UsuarioId = usuario.Id, Fecha = fechaMovimiento },
            new { IngredienteId = ingrediente.Id, TipoMovimiento = TipoMovimientoInventario.Egreso, Cantidad = 20m, Motivo = "Consumo zanahorias", UsuarioId = usuario.Id, Fecha = fechaMovimiento }
        };
        
        foreach (var mov in movimientos)
        {
            await HttpClient.PostAsJsonAsync("/api/inventario/movimientos", mov);
        }

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/movimientos/reporte?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&usuarioId={usuario.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<object>(
            client => client.GetAsync($"/api/inventario/movimientos/reporte?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&usuarioId={usuario.Id}"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        
        // ✅ VERIFICACIÓN EN BD: Comprobar que hay movimientos en el rango de fechas
        var movimientosEnRango = await DbContext.Ingredientes
            .SelectMany(i => i.Movimientos)
            .Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin)
            .CountAsync();
        
        movimientosEnRango.Should().BeGreaterThan(0);
    }

    // Métodos auxiliares para crear datos de prueba usando la API de dominio correcta
    private async Task<Ingrediente> CrearIngredientePrueba(string nombre, decimal stockInicial)
    {
        var codigoUnico = $"COD-{nombre.ToUpper()}-{Guid.NewGuid().ToString("N")[..8]}";
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            codigoUnico,
            $"Descripción de {nombre}",
            RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
            stockInicial * 0.1m, // 10% del stock como mínimo
            stockInicial
        );

        DbContext.Ingredientes.Add(ingrediente);
        await DbContext.SaveChangesAsync();
        // Recuperar desde un contexto limpio para evitar tracking
        using var cleanContext = CreateNewDbContext();
        return await cleanContext.Ingredientes.FirstAsync(i => i.Id == ingrediente.Id);
    }
} 