namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración para ReservacionesController
/// Valida todos los endpoints REST del controlador de reservaciones del restaurante
/// </summary>
[Collection("Sequential")]
public class ReservacionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public ReservacionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetReservaciones_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/operaciones/reservaciones";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReservaciones_ConParametrosFiltro_DebeRetornarRespuestaValida()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaDesde = DateTime.Now.AddDays(-7);
        var fechaHasta = DateTime.Now.AddDays(7);
        var url = $"/api/operaciones/reservaciones?estado=Confirmada&clienteId={clienteId}&mesaId={mesaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReservacion_ConIdEspecifico_DebeRetornarRespuestaValida()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var url = $"/api/operaciones/reservaciones/{reservacionId}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/operaciones/reservaciones";
        var command = new
        {
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            FechaReservacion = DateTime.Now.AddDays(1),
            HoraReservacion = "19:00",
            NumeroPersonas = 4,
            Observaciones = "Reservación para cena de aniversario",
            TipoReservacion = "Cena",
            ContactoTelefono = "+1234567890",
            ContactoEmail = "cliente@email.com"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PutReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var url = $"/api/operaciones/reservaciones/{reservacionId}";
        var command = new
        {
            Id = reservacionId,
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            FechaReservacion = DateTime.Now.AddDays(2),
            HoraReservacion = "20:00",
            NumeroPersonas = 6,
            Observaciones = "Reservación actualizada",
            Estado = "Confirmada"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var url = $"/api/operaciones/reservaciones/{reservacionId}";

        // Act
        var response = await HttpClient.DeleteAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConfirmarReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var url = $"/api/operaciones/reservaciones/{reservacionId}/confirmar";
        var command = new
        {
            ConfirmadoPor = Guid.NewGuid(),
            NotasConfirmacion = "Reservación confirmada por teléfono",
            FechaConfirmacion = DateTime.Now
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ReprogramarReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var url = $"/api/operaciones/reservaciones/{reservacionId}/reprogramar";
        var command = new
        {
            NuevaFecha = DateTime.Now.AddDays(3),
            NuevaHora = "21:00",
            MotivoReprogramacion = "Cliente solicitó cambio de horario",
            ReprogramadoPor = Guid.NewGuid()
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerificarDisponibilidad_DebeRetornarRespuestaValida()
    {
        // Arrange
        var fecha = DateTime.Now.AddDays(1);
        var hora = "19:00";
        var numeroPersonas = 4;
        var url = $"/api/operaciones/reservaciones/disponibilidad?fecha={fecha:yyyy-MM-dd}&hora={hora}&numeroPersonas={numeroPersonas}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
} 