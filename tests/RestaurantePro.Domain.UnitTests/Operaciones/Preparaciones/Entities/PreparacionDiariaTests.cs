using System;
using System.Linq;
using FluentAssertions;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Events;
using Xunit;

namespace RestaurantePro.Domain.UnitTests.Operaciones.Preparaciones.Entities;

/// <summary>
/// Pruebas unitarias para la entidad PreparacionDiaria
/// </summary>
public class PreparacionDiariaTests
{
    private readonly Guid _productoId;
    private readonly Guid _chefId;
    private readonly DateTime _fechaActual;
    private readonly DateTime _fechaVencimientoPredeterminada;

    public PreparacionDiariaTests()
    {
        _productoId = Guid.NewGuid();
        _chefId = Guid.NewGuid();
        _fechaActual = DateTime.Now;
        _fechaVencimientoPredeterminada = _fechaActual.AddHours(8);
    }

    #region Tests de Crear

    [Fact]
    public void Crear_ConParametrosValidos_DebeCrearPreparacion()
    {
        // Act
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Assert
        preparacion.Should().NotBeNull();
        preparacion.ProductoId.Should().Be(_productoId);
        preparacion.CantidadPreparada.Should().Be(10);
        preparacion.CantidadDisponible.Should().Be(10);
        preparacion.ChefId.Should().Be(_chefId);
        preparacion.FechaPreparacion.Should().BeOnOrAfter(_fechaActual.Date);
        preparacion.FechaVencimiento.Should().Be(_fechaVencimientoPredeterminada);
        preparacion.Estado.Should().Be(EstadoPreparacion.Preparando);
        preparacion.Observaciones.Should().BeNull();
    }

    [Fact]
    public void Crear_SinFechaVencimiento_DebeCrearPreparacionSinFecha()
    {
        // Act
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Assert
        preparacion.Should().NotBeNull();
        preparacion.FechaVencimiento.Should().Be(_fechaVencimientoPredeterminada);
    }

    [Fact]
    public void Crear_ConObservacionesEspacios_DebeTrimarObservaciones()
    {
        // Arrange
        var observaciones = "   Observaciones con espacios   ";

        // Act
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada, observaciones);

        // Assert
        preparacion.Observaciones.Should().Be("Observaciones con espacios");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Crear_ConProductoIdVacio_DebeLanzarException(string guidString)
    {
        // Arrange
        var productoIdVacio = Guid.Parse(guidString);

        // Act
        Action act = () => PreparacionDiaria.Crear(productoIdVacio, 10, _chefId, _fechaVencimientoPredeterminada);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*producto*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Crear_ConCantidadInvalida_DebeLanzarException(int cantidadInvalida)
    {
        // Act
        Action act = () => PreparacionDiaria.Crear(_productoId, cantidadInvalida, _chefId, _fechaVencimientoPredeterminada);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cantidad*");
    }

    [Fact]
    public void Crear_ConChefIdVacio_DebeLanzarException()
    {
        // Arrange
        var chefIdVacio = Guid.Empty;

        // Act
        Action act = () => PreparacionDiaria.Crear(_productoId, 10, chefIdVacio, _fechaVencimientoPredeterminada);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*chef*");
    }

    [Fact]
    public void Crear_ConFechaVencimientoPasada_DebeLanzarException()
    {
        // Arrange
        var fechaVencimientoPasada = _fechaActual.AddHours(-1);

        // Act
        Action act = () => PreparacionDiaria.Crear(_productoId, 10, _chefId, fechaVencimientoPasada);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*fecha de vencimiento*");
    }

    #endregion

    #region Tests de MarcarComoDisponible

    [Fact]
    public void MarcarComoDisponible_ConEstadoPreparando_DebeMarcarComoDisponible()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Act
        preparacion.MarcarComoDisponible();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
    }

    [Fact]
    public void MarcarComoDisponible_ConEstadoDistintoAPreparando_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible(); // Ya está disponible

        // Act
        preparacion.MarcarComoDisponible();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
    }

    #endregion

    #region Tests de ConsumirCantidad

    [Fact]
    public void ConsumirCantidad_ConCantidadValida_DebeReducirDisponible()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();
        var cantidadAConsumir = 3;

        // Act
        var resultado = preparacion.ConsumirCantidad(cantidadAConsumir);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        preparacion.CantidadDisponible.Should().Be(7);
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
    }

    [Fact]
    public void ConsumirCantidad_ConsumirTodo_DebeMarcarComoAgotada()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();

        // Act
        var resultado = preparacion.ConsumirCantidad(10);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        preparacion.CantidadDisponible.Should().Be(0);
        preparacion.Estado.Should().Be(EstadoPreparacion.Agotada);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void ConsumirCantidad_ConCantidadInvalida_DebeRetornarError(int cantidadInvalida)
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();

        // Act
        Action act = () => preparacion.ConsumirCantidad(cantidadInvalida);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*La cantidad a consumir debe ser mayor*");
    }

    [Fact]
    public void ConsumirCantidad_ConCantidadMayorADisponible_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();

        // Act
        Action act = () => preparacion.ConsumirCantidad(15);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No hay suficiente cantidad disponible*");
    }

    [Fact]
    public void ConsumirCantidad_ConEstadoVencida_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoVencida();

        // Act
        Action act = () => preparacion.ConsumirCantidad(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No se puede consumir una preparación vencida*");
    }

    [Fact]
    public void ConsumirCantidad_ConEstadoAgotada_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();
        preparacion.ConsumirCantidad(10); // Agotar

        // Act
        Action act = () => preparacion.ConsumirCantidad(1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No se puede consumir una preparación agotada*");
    }

    #endregion

    #region Tests de AgregarCantidad

    [Fact]
    public void AgregarCantidad_ConCantidadValida_DebeAumentarCantidades()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();
        var cantidadAdicional = 5;

        // Act
        preparacion.AgregarCantidad(cantidadAdicional);

        // Assert
        preparacion.CantidadPreparada.Should().Be(15);
        preparacion.CantidadDisponible.Should().Be(15);
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
    }

    [Fact]
    public void AgregarCantidad_ConPreparacionAgotada_DebeCambiarADisponible()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();
        preparacion.ConsumirCantidad(10); // Agotar
        
        // Act
        preparacion.AgregarCantidad(5);

        // Assert
        preparacion.CantidadDisponible.Should().Be(5);
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void AgregarCantidad_ConCantidadInvalida_DebeRetornarError(int cantidadInvalida)
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();

        // Act
        Action act = () => preparacion.AgregarCantidad(cantidadInvalida);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*La cantidad a agregar debe ser mayor a cero*");
    }

    [Fact]
    public void AgregarCantidad_ConEstadoVencida_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoVencida();

        // Act
        Action act = () => preparacion.AgregarCantidad(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No se puede agregar cantidad a una preparación vencida*");
    }

    #endregion

    #region Tests de MarcarComoVencida

    [Fact]
    public void MarcarComoVencida_ConEstadoValido_DebeMarcarComoVencida()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();

        // Act
        preparacion.MarcarComoVencida();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.Vencida);
    }

    [Fact]
    public void MarcarComoVencida_ConEstadoNoValido_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Act
        preparacion.MarcarComoVencida();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.Vencida);
    }

    #endregion

    #region Tests de MarcarComoPorVencer

    [Fact]
    public void MarcarComoPorVencer_ConEstadoValido_DebeMarcarComoPorVencer()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();

        // Act
        preparacion.MarcarComoPorVencer();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.PorVencer);
    }

    [Fact]
    public void MarcarComoPorVencer_ConEstadoNoValido_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Act
        preparacion.MarcarComoPorVencer();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.PorVencer);
    }

    [Fact]
    public void MarcarComoDisponible_ConEstadoValido_DebeMarcarComoDisponible()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Act
        preparacion.MarcarComoDisponible();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.Disponible);
    }

    [Fact]
    public void MarcarComoDisponible_ConEstadoVencida_DebeRetornarError()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoVencida();

        // Act
        preparacion.MarcarComoDisponible();

        // Assert
        preparacion.Estado.Should().Be(EstadoPreparacion.Vencida);
    }

    #endregion

    #region Tests de Eventos de Dominio

    [Fact]
    public void Crear_DebeGenerarEventoPreparacionCreada()
    {
        // Act
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);

        // Assert
        var eventos = preparacion.DomainEvents;
        eventos.Should().ContainSingle(e => e is PreparacionCreada);
        var evento = eventos.OfType<PreparacionCreada>().First();
        evento.PreparacionId.Should().Be(preparacion.Id);
    }

    [Fact]
    public void MarcarComoDisponible_DebeGenerarEventoPreparacionDisponible()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        
        // Act
        preparacion.MarcarComoDisponible();

        // Assert
        var eventos = preparacion.DomainEvents;
        eventos.Should().Contain(e => e is PreparacionDisponible);
        var evento = eventos.OfType<PreparacionDisponible>().First();
        evento.PreparacionId.Should().Be(preparacion.Id);
    }

    [Fact]
    public void ConsumirCantidad_DebeGenerarEventoPreparacionConsumida()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();
        
        // Act
        preparacion.ConsumirCantidad(3);

        // Assert
        var eventos = preparacion.DomainEvents;
        eventos.Should().Contain(e => e is PreparacionConsumida);
        var evento = eventos.OfType<PreparacionConsumida>().First();
        evento.PreparacionId.Should().Be(preparacion.Id);
        evento.CantidadConsumida.Should().Be(3);
    }

    [Fact]
    public void ConsumirCantidad_AgotarCompletamente_DebeGenerarEventoPreparacionAgotada()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        preparacion.MarcarComoDisponible();
        
        // Act
        preparacion.ConsumirCantidad(10);

        // Assert
        var eventos = preparacion.DomainEvents;
        eventos.Should().Contain(e => e is PreparacionAgotada);
        var evento = eventos.OfType<PreparacionAgotada>().First();
        evento.PreparacionId.Should().Be(preparacion.Id);
    }

    [Fact]
    public void MarcarComoVencida_DebeGenerarEventoPreparacionVencida()
    {
        // Arrange
        var preparacion = PreparacionDiaria.Crear(_productoId, 10, _chefId, _fechaVencimientoPredeterminada);
        
        // Act
        preparacion.MarcarComoVencida();

        // Assert
        var eventos = preparacion.DomainEvents;
        eventos.Should().Contain(e => e is PreparacionVencida);
        var evento = eventos.OfType<PreparacionVencida>().First();
        evento.PreparacionId.Should().Be(preparacion.Id);
    }

    #endregion
} 