namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA GENERAR REPORTE VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de generación de reportes
/// Cobertura: 100% de reglas de negocio del GenerarReporteValidator
/// </summary>
public class GenerarReporteValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GenerarReporteValidator _validator;
    private readonly Mock<DbSet<Usuario>> _usuariosMock;
    private readonly Mock<DbSet<Comanda>> _comandasMock;
    private readonly Mock<DbSet<MovimientoInventario>> _movimientosMock;

    public GenerarReporteValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _usuariosMock = new Mock<DbSet<Usuario>>();
        _comandasMock = new Mock<DbSet<Comanda>>();
        _movimientosMock = new Mock<DbSet<MovimientoInventario>>();

        _contextMock.Setup(x => x.Usuarios).Returns(_usuariosMock.Object);
        _contextMock.Setup(x => x.Comandas).Returns(_comandasMock.Object);
        _contextMock.Setup(x => x.MovimientosInventario).Returns(_movimientosMock.Object);

        _validator = new GenerarReporteValidator(_contextMock.Object);
    }

    #region Helper Methods

    private GenerarReporteCommand CrearCommandValido()
    {
        return new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.VentasDiarias,
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaFin = DateTime.Today,
            Formato = FormatoReporte.PDF,
            UsuarioSolicitanteId = Guid.NewGuid(),
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true,
            Prioridad = NivelPrioridad.Media,
            EnviarPorEmail = false
        };
    }

    private void ConfigurarUsuarioExistente(Guid usuarioId, List<RolUsuario> roles, bool esAdministrador = false)
    {
        var usuario = new Usuario
        {
            Id = usuarioId,
            Roles = roles,
            EsAdministrador = esAdministrador
        };

        var usuarios = new List<Usuario> { usuario }.AsQueryable();
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuarios.Provider);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuarios.Expression);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());
    }

    private void ConfigurarDatosExistentes(DateTime fechaInicio, DateTime fechaFin, bool tieneComandas = true, bool tieneMovimientos = true)
    {
        if (tieneComandas)
        {
            var comandas = new List<Comanda>
            {
                Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "Test comanda", $"COM-{DateTime.Now:yyyyMMdd}-TEST")
            }.AsQueryable();

            _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandas.Provider);
            _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandas.Expression);
            _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandas.ElementType);
            _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandas.GetEnumerator());
        }

        if (tieneMovimientos)
        {
            var movimientos = new List<MovimientoInventario>
            {
                MovimientoInventario.CrearIngreso(Guid.NewGuid(), 10.0m, "Test movimiento", fechaInicio.AddHours(12))
            }.AsQueryable();

            _movimientosMock.As<IQueryable<MovimientoInventario>>().Setup(m => m.Provider).Returns(movimientos.Provider);
            _movimientosMock.As<IQueryable<MovimientoInventario>>().Setup(m => m.Expression).Returns(movimientos.Expression);
            _movimientosMock.As<IQueryable<MovimientoInventario>>().Setup(m => m.ElementType).Returns(movimientos.ElementType);
            _movimientosMock.As<IQueryable<MovimientoInventario>>().Setup(m => m.GetEnumerator()).Returns(movimientos.GetEnumerator());
        }
    }

    #endregion

    #region Validaciones Básicas

    [Fact]
    public async Task Validate_ConCommandValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(TipoReporte.VentasDiarias)]
    [InlineData(TipoReporte.VentasSemanales)]
    [InlineData(TipoReporte.Inventario)]
    [InlineData(TipoReporte.Financiero)]
    [InlineData(TipoReporte.Personalizado)]
    public async Task Validate_ConTiposReporteValidos_DeberiaSerValido(TipoReporte tipoReporte)
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = tipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            NombrePersonalizado = tipoReporte == TipoReporte.Personalizado ? "Reporte Personalizado Test" : null,
            Prioridad = tipoReporte == TipoReporte.Financiero ? NivelPrioridad.Alta : baseCommand.Prioridad
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador }, true);
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(FormatoReporte.PDF)]
    [InlineData(FormatoReporte.Excel)]
    [InlineData(FormatoReporte.CSV)]
    [InlineData(FormatoReporte.JSON)]
    [InlineData(FormatoReporte.HTML)]
    public async Task Validate_ConFormatosValidos_DeberiaSerValido(FormatoReporte formato)
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.UsuarioSolicitanteId) &&
            e.ErrorMessage.Contains("El ID del usuario solicitante es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var usuariosVacios = new List<Usuario>().AsQueryable();
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuariosVacios.Provider);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuariosVacios.Expression);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuariosVacios.ElementType);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuariosVacios.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.UsuarioSolicitanteId) &&
            e.ErrorMessage.Contains("El usuario solicitante no existe"));
    }

    #endregion

    #region Validaciones de Fechas

    [Fact]
    public async Task Validate_ConFechaInicioVacia_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = default,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.FechaInicio) &&
            e.ErrorMessage.Contains("La fecha de inicio es requerida"));
    }

    [Fact]
    public async Task Validate_ConFechaInicioFutura_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(2),
            FechaFin = DateTime.Today.AddDays(3),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.FechaInicio) &&
            e.ErrorMessage.Contains("La fecha de inicio no puede ser futura"));
    }

    [Fact]
    public async Task Validate_ConFechaInicioMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddYears(-6),
            FechaFin = DateTime.Today.AddYears(-6).AddDays(1),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.FechaInicio) &&
            e.ErrorMessage.Contains("La fecha de inicio no puede ser mayor a 5 años atrás"));
    }

    [Fact]
    public async Task Validate_ConFechaFinAnteriorAInicio_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaFin = DateTime.Today.AddDays(-2),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.FechaFin) &&
            e.ErrorMessage.Contains("La fecha de fin debe ser posterior o igual a la fecha de inicio"));
    }

    [Fact]
    public async Task Validate_ConRangoFechasMayorAUnAno_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(-400),
            FechaFin = DateTime.Today,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("El rango de fechas no puede exceder 1 año"));
    }

    #endregion

    #region Validaciones de Formato y Contenido

    [Fact]
    public async Task Validate_ConMuchosFiltrosEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            FiltrosEspecificos = Enumerable.Range(1, 101).Select(_ => Guid.NewGuid()).ToList()
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.FiltrosEspecificos) &&
            e.ErrorMessage.Contains("No se pueden especificar más de 100 filtros específicos"));
    }

    [Fact]
    public async Task Validate_ConFiltrosEspecificosConGuidVacio_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            FiltrosEspecificos = new List<Guid> { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() }
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.FiltrosEspecificos) &&
            e.ErrorMessage.Contains("Todos los IDs de filtros deben ser válidos"));
    }

    [Fact]
    public async Task Validate_ConMuchosParametrosAdicionales_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            ParametrosAdicionales = Enumerable.Range(1, 51)
                .ToDictionary(i => $"param{i}", i => (object)$"value{i}")
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.ParametrosAdicionales) &&
            e.ErrorMessage.Contains("No se pueden especificar más de 50 parámetros adicionales"));
    }

    [Fact]
    public async Task Validate_SinContenidoIncluido_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            IncluirGraficos = false,
            IncluirDetalles = false,
            IncluirResumenEjecutivo = false
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("Debe incluir al menos gráficos, detalles o resumen ejecutivo"));
    }

    [Fact]
    public async Task Validate_ConNombrePersonalizadoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            NombrePersonalizado = new string('A', 201)
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.NombrePersonalizado) &&
            e.ErrorMessage.Contains("El nombre personalizado no puede exceder 200 caracteres"));
    }

    #endregion

    #region Validaciones de Permisos

    [Fact]
    public async Task Validate_ConUsuarioSinPermisosReportes_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Mesero });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.UsuarioSolicitanteId) &&
            e.ErrorMessage.Contains("El usuario no tiene permisos para generar reportes"));
    }

    [Fact]
    public async Task Validate_ConReporteFinancieroSinPermisosAdministrador_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Financiero,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            Prioridad = NivelPrioridad.Alta
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("El usuario no tiene permisos para generar reportes financieros"));
    }

    [Fact]
    public async Task Validate_ConReporteInventarioSinPermisos_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Inventario,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Mesero });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("El usuario no tiene permisos para generar reportes de inventario"));
    }

    [Fact]
    public async Task Validate_ConAdministradorParaReporteFinanciero_DeberiaSerValido()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Financiero,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            Prioridad = NivelPrioridad.Alta
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador }, true);
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConEnvioEmailSinEmailDestino_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            EnviarPorEmail = true,
            EmailDestino = null
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.EmailDestino) &&
            e.ErrorMessage.Contains("El email de destino es requerido cuando se solicita envío por email"));
    }

    [Fact]
    public async Task Validate_ConEmailDestinoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            EnviarPorEmail = true,
            EmailDestino = "email-invalido"
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.EmailDestino) &&
            e.ErrorMessage.Contains("El formato del email de destino no es válido"));
    }

    [Fact]
    public async Task Validate_ConEnvioEmailYRangoMayorA30Dias_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(-35),
            FechaFin = DateTime.Today,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            EnviarPorEmail = true,
            EmailDestino = "test@example.com"
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("Los reportes enviados por email no pueden exceder 30 días de rango"));
    }

    [Fact]
    public async Task Validate_ConEmailValidoYRangoCorto_DeberiaSerValido()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            EnviarPorEmail = true,
            EmailDestino = "test@example.com"
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConReporteFinancieroSinPrioridadAlta_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Financiero,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            Prioridad = NivelPrioridad.Baja
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador }, true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("Los reportes financieros deben tener prioridad alta"));
    }

    [Fact]
    public async Task Validate_ConReportePersonalizadoSinNombre_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Personalizado,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            NombrePersonalizado = null
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(GenerarReporteCommand.NombrePersonalizado) &&
            e.ErrorMessage.Contains("El nombre personalizado es requerido para reportes personalizados"));
    }

    [Fact]
    public async Task Validate_ConPeriodoSinDatos_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(-100),
            FechaFin = DateTime.Today.AddDays(-95),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin, false, false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("No hay datos disponibles para el período especificado"));
    }

    #endregion

    #region Tests de Escenarios Complejos

    [Fact]
    public async Task Validate_ConReporteCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.VentasMensuales,
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            Formato = FormatoReporte.Excel,
            UsuarioSolicitanteId = Guid.NewGuid(),
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true,
            NombrePersonalizado = "Reporte Mensual Personalizado",
            Prioridad = NivelPrioridad.Media,
            EnviarPorEmail = true,
            EmailDestino = "gerente@restaurante.com",
            FiltrosEspecificos = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            ParametrosAdicionales = new Dictionary<string, object>
            {
                { "incluirComparativo", true },
                { "nivelDetalle", "completo" }
            }
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Financiero,
            FechaInicio = DateTime.Today.AddDays(1), // Fecha futura
            FechaFin = DateTime.Today.AddDays(-1), // Fecha fin anterior a inicio
            Formato = FormatoReporte.PDF,
            UsuarioSolicitanteId = Guid.Empty, // Usuario vacío
            IncluirGraficos = false,
            IncluirDetalles = false,
            IncluirResumenEjecutivo = false, // Sin contenido
            Prioridad = NivelPrioridad.Baja, // Prioridad incorrecta para financiero
            EnviarPorEmail = true,
            EmailDestino = "email-invalido" // Email inválido
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("El ID del usuario solicitante es requerido"));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("La fecha de inicio no puede ser futura"));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Debe incluir al menos gráficos, detalles o resumen ejecutivo"));
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(command);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
    }

    #endregion
} 