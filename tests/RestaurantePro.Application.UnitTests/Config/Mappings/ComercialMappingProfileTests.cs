namespace RestaurantePro.Application.UnitTests.Config.Mappings;

/// <summary>
/// Tests unitarios para ComercialMappingProfile
/// Cobertura completa de mapeos de Cliente y Facturación, validación de value objects y edge cases
/// </summary>
public class ComercialMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public ComercialMappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ComercialMappingProfile>();
        });
        
        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_DeberiaSerValida()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    #region Cliente Mappings Tests

    [Fact]
    public void Map_ClienteToClienteDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var cliente = CrearClienteEjemplo();

        // Act
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(cliente.Id);
        dto.Nombre.Should().Be(cliente.Nombre.Nombre);
        dto.Apellido.Should().Be(cliente.Nombre.Apellido);
        dto.Email.Should().Be(cliente.Email.Value);
        dto.Telefono.Should().Be(cliente.Telefono.Value);
        dto.Activo.Should().Be(cliente.EstaActivo);
    }

    [Fact]
    public void Map_ClienteToClienteSummaryDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var cliente = CrearClienteEjemplo();

        // Act
        var dto = _mapper.Map<ClienteSummaryDto>(cliente);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(cliente.Id);
        dto.NombreCompleto.Should().Be($"{cliente.Nombre.Nombre} {cliente.Nombre.Apellido}".Trim());
        dto.Email.Should().Be(cliente.Email.Value);
        dto.Telefono.Should().Be(cliente.Telefono.Value);
        dto.Activo.Should().Be(cliente.EstaActivo);
    }

    [Fact]
    public void Map_ClienteToClienteSummaryDto_ConNombreSolo_DeberiaMapearCorrectamente()
    {
        // Arrange
        var cliente = CrearClienteEjemplo();
        var nombreMock = CrearNombreMock("Juan", "");
        typeof(Cliente).GetProperty("Nombre")?.SetValue(cliente, nombreMock);

        // Act
        var dto = _mapper.Map<ClienteSummaryDto>(cliente);

        // Assert
        dto.NombreCompleto.Should().Be("Juan");
    }

    [Fact]
    public void Map_ClienteToClienteSummaryDto_ConApellidoSolo_DeberiaMapearCorrectamente()
    {
        // Arrange
        var cliente = CrearClienteEjemplo();
        var nombreMock = CrearNombreMock("", "Pérez");
        typeof(Cliente).GetProperty("Nombre")?.SetValue(cliente, nombreMock);

        // Act
        var dto = _mapper.Map<ClienteSummaryDto>(cliente);

        // Assert
        dto.NombreCompleto.Should().Be("Pérez");
    }

    [Fact]
    public void Map_ClienteCreateDtoToCrearClienteCommand_DeberiaMapearCorrectamente()
    {
        // Arrange
        var createDto = new ClienteCreateDto
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan.perez@email.com",
            Telefono = "555-1234",
            FechaNacimiento = DateTime.Now.AddYears(-30),
            Direccion = "Calle 123",
            Ciudad = "Ciudad de México",
            CodigoPostal = "12345"
        };

        // Act
        var command = _mapper.Map<CrearClienteCommand>(createDto);

        // Assert
        command.Should().NotBeNull();
        command.Nombre.Should().Be(createDto.Nombre);
        command.Apellido.Should().Be(createDto.Apellido);
        command.Email.Should().Be(createDto.Email);
        command.Telefono.Should().Be(createDto.Telefono);
        command.FechaNacimiento.Should().Be(createDto.FechaNacimiento);
        command.Direccion.Should().Be(createDto.Direccion);
        command.Ciudad.Should().Be(createDto.Ciudad);
        command.CodigoPostal.Should().Be(createDto.CodigoPostal);
    }

    [Fact]
    public void Map_ClienteUpdateDtoToActualizarClienteCommand_DeberiaMapearCorrectamente()
    {
        // Arrange
        var updateDto = new ClienteUpdateDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Juan Carlos",
            Apellido = "Pérez García",
            Email = "juan.carlos@email.com",
            Telefono = "555-5678",
            FechaNacimiento = DateTime.Now.AddYears(-35),
            Direccion = "Avenida 456",
            Ciudad = "Guadalajara",
            CodigoPostal = "54321"
        };

        // Act
        var command = _mapper.Map<ActualizarClienteCommand>(updateDto);

        // Assert
        command.Should().NotBeNull();
        command.Nombre.Should().Be(updateDto.Nombre);
        command.Apellido.Should().Be(updateDto.Apellido);
        command.Email.Should().Be(updateDto.Email);
        command.Telefono.Should().Be(updateDto.Telefono);
        command.FechaNacimiento.Should().Be(updateDto.FechaNacimiento);
        command.Direccion.Should().Be(updateDto.Direccion);
        command.Ciudad.Should().Be(updateDto.Ciudad);
        command.CodigoPostal.Should().Be(updateDto.CodigoPostal);
        // El ID se ignora en el mapeo según configuración
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Map_ClienteToDto_ConDiferentesEstadosActivo_DeberiaMapearCorrectamente(bool estaActivo, bool expectedActivo)
    {
        // Arrange
        var cliente = CrearClienteEjemplo();
        typeof(Cliente).GetProperty("EstaActivo")?.SetValue(cliente, estaActivo);

        // Act
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Assert
        dto.Activo.Should().Be(expectedActivo);
    }

    #endregion

    #region Facturación Mappings Tests

    [Fact]
    public void Map_FacturaToFacturaDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var factura = CrearFacturaEjemplo();

        // Act
        var dto = _mapper.Map<FacturaDto>(factura);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(factura.Id);
        dto.NumeroFactura.Should().Be(factura.Numero.Value);
        dto.FechaEmision.Should().Be(factura.FechaEmision);
        dto.FechaVencimiento.Should().Be(factura.FechaVencimiento);
        dto.EstadoTexto.Should().Be(factura.Estado.ToString());
        dto.TipoFacturaTexto.Should().Be(factura.TipoFactura.ToString());
        dto.SubTotal.Should().Be(factura.SubTotal.Amount);
        dto.TotalImpuestos.Should().Be(factura.TotalImpuestos.Amount);
        dto.Total.Should().Be(factura.Total.Amount);
        dto.EstaPagada.Should().Be(factura.EstaPagada);
        dto.EstaPendiente.Should().Be(factura.EstaPendiente);
        dto.EstaVencida.Should().Be(factura.EstaVencida);
        dto.TieneSaldo.Should().Be(factura.TieneSaldo);
        dto.Saldo.Should().Be(factura.Saldo.Amount);
    }

    [Theory]
    [InlineData(EstadoFactura.Borrador, "Borrador")]
    [InlineData(EstadoFactura.Emitida, "Emitida")]
    [InlineData(EstadoFactura.Pagada, "Pagada")]
    [InlineData(EstadoFactura.Anulada, "Anulada")]
    [InlineData(EstadoFactura.Vencida, "Vencida")]
    public void Map_FacturaToDto_ConDiferentesEstados_DeberiaMapearTextoCorrectamente(EstadoFactura estado, string expectedTexto)
    {
        // Arrange
        var factura = CrearFacturaEjemplo();
        typeof(Factura).GetProperty("Estado")?.SetValue(factura, estado);

        // Act
        var dto = _mapper.Map<FacturaDto>(factura);

        // Assert
        dto.EstadoTexto.Should().Be(expectedTexto);
    }

    [Theory]
    [InlineData(TipoFactura.Venta, "Venta")]
    [InlineData(TipoFactura.Devolucion, "Devolucion")]
    [InlineData(TipoFactura.NotaCredito, "NotaCredito")]
    [InlineData(TipoFactura.NotaDebito, "NotaDebito")]
    public void Map_FacturaToDto_ConDiferentesTipos_DeberiaMapearTextoCorrectamente(TipoFactura tipo, string expectedTexto)
    {
        // Arrange
        var factura = CrearFacturaEjemplo();
        typeof(Factura).GetProperty("TipoFactura")?.SetValue(factura, tipo);

        // Act
        var dto = _mapper.Map<FacturaDto>(factura);

        // Assert
        dto.TipoFacturaTexto.Should().Be(expectedTexto);
    }

    [Fact]
    public void Map_DetalleFacturaToDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var detalle = CrearDetalleFacturaEjemplo();

        // Act
        var dto = _mapper.Map<DetalleFacturaDto>(detalle);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(detalle.Id);
        dto.ProductoNombre.Should().Be(detalle.ProductoNombre);
        dto.Cantidad.Should().Be(detalle.Cantidad);
        dto.PrecioUnitario.Should().Be(detalle.PrecioUnitario.Amount);
        dto.Subtotal.Should().Be(detalle.Subtotal.Amount);
    }

    [Fact]
    public void Map_DescuentoFacturaToDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var descuento = CrearDescuentoFacturaEjemplo();

        // Act
        var dto = _mapper.Map<DescuentoFacturaDto>(descuento);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(descuento.Id);
        dto.TipoDescuento.Should().Be(descuento.TipoDescuento.ToString());
        dto.Valor.Should().Be(descuento.Valor.Amount);
        dto.Concepto.Should().Be(descuento.Concepto);
        dto.FechaAplicacion.Should().Be(descuento.FechaAplicacion);
    }

    [Theory]
    [InlineData(TipoDescuento.Porcentaje, "Porcentaje")]
    [InlineData(TipoDescuento.MontoFijo, "MontoFijo")]
    [InlineData(TipoDescuento.Promocional, "Promocional")]
    public void Map_DescuentoFacturaToDto_ConDiferentesTipos_DeberiaMapearTextoCorrectamente(TipoDescuento tipo, string expectedTexto)
    {
        // Arrange
        var descuento = CrearDescuentoFacturaEjemplo();
        typeof(DescuentoFactura).GetProperty("TipoDescuento")?.SetValue(descuento, tipo);

        // Act
        var dto = _mapper.Map<DescuentoFacturaDto>(descuento);

        // Assert
        dto.TipoDescuento.Should().Be(expectedTexto);
    }

    #endregion

    #region Edge Cases y Null Handling

    [Fact]
    public void Map_ClienteNull_DeberiaRetornarNull()
    {
        // Arrange
        Cliente? cliente = null;

        // Act
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_FacturaNull_DeberiaRetornarNull()
    {
        // Arrange
        Factura? factura = null;

        // Act
        var dto = _mapper.Map<FacturaDto>(factura);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ListaClientes_DeberiaMapearTodos()
    {
        // Arrange
        var clientes = new List<Cliente>
        {
            CrearClienteEjemplo(),
            CrearClienteEjemplo(),
            CrearClienteEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<ClienteDto>>(clientes);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaFacturas_DeberiaMapearTodas()
    {
        // Arrange
        var facturas = new List<Factura>
        {
            CrearFacturaEjemplo(),
            CrearFacturaEjemplo(),
            CrearFacturaEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<FacturaDto>>(facturas);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var clientes = new List<Cliente>();

        // Act
        var dtos = _mapper.Map<List<ClienteDto>>(clientes);

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Map_ClienteToDto_DeberiaSerRapido()
    {
        // Arrange
        var cliente = CrearClienteEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<ClienteDto>(cliente);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    [Fact]
    public void Map_FacturaToDto_DeberiaSerRapido()
    {
        // Arrange
        var factura = CrearFacturaEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<FacturaDto>(factura);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    #endregion

    #region Helper Methods

    private Cliente CrearClienteEjemplo()
    {
        // Usar reflection para crear cliente con propiedades privadas
        var cliente = (Cliente)Activator.CreateInstance(typeof(Cliente), true)!;
        
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, Guid.NewGuid());
        typeof(Cliente).GetProperty("EstaActivo")?.SetValue(cliente, true);
        
        // Crear value objects mock
        var nombre = CrearNombreMock("Juan", "Pérez");
        var email = CrearEmailMock("juan.perez@email.com");
        var telefono = CrearTelefonoMock("555-1234");
        
        typeof(Cliente).GetProperty("Nombre")?.SetValue(cliente, nombre);
        typeof(Cliente).GetProperty("Email")?.SetValue(cliente, email);
        typeof(Cliente).GetProperty("Telefono")?.SetValue(cliente, telefono);
        
        return cliente;
    }

    private Factura CrearFacturaEjemplo()
    {
        // Usar reflection para crear factura con propiedades privadas
        var factura = (Factura)Activator.CreateInstance(typeof(Factura), true)!;
        
        typeof(Factura).GetProperty("Id")?.SetValue(factura, Guid.NewGuid());
        typeof(Factura).GetProperty("FechaEmision")?.SetValue(factura, DateTime.UtcNow);
        typeof(Factura).GetProperty("FechaVencimiento")?.SetValue(factura, DateTime.UtcNow.AddDays(30));
        typeof(Factura).GetProperty("Estado")?.SetValue(factura, EstadoFactura.Emitida);
        typeof(Factura).GetProperty("TipoFactura")?.SetValue(factura, TipoFactura.Venta);
        typeof(Factura).GetProperty("EstaPagada")?.SetValue(factura, false);
        typeof(Factura).GetProperty("EstaPendiente")?.SetValue(factura, true);
        typeof(Factura).GetProperty("EstaVencida")?.SetValue(factura, false);
        typeof(Factura).GetProperty("TieneSaldo")?.SetValue(factura, true);
        
        // Crear value objects mock
        var numero = CrearNumeroFacturaMock("FAC-001");
        var subtotal = CrearMoneyMock(100.00m);
        var impuestos = CrearMoneyMock(16.00m);
        var total = CrearMoneyMock(116.00m);
        var saldo = CrearMoneyMock(116.00m);
        
        typeof(Factura).GetProperty("Numero")?.SetValue(factura, numero);
        typeof(Factura).GetProperty("SubTotal")?.SetValue(factura, subtotal);
        typeof(Factura).GetProperty("TotalImpuestos")?.SetValue(factura, impuestos);
        typeof(Factura).GetProperty("Total")?.SetValue(factura, total);
        typeof(Factura).GetProperty("Saldo")?.SetValue(factura, saldo);
        
        return factura;
    }

    private DetalleFactura CrearDetalleFacturaEjemplo()
    {
        // Usar reflection para crear detalle con propiedades privadas
        var detalle = (DetalleFactura)Activator.CreateInstance(typeof(DetalleFactura), true)!;
        
        typeof(DetalleFactura).GetProperty("Id")?.SetValue(detalle, Guid.NewGuid());
        typeof(DetalleFactura).GetProperty("ProductoNombre")?.SetValue(detalle, "Pizza Margherita");
        typeof(DetalleFactura).GetProperty("Cantidad")?.SetValue(detalle, 2);
        
        // Crear value objects mock
        var precioUnitario = CrearMoneyMock(50.00m);
        var subtotal = CrearMoneyMock(100.00m);
        
        typeof(DetalleFactura).GetProperty("PrecioUnitario")?.SetValue(detalle, precioUnitario);
        typeof(DetalleFactura).GetProperty("Subtotal")?.SetValue(detalle, subtotal);
        
        return detalle;
    }

    private DescuentoFactura CrearDescuentoFacturaEjemplo()
    {
        // Usar reflection para crear descuento con propiedades privadas
        var descuento = (DescuentoFactura)Activator.CreateInstance(typeof(DescuentoFactura), true)!;
        
        typeof(DescuentoFactura).GetProperty("Id")?.SetValue(descuento, Guid.NewGuid());
        typeof(DescuentoFactura).GetProperty("TipoDescuento")?.SetValue(descuento, TipoDescuento.Porcentaje);
        typeof(DescuentoFactura).GetProperty("Concepto")?.SetValue(descuento, "Descuento cliente frecuente");
        typeof(DescuentoFactura).GetProperty("FechaAplicacion")?.SetValue(descuento, DateTime.UtcNow);
        
        // Crear value object mock
        var valor = CrearMoneyMock(10.00m);
        typeof(DescuentoFactura).GetProperty("Valor")?.SetValue(descuento, valor);
        
        return descuento;
    }

    private object CrearNombreMock(string nombre, string apellido)
    {
        // Crear mock del value object Nombre
        var nombreMock = new Mock<object>();
        var nombreType = typeof(Cliente).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Nombre"));
            
        if (nombreType != null)
        {
            try
            {
                var nombreObj = Activator.CreateInstance(nombreType, true);
                nombreType.GetProperty("Nombre")?.SetValue(nombreObj, nombre);
                nombreType.GetProperty("Apellido")?.SetValue(nombreObj, apellido);
                return nombreObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Nombre = nombre, Apellido = apellido };
    }

    private object CrearEmailMock(string email)
    {
        // Crear mock del value object Email
        var emailType = typeof(Cliente).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Email"));
            
        if (emailType != null)
        {
            try
            {
                var emailObj = Activator.CreateInstance(emailType, true);
                emailType.GetProperty("Value")?.SetValue(emailObj, email);
                return emailObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Value = email };
    }

    private object CrearTelefonoMock(string telefono)
    {
        // Crear mock del value object Telefono
        var telefonoType = typeof(Cliente).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Telefono"));
            
        if (telefonoType != null)
        {
            try
            {
                var telefonoObj = Activator.CreateInstance(telefonoType, true);
                telefonoType.GetProperty("Value")?.SetValue(telefonoObj, telefono);
                return telefonoObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Value = telefono };
    }

    private object CrearNumeroFacturaMock(string numero)
    {
        // Crear mock del value object NumeroFactura
        var numeroType = typeof(Factura).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Numero"));
            
        if (numeroType != null)
        {
            try
            {
                var numeroObj = Activator.CreateInstance(numeroType, true);
                numeroType.GetProperty("Value")?.SetValue(numeroObj, numero);
                return numeroObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Value = numero };
    }

    private object CrearMoneyMock(decimal amount)
    {
        // Crear mock del value object Money
        var moneyType = typeof(Factura).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Money"));
            
        if (moneyType != null)
        {
            try
            {
                var moneyObj = Activator.CreateInstance(moneyType, true);
                moneyType.GetProperty("Amount")?.SetValue(moneyObj, amount);
                return moneyObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Amount = amount };
    }

    #endregion
} 