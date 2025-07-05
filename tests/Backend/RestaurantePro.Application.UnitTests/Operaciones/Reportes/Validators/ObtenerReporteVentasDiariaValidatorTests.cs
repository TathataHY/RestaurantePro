namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Validators;

public class ObtenerReporteVentasDiariaValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ObtenerReporteVentasDiariaValidator _validator;
    private readonly Mock<DbSet<Mesa>> _mesasMock;
    private readonly Mock<DbSet<Usuario>> _usuariosMock;
    private readonly Mock<DbSet<Comanda>> _comandasMock;

    public ObtenerReporteVentasDiariaValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mesasMock = new Mock<DbSet<Mesa>>();
        _usuariosMock = new Mock<DbSet<Usuario>>();
        _comandasMock = new Mock<DbSet<Comanda>>();

        _contextMock.Setup(c => c.Mesas).Returns(_mesasMock.Object);
        _contextMock.Setup(c => c.Usuarios).Returns(_usuariosMock.Object);
        _contextMock.Setup(c => c.Comandas).Returns(_comandasMock.Object);

        _validator = new ObtenerReporteVentasDiariaValidator(_contextMock.Object);
    }

    [Fact]
    public async Task Validate_ConQueryValida_DeberiaSerValido()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Basico
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConFechaReporteFutura_DeberiaRetornarError()
    {
        // Arrange
        var fechaFutura = DateTime.Today.AddDays(1);
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = fechaFutura,
            NivelDetalle = NivelDetalle.Basico
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.FechaReporte) &&
            e.ErrorMessage.Contains("La fecha del reporte no puede ser futura"));
    }
} 