using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System.Text.Json;
using Xunit;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Promociones;

/// <summary>
/// Tests de rendimiento para el módulo de Promociones
/// </summary>
[Collection("IntegrationTests")]
public class PromocionesPerformanceTests : BaseIntegrationTest
{
    public PromocionesPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerPromociones_ConMuchasPromociones_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedPromocionesAsync(100); // 100 promociones
        var client = CreateAuthenticatedClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().HaveCount(100);
    }

    [Fact]
    public async Task ObtenerPromociones_ConFiltros_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedPromocionesConEstadosAsync(50);
        var client = CreateAuthenticatedClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=Activa&tipo=PorcentajeTotal");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1500); // Menos de 1.5 segundos
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearPromocion_ConDatosValidos_DeberiaResponderRapidamente()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "PERF_TEST",
            Nombre = "Test de Rendimiento",
            Descripcion = "Promoción para test de rendimiento",
            Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.PostAsync("/api/comercial/promociones", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ActivarPromocion_ConIdValido_DeberiaResponderRapidamente()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var client = CreateAuthenticatedClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/activar", null);
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800); // Menos de 800ms
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AplicarPromocion_ConDatosValidos_DeberiaResponderRapidamente()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var request = new
        {
            PromocionId = promocionId,
            ClienteId = clienteId,
            FacturaId = facturaId,
            ComandaId = comandaId,
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 200m
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(request, GetJsonOptions());
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.PostAsync("/api/comercial/promociones/aplicar", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1200); // Menos de 1.2 segundos
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AplicarPromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerPromocionesAplicables_ConMuchasPromociones_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedPromocionesActivasAsync(50);
        var clienteId = Guid.NewGuid();
        var monto = 200m;
        var client = CreateAuthenticatedClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/aplicables?clienteId={clienteId}&monto={monto}");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1500); // Menos de 1.5 segundos
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearPromociones_EnLote_DeberiaMantenerRendimiento()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Crear 10 promociones en paralelo
        for (int i = 0; i < 10; i++)
        {
            var command = new CrearPromocionCommand
            {
                Codigo = $"BATCH_{i}",
                Nombre = $"Promoción Batch {i}",
                Descripcion = $"Promoción de lote {i}",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10 + i,
                MontoMinimo = 50 + (i * 10),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = i + 1
            };

            var json = JsonSerializer.Serialize(command, GetJsonOptions());
            var task = client.PostAsync("/api/comercial/promociones", 
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Menos de 5 segundos para 10 promociones
        
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
    }

    [Fact]
    public async Task AplicarPromociones_EnLote_DeberiaMantenerRendimiento()
    {
        // Arrange
        var promocionesIds = await SeedPromocionesActivasAsync(5);
        var clienteId = Guid.NewGuid();
        var client = CreateAuthenticatedClient();
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Aplicar 5 promociones en paralelo
        for (int i = 0; i < 5; i++)
        {
            var request = new
            {
                PromocionId = promocionesIds[i],
                ClienteId = clienteId,
                FacturaId = Guid.NewGuid(),
                ComandaId = Guid.NewGuid(),
                ProductosIds = new List<Guid> { Guid.NewGuid() },
                MontoOriginal = 200m + (i * 50)
            };

            var json = JsonSerializer.Serialize(request, GetJsonOptions());
            var task = client.PostAsync("/api/comercial/promociones/aplicar", 
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos para 5 aplicaciones
        
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
    }

    [Fact]
    public async Task ObtenerPromociones_ConPaginacion_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedPromocionesAsync(200); // 200 promociones
        var client = CreateAuthenticatedClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Menos de 3 segundos para 200 promociones
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().HaveCount(200);
    }

    [Fact]
    public async Task BuscarPromociones_ConTermino_DeberiaResponderRapidamente()
    {
        // Arrange
        await SeedPromocionesConNombresAsync(100);
        var client = CreateAuthenticatedClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=Activa");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
    }

    #region Helper Methods

    private async Task<Guid> SeedPromocionAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "PERF_SINGLE",
            Nombre = "Promoción Individual",
            Descripcion = "Promoción para test de rendimiento individual",
            Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        var result = await mediator.Send(command);
        return result.Value.Id;
    }

    private async Task<Guid> SeedPromocionActivaAsync()
    {
        var promocionId = await SeedPromocionAsync();
        var command = new ActivarPromocionCommand { Id = promocionId };
        
        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        await mediator.Send(command);
        
        return promocionId;
    }

    private async Task SeedPromocionesAsync(int cantidad)
    {
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"PERF_{i:D3}",
                Nombre = $"Promoción de Rendimiento {i}",
                Descripcion = $"Promoción {i} para test de rendimiento",
                Tipo = i % 2 == 0 ? RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal : RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.MontoFijoTotal,
                ValorDescuento = 10 + (i % 20),
                MontoMinimo = 50 + (i * 10),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = i % 3 == 0,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task SeedPromocionesConEstadosAsync(int cantidad)
    {
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"ESTADO_{i:D3}",
                Nombre = $"Promoción Estado {i}",
                Descripcion = $"Promoción {i} con estado específico",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10 + (i % 15),
                MontoMinimo = 50 + (i * 5),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task<List<Guid>> SeedPromocionesActivasAsync(int cantidad)
    {
        var promocionesIds = new List<Guid>();
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"ACTIVA_{i:D3}",
                Nombre = $"Promoción Activa {i}",
                Descripcion = $"Promoción activa {i}",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10 + (i % 20),
                MontoMinimo = 50 + (i * 10),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            var result = await mediator.Send(command);
            if (result.Succeeded)
            {
                await mediator.Send(new ActivarPromocionCommand { Id = result.Value.Id });
                promocionesIds.Add(result.Value.Id);
            }
        }

        return promocionesIds;
    }

    private async Task SeedPromocionesConNombresAsync(int cantidad)
    {
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"NOMBRE_{i:D3}",
                Nombre = $"Promoción Especial {i}",
                Descripcion = $"Promoción {i} con nombre específico",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10 + (i % 15),
                MontoMinimo = 50 + (i * 5),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    #endregion
}

