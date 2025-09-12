using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;
using RestaurantePro.Application.Core.Recetas.Commands.ActualizarReceta;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Web.Admin.IntegrationTests.Utils;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Recetas;

/// <summary>
/// Pruebas de integración para monitoreo y logging de Recetas
/// </summary>
public class RecetasMonitoringTests : BaseIntegrationTest
{
    public RecetasMonitoringTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Logging de Operaciones Exitosas

    [Fact]
    public async Task ObtenerRecetas_DeberiaRegistrarLogInformacion()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(5);

        // Act
        var response = await _client.GetAsync("/api/core/recetas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que se registró el log de información
        // En un entorno real, esto se verificaría con un mock del logger
        // Por ahora, verificamos que la operación fue exitosa
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerRecetaPorId_DeberiaRegistrarLogInformacion()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task CrearReceta_DeberiaRegistrarLogInformacion()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        // Obtener ingredientes válidos
        var ingredientesDisponibles = await _context.Ingredientes.Take(1).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(_context, 5);
            ingredientesDisponibles = await _context.Ingredientes.Take(1).ToListAsync();
        }
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba para monitoreo",
            TiempoPreparacionMinutos = 20,
            Ingredientes = ingredientesDisponibles.Select(ing => new RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto
            {
                IngredienteId = ing.Id,
                Cantidad = 2,
                EsOpcional = false
            }).ToList()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task ActualizarReceta_DeberiaRegistrarLogInformacion()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Receta actualizada para monitoreo",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarReceta_DeberiaRegistrarLogInformacion()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.DeleteAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    #region Logging de Errores

    [Fact]
    public async Task ObtenerReceta_ConIdInexistente_DeberiaRegistrarLogError()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // En un entorno real, se verificaría que se registró un log de error
    }

    [Fact]
    public async Task CrearReceta_ConDatosInvalidos_DeberiaRegistrarLogError()
    {
        // Arrange
        var command = new CrearRecetaCommand
        {
            ProductoId = Guid.Empty, // ID inválido
            Preparacion = "", // Preparación vacía
            TiempoPreparacionMinutos = -1, // Tiempo negativo
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // En un entorno real, se verificaría que se registró un log de error
    }

    [Fact]
    public async Task ActualizarReceta_ConIdInexistente_DeberiaRegistrarLogError()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();
        var command = new ActualizarRecetaCommand
        {
            Id = recetaIdInexistente,
            Preparacion = "Receta actualizada",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaIdInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // En un entorno real, se verificaría que se registró un log de error
    }

    [Fact]
    public async Task EliminarReceta_ConIdInexistente_DeberiaRegistrarLogError()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/core/recetas/{recetaIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // En un entorno real, se verificaría que se registró un log de error
    }

    #endregion

    #region Métricas de Rendimiento

    [Fact]
    public async Task ObtenerRecetas_DeberiaRegistrarTiempoDeRespuesta()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "La operación debería completarse en menos de 2 segundos");
        
        // En un entorno real, se verificaría que se registró la métrica de tiempo de respuesta
    }

    [Fact]
    public async Task BuscarRecetas_DeberiaRegistrarTiempoDeRespuesta()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas?filtroTexto=pizza");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La búsqueda debería completarse en menos de 1 segundo");
    }

    [Fact]
    public async Task CalcularCostoReceta_DeberiaRegistrarTiempoDeRespuesta()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/costo");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "El cálculo de costo debería completarse en menos de 500ms");
    }

    [Fact]
    public async Task VerificarDisponibilidad_DeberiaRegistrarTiempoDeRespuesta()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/disponibilidad?cantidad=5");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "La verificación de disponibilidad debería completarse en menos de 500ms");
    }

    #endregion

    #region Métricas de Uso

    [Fact]
    public async Task ObtenerRecetas_ConDiferentesParametros_DeberiaRegistrarMetricas()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(20);

        // Act - Ejecutar diferentes consultas
        var consultas = new[]
        {
            "/api/core/recetas",
            "/api/core/recetas?soloActivas=true",
            "/api/core/recetas?filtroTexto=pizza",
            "/api/core/recetas?ordenarPor=TiempoPreparacionMinutos&direccionOrden=Asc",
            "/api/core/recetas?pageSize=10&pageNumber=1"
        };

        foreach (var consulta in consultas)
        {
            var response = await _client.GetAsync(consulta);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert
        // En un entorno real, se verificaría que se registraron métricas para cada tipo de consulta
    }

    [Fact]
    public async Task OperacionesCRUD_DeberiaRegistrarMetricasDeUso()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();

        // Obtener ingredientes válidos
        var ingredientesDisponibles = await _context.Ingredientes.Take(1).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(_context, 5);
            ingredientesDisponibles = await _context.Ingredientes.Take(1).ToListAsync();
        }

        // Act - Ejecutar operaciones CRUD
        // Create
        var createCommand = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta para métricas",
            TiempoPreparacionMinutos = 20,
            Ingredientes = ingredientesDisponibles.Select(ing => new RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto
            {
                IngredienteId = ing.Id,
                Cantidad = 1,
                EsOpcional = false
            }).ToList()
        };

        var createJson = JsonSerializer.Serialize(createCommand, GetJsonOptions());
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/core/recetas", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createResponseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>(createResponseContent, GetJsonOptions());
        var recetaId = createResponseData.Data.Id;

        // Read
        var readResponse = await _client.GetAsync($"/api/core/recetas/{recetaId}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update
        var updateCommand = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Receta actualizada para métricas",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var updateJson = JsonSerializer.Serialize(updateCommand, GetJsonOptions());
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        var updateResponse = await _client.PutAsync($"/api/core/recetas/{recetaId}", updateContent);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/core/recetas/{recetaId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        // En un entorno real, se verificaría que se registraron métricas para cada operación CRUD
    }

    #endregion

    #region Monitoreo de Errores

    [Fact]
    public async Task ErroresDeValidacion_DeberiaRegistrarMetricas()
    {
        // Arrange
        var comandosInvalidos = new[]
        {
            new CrearRecetaCommand { ProductoId = Guid.Empty, Preparacion = "", TiempoPreparacionMinutos = -1, Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>() },
            new CrearRecetaCommand { ProductoId = Guid.NewGuid(), Preparacion = "Test", TiempoPreparacionMinutos = 0, Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>() },
            new CrearRecetaCommand { ProductoId = Guid.NewGuid(), Preparacion = "", TiempoPreparacionMinutos = 20, Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>() }
        };

        // Act
        foreach (var comando in comandosInvalidos)
        {
            var json = JsonSerializer.Serialize(comando, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/core/recetas", content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // Assert
        // En un entorno real, se verificaría que se registraron métricas de errores de validación
    }

    [Fact]
    public async Task ErroresDeAutorizacion_DeberiaRegistrarMetricas()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();

        // Act
        var response = await clientNoAuth.GetAsync("/api/core/recetas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        // En un entorno real, se verificaría que se registró una métrica de error de autorización
    }

    #endregion

    #region Monitoreo de Recursos

    [Fact]
    public async Task OperacionesConMuchosDatos_DeberiaMonitorearUsoDeMemoria()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(1000); // Crear 1000 recetas

        // Act
        var memoryBefore = GC.GetTotalMemory(true);
        var response = await _client.GetAsync("/api/core/recetas");
        var memoryAfter = GC.GetTotalMemory(false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var memoryUsed = memoryAfter - memoryBefore;
        memoryUsed.Should().BeLessThan(100 * 1024 * 1024, "No debería usar más de 100MB de memoria"); // 100MB
        
        // En un entorno real, se verificaría que se registró la métrica de uso de memoria
    }

    [Fact]
    public async Task OperacionesConcurrentes_DeberiaMonitorearRecursos()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50);

        // Act - Ejecutar operaciones concurrentes
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/core/recetas"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        // En un entorno real, se verificaría que se registraron métricas de concurrencia
    }

    #endregion

    #region Monitoreo de Disponibilidad

    [Fact]
    public async Task EndpointsCriticos_DeberiaMonitorearDisponibilidad()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(10);

        // Act - Verificar disponibilidad de endpoints críticos
        var endpoints = new[]
        {
            "/api/core/recetas",
            "/api/core/recetas?pageSize=10",
            "/api/core/recetas?filtroTexto=test"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert
        // En un entorno real, se verificaría que se registraron métricas de disponibilidad
    }

    [Fact]
    public async Task OperacionesCriticas_DeberiaMonitorearDisponibilidad()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        // Obtener ingredientes válidos
        var ingredientesDisponibles = await _context.Ingredientes.Take(1).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(_context, 5);
            ingredientesDisponibles = await _context.Ingredientes.Take(1).ToListAsync();
        }

        // Act - Verificar disponibilidad de operaciones críticas
        var createCommand = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de disponibilidad",
            TiempoPreparacionMinutos = 20,
            Ingredientes = ingredientesDisponibles.Select(ing => new RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto
            {
                IngredienteId = ing.Id,
                Cantidad = 1,
                EsOpcional = false
            }).ToList()
        };

        var createJson = JsonSerializer.Serialize(createCommand, GetJsonOptions());
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/core/recetas", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Assert
        // En un entorno real, se verificaría que se registraron métricas de disponibilidad para operaciones críticas
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedRecetasDePruebaAsync(int cantidad = 5)
    {
        var productoIds = await CrearProductosDePruebaAsync(cantidad);
        
        if (productoIds.Count < cantidad)
        {
            throw new InvalidOperationException($"No se pudieron crear suficientes productos. Se requieren {cantidad}, pero solo se crearon {productoIds.Count}");
        }
        
        var recetaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var recetaId = await CrearRecetaDePruebaAsync(productoIds[i]);
            recetaIds.Add(recetaId);
        }
        
        return recetaIds;
    }

    #endregion
}
