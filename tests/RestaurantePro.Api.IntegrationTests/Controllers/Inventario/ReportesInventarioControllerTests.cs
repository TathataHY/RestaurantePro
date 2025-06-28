using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Application.Inventario.Reportes.Commands.RealizarInventarioFisico;
using RestaurantePro.Application.Inventario.Reportes.Commands.ExportarInventario;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerReporteGeneral;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAlertasInventario;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerRecomendacionesCompra;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerValorTotalInventario;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración para ReportesInventarioController
/// Valida todos los endpoints REST del controlador de reportes de inventario
/// </summary>
[Collection("Sequential")]
public class ReportesInventarioControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public ReportesInventarioControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetInventarioGeneral_DebeRetornarDatosReales()
    {
        // Arrange: Crear ingredientes reales en la BD
        var usuario = await CrearUsuarioPrueba("usuario.inventario", "Usuario Inventario", "inventario@test.com", RolUsuario.Administrador);
        // Arroz: stock 20, stockMinimo 30 → 'Bajo' (15 < 20 <= 30)
        var ingrediente1 = await CrearIngredientePrueba("Arroz", "ARZ-01", stockInicial: 20, stockMinimo: 30, costoPromedio: 2.5m);
        // Frijol: stock 10, stockMinimo 30 → 'Crítico' (10 <= 15)
        var ingrediente2 = await CrearIngredientePrueba("Frijol", "FRJ-01", stockInicial: 10, stockMinimo: 30, costoPromedio: 3.0m);
        // Aceite: stock 40, stockMinimo 30 → 'Normal' (40 > 30)
        var ingrediente3 = await CrearIngredientePrueba("Aceite", "ACE-01", stockInicial: 40, stockMinimo: 30, costoPromedio: 8.0m);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/general");
        var apiResponse = await ExecuteAndDeserializeAsync<ReporteGeneralInventarioDto>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Should().NotBeNull();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.IngredientesStockBajo.Should().BeGreaterThanOrEqualTo(1);
        apiResponse.Data.IngredientesStockCritico.Should().BeGreaterThanOrEqualTo(1);
        apiResponse.Data.TotalIngredientes.Should().BeGreaterThanOrEqualTo(3);
    }
    
    [Fact]
    public async Task GetAlertas_DebeRetornarAlertasReales()
    {
        // Arrange: Crear ingredientes con stock bajo y crítico
        var usuario = await CrearUsuarioPrueba("usuario.alertas", "Usuario Alertas", "alertas@test.com", RolUsuario.Administrador);
        var ingredienteBajo = await CrearIngredientePrueba("Sal", "SAL-01", stockInicial: 8, stockMinimo: 10);
        var ingredienteCritico = await CrearIngredientePrueba("Azúcar", "AZC-01", stockInicial: 2, stockMinimo: 10);
        var ingredienteNormal = await CrearIngredientePrueba("Pasta", "PAS-01", stockInicial: 50, stockMinimo: 10);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
        var apiResponse = await ExecuteAndDeserializeAsync<List<AlertaInventarioDto>>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().Contain(a => a.NombreIngrediente == "Sal" && a.TipoAlerta == "Bajo");
        apiResponse.Data.Should().Contain(a => a.NombreIngrediente == "Azúcar" && a.TipoAlerta == "Crítico");
        apiResponse.Data.Should().NotContain(a => a.NombreIngrediente == "Pasta");
    }

    [Fact]
    public async Task GetAnalisisInventario_DebeRetornarAnalisisReal()
    {
        // Arrange: Crear ingredientes y movimientos reales
        var usuario = await CrearUsuarioPrueba("usuario.analisis", "Usuario Analisis", "analisis@test.com", RolUsuario.Administrador);
        var ingrediente1 = await CrearIngredientePrueba("Lentejas", "LEN-01", stockInicial: 40, stockMinimo: 10);
        var ingrediente2 = await CrearIngredientePrueba("Garbanzo", "GAR-01", stockInicial: 15, stockMinimo: 10);
        // Simular movimientos (si el modelo lo permite, si no, solo stock inicial)

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/analisis");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRecomendacionesCompra_DebeRetornarRecomendacionesReales()
    {
        // Arrange: Crear ingredientes con stock bajo y crítico
        var usuario = await CrearUsuarioPrueba("usuario.recomendaciones", "Usuario Recomendaciones", "recomendaciones@test.com", RolUsuario.Administrador);
        var ingredienteBajo = await CrearIngredientePrueba("Harina", "HAR-01", stockInicial: 8, stockMinimo: 10);
        var ingredienteCritico = await CrearIngredientePrueba("Levadura", "LEV-01", stockInicial: 2, stockMinimo: 10);
        var ingredienteNormal = await CrearIngredientePrueba("Cacao", "CAC-01", stockInicial: 50, stockMinimo: 10);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/recomendaciones-compra");
        var apiResponse = await ExecuteAndDeserializeAsync<List<RecomendacionCompraDto>>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().Contain(r => r.NombreIngrediente == "Harina");
        apiResponse.Data.Should().Contain(r => r.NombreIngrediente == "Levadura");
        apiResponse.Data.Should().NotContain(r => r.NombreIngrediente == "Cacao");
        apiResponse.Data.All(r => r.CantidadRecomendada > 0).Should().BeTrue();
    }

    [Fact]
    public async Task RealizarInventarioFisico_DebeRetornarResultadoReal()
    {
        // Arrange: Crear ingredientes y preparar conteos
        var usuario = await CrearUsuarioPrueba("usuario.fisico", "Usuario Fisico", "fisico@test.com", RolUsuario.Administrador);
        var ingrediente1 = await CrearIngredientePrueba("Avena", "AVE-01", stockInicial: 20, stockMinimo: 10);
        var ingrediente2 = await CrearIngredientePrueba("Café", "CAF-01", stockInicial: 15, stockMinimo: 10);
        var fechaInventario = DateTime.Today;
        var conteos = new[]
        {
            new { IngredienteId = ingrediente1.Id, CantidadContada = 18m }, // Diferencia -2
            new { IngredienteId = ingrediente2.Id, CantidadContada = 17m }  // Diferencia +2
        };
        var command = new {
            FechaInventario = fechaInventario,
            UsuarioId = usuario.Id,
            Observaciones = "Conteo de prueba",
            Conteos = conteos
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/inventario/reportes/inventario-fisico", command);
        var apiResponse = await ExecuteAndDeserializeAsync<ResultadoInventarioFisicoDto>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        VerificarRespuestaExitosa(response, apiResponse, response.StatusCode);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.TotalItemsContados.Should().Be(2);
        apiResponse.Data.ItemsConDiferencias.Should().BeGreaterThan(0);
        apiResponse.Data.Diferencias.Should().Contain(d => d.NombreIngrediente == "Avena" && d.Diferencia < 0);
        apiResponse.Data.Diferencias.Should().Contain(d => d.NombreIngrediente == "Café" && d.Diferencia > 0);
    }

    [Fact]
    public async Task ExportarReporte_DebeRetornarArchivoExitoso()
    {
        // Arrange: Crear ingredientes reales
        var usuario = await CrearUsuarioPrueba("usuario.exportar", "Usuario Exportar", "exportar@test.com", RolUsuario.Administrador);
        await CrearIngredientePrueba("Maíz", "MAI-01", stockInicial: 30, stockMinimo: 10);
        await CrearIngredientePrueba("Trigo", "TRI-01", stockInicial: 25, stockMinimo: 10);

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/exportar?formato=PDF");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetValorTotalInventario_DebeRetornarValorCorrecto()
    {
        // Arrange: Crear ingredientes reales
        var usuario = await CrearUsuarioPrueba("usuario.valor", "Usuario Valor", "valor@test.com", RolUsuario.Administrador);
        var ingrediente1 = await CrearIngredientePrueba("Quinua", "QUI-01", stockInicial: 10, stockMinimo: 5, costoPromedio: 4.0m);
        var ingrediente2 = await CrearIngredientePrueba("Chía", "CHI-01", stockInicial: 20, stockMinimo: 5, costoPromedio: 3.0m);
        var valorEsperado = 10 * 4.0m + 20 * 3.0m; // 40 + 60 = 100

        // Act
        var response = await HttpClient.GetAsync("/api/inventario/reportes/valor-total");
        var apiResponse = await ExecuteAndDeserializeAsync<ValorTotalInventarioDto>(r => Task.FromResult(response));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.ValorTotal.Should().BeApproximately(valorEsperado, 0.01m);
        apiResponse.Data.TotalIngredientes.Should().BeGreaterThanOrEqualTo(2);
        apiResponse.Data.ValoresPorCategoria.Should().NotBeNull();
    }

    public new void Dispose()
    {
        base.Dispose();
    }
} 