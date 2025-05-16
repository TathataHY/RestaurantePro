
namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Entities
{
    public class HistorialPuntosTests
    {
        [Fact]
        public void CrearHistorialPuntosAgregados_DatosValidos_DebeCrearHistorial()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var puntos = 100;
            var concepto = "Compra restaurante";
            var tipoOperacion = TipoOperacionPuntos.Agregados;

            // Act
            var historial = HistorialPuntos.CrearRegistroAgregados(tarjetaId, puntos, concepto);

            // Assert
            historial.Should().NotBeNull();
            historial.TarjetaFidelizacionId.Should().Be(tarjetaId);
            historial.Puntos.Should().Be(puntos);
            historial.Concepto.Should().Be(concepto);
            historial.TipoOperacion.Should().Be(tipoOperacion);
            historial.FechaOperacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
            historial.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void CrearHistorialPuntosPorCompra_MontoValido_DebeCalcularPuntos()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var monto = 1000m;
            var factorConversion = 10; // $10 = 1 punto
            var puntosEsperados = 100;
            var concepto = "Compra restaurante $1000";

            // Act
            var historial = HistorialPuntos.CrearRegistroPorCompra(tarjetaId, monto, factorConversion, concepto);

            // Assert
            historial.Should().NotBeNull();
            historial.TarjetaFidelizacionId.Should().Be(tarjetaId);
            historial.Puntos.Should().Be(puntosEsperados);
            historial.Concepto.Should().Be(concepto);
            historial.TipoOperacion.Should().Be(TipoOperacionPuntos.Agregados);
            historial.MontoCompra.Should().Be(monto);
            historial.FactorConversion.Should().Be(factorConversion);
        }

        [Fact]
        public void CrearHistorialPuntosCanjeados_DatosValidos_DebeCrearHistorial()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var puntos = 50;
            var concepto = "Descuento en comida";
            var tipoOperacion = TipoOperacionPuntos.Canjeados;

            // Act
            var historial = HistorialPuntos.CrearRegistroCanjeados(tarjetaId, puntos, concepto);

            // Assert
            historial.Should().NotBeNull();
            historial.TarjetaFidelizacionId.Should().Be(tarjetaId);
            historial.Puntos.Should().Be(puntos);
            historial.Concepto.Should().Be(concepto);
            historial.TipoOperacion.Should().Be(tipoOperacion);
            historial.FechaOperacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void CrearHistorialPuntosVencidos_DatosValidos_DebeCrearHistorial()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var puntos = 25;
            var concepto = "Vencimiento anual de puntos";
            var tipoOperacion = TipoOperacionPuntos.Vencidos;

            // Act
            var historial = HistorialPuntos.CrearRegistroVencidos(tarjetaId, puntos, concepto);

            // Assert
            historial.Should().NotBeNull();
            historial.TarjetaFidelizacionId.Should().Be(tarjetaId);
            historial.Puntos.Should().Be(puntos);
            historial.Concepto.Should().Be(concepto);
            historial.TipoOperacion.Should().Be(tipoOperacion);
        }

        [Fact]
        public void CrearHistorialPuntosAjuste_DatosValidos_DebeCrearHistorial()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var puntos = 20;
            var concepto = "Ajuste administrativo";
            var tipoOperacion = TipoOperacionPuntos.Ajuste;

            // Act
            var historial = HistorialPuntos.CrearRegistroAjuste(tarjetaId, puntos, concepto);

            // Assert
            historial.Should().NotBeNull();
            historial.TarjetaFidelizacionId.Should().Be(tarjetaId);
            historial.Puntos.Should().Be(puntos);
            historial.Concepto.Should().Be(concepto);
            historial.TipoOperacion.Should().Be(tipoOperacion);
        }

        [Fact]
        public void CrearHistorialPuntos_PuntosNegativos_DebeLanzarArgumentException()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var puntos = -10;
            var concepto = "Puntos inválidos";

            // Act
            Action action = () => HistorialPuntos.CrearRegistroAgregados(tarjetaId, puntos, concepto);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*puntos*");
        }

        [Fact]
        public void CrearHistorialPuntos_ConceptoVacio_DebeLanzarArgumentException()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var puntos = 50;
            var concepto = string.Empty;

            // Act
            Action action = () => HistorialPuntos.CrearRegistroAgregados(tarjetaId, puntos, concepto);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*concepto*");
        }

        [Fact]
        public void CrearHistorialPorCompra_MontoNegativo_DebeLanzarArgumentException()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var monto = -100m;
            var factorConversion = 10;
            var concepto = "Monto inválido";

            // Act
            Action action = () => HistorialPuntos.CrearRegistroPorCompra(tarjetaId, monto, factorConversion, concepto);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*monto*");
        }

        [Fact]
        public void CrearHistorialPorCompra_FactorConversionCero_DebeLanzarArgumentException()
        {
            // Arrange
            var tarjetaId = Guid.NewGuid();
            var monto = 100m;
            var factorConversion = 0;
            var concepto = "Factor inválido";

            // Act
            Action action = () => HistorialPuntos.CrearRegistroPorCompra(tarjetaId, monto, factorConversion, concepto);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*factor*");
        }
    }
}
