using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;
using RestaurantePro.Application.Core.Recetas.Commands.ActualizarReceta;
using RestaurantePro.Application.Core.Recetas.Commands.EliminarReceta;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Recetas;

/// <summary>
/// Pruebas de integración para manejo de errores en Recetas
/// </summary>
public class RecetasErrorHandlingTests : BaseIntegrationTest
{
    public RecetasErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Errores de Validación

    [Fact]
    public async Task CrearReceta_ConProductoIdVacio_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var command = new CrearRecetaCommand
        {
            ProductoId = Guid.Empty,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Error al crear la receta");
    }

    [Fact]
    public async Task CrearReceta_ConPreparacionVacia_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "", // Preparación vacía
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CrearReceta_ConTiempoPreparacionNegativo_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = -5, // Tiempo negativo
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CrearReceta_ConTiempoPreparacionCero_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 0, // Tiempo cero
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CrearReceta_ConIngredienteConCantidadNegativa_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = -1, EsOpcional = false } // Cantidad negativa
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CrearReceta_ConIngredienteConCantidadCero_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 0, EsOpcional = false } // Cantidad cero
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    #endregion

    #region Errores de Negocio

    [Fact]
    public async Task CrearReceta_ConProductoInexistente_DeberiaRetornarErrorNegocio()
    {
        // Arrange
        var productoIdInexistente = Guid.NewGuid();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoIdInexistente,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Error al crear la receta");
    }

    [Fact]
    public async Task CrearReceta_ConIngredienteInexistente_DeberiaRetornarErrorNegocio()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        var ingredienteIdInexistente = Guid.NewGuid();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = ingredienteIdInexistente, Cantidad = 1, EsOpcional = false }
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CrearReceta_ConIngredientesDuplicados_DeberiaRetornarErrorNegocio()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        var ingredienteId = Guid.NewGuid();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = ingredienteId, Cantidad = 1, EsOpcional = false },
                new() { IngredienteId = ingredienteId, Cantidad = 2, EsOpcional = false } // Duplicado
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    #endregion

    #region Errores de Actualización

    [Fact]
    public async Task ActualizarReceta_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaIdInexistente,
            Preparacion = "Preparación actualizada",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaIdInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Receta no encontrada");
    }

    [Fact]
    public async Task ActualizarReceta_ConDatosInvalidos_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "", // Preparación vacía
            TiempoPreparacionMinutos = -10, // Tiempo negativo
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
    }

    #endregion

    #region Errores de Eliminación

    [Fact]
    public async Task EliminarReceta_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/core/recetas/{recetaIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Receta no encontrada");
    }

    [Fact]
    public async Task EliminarReceta_ConIdInvalido_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInvalido = "id-invalido";

        // Act
        var response = await _client.DeleteAsync($"/api/core/recetas/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Errores de Consulta

    [Fact]
    public async Task ObtenerReceta_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Receta no encontrada");
    }

    [Fact]
    public async Task ObtenerReceta_ConIdInvalido_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInvalido = "id-invalido";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerRecetasPorProducto_ConProductoInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var productoIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/producto/{productoIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Producto no encontrado");
    }

    #endregion

    #region Errores de Parámetros

    [Fact]
    public async Task ObtenerRecetas_ConPageNumberInvalido_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas?pageNumber=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Parámetros de paginación inválidos");
    }

    [Fact]
    public async Task ObtenerRecetas_ConPageSizeInvalido_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas?pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Parámetros de paginación inválidos");
    }

    [Fact]
    public async Task ObtenerRecetas_ConPageSizeExcesivo_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas?pageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Parámetros de paginación inválidos");
    }

    #endregion

    #region Errores de JSON

    [Fact]
    public async Task CrearReceta_ConJsonInvalido_DeberiaRetornarError()
    {
        // Arrange
        var jsonInvalido = "{ \"productoId\": \"invalid-guid\", \"preparacion\": \"test\" }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarReceta_ConJsonInvalido_DeberiaRetornarError()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        var jsonInvalido = "{ \"id\": \"invalid-guid\", \"preparacion\": \"test\" }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearReceta_ConJsonVacio_DeberiaRetornarError()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Errores de Timeout

    [Fact]
    public async Task ObtenerRecetas_ConTimeoutCorto_DeberiaManejarCorrectamente()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(100); // Crear muchas recetas para simular lentitud

        // Act & Assert
        var action = async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Timeout muy corto
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/recetas");
            var response = await _client.SendAsync(request, cts.Token);
            return response;
        };

        await action.Should().ThrowAsync<TaskCanceledException>()
            .WithMessage("*");
    }

    #endregion

    #region Errores de Concurrencia

    [Fact]
    public async Task ActualizarReceta_ConActualizacionSimultanea_DeberiaManejarConcurrencia()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        var command1 = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Preparación 1",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var command2 = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Preparación 2",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json1 = JsonSerializer.Serialize(command1, GetJsonOptions());
        var content1 = new StringContent(json1, Encoding.UTF8, "application/json");

        var json2 = JsonSerializer.Serialize(command2, GetJsonOptions());
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

        // Act - Ejecutar actualizaciones simultáneas
        var task1 = _client.PutAsync($"/api/core/recetas/{recetaId}", content1);
        var task2 = _client.PutAsync($"/api/core/recetas/{recetaId}", content2);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - Al menos una debe ser exitosa
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.OK);
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedRecetasDePruebaAsync(int cantidad = 5)
    {
        // Crear productos primero
        var productoIds = await CrearProductosDePruebaAsync(cantidad);
        
        // Crear recetas para cada producto
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
