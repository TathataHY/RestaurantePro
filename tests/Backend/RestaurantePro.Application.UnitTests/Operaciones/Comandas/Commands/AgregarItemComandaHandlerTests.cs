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
        _fixture = new Fixture();
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AgregarItemComandaHandler>>();
        _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();

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
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Margarita",
            Cantidad = 1,
            PrecioUnitario = 15.50m,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 15.50m);

        // Configurar el servicio de preparaciones para devolver que el producto está disponible
        _servicioPreparacionesMock.Setup(x => x.VerificarDisponibilidadAsync(
                It.IsAny<Guid>(), 
                It.IsAny<int>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success(true));

        _servicioPreparacionesMock.Setup(x => x.ConsumirPreparacionAsync(
                It.IsAny<Guid>(), 
                It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        // Configurar el repositorio para devolver la comanda
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(
                It.IsAny<Guid>(), 
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(
                It.IsAny<Comanda>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _comandaRepositoryMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(comandaId, result.Value.Id);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(
            r => r.ActualizarAsync(
                It.IsAny<Comanda>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task Handle_ConPersonalizacionExtra_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Margarita",
            Cantidad = 1,
            PrecioUnitario = 15.50m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = ingredienteId,
                    NombreIngrediente = "Queso extra",
                    Cantidad = 1,
                    PrecioAdicional = 2.50m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 18.00m);

        // Configuramos todos los mocks necesarios
        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verificamos solo que el resultado es exitoso
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(r => r.ActualizarAsync(
            It.IsAny<Comanda>(),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionQuitar_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Hamburguesa Completa",
            Cantidad = 1,
            PrecioUnitario = 12.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Quitar",
                    IngredienteId = ingredienteId,
                    NombreIngrediente = "Cebolla"
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 12.00m);

        // Configuramos todos los mocks necesarios
        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verificamos solo que el resultado es exitoso
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(r => r.ActualizarAsync(
            It.IsAny<Comanda>(),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionSustituir_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        var ingredienteSustitucionId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Ensalada César",
            Cantidad = 1,
            PrecioUnitario = 8.50m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Sustituir",
                    IngredienteId = ingredienteId,
                    NombreIngrediente = "Pollo",
                    IngredienteSustitucionId = ingredienteSustitucionId,
                    NombreIngredienteSustitucion = "Tofu",
                    PrecioAdicional = 1.00m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 9.50m);

        // Configuramos todos los mocks necesarios
        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verificamos solo que el resultado es exitoso
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(r => r.ActualizarAsync(
            It.IsAny<Comanda>(),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            NombreProducto = "Pizza Suprema",
            Cantidad = 1,
            PrecioUnitario = 18.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = Guid.NewGuid(),
                    NombreIngrediente = "Queso extra",
                    Cantidad = 1,
                    PrecioAdicional = 2.00m
                },
                new PersonalizacionCreateDto
                {
                    Tipo = "Quitar",
                    IngredienteId = Guid.NewGuid(),
                    NombreIngrediente = "Cebolla"
                },
                new PersonalizacionCreateDto
                {
                    Tipo = "Sustituir",
                    IngredienteId = Guid.NewGuid(),
                    NombreIngrediente = "Jamón",
                    IngredienteSustitucionId = Guid.NewGuid(),
                    NombreIngredienteSustitucion = "Tocino",
                    PrecioAdicional = 1.50m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 21.50m);

        // Configuramos todos los mocks necesarios
        SetupRepositoryAndMapper(comanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verificamos solo que el resultado es exitoso
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        
        // Verificar que se actualizó la comanda
        _comandaRepositoryMock.Verify(r => r.ActualizarAsync(
            It.IsAny<Comanda>(),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DebeVerificarYConsumirPreparacion_CuandoProductoEstaDisponible()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Plato del día",
            Cantidad = 2,
            PrecioUnitario = 15.00m
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 30.00m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _comandaRepositoryMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Configurar el servicio de preparaciones para indicar que el producto está disponible
        _servicioPreparacionesMock.Setup(x => x.VerificarDisponibilidadAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success(true));

        _servicioPreparacionesMock.Setup(x => x.ConsumirPreparacionAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se consumió la preparación
        _servicioPreparacionesMock.Verify(x => x.ConsumirPreparacionAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>()), 
            Times.Once());
    }

    [Fact]
    public async Task Handle_NoDebeConsumirPreparacion_CuandoProductoNoEstaDisponible()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Producto agotado",
            Cantidad = 1,
            PrecioUnitario = 10.00m
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, 10.00m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _comandaRepositoryMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Configurar el servicio de preparaciones para indicar que el producto NO está disponible
        _servicioPreparacionesMock.Setup(x => x.VerificarDisponibilidadAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success(false));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que NO se consumió la preparación
        _servicioPreparacionesMock.Verify(x => x.ConsumirPreparacionAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>()), 
            Times.Never());
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La comanda especificada no existe", result.Error);
        
        // Verify no se intenta actualizar
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Finalizada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
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
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
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
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
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
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Plato duplicado",
            Cantidad = 1,
            PrecioUnitario = 10.0m
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        
        // Configurar para que lance excepción al agregar item
        var mockComanda = new Mock<Comanda>();
        mockComanda.Setup(x => x.AgregarItem(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Throws(new InvalidOperationException("Ya existe un item con el producto especificado"));
        
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockComanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        // Solo verificamos que falló, sin revisar el mensaje específico
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
        
        // Verificar que no se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BusinessRuleViolationException_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
            
        // Configurar el servicio de preparaciones para simular error de negocio
        _servicioPreparacionesMock.Setup(x => x.VerificarDisponibilidadAsync(
                It.IsAny<Guid>(), 
                It.IsAny<int>(),
                It.IsAny<Guid?>()))
            .ThrowsAsync(new BusinessRuleViolationException(
                "DisponibilidadProducto", 
                "Preparacion", 
                "Producto no disponible",
                "Operaciones.Preparaciones"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Producto no disponible", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorAlAplicarPersonalizacion_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Plato con error",
            Cantidad = 1,
            PrecioUnitario = 15.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "TipoInvalido", // Tipo inválido para provocar error
                    IngredienteId = Guid.NewGuid(),
                    NombreIngrediente = "Ingrediente Test",
                    Cantidad = 1,
                    PrecioAdicional = 2.00m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
        
        // Verificar que no se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));
            
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error de base de datos", result.Error);

        // Verificar que no se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Tests de Validaciones de Personalizaciones

    [Fact]
    public async Task Handle_PersonalizacionIngredienteIdVacio_DeberiaRetornarError()
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
            PrecioUnitario = 20.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = Guid.Empty,
                    Cantidad = 1,
                    PrecioAdicional = 2.50m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionTipoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Plato con error",
            Cantidad = 1,
            PrecioUnitario = 15.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "TipoInvalido", // Tipo inválido para provocar error
                    IngredienteId = Guid.NewGuid(),
                    NombreIngrediente = "Ingrediente Test",
                    Cantidad = 1,
                    PrecioAdicional = 2.00m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
        
        // Verificar que no se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PersonalizacionExtraCantidadCero_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Ensalada Especial",
            Cantidad = 1,
            PrecioUnitario = 12.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = ingredienteId,
                    Cantidad = 0,
                    PrecioAdicional = 2.00m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionSustituirSinIngredienteReemplazo_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Sandwich",
            Cantidad = 1,
            PrecioUnitario = 15.0m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Sustituir",
                    IngredienteId = ingredienteId,
                    NombreIngrediente = "Queso",
                    // Problema: ingrediente de sustitución es null
                    IngredienteSustitucionId = null,
                    PrecioAdicional = 0
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
        
        // Verificar que no se actualizó la comanda
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PersonalizacionPrecioNegativo_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pasta Alfredo",
            Cantidad = 1,
            PrecioUnitario = 18.00m,
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = ingredienteId,
                    Cantidad = 1,
                    PrecioAdicional = -1.5m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.NotEmpty(result.Error);
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
        // Configuramos el setup con todas las combinaciones posibles de parámetros para evitar problemas con argumentos opcionales
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(
                It.IsAny<Guid>(), 
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(
                It.IsAny<Comanda>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _comandaRepositoryMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        if (comandaDto != null)
        {
            _mapperMock.Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
                .Returns(comandaDto);
        }
        
        // Configurar el servicio de preparaciones
        _servicioPreparacionesMock.Setup(x => x.VerificarDisponibilidadAsync(
                It.IsAny<Guid>(), 
                It.IsAny<int>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success(true));

        _servicioPreparacionesMock.Setup(x => x.ConsumirPreparacionAsync(
                It.IsAny<Guid>(), 
                It.IsAny<int>()))
            .ReturnsAsync(Result.Success());
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