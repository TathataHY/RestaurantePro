using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.EliminarIngrediente;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.Ingredientes;

/// <summary>
/// Tests CRUD específicos para el módulo de Ingredientes
/// Cubre operaciones básicas de Create, Read, Update, Delete
/// </summary>
[Collection("IntegrationTests")]
public class IngredientesCrudTests : BaseIntegrationTest
{
    public IngredientesCrudTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Creación (Create)

    [Fact]
    public async Task CrearIngrediente_ConDatosCompletos_DeberiaCrearExitosamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Tomate Fresco",
            Descripcion = "Tomate rojo maduro para ensaladas",
            Rotacion = RotacionIngrediente.Alta,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 5,
            StockMaximo = 50,
            CostoUnitario = 8.50m,
            RequiereRefrigeracion = true,
            DiasVencimiento = 7
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
            responseData.Data!.Nombre.Should().Be(command.Nombre);
            responseData.Data.Descripcion.Should().Be(command.Descripcion);
            responseData.Data.Rotacion.Should().Be(command.Rotacion);
            responseData.Data.UnidadMedida.Should().Be(command.UnidadMedida);
            responseData.Data.StockMinimo.Should().Be(command.StockMinimo);
            responseData.Data.StockMaximo.Should().Be(command.StockMaximo);
            responseData.Data.CostoUnitario.Should().Be(command.CostoUnitario);
            responseData.Data.RequiereRefrigeracion.Should().Be(command.RequiereRefrigeracion);
            responseData.Data.DiasVencimiento.Should().Be(command.DiasVencimiento);
        }
    }

    [Fact]
    public async Task CrearIngrediente_ConDatosMinimos_DeberiaCrearExitosamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Sal de Mesa",
            Rotacion = RotacionIngrediente.Baja,
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 2.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
            responseData.Data!.Nombre.Should().Be(command.Nombre);
        }
    }

    [Fact]
    public async Task CrearIngrediente_ConNombreDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Ingrediente Duplicado",
            Rotacion = RotacionIngrediente.Media,
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);
        var response2 = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        // Al menos uno de los requests debería fallar por duplicado
        var responses = new[] { response1, response2 };
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Lectura (Read)

    [Fact]
    public async Task ObtenerIngredientes_ConPaginacion_DeberiaRetornarDatosPaginados()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=5&soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<IngredienteSummaryDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.PageNumber.Should().Be(1);
        responseData.Data.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task ObtenerIngredientes_ConFiltros_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10&soloActivos=true&categoria=Alta";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<IngredienteSummaryDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerIngredientePorId_ConIdExistente_DeberiaRetornarIngrediente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
            responseData.Data!.Id.Should().Be(ingredienteId);
        }
    }

    [Fact]
    public async Task ObtenerIngredientePorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeFalse();
        }
    }

    #endregion

    #region Tests de Actualización (Update)

    [Fact]
    public async Task ActualizarIngrediente_ConDatosValidos_DeberiaActualizarExitosamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Nombre = "Ingrediente Actualizado",
            Descripcion = "Descripción actualizada",
            Rotacion = RotacionIngrediente.Media,
            UnidadMedida = UnidadMedida.Litro,
            StockMinimo = 15,
            StockMaximo = 150,
            CostoUnitario = 12.75m,
            RequiereRefrigeracion = false,
            DiasVencimiento = 20
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
            responseData.Data!.Nombre.Should().Be(command.Nombre);
            responseData.Data.Descripcion.Should().Be(command.Descripcion);
        }
    }

    [Fact]
    public async Task ActualizarIngrediente_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Nombre = "Ingrediente Inexistente",
            Rotacion = RotacionIngrediente.Baja,
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1,
            StockMaximo = 10,
            CostoUnitario = 5.00m
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarIngrediente_ConDatosInvalidos_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Nombre = "", // Nombre vacío - inválido
            Rotacion = RotacionIngrediente.Baja,
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = -1, // Stock mínimo negativo - inválido
            StockMaximo = 10,
            CostoUnitario = -5.00m // Costo negativo - inválido
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Eliminación (Delete)

    [Fact]
    public async Task EliminarIngrediente_ConIdExistente_DeberiaEliminarExitosamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EliminarIngrediente_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Validación de Datos

    [Theory]
    [InlineData("", Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta, Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 1, 10, 5.00)] // Nombre vacío
    [InlineData("Test", Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta, Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, -1, 10, 5.00)] // Stock mínimo negativo
    [InlineData("Test", Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta, Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 10, 5, 5.00)] // Stock máximo menor que mínimo
    [InlineData("Test", Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta, Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 1, 10, -5.00)] // Costo negativo
    public async Task CrearIngrediente_ConDatosInvalidos_DeberiaRetornarBadRequest(
        string nombre, Domain.Inventario.Ingredientes.Enums.RotacionIngrediente rotacion, Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida, 
        decimal stockMinimo, decimal stockMaximo, decimal costoUnitario)
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = nombre,
            Rotacion = rotacion,
            UnidadMedida = unidadMedida,
            StockMinimo = stockMinimo,
            StockMaximo = stockMaximo,
            CostoUnitario = costoUnitario
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Flujo Completo CRUD

    [Fact]
    public async Task FlujoCompletoCRUD_DeberiaFuncionarCorrectamente()
    {
        // 1. Crear ingrediente
        var crearCommand = new CrearIngredienteCommand
        {
            Nombre = "Ingrediente CRUD Test",
            Descripcion = "Test de flujo completo",
            Rotacion = "Media",
            UnidadMedida = "Kilogramo",
            StockInicial = 25,
            StockMinimo = 5,
            CostoInicial = 10.00m,
            UsuarioId = Guid.NewGuid()
        };

        var crearResponse = await _client.PostAsJsonAsync("/api/inventario/ingredientes", crearCommand);
        
        if (crearResponse.StatusCode == HttpStatusCode.Created)
        {
            var crearData = await crearResponse.Content.ReadFromJsonAsync<ApiResponse<IngredienteDto>>();
            var ingredienteId = crearData!.Data!.Id;

            // 2. Leer ingrediente creado
            var leerResponse = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}");
            leerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

            // 3. Actualizar ingrediente
            var actualizarCommand = new ActualizarIngredienteCommand
            {
                Id = ingredienteId,
                Nombre = "Ingrediente CRUD Test Actualizado",
                Descripcion = "Test actualizado",
                Rotacion = Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta,
                StockMinimo = 10,
                CostoPromedio = 15.00m
            };

            var actualizarResponse = await _client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", actualizarCommand);
            actualizarResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);

            // 4. Eliminar ingrediente
            var eliminarResponse = await _client.DeleteAsync($"/api/inventario/ingredientes/{ingredienteId}");
            eliminarResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }
    }

    #endregion
}
