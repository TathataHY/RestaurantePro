using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.DTOs;
using System.ComponentModel;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Models;

/// <summary>
/// Pruebas unitarias para MesaDto
/// </summary>
public class MesaDtoTests
{
    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var mesa = new MesaDto();

        // Assert
        mesa.Id.Should().Be(Guid.Empty);
        mesa.Numero.Should().BeEmpty();
        mesa.Capacidad.Should().Be(0);
        mesa.Estado.Should().BeEmpty();
        mesa.ClienteId.Should().BeNull();
        mesa.NombreCliente.Should().BeEmpty();
        mesa.Zona.Should().BeEmpty();
        mesa.Ubicacion.Should().BeEmpty();
        mesa.Tipo.Should().BeEmpty();
        mesa.Observaciones.Should().BeEmpty();
        mesa.UltimaActualizacion.Should().Be(default(DateTime));
    }

    #endregion

    #region Propiedades Básicas

    [Theory]
    [InlineData("Mesa 1")]
    [InlineData("Mesa 2")]
    [InlineData("")]
    [InlineData("Mesa con espacios   ")]
    public void Numero_DeberiaEstablecerCorrectamente(string numero)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Numero = numero;

        // Assert
        mesa.Numero.Should().Be(numero);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Capacidad_DeberiaEstablecerCorrectamente(int capacidad)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Capacidad = capacidad;

        // Assert
        mesa.Capacidad.Should().Be(capacidad);
    }

    [Theory]
    [InlineData("Disponible")]
    [InlineData("Ocupada")]
    [InlineData("Reservada")]
    [InlineData("Fuera de Servicio")]
    [InlineData("")]
    public void Estado_DeberiaEstablecerCorrectamente(string estado)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.Estado.Should().Be(estado);
    }

    #endregion

    #region Propiedades de Estado

    [Theory]
    [InlineData("Disponible", true, false, false, false)]
    [InlineData("disponible", true, false, false, false)]
    [InlineData("DISPONIBLE", true, false, false, false)]
    [InlineData("Ocupada", false, true, false, false)]
    [InlineData("ocupada", false, true, false, false)]
    [InlineData("OCUPADA", false, true, false, false)]
    [InlineData("Reservada", false, false, true, false)]
    [InlineData("reservada", false, false, true, false)]
    [InlineData("RESERVADA", false, false, true, false)]
    [InlineData("Fuera de Servicio", false, false, false, true)]
    [InlineData("fueradeservicio", false, false, false, true)]
    [InlineData("FUERA DE SERVICIO", false, false, false, true)]
    [InlineData("Estado Desconocido", false, false, false, false)]
    [InlineData("", false, false, false, false)]
    [InlineData("   ", false, false, false, false)]
    public void PropiedadesEstado_DeberiaEvaluarCorrectamente(string estado, bool disponible, bool ocupada, bool reservada, bool fueraDeServicio)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.Disponible.Should().Be(disponible);
        mesa.Ocupada.Should().Be(ocupada);
        mesa.Reservada.Should().Be(reservada);
        mesa.FueraDeServicio.Should().Be(fueraDeServicio);
    }

    #endregion

    #region EstadoDescripcion

    [Theory]
    [InlineData("Disponible", "Disponible")]
    [InlineData("disponible", "Disponible")]
    [InlineData("DISPONIBLE", "Disponible")]
    [InlineData("Ocupada", "Ocupada")]
    [InlineData("ocupada", "Ocupada")]
    [InlineData("OCUPADA", "Ocupada")]
    [InlineData("Reservada", "Reservada")]
    [InlineData("reservada", "Reservada")]
    [InlineData("RESERVADA", "Reservada")]
    [InlineData("Fuera de Servicio", "Fuera de Servicio")]
    [InlineData("fueradeservicio", "Fuera de Servicio")]
    [InlineData("FUERA DE SERVICIO", "Fuera de Servicio")]
    [InlineData("Estado Desconocido", "Estado Desconocido")]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void EstadoDescripcion_DeberiaRetornarDescripcionCorrecta(string estado, string descripcionEsperada)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.EstadoDescripcion.Should().Be(descripcionEsperada);
    }

    #endregion

    #region EstadoColor

    [Theory]
    [InlineData("Disponible", "#4CAF50")]
    [InlineData("disponible", "#4CAF50")]
    [InlineData("DISPONIBLE", "#4CAF50")]
    [InlineData("Ocupada", "#F44336")]
    [InlineData("ocupada", "#F44336")]
    [InlineData("OCUPADA", "#F44336")]
    [InlineData("Reservada", "#FF9800")]
    [InlineData("reservada", "#FF9800")]
    [InlineData("RESERVADA", "#FF9800")]
    [InlineData("Fuera de Servicio", "#9E9E9E")]
    [InlineData("fueradeservicio", "#9E9E9E")]
    [InlineData("FUERA DE SERVICIO", "#9E9E9E")]
    [InlineData("Estado Desconocido", "#607D8B")]
    [InlineData("", "#607D8B")]
    [InlineData("   ", "#607D8B")]
    public void EstadoColor_DeberiaRetornarColorCorrecto(string estado, string colorEsperado)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.EstadoColor.Should().NotBeNull();
        mesa.EstadoColor.ToArgbHex().Should().Be(colorEsperado);
    }

    #endregion

    #region INotifyPropertyChanged

    [Fact]
    public void Estado_AlCambiar_DeberiaDispararPropertyChanged()
    {
        // Arrange
        var mesa = new MesaDto();
        var eventosDisparados = new List<string>();
        mesa.PropertyChanged += (sender, e) => eventosDisparados.Add(e.PropertyName!);

        // Act
        mesa.Estado = "Disponible";

        // Assert
        eventosDisparados.Should().Contain("Estado");
        eventosDisparados.Should().Contain("EstadoDescripcion");
        eventosDisparados.Should().Contain("EstadoColor");
        eventosDisparados.Should().Contain("Disponible");
        eventosDisparados.Should().Contain("Ocupada");
        eventosDisparados.Should().Contain("Reservada");
        eventosDisparados.Should().Contain("FueraDeServicio");
    }

    [Fact]
    public void Estado_AlCambiarAlMismoValor_NoDeberiaDispararPropertyChanged()
    {
        // Arrange
        var mesa = new MesaDto();
        mesa.Estado = "Disponible";
        var eventosDisparados = new List<string>();
        mesa.PropertyChanged += (sender, e) => eventosDisparados.Add(e.PropertyName!);

        // Act
        mesa.Estado = "Disponible";

        // Assert
        eventosDisparados.Should().BeEmpty();
    }

    [Fact]
    public void Estado_AlCambiarDeNullAValor_DeberiaDispararPropertyChanged()
    {
        // Arrange
        var mesa = new MesaDto();
        var eventosDisparados = new List<string>();
        mesa.PropertyChanged += (sender, e) => eventosDisparados.Add(e.PropertyName!);

        // Act
        mesa.Estado = "Disponible";

        // Assert
        eventosDisparados.Should().NotBeEmpty();
    }

    #endregion

    #region Propiedades Adicionales

    [Theory]
    [InlineData("Zona A")]
    [InlineData("Zona B")]
    [InlineData("")]
    public void Zona_DeberiaEstablecerCorrectamente(string zona)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Zona = zona;

        // Assert
        mesa.Zona.Should().Be(zona);
    }

    [Theory]
    [InlineData("Ubicación 1")]
    [InlineData("Ubicación 2")]
    [InlineData("")]
    public void Ubicacion_DeberiaEstablecerCorrectamente(string ubicacion)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Ubicacion = ubicacion;

        // Assert
        mesa.Ubicacion.Should().Be(ubicacion);
    }

    [Theory]
    [InlineData("Tipo 1")]
    [InlineData("Tipo 2")]
    [InlineData("")]
    public void Tipo_DeberiaEstablecerCorrectamente(string tipo)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Tipo = tipo;

        // Assert
        mesa.Tipo.Should().Be(tipo);
    }

    [Theory]
    [InlineData("Observación 1")]
    [InlineData("Observación 2")]
    [InlineData("")]
    public void Observaciones_DeberiaEstablecerCorrectamente(string observaciones)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.Observaciones = observaciones;

        // Assert
        mesa.Observaciones.Should().Be(observaciones);
    }

    [Fact]
    public void ClienteId_DeberiaEstablecerCorrectamente()
    {
        // Arrange
        var mesa = new MesaDto();
        var clienteId = Guid.NewGuid();

        // Act
        mesa.ClienteId = clienteId;

        // Assert
        mesa.ClienteId.Should().Be(clienteId);
    }

    [Theory]
    [InlineData("Cliente 1")]
    [InlineData("Cliente 2")]
    [InlineData("")]
    public void NombreCliente_DeberiaEstablecerCorrectamente(string nombreCliente)
    {
        // Arrange
        var mesa = new MesaDto();

        // Act
        mesa.NombreCliente = nombreCliente;

        // Assert
        mesa.NombreCliente.Should().Be(nombreCliente);
    }

    [Fact]
    public void UltimaActualizacion_DeberiaEstablecerCorrectamente()
    {
        // Arrange
        var mesa = new MesaDto();
        var fecha = DateTime.Now;

        // Act
        mesa.UltimaActualizacion = fecha;

        // Assert
        mesa.UltimaActualizacion.Should().Be(fecha);
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void Estado_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var mesa = new MesaDto();
        var estado = "Disponible con espacios y caracteres especiales: áéíóú";

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.Estado.Should().Be(estado);
        // El código actual normaliza solo letras, no caracteres especiales
        // Por lo tanto, no encuentra "disponible" en el string con caracteres especiales
        mesa.Disponible.Should().BeFalse(); // No encuentra "disponible" normalizado
    }

    [Fact]
    public void Estado_ConNumeros_DeberiaManejarCorrectamente()
    {
        // Arrange
        var mesa = new MesaDto();
        var estado = "Disponible123";

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.Estado.Should().Be(estado);
        mesa.Disponible.Should().BeTrue(); // Debería normalizar y encontrar "disponible"
    }

    [Fact]
    public void Estado_ConEspaciosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var mesa = new MesaDto();
        var estado = "   Disponible   ";

        // Act
        mesa.Estado = estado;

        // Assert
        mesa.Estado.Should().Be(estado);
        mesa.Disponible.Should().BeTrue(); // Debería normalizar y encontrar "disponible"
    }

    #endregion
}

/// <summary>
/// Extensión para convertir Color a string hexadecimal
/// </summary>
public static class ColorExtensions
{
    public static string ToArgbHex(this Microsoft.Maui.Graphics.Color color)
    {
        var r = (int)(color.Red * 255);
        var g = (int)(color.Green * 255);
        var b = (int)(color.Blue * 255);
        var a = (int)(color.Alpha * 255);
        return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
    }
}
