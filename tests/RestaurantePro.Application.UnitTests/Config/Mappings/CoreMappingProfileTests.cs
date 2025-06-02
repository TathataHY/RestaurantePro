namespace RestaurantePro.Application.UnitTests.Config.Mappings;

/// <summary>
/// Tests unitarios para CoreMappingProfile
/// Cobertura completa de mapeos de Producto y Usuario, validación de configuración y edge cases
/// </summary>
public class CoreMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public CoreMappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CoreMappingProfile>();
        });
        
        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_DeberiaSerValida()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    #region Producto Mappings Tests

    [Fact]
    public void Map_ProductoToProductoDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var producto = CrearProductoEjemplo();

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(producto.Id);
        dto.Nombre.Should().Be(producto.Nombre);
        dto.Descripcion.Should().Be(producto.Descripcion);
        dto.Precio.Should().Be(producto.Precio?.Valor ?? 0);
        dto.CategoriaNombre.Should().Be(producto.CategoriaNombre ?? "Sin categoría");
        dto.Activo.Should().Be(producto.EstaActivo);
        dto.CreadoPor.Should().Be("Sistema");
    }

    [Fact]
    public void Map_ProductoToProductoDto_ConPrecioNull_DeberiaMapearPrecioCero()
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        typeof(Producto).GetProperty("Precio")?.SetValue(producto, null);

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        dto.Precio.Should().Be(0);
    }

    [Fact]
    public void Map_ProductoToProductoDto_ConCategoriaNombreNull_DeberiaMapearSinCategoria()
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        typeof(Producto).GetProperty("CategoriaNombre")?.SetValue(producto, null);

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        dto.CategoriaNombre.Should().Be("Sin categoría");
    }

    [Fact]
    public void Map_ProductoToProductoSummaryDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var producto = CrearProductoEjemplo();

        // Act
        var dto = _mapper.Map<ProductoSummaryDto>(producto);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(producto.Id);
        dto.Nombre.Should().Be(producto.Nombre);
        dto.Precio.Should().Be(producto.Precio?.Valor ?? 0);
        dto.CategoriaNombre.Should().Be(producto.CategoriaNombre ?? "Sin categoría");
        dto.Activo.Should().Be(producto.EstaActivo);
        dto.Disponible.Should().Be(producto.EstaActivo);
        dto.CreadoPor.Should().Be("Sistema");
        dto.TotalIngredientes.Should().Be(0); // TODO value
        dto.CostoEstimado.Should().BeNull(); // TODO value
    }

    [Fact]
    public void Map_ProductoToProductoSummaryDto_ConDescripcionLarga_DeberiaRecortarDescripcion()
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        var descripcionLarga = new string('A', 150); // 150 caracteres
        typeof(Producto).GetProperty("Descripcion")?.SetValue(producto, descripcionLarga);

        // Act
        var dto = _mapper.Map<ProductoSummaryDto>(producto);

        // Assert
        dto.DescripcionCorta.Should().HaveLength(103); // 100 + "..."
        dto.DescripcionCorta.Should().EndWith("...");
        dto.DescripcionCorta.Should().StartWith(descripcionLarga.Substring(0, 100));
    }

    [Fact]
    public void Map_ProductoToProductoSummaryDto_ConDescripcionCorta_DeberiaManternerDescripcion()
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        var descripcionCorta = "Descripción corta";
        typeof(Producto).GetProperty("Descripcion")?.SetValue(producto, descripcionCorta);

        // Act
        var dto = _mapper.Map<ProductoSummaryDto>(producto);

        // Assert
        dto.DescripcionCorta.Should().Be(descripcionCorta);
    }

    [Fact]
    public void Map_ProductoToProductoSummaryDto_ConDescripcionNull_DeberiaMapearSinDescripcion()
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        typeof(Producto).GetProperty("Descripcion")?.SetValue(producto, null);

        // Act
        var dto = _mapper.Map<ProductoSummaryDto>(producto);

        // Assert
        dto.DescripcionCorta.Should().Be("Sin descripción");
    }

    [Fact]
    public void Map_ProductoCreateDtoToCrearProductoCommand_DeberiaMapearCorrectamente()
    {
        // Arrange
        var createDto = new ProductoCreateDto
        {
            Nombre = "Pizza Margherita",
            Descripcion = "Pizza clásica italiana",
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var command = _mapper.Map<CrearProductoCommand>(createDto);

        // Assert
        command.Should().NotBeNull();
        command.Nombre.Should().Be(createDto.Nombre);
        command.Descripcion.Should().Be(createDto.Descripcion);
        command.Precio.Should().Be(createDto.Precio);
        command.CategoriaId.Should().Be(createDto.CategoriaId);
    }

    [Fact]
    public void Map_ProductoUpdateDtoToActualizarProductoCommand_DeberiaMapearCorrectamente()
    {
        // Arrange
        var updateDto = new ProductoUpdateDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita Actualizada",
            Descripcion = "Pizza clásica italiana actualizada",
            Precio = 17.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var command = _mapper.Map<ActualizarProductoCommand>(updateDto);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(updateDto.Id);
        command.Nombre.Should().Be(updateDto.Nombre);
        command.Descripcion.Should().Be(updateDto.Descripcion);
        command.Precio.Should().Be(updateDto.Precio);
        command.CategoriaId.Should().Be(updateDto.CategoriaId);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Map_ProductoToProductoDto_ConDiferentesEstadosActivo_DeberiaMapearCorrectamente(bool estaActivo, bool expectedActivo)
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        typeof(Producto).GetProperty("EstaActivo")?.SetValue(producto, estaActivo);

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        dto.Activo.Should().Be(expectedActivo);
    }

    #endregion

    #region Usuario Mappings Tests

    [Fact]
    public void Map_UsuarioToUsuarioDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(usuario.Id);
        dto.NombreUsuario.Should().Be(usuario.NombreUsuario);
        dto.Email.Should().Be(usuario.Email);
        dto.Estado.Should().Be(usuario.Estado);
        dto.TipoUsuario.Should().Be(usuario.TipoUsuario);
        dto.Rol.Should().Be(usuario.Rol);
        dto.NivelAcceso.Should().Be(usuario.NivelAcceso);
        dto.Permisos.Should().BeEquivalentTo(usuario.Permisos.ToList());
        dto.SupervisorId.Should().Be(usuario.SupervisorId);
        dto.Departamento.Should().Be(usuario.Departamento);
        dto.Posicion.Should().Be(usuario.Posicion);
        dto.Identificacion.Should().Be(usuario.Identificacion);
        dto.UltimoAcceso.Should().Be(usuario.UltimoAcceso);
        dto.MotivoBloqueo.Should().Be(usuario.MotivoBloqueo);
        dto.EsAdministrador.Should().Be(usuario.EsAdministrador);
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_ConNombreCompleto_DeberiaMapearNombreYApellido()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("NombreCompleto")?.SetValue(usuario, "Juan Carlos Pérez García");

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Nombre.Should().Be("Juan");
        dto.Apellido.Should().Be("Carlos Pérez García");
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_ConNombreCompletoSoloNombre_DeberiaMapearCorrectamente()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("NombreCompleto")?.SetValue(usuario, "Juan");

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Nombre.Should().Be("Juan");
        dto.Apellido.Should().BeEmpty();
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_ConNombreCompletoNull_DeberiaMapearVacio()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("NombreCompleto")?.SetValue(usuario, null);

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Nombre.Should().BeEmpty();
        dto.Apellido.Should().BeEmpty();
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_ConNombreCompletoVacio_DeberiaMapearVacio()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("NombreCompleto")?.SetValue(usuario, "");

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Nombre.Should().BeEmpty();
        dto.Apellido.Should().BeEmpty();
    }

    [Theory]
    [InlineData(EstadoUsuario.Activo, true, true)]
    [InlineData(EstadoUsuario.Inactivo, false, false)]
    [InlineData(EstadoUsuario.Bloqueado, false, false)]
    [InlineData(EstadoUsuario.Suspendido, false, false)]
    public void Map_UsuarioToUsuarioDto_ConDiferentesEstados_DeberiaMapearCorrectamente(
        EstadoUsuario estado, bool expectedActivo, bool expectedVerificado)
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("Estado")?.SetValue(usuario, estado);

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Estado.Should().Be(estado);
        dto.Activo.Should().Be(expectedActivo);
        dto.Verificado.Should().Be(expectedVerificado);
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_ConEstadoBloqueado_DeberiaMapearFechaBloqueado()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("Estado")?.SetValue(usuario, EstadoUsuario.Bloqueado);

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.FechaBloqueado.Should().NotBeNull();
        dto.FechaBloqueado.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_ConEstadoActivo_NoDeberiaMapearFechaBloqueado()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        typeof(Usuario).GetProperty("Estado")?.SetValue(usuario, EstadoUsuario.Activo);

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.FechaBloqueado.Should().BeNull();
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_PropiedadesCalculadas_DeberianTenerValoresPorDefecto()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.NumeroIdentificacion.Should().Be(usuario.Identificacion);
        dto.Cargo.Should().Be(usuario.Posicion);
        dto.FechaUltimaConexion.Should().Be(usuario.UltimoAcceso);
        dto.EsTemporal.Should().BeFalse();
        dto.IntentosFallidos.Should().Be(0);
        dto.DebeResetearPassword.Should().BeFalse();
        dto.SucursalesAcceso.Should().BeEmpty();
        dto.ZonaHoraria.Should().Be("America/Mexico_City");
        dto.Idioma.Should().Be("es-MX");
        dto.RolesAdicionales.Should().BeEmpty();
        dto.CantidadSubordinados.Should().Be(0);
    }

    [Fact]
    public void Map_UsuarioToUsuarioDto_PropiedadesHeredadas_DeberianMapearseCorrectamente()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        var fechaCreacion = DateTime.UtcNow.AddDays(-30);
        typeof(Usuario).GetProperty("FechaCreacion")?.SetValue(usuario, fechaCreacion);

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.FechaCreacion.Should().Be(fechaCreacion);
    }

    #endregion

    #region Edge Cases y Null Handling

    [Fact]
    public void Map_ProductoNull_DeberiaRetornarNull()
    {
        // Arrange
        Producto? producto = null;

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_UsuarioNull_DeberiaRetornarNull()
    {
        // Arrange
        Usuario? usuario = null;

        // Act
        var dto = _mapper.Map<UsuarioDto>(usuario);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ListaProductos_DeberiaMapearTodos()
    {
        // Arrange
        var productos = new List<Producto>
        {
            CrearProductoEjemplo(),
            CrearProductoEjemplo(),
            CrearProductoEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<ProductoDto>>(productos);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaUsuarios_DeberiaMapearTodos()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            CrearUsuarioEjemplo(),
            CrearUsuarioEjemplo(),
            CrearUsuarioEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<UsuarioDto>>(usuarios);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var productos = new List<Producto>();

        // Act
        var dtos = _mapper.Map<List<ProductoDto>>(productos);

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Map_ProductoToDto_DeberiaSerRapido()
    {
        // Arrange
        var producto = CrearProductoEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<ProductoDto>(producto);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    [Fact]
    public void Map_UsuarioToDto_DeberiaSerRapido()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<UsuarioDto>(usuario);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    #endregion

    #region Helper Methods

    private Producto CrearProductoEjemplo()
    {
        // Usar reflection para crear producto con propiedades privadas
        var producto = (Producto)Activator.CreateInstance(typeof(Producto), true)!;
        
        typeof(Producto).GetProperty("Id")?.SetValue(producto, Guid.NewGuid());
        typeof(Producto).GetProperty("Nombre")?.SetValue(producto, "Pizza Margherita");
        typeof(Producto).GetProperty("Descripcion")?.SetValue(producto, "Pizza clásica italiana con tomate y mozzarella");
        typeof(Producto).GetProperty("CategoriaNombre")?.SetValue(producto, "Pizzas");
        typeof(Producto).GetProperty("EstaActivo")?.SetValue(producto, true);
        
        // Crear precio usando reflection o builder si está disponible
        var precioProperty = typeof(Producto).GetProperty("Precio");
        if (precioProperty != null)
        {
            // Intentar crear un precio mock - esto depende de la implementación del ValueObject
            var precio = CrearPrecioMock(15.99m);
            precioProperty.SetValue(producto, precio);
        }
        
        return producto;
    }

    private Usuario CrearUsuarioEjemplo()
    {
        // Usar reflection para crear usuario con propiedades privadas
        var usuario = (Usuario)Activator.CreateInstance(typeof(Usuario), true)!;
        
        typeof(Usuario).GetProperty("Id")?.SetValue(usuario, Guid.NewGuid());
        typeof(Usuario).GetProperty("NombreUsuario")?.SetValue(usuario, "jperez");
        typeof(Usuario).GetProperty("Email")?.SetValue(usuario, "juan.perez@restaurante.com");
        typeof(Usuario).GetProperty("NombreCompleto")?.SetValue(usuario, "Juan Pérez García");
        typeof(Usuario).GetProperty("Estado")?.SetValue(usuario, EstadoUsuario.Activo);
        typeof(Usuario).GetProperty("TipoUsuario")?.SetValue(usuario, TipoUsuario.Empleado);
        typeof(Usuario).GetProperty("Rol")?.SetValue(usuario, "Mesero");
        typeof(Usuario).GetProperty("NivelAcceso")?.SetValue(usuario, 5);
        typeof(Usuario).GetProperty("Permisos")?.SetValue(usuario, new List<string> { "LEER_MENU", "CREAR_ORDEN" });
        typeof(Usuario).GetProperty("SupervisorId")?.SetValue(usuario, Guid.NewGuid());
        typeof(Usuario).GetProperty("Departamento")?.SetValue(usuario, "Servicio");
        typeof(Usuario).GetProperty("Posicion")?.SetValue(usuario, "Mesero Senior");
        typeof(Usuario).GetProperty("Identificacion")?.SetValue(usuario, "12345678");
        typeof(Usuario).GetProperty("UltimoAcceso")?.SetValue(usuario, DateTime.UtcNow.AddHours(-2));
        typeof(Usuario).GetProperty("MotivoBloqueo")?.SetValue(usuario, null);
        typeof(Usuario).GetProperty("EsAdministrador")?.SetValue(usuario, false);
        typeof(Usuario).GetProperty("FechaCreacion")?.SetValue(usuario, DateTime.UtcNow.AddDays(-30));
        
        return usuario;
    }

    private object? CrearPrecioMock(decimal valor)
    {
        // Intentar crear un precio mock - esto depende de la implementación
        // Si no funciona, retornar null y el test manejará el caso
        try
        {
            // Buscar el tipo Precio en el assembly del domain
            var precioType = typeof(Producto).Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "Precio");
                
            if (precioType != null)
            {
                // Intentar crear instancia con reflection
                var precio = Activator.CreateInstance(precioType, true);
                precioType.GetProperty("Valor")?.SetValue(precio, valor);
                return precio;
            }
        }
        catch
        {
            // Si falla, retornar null
        }
        
        return null;
    }

    #endregion
} 