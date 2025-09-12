using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;
using RestaurantePro.Application.Core.Recetas.Commands.ActualizarReceta;
using RestaurantePro.Application.Core.Recetas.Commands.EliminarReceta;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Recetas;

/// <summary>
/// Pruebas de integración para operaciones CRUD de Recetas
/// </summary>
public class RecetasCrudTests : BaseIntegrationTest
{
    public RecetasCrudTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Crear Receta

    [Fact]
    public async Task CrearReceta_ConDatosValidos_DeberiaCrearRecetaExitosamente()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Mezclar todos los ingredientes y hornear por 20 minutos",
            TiempoPreparacionMinutos = 30,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 2, EsOpcional = false },
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 1, EsOpcional = true }
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.ProductoId.Should().Be(productoId);
        responseData.Data.Preparacion.Should().Be(command.Preparacion);
        responseData.Data.TiempoPreparacionMinutos.Should().Be(command.TiempoPreparacionMinutos);
        responseData.Data.Ingredientes.Should().HaveCount(2);
        responseData.Data.EstaActiva.Should().BeTrue();
    }

    [Fact]
    public async Task CrearReceta_ConProductoInexistente_DeberiaRetornarError()
    {
        // Arrange
        var productoIdInexistente = Guid.NewGuid();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoIdInexistente,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 15,
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
    public async Task CrearReceta_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearRecetaCommand
        {
            ProductoId = Guid.Empty,
            Preparacion = "", // Preparación vacía
            TiempoPreparacionMinutos = -1, // Tiempo negativo
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
    public async Task CrearReceta_ConIngredientesDuplicados_DeberiaRetornarError()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        var ingredienteId = Guid.NewGuid();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta con ingredientes duplicados",
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

    #region Actualizar Receta

    [Fact]
    public async Task ActualizarReceta_ConDatosValidos_DeberiaActualizarRecetaExitosamente()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Preparación actualizada - Mezclar ingredientes y cocinar por 25 minutos",
            TiempoPreparacionMinutos = 35,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 3, EsOpcional = false }
            }
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(recetaId);
        responseData.Data.Preparacion.Should().Be(command.Preparacion);
        responseData.Data.TiempoPreparacionMinutos.Should().Be(command.TiempoPreparacionMinutos);
        responseData.Data.FechaModificacion.Should().NotBeNull();
    }

    [Fact]
    public async Task ActualizarReceta_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaIdInexistente,
            Preparacion = "Preparación de prueba",
            TiempoPreparacionMinutos = 20,
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
    public async Task ActualizarReceta_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "", // Preparación vacía
            TiempoPreparacionMinutos = -5, // Tiempo negativo
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

    [Fact]
    public async Task ActualizarReceta_ConIdInconsistente_DeberiaUsarIdDeLaUrl()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        var idDiferente = Guid.NewGuid();
        
        var command = new ActualizarRecetaCommand
        {
            Id = idDiferente, // ID diferente al de la URL
            Preparacion = "Preparación actualizada",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Id.Should().Be(recetaId); // Debe usar el ID de la URL
    }

    #endregion

    #region Eliminar Receta

    [Fact]
    public async Task EliminarReceta_ConIdValido_DeberiaEliminarRecetaExitosamente()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.DeleteAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().BeTrue();

        // Verificar que la receta ya no existe
        var getResponse = await _client.GetAsync($"/api/core/recetas/{recetaId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

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

    #region Operaciones en Lote

    [Fact]
    public async Task CrearMultiplesRecetas_ConDatosValidos_DeberiaCrearTodasExitosamente()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(3);
        var recetasCreadas = new List<Guid>();

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            var command = new CrearRecetaCommand
            {
                ProductoId = productoIds[i],
                Preparacion = $"Receta {i + 1} - Preparación detallada",
                TiempoPreparacionMinutos = 20 + (i * 5),
                Ingredientes = new List<AgregarIngredienteDto>
                {
                    new() { IngredienteId = Guid.NewGuid(), Cantidad = 1 + i, EsOpcional = false }
                }
            };

            var json = JsonSerializer.Serialize(command, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/core/recetas", content);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(responseContent, GetJsonOptions());
            recetasCreadas.Add(responseData.Data.Id);
        }

        // Verificar que todas las recetas fueron creadas
        recetasCreadas.Should().HaveCount(3);
        recetasCreadas.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ActualizarMultiplesRecetas_ConDatosValidos_DeberiaActualizarTodasExitosamente()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(3);

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            var command = new ActualizarRecetaCommand
            {
                Id = recetaIds[i],
                Preparacion = $"Receta actualizada {i + 1} - Nueva preparación",
                TiempoPreparacionMinutos = 25 + (i * 5),
                Ingredientes = new List<AgregarIngredienteDto>
                {
                    new() { IngredienteId = Guid.NewGuid(), Cantidad = 2 + i, EsOpcional = false }
                }
            };

            var json = JsonSerializer.Serialize(command, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/core/recetas/{recetaIds[i]}", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task EliminarMultiplesRecetas_ConIdsValidos_DeberiaEliminarTodasExitosamente()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(3);

        // Act & Assert
        foreach (var recetaId in recetaIds)
        {
            var response = await _client.DeleteAsync($"/api/core/recetas/{recetaId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Verificar que todas las recetas fueron eliminadas
        foreach (var recetaId in recetaIds)
        {
            var getResponse = await _client.GetAsync($"/api/core/recetas/{recetaId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region Validaciones de Integridad

    [Fact]
    public async Task CrearReceta_DeberiaAsociarCorrectamenteConProducto()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(responseContent, GetJsonOptions());
        
        responseData.Data.ProductoId.Should().Be(productoId);
        
        // Verificar que la receta aparece en las recetas del producto
        var getRecetasResponse = await _client.GetAsync($"/api/core/recetas/producto/{productoId}");
        getRecetasResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var recetasContent = await getRecetasResponse.Content.ReadAsStringAsync();
        var recetasData = JsonSerializer.Deserialize<ApiResponse<List<RecetaDto>>>(recetasContent, GetJsonOptions());
        recetasData.Data.Should().Contain(r => r.Id == responseData.Data.Id);
    }

    [Fact]
    public async Task ActualizarReceta_DeberiaMantenerAsociacionConProducto()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        // Obtener la receta original para verificar el ProductoId
        var getResponse = await _client.GetAsync($"/api/core/recetas/{recetaId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(getContent, GetJsonOptions());
        var productoIdOriginal = getData.Data.ProductoId;
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Preparación actualizada",
            TiempoPreparacionMinutos = 30,
            Ingredientes = new List<AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RecetaDto>>(responseContent, GetJsonOptions());
        
        responseData.Data.ProductoId.Should().Be(productoIdOriginal); // Debe mantener el mismo ProductoId
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
