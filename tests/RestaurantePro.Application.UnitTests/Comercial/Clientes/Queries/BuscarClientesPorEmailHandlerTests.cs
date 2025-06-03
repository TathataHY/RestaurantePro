using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Comercial.Clientes.Queries.BuscarClientesPorEmail;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.UnitTests.Common;
using RestaurantePro.Domain.Comercial.Clientes;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Queries;

/// <summary>
/// Tests unitarios para BuscarClientesPorEmailHandler
/// Cobertura completa de búsqueda de clientes, filtros avanzados, paginación y ordenamiento
/// </summary>
public class BuscarClientesPorEmailHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<BuscarClientesPorEmailHandler>> _mockLogger;
    private readonly Mock<DbSet<Cliente>> _mockClientesDbSet;
    private readonly BuscarClientesPorEmailHandler _handler;
    private readonly List<Cliente> _clientesEjemplo;

    public BuscarClientesPorEmailHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<BuscarClientesPorEmailHandler>>();
        _mockClientesDbSet = new Mock<DbSet<Cliente>>();
        _clientesEjemplo = CrearClientesEjemplo();

        ConfigurarMockDbSet();
        ConfigurarMockMapper();
        ConfigurarMockContext();

        _handler = new BuscarClientesPorEmailHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ConBusquedaExactaPorEmail_DeberiaRetornarClienteCorrectamente()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "juan.perez@gmail.com",
            BusquedaExacta = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Items.Should().HaveCount(1);
        resultado.Value.Items.First().Email.Should().Be("juan.perez@gmail.com");
        resultado.Value.TotalCount.Should().Be(1);
        resultado.Value.PageNumber.Should().Be(1);

        // Verificar logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando búsqueda de clientes por email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConBusquedaParcialPorEmail_DeberiaRetornarClientesCoincidentes()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "gmail",
            BusquedaExacta = false,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(3); // juan.perez@gmail.com, maria.garcia@gmail.com, carlos.lopez@gmail.com
        resultado.Value.Items.Should().OnlyContain(c => c.Email.Contains("gmail"));
        resultado.Value.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ConBusquedaPorDominio_DeberiaRetornarClientesDelDominio()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Dominio = "empresa.com",
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(2); // ana.rodriguez@empresa.com, luis.martinez@empresa.com
        resultado.Value.Items.Should().OnlyContain(c => c.Email.Contains("@empresa.com"));
        resultado.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ConEmailInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "inexistente@test.com",
            BusquedaExacta = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().BeEmpty();
        resultado.Value.TotalCount.Should().Be(0);

        // Verificar logging de no encontrados
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No se encontraron clientes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Email", "asc")]
    [InlineData("Email", "desc")]
    [InlineData("Nombre", "asc")]
    [InlineData("FechaCreacion", "desc")]
    public async Task Handle_ConDiferentesOrdenamientos_DeberiaOrdenarCorrectamente(string ordenarPor, string direccion)
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "gmail",
            BusquedaExacta = false,
            OrdenarPor = ordenarPor,
            DireccionOrden = direccion,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().NotBeEmpty();

        // Verificar que los resultados están ordenados
        var emails = resultado.Value.Items.Select(c => c.Email).ToList();
        if (ordenarPor == "Email")
        {
            if (direccion == "asc")
                emails.Should().BeInAscendingOrder();
            else
                emails.Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task Handle_ConPaginacion_DeberiaRetornarPaginaCorrectamente()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "gmail",
            BusquedaExacta = false,
            Pagina = 2,
            TamanoPagina = 2
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.PageNumber.Should().Be(2);
        resultado.Value.PageSize.Should().Be(2);
        resultado.Value.TotalCount.Should().Be(3); // Total de clientes con gmail
        resultado.Value.TotalPages.Should().Be(2); // 3 clientes / 2 por página = 2 páginas
        resultado.Value.Items.Should().HaveCount(1); // Solo 1 cliente en la segunda página
    }

    [Theory]
    [InlineData(1, 5, 5)] // Primera página, 5 elementos
    [InlineData(2, 3, 2)] // Segunda página, 2 elementos restantes
    [InlineData(3, 3, 0)] // Tercera página, sin elementos
    public async Task Handle_ConDiferentesPaginaciones_DeberiaCalcularCorrectamente(int pagina, int tamanoPagina, int elementosEsperados)
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "", // Buscar todos
            BusquedaExacta = false,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(elementosEsperados);
        resultado.Value.PageNumber.Should().Be(pagina);
        resultado.Value.PageSize.Should().Be(tamanoPagina);
    }

    [Fact]
    public async Task Handle_ConBusquedaCaseInsensitive_DeberiaEncontrarCoincidencias()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "JUAN.PEREZ@GMAIL.COM",
            BusquedaExacta = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(1);
        resultado.Value.Items.First().Email.Should().Be("juan.perez@gmail.com");
    }

    [Fact]
    public async Task Handle_ConDominioCaseInsensitive_DeberiaEncontrarCoincidencias()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Dominio = "EMPRESA.COM",
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(2);
        resultado.Value.Items.Should().OnlyContain(c => c.Email.Contains("@empresa.com"));
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorCampoInvalido_DeberiaUsarOrdenamientoPorDefecto()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "gmail",
            BusquedaExacta = false,
            OrdenarPor = "CampoInexistente",
            DireccionOrden = "asc",
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().NotBeEmpty();
        
        // Debería estar ordenado por email (fallback)
        var emails = resultado.Value.Items.Select(c => c.Email).ToList();
        emails.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "test@test.com",
            Pagina = 1,
            TamanoPagina = 10
        };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cancellationToken));
    }

    [Theory]
    [InlineData("", false, 5)] // Búsqueda vacía, todos los clientes
    [InlineData("@", false, 5)] // Búsqueda por @, todos los clientes
    [InlineData("com", false, 5)] // Búsqueda por extensión común
    [InlineData("xyz", false, 0)] // Búsqueda sin coincidencias
    public async Task Handle_ConDiferentesPatronesBusqueda_DeberiaRetornarResultadosCorrectamente(
        string email, bool busquedaExacta, int resultadosEsperados)
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = email,
            BusquedaExacta = busquedaExacta,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(resultadosEsperados);
        resultado.Value.TotalCount.Should().Be(resultadosEsperados);
    }

    [Fact]
    public async Task Handle_ConBusquedaCompleta_DeberiaLoggearInformacionCompleta()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "gmail",
            BusquedaExacta = false,
            Pagina = 1,
            TamanoPagina = 5
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando búsqueda de clientes por email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de completado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Búsqueda completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConEmailYDominioSimultaneos_DeberiaAplicarAmbosFiltros()
    {
        // Arrange
        var query = new BuscarClientesPorEmailQuery
        {
            Email = "ana",
            Dominio = "empresa.com",
            BusquedaExacta = false,
            Pagina = 1,
            TamanoPagina = 10
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().HaveCount(1); // Solo ana.rodriguez@empresa.com
        resultado.Value.Items.First().Email.Should().Be("ana.rodriguez@empresa.com");
    }

    #region Métodos de Apoyo

    private void ConfigurarMockDbSet()
    {
        // Usar el MockDbSetHelper centralizado
        var mockDbSet = MockDbSetHelper.CreateMockDbSet(_clientesEjemplo.AsQueryable());
        
        _mockContext.Setup(c => c.Clientes).Returns(mockDbSet.Object);
    }

    private List<Cliente> CrearClientesEjemplo()
    {
        return new List<Cliente>
        {
            CrearCliente(Guid.NewGuid(), "juan.perez@gmail.com", "555-1001", DateTime.Now.AddDays(-30)),
            CrearCliente(Guid.NewGuid(), "maria.garcia@gmail.com", "555-1002", DateTime.Now.AddDays(-25)),
            CrearCliente(Guid.NewGuid(), "carlos.lopez@gmail.com", "555-1003", DateTime.Now.AddDays(-20)),
            CrearCliente(Guid.NewGuid(), "ana.rodriguez@empresa.com", "555-1004", DateTime.Now.AddDays(-15)),
            CrearCliente(Guid.NewGuid(), "luis.martinez@empresa.com", "555-1005", DateTime.Now.AddDays(-10))
        };
    }

    private Cliente CrearCliente(Guid id, string email, string telefono, DateTime fechaCreacion)
    {
        // Usar reflection para crear el cliente con propiedades privadas
        var cliente = (Cliente)Activator.CreateInstance(typeof(Cliente), true)!;
        
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, id);
        typeof(Cliente).GetProperty("Email")?.SetValue(cliente, Email.Create(email));
        typeof(Cliente).GetProperty("Telefono")?.SetValue(cliente, PhoneNumber.Create(telefono));
        typeof(Cliente).GetProperty("FechaCreacion")?.SetValue(cliente, fechaCreacion);
        typeof(Cliente).GetProperty("PuntosAcumulados")?.SetValue(cliente, 100);
        typeof(Cliente).GetProperty("CantidadVisitas")?.SetValue(cliente, 5);
        typeof(Cliente).GetProperty("Segmento")?.SetValue(cliente, SegmentoCliente.Regular);
        
        return cliente;
    }

    private void ConfigurarMockMapper()
    {
        _mockMapper.Setup(m => m.Map<Cliente, ClienteSummaryDto>(It.IsAny<Cliente>()))
            .Returns((Cliente cliente) => new ClienteSummaryDto
            {
                Id = cliente.Id,
                NombreCompleto = "Cliente Test", // Simulamos el nombre completo
                Email = cliente.Email.Value,
                Telefono = cliente.Telefono.Value,
                TipoCliente = cliente.Segmento.ToString(),
                Activo = true,
                FechaRegistro = cliente.FechaCreacion,
                RegistradoPor = "Sistema",
                FechaNacimiento = null,
                Ciudad = "",
                Pais = "",
                PuntosFidelizacion = cliente.PuntosAcumulados,
                NivelFidelizacion = "Regular",
                TotalOrdenes = cliente.CantidadVisitas,
                MontoTotalCompras = 1000m,
                FechaUltimaOrden = cliente.FechaCreacion,
                PromedioCompra = 100m,
                EsFrecuente = cliente.CantidadVisitas > 5,
                DiasSinVisitar = 15,
                EsVIP = cliente.Segmento == SegmentoCliente.Premium
            });
    }

    private void ConfigurarMockContext()
    {
        // Ya configurado en ConfigurarMockDbSet()
    }

    #endregion
} 