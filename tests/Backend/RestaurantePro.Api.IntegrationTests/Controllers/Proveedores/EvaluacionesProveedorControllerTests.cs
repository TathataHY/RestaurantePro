using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.CrearEvaluacionProveedor;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Proveedores;

/// <summary>
/// Tests de integración para EvaluacionesProveedorController
/// </summary>
[Collection("Sequential")]
public class EvaluacionesProveedorControllerTests : ApiIntegrationTestBase
{
    public EvaluacionesProveedorControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetEvaluaciones_DebeRetornarListaVaciaCuandoNoHayEvaluaciones()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/proveedores/evaluaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<EvaluacionProveedorDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearEvaluacion_DebeCrearEvaluacionExitosamente()
    {
        // Arrange - Crear proveedor de prueba
        var proveedor = await CreateTestProveedorAsync();
        var command = new CrearEvaluacionProveedorCommand
        {
            ProveedorId = proveedor.Id,
            CalificacionGeneral = 4,
            CalificacionCalidad = 5,
            CalificacionPuntualidad = 4,
            CalificacionComunicacion = 3,
            CalificacionPrecios = 4,
            Comentarios = "Excelente proveedor, muy confiable"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/proveedores/evaluaciones", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EvaluacionProveedorDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().NotBeEmpty();
        apiResponse.Data.ProveedorId.Should().Be(proveedor.Id);
        apiResponse.Data.CalificacionGeneral.Should().Be(4);
        apiResponse.Data.PromedioPonderado.Should().BeGreaterThan(0);
        apiResponse.Data.Activa.Should().BeTrue();
    }

    [Fact]
    public async Task GetEvaluacion_DebeRetornarEvaluacionExistente()
    {
        // Arrange - Crear proveedor y evaluación
        var proveedor = await CreateTestProveedorAsync();
        var evaluacion = await CreateTestEvaluacionAsync(proveedor.Id);

        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/{evaluacion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EvaluacionProveedorDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(evaluacion.Id);
        apiResponse.Data.ProveedorId.Should().Be(proveedor.Id);
    }

    [Fact]
    public async Task GetEvaluacion_DebeRetornar404ParaEvaluacionInexistente()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarEvaluacion_DebeActualizarEvaluacionExitosamente()
    {
        // Arrange - Crear proveedor y evaluación
        var proveedor = await CreateTestProveedorAsync();
        var evaluacion = await CreateTestEvaluacionAsync(proveedor.Id);
        
        var updateCommand = new
        {
            Id = evaluacion.Id,
            CalificacionGeneral = 5,
            CalificacionCalidad = 5,
            CalificacionPuntualidad = 5,
            CalificacionComunicacion = 5,
            CalificacionPrecios = 5,
            Comentarios = "Evaluación actualizada - Excelente en todos los aspectos"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/proveedores/evaluaciones/{evaluacion.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EvaluacionProveedorDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.CalificacionGeneral.Should().Be(5);
        apiResponse.Data.Comentarios.Should().Contain("actualizada");
    }

    [Fact]
    public async Task EliminarEvaluacion_DebeEliminarEvaluacionExitosamente()
    {
        // Arrange - Crear proveedor y evaluación
        var proveedor = await CreateTestProveedorAsync();
        var evaluacion = await CreateTestEvaluacionAsync(proveedor.Id);

        // Act
        var response = await HttpClient.DeleteAsync($"/api/proveedores/evaluaciones/{evaluacion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();

        // Verificar que la evaluación sigue accesible pero está desactivada (soft delete)
        var getResponse = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/{evaluacion.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var evaluacionResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<EvaluacionProveedorDto>>();
        evaluacionResponse.Should().NotBeNull();
        evaluacionResponse!.Success.Should().BeTrue();
        evaluacionResponse.Data.Should().NotBeNull();
        evaluacionResponse.Data!.Activa.Should().BeFalse(); // Debe estar desactivada

        // Verificar que no aparece en las listas cuando se filtran por activas
        var listResponse = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/proveedor/{proveedor.Id}?soloActivas=true");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listApiResponse = await listResponse.Content.ReadFromJsonAsync<ApiResponse<List<EvaluacionProveedorDto>>>();
        listApiResponse.Should().NotBeNull();
        listApiResponse!.Success.Should().BeTrue();
        listApiResponse.Data.Should().NotBeNull();
        listApiResponse.Data!.Should().NotContain(e => e.Id == evaluacion.Id); // No debe aparecer en la lista de activas
    }

    [Fact]
    public async Task GetEvaluacionesPorProveedor_DebeRetornarEvaluacionesDelProveedor()
    {
        // Arrange - Crear proveedor y múltiples evaluaciones
        var proveedor = await CreateTestProveedorAsync();
        var evaluacion1 = await CreateTestEvaluacionAsync(proveedor.Id);
        var evaluacion2 = await CreateTestEvaluacionAsync(proveedor.Id);

        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/proveedor/{proveedor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<EvaluacionProveedorDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().BeGreaterThanOrEqualTo(2);
        apiResponse.Data.Should().OnlyContain(e => e.ProveedorId == proveedor.Id);
    }

    [Fact]
    public async Task GetPromedioEvaluaciones_DebeRetornarPromedioCorrecto()
    {
        // Arrange - Crear proveedor y evaluaciones
        var proveedor = await CreateTestProveedorAsync();
        await CreateTestEvaluacionAsync(proveedor.Id, 4, 4, 4, 4, 4);
        await CreateTestEvaluacionAsync(proveedor.Id, 5, 5, 5, 5, 5);

        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/promedio/{proveedor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PromedioEvaluacionesDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.ProveedorId.Should().Be(proveedor.Id);
        apiResponse.Data.TotalEvaluaciones.Should().BeGreaterThanOrEqualTo(2);
        apiResponse.Data.PromedioPonderado.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CrearEvaluacion_DebeRetornar400ParaProveedorInexistente()
    {
        // Arrange
        var command = new CrearEvaluacionProveedorCommand
        {
            ProveedorId = Guid.NewGuid(), // Proveedor que no existe
            CalificacionGeneral = 4,
            CalificacionCalidad = 4,
            CalificacionPuntualidad = 4,
            CalificacionComunicacion = 4,
            CalificacionPrecios = 4,
            Comentarios = "Test"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/proveedores/evaluaciones", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Métodos auxiliares para crear datos de prueba
    private async Task<Proveedor> CreateTestProveedorAsync()
    {
        var proveedor = Proveedor.Crear(
            "Proveedor Test",
            "Contacto Test",
            "test@proveedor.com",
            "+56912345678",
            "Dirección Test",
            "Santiago",
            "1234567",
            "Chile",
            "TEST123456",
            "Banco Test",
            30
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IProveedoresDbContext>();
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();

        return proveedor;
    }

    private async Task<EvaluacionProveedor> CreateTestEvaluacionAsync(
        Guid proveedorId, 
        int calGeneral = 4, 
        int calCalidad = 4, 
        int calPuntualidad = 4, 
        int calComunicacion = 4, 
        int calPrecios = 4)
    {
        var evaluacion = EvaluacionProveedor.Crear(
            proveedorId,
            Guid.NewGuid(), // Evaluador de prueba
            calGeneral,
            calCalidad,
            calPuntualidad,
            calComunicacion,
            calPrecios,
            "Evaluación de prueba"
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IProveedoresDbContext>();
        context.EvaluacionesProveedores.Add(evaluacion);
        await context.SaveChangesAsync();

        return evaluacion;
    }
} 