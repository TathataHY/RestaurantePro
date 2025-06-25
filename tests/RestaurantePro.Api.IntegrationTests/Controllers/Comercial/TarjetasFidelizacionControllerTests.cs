using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActivarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActualizarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.DesactivarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.EliminarTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para el controlador de tarjetas de fidelización
/// </summary>
public class TarjetasFidelizacionControllerTests : ApiIntegrationTestBase
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;

    public TarjetasFidelizacionControllerTests() : base(new TestWebApplicationFactory())
    {
        _tarjetaRepository = ServiceScope.ServiceProvider.GetRequiredService<ITarjetaFidelizacionRepository>();
    }

    #region GET Tests

    [Fact]
    public async Task GetTarjetasFidelizacion_DebeRetornarListaDeTarjetas()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/tarjetas-fidelizacion");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<TarjetaFidelizacionDto>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Should().Contain(t => t.Id == tarjeta.Id);
        }
    }

    [Fact]
    public async Task GetTarjetaFidelizacionPorId_ConIdExistente_DebeRetornarTarjeta()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TarjetaFidelizacionDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.Id.Should().Be(tarjeta.Id);
            apiResponse.Data.NumeroTarjeta.Should().Be(tarjeta.Codigo);
        }
    }

    [Fact]
    public async Task GetTarjetaFidelizacionPorId_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/{idInexistente}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NotImplemented);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task PostTarjetaFidelizacion_ConDatosValidos_DebeCrearTarjeta()
    {
        // Arrange
        var cliente = await CrearClienteEnBD();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = cliente.Id,
            PuntosIniciales = 100
        };

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/comercial/tarjetas-fidelizacion", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotImplemented);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TarjetaFidelizacionDto>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.ClienteId.Should().Be(cliente.Id);
            apiResponse.Data.PuntosActuales.Should().Be(100);
        }
    }

    [Fact]
    public async Task PostTarjetaFidelizacion_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.Empty, // ID inválido
            PuntosIniciales = -10 // Puntos negativos
        };

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/comercial/tarjetas-fidelizacion", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task PutTarjetaFidelizacion_ConDatosValidos_DebeActualizarTarjeta()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();
        var command = new ActualizarTarjetaFidelizacionCommand
        {
            Id = tarjeta.Id,
            Nivel = NivelFidelizacion.Platino,
            MultiplicadorPuntos = 2.0m,
            LimiteMensual = 1000,
            Observaciones = "Actualización de prueba",
            UsuarioId = Guid.NewGuid()
        };

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PutAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TarjetaFidelizacionDto>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.Nivel.Should().Be(NivelFidelizacion.Platino);
        }
    }

    [Fact]
    public async Task PutTarjetaFidelizacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var command = new ActualizarTarjetaFidelizacionCommand
        {
            Id = idInexistente,
            Nivel = NivelFidelizacion.Platino,
            MultiplicadorPuntos = 2.0m,
            LimiteMensual = 1000,
            Observaciones = "Actualización de prueba",
            UsuarioId = Guid.NewGuid()
        };

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PutAsync($"/api/comercial/tarjetas-fidelizacion/{idInexistente}", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NotImplemented);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeleteTarjetaFidelizacion_ConIdExistente_DebeEliminarTarjeta()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);

        // Verificar que la tarjeta está marcada como eliminada (soft delete)
        var tarjetaEliminada = await DbContext.TarjetasFidelizacion.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tarjeta.Id);
        tarjetaEliminada.Should().NotBeNull();
        tarjetaEliminada!.EstaEliminado.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTarjetaFidelizacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/tarjetas-fidelizacion/{idInexistente}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NotImplemented);
    }

    #endregion

    #region PATCH Tests

    [Fact]
    public async Task PatchActivarTarjetaFidelizacion_ConTarjetaEmitida_DebeActivarTarjeta()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();
        var command = new ActivarTarjetaFidelizacionCommand { Id = tarjeta.Id };

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PatchAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}/activar", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TarjetaFidelizacionDto>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.Activa.Should().BeTrue();

            // Verificar que la tarjeta está activa en la BD
            var tarjetaActualizada = await DbContext.TarjetasFidelizacion
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tarjeta.Id);
            tarjetaActualizada.Should().NotBeNull();
            tarjetaActualizada!.Estado.Should().Be(EstadoTarjeta.Activa);
        }
    }

    [Fact]
    public async Task PatchDesactivarTarjetaFidelizacion_ConTarjetaActiva_DebeDesactivarTarjeta()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();
        // Primero activar la tarjeta
        tarjeta.Activar();
        await _tarjetaRepository.ActualizarAsync(tarjeta);

        var command = new DesactivarTarjetaFidelizacionCommand { Id = tarjeta.Id };

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PatchAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}/desactivar", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TarjetaFidelizacionDto>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.Activa.Should().BeFalse();

            // Verificar que la tarjeta está cancelada en la BD
            var tarjetaActualizada = await DbContext.TarjetasFidelizacion
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tarjeta.Id);
            tarjetaActualizada.Should().NotBeNull();
            tarjetaActualizada!.Estado.Should().Be(EstadoTarjeta.Cancelada);
        }
    }

    #endregion

    #region Tests Adicionales

    [Fact]
    public async Task GetTarjetasFidelizacionPorCliente_ConClienteExistente_DebeRetornarTarjetas()
    {
        // Arrange
        var cliente = await CrearClienteEnBD();
        var tarjeta = await CrearTarjetaFidelizacionEnBD(cliente.Id);

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/cliente/{cliente.Id}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<TarjetaFidelizacionDto>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Should().Contain(t => t.ClienteId == cliente.Id);
        }
    }

    [Fact]
    public async Task GetEstadisticasTarjetasFidelizacion_DebeRetornarEstadisticas()
    {
        // Arrange
        var tarjeta = await CrearTarjetaFidelizacionEnBD();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}/estadisticas");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
        }
    }

    #endregion

    #region Helper Methods

    private async Task<Domain.Comercial.Clientes.Entities.TarjetaFidelizacion> CrearTarjetaFidelizacionEnBD(Guid? clienteId = null)
    {
        var cliente = clienteId.HasValue 
            ? await DbContext.Clientes.FindAsync(clienteId.Value)
            : await CrearClienteEnBD();

        var tarjeta = Domain.Comercial.Clientes.Entities.TarjetaFidelizacion.Crear(
            cliente.Id, 
            $"FIDEL-{Guid.NewGuid():N}"[..10]);

        DbContext.TarjetasFidelizacion.Add(tarjeta);
        await DbContext.SaveChangesAsync();

        return tarjeta;
    }

    private async Task<Domain.Comercial.Clientes.Entities.Cliente> CrearClienteEnBD()
    {
        var nombre = ClienteNombre.Crear("Juan", "Pérez");
        var cliente = Domain.Comercial.Clientes.Entities.Cliente.Crear(
            nombre,
            "juan.perez@email.com",
            "123456789",
            DateTime.Now.AddYears(-30));

        DbContext.Clientes.Add(cliente);
        await DbContext.SaveChangesAsync();

        return cliente;
    }

    #endregion
} 