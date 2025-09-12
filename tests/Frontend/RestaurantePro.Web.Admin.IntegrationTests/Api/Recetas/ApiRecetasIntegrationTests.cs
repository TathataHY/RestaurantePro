using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Recetas;

/// <summary>
/// Pruebas de integración para el controlador de Recetas
/// </summary>
public class ApiRecetasIntegrationTests : BaseIntegrationTest
{
    public ApiRecetasIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Obtener Recetas

    [Fact]
    public async Task ObtenerRecetas_ConParametrosPorDefecto_DeberiaRetornarRecetasActivas()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().NotBeEmpty();
        responseData.Data.Items.Should().OnlyContain(r => r.EstaActiva == true);
    }

    [Fact]
    public async Task ObtenerRecetas_ConSoloActivasFalse_DeberiaRetornarTodasLasRecetas()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas?soloActivas=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerRecetas_ConFiltroTexto_DeberiaRetornarRecetasFiltradas()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();
        var terminoBusqueda = "Pizza";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas?filtroTexto={terminoBusqueda}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().OnlyContain(r => 
            r.Preparacion.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase) ||
            r.NombreProducto.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ObtenerRecetas_ConPaginacion_DeberiaRetornarResultadosPaginados()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(15); // Crear 15 recetas

        // Act
        var response = await _client.GetAsync("/api/core/recetas?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().HaveCount(5);
        responseData.Data.PageNumber.Should().Be(1);
        responseData.Data.PageSize.Should().Be(5);
        responseData.Data.TotalCount.Should().BeGreaterOrEqualTo(15);
    }

    [Fact]
    public async Task ObtenerRecetas_ConOrdenamiento_DeberiaRetornarRecetasOrdenadas()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas?ordenarPor=TiempoPreparacionMinutos&direccionOrden=Asc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().NotBeEmpty();
        
        // Verificar que estén ordenadas por tiempo de preparación ascendente
        var tiempos = responseData.Data.Items.Select(r => r.TiempoPreparacionMinutos).ToList();
        tiempos.Should().BeInAscendingOrder();
    }

    #endregion

    #region Obtener Receta por ID

    [Fact]
    public async Task ObtenerRecetaPorId_ConIdValido_DeberiaRetornarReceta()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync();
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(recetaId);
    }

    [Fact]
    public async Task ObtenerRecetaPorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Receta no encontrada");
    }

    [Fact]
    public async Task ObtenerRecetaPorId_ConIdInvalido_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInvalido = "id-invalido";

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Obtener Recetas por Producto

    [Fact]
    public async Task ObtenerRecetasPorProducto_ConProductoValido_DeberiaRetornarRecetasDelProducto()
    {
        // Arrange
        var (productoId, recetaIds) = await SeedRecetasConProductoAsync();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/producto/{productoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().OnlyContain(r => r.ProductoId == productoId);
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

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Producto no encontrado");
    }

    #endregion

    #region Calcular Costo de Receta

    [Fact]
    public async Task CalcularCostoReceta_ConRecetaValida_DeberiaRetornarCostoTotal()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync();
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/costo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<decimal>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalcularCostoReceta_ConRecetaInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaIdInexistente}/costo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Receta no encontrada");
    }

    #endregion

    #region Verificar Disponibilidad

    [Fact]
    public async Task VerificarDisponibilidad_ConRecetaValida_DeberiaRetornarEstadoDisponibilidad()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync();
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/disponibilidad?cantidad=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Recetas.DTOs.DisponibilidadRecetaDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.MaximaPorcionesDisponibles.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task VerificarDisponibilidad_ConCantidadPersonalizada_DeberiaRetornarEstadoCorrecto()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync();
        var recetaId = recetaIds.First();
        var cantidad = 5;

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}/disponibilidad?cantidad={cantidad}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Recetas.DTOs.DisponibilidadRecetaDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task VerificarDisponibilidad_ConRecetaInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var recetaIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaIdInexistente}/disponibilidad");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("Receta no encontrada");
    }

    #endregion

    #region Validaciones de Estructura de Datos

    [Fact]
    public async Task ObtenerRecetas_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        await SeedRecetasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/recetas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>>(content, GetJsonOptions());
        
        responseData.Data.Items.Should().NotBeEmpty();
        var receta = responseData.Data.Items.First();
        
        receta.Id.Should().NotBeEmpty();
        receta.ProductoId.Should().NotBeEmpty();
        receta.NombreProducto.Should().NotBeNullOrEmpty();
        receta.Preparacion.Should().NotBeNullOrEmpty();
        receta.TiempoPreparacionMinutos.Should().BeGreaterThan(0);
        receta.Ingredientes.Should().NotBeNull();
        receta.CostoTotal.Should().BeGreaterOrEqualTo(0);
        receta.EstaActiva.Should().BeTrue();
        receta.FechaCreacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task ObtenerRecetaPorId_DeberiaIncluirTodasLasPropiedadesRequeridas()
    {
        // Arrange
        var recetaIds = await SeedRecetasDePruebaAsync();
        var recetaId = recetaIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/recetas/{recetaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        var receta = responseData.Data;
        
        receta.Id.Should().Be(recetaId);
        receta.ProductoId.Should().NotBeEmpty();
        receta.NombreProducto.Should().NotBeNullOrEmpty();
        receta.Preparacion.Should().NotBeNullOrEmpty();
        receta.TiempoPreparacionMinutos.Should().BeGreaterThan(0);
        receta.Ingredientes.Should().NotBeNull();
        receta.CostoTotal.Should().BeGreaterOrEqualTo(0);
        receta.EstaActiva.Should().BeTrue();
        receta.FechaCreacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(5));
    }

    #endregion

    #region Rendimiento

    [Fact]
    public async Task ObtenerRecetas_ConMuchasRecetas_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50); // Crear 50 recetas

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    [Fact]
    public async Task BuscarRecetas_ConFiltroTexto_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedRecetasDePruebaAsync(50); // Crear 50 recetas

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/core/recetas?filtroTexto=pizza");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedRecetasDePruebaAsync(int cantidad = 5)
    {
        // Crear productos primero
        var productoIds = await CrearProductosDePruebaAsync(cantidad);
        
        if (productoIds.Count < cantidad)
        {
            throw new InvalidOperationException($"No se pudieron crear suficientes productos. Se requieren {cantidad}, pero solo se crearon {productoIds.Count}");
        }
        
        // Crear recetas para cada producto
        var recetaIds = new List<Guid>();
        for (int i = 0; i < cantidad; i++)
        {
            var recetaId = await CrearRecetaDePruebaAsync(productoIds[i]);
            recetaIds.Add(recetaId);
        }
        
        return recetaIds;
    }

    private async Task<(Guid productoId, List<Guid> recetaIds)> SeedRecetasConProductoAsync()
    {
        // Crear un producto
        var productoIds = await CrearProductosDePruebaAsync(1);
        var productoId = productoIds.First();
        
        // Crear múltiples recetas para el mismo producto
        var recetaIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var recetaId = await CrearRecetaDePruebaAsync(productoId);
            recetaIds.Add(recetaId);
        }
        
        return (productoId, recetaIds);
    }

    #endregion
}
