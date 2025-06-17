using FluentAssertions;
using Moq;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA MODIFICAR RESERVACION VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de modificación de reservaciones
/// Cobertura: 100% de reglas de negocio del ModificarReservacionValidator
/// </summary>
public class ModificarReservacionValidatorTests
{
    private readonly ModificarReservacionValidator _validator;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<DbSet<Reservacion>> _reservacionesDbSetMock;
    private readonly Mock<DbSet<Mesa>> _mesasDbSetMock;
    private readonly Mock<DbSet<Cliente>> _clientesDbSetMock;

    public ModificarReservacionValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _reservacionesDbSetMock = new Mock<DbSet<Reservacion>>();
        _mesasDbSetMock = new Mock<DbSet<Mesa>>();
        _clientesDbSetMock = new Mock<DbSet<Cliente>>();
        
        _contextMock.Setup(x => x.Reservaciones).Returns(_reservacionesDbSetMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(_mesasDbSetMock.Object);
        _contextMock.Setup(x => x.Clientes).Returns(_clientesDbSetMock.Object);
        
        _validator = new ModificarReservacionValidator();
    }

    #region Validation Command Helper

    private ModificarReservacionCommand CrearCommandValido()
    {
        return new ModificarReservacionCommand
        {
            ReservacionId = Guid.NewGuid(),
            NuevaFechaReservacion = DateTime.Now.AddDays(1),
            NuevaHoraReservacion = TimeSpan.FromHours(19), // 7:00 PM
            NuevoNumeroPersonas = 4,
            NuevaMesaId = Guid.NewGuid(),
            NuevoClienteId = Guid.NewGuid(),
            MotivoModificacion = "Cambio de horario solicitado por cliente",
            ObservacionesModificacion = "Cliente prefiere horario más tarde",
            UsuarioId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación ReservacionId

    [Fact]
    public async Task Validate_ConReservacionIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ReservacionId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.ReservacionId) &&
            e.ErrorMessage.Contains("El ID de la reservación es requerido") &&
            e.ErrorCode == "RESERVACION_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConReservacionIdValido_NoDeberiaRetornarErrorDeReservacionId()
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacionId = Guid.NewGuid();
        command.ReservacionId = reservacionId;

        // Mock reservación existente
        var reservaciones = new List<Reservacion>
        {
            CrearReservacionMock(reservacionId, EstadoReservacion.Confirmada)
        }.AsQueryable();

        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.Provider).Returns(reservaciones.Provider);
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.Expression).Returns(reservaciones.Expression);
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.ElementType).Returns(reservaciones.ElementType);
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.GetEnumerator()).Returns(reservaciones.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ModificarReservacionCommand.ReservacionId));
    }

    #endregion

    #region Validación NuevaFechaReservacion

    [Fact]
    public async Task Validate_ConFechaEnElPasado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaFechaReservacion = DateTime.Now.AddDays(-1); // Ayer

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaFechaReservacion) &&
            e.ErrorMessage.Contains("La fecha de reservación no puede ser en el pasado") &&
            e.ErrorCode == "FECHA_RESERVACION_PASADO");
    }

    [Fact]
    public async Task Validate_ConFechaMuyFutura_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaFechaReservacion = DateTime.Now.AddDays(91); // Más de 90 días

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaFechaReservacion) &&
            e.ErrorMessage.Contains("La fecha de reservación no puede ser más de 90 días en el futuro") &&
            e.ErrorCode == "FECHA_RESERVACION_MUY_FUTURA");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)] // Límite máximo
    public async Task Validate_ConFechaValida_NoDeberiaRetornarErrorDeFecha(int diasFuturos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaFechaReservacion = DateTime.Now.AddDays(diasFuturos);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaFechaReservacion));
    }

    #endregion

    #region Validación NuevaHoraReservacion

    [Fact]
    public async Task Validate_ConHoraAntesDeLaApertura_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaHoraReservacion = TimeSpan.FromHours(8); // 8:00 AM (antes de apertura 10:00 AM)

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaHoraReservacion) &&
            e.ErrorMessage.Contains("La hora de reservación debe estar entre las 10:00 y las 22:30") &&
            e.ErrorCode == "HORA_RESERVACION_FUERA_HORARIO");
    }

    [Fact]
    public async Task Validate_ConHoraDespuesDelCierre_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaHoraReservacion = TimeSpan.FromHours(23); // 11:00 PM (después de cierre 10:30 PM)

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaHoraReservacion) &&
            e.ErrorMessage.Contains("La hora de reservación debe estar entre las 10:00 y las 22:30") &&
            e.ErrorCode == "HORA_RESERVACION_FUERA_HORARIO");
    }

    [Theory]
    [InlineData(10, 0)]  // 10:00 AM - Apertura
    [InlineData(12, 30)] // 12:30 PM - Almuerzo
    [InlineData(19, 0)]  // 7:00 PM - Cena
    [InlineData(22, 30)] // 10:30 PM - Último horario
    public async Task Validate_ConHoraValida_NoDeberiaRetornarErrorDeHora(int horas, int minutos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaHoraReservacion = new TimeSpan(horas, minutos, 0);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaHoraReservacion));
    }

    #endregion

    #region Validación NuevoNumeroPersonas

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_ConNumeroPersonasMenorIgualCero_DeberiaRetornarError(int numeroPersonasInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoNumeroPersonas = numeroPersonasInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevoNumeroPersonas) &&
            e.ErrorMessage.Contains("El número de personas debe ser mayor a 0") &&
            e.ErrorCode == "NUMERO_PERSONAS_INVALIDO");
    }

    [Fact]
    public async Task Validate_ConNumeroPersonasExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoNumeroPersonas = 21; // Más de 20 personas

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevoNumeroPersonas) &&
            e.ErrorMessage.Contains("El número de personas no puede exceder 20") &&
            e.ErrorCode == "NUMERO_PERSONAS_EXCESIVO");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(20)] // Límite máximo
    public async Task Validate_ConNumeroPersonasValido_NoDeberiaRetornarErrorDeNumeroPersonas(int numeroPersonasValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoNumeroPersonas = numeroPersonasValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevoNumeroPersonas));
    }

    #endregion

    #region Validación NuevaMesaId

    [Fact]
    public async Task Validate_ConMesaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevaMesaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevaMesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es requerido") &&
            e.ErrorCode == "MESA_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConMesaIdValido_NoDeberiaRetornarErrorDeMesaId()
    {
        // Arrange
        var command = CrearCommandValido();
        var mesaId = Guid.NewGuid();
        command.NuevaMesaId = mesaId;

        // Mock mesa existente y disponible
        var mesas = new List<Mesa>
        {
            CrearMesaMock(mesaId, 1, 4, EstadoMesa.Disponible)
        }.AsQueryable();

        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(mesas.Provider);
        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(mesas.Expression);
        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(mesas.ElementType);
        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(mesas.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ModificarReservacionCommand.NuevaMesaId));
    }

    #endregion

    #region Validación NuevoClienteId

    [Fact]
    public async Task Validate_ConClienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoClienteId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.NuevoClienteId) &&
            e.ErrorMessage.Contains("El ID del cliente es requerido") &&
            e.ErrorCode == "CLIENTE_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConClienteIdValido_NoDeberiaRetornarErrorDeClienteId()
    {
        // Arrange
        var command = CrearCommandValido();
        var clienteId = Guid.NewGuid();
        command.NuevoClienteId = clienteId;

        // Mock cliente existente y activo
        var clientes = new List<Cliente>
        {
            CrearClienteMock(clienteId, "Cliente Test", "test@email.com", true)
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ModificarReservacionCommand.NuevoClienteId));
    }

    #endregion

    #region Validación MotivoModificacion

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConMotivoModificacionVacio_DeberiaRetornarError(string motivoVacio)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoModificacion = motivoVacio;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.MotivoModificacion) &&
            e.ErrorMessage.Contains("El motivo de modificación es requerido") &&
            e.ErrorCode == "MOTIVO_MODIFICACION_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConMotivoModificacionMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoModificacion = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.MotivoModificacion) &&
            e.ErrorMessage.Contains("El motivo de modificación no puede exceder 500 caracteres") &&
            e.ErrorCode == "MOTIVO_MODIFICACION_LONGITUD");
    }

    [Theory]
    [InlineData("Cambio de horario")]
    [InlineData("Cambio de número de personas")]
    [InlineData("Solicitud de mesa diferente")]
    [InlineData("Modificación solicitada por cliente")]
    public async Task Validate_ConMotivoModificacionValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoModificacion = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.MotivoModificacion));
    }

    #endregion

    #region Validación ObservacionesModificacion

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarObservaciones(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesModificacion = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.ObservacionesModificacion));
    }

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesModificacion = new string('A', 1001); // Más de 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.ObservacionesModificacion) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres") &&
            e.ErrorCode == "OBSERVACIONES_LONGITUD");
    }

    #endregion

    #region Validación UsuarioId

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ModificarReservacionCommand.UsuarioId) &&
            e.ErrorMessage == "El ID del usuario es requerido" &&
            e.ErrorCode == "USUARIO_ID_REQUERIDO");
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompleto_DeberiaSerValido()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Now.AddDays(3),
            NuevaHoraReservacion = TimeSpan.FromHours(20), // 8:00 PM
            NuevoNumeroPersonas = 6,
            NuevaMesaId = mesaId,
            NuevoClienteId = clienteId,
            MotivoModificacion = "Cliente solicita cambio de horario y aumento de comensales",
            ObservacionesModificacion = "Cliente confirmó que llegará con 2 personas adicionales",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new ModificarReservacionCommand
        {
            ReservacionId = Guid.Empty, // Error
            NuevaFechaReservacion = DateTime.Now.AddDays(-1), // Error - pasado
            NuevaHoraReservacion = TimeSpan.FromHours(8), // Error - muy temprano
            NuevoNumeroPersonas = 0, // Error
            NuevaMesaId = Guid.Empty, // Error
            NuevoClienteId = Guid.Empty, // Error
            MotivoModificacion = "", // Error
            ObservacionesModificacion = new string('X', 1001), // Error
            UsuarioId = Guid.Empty // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(9);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Cambio de horario", "Cliente prefiere horario más tarde")]
    [InlineData("Aumento de comensales", "Se agregaron 2 personas más")]
    [InlineData("Cambio de mesa", "Mesa solicitada tiene mejor vista")]
    [InlineData("Solicitud especial", "Cliente requiere mesa cerca de la entrada")]
    public async Task Validate_ConDiferentesEscenariosModificacion_DeberiaSerValido(string motivo, string observaciones)
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        
        var command = CrearCommandValido();
        command.ReservacionId = reservacionId;
        command.NuevaMesaId = mesaId;
        command.NuevoClienteId = clienteId;
        command.MotivoModificacion = motivo;
        command.ObservacionesModificacion = observaciones;

        // Mock entidades
        SetupValidEntities(reservacionId, mesaId, clienteId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var command = CrearCommandValido();
        SetupValidEntities(command.ReservacionId, command.NuevaMesaId, command.NuevoClienteId);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(command);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150);
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public void Command_DeberiaCrearseConConstructorDirecto()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var fechaNueva = DateTime.Now.AddDays(2);
        var horaNueva = TimeSpan.FromHours(19);
        var usuarioId = Guid.NewGuid();

        // Act
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = fechaNueva,
            NuevaHoraReservacion = horaNueva,
            NuevoNumeroPersonas = 4,
            MotivoModificacion = "Cambio de horario",
            UsuarioId = usuarioId
        };

        // Assert
        command.ReservacionId.Should().Be(reservacionId);
        command.NuevaFechaReservacion.Should().Be(fechaNueva);
        command.NuevaHoraReservacion.Should().Be(horaNueva);
        command.NuevoNumeroPersonas.Should().Be(4);
        command.MotivoModificacion.Should().Be("Cambio de horario");
        command.UsuarioId.Should().Be(usuarioId);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSets(IQueryable<Reservacion> reservaciones, IQueryable<Mesa> mesas, IQueryable<Cliente> clientes)
    {
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.Provider).Returns(reservaciones.Provider);
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.Expression).Returns(reservaciones.Expression);
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.ElementType).Returns(reservaciones.ElementType);
        _reservacionesDbSetMock.As<IQueryable<Reservacion>>().Setup(m => m.GetEnumerator()).Returns(reservaciones.GetEnumerator());

        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(mesas.Provider);
        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(mesas.Expression);
        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(mesas.ElementType);
        _mesasDbSetMock.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(mesas.GetEnumerator());

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());
    }

    private void SetupValidEntities(Guid reservacionId, Guid? mesaId, Guid? clienteId)
    {
        var reservaciones = new List<Reservacion>
        {
            CrearReservacionMock(reservacionId, EstadoReservacion.Confirmada)
        }.AsQueryable();

        var mesas = new List<Mesa>();
        if (mesaId.HasValue)
        {
            mesas.Add(CrearMesaMock(mesaId.Value, 1, 8, EstadoMesa.Disponible));
        }

        var clientes = new List<Cliente>();
        if (clienteId.HasValue)
        {
            clientes.Add(CrearClienteMock(clienteId.Value, "Cliente Test", "test@email.com", true));
        }

        SetupMockDbSets(reservaciones, mesas.AsQueryable(), clientes.AsQueryable());
    }

    #endregion

    #region Helper Methods para crear Mocks de Entidades

    private Reservacion CrearReservacionMock(Guid id, EstadoReservacion estado)
    {
        // Usar reflection para crear la entidad con constructor privado
        var reservacion = (Reservacion)Activator.CreateInstance(typeof(Reservacion), true)!;
        
        typeof(Reservacion).GetProperty("Id")?.SetValue(reservacion, id);
        typeof(Reservacion).GetProperty("Estado")?.SetValue(reservacion, estado);
        typeof(Reservacion).GetProperty("ClienteId")?.SetValue(reservacion, Guid.NewGuid());
        typeof(Reservacion).GetProperty("MesaId")?.SetValue(reservacion, Guid.NewGuid());
        typeof(Reservacion).GetProperty("Fecha")?.SetValue(reservacion, DateTime.Today.AddDays(1));
        typeof(Reservacion).GetProperty("Hora")?.SetValue(reservacion, TimeSpan.FromHours(19));
        typeof(Reservacion).GetProperty("CantidadPersonas")?.SetValue(reservacion, 4);
        typeof(Reservacion).GetProperty("Telefono")?.SetValue(reservacion, "555-1234");
        typeof(Reservacion).GetProperty("Email")?.SetValue(reservacion, "test@email.com");
        typeof(Reservacion).GetProperty("Observaciones")?.SetValue(reservacion, "Test");
        typeof(Reservacion).GetProperty("FechaCreacion")?.SetValue(reservacion, DateTime.Now);
        
        return reservacion;
    }

    private Mesa CrearMesaMock(Guid id, int numero, int capacidad, EstadoMesa estado)
    {
        var mock = new Mock<Mesa>();
        mock.Setup(m => m.Id).Returns(id);
        mock.Setup(m => m.Numero).Returns(numero);
        mock.Setup(m => m.Capacidad).Returns(capacidad);
        mock.Setup(m => m.Estado).Returns(estado);
        return mock.Object;
    }

    private Cliente CrearClienteMock(Guid id, string nombre, string email, bool activo)
    {
        var mock = new Mock<Cliente>();
        var nombreCompleto = ClienteNombre.Crear(nombre, "Test");
        var emailVO = Email.Create(email);
        var telefono = PhoneNumber.Create("123456789");

        mock.Setup(c => c.Id).Returns(id);
        mock.Setup(c => c.Nombre).Returns(nombreCompleto);
        mock.Setup(c => c.Email).Returns(emailVO);
        mock.Setup(c => c.Telefono).Returns(telefono);
        mock.Setup(c => c.EstaActivo).Returns(activo);
        
        return mock.Object;
    }

    #endregion
} 