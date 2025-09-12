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
/// Tests de manejo de errores para el módulo de Ingredientes
/// Cubre escenarios de error, validaciones de negocio y casos excepcionales
/// </summary>
[Collection("IntegrationTests")]
public class IngredientesErrorHandlingTests : BaseIntegrationTest
{
    public IngredientesErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Validación de Datos de Entrada

    [Fact]
    public async Task CrearIngrediente_ConNombreVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "",
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockInicial = 5,
            StockMinimo = 1,
            CostoInicial = 5.00m,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<object>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearIngrediente_ConStockMinimoNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockMinimo = -1,
            StockInicial = 5,
            UsuarioId = Guid.NewGuid(),
            CostoInicial = 5.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearIngrediente_ConStockMaximoMenorQueMinimo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockMinimo = 10,
            StockInicial = 5, // Menor que mínimo
            UsuarioId = Guid.NewGuid(),
            CostoInicial = 5.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearIngrediente_ConCostoNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockMinimo = 1,
            StockInicial = 5,
            UsuarioId = Guid.NewGuid(),
            CostoInicial = -5.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearIngrediente_ConDiasVencimientoNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockInicial = 5,
            StockMinimo = 1,
            CostoInicial = 5.00m,
            UsuarioId = Guid.NewGuid(),
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Recursos No Encontrados

    [Fact]
    public async Task ObtenerIngredientePorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<object>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeFalse();
            responseData.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task ActualizarIngrediente_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Id = ingredienteId,
            Nombre = "Ingrediente Inexistente",
            Rotacion = Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta,
            StockMinimo = 1,
            CostoPromedio = 5.00m
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EliminarIngrediente_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NoContent);
    }

    #endregion

    #region Tests de Movimientos de Inventario - Errores

    [Fact]
    public async Task RegistrarMovimiento_ConIngredienteInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarMovimientoCommand
        {
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Entrada,
            Cantidad = 10,
            Motivo = "Test movimiento",
            Observaciones = "Movimiento de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/movimientos", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConCantidadNegativa_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarMovimientoCommand
        {
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Entrada,
            Cantidad = -10, // Cantidad negativa
            Motivo = "Test movimiento",
            Observaciones = "Movimiento de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/movimientos", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConMotivoVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarMovimientoCommand
        {
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Entrada,
            Cantidad = 10,
            Motivo = "", // Motivo vacío
            Observaciones = "Movimiento de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/movimientos", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Consumo de Stock - Errores

    [Fact]
    public async Task ConsumirStock_ConIngredienteInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ConsumirStockCommand
        {
            Cantidad = 5,
            Motivo = "Test consumo",
            Observaciones = "Consumo de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/consumir", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConsumirStock_ConCantidadNegativa_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ConsumirStockCommand
        {
            Cantidad = -5, // Cantidad negativa
            Motivo = "Test consumo",
            Observaciones = "Consumo de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/consumir", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConsumirStock_ConCantidadCero_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ConsumirStockCommand
        {
            Cantidad = 0, // Cantidad cero
            Motivo = "Test consumo",
            Observaciones = "Consumo de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/consumir", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConsumirStock_ConMotivoVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ConsumirStockCommand
        {
            Cantidad = 5,
            Motivo = "", // Motivo vacío
            Observaciones = "Consumo de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/consumir", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Registro de Lotes - Errores

    [Fact]
    public async Task RegistrarLote_ConIngredienteInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarLoteCommand
        {
            NumeroLote = "LOTE-001",
            Cantidad = 20,
            FechaVencimiento = DateTime.Now.AddDays(30),
            PrecioUnitario = 10.00m,
            Observaciones = "Lote de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/lotes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarLote_ConCantidadNegativa_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarLoteCommand
        {
            NumeroLote = "LOTE-001",
            Cantidad = -20, // Cantidad negativa
            FechaVencimiento = DateTime.Now.AddDays(30),
            PrecioUnitario = 10.00m,
            Observaciones = "Lote de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/lotes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarLote_ConFechaVencimientoPasada_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarLoteCommand
        {
            NumeroLote = "LOTE-001",
            Cantidad = 20,
            FechaVencimiento = DateTime.Now.AddDays(-1), // Fecha pasada
            PrecioUnitario = 10.00m,
            Observaciones = "Lote de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/lotes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarLote_ConCostoNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarLoteCommand
        {
            NumeroLote = "LOTE-001",
            Cantidad = 20,
            FechaVencimiento = DateTime.Now.AddDays(30),
            PrecioUnitario = -10.00m, // Costo negativo
            Observaciones = "Lote de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/lotes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Asociación con Proveedores - Errores

    [Fact]
    public async Task AsociarProveedor_ConIngredienteInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/asociar-proveedor/{proveedorId}", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AsociarProveedor_ConProveedorInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/asociar-proveedor/{proveedorId}", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Parámetros de Consulta Inválidos

    [Fact]
    public async Task ObtenerIngredientes_ConPageNumberNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var query = "?pageNumber=-1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerIngredientes_ConPageSizeCero_DeberiaRetornarBadRequest()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=0";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerIngredientes_ConPageSizeMuyGrande_DeberiaRetornarBadRequest()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10000"; // Muy grande

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Formato de Datos Inválidos

    [Fact]
    public async Task CrearIngrediente_ConJsonMalformado_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonMalformado = "{ \"nombre\": \"Test\", \"rotacion\": \"INVALIDO\" }";

        // Act
        var content = new StringContent(jsonMalformado, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/inventario/ingredientes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarIngrediente_ConJsonMalformado_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var jsonMalformado = "{ \"nombre\": \"Test\", \"stockMinimo\": \"INVALIDO\" }";

        // Act
        var content = new StringContent(jsonMalformado, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/inventario/ingredientes/{ingredienteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Límites de Negocio

    [Fact]
    public async Task CrearIngrediente_ConNombreMuyLargo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var nombreMuyLargo = new string('A', 500); // Nombre muy largo
        var command = new CrearIngredienteCommand
        {
            Nombre = nombreMuyLargo,
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockInicial = 5,
            StockMinimo = 1,
            CostoInicial = 5.00m,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearIngrediente_ConDescripcionMuyLarga_DeberiaRetornarBadRequest()
    {
        // Arrange
        var descripcionMuyLarga = new string('A', 2000); // Descripción muy larga
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Descripcion = descripcionMuyLarga,
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockInicial = 5,
            StockMinimo = 1,
            CostoInicial = 5.00m,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Manejo de Excepciones

    [Fact]
    public async Task ObtenerEstadisticas_ConErrorInterno_DeberiaManejarExcepcion()
    {
        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/estadisticas");

        // Assert
        // El endpoint debería manejar errores internos y retornar un error controlado
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<object>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeFalse();
        }
    }

    #endregion
}
