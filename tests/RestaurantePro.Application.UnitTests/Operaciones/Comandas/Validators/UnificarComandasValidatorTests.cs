using FluentAssertions;
using Moq;
using RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using MockQueryable.Moq;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// Tests unitarios para UnificarComandasValidator
/// Validación completa de reglas de negocio para unificación de comandas
/// </summary>
public class UnificarComandasValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UnificarComandasValidator _validator;

    public UnificarComandasValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new UnificarComandasValidator(_contextMock.Object);
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
    public async Task Validator_SinComandasIds_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos 2 comandas para unificar.");
    }

    [Fact]
    public async Task Validator_ConSoloUnaComanda_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid() };
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos 2 comandas para unificar.");
    }

    [Fact]
    public async Task Validator_ConDemasiadasComandas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = Enumerable.Range(1, 11).Select(_ => Guid.NewGuid()).ToList();
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden unificar más de 10 comandas a la vez.");
    }

    [Fact]
    public async Task Validator_ConIdsComandaDuplicados_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var id = Guid.NewGuid();
        command.ComandasIds = new List<Guid> { id, id };
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("únicos") || x.ErrorMessage.Contains("duplicados"));
    }

    [Fact]
    public async Task Validator_ConMesaDestinoIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaDestinoId = Guid.Empty;
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("mesa") && x.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public async Task Validator_ConMeseroIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MeseroId = Guid.Empty;
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("mesero") && x.ErrorMessage.Contains("requerido"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]  // Muy corto
    public async Task Validator_ConMotivoInvalido_DeberiaFallar(string motivoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MotivoUnificacion = motivoInvalido;
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("motivo"));
    }

    [Fact]
    public async Task Validator_ConEstrategiaDescuentosInvalida_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.EstrategiaDescuentos = (EstrategiaDescuentos)999; // Valor enum inválido
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("estrategia"));
    }

    [Fact]
    public async Task Validator_ConComandaNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasNoExisten();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Una o más comandas especificadas no existen.");
    }

    [Fact]
    public async Task Validator_ConComandasNoUnificables_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasNoUnificables(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Una o más comandas no pueden ser unificadas en su estado actual.");
    }

    // TODO: Habilitar cuando se implemente la verificación de comandas facturadas en UnificarComandasValidator
    // [Fact]
    // public async Task Validator_ConComandasFacturadas_DeberiaFallar()
    // {
    //     // Arrange
    //     var command = CrearComandoValido();
    //     ConfigurarMockComandasFacturadas(command);
    // 
    //     // Act
    //     var result = await _validator.ValidateAsync(command);
    // 
    //     // Assert
    //     result.IsValid.Should().BeFalse();
    //     result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden unificar comandas que ya han sido facturadas.");
    // }

    [Fact]
    public async Task Validator_ConMesaDestinoNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMesaNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConMeseroNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMeseroNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El mesero especificado no existe.");
    }

    [Fact]
    public async Task Validator_ConNotasUnificacionMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasUnificacion = new string('A', 501); // Más de 500 caracteres
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("notas") && x.ErrorMessage.Contains("500"));
    }

    [Fact]
    public async Task Validator_ConObservacionesUnificadaMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ObservacionesUnificada = new string('A', 501); // Más de 500 caracteres
        ConfigurarMocksBasicos();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las observaciones de la comanda unificada no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksBasicos()
    {
        // Configurar mocks usando MockQueryable
        var comandasVacias = new List<Comanda>().AsQueryable().BuildMockDbSet();
        var mesasVacias = new List<Mesa>().AsQueryable().BuildMockDbSet();
        var usuariosVacios = new List<Usuario>().AsQueryable().BuildMockDbSet();

        _contextMock.Setup(x => x.Comandas).Returns(comandasVacias.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasVacias.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosVacios.Object);
    }

    private void ConfigurarMocksParaValidacion(UnificarComandasCommand command)
    {
        // Mock para comandas
        var comandas = command.ComandasIds.Select(id => CrearComandaMock(id, EstadoComanda.Creada)).ToList();
        var comandasMock = comandas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);

        // Mock para mesas
        var mesa = CrearMesaMock(command.MesaDestinoId, EstadoMesa.Disponible);
        var mesas = new List<Mesa> { mesa };
        var mesasMock = mesas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);

        // Mock para usuarios (meseros)
        var usuario = CrearUsuarioMock(command.MeseroId, true);
        var usuarios = new List<Usuario> { usuario };
        var usuariosMock = usuarios.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    private void ConfigurarMockComandasNoExisten()
    {
        var comandasVacias = new List<Comanda>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Comandas).Returns(comandasVacias.Object);
        
        // Configurar mocks básicos para otras entidades para evitar NullReferenceException
        var mesasVacias = new List<Mesa>().AsQueryable().BuildMockDbSet();
        var usuariosVacios = new List<Usuario>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Mesas).Returns(mesasVacias.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosVacios.Object);
    }

    private void ConfigurarMockComandasNoUnificables(UnificarComandasCommand command)
    {
        // Crear comandas que NO son unificables (diferentes estados)
        var comandas = command.ComandasIds.Select((id, index) => 
            CrearComandaMock(id, index == 0 ? EstadoComanda.Finalizada : EstadoComanda.Creada)
        ).ToList();
        
        var comandasMock = comandas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        
        // Configurar mocks básicos para otras entidades para evitar NullReferenceException
        var mesasVacias = new List<Mesa>().AsQueryable().BuildMockDbSet();
        var usuariosVacios = new List<Usuario>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Mesas).Returns(mesasVacias.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosVacios.Object);
    }

    private void ConfigurarMockComandasFacturadas(UnificarComandasCommand command)
    {
        // Crear comandas que están facturadas
        var comandas = command.ComandasIds.Select(id => 
        {
            var comanda = CrearComandaMock(id, EstadoComanda.Finalizada);
            // Marcar como facturada usando reflection si es necesario
            return comanda;
        }).ToList();
        
        var comandasMock = comandas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);

        // Configurar mocks básicos para otras entidades
        var mesasVacias = new List<Mesa>().AsQueryable().BuildMockDbSet();
        var usuariosVacios = new List<Usuario>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Mesas).Returns(mesasVacias.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosVacios.Object);
    }

    private void ConfigurarMockMesaNoExiste()
    {
        // Configurar comandas válidas pero mesa no existente
        var comandas = new List<Comanda> 
        { 
            CrearComandaMock(Guid.NewGuid(), EstadoComanda.Creada),
            CrearComandaMock(Guid.NewGuid(), EstadoComanda.Creada)
        };
        var comandasMock = comandas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);

        // Mesas vacías (no existe la mesa destino)
        var mesasVacias = new List<Mesa>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Mesas).Returns(mesasVacias.Object);

        // Usuarios válidos
        var usuarios = new List<Usuario> { CrearUsuarioMock(Guid.NewGuid(), true) };
        var usuariosMock = usuarios.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    private void ConfigurarMockMeseroNoExiste()
    {
        // Configurar comandas válidas
        var comandas = new List<Comanda> 
        { 
            CrearComandaMock(Guid.NewGuid(), EstadoComanda.Creada),
            CrearComandaMock(Guid.NewGuid(), EstadoComanda.Creada)
        };
        var comandasMock = comandas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);

        // Mesa válida
        var mesas = new List<Mesa> { CrearMesaMock(Guid.NewGuid(), EstadoMesa.Disponible) };
        var mesasMock = mesas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);

        // Usuarios vacíos (no existe el mesero)
        var usuariosVacios = new List<Usuario>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosVacios.Object);
    }

    private Comanda CrearComandaMock(Guid id, EstadoComanda estado)
    {
        var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "Test", "TEST-001");
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, id);
        
        // Solo cambiar estado si es diferente al actual
        if (comanda.Estado != estado)
        {
            switch (estado)
            {
                case EstadoComanda.EnProceso:
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    break;
                case EstadoComanda.Finalizada:
                    comanda.AgregarProducto(Guid.NewGuid(), 1, 10.00m, "Producto");
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    comanda.ActualizarEstado(EstadoComanda.Lista);
                    comanda.ActualizarEstado(EstadoComanda.Entregada);
                    comanda.ActualizarEstado(EstadoComanda.Finalizada);
                    break;
                case EstadoComanda.Cancelada:
                    comanda.Cancelar("Cancelada para test");
                    break;
            }
        }
        
        return comanda;
    }

    private Mesa CrearMesaMock(Guid id, EstadoMesa estado)
    {
        var mesa = Mesa.Crear(1, 4, "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        
        if (estado != EstadoMesa.Disponible)
        {
            switch (estado)
            {
                case EstadoMesa.Ocupada:
                    mesa.MarcarComoOcupada();
                    break;
                case EstadoMesa.Reservada:
                    mesa.MarcarComoReservada();
                    break;
                case EstadoMesa.FueraDeServicio:
                    mesa.MarcarComoFueraDeServicio("Test");
                    break;
            }
        }
        
        return mesa;
    }

    private Usuario CrearUsuarioMock(Guid id, bool activo)
    {
        var usuario = Usuario.Crear("testuser", "Usuario Test", "test@test.com", RolUsuario.Mesero);
        typeof(Usuario).GetProperty("Id")?.SetValue(usuario, id);
        
        if (!activo)
        {
            usuario.Desactivar();
        }
        
        return usuario;
    }

    private UnificarComandasCommand CrearComandoValido()
    {
        return new UnificarComandasCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Solicitud del cliente para mesa más grande",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar,
            NotasUnificacion = "Notas de prueba",
            ObservacionesUnificada = "Observaciones de la comanda unificada"
        };
    }
} 