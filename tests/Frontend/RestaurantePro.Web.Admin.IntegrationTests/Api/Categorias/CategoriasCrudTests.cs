using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Categorias;

/// <summary>
/// Pruebas de integración para operaciones CRUD de categorías
/// </summary>
public class CategoriasCrudTests : BaseIntegrationTest
{
    private readonly JsonSerializerOptions _jsonOptions;

    public CategoriasCrudTests(WebApplicationFactory factory) : base(factory)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Crear Categoría (POST)

    [Fact]
    public async Task CrearCategoria_ConDatosValidos_DeberiaRetornarCategoriaCreada()
    {
        // Arrange
        var nuevaCategoria = new
        {
            nombre = "Nueva Categoría",
            descripcion = "Descripción de la nueva categoría",
            orden = 10,
            activa = true
        };

        var json = JsonSerializer.Serialize(nuevaCategoria, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/categorias", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Productos.DTOs.CategoriaProductoDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Nombre.Should().Be(nuevaCategoria.nombre);
        responseData.Data.Descripcion.Should().Be(nuevaCategoria.descripcion);
        responseData.Data.Orden.Should().Be(nuevaCategoria.orden);
        responseData.Data.Activa.Should().Be(nuevaCategoria.activa);
        responseData.Data.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CrearCategoria_ConNombreDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var categoriaExistente = new
        {
            nombre = "Bebidas", // Nombre que ya existe en el seeder
            descripcion = "Descripción duplicada",
            orden = 1,
            activa = true
        };

        var json = JsonSerializer.Serialize(categoriaExistente, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/categorias", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("duplicado");
    }

    [Theory]
    [InlineData("", "Descripción válida", 1, true, "El nombre es requerido")]
    [InlineData("   ", "Descripción válida", 1, true, "El nombre no puede estar vacío")]
    [InlineData("Categoría Válida", "Descripción válida", -1, true, "El orden debe ser mayor o igual a 0")]
    [InlineData("Categoría Válida", "Descripción válida", 0, true, null)] // Caso válido
    public async Task CrearCategoria_ConDatosInvalidos_DeberiaRetornarError(
        string nombre, string descripcion, int orden, bool activa, string? mensajeError)
    {
        // Arrange
        var categoriaInvalida = new
        {
            nombre,
            descripcion,
            orden,
            activa
        };

        var json = JsonSerializer.Serialize(categoriaInvalida, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/categorias", content);

        // Assert
        if (mensajeError != null)
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
            
            responseData.Should().NotBeNull();
            responseData.Success.Should().BeFalse();
            responseData.Message.Should().Contain(mensajeError);
        }
        else
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }
    }

    #endregion

    #region Actualizar Categoría (PUT)

    [Fact]
    public async Task ActualizarCategoria_ConDatosValidos_DeberiaRetornarCategoriaActualizada()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        var categoriaActualizada = new
        {
            nombre = "Categoría Actualizada",
            descripcion = "Descripción actualizada",
            orden = 99,
            activa = false
        };

        var json = JsonSerializer.Serialize(categoriaActualizada, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/categorias/{categoriaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Productos.DTOs.CategoriaProductoDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(categoriaId);
        responseData.Data.Nombre.Should().Be(categoriaActualizada.nombre);
        responseData.Data.Descripcion.Should().Be(categoriaActualizada.descripcion);
        responseData.Data.Orden.Should().Be(categoriaActualizada.orden);
        responseData.Data.Activa.Should().Be(categoriaActualizada.activa);
    }

    [Fact]
    public async Task ActualizarCategoria_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var categoriaActualizada = new
        {
            nombre = "Categoría Actualizada",
            descripcion = "Descripción actualizada",
            orden = 99,
            activa = true
        };

        var json = JsonSerializer.Serialize(categoriaActualizada, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/categorias/{idInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task ActualizarCategoria_ConNombreDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        var categoriaActualizada = new
        {
            nombre = "Bebidas", // Nombre que ya existe en otra categoría
            descripcion = "Descripción actualizada",
            orden = 99,
            activa = true
        };

        var json = JsonSerializer.Serialize(categoriaActualizada, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/categorias/{categoriaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("duplicado");
    }

    #endregion

    #region Eliminar Categoría (DELETE)

    [Fact]
    public async Task EliminarCategoria_ConIdValido_DeberiaRetornarOk()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var response = await _client.DeleteAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Message.Should().Contain("eliminada");
    }

    [Fact]
    public async Task EliminarCategoria_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/core/categorias/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task EliminarCategoria_ConProductosAsociados_DeberiaRetornarError()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First(); // Esta categoría tiene productos asociados

        // Act
        var response = await _client.DeleteAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("productos asociados");
    }

    #endregion

    #region Pruebas de Integridad de Datos

    [Fact]
    public async Task CrearCategoria_DeberiaPersistirEnBaseDeDatos()
    {
        // Arrange
        var nuevaCategoria = new
        {
            nombre = "Categoría de Prueba",
            descripcion = "Descripción de prueba",
            orden = 50,
            activa = true
        };

        var json = JsonSerializer.Serialize(nuevaCategoria, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/categorias", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Productos.DTOs.CategoriaProductoDto>>(responseContent, _jsonOptions);
        var categoriaId = responseData.Data.Id;

        // Verificar que se puede obtener la categoría creada
        var getResponse = await _client.GetAsync($"/api/core/categorias/{categoriaId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Productos.DTOs.CategoriaProductoDto>>(getContent, _jsonOptions);
        
        getData.Data.Nombre.Should().Be(nuevaCategoria.nombre);
        getData.Data.Descripcion.Should().Be(nuevaCategoria.descripcion);
        getData.Data.Orden.Should().Be(nuevaCategoria.orden);
        getData.Data.Activa.Should().Be(nuevaCategoria.activa);
    }

    [Fact]
    public async Task ActualizarCategoria_DeberiaPersistirCambiosEnBaseDeDatos()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        var categoriaActualizada = new
        {
            nombre = "Categoría Modificada",
            descripcion = "Descripción modificada",
            orden = 77,
            activa = false
        };

        var json = JsonSerializer.Serialize(categoriaActualizada, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/categorias/{categoriaId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que los cambios se persistieron
        var getResponse = await _client.GetAsync($"/api/core/categorias/{categoriaId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Productos.DTOs.CategoriaProductoDto>>(getContent, _jsonOptions);
        
        getData.Data.Nombre.Should().Be(categoriaActualizada.nombre);
        getData.Data.Descripcion.Should().Be(categoriaActualizada.descripcion);
        getData.Data.Orden.Should().Be(categoriaActualizada.orden);
        getData.Data.Activa.Should().Be(categoriaActualizada.activa);
    }

    [Fact]
    public async Task EliminarCategoria_DeberiaEliminarDeBaseDeDatos()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.Last(); // Usar la última categoría (menos probable que tenga productos)

        // Act
        var response = await _client.DeleteAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que la categoría ya no existe
        var getResponse = await _client.GetAsync($"/api/core/categorias/{categoriaId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private async Task<List<Guid>> SeedCategoriasDePruebaAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantePro.Infrastructure.Persistence.Contexts.RestauranteProDbContext>();
        
        // Limpiar datos existentes
        context.Productos.RemoveRange(context.Productos);
        context.ProductoCategorias.RemoveRange(context.ProductoCategorias);
        await context.SaveChangesAsync();

        // Crear categorías de prueba
        var categoriaIds = await Utils.ProductosTestSeeder.SeedCategoriasAsync(context);
        
        // Crear productos de prueba para que las categorías no estén vacías
        await Utils.ProductosTestSeeder.SeedProductosAsync(context, categoriaIds);
        
        return categoriaIds;
    }

    #endregion
}
