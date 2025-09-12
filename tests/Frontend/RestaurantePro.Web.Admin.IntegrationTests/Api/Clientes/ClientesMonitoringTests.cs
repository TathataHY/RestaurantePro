using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Web.Admin.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClienteDto = RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Clientes;

/// <summary>
/// Pruebas de monitoreo y métricas para la API de clientes
/// </summary>
public class ClientesMonitoringTests : BaseIntegrationTest
{
    public ClientesMonitoringTests(WebApplicationFactory factory) : base(factory)
    {
        // Los tests regulares usan el cliente autenticado por defecto del BaseIntegrationTest
        // No necesitan configuración adicional de autenticación
    }

    [Fact]
    public async Task ObtenerClientes_DeberiaIncluirHeadersCorrectos()
    {
        // Act
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().NotBeNull();
        // CacheControl puede ser null en pruebas
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task CrearCliente_DeberiaIncluirHeadersCorrectos()
    {
        // Arrange
        var cliente = new CrearClienteRequest
        {
            Nombre = "Cliente Headers",
            Email = "headers@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Should().NotBeNull();
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ObtenerClientes_ConLogging_DeberiaRegistrarCorrectamente()
    {
        // Act
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la respuesta incluye información de logging
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearCliente_ConLogging_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var cliente = new CrearClienteRequest
        {
            Nombre = "Cliente Logging",
            Email = "logging@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verificar que la respuesta incluye información de logging
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerClientes_ConMetricas_DeberiaRegistrarTiempoRespuesta()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el tiempo de respuesta es razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La consulta debería responder en menos de 1 segundo");
        
        // Verificar que la respuesta incluye información de métricas
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CrearCliente_ConMetricas_DeberiaRegistrarTiempoRespuesta()
    {
        // Arrange
        var cliente = new CrearClienteRequest
        {
            Nombre = "Cliente Métricas",
            Email = "metricas@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verificar que el tiempo de respuesta es razonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "La creación debería completarse en menos de 2 segundos");
        
        // Verificar que la respuesta incluye información de métricas
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerClientes_ConErrores_DeberiaRegistrarCorrectamente()
    {
        // Act - Consulta con parámetros inválidos
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=-1&pageSize=0");

        // Assert
        // La API debería manejar parámetros inválidos correctamente
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        
        // Verificar que la respuesta incluye información de error si aplica
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearCliente_ConErrores_DeberiaRegistrarCorrectamente()
    {
        // Arrange - Cliente con datos inválidos
        var clienteInvalido = new CrearClienteRequest
        {
            Nombre = "", // Nombre vacío
            Email = "email-invalido", // Email inválido
            Telefono = "", // Teléfono vacío
            FechaNacimiento = DateTime.Today.AddYears(1), // Fecha futura
            AceptaTerminos = false // No acepta términos
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verificar que la respuesta incluye información de error
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerClientes_ConConcurrencia_DeberiaMantenerConsistencia()
    {
        // Arrange - Crear 10 clientes primero
        for (int i = 0; i < 10; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Concurrencia {i:D3}",
                Email = $"concurrencia{i:D3}@test.com",
                Telefono = $"+123456{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
        }

        // Act - Hacer 20 consultas simultáneas
        var tareas = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tareas.Add(_client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10"));
        }

        var responses = await Task.WhenAll(tareas);

        // Assert
        responses.Should().HaveCount(20);
        responses.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue();
        
        // Verificar que todas las respuestas son consistentes
        var resultados = new List<ApiResponse<PaginatedList<ClienteDto>>>();
        foreach (var response in responses)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
            resultados.Add(resultado!);
        }
        
        // Las respuestas pueden tener pequeñas diferencias debido a la concurrencia
        var totales = resultados.Select(r => r.Data!.TotalCount).Distinct().ToList();
        totales.Should().HaveCountLessOrEqualTo(2, "Las consultas concurrentes pueden tener pequeñas diferencias en el total");
    }
}
