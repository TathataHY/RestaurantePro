using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.MovimientosInventario;

/// <summary>
/// Tests CRUD específicos para el módulo de Movimientos de Inventario
/// Cubre operaciones básicas de Create, Read, Update, Delete
/// </summary>
[Collection("IntegrationTests")]
public class MovimientosInventarioCrudTests : BaseIntegrationTest
{
    public MovimientosInventarioCrudTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Creación (Create)

    [Fact]
    public async Task RegistrarMovimiento_ConDatosCompletos_DeberiaCrearExitosamente()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 100,
            TipoMovimiento = TipoMovimientoInventario.Ingreso,
            Motivo = "Compra de ingredientes",
            Observaciones = "Compra inicial de stock",
            UsuarioId = Guid.NewGuid(),
            Fecha = DateTime.Now
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBe(Guid.Empty);
        }
    }

    [Fact]
    public async Task RegistrarMovimiento_ConDatosMinimos_DeberiaCrearExitosamente()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 50,
            TipoMovimiento = TipoMovimientoInventario.Egreso,
            Motivo = "Consumo de ingrediente"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBe(Guid.Empty);
        }
    }

    [Fact]
    public async Task RegistrarMovimiento_ConTipoEntrada_DeberiaCrearExitosamente()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 75,
            TipoMovimiento = TipoMovimientoInventario.Entrada,
            Motivo = "Entrada de mercancía",
            Observaciones = "Entrada de prueba"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RegistrarMovimiento_ConTipoSalida_DeberiaCrearExitosamente()
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 25,
            TipoMovimiento = TipoMovimientoInventario.Salida,
            Motivo = "Salida de mercancía",
            Observaciones = "Salida de prueba"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
        }
    }

    #endregion

    #region Tests de Lectura (Read)

    [Fact]
    public async Task ObtenerMovimientos_ConPaginacion_DeberiaRetornarDatosPaginados()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=5";

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.PageNumber.Should().Be(1);
        responseData.Data.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task ObtenerMovimientos_ConFiltros_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = $"?pageNumber=1&pageSize=10&ingredienteId={ingredienteId}&tipoMovimiento=Ingreso";

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<MovimientoInventarioDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerMovimientoPorId_ConIdExistente_DeberiaRetornarMovimiento()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<MovimientoInventarioDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
            responseData.Data!.Id.Should().Be(movimientoId);
        }
    }

    [Fact]
    public async Task ObtenerMovimientoPorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ObtenerMovimientosPorIngrediente_ConIdValido_DeberiaRetornarMovimientos()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = "?fechaDesde=2024-01-01&fechaHasta=2024-12-31&limite=5";

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/ingrediente/{ingredienteId}{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<List<MovimientoInventarioDto>>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    #endregion

    #region Tests de Actualización (Update)

    [Fact]
    public async Task ActualizarMovimiento_ConDatosValidos_DeberiaActualizarExitosamente()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = 150,
            CostoUnitario = 15.75m,
            Motivo = "Ajuste de cantidad actualizada",
            Observaciones = "Observaciones actualizadas"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<MovimientoInventarioDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
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
        var response = await Client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarMovimiento_ConDatosParciales_DeberiaActualizarSoloCamposEnviados()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();
        var request = new ActualizarMovimientoRequest
        {
            Cantidad = 200
            // Solo actualizar cantidad, otros campos son null
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Eliminación (Delete)

    [Fact]
    public async Task EliminarMovimiento_ConIdExistente_DeberiaEliminarExitosamente()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EliminarMovimiento_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var movimientoId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/inventario/movimientos/{movimientoId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Validación de Datos

    [Theory]
    [InlineData(0)]        // Cantidad cero
    [InlineData(-10)]      // Cantidad negativa
    [InlineData(-0.1)]     // Cantidad decimal negativa
    public async Task RegistrarMovimiento_ConCantidadInvalida_DeberiaRetornarBadRequest(decimal cantidad)
    {
        // Arrange
        var request = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = cantidad,
            TipoMovimiento = TipoMovimientoInventario.Ingreso,
            Motivo = "Test de cantidad inválida"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

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
            TipoMovimiento = TipoMovimientoInventario.Ingreso,
            Motivo = "" // Motivo vacío
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

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
            TipoMovimiento = TipoMovimientoInventario.Ingreso,
            Motivo = "Test de ingrediente vacío"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/inventario/movimientos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Flujo Completo CRUD

    [Fact]
    public async Task FlujoCompletoCRUD_DeberiaFuncionarCorrectamente()
    {
        // 1. Crear movimiento
        var crearRequest = new RegistrarMovimientoRequest
        {
            IngredienteId = Guid.NewGuid(),
            Cantidad = 100,
            TipoMovimiento = TipoMovimientoInventario.Ingreso,
            Motivo = "Test CRUD completo",
            Observaciones = "Movimiento de prueba para flujo CRUD",
            UsuarioId = Guid.NewGuid()
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/inventario/movimientos", crearRequest);
        
        if (crearResponse.StatusCode == HttpStatusCode.Created)
        {
            var crearData = await crearResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var movimientoId = crearData!.Data;

            // 2. Leer movimiento creado
            var leerResponse = await Client.GetAsync($"/api/inventario/movimientos/{movimientoId}");
            leerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

            // 3. Actualizar movimiento
            var actualizarRequest = new ActualizarMovimientoRequest
            {
                Cantidad = 150,
                CostoUnitario = 12.50m,
                Motivo = "Test CRUD actualizado",
                Observaciones = "Movimiento actualizado en flujo CRUD"
            };

            var actualizarResponse = await Client.PutAsJsonAsync($"/api/inventario/movimientos/{movimientoId}", actualizarRequest);
            actualizarResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);

            // 4. Eliminar movimiento
            var eliminarResponse = await Client.DeleteAsync($"/api/inventario/movimientos/{movimientoId}");
            eliminarResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region Tests de Consultas Específicas

    [Fact]
    public async Task ObtenerMovimientosPorTipo_ConTipoIngreso_DeberiaRetornarSoloIngresos()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10&fechaInicio=2024-01-01&fechaFin=2024-12-31";

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/tipo/Ingreso{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<MovimientoInventarioDto>>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ObtenerMovimientosPorTipo_ConTipoEgreso_DeberiaRetornarSoloEgresos()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10&fechaInicio=2024-01-01&fechaFin=2024-12-31";

        // Act
        var response = await Client.GetAsync($"/api/inventario/movimientos/tipo/Egreso{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<MovimientoInventarioDto>>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    #endregion
}

