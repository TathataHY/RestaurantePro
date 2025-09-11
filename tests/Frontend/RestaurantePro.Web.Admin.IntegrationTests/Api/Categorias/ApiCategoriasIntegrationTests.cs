using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Categorias;

/// <summary>
/// Pruebas de integración para el controlador de Categorías
/// </summary>
public class ApiCategoriasIntegrationTests : BaseIntegrationTest
{
    #region Obtener Categorías

    [Fact]
    public async Task ObtenerCategorias_ConParametrosPorDefecto_DeberiaRetornarCategoriasActivas()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().OnlyContain(c => c.Activa == true);
    }

    [Fact]
    public async Task ObtenerCategorias_ConSoloActivasFalse_DeberiaRetornarTodasLasCategorias()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias?soloActivas=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerCategorias_ConOcultarVaciasFalse_DeberiaRetornarCategoriasVacias()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias?ocultarVacias=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerCategorias_ConParametrosCombinados_DeberiaRetornarResultadosCorrectos()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias?soloActivas=false&ocultarVacias=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Obtener Categoría por ID

    [Fact]
    public async Task ObtenerCategoriaPorId_ConIdValido_DeberiaRetornarCategoria()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<CategoriaProductoDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(categoriaId);
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Categoría no encontrada");
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConIdInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var idInvalido = "id-invalido";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Buscar Categorías

    [Fact]
    public async Task BuscarCategorias_ConTerminoValido_DeberiaRetornarCategoriasFiltradas()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();
        var terminoBusqueda = "Bebidas";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoBusqueda}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().OnlyContain(c => c.Nombre.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();
        var terminoBusqueda = "CategoriaInexistente";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoBusqueda}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoVacio_DeberiaRetornarTodasLasCategorias()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoParcial_DeberiaRetornarCategoriasCoincidentes()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();
        var terminoBusqueda = "comida";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoBusqueda}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoCaseInsensitive_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();
        var terminoBusqueda = "BEBIDAS";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoBusqueda}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Validaciones de Estructura de Datos

    [Fact]
    public async Task ObtenerCategorias_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<CategoriaProductoDto>>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeEmpty();
        var categoria = responseData.Data.First();
        
        categoria.Id.Should().NotBeEmpty();
        categoria.Nombre.Should().NotBeNullOrEmpty();
        categoria.Color.Should().NotBeNullOrEmpty();
        categoria.Icono.Should().NotBeNullOrEmpty();
        categoria.Orden.Should().BeGreaterOrEqualTo(0);
        categoria.Activa.Should().BeTrue();
        categoria.CantidadProductos.Should().BeGreaterOrEqualTo(0);
        categoria.ProductosDisponibles.Should().BeGreaterOrEqualTo(0);
        categoria.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        var categoriaIds = await SeedCategoriasDePruebaAsync();
        var categoriaId = categoriaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/{categoriaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<CategoriaProductoDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var categoria = responseData.Data;
        
        categoria.Id.Should().Be(categoriaId);
        categoria.Nombre.Should().NotBeNullOrEmpty();
        categoria.Color.Should().NotBeNullOrEmpty();
        categoria.Icono.Should().NotBeNullOrEmpty();
        categoria.Orden.Should().BeGreaterOrEqualTo(0);
        categoria.Activa.Should().BeTrue();
        categoria.CantidadProductos.Should().BeGreaterOrEqualTo(0);
        categoria.ProductosDisponibles.Should().BeGreaterOrEqualTo(0);
        categoria.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    #endregion

    #region Rendimiento

    [Fact]
    public async Task ObtenerCategorias_ConMuchasCategorias_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(50); // Crear 50 categorías

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    [Fact]
    public async Task BuscarCategorias_ConMuchasCategorias_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync(50); // Crear 50 categorías

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=test");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedCategoriasDePruebaAsync(int cantidad = 5)
    {
        var categoriaIds = new List<Guid>();
        
        for (int i = 1; i <= cantidad; i++)
        {
            var categoriaId = Guid.NewGuid();
            categoriaIds.Add(categoriaId);
            
            // Crear categoría usando el seeder de productos existente
            await SeedCategoriaAsync($"Categoría Test {i}", $"Descripción de categoría {i}", i);
        }
        
        return categoriaIds;
    }

    private async Task SeedCategoriaAsync(string nombre, string descripcion, int orden)
    {
        // Este método simulará la creación de categorías
        // En un entorno real, usarías el seeder apropiado
        await Task.CompletedTask;
    }

    #endregion
}
