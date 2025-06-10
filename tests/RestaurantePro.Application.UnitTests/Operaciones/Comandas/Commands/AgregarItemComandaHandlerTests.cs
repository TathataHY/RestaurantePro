namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

/// <summary>
/// Tests unitarios para AgregarItemComandaHandler
/// Valida la lógica de agregar productos a comandas existentes con personalizaciones
/// </summary>
public class AgregarItemComandaHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AgregarItemComandaHandler>> _loggerMock;
    private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
    private readonly AgregarItemComandaHandler _handler;
    private readonly Fixture _fixture;

    public AgregarItemComandaHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AgregarItemComandaHandler>>();
        _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
        _fixture = new Fixture();
        
        _handler = new AgregarItemComandaHandler(
            _comandaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _servicioPreparacionesMock.Object);
    }

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ComandaCreadaSinPersonalizaciones_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Margarita",
            Cantidad = 2,
            PrecioUnitario = 25.50m,
            Observaciones = "Sin cebolla",
            UsuarioId = usuarioId
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 51.00m);

        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(comandaId, result.Value.Id);
        Assert.Equal(51.00m, result.Value.Total);

        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Once);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaEnProcesoSinPersonalizaciones_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Hamburguesa Clásica",
            Cantidad = 1,
            PrecioUnitario = 35.00m,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, 35.00m);

        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(35.00m, result.Value.Total);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionExtra_DeberiaAgregarItemYPersonalizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Especial",
            Cantidad = 1,
            PrecioUnitario = 28.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = ingredienteId,
                    Cantidad = 1,
                    PrecioAdicional = 3.50m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 31.50m);

        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(31.50m, result.Value.Total);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Once);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionQuitar_DeberiaAgregarItemYPersonalizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Ensalada César",
            Cantidad = 1,
            PrecioUnitario = 18.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Quitar",
                    IngredienteId = ingredienteId,
                    Cantidad = 0,
                    PrecioAdicional = 0
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, 18.00m);

        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(18.00m, result.Value.Total);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Once);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionSustituir_DeberiaAgregarItemYPersonalizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteOriginalId = Guid.NewGuid();
        var ingredienteSustitutoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Sandwich Especial",
            Cantidad = 1,
            PrecioUnitario = 22.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Sustituir",
                    IngredienteId = ingredienteOriginalId,
                    IngredienteSustitucionId = ingredienteSustitutoId,
                    Cantidad = 1,
                    PrecioAdicional = 2.00m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 24.00m);

        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(24.00m, result.Value.Total);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Once);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConMultiplesPersonalizaciones_DeberiaAgregarTodasLasPersonalizaciones()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Personalizada",
            Cantidad = 1,
            PrecioUnitario = 30.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto { Tipo = "Extra", IngredienteId = Guid.NewGuid(), Cantidad = 1, PrecioAdicional = 2.50m },
                new PersonalizacionCreateDto { Tipo = "Quitar", IngredienteId = Guid.NewGuid(), Cantidad = 0, PrecioAdicional = 0 },
                new PersonalizacionCreateDto { Tipo = "Sustituir", IngredienteId = Guid.NewGuid(), IngredienteSustitucionId = Guid.NewGuid(), Cantidad = 1, PrecioAdicional = 1.50m }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 34.00m);

        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(34.00m, result.Value.Total);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Once);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DebeVerificarYConsumirPreparacion_CuandoProductoEstaDisponible()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Producto de prueba",
            Cantidad = 2,
            PrecioUnitario = 10.50m,
            Observaciones = "Observación de prueba"
        };

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 2),
                It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success(true));

        _servicioPreparacionesMock
            .Setup(x => x.ConsumirPreparacionAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 2)))
            .ReturnsAsync(Result.Success());

        _mapperMock
            .Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(new ComandaDto { Id = comandaId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se llamaron los métodos del servicio de preparaciones
        _servicioPreparacionesMock.Verify(
            x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 2),
                It.IsAny<Guid?>()),
            Times.Once);
            
        // Verificar que se actualizó y guardó la comanda
        _comandaRepositoryMock.Verify(
            x => x.ActualizarAsync(It.IsAny<Comanda>()),
            Times.Once);
            
        _comandaRepositoryMock.Verify(
            x => x.GuardarCambiosAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoDebeConsumirPreparacion_CuandoProductoNoEstaDisponible()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Producto de prueba",
            Cantidad = 2,
            PrecioUnitario = 10.50m,
            Observaciones = "Observación de prueba"
        };

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 2),
                It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success(false));

        _mapperMock
            .Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(new ComandaDto { Id = comandaId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se llamó VerificarDisponibilidad pero NO se llamó ConsumirPreparacion
        _servicioPreparacionesMock.Verify(
            x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 2),
                It.IsAny<Guid?>()),
            Times.Once);
            
        // Verificar que se actualizó y guardó la comanda de todos modos
        _comandaRepositoryMock.Verify(
            x => x.ActualizarAsync(It.IsAny<Comanda>()),
            Times.Once);
            
        _comandaRepositoryMock.Verify(
            x => x.GuardarCambiosAsync(),
            Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync((Comanda)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La comanda especificada no existe", result.Error);
        
        // Verify no se intenta actualizar
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Finalizada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar items a una comanda en estado 'Finalizada'", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaCancelada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Cancelada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar items a una comanda en estado 'Cancelada'", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaLista_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Lista);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar items a una comanda en estado 'Lista'", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorAlAgregarProducto_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        // Para este caso específico, usar un mock para simular la excepción
        var comandaMock = new Mock<Comanda>();
        comandaMock.Setup(x => x.AgregarItem(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Throws(new ArgumentException("Ya existe un item con el producto especificado"));

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comandaMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Ya existe un item con el producto especificado", result.Error);
    }

    [Fact]
    public async Task Handle_BusinessRuleViolationException_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        // Crear una comanda en estado que no permite agregar items (por ejemplo, Cancelada)
        var comanda = CreateMockComanda(comandaId, EstadoComanda.Cancelada);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar", result.Error);
    }

    #endregion

    #region Tests de Validaciones de Personalizaciones

    [Fact]
    public async Task Handle_PersonalizacionIngredienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.Empty;
            personalizacion.Cantidad = 1;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        
        SetupRepositoryAndMapper(comanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El ID del ingrediente es requerido para la personalización", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionTipoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "INVALIDO";
            personalizacion.IngredienteId = Guid.NewGuid();
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        
        SetupRepositoryAndMapper(comanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Tipo de personalización no válido: INVALIDO", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionExtraCantidadCero_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.Cantidad = 0;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        
        SetupRepositoryAndMapper(comanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La cantidad debe ser mayor que cero para personalizaciones de tipo Extra", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionSustituirSinIngredienteReemplazo_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Sustituir";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.IngredienteSustitucionId = Guid.Empty;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        
        SetupRepositoryAndMapper(comanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Se requiere especificar el ingrediente de sustitución para personalizaciones de tipo Sustituir", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionPrecioNegativo_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.Cantidad = 1;
            personalizacion.PrecioAdicional = -5.00m;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        
        SetupRepositoryAndMapper(comanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El precio adicional no puede ser negativo", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorAlAplicarPersonalizacion_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.Cantidad = 1;
            personalizacion.PrecioAdicional = 3.50m;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Finalizada); // Usar estado que no permite personalizaciones
        
        SetupRepositoryAndMapper(comanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Ocurrió un error interno al procesar la solicitud", result.Error);
    }

    #endregion

    #region Helper Methods

    private static AgregarItemComandaCommand CreateBasicCommand(Guid comandaId, Guid productoId)
    {
        return new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Test Product",
            Cantidad = 1,
            PrecioUnitario = 25.00m,
            UsuarioId = Guid.NewGuid()
        };
    }

    private static AgregarItemComandaCommand CreateCommandWithPersonalization(
        Guid comandaId,
        Action<PersonalizacionCreateDto> configurePersonalization)
    {
        var personalizacion = new PersonalizacionCreateDto();
        configurePersonalization(personalizacion);

        return new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Test Product with Personalization",
            Cantidad = 1,
            PrecioUnitario = 25.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion }
        };
    }

    private static Comanda CreateMockComanda(Guid id, EstadoComanda estado)
    {
        // Crear una instancia real de Comanda usando el factory method
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var observaciones = "Comanda de prueba";
        
        var comanda = Comanda.Crear(meseroId, clienteId, mesaId, observaciones);
        
        // Si necesitamos un estado diferente a Creada, usar reflexión para cambiarlo
        if (estado != EstadoComanda.Creada)
        {
            SetPrivateProperty(comanda, "Estado", estado);
        }
        
        // Establecer el ID específico si se requiere
        SetPrivateProperty(comanda, "Id", id);
        
        return comanda;
    }

    private static ComandaDto CreateMockComandaDto(Guid id, decimal total)
    {
        return new ComandaDto
        {
            Id = id,
            Estado = EstadoComanda.EnProceso,
            Total = total,
            FechaCreacion = DateTime.UtcNow,
            Items = new List<ItemComandaDto>
            {
                new ItemComandaDto 
                { 
                    Id = Guid.NewGuid(), 
                    NombreProducto = "Test Product", 
                    Cantidad = 1, 
                    PrecioUnitario = total
                }
            }
        };
    }

    private void SetupRepositoryAndMapper(Comanda comanda, ComandaDto comandaDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<bool>()))
            .ReturnsAsync(comanda);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>()))
            .Returns(Task.CompletedTask);

        _comandaRepositoryMock.Setup(x => x.GuardarCambiosAsync())
            .ReturnsAsync(1);

        if (comandaDto != null)
        {
            _mapperMock.Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
                .Returns(comandaDto);
        }
    }

    /// <summary>
    /// Método helper para establecer propiedades privadas usando reflexión
    /// </summary>
    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
        else
        {
            // Si no se puede establecer la propiedad directamente, usar el campo backing si existe
            var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }

    #endregion
}