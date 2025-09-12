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
/// Pruebas de estrés y carga para la API de clientes
/// </summary>
public class ClientesStressTests : BaseIntegrationTest
{
    public ClientesStressTests(WebApplicationFactory factory) : base(factory)
    {
        // Los tests regulares usan el cliente autenticado por defecto del BaseIntegrationTest
        // No necesitan configuración adicional de autenticación
    }

    [Fact]
    public async Task CrearClientes_Masiva_DeberiaManejarCorrectamente()
    {
        // Arrange - Crear 100 clientes simultáneamente
        var tareas = new List<Task<HttpResponseMessage>>();
        var clientes = new List<CrearClienteRequest>();

        for (int i = 0; i < 100; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Stress {i:D3}",
                Email = $"stress{i:D3}@test.com",
                Telefono = $"+123456{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            clientes.Add(cliente);
            tareas.Add(_client.PostAsJsonAsync("/api/comercial/clientes", cliente));
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var responses = await Task.WhenAll(tareas);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(100);
        responses.All(r => r.StatusCode == HttpStatusCode.Created).Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "100 clientes deberían crearse en menos de 10 segundos");
    }

    [Fact]
    public async Task ObtenerClientes_ConCargaAlta_DeberiaMantenerRendimiento()
    {
        // Arrange - Crear 50 clientes primero
        for (int i = 0; i < 50; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Carga {i:D3}",
                Email = $"carga{i:D3}@test.com",
                Telefono = $"+123456{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
        }

        // Act - Hacer 100 consultas simultáneas
        var tareas = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 100; i++)
        {
            tareas.Add(_client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10"));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var responses = await Task.WhenAll(tareas);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(100);
        responses.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "100 consultas deberían completarse en menos de 5 segundos");
    }

    [Fact]
    public async Task ActualizarClientes_ConCargaAlta_DeberiaMantenerConsistencia()
    {
        // Arrange - Crear 20 clientes primero
        var clientesIds = new List<Guid>();
        for (int i = 0; i < 20; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Actualizar {i:D3}",
                Email = $"actualizar{i:D3}@test.com",
                Telefono = $"+123456{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            
            var response = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
            clientesIds.Add(resultado!.Data!.Id);
        }

        // Act - Actualizar todos los clientes simultáneamente
        var tareas = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < clientesIds.Count; i++)
        {
            var clienteActualizado = new ActualizarClienteRequest
            {
                Nombre = $"Cliente Actualizado {i:D3}",
                Apellidos = $"Apellido {i:D3}",
                Email = $"actualizado{i:D3}@test.com",
                Telefono = $"+987654{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-30),
                AceptaMarketing = true
            };
            
            tareas.Add(_client.PutAsJsonAsync($"/api/comercial/clientes/{clientesIds[i]}", clienteActualizado));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var responses = await Task.WhenAll(tareas);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(20);
        responses.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "20 actualizaciones deberían completarse en menos de 5 segundos");
    }

    [Fact]
    public async Task CrearClientes_ConDatosVariados_DeberiaManejarCorrectamente()
    {
        // Arrange - Crear clientes con datos muy variados
        var tareas = new List<Task<HttpResponseMessage>>();
        var clientes = new List<CrearClienteRequest>();

        // Cliente con caracteres especiales
        clientes.Add(new CrearClienteRequest
        {
            Nombre = "José María OConnor Smith",
            Email = "jose.oconnor@test.com",
            Telefono = "+51-987-654-321",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        });

        // Cliente con email internacional
        clientes.Add(new CrearClienteRequest
        {
            Nombre = "John Smith",
            Email = "john@company.co.uk",
            Telefono = "+442079460958",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            AceptaTerminos = true
        });

        // Cliente con nombre largo pero válido
        clientes.Add(new CrearClienteRequest
        {
            Nombre = "María de los Ángeles García",
            Email = "maria.angeles@test.com",
            Telefono = "+51987654321",
            FechaNacimiento = DateTime.Today.AddYears(-35),
            AceptaTerminos = true
        });

        // Cliente con datos mínimos
        clientes.Add(new CrearClienteRequest
        {
            Nombre = "Ana García",
            Email = "ana@test.com",
            Telefono = "+51987654321",
            FechaNacimiento = DateTime.Today.AddYears(-20),
            AceptaTerminos = true
        });

        // Cliente con datos completos
        clientes.Add(new CrearClienteRequest
        {
            Nombre = "Carlos López",
            Email = "carlos@test.com",
            Telefono = "+51987654321",
            FechaNacimiento = DateTime.Today.AddYears(-40),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        });

        foreach (var cliente in clientes)
        {
            tareas.Add(_client.PostAsJsonAsync("/api/comercial/clientes", cliente));
        }

        // Act
        var responses = await Task.WhenAll(tareas);

        // Assert
        responses.Should().HaveCount(5);
        responses.All(r => r.StatusCode == HttpStatusCode.Created).Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerClientes_ConPaginacionExtrema_DeberiaMantenerRendimiento()
    {
        // Arrange - Crear 100 clientes primero
        for (int i = 0; i < 100; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Paginacion {i:D3}",
                Email = $"paginacion{i:D3}@test.com",
                Telefono = $"+123456{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
        }

        // Act - Probar diferentes configuraciones de paginación
        var configuraciones = new[]
        {
            "pageNumber=1&pageSize=1",
            "pageNumber=50&pageSize=2",
            "pageNumber=1&pageSize=100",
            "pageNumber=2&pageSize=50",
            "pageNumber=10&pageSize=10"
        };

        var tareas = new List<Task<HttpResponseMessage>>();
        foreach (var config in configuraciones)
        {
            tareas.Add(_client.GetAsync($"/api/comercial/clientes?{config}"));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var responses = await Task.WhenAll(tareas);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(5);
        responses.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "5 consultas de paginación deberían completarse en menos de 3 segundos");
    }
}
