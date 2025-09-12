using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Recetas;

/// <summary>
/// Pruebas de integración para rendimiento de Recetas
/// </summary>
public class RecetasPerformanceTests : BaseIntegrationTest
{
    public RecetasPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Rendimiento de Consultas

    [Fact]
    public async Task ObtenerRecetas_ConMuchasRecetas_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100); // Crear 100 recetas

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "La consulta de recetas debería ser rápida");
    }

    [Fact]
    public async Task ObtenerRecetas_ConPaginacionGrande_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas?pageSize=50");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1500, "La paginación grande debería ser rápida");
    }

    [Fact]
    public async Task BuscarRecetas_ConFiltroTexto_DeberiaSerRapida()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas?filtroTexto=pizza");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "La búsqueda por texto debería ser muy rápida");
    }

    [Fact]
    public async Task ObtenerRecetas_ConFiltrosCombinados_DeberiaSerRapida()
    {
        // Arrange
        var (productoId, _) = await SeedRecetasConProductoAsync(20);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas?productoId={productoId}&soloActivas=true&ordenarPor=TiempoPreparacionMinutos&direccionOrden=Asc");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "Los filtros combinados deberían ser rápidos");
    }

    [Fact]
    public async Task ObtenerRecetaPorId_ConRecetaExistente_DeberiaSerRapida()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(50);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "La consulta por ID debería ser muy rápida");
    }

    [Fact]
    public async Task ObtenerRecetasPorProducto_ConMuchasRecetas_DeberiaSerRapida()
    {
        // Arrange
        var (productoId, _) = await SeedRecetasConProductoAsync(30);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/producto/{productoId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300, "La consulta por producto debería ser rápida");
    }

    #endregion

    #region Rendimiento de Cálculos

    [Fact]
    public async Task CalcularCostoReceta_ConRecetaCompleja_DeberiaSerRapida()
    {
        // Arrange
        var recetaIds = await SeedRecetasComplejasAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/costo");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "El cálculo de costo debería ser rápido");
    }

    [Fact]
    public async Task CalcularCostoReceta_ConMultiplesIngredientes_DeberiaSerRapida()
    {
        // Arrange
        var recetaIds = await SeedRecetasConMuchosIngredientesAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/costo");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300, "El cálculo con muchos ingredientes debería ser rápido");
    }

    [Fact]
    public async Task VerificarDisponibilidad_ConRecetaCompleja_DeberiaSerRapida()
    {
        // Arrange
        var recetaIds = await SeedRecetasComplejasAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/disponibilidad?cantidad=5");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "La verificación de disponibilidad debería ser rápida");
    }

    [Fact]
    public async Task VerificarDisponibilidad_ConCantidadGrande_DeberiaSerRapida()
    {
        // Arrange
        var recetaIds = await SeedRecetasComplejasAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/disponibilidad?cantidad=100");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300, "La verificación con cantidad grande debería ser rápida");
    }

    #endregion

    #region Rendimiento de Operaciones CRUD

    [Fact]
    public async Task CrearReceta_ConDatosValidos_DeberiaSerRapida()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba para rendimiento",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 2, EsOpcional = false },
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 1, EsOpcional = true }
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/core/recetas", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "La creación de receta debería ser rápida");
    }

    [Fact]
    public async Task CrearReceta_ConMuchosIngredientes_DeberiaSerRapida()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>();
        for (int i = 0; i < 20; i++) // 20 ingredientes
        {
            ingredientes.Add(new RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = 1 + i,
                EsOpcional = i % 3 == 0
            });
        }
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta compleja con muchos ingredientes",
            TiempoPreparacionMinutos = 45,
            Ingredientes = ingredientes
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/core/recetas", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La creación con muchos ingredientes debería ser rápida");
    }

    #endregion

    #region Rendimiento de Carga

    [Fact]
    public async Task ObtenerRecetas_ConCargaAlta_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(200); // Crear 200 recetas

        // Act - Ejecutar múltiples consultas simultáneas
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/core/recetas"));
        }

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Las consultas simultáneas deberían completarse rápidamente");
    }

    [Fact]
    public async Task BuscarRecetas_ConCargaAlta_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(150);

        // Act - Ejecutar múltiples búsquedas simultáneas
        var tasks = new List<Task<HttpResponseMessage>>();
        var terminos = new[] { "pizza", "pasta", "ensalada", "bebida", "postre" };
        
        for (int i = 0; i < 15; i++)
        {
            var termino = terminos[i % terminos.Length];
            tasks.Add(_client.GetAsync($"/api/core/recetas?filtroTexto={termino}"));
        }

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Las búsquedas simultáneas deberían completarse rápidamente");
    }

    [Fact]
    public async Task CalcularCostos_ConCargaAlta_DeberiaMantenerRendimiento()
    {
        // Arrange
        var recetaIds = await SeedRecetasComplejasAsync(50);

        // Act - Ejecutar múltiples cálculos simultáneos
        var tasks = new List<Task<HttpResponseMessage>>();
        foreach (var recetaId in recetaIds.Take(20)) // Calcular costo de 20 recetas
        {
            tasks.Add(_client.GetAsync($"/api/core/recetas/{recetaId}/costo"));
        }

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Los cálculos simultáneos deberían completarse rápidamente");
    }

    #endregion

    #region Rendimiento de Memoria

    [Fact]
    public async Task ObtenerRecetas_ConMuchosDatos_NoDeberiaConsumirMuchaMemoria()
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
        memoryUsed.Should().BeLessThan(50 * 1024 * 1024, "No debería consumir más de 50MB de memoria"); // 50MB
    }

    [Fact]
    public async Task BuscarRecetas_ConMuchosDatos_NoDeberiaConsumirMuchaMemoria()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(1000);

        // Act
        var memoryBefore = GC.GetTotalMemory(true);
        var response = await _client.GetAsync("/api/core/recetas?filtroTexto=test");
        var memoryAfter = GC.GetTotalMemory(false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var memoryUsed = memoryAfter - memoryBefore;
        memoryUsed.Should().BeLessThan(10 * 1024 * 1024, "No debería consumir más de 10MB de memoria"); // 10MB
    }

    #endregion

    #region Rendimiento de Ordenamiento

    [Fact]
    public async Task ObtenerRecetas_ConOrdenamientoComplejo_DeberiaSerRapida()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas?ordenarPor=TiempoPreparacionMinutos&direccionOrden=Desc");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "El ordenamiento debería ser rápido");
    }

    [Fact]
    public async Task ObtenerRecetas_ConMultiplesOrdenamientos_DeberiaSerRapida()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100);

        var ordenamientos = new[]
        {
            "TiempoPreparacionMinutos",
            "FechaCreacion",
            "CostoTotal"
        };

        // Act & Assert
        foreach (var ordenamiento in ordenamientos)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync($"/api/core/recetas?ordenarPor={ordenamiento}&direccionOrden=Asc");
            stopwatch.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(300, $"El ordenamiento por {ordenamiento} debería ser rápido");
        }
    }

    #endregion

    #region Rendimiento de Filtros

    [Fact]
    public async Task ObtenerRecetas_ConFiltrosComplejos_DeberiaSerRapida()
    {
        // Arrange
        var (productoId, _) = await SeedRecetasConProductoAsync(50);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/core/recetas?productoId={productoId}&soloActivas=true&filtroTexto=pizza&ordenarPor=TiempoPreparacionMinutos&direccionOrden=Asc");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "Los filtros complejos deberían ser rápidos");
    }

    [Fact]
    public async Task ObtenerRecetas_ConFiltrosSinResultados_DeberiaSerRapida()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas?filtroTexto=terminoInexistente");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "Los filtros sin resultados deberían ser muy rápidos");
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

    private async Task<(Guid productoId, List<Guid> recetaIds)> SeedRecetasConProductoAsync(int cantidadRecetas)
    {
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        var recetaIds = new List<Guid>();
        
        for (int i = 0; i < cantidadRecetas; i++)
        {
            var recetaId = await CrearRecetaDePruebaAsync(productoId);
            recetaIds.Add(recetaId);
        }
        
        return (productoId, recetaIds);
    }

    private async Task<List<Guid>> SeedRecetasComplejasAsync(int cantidad)
    {
        var productoIds = await CrearProductosDePruebaAsync(cantidad);
        var recetaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var recetaId = await CrearRecetaComplejaAsync(productoIds[i]);
            recetaIds.Add(recetaId);
        }
        
        return recetaIds;
    }

    private async Task<List<Guid>> SeedRecetasConMuchosIngredientesAsync(int cantidad)
    {
        var productoIds = await CrearProductosDePruebaAsync(cantidad);
        var recetaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var recetaId = await CrearRecetaConMuchosIngredientesAsync(productoIds[i]);
            recetaIds.Add(recetaId);
        }
        
        return recetaIds;
    }

    #endregion
}
