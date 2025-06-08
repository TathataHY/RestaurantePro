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
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.SharedKernel;

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
    private readonly AnularFacturaHandler _handler;
    private readonly Mock<DbSet<Factura>> _facturaDbSetMock;
    private readonly Mock<DbSet<Usuario>> _usuarioDbSetMock;
    
    public AnularFacturaHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AnularFacturaHandler>>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificacionServiceMock = new Mock<INotificationService>();
        
        _facturaDbSetMock = new Mock<DbSet<Factura>>();
        _usuarioDbSetMock = new Mock<DbSet<Usuario>>();
        
        _contextMock.Setup(c => c.Facturas).Returns(_facturaDbSetMock.Object);
        _contextMock.Setup(c => c.Usuarios).Returns(_usuarioDbSetMock.Object);
        
        _handler = new AnularFacturaHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object,
            _notificacionServiceMock.Object);
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
            porcentajeDescuento: 0.0m
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
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(facturaDto, result.Value);
    }
    
    [Fact]
    public async Task Handle_AnulacionConAprobacionGerencia_DeberiaValidarGerente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Error administrativo",
            UsuarioAutorizaId = gerenteId,
            RequiereAprobacionGerencia = true,
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
        
        // Crear gerente
        var gerente = Usuario.Crear("Gerente Test", "gerente@test.com", "gerente@test.com", RolUsuario.Gerente);
        
        // Usar reflection para establecer el ID del gerente
        typeof(EntityBase).GetProperty("Id")?.SetValue(gerente, gerenteId);
        
        var usuarios = new List<Usuario> { gerente };
        SetupUsuarioDbSet(usuarios, gerenteId);
        
        // Mock de mapeo para FacturaDto
        var facturaDto = CreateMockFacturaDto(facturaId);
        _mapperMock.Setup(m => m.Map<FacturaDto>(It.IsAny<Factura>())).Returns(facturaDto);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(facturaDto, result.Value);
    }
    
    [Fact]
    public async Task Handle_RequiereAprobacionSinGerente_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Error administrativo",
            UsuarioAutorizaId = usuarioId,
            RequiereAprobacionGerencia = true,
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
        
        // Crear usuario que no es gerente
        var usuario = Usuario.Crear("Usuario Test", "usuario@test.com", "usuario@test.com", RolUsuario.Cajero);
        
        // Usar reflection para establecer el ID del usuario
        typeof(EntityBase).GetProperty("Id")?.SetValue(usuario, usuarioId);
        
        var usuarios = new List<Usuario> { usuario };
        SetupUsuarioDbSet(usuarios, usuarioId);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Succeeded);
        string errorLower = result.Error?.ToLower() ?? string.Empty;
        Assert.Contains("gerente", errorLower);
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
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        // Verificar que se loggeó la generación de nota de crédito
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("nota de crédito")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Succeeded);
        // Verificar que se loggeó la devolución de pago
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("devolución")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
    
    private void SetupFacturaDbSet(List<Factura> facturas, Guid facturaId)
    {
        var queryableFacturas = facturas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Facturas).Returns(queryableFacturas.Object);
        _contextMock.Setup(c => c.Facturas.FindAsync(new object[] { facturaId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facturas.FirstOrDefault(f => f.Id == facturaId));
    }
    
    private void SetupUsuarioDbSet(List<Usuario> usuarios, Guid usuarioId)
    {
        var queryableUsuarios = usuarios.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Usuarios).Returns(queryableUsuarios.Object);
        _contextMock.Setup(c => c.Usuarios.FindAsync(new object[] { usuarioId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios.FirstOrDefault(u => u.Id == usuarioId));
    }
    
    private FacturaDto CreateMockFacturaDto(Guid id)
    {
        var dto = new FacturaDto
        {
            Id = id,
            Estado = EstadoFactura.Anulada,
            // Asignar número en el constructor en lugar de la propiedad readonly
            Numero = "123456"
        };
        
        return dto;
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