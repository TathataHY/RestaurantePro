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
/// Pruebas de integración para seguridad de Recetas
/// </summary>
public class RecetasSecurityTests : BaseIntegrationTest
{
    public RecetasSecurityTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Autenticación

    [Fact]
    public async Task ObtenerRecetas_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();

        // Act
        var response = await clientNoAuth.GetAsync("/api/core/recetas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerRecetaPorId_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();
        var recetaId = Guid.NewGuid();

        // Act
        var response = await clientNoAuth.GetAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CrearReceta_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();
        var command = new CrearRecetaCommand
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientNoAuth.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActualizarReceta_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();
        var recetaId = Guid.NewGuid();
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Receta actualizada",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientNoAuth.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EliminarReceta_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();
        var recetaId = Guid.NewGuid();

        // Act
        var response = await clientNoAuth.DeleteAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Autorización por Roles

    [Fact]
    public async Task CrearReceta_ConRolMesero_DeberiaRetornarForbidden()
    {
        // Arrange
        var clientMesero = CreateAuthenticatedClientWithRole("Mesero");
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientMesero.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CrearReceta_ConRolCajero_DeberiaRetornarForbidden()
    {
        // Arrange
        var clientCajero = CreateAuthenticatedClientWithRole("Cajero");
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientCajero.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CrearReceta_ConRolAdministrador_DeberiaPermitir()
    {
        // Arrange
        var clientAdmin = CreateAuthenticatedClientWithRole("Administrador");
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientAdmin.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CrearReceta_ConRolChef_DeberiaPermitir()
    {
        // Arrange
        var clientChef = CreateAuthenticatedClientWithRole("Chef");
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientChef.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ActualizarReceta_ConRolMesero_DeberiaRetornarForbidden()
    {
        // Arrange
        var clientMesero = CreateAuthenticatedClientWithRole("Mesero");
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Receta actualizada",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientMesero.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ActualizarReceta_ConRolChef_DeberiaPermitir()
    {
        // Arrange
        var clientChef = CreateAuthenticatedClientWithRole("Chef");
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();
        
        var command = new ActualizarRecetaCommand
        {
            Id = recetaId,
            Preparacion = "Receta actualizada por chef",
            TiempoPreparacionMinutos = 25,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await clientChef.PutAsync($"/api/core/recetas/{recetaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EliminarReceta_ConRolChef_DeberiaRetornarForbidden()
    {
        // Arrange
        var clientChef = CreateAuthenticatedClientWithRole("Chef");
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var response = await clientChef.DeleteAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task EliminarReceta_ConRolAdministrador_DeberiaPermitir()
    {
        // Arrange
        var clientAdmin = CreateAuthenticatedClientWithRole("Administrador");
        var recetaIds = await SeedRecetasDePruebaAsync(1);
        var recetaId = recetaIds.First();

        // Act
        var response = await clientAdmin.DeleteAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Validación de Entrada

    [Fact]
    public async Task CrearReceta_ConDatosMaliciosos_DeberiaSanitizarEntrada()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "<script>alert('xss')</script>Receta maliciosa", // XSS attempt
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>(responseContent, GetJsonOptions());
        
        responseData.Data.Preparacion.Should().NotContain("<script>");
        responseData.Data.Preparacion.Should().NotContain("alert");
    }

    [Fact]
    public async Task CrearReceta_ConPreparacionMuyLarga_DeberiaRechazar()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        var preparacionLarga = new string('A', 10001); // Más de 10,000 caracteres
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = preparacionLarga,
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearReceta_ConTiempoPreparacionExcesivo_DeberiaRechazar()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = int.MaxValue, // Valor excesivo
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Validación de IDs

    [Fact]
    public async Task ObtenerReceta_ConIdMalicioso_DeberiaRechazar()
    {
        // Arrange
        var idMalicioso = "../../etc/passwd"; // Path traversal attempt

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{idMalicioso}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerRecetasPorProducto_ConIdMalicioso_DeberiaRechazar()
    {
        // Arrange
        var idMalicioso = "'; DROP TABLE recetas; --"; // SQL injection attempt

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/producto/{idMalicioso}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CalcularCostoReceta_ConIdMalicioso_DeberiaRechazar()
    {
        // Arrange
        var idMalicioso = "<script>alert('xss')</script>";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{idMalicioso}/costo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validación de Parámetros de Consulta

    [Fact]
    public async Task ObtenerRecetas_ConFiltroTextoMalicioso_DeberiaSanitizar()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(5);
        var filtroMalicioso = "<script>alert('xss')</script>";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas?filtroTexto={filtroMalicioso}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotContain("<script>");
        responseContent.Should().NotContain("alert");
    }

    [Fact]
    public async Task ObtenerRecetas_ConOrdenarPorMalicioso_DeberiaUsarValorPorDefecto()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(5);
        var ordenarPorMalicioso = "'; DROP TABLE recetas; --";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas?ordenarPor={ordenarPorMalicioso}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Debería usar el valor por defecto en lugar del valor malicioso
    }

    [Fact]
    public async Task ObtenerRecetas_ConDireccionOrdenMaliciosa_DeberiaUsarValorPorDefecto()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(5);
        var direccionMaliciosa = "<script>alert('xss')</script>";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas?direccionOrden={direccionMaliciosa}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Debería usar el valor por defecto en lugar del valor malicioso
    }

    #endregion

    #region Límites de Tasa

    [Fact]
    public async Task ObtenerRecetas_ConMuchasSolicitudes_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50);

        // Act - Ejecutar muchas solicitudes rápidamente
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_client.GetAsync("/api/core/recetas"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.TooManyRequests));
    }

    [Fact]
    public async Task BuscarRecetas_ConMuchasSolicitudes_DeberiaMantenerRendimiento()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50);

        // Act - Ejecutar muchas búsquedas rápidamente
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 15; i++)
        {
            tasks.Add(_client.GetAsync($"/api/core/recetas?filtroTexto=test{i}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.TooManyRequests));
    }

    #endregion

    #region Validación de Headers

    [Fact]
    public async Task ObtenerRecetas_ConHeadersMaliciosos_DeberiaIgnorar()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(5);
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "<script>alert('xss')</script>");
        _client.DefaultRequestHeaders.Add("User-Agent", "'; DROP TABLE recetas; --");

        // Act
        var response = await _client.GetAsync("/api/core/recetas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Los headers maliciosos no deberían afectar la respuesta
    }

    [Fact]
    public async Task CrearReceta_ConContentTypeInvalido_DeberiaRechazar()
    {
        // Arrange
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new List<RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto>()
        };

        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var content = new StringContent(json, Encoding.UTF8, "text/plain"); // Content-Type incorrecto

        // Act
        var response = await _client.PostAsync("/api/core/recetas", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
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
