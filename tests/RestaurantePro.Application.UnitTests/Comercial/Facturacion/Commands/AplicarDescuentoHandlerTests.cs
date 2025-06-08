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
using RestaurantePro.Application.Comercial.Facturacion.Commands.AplicarDescuento;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Application.UnitTests.Common;
using MockQueryable.Moq;

namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

/// <summary>
/// Tests unitarios para AplicarDescuentoHandler
/// Valida la lógica completa de aplicación de descuentos con múltiples tipos y validaciones
/// </summary>
public class AplicarDescuentoHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AplicarDescuentoHandler>> _loggerMock;
    private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AplicarDescuentoHandler _handler;

    public AplicarDescuentoHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AplicarDescuentoHandler>>();
        _servicioFacturacionMock = new Mock<IServicioFacturacion>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        
        // Crear el handler con los mocks
        _handler = new AplicarDescuentoHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _servicioFacturacionMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public void CrearDescuentoPorcentaje_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var porcentaje = 10m;
        var concepto = "Descuento por cliente frecuente";
        var motivo = "Cliente con más de 5 compras";

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoPorcentaje(
            facturaId: facturaId,
            porcentaje: porcentaje,
            concepto: concepto,
            motivo: motivo,
            usuarioAutorizaId: usuarioId);

        // Assert
        Assert.Equal("General", command.TipoDescuento);
        Assert.Equal(porcentaje, command.Porcentaje);
        Assert.Equal(concepto, command.Concepto);
        Assert.Equal(motivo, command.Motivo);
        Assert.Equal(usuarioId, command.UsuarioAutorizaId);
    }

    [Fact]
    public async Task Handle_DescuentoPorcentajeSimple_DeberiaAplicarDescuentoExitosamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m, // 10% de 1000 = 100
            Concepto = "Descuento de prueba",
            Motivo = "Test unitario",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _servicioFacturacionMock.Verify(x => x.AplicarDescuentoAsync(
            facturaId, "General", 100m, "Descuento de prueba", "Test unitario", usuarioId, true, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m,
            Concepto = "Descuento test",
            Motivo = "Test",
            UsuarioAutorizaId = Guid.NewGuid()
        };
        
        // Configurar base de datos vacía - no hay factura
        SetupFacturasDbSet(new List<Factura>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La factura especificada no existe", result.Error ?? string.Empty);
    }

    #region Helper Methods - Setup

    private void SetupFacturasDbSet(List<Factura> facturas)
    {
        // Crear un IQueryable<Factura> a partir de la lista y usar la extensión BuildMockDbSet directamente
        var mockDbSet = facturas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Facturas).Returns(mockDbSet.Object);
    }

    #endregion

    #region Helper Methods - Data Creation

    private static Factura CreateFacturaEmitida(Guid id, decimal total = 1000.00m, decimal subtotal = 800.00m)
    {
        // Crear factura real usando el factory method
        var factura = Factura.Crear(
            numeroFactura: $"FAC-{id.ToString().Substring(0, 8)}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test"
        );
        
        // Agregar detalles para llegar al total deseado
        var precioUnitario = subtotal / 2; // 2 items para alcanzar el subtotal
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto Test 1",
            cantidad: 1,
            precioUnitario: precioUnitario,
            porcentajeImpuesto: 18m
        );
        
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto Test 2",
            cantidad: 1,
            precioUnitario: precioUnitario,
            porcentajeImpuesto: 18m
        );
        
        // Emitir la factura para que esté en estado válido
        factura.Emitir();
        
        // Usar reflexión para establecer el Id si es necesario
        if (factura.Id != id)
        {
            var idProperty = typeof(EntityBase).GetProperty("Id");
            idProperty?.SetValue(factura, id);
        }
        
        return factura;
    }

    private static FacturaDto CreateMockFacturaDto(Guid id)
    {
        return new FacturaDto
        {
            Id = id,
            Numero = $"FAC-{id.ToString().Substring(0, 8)}",
            Estado = EstadoFactura.Emitida,
            FechaEmision = DateTime.UtcNow.AddDays(-1),
            NombreCliente = "Cliente Test",
            Total = 1000.00m,
            Subtotal = 862.07m,
            Impuestos = 137.93m,
            Descuentos = 50.00m // Con descuento aplicado
        };
    }

    #endregion
} 