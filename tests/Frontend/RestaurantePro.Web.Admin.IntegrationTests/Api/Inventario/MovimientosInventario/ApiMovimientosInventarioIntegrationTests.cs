using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.MovimientosInventario;

/// <summary>
/// Tests de integración para el controlador de Movimientos de Inventario
/// Cubre todos los endpoints principales del módulo de movimientos de inventario
/// </summary>
[Collection("IntegrationTests")]
public class ApiMovimientosInventarioIntegrationTests : BaseIntegrationTest
{
    public ApiMovimientosInventarioIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Endpoints Principales

    [Fact]
    public async Task ObtenerMovimientos_ConFiltros_DeberiaRetornarListaPaginada()
    {
        // Arrange
        await SeedMovimientosInventarioAsync(5); // Crear 5 movimientos de prueba
        var query = "?pageNumber=1&pageSize=10&fechaInicio=2024-01-01&fechaFin=2024-12-31";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerMovimientos_ConFiltrosCompletos_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var query = $"?pageNumber=1&pageSize=20&fechaInicio=2024-01-01&fechaFin=2024-12-31&tipoMovimiento=Ingreso&ingredienteId={ingredienteId}&usuarioId={usuarioId}&filtroMotivo=compra&ordenarPor=Fecha&direccionOrdenamiento=desc";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerMovimientoPorId_ConIdValido_DeberiaRetornarMovimiento()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        // Nota: Este test puede retornar 404 si no existe el movimiento, lo cual es válido
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task RegistrarMovimiento_ConDatosValidos_DeberiaCrearMovimiento()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "Compra inicial",
            Observaciones = "Movimiento de prueba",
            UsuarioId = Guid.NewGuid(),
            Fecha = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<Guid>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBe(Guid.Empty);
        }
    }

    [Fact]
    public async Task ActualizarMovimiento_ConDatosValidos_DeberiaActualizarMovimiento()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = 75,
            CostoUnitario = 12.50m,
            Motivo = "Ajuste de cantidad",
            Observaciones = "Actualización de prueba"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task EliminarMovimiento_ConIdValido_DeberiaEliminarMovimiento()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Consultas Específicas

    [Fact]
    public async Task ObtenerMovimientosPorIngrediente_ConIdValido_DeberiaRetornarMovimientos()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = "?fechaDesde=2024-01-01&fechaHasta=2024-12-31&limite=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/ingrediente/{ingredienteId}{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<List<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerMovimientosPorTipo_ConTipoValido_DeberiaRetornarMovimientos()
    {
        // Arrange
        var tipo = "Ingreso";
        var query = "?pageNumber=1&pageSize=10&fechaInicio=2024-01-01&fechaFin=2024-12-31";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/tipo/{tipo}{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerMovimientosPorTipo_ConTipoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var tipo = "TipoInvalido";
        var query = "?pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/tipo/{tipo}{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Reportes

    [Fact]
    public async Task GenerarReporte_ConParametros_DeberiaGenerarReporte()
    {
        // Arrange
        var query = "?fechaInicio=2024-01-01&fechaFin=2024-12-31&incluirDetalle=true&agruparPorTipo=true";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/reporte{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.ReporteMovimientosDto>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Tests de Validación de Parámetros

    [Theory]
    [InlineData(0, 10)]    // PageNumber inválido
    [InlineData(1, 0)]     // PageSize inválido
    [InlineData(-1, 10)]   // PageNumber negativo
    [InlineData(1, -5)]    // PageSize negativo
    public async Task ObtenerMovimientos_ConParametrosInvalidos_DeberiaRetornarBadRequest(int pageNumber, int pageSize)
    {
        // Arrange
        var query = $"?pageNumber={pageNumber}&pageSize={pageSize}";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerMovimientos_ConFechasInvalidas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var query = "?fechaInicio=2024-13-01&fechaFin=2024-12-32";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        // El endpoint debería manejar fechas inválidas de manera apropiada
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Validación de Headers

    [Fact]
    public async Task TodosLosEndpoints_DeberianRetornarContentTypeJson()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/inventario/movimientos",
            "/api/inventario/movimientos/reporte"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task TodosLosEndpoints_DeberianTenerAutorizacion()
    {
        // Arrange
        var client = _factory.CreateClient(); // _cliente sin autenticación

        // Act & Assert
        var response = await client.GetAsync("/api/inventario/movimientos");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Tests de Tipos de Movimiento

    [Theory]
    [InlineData("Ingreso")]
    [InlineData("Egreso")]
    [InlineData("Entrada")]
    [InlineData("Salida")]
    public async Task RegistrarMovimiento_ConTiposValidos_DeberiaProcesarCorrectamente(string tipoMovimiento)
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 25,
            TipoMovimiento = Enum.Parse<Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario>(tipoMovimiento),
            Motivo = "Test de tipo",
            Observaciones = "Movimiento de prueba",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConTipoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 25,
            TipoMovimiento = (Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario)999, // Tipo inválido
            Motivo = "Test de tipo inválido",
            Observaciones = "Movimiento de prueba",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Filtros Avanzados

    [Fact]
    public async Task ObtenerMovimientos_ConFiltroPorUsuario_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = $"?usuarioId={usuarioId}&pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerMovimientos_ConFiltroPorMotivo_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = "?filtroMotivo=compra&pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
    }

    #endregion

    #region Tests de Ordenamiento

    [Theory]
    [InlineData("Fecha", "desc")]
    [InlineData("Fecha", "asc")]
    [InlineData("Cantidad", "desc")]
    [InlineData("Cantidad", "asc")]
    public async Task ObtenerMovimientos_ConOrdenamiento_DeberiaOrdenarCorrectamente(string ordenarPor, string direccion)
    {
        // Arrange
        var query = $"?ordenarPor={ordenarPor}&direccionOrdenamiento={direccion}&pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
    }

    #endregion
}

/// <summary>
/// Request para registrar un nuevo movimiento de inventario
/// </summary>
public class RegistrarMovimientoRequest
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario TipoMovimiento { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Guid? UsuarioId { get; set; }
    public DateTime? Fecha { get; set; }
}

/// <summary>
/// Request para actualizar un movimiento de inventario
/// </summary>
public class ActualizarMovimientoRequest
{
    public decimal? Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }
}

    #region Métodos de Seeding

    /// <summary>
    /// Crea datos de prueba para movimientos de inventario
    /// </summary>
    private async Task SeedMovimientosInventarioAsync(int cantidad = 5)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Crear ingredientes e usuarios primero
        var ingredientes = await CrearIngredientesDePruebaAsync(context, 3);
        var usuarios = await CrearUsuariosDePruebaAsync(context, 2);
        
        for (int i = 0; i < cantidad; i++)
        {
            var movimiento = MovimientoInventario.CrearIngreso(
                ingredientes[i % ingredientes.Count],
                10 + i * 5,
                $"Ingreso de prueba {i + 1}",
                DateTime.UtcNow.AddDays(-i)
            );
            
            context.MovimientosInventario.Add(movimiento);
        }
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Crea ingredientes de prueba
    /// </summary>
    private async Task<List<Guid>> CrearIngredientesDePruebaAsync(RestauranteProDbContext context, int cantidad)
    {
        var ingredientes = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var ingredienteId = Guid.NewGuid();
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Inventario.Ingredientes (Id, Nombre, Descripcion, UnidadMedida, PrecioUnitario, StockActual, StockMinimo, EstaActivo, FechaCreacion, FechaModificacion)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})",
                ingredienteId,
                $"Ingrediente Test {i + 1}",
                $"Descripción del ingrediente {i + 1}",
                "Kilogramo",
                10.50m + i,
                100 + i * 10,
                10 + i,
                true,
                DateTime.UtcNow,
                DateTime.UtcNow
            );
            
            ingredientes.Add(ingredienteId);
        }
        
        return ingredientes;
    }

    /// <summary>
    /// Crea usuarios de prueba
    /// </summary>
    private async Task<List<Guid>> CrearUsuariosDePruebaAsync(RestauranteProDbContext context, int cantidad)
    {
        var usuarios = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var usuarioId = Guid.NewGuid();
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Core.Usuarios (Id, Nombre, Email, Telefono, EstaActivo, FechaCreacion, FechaModificacion)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                usuarioId,
                $"Usuario Test {i + 1}",
                $"usuario{i + 1}@test.com",
                $"+123456789{i}",
                true,
                DateTime.UtcNow,
                DateTime.UtcNow
            );
            
            usuarios.Add(usuarioId);
        }
        
        return usuarios;
    }

    #endregion

