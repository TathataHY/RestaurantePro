using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.SharedKernel;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;

namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

/// <summary>
/// Tests unitarios para AnularFacturaHandler
/// Valida la lógica completa de anulación de facturas con procesos empresariales
/// </summary>
public class AnularFacturaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AnularFacturaHandler>> _loggerMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationService> _notificacionServiceMock;
    private Mock<DbSet<Factura>> _facturaDbSetMock;
    private Mock<DbSet<Usuario>> _usuarioDbSetMock;
    private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly AnularFacturaHandler _handler;
    
    public AnularFacturaHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AnularFacturaHandler>>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificacionServiceMock = new Mock<INotificationService>();
        _facturaRepositoryMock = new Mock<IFacturaRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        
        // Inicializar DbSets
        _facturaDbSetMock = new Mock<DbSet<Factura>>();
        _usuarioDbSetMock = new Mock<DbSet<Usuario>>();
        _contextMock.Setup(c => c.Facturas).Returns(_facturaDbSetMock.Object);
        _contextMock.Setup(c => c.Usuarios).Returns(_usuarioDbSetMock.Object);
        
        // Configurar fecha actual para pruebas
        _dateTimeServiceMock.Setup(d => d.Now).Returns(new DateTime(2023, 1, 1, 10, 0, 0));
        _dateTimeServiceMock.Setup(d => d.UtcNow).Returns(new DateTime(2023, 1, 1, 15, 0, 0));
        
        // Usuario actual por defecto
        _currentUserServiceMock.Setup(c => c.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserServiceMock.Setup(c => c.Rol).Returns("Cajero");
        
        // Crear handler
        _handler = CreateHandler();
    }

    private AnularFacturaHandler CreateHandler()
    {
        return new AnularFacturaHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object,
            _notificacionServiceMock.Object
        );
    }

    private Factura CrearFacturaConDetalles(string numeroFactura = "F-001", TipoFactura tipoFactura = TipoFactura.Normal, string nombreCliente = "Cliente Test")
    {
        var factura = Factura.Crear(
            numeroFactura: numeroFactura,
            tipoFactura: tipoFactura,
            nombreCliente: nombreCliente,
            observaciones: "Factura para test"
        );
        
        // Agregar al menos un detalle para poder emitir la factura
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto Test",
            cantidad: 1,
            precioUnitario: 100.0m,
            porcentajeImpuesto: 16.0m,
            porcentajeDescuento: 0
        );
        
        return factura;
    }

    [Fact]
    public async Task Handle_AnulacionNormalSimple_DeberiaAnularFacturaExitosamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Error administrativo",
            TipoAnulacion = "Normal"
        };
        
        // Crear factura real en lugar de un mock, ahora con detalles
        var factura = CrearFacturaConDetalles();
        
        // Usar reflection para establecer el ID de la factura para coincidir con el comando
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, facturaId);
        
        // Emitir la factura para que pueda ser anulada
        factura.Emitir(_dateTimeServiceMock.Object);
        
        var facturas = new List<Factura> { factura };
        SetupFacturaDbSet(facturas, facturaId);
        
        // Mock de mapeo para FacturaDto
        var facturaDto = CreateMockFacturaDto(facturaId);
        _mapperMock.Setup(m => m.Map<FacturaDto>(It.IsAny<Factura>())).Returns(facturaDto);
        
        // Configurar SaveChangesAsync para devolver 1 (una fila afectada)
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(facturaId, result.Value.Id);
        Assert.Equal(EstadoFactura.Anulada, result.Value.Estado);
    }
    
    [Fact]
    public async Task Handle_AnulacionConAprobacionGerencia_DeberiaValidarGerente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioGerenteId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Anulación que requiere aprobación",
            TipoAnulacion = "ErrorSistema",
            RequiereAprobacionGerencia = true,
            GerenteAprobadorId = usuarioGerenteId
        };

        var factura = CrearFacturaConDetalles();
        factura.Emitir(); // Necesitamos una factura emitida para poder anularla
        AsignarId(factura, facturaId);

        // Crear un usuario gerente con un formato de email válido
        var usuarioGerente = Usuario.Crear(
            nombreUsuario: "gerente", 
            nombreCompleto: "Gerente Test", 
            email: "gerente@test.com", 
            rol: RolUsuario.Gerente
        );
        AsignarId(usuarioGerente, usuarioGerenteId);

        // Configurar mocks
        _facturaDbSetMock = BuildMockDbSet(new List<Factura> { factura });
        _usuarioDbSetMock = BuildMockDbSet(new List<Usuario> { usuarioGerente });

        _contextMock.Setup(c => c.Facturas).Returns(_facturaDbSetMock.Object);
        _contextMock.Setup(c => c.Usuarios).Returns(_usuarioDbSetMock.Object);

        _facturaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
        _usuarioRepositoryMock.Setup(r => r.ObtenerPorIdAsync(usuarioGerenteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioGerente);

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("aprobada por gerente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_RequiereAprobacionSinGerente_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioGerenteId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Anulación que requiere aprobación",
            TipoAnulacion = "ErrorSistema",
            RequiereAprobacionGerencia = true,
            GerenteAprobadorId = usuarioGerenteId
        };

        var factura = CrearFacturaConDetalles();
        factura.Emitir(); // Necesitamos una factura emitida para poder anularla
        AsignarId(factura, facturaId);

        // Crear un usuario NON-gerente con email válido
        var usuarioNoGerente = Usuario.Crear(
            nombreUsuario: "cajero", 
            nombreCompleto: "Cajero Test", 
            email: "cajero@test.com", 
            rol: RolUsuario.Cajero
        );
        AsignarId(usuarioNoGerente, usuarioGerenteId);

        // Configurar mocks
        _facturaDbSetMock = BuildMockDbSet(new List<Factura> { factura });
        _usuarioDbSetMock = BuildMockDbSet(new List<Usuario> { usuarioNoGerente });

        _contextMock.Setup(c => c.Facturas).Returns(_facturaDbSetMock.Object);
        _contextMock.Setup(c => c.Usuarios).Returns(_usuarioDbSetMock.Object);

        _facturaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
        _usuarioRepositoryMock.Setup(r => r.ObtenerPorIdAsync(usuarioGerenteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioNoGerente);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("aprobador no es gerente", result.Error);
    }
    
    [Fact]
    public async Task Handle_AnulacionConcurrente_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Error administrativo",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Normal"
        };
        
        // Crear factura real en lugar de un mock, ahora con detalles
        var factura = CrearFacturaConDetalles();
        
        // Usar reflection para establecer el ID de la factura para coincidir con el comando
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, facturaId);
        
        // Emitir y luego anular la factura para simular una factura ya anulada
        factura.Emitir(_dateTimeServiceMock.Object);
        factura.Anular("Anulación previa", _dateTimeServiceMock.Object);
        
        var facturas = new List<Factura> { factura };
        SetupFacturaDbSet(facturas, facturaId);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Succeeded);
        string errorLower = result.Error?.ToLower() ?? string.Empty;
        Assert.Contains("ya ha sido anulada", errorLower);
    }
    
    [Fact]
    public async Task Handle_AnulacionConNotaCredito_DeberiaLoggearGeneracion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Emitir nota de crédito",
            TipoAnulacion = "NotaCredito",
            GenerarNotaCredito = true
        };
        
        // Crear factura real en lugar de un mock, ahora con detalles
        var factura = CrearFacturaConDetalles();
        
        // Usar reflection para establecer el ID de la factura para coincidir con el comando
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, facturaId);
        
        // Emitir la factura para que pueda ser anulada
        factura.Emitir(_dateTimeServiceMock.Object);
        
        var facturas = new List<Factura> { factura };
        SetupFacturaDbSet(facturas, facturaId);
        
        // Mock de mapeo para FacturaDto
        var facturaDto = CreateMockFacturaDto(facturaId);
        _mapperMock.Setup(m => m.Map<FacturaDto>(It.IsAny<Factura>())).Returns(facturaDto);
        
        // Configurar SaveChangesAsync para devolver 1 (una fila afectada)
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se loggeó la generación de nota de crédito
        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("nota de crédito")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Handle_AnulacionConDevolucionPago_DeberiaLoggearProceso()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Devolución de pago",
            TipoAnulacion = "DevolucionPago",
            ProcesarDevolucionPago = true,
            MetodoDevolucion = "Efectivo"
        };
        
        // Crear factura real en lugar de un mock, ahora con detalles
        var factura = CrearFacturaConDetalles();
        
        // Usar reflection para establecer el ID de la factura para coincidir con el comando
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, facturaId);
        
        // Emitir la factura para que pueda ser anulada
        factura.Emitir(_dateTimeServiceMock.Object);
        
        var facturas = new List<Factura> { factura };
        SetupFacturaDbSet(facturas, facturaId);
        
        // Mock de mapeo para FacturaDto
        var facturaDto = CreateMockFacturaDto(facturaId);
        _mapperMock.Setup(m => m.Map<FacturaDto>(It.IsAny<Factura>())).Returns(facturaDto);
        
        // Configurar SaveChangesAsync para devolver 1 (una fila afectada)
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se loggeó el proceso de devolución de pago
        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Devoluciones de pagos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    
    private void SetupFacturaDbSet(List<Factura> facturas, Guid facturaId)
    {
        // Configurar el DbSet mock
        _facturaDbSetMock = facturas.AsQueryable().BuildMockDbSet();
        
        // Configurar FindAsync para devolver la factura correspondiente
        _facturaDbSetMock.Setup(d => d.FindAsync(
                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == facturaId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(facturas.FirstOrDefault(f => f.Id == facturaId));
        
        // Asignar el DbSet mock al contexto
        _contextMock.Setup(c => c.Facturas).Returns(_facturaDbSetMock.Object);
    }
    
    private void SetupUsuarioDbSet(List<Usuario> usuarios, Guid usuarioId)
    {
        // Configurar el DbSet mock
        _usuarioDbSetMock = usuarios.AsQueryable().BuildMockDbSet();
        
        // Configurar FindAsync para devolver el usuario correspondiente
        _usuarioDbSetMock.Setup(d => d.FindAsync(
                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == usuarioId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios.FirstOrDefault(u => u.Id == usuarioId));
        
        // Asignar el DbSet mock al contexto
        _contextMock.Setup(c => c.Usuarios).Returns(_usuarioDbSetMock.Object);
    }
    
    private FacturaDto CreateMockFacturaDto(Guid id)
    {
        // En lugar de hacer mock, creamos un objeto real
        return new FacturaDto
        {
            Id = id,
            Numero = "F-TEST-001",
            Estado = EstadoFactura.Anulada,
            FechaEmision = DateTime.Now.AddDays(-1),
            Total = 100.0m,
            Subtotal = 85.0m,
            Impuestos = 15.0m
        };
    }

    // Método para crear mock de DbSet para pruebas
    private Mock<DbSet<T>> BuildMockDbSet<T>(List<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());
        mockDbSet.Setup(d => d.Add(It.IsAny<T>())).Returns((T entity) => entity);

        // Para FindAsync
        mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] ids) => {
                var id = ids[0];
                return data.FirstOrDefault(d => GetEntityId(d).Equals(id));
            });

        return mockDbSet;
    }

    // Método auxiliar para obtener el Id de una entidad mediante reflection
    private Guid GetEntityId<T>(T entity)
    {
        var property = typeof(T).GetProperty("Id");
        return (Guid)property.GetValue(entity);
    }

    // Método para asignar Id a entidades que tienen propiedades de solo lectura
    private void AsignarId<T>(T entidad, Guid id) where T : class
    {
        // Usar reflection para establecer el ID aunque sea de solo lectura
        var field = typeof(T).BaseType.GetField("<Id>k__BackingField", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
        {
            field.SetValue(entidad, id);
        }
    }
}

public static class QueryableExtensions
{
    public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
    {
        var mock = new Mock<DbSet<T>>();
        
        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        return mock;
    }
} 

