namespace RestaurantePro.Application.UnitTests.Config.Mappings;

/// <summary>
/// Tests unitarios para ProveedoresMappingProfile
/// Cobertura completa de mapeos de Proveedor y ContactoProveedor
/// </summary>
public class ProveedoresMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public ProveedoresMappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProveedoresMappingProfile>();
        });
        
        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_DeberiaSerValida()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    #region Proveedor Mappings Tests

    [Fact]
    public void Map_ProveedorToProveedorDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var proveedor = CrearProveedorEjemplo();

        // Act
        var dto = _mapper.Map<ProveedorDto>(proveedor);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(proveedor.Id);
        dto.Nombre.Should().Be(proveedor.Nombre);
        dto.RazonSocial.Should().Be(proveedor.RazonSocial);
        dto.Rfc.Should().Be(proveedor.Rfc);
        dto.Telefono.Should().Be(proveedor.Telefono);
        dto.Email.Should().Be(proveedor.Email);
        dto.Direccion.Should().Be(proveedor.Direccion);
        dto.Ciudad.Should().Be(proveedor.Ciudad);
        dto.Estado.Should().Be(proveedor.Estado);
        dto.CodigoPostal.Should().Be(proveedor.CodigoPostal);
        dto.Pais.Should().Be(proveedor.Pais);
        dto.Activo.Should().Be(proveedor.Activo);
        dto.FechaCreacion.Should().Be(proveedor.FechaCreacion);
        dto.FechaModificacion.Should().Be(proveedor.FechaModificacion);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Map_ProveedorToDto_ConDiferentesEstadosActivo_DeberiaMapearCorrectamente(bool activo, bool expectedActivo)
    {
        // Arrange
        var proveedor = CrearProveedorEjemplo();
        typeof(Proveedor).GetProperty("Activo")?.SetValue(proveedor, activo);

        // Act
        var dto = _mapper.Map<ProveedorDto>(proveedor);

        // Assert
        dto.Activo.Should().Be(expectedActivo);
    }

    [Fact]
    public void Map_ProveedorToDto_ConCamposNulos_DeberiaMapearCorrectamente()
    {
        // Arrange
        var proveedor = CrearProveedorEjemplo();
        typeof(Proveedor).GetProperty("Email")?.SetValue(proveedor, null);
        typeof(Proveedor).GetProperty("Telefono")?.SetValue(proveedor, null);
        typeof(Proveedor).GetProperty("FechaModificacion")?.SetValue(proveedor, null);

        // Act
        var dto = _mapper.Map<ProveedorDto>(proveedor);

        // Assert
        dto.Should().NotBeNull();
        dto.Email.Should().BeNull();
        dto.Telefono.Should().BeNull();
        dto.FechaModificacion.Should().BeNull();
    }

    [Fact]
    public void Map_ProveedorToDto_ConCamposVacios_DeberiaMapearCorrectamente()
    {
        // Arrange
        var proveedor = CrearProveedorEjemplo();
        typeof(Proveedor).GetProperty("Email")?.SetValue(proveedor, "");
        typeof(Proveedor).GetProperty("Telefono")?.SetValue(proveedor, "");

        // Act
        var dto = _mapper.Map<ProveedorDto>(proveedor);

        // Assert
        dto.Should().NotBeNull();
        dto.Email.Should().BeEmpty();
        dto.Telefono.Should().BeEmpty();
    }

    #endregion

    #region ContactoProveedor Mappings Tests

    [Fact]
    public void Map_ContactoProveedorToContactoProveedorDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var contacto = CrearContactoProveedorEjemplo();

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(contacto.Id);
        dto.ProveedorId.Should().Be(contacto.ProveedorId);
        dto.Nombre.Should().Be(contacto.Nombre);
        dto.Apellido.Should().Be(contacto.Apellido);
        dto.Cargo.Should().Be(contacto.Cargo);
        dto.Telefono.Should().Be(contacto.Telefono);
        dto.Email.Should().Be(contacto.Email);
        dto.EsPrincipal.Should().Be(contacto.EsPrincipal);
        dto.Activo.Should().Be(contacto.Activo);
        dto.FechaCreacion.Should().Be(contacto.FechaCreacion);
        dto.FechaModificacion.Should().Be(contacto.FechaModificacion);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Map_ContactoProveedorToDto_ConDiferentesEstadosActivo_DeberiaMapearCorrectamente(bool activo, bool expectedActivo)
    {
        // Arrange
        var contacto = CrearContactoProveedorEjemplo();
        typeof(ContactoProveedor).GetProperty("Activo")?.SetValue(contacto, activo);

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.Activo.Should().Be(expectedActivo);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Map_ContactoProveedorToDto_ConDiferentesEstadosPrincipal_DeberiaMapearCorrectamente(bool esPrincipal, bool expectedPrincipal)
    {
        // Arrange
        var contacto = CrearContactoProveedorEjemplo();
        typeof(ContactoProveedor).GetProperty("EsPrincipal")?.SetValue(contacto, esPrincipal);

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.EsPrincipal.Should().Be(expectedPrincipal);
    }

    [Fact]
    public void Map_ContactoProveedorToDto_ConCamposNulos_DeberiaMapearCorrectamente()
    {
        // Arrange
        var contacto = CrearContactoProveedorEjemplo();
        typeof(ContactoProveedor).GetProperty("Apellido")?.SetValue(contacto, null);
        typeof(ContactoProveedor).GetProperty("Cargo")?.SetValue(contacto, null);
        typeof(ContactoProveedor).GetProperty("FechaModificacion")?.SetValue(contacto, null);

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.Should().NotBeNull();
        dto.Apellido.Should().BeNull();
        dto.Cargo.Should().BeNull();
        dto.FechaModificacion.Should().BeNull();
    }

    [Fact]
    public void Map_ContactoProveedorToDto_ConCamposVacios_DeberiaMapearCorrectamente()
    {
        // Arrange
        var contacto = CrearContactoProveedorEjemplo();
        typeof(ContactoProveedor).GetProperty("Apellido")?.SetValue(contacto, "");
        typeof(ContactoProveedor).GetProperty("Cargo")?.SetValue(contacto, "");

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.Should().NotBeNull();
        dto.Apellido.Should().BeEmpty();
        dto.Cargo.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases y Null Handling

    [Fact]
    public void Map_ProveedorNull_DeberiaRetornarNull()
    {
        // Arrange
        Proveedor? proveedor = null;

        // Act
        var dto = _mapper.Map<ProveedorDto>(proveedor);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ContactoProveedorNull_DeberiaRetornarNull()
    {
        // Arrange
        ContactoProveedor? contacto = null;

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ListaProveedores_DeberiaMapearTodos()
    {
        // Arrange
        var proveedores = new List<Proveedor>
        {
            CrearProveedorEjemplo(),
            CrearProveedorEjemplo(),
            CrearProveedorEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<ProveedorDto>>(proveedores);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaContactos_DeberiaMapearTodos()
    {
        // Arrange
        var contactos = new List<ContactoProveedor>
        {
            CrearContactoProveedorEjemplo(),
            CrearContactoProveedorEjemplo(),
            CrearContactoProveedorEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>>(contactos);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var proveedores = new List<Proveedor>();

        // Act
        var dtos = _mapper.Map<List<ProveedorDto>>(proveedores);

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Map_ProveedorToDto_DeberiaSerRapido()
    {
        // Arrange
        var proveedor = CrearProveedorEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<ProveedorDto>(proveedor);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    [Fact]
    public void Map_ContactoProveedorToDto_DeberiaSerRapido()
    {
        // Arrange
        var contacto = CrearContactoProveedorEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Map_ProveedorConTodosLosCampos_DeberiaMapearCompleto()
    {
        // Arrange
        var proveedor = CrearProveedorCompletoEjemplo();

        // Act
        var dto = _mapper.Map<ProveedorDto>(proveedor);

        // Assert
        dto.Should().NotBeNull();
        dto.Nombre.Should().NotBeNullOrEmpty();
        dto.RazonSocial.Should().NotBeNullOrEmpty();
        dto.Rfc.Should().NotBeNullOrEmpty();
        dto.Telefono.Should().NotBeNullOrEmpty();
        dto.Email.Should().NotBeNullOrEmpty();
        dto.Direccion.Should().NotBeNullOrEmpty();
        dto.Ciudad.Should().NotBeNullOrEmpty();
        dto.Estado.Should().NotBeNullOrEmpty();
        dto.CodigoPostal.Should().NotBeNullOrEmpty();
        dto.Pais.Should().NotBeNullOrEmpty();
        dto.FechaCreacion.Should().NotBe(default);
    }

    [Fact]
    public void Map_ContactoProveedorCompleto_DeberiaMapearCompleto()
    {
        // Arrange
        var contacto = CrearContactoProveedorCompletoEjemplo();

        // Act
        var dto = _mapper.Map<RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>(contacto);

        // Assert
        dto.Should().NotBeNull();
        dto.Nombre.Should().NotBeNullOrEmpty();
        dto.Apellido.Should().NotBeNullOrEmpty();
        dto.Cargo.Should().NotBeNullOrEmpty();
        dto.Telefono.Should().NotBeNullOrEmpty();
        dto.Email.Should().NotBeNullOrEmpty();
        dto.FechaCreacion.Should().NotBe(default);
    }

    #endregion

    #region Helper Methods

    private Proveedor CrearProveedorEjemplo()
    {
        // Usar reflection para crear proveedor con propiedades privadas
        var proveedor = (Proveedor)Activator.CreateInstance(typeof(Proveedor), true)!;
        
        typeof(Proveedor).GetProperty("Id")?.SetValue(proveedor, Guid.NewGuid());
        typeof(Proveedor).GetProperty("Nombre")?.SetValue(proveedor, "Distribuidora ABC");
        typeof(Proveedor).GetProperty("RazonSocial")?.SetValue(proveedor, "Distribuidora ABC S.A. de C.V.");
        typeof(Proveedor).GetProperty("Rfc")?.SetValue(proveedor, "DABC123456789");
        typeof(Proveedor).GetProperty("Telefono")?.SetValue(proveedor, "555-1234567");
        typeof(Proveedor).GetProperty("Email")?.SetValue(proveedor, "contacto@distribuidoraabc.com");
        typeof(Proveedor).GetProperty("Direccion")?.SetValue(proveedor, "Av. Principal 123");
        typeof(Proveedor).GetProperty("Ciudad")?.SetValue(proveedor, "Ciudad de México");
        typeof(Proveedor).GetProperty("Estado")?.SetValue(proveedor, "CDMX");
        typeof(Proveedor).GetProperty("CodigoPostal")?.SetValue(proveedor, "12345");
        typeof(Proveedor).GetProperty("Pais")?.SetValue(proveedor, "México");
        typeof(Proveedor).GetProperty("Activo")?.SetValue(proveedor, true);
        typeof(Proveedor).GetProperty("FechaCreacion")?.SetValue(proveedor, DateTime.UtcNow.AddDays(-30));
        typeof(Proveedor).GetProperty("FechaModificacion")?.SetValue(proveedor, DateTime.UtcNow.AddDays(-5));
        
        return proveedor;
    }

    private Proveedor CrearProveedorCompletoEjemplo()
    {
        return CrearProveedorEjemplo(); // Ya está completo
    }

    private ContactoProveedor CrearContactoProveedorEjemplo()
    {
        // Usar reflection para crear contacto con propiedades privadas
        var contacto = (ContactoProveedor)Activator.CreateInstance(typeof(ContactoProveedor), true)!;
        
        typeof(ContactoProveedor).GetProperty("Id")?.SetValue(contacto, Guid.NewGuid());
        typeof(ContactoProveedor).GetProperty("ProveedorId")?.SetValue(contacto, Guid.NewGuid());
        typeof(ContactoProveedor).GetProperty("Nombre")?.SetValue(contacto, "Juan");
        typeof(ContactoProveedor).GetProperty("Apellido")?.SetValue(contacto, "Pérez");
        typeof(ContactoProveedor).GetProperty("Cargo")?.SetValue(contacto, "Gerente de Ventas");
        typeof(ContactoProveedor).GetProperty("Telefono")?.SetValue(contacto, "555-9876543");
        typeof(ContactoProveedor).GetProperty("Email")?.SetValue(contacto, "juan.perez@distribuidoraabc.com");
        typeof(ContactoProveedor).GetProperty("EsPrincipal")?.SetValue(contacto, true);
        typeof(ContactoProveedor).GetProperty("Activo")?.SetValue(contacto, true);
        typeof(ContactoProveedor).GetProperty("FechaCreacion")?.SetValue(contacto, DateTime.UtcNow.AddDays(-25));
        typeof(ContactoProveedor).GetProperty("FechaModificacion")?.SetValue(contacto, DateTime.UtcNow.AddDays(-3));
        
        return contacto;
    }

    private ContactoProveedor CrearContactoProveedorCompletoEjemplo()
    {
        return CrearContactoProveedorEjemplo(); // Ya está completo
    }

    #endregion
} 