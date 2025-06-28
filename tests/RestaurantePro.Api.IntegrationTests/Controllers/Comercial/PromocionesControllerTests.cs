using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;
using RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.EliminarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.AsignarProductos;
using RestaurantePro.Application.Comercial.Promociones.Commands.QuitarProductos;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Core.Productos;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para PromocionesController
/// Valida todos los endpoints REST del controlador de gestión de promociones
/// </summary>
[Collection("Sequential")]
public class PromocionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly PromocionTestDataBuilder _promocionBuilder;

    public PromocionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
        _promocionBuilder = new PromocionTestDataBuilder(DbContext, DateTimeService);
    }

    [Fact]
    public async Task ObtenerTodasLasPromociones_DeberiaRetornarListaVacia()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/promociones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<object>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerPromocionPorId_ConPromocionExistente_DeberiaRetornarPromocion()
    {
        // Arrange
        var promocion = await _promocionBuilder.CrearPromocionPorcentajeAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/promociones/{promocion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerPromocionPorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/promociones/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearPromocion_ConDatosValidos_DeberiaCrearPromocion()
    {
        // Arrange
        var request = new CrearPromocionCommand
        {
            Codigo = $"PROMO{Guid.NewGuid():N}".ToUpperInvariant().Substring(0, 20).Replace("-", "_"),
            Nombre = "Promocion Test",
            Descripcion = "Promocion de prueba",
            Tipo = TipoPromocion.PorcentajeTotal,
            ValorDescuento = 15.0m,
            FechaInicio = DateTime.UtcNow.AddDays(1).AddHours(1), // Mañana a las 1 AM UTC
            FechaFin = DateTime.UtcNow.AddDays(30).AddHours(1),   // 30 días después a las 1 AM UTC
            MaximoUsos = 100,
            EsAcumulable = false
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/promociones", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.Codigo.Should().Be(request.Codigo);
        responseContent.Data.Nombre.Should().Be(request.Nombre);

        // Verificar que se guardó en la base de datos
        var promocionGuardada = await DbContext.Promociones.FirstOrDefaultAsync(p => p.Codigo == request.Codigo);
        promocionGuardada.Should().NotBeNull();
        promocionGuardada!.Nombre.Should().Be(request.Nombre);
    }

    [Fact]
    public async Task CrearPromocion_ConCodigoDuplicado_DeberiaRetornarBadRequest()
    {
        // Arrange
        var codigo = $"PROMO{Guid.NewGuid():N}".ToUpperInvariant().Substring(0, 20).Replace("-", "_");
        await _promocionBuilder.ConCodigo(codigo).BuildAsync();

        var request = new CrearPromocionCommand
        {
            Codigo = codigo,
            Nombre = "Promocion Duplicada",
            Descripcion = "Promocion con codigo duplicado",
            Tipo = TipoPromocion.PorcentajeTotal,
            ValorDescuento = 10.0m,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(30),
            MaximoUsos = 50,
            EsAcumulable = false
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/promociones", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarPromocion_ConDatosValidos_DeberiaActualizarPromocion()
    {
        // Arrange
        var promocion = await _promocionBuilder.CrearPromocionMontoFijoAsync();
        
        var command = new ActualizarPromocionCommand
        {
            Nombre = "Promoción Actualizada",
            Descripcion = "Descripción actualizada",
            Tipo = TipoPromocion.MontoFijoTotal,
            ValorDescuento = 5000.0m,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(60),
            MontoMinimo = 10000.0m,
            PuntosRequeridos = 50,
            MaximoUsos = 200,
            EsAcumulable = true
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/promociones/{promocion.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.Nombre.Should().Be("Promoción Actualizada");
        responseContent.Data.Tipo.Should().Be(TipoPromocion.MontoFijoTotal);
        responseContent.Data.ValorDescuento.Should().Be(5000.0m);

        // Verificar que se guardó en la base de datos - recargar desde BD
        await DbContext.Entry(promocion).ReloadAsync();
        promocion.Nombre.Should().Be("Promoción Actualizada");
        promocion.Tipo.Should().Be(TipoPromocion.MontoFijoTotal);
        promocion.ValorDescuento.Should().Be(5000.0m);
    }

    [Fact]
    public async Task EliminarPromocion_ConPromocionExistente_DeberiaEliminarPromocion()
    {
        // Arrange
        var promocion = await _promocionBuilder.CrearPromocionPorcentajeAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/promociones/{promocion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().BeTrue();

        // Verificar que se canceló en la base de datos (soft delete)
        await DbContext.Entry(promocion).ReloadAsync();
        promocion.Estado.Should().Be(EstadoPromocion.Cancelada);
    }

    [Fact]
    public async Task ActivarPromocion_ConPromocionPausada_DeberiaActivarPromocion()
    {
        // Arrange
        var promocion = await _promocionBuilder
            .ConEstado(EstadoPromocion.Pausada)
            .ConFechasVigencia(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(30)) // Fecha de inicio = ayer
            .ConCodigo($"ACTIVAR{Guid.NewGuid():N}".Substring(0, 20).ToUpperInvariant())
            .BuildAsync(); // Usar BuildAsync directamente para respetar las fechas

        // Act
        var response = await HttpClient.PatchAsync($"/api/comercial/promociones/{promocion.Id}/activar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data.Estado.Should().Be(EstadoPromocion.Activa);

        // Verificar en la base de datos (nuevo contexto)
        var promocionBD = await GetPromocionFromDatabase(promocion.Id);
        promocionBD.Should().NotBeNull();
        promocionBD!.Estado.Should().Be(EstadoPromocion.Activa);
    }

    [Fact]
    public async Task PausarPromocion_ConPromocionActiva_DeberiaPausarPromocion()
    {
        // Arrange
        var promocion = await _promocionBuilder
            .ConEstado(EstadoPromocion.Activa)
            .ConFechasVigencia(DateTime.Today, DateTime.Today.AddDays(30))
            .ConCodigo($"PAUSAR{Guid.NewGuid():N}".Substring(0, 20).ToUpperInvariant())
            .BuildAsync(); // Usar BuildAsync directamente para respetar el estado

        // Verificar que la promoción esté activa antes de pausar
        var promocionInicial = await GetPromocionFromDatabase(promocion.Id);
        promocionInicial.Should().NotBeNull();
        promocionInicial!.Estado.Should().Be(EstadoPromocion.Activa);

        // Act
        var response = await HttpClient.PatchAsync($"/api/comercial/promociones/{promocion.Id}/pausar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.Estado.Should().Be(EstadoPromocion.Pausada);

        // Verificar que se guardó en la base de datos (nuevo contexto)
        var promocionBD = await GetPromocionFromDatabase(promocion.Id);
        promocionBD.Should().NotBeNull();
        promocionBD!.Estado.Should().Be(EstadoPromocion.Pausada);
    }

    [Fact]
    public async Task AsignarProductos_ConProductosValidos_DeberiaAsignarProductos()
    {
        // Arrange
        var promocion = await _promocionBuilder.CrearPromocionPorcentajeAsync();
        var producto = await CrearProductoPrueba("Producto Test", 100.00m);
        var productosIds = new List<Guid> { producto.Id };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/promociones/{promocion.Id}/productos", productosIds);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.ProductosAplicablesIds.Should().Contain(producto.Id);

        // Verificar en la base de datos (nuevo contexto)
        var promocionBD = await GetPromocionFromDatabase(promocion.Id);
        promocionBD.Should().NotBeNull();
        promocionBD!.ProductosAplicablesIds.Should().Contain(producto.Id);
    }

    [Fact]
    public async Task QuitarProductos_ConProductosAsignados_DeberiaQuitarProductos()
    {
        // Arrange
        var promocion = await _promocionBuilder.CrearPromocionPorcentajeAsync();
        var producto1 = await CrearProductoPrueba("Producto 1", 100.00m);
        var producto2 = await CrearProductoPrueba("Producto 2", 150.00m);
        var productosIds = new List<Guid> { producto1.Id, producto2.Id };

        // Asignar productos usando el endpoint
        var asignarResponse = await HttpClient.PostAsJsonAsync($"/api/comercial/promociones/{promocion.Id}/productos", productosIds);
        asignarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Quitar producto1 usando el endpoint
        var quitarResponse = await HttpClient.DeleteAsJsonAsync($"/api/comercial/promociones/{promocion.Id}/productos", new List<Guid> { producto1.Id });
        quitarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await quitarResponse.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.ProductosAplicablesIds.Should().NotContain(producto1.Id);
        responseContent.Data.ProductosAplicablesIds.Should().Contain(producto2.Id);

        // Verificar en la base de datos (nuevo contexto)
        var promocionBD = await GetPromocionFromDatabase(promocion.Id);
        promocionBD.Should().NotBeNull();
        promocionBD!.ProductosAplicablesIds.Should().NotContain(producto1.Id);
        promocionBD.ProductosAplicablesIds.Should().Contain(producto2.Id);
    }

    [Fact]
    public async Task VerificarAplicabilidad_ConPromocionValida_DeberiaRetornarAplicable()
    {
        // Arrange
        var cliente = await CrearClientePrueba();
        var producto = await CrearProductoPrueba("Producto Test", 100.00m);
        var promocion = await _promocionBuilder
            .CrearPromocionParaProductosAsync(new List<Guid> { producto.Id });

        var request = new AplicarPromocionCommand
        {
            PromocionId = promocion.Id,
            ClienteId = cliente.Id,
            ProductosIds = new List<Guid> { producto.Id },
            TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos
        };

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/promociones/aplicables?clienteId={cliente.Id}&monto=100.0m");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task TestMediatR_DeberiaFuncionarConHandlerExistente()
    {
        // Arrange - Usar un handler que sabemos que funciona
        var promocion = await _promocionBuilder
            .ConEstado(EstadoPromocion.Activa)
            .ConFechasVigencia(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(30))
            .CrearPromocionPorcentajeAsync();

        // Act - Intentar obtener la promoción (esto usa un Query que debería funcionar)
        var response = await HttpClient.GetAsync($"/api/comercial/promociones/{promocion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadFromJsonAsync<ApiResponse<PromocionDto>>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Data.Should().NotBeNull();
        responseContent.Data.Id.Should().Be(promocion.Id);
    }

    // Métodos helper
    private async Task CreatePromocionInDatabase(Promocion promocion)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        context.Promociones.Add(promocion);
        await context.SaveChangesAsync();
    }

    private async Task<Promocion?> GetPromocionFromDatabase(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        return await context.Promociones.FindAsync(id);
    }

    public override void Dispose()
    {
        _factory.Dispose();
        base.Dispose();
    }
} 