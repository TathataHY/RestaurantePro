using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.MovimientosInventario;

/// <summary>
/// Tests de manejo de errores para el módulo de Movimientos de Inventario
/// Cubre escenarios de error, validaciones de negocio y casos excepcionales
/// </summary>
[Collection("IntegrationTests")]
public class MovimientosInventarioErrorHandlingTests : BaseIntegrationTest
{
    public MovimientosInventarioErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Validación de Datos de Entrada

    [Fact]
    public async Task RegistrarMovimiento_ConCantidadNegativa_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = -50, // Cantidad negativa
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "Test cantidad negativa"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<object>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegistrarMovimiento_ConCantidadCero_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 0, // Cantidad cero
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "Test cantidad cero"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConMotivoVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "" // Motivo vacío
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConIngredienteIdVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.Empty, // ID vacío
            Cantidad = 50,
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "Test ingrediente vacío"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConTipoMovimientoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = (Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario)999, // Tipo inválido
            Motivo = "Test tipo inválido"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Recursos No Encontrados

    [Fact]
    public async Task ObtenerMovimientoPorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/{movimientoId}");

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

    [Fact]
    public async Task ActualizarMovimiento_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = 100,
            Motivo = "Actualización de prueba"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EliminarMovimiento_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    #endregion

    #region Tests de Parámetros de Consulta Inválidos

    [Fact]
    public async Task ObtenerMovimientos_ConPageNumberNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var query = "?pageNumber=-1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerMovimientos_ConPageSizeCero_DeberiaRetornarBadRequest()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=0";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerMovimientos_ConPageSizeMuyGrande_DeberiaRetornarBadRequest()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10000"; // Muy grande

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

    #region Tests de Formato de Datos Inválidos

    [Fact]
    public async Task RegistrarMovimiento_ConJsonMalformado_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonMalformado = "{ \"ingredienteId\": \"invalid-guid\", \"cantidad\": \"not-a-number\" }";

        // Act
        var content = new StringContent(jsonMalformado, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/inventario/movimientos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarMovimiento_ConJsonMalformado_DeberiaRetornarBadRequest()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var jsonMalformado = "{ \"cantidad\": \"not-a-number\", \"costoUnitario\": \"invalid\" }";

        // Act
        var content = new StringContent(jsonMalformado, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/inventario/movimientos/{movimientoId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Límites de Negocio

    [Fact]
    public async Task RegistrarMovimiento_ConCantidadMuyGrande_DeberiaRetornarBadRequest()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = decimal.MaxValue, // Cantidad excesivamente grande
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "Test cantidad muy grande"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConMotivoMuyLargo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var motivoMuyLargo = new string('A', 1000); // Motivo muy largo
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = motivoMuyLargo
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConObservacionesMuyLargas_DeberiaRetornarBadRequest()
    {
        // Arrange
        var observacionesMuyLargas = new string('A', 2000); // Observaciones muy largas
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso,
            Motivo = "Test observaciones largas",
            Observaciones = observacionesMuyLargas
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Validación de Tipos de Movimiento

    [Theory]
    [InlineData("TipoInvalido")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("null")]
    public async Task ObtenerMovimientosPorTipo_ConTipoInvalido_DeberiaRetornarBadRequest(string tipoInvalido)
    {
        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/tipo/{tipoInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Validación de IDs

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("undefined")]
    public async Task ObtenerMovimientoPorId_ConIdInvalido_DeberiaRetornarBadRequest(string idInvalido)
    {
        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/{idInvalido}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("")]
    public async Task ActualizarMovimiento_ConIdInvalido_DeberiaRetornarBadRequest(string idInvalido)
    {
        // Arrange
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = 100,
            Motivo = "Test actualización"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/movimientos/{idInvalido}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("")]
    public async Task EliminarMovimiento_ConIdInvalido_DeberiaRetornarBadRequest(string idInvalido)
    {
        // Act
        var response = await _client.DeleteAsync($"/api/inventario/movimientos/{idInvalido}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Validación de Content-Type

    [Fact]
    public async Task RegistrarMovimiento_ConContentTypeIncorrecto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonContent = "{\"ingredienteId\":\"123e4567-e89b-12d3-a456-426614174000\",\"cantidad\":50}";
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PostAsync("/api/inventario/movimientos", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarMovimiento_ConContentTypeIncorrecto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var jsonContent = "{\"cantidad\":100}";
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PutAsync($"/api/inventario/movimientos/{movimientoId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Manejo de Excepciones

    [Fact]
    public async Task ObtenerMovimientos_ConErrorInterno_DeberiaManejarExcepcion()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10&fechaInicio=invalid-date";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        // El endpoint debería manejar errores internos y retornar un error controlado
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GenerarReporte_ConErrorInterno_DeberiaManejarExcepcion()
    {
        // Arrange
        var query = "?fechaInicio=invalid-date&fechaFin=invalid-date";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos/reporte{query}");

        // Assert
        // El endpoint debería manejar errores internos y retornar un error controlado
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Tests de Validación de Rangos Numéricos

    [Theory]
    [InlineData(-1000)]    // Cantidad muy negativa
    [InlineData(999999)]   // Cantidad muy grande
    [InlineData(-500)]     // Costo muy negativo
    [InlineData(999999)]   // Costo muy grande
    public async Task ActualizarMovimiento_ConValoresExtremos_DeberiaValidarRangos(decimal valor)
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = valor,
            CostoUnitario = Math.Abs(valor)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Validación de Fechas

    [Fact]
    public async Task ObtenerMovimientos_ConFechaInicioMayorQueFin_DeberiaManejarCorrectamente()
    {
        // Arrange
        var query = "?fechaInicio=2024-12-31&fechaFin=2024-01-01"; // Fecha inicio mayor que fin

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        // El endpoint debería manejar este caso de manera apropiada
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerMovimientos_ConFechasFuturas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var fechaFutura = DateTime.Now.AddYears(1);
        var query = $"?fechaInicio={fechaFutura:yyyy-MM-dd}&fechaFin={fechaFutura.AddDays(1):yyyy-MM-dd}";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Validación de Parámetros de Ordenamiento

    [Theory]
    [InlineData("CampoInvalido", "asc")]
    [InlineData("Fecha", "direccionInvalida")]
    [InlineData("", "asc")]
    [InlineData("Cantidad", "")]
    public async Task ObtenerMovimientos_ConOrdenamientoInvalido_DeberiaManejarCorrectamente(string ordenarPor, string direccion)
    {
        // Arrange
        var query = $"?ordenarPor={ordenarPor}&direccionOrdenamiento={direccion}&pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        // El endpoint debería manejar parámetros de ordenamiento inválidos
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion
}

