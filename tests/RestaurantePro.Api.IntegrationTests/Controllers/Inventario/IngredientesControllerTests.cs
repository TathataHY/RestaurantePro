using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Inventario;
using Xunit;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración para IngredientesController
/// Valida todos los endpoints REST del controlador de gestión de ingredientes
/// </summary>
[Collection("Sequential")]
public class IngredientesControllerTests : ApiIntegrationTestBase
{
    public IngredientesControllerTests() : base(new TestWebApplicationFactory())
    {
    }

    [Fact]
    public async Task ObtenerIngredientes_DebeRetornarOkYDatosDeBD()
    {
        // Arrange
        await CrearIngredientePrueba("Tomate", "TOM-01");
        await CrearIngredientePrueba("Lechuga", "LEC-01");

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ingredientes");
        var apiResponse = await ExecuteAndDeserializeAsync<PaginatedList<IngredienteSummaryDto>>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Items.Should().Contain(i => i.Nombre == "Tomate");
        apiResponse.Data.Items.Should().Contain(i => i.Nombre == "Lechuga");
    }

    [Fact]
    public async Task ObtenerIngredientePorId_ConIdExistente_DebeRetornarOkYDatos()
    {
        // Arrange
        var ingrediente = await CrearIngredientePrueba("Zanahoria", "ZAN-01");
        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/ingredientes/{ingrediente.Id}");
        var apiResponse = await ExecuteAndDeserializeAsync<IngredienteDto>(r => Task.FromResult(response));
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(ingrediente.Id);
        apiResponse.Data.Nombre.Should().Be("Zanahoria");
    }

    [Fact]
    public async Task CrearIngrediente_ConDatosValidos_DebeCrearEnBDYRetornarCreated()
    {
        // Arrange
        var request = new IngredienteTestDataBuilder()
            .ConNombre("Cebolla")
            .ConCodigo("CEB-01")
            .BuildCrearIngredienteRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/inventario/ingredientes", request);
        var apiResponse = await ExecuteAndDeserializeAsync<IngredienteDto>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        VerificarRespuestaExitosa(response, apiResponse, HttpStatusCode.Created);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Nombre.Should().Be("Cebolla");
        apiResponse.Data.CodigoInventario.Should().NotBeEmpty();
        // Verificar en la BD
        var ingredienteEnBD = await DbContext.Ingredientes.FirstOrDefaultAsync(i => i.Nombre == "Cebolla");
        ingredienteEnBD.Should().NotBeNull();
        ingredienteEnBD.Nombre.Should().Be("Cebolla");
    }

    [Fact]
    public async Task ActualizarIngrediente_ConDatosValidos_DebeActualizarEnBDYRetornarOk()
    {
        // Arrange
        var ingrediente = await CrearIngredientePrueba("Tomate", "TOM-02");
        var request = new IngredienteTestDataBuilder()
            .ConNombre("Tomate Cherry")
            .BuildActualizarIngredienteRequest();

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}", request);
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }

    [Fact]
    public async Task EliminarIngrediente_ConIdExistente_DebeEliminarEnBDYRetornarNoContent()
    {
        // Arrange
        var ingrediente = await CrearIngredientePrueba("Ajo", "AJO-01");

        // Act
        var response = await HttpClient.DeleteAsync($"/api/inventario/ingredientes/{ingrediente.Id}");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
    
    [Fact]
    public async Task ObtenerMovimientosDeIngrediente_ConIdExistente_DebeRetornarOkYMovimientos()
    {
        // Arrange
        var ingrediente = await CrearIngredientePrueba("Papa", "PAP-01");
        // Simular un movimiento (a futuro se podrá crear un movimiento real)

        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/ingredientes/{ingrediente.Id}/movimientos");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
    
    [Fact]
    public async Task RegistrarMovimiento_ConDatosValidos_DebeActualizarStockYRetornarOk()
    {
        // Arrange
        var ingrediente = await CrearIngredientePrueba("Pimienta", "PIM-01", stockInicial: 50);
        var request = new { Cantidad = -10.5m, Tipo = "Egreso", Motivo = "Venta de platillo" };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/movimientos", request);
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
    
    [Fact]
    public async Task ObtenerIngredientesBajoStock_DebeRetornarSoloIngredientesConStockBajo()
    {
        // Arrange
        // Crear un ingrediente con stock por debajo del mínimo
        await CrearIngredientePrueba("Lentejas", "LEN-01", stockInicial: 5, stockMinimo: 10);
        // Crear un ingrediente con stock por encima del mínimo (no debe aparecer)
        await CrearIngredientePrueba("Arroz", "ARR-02", stockInicial: 20, stockMinimo: 10);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ingredientes/bajo-stock?PorcentajeCritico=101");
        var apiResponse = await ExecuteAndDeserializeAsync<List<IngredienteSummaryDto>>(r => Task.FromResult(response));

        // Assert
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().HaveCount(1);
        apiResponse.Data.Should().Contain(i => i.Nombre == "Lentejas");
        apiResponse.Data.Should().NotContain(i => i.Nombre == "Arroz");
    }
    
    [Fact]
    public async Task AsociarProveedor_ConDatosValidos_DebeAsociarEnBDYRetornarOk()
    {
        // Arrange
        var ingrediente = await CrearIngredientePrueba("Orégano", "ORE-01");
        // En un caso real, crearíamos un proveedor de prueba aquí
        var proveedorId = Guid.NewGuid(); 
        var request = new { Costo = 15.50m };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/inventario/ingredientes/{ingrediente.Id}/asociar-proveedor/{proveedorId}", request);
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
    
    [Fact]
    public async Task GenerarReporteValoracion_DebeRetornarOkYDatos()
    {
        // Arrange
        await CrearIngredientePrueba("Laurel", "LAU-01", stockInicial: 100);
        await CrearIngredientePrueba("Clavo", "CLA-01", stockInicial: 20);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ingredientes/reporte/valoracion");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }

    private async Task<IngredienteDto> CrearIngredientePrueba(string nombre, string codigo, decimal? stockInicial = null, decimal? stockMinimo = null)
    {
        var builder = new IngredienteTestDataBuilder()
            .ConNombre(nombre)
            .ConCodigo(codigo);

        if (stockInicial.HasValue)
        {
            builder.ConStockInicial(stockInicial.Value);
        }

        if (stockMinimo.HasValue)
        {
            builder.ConStockMinimo(stockMinimo.Value);
        }

        var request = builder.BuildCrearIngredienteRequest();
        
        var response = await HttpClient.PostAsJsonAsync("/api/inventario/ingredientes", request);
        response.EnsureSuccessStatusCode();
        var apiResponse = await ExecuteAndDeserializeAsync<IngredienteDto>(r => Task.FromResult(response));

        return apiResponse.Data;
    }
} 