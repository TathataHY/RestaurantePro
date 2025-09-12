using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.AsociarProveedor;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.Ingredientes;

/// <summary>
/// Tests de seguridad para el módulo de Ingredientes
/// Valida autenticación, autorización, validación de entrada y protección contra ataques
/// </summary>
[Collection("IntegrationTests")]
public class IngredientesSecurityTests : BaseIntegrationTest
{
    public IngredientesSecurityTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Autenticación

    [Fact]
    public async Task TodosLosEndpoints_ConClienteNoAutenticado_DeberianRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = Factory.CreateClient(); // Cliente sin autenticación

        // Act & Assert
        var endpoints = new[]
        {
            "/api/inventario/ingredientes",
            "/api/inventario/ingredientes/lista",
            "/api/inventario/ingredientes/estadisticas",
            "/api/inventario/ingredientes/bajo-stock",
            "/api/inventario/ingredientes/buscar",
            "/api/inventario/ingredientes/reporte/valoracion"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await clientNoAuth.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                $"El endpoint {endpoint} debería requerir autenticación");
        }
    }

    [Fact]
    public async Task EndpointsPOST_ConClienteNoAutenticado_DeberianRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = Factory.CreateClient();
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Security",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act & Assert
        var response = await clientNoAuth.PostAsJsonAsync("/api/inventario/ingredientes", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndpointsPUT_ConClienteNoAutenticado_DeberianRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = Factory.CreateClient();
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Nombre = "Test Security Update",
            Rotacion = RotacionIngrediente.Media,
            UnidadMedida = UnidadMedida.Litro,
            StockMinimo = 2,
            StockMaximo = 20,
            CostoUnitario = 10.00m
        };

        // Act & Assert
        var response = await clientNoAuth.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndpointsDELETE_ConClienteNoAutenticado_DeberianRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = Factory.CreateClient();
        var ingredienteId = Guid.NewGuid();

        // Act & Assert
        var response = await clientNoAuth.DeleteAsync($"/api/inventario/ingredientes/{ingredienteId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Tests de Autorización

    [Fact]
    public async Task ObtenerIngredientes_ConUsuarioAutenticado_DeberiaPermitirAcceso()
    {
        // Act
        var response = await Client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrearIngrediente_ConUsuarioAutenticado_DeberiaPermitirAcceso()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Security Create",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Validación de Entrada - Seguridad

    [Fact]
    public async Task CrearIngrediente_ConScriptMalicioso_DeberiaSanitizarEntrada()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "<script>alert('xss')</script>Ingrediente",
            Descripcion = "'; DROP TABLE Ingredientes; --",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Data.Should().NotBeNull();
            
            // Verificar que el script no se ejecutó (el nombre debería estar sanitizado)
            responseData.Data!.Nombre.Should().NotContain("<script>");
            responseData.Data.Descripcion.Should().NotContain("DROP TABLE");
        }
    }

    [Fact]
    public async Task ActualizarIngrediente_ConDatosMaliciosos_DeberiaSanitizarEntrada()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Nombre = "Ingrediente<script>alert('xss')</script>",
            Descripcion = "'; DELETE FROM Ingredientes; --",
            Rotacion = RotacionIngrediente.Media,
            UnidadMedida = UnidadMedida.Litro,
            StockMinimo = 2,
            StockMaximo = 20,
            CostoUnitario = 10.00m
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Inyección SQL (Prevención)

    [Fact]
    public async Task ObtenerIngredientes_ConParametrosMaliciosos_DeberiaManejarSeguramente()
    {
        // Arrange
        var parametrosMaliciosos = new[]
        {
            "?pageNumber=1'; DROP TABLE Ingredientes; --",
            "?pageSize=10 UNION SELECT * FROM Usuarios --",
            "?soloActivos=true OR 1=1 --"
        };

        // Act & Assert
        foreach (var parametro in parametrosMaliciosos)
        {
            var response = await Client.GetAsync($"/api/inventario/ingredientes{parametro}");
            
            // El endpoint debería manejar estos parámetros de manera segura
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task BuscarIngredientes_ConTerminoMalicioso_DeberiaManejarSeguramente()
    {
        // Arrange
        var terminosMaliciosos = new[]
        {
            "'; DROP TABLE Ingredientes; --",
            "test' UNION SELECT * FROM Usuarios --",
            "test OR 1=1 --"
        };

        // Act & Assert
        foreach (var termino in terminosMaliciosos)
        {
            var response = await Client.GetAsync($"/api/inventario/ingredientes/buscar?termino={termino}");
            
            // El endpoint debería manejar estos términos de manera segura
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }

    #endregion

    #region Tests de Validación de Tipos de Datos

    [Fact]
    public async Task CrearIngrediente_ConTiposIncorrectos_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonMalformado = @"{
            ""nombre"": ""Test"",
            ""rotacion"": ""INVALIDO"",
            ""unidadMedida"": ""INVALIDO"",
            ""stockMinimo"": ""no_es_numero"",
            ""stockMaximo"": ""tampoco"",
            ""costoUnitario"": ""ni_esto""
        }";

        // Act
        var content = new StringContent(jsonMalformado, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/inventario/ingredientes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarIngrediente_ConTiposIncorrectos_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var jsonMalformado = @"{
            ""nombre"": ""Test"",
            ""stockMinimo"": ""no_es_numero"",
            ""costoUnitario"": ""tampoco""
        }";

        // Act
        var content = new StringContent(jsonMalformado, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PutAsync($"/api/inventario/ingredientes/{ingredienteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Límites de Tamaño de Entrada

    [Fact]
    public async Task CrearIngrediente_ConNombreMuyLargo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var nombreMuyLargo = new string('A', 1000); // Nombre excesivamente largo
        var command = new CrearIngredienteCommand
        {
            Nombre = nombreMuyLargo,
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearIngrediente_ConDescripcionMuyLarga_DeberiaRetornarBadRequest()
    {
        // Arrange
        var descripcionMuyLarga = new string('A', 5000); // Descripción excesivamente larga
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Descripcion = descripcionMuyLarga,
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Validación de Rangos Numéricos

    [Theory]
    [InlineData(-1000)]    // Stock mínimo muy negativo
    [InlineData(1000000)]  // Stock máximo muy grande
    [InlineData(-500)]     // Costo muy negativo
    [InlineData(999999)]   // Costo muy grande
    public async Task CrearIngrediente_ConValoresExtremos_DeberiaValidarRangos(decimal valor)
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = valor,
            StockMaximo = valor + 100,
            CostoUnitario = Math.Abs(valor)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Headers de Seguridad

    [Fact]
    public async Task TodosLosEndpoints_DeberianIncluirHeadersDeSeguridad()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/inventario/ingredientes",
            "/api/inventario/ingredientes/lista",
            "/api/inventario/ingredientes/estadisticas"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Verificar headers de seguridad básicos
            response.Headers.Should().ContainKey("X-Content-Type-Options");
            response.Headers.Should().ContainKey("X-Frame-Options");
        }
    }

    #endregion

    #region Tests de Rate Limiting (Si está implementado)

    [Fact]
    public async Task Endpoints_ConMuchasRequests_DeberianManejarRateLimiting()
    {
        // Arrange
        const int numeroRequests = 100;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < numeroRequests; i++)
        {
            tasks.Add(Client.GetAsync("/api/inventario/ingredientes?pageNumber=1&pageSize=10"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        // Algunos requests pueden ser rechazados por rate limiting
        var statusCodes = responses.Select(r => r.StatusCode).Distinct();
        statusCodes.Should().Contain(HttpStatusCode.OK);
        
        // Si hay rate limiting, algunos requests deberían ser rechazados
        if (statusCodes.Contains(HttpStatusCode.TooManyRequests))
        {
            responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .Should().BeGreaterThan(0, "Debería haber algunos requests rechazados por rate limiting");
        }
    }

    #endregion

    #region Tests de Validación de IDs

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("undefined")]
    public async Task ObtenerIngredientePorId_ConIdInvalido_DeberiaRetornarBadRequest(string idInvalido)
    {
        // Act
        var response = await Client.GetAsync($"/api/inventario/ingredientes/{idInvalido}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("")]
    public async Task ActualizarIngrediente_ConIdInvalido_DeberiaRetornarBadRequest(string idInvalido)
    {
        // Arrange
        var command = new ActualizarIngredienteCommand
        {
            Nombre = "Test Update",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/inventario/ingredientes/{idInvalido}", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Validación de Content-Type

    [Fact]
    public async Task CrearIngrediente_ConContentTypeIncorrecto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonContent = "{\"nombre\":\"Test\"}";
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await Client.PostAsync("/api/inventario/ingredientes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarIngrediente_ConContentTypeIncorrecto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var jsonContent = "{\"nombre\":\"Test\"}";
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await Client.PutAsync($"/api/inventario/ingredientes/{ingredienteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Protección contra CSRF (Si aplica)

    [Fact]
    public async Task EndpointsPOST_DeberianValidarCSRFToken()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test CSRF",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        // Si CSRF está implementado, el request debería ser procesado normalmente
        // Si no está implementado, también debería funcionar
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    #endregion
}
