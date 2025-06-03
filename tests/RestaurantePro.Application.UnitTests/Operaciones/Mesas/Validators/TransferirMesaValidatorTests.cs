using FluentAssertions;
using Moq;
using RestaurantePro.Application.Operaciones.Mesas.Commands.TransferirMesa;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.UnitTests.Common;

namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// Tests unitarios para TransferirMesaValidator
/// Validación completa de reglas de negocio para transferencia de comandas entre mesas
/// </summary>
public class TransferirMesaValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly TransferirMesaValidator _validator;

    public TransferirMesaValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new TransferirMesaValidator(_contextMock.Object);
    }

    [Fact]
    public async Task Validator_ConComandoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMocksParaValidacion(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConComandaIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID de la comanda no puede ser un GUID vacío.");
    }

    [Fact]
    public async Task Validator_ConMesasIguales_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaDestinoId = command.MesaOrigenId;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino debe ser diferente a la mesa de origen.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]  // Muy corto
    public async Task Validator_ConMotivoInvalido_DeberiaFallar(string motivoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MotivoTransferencia = motivoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("motivo"));
    }

    [Fact]
    public async Task Validator_ConComandaNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandaNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConComandaNoTransferible_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandaNoTransferible(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda no puede ser transferida en su estado actual.");
    }

    [Fact]
    public async Task Validator_ConMesaDestinoNoDisponible_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMesaDestinoNoDisponible(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino no está disponible.");
    }

    [Fact]
    public async Task Validator_ConNotasTransferenciaMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasTransferencia = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las notas de transferencia no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksParaValidacion(TransferirMesaCommand command)
    {
        // Crear entidades de prueba
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-001");
        // Usar reflection para establecer el ID y estado si es necesario
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, EstadoComanda.Creada);

        var mesaOrigen = Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaOrigen, command.MesaOrigenId);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesaOrigen, EstadoMesa.Ocupada);

        var mesaDestino = Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaDestino, command.MesaDestinoId);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesaDestino, EstadoMesa.Disponible);

        // Configurar mocks usando MockDbSetHelper
        var comandasList = new List<Comanda> { comanda };
        var mesasList = new List<Mesa> { mesaOrigen, mesaDestino };

        var comandasMock = MockDbSetHelper.CreateMockDbSet(comandasList.AsQueryable());
        var mesasMock = MockDbSetHelper.CreateMockDbSet(mesasList.AsQueryable());

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockComandaNoExiste()
    {
        // Configurar DbSet vacío
        var comandasMock = MockDbSetHelper.CreateEmptyMockDbSet<Comanda>();
        var mesasMock = MockDbSetHelper.CreateMockDbSet(new List<Mesa>
        {
            Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior"),
            Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior")
        }.AsQueryable());

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockComandaNoTransferible(TransferirMesaCommand command)
    {
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-002");
        // Usar reflection para establecer el ID y estado finalizado
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, EstadoComanda.Finalizada);

        var comandasList = new List<Comanda> { comanda };
        var comandasMock = MockDbSetHelper.CreateMockDbSet(comandasList.AsQueryable());

        var mesasList = new List<Mesa>
        {
            Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior"),
            Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior")
        };
        var mesasMock = MockDbSetHelper.CreateMockDbSet(mesasList.AsQueryable());

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockMesaDestinoNoDisponible(TransferirMesaCommand command)
    {
        // Crear comanda en la mesa destino para hacerla no disponible
        var comandaEnMesaDestino = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaDestinoId,
            observaciones: "Comanda en mesa destino",
            numeroComanda: "TEST-003");
        typeof(Comanda).GetProperty("Id")?.SetValue(comandaEnMesaDestino, Guid.NewGuid());
        typeof(Comanda).GetProperty("Estado")?.SetValue(comandaEnMesaDestino, EstadoComanda.EnProceso);

        var comandaOriginal = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-001");
        typeof(Comanda).GetProperty("Id")?.SetValue(comandaOriginal, command.ComandaId);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comandaOriginal, EstadoComanda.Creada);

        var comandasList = new List<Comanda> { comandaOriginal, comandaEnMesaDestino };
        var comandasMock = MockDbSetHelper.CreateMockDbSet(comandasList.AsQueryable());

        var mesaOrigen = Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaOrigen, command.MesaOrigenId);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesaOrigen, EstadoMesa.Ocupada);

        var mesaDestino = Mesa.Crear(numero: 2, capacidad: 6, ubicacion: "Terraza");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaDestino, command.MesaDestinoId);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesaDestino, EstadoMesa.Ocupada);

        var mesasList = new List<Mesa> { mesaOrigen, mesaDestino };
        var mesasMock = MockDbSetHelper.CreateMockDbSet(mesasList.AsQueryable());

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private TransferirMesaCommand CrearComandoValido()
    {
        return new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Cliente solicita cambio de mesa",
            NotificarMesero = true,
            MantenerEstado = true
        };
    }
} 