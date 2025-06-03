using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
// using RestaurantePro.Application.Common.Interfaces.Persistence; // TEMPORAL: Este namespace no existe
using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;
// using RestaurantePro.Domain.Core.Entities.Operaciones; // TEMPORAL: Este namespace no existe
// using RestaurantePro.Domain.Core.Entities.Sistema; // TEMPORAL: Este namespace no existe  
// using RestaurantePro.Domain.Core.Enums; // TEMPORAL: Este namespace no existe
// using RestaurantePro.Domain.Operaciones.Entities; // TEMPORAL: Este namespace no existe
using Xunit;

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

    public GenerarReporteValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
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
        // Crear usuario con el primer rol de la lista o con un rol por defecto
        var rolPrincipal = roles.FirstOrDefault();
        if (rolPrincipal == default)
            rolPrincipal = RolUsuario.Cajero;
            
        var usuario = Usuario.Crear("testuser", "Test User", "test@test.com", rolPrincipal);
        
        // Usar reflexión para asignar el Id ya que es de solo lectura
        var idProperty = typeof(Usuario).BaseType?.GetProperty("Id");
        if (idProperty != null)
        {
            idProperty.SetValue(usuario, usuarioId);
        }
        
        // Asignar roles adicionales si hay más de uno
        foreach (var rol in roles.Skip(1))
        {
            usuario.AsignarRol(rol);
        }

        var usuarios = new List<Usuario> { usuario };
        var mockDbSet = usuarios.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Usuarios).Returns(mockDbSet.Object);
    }

    private void ConfigurarDatosExistentes(DateTime fechaInicio, DateTime fechaFin, bool tieneComandas = true, bool tieneMovimientos = true)
    {
        if (tieneComandas)
        {
            var comandas = new List<Comanda>
            {
                Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "Test comanda", $"COM-{DateTime.Now:yyyyMMdd}-TEST")
            };

            var mockComandasDbSet = comandas.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(x => x.Comandas).Returns(mockComandasDbSet.Object);
        }
        else
        {
            var comandasVacias = new List<Comanda>();
            var mockComandasDbSet = comandasVacias.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(x => x.Comandas).Returns(mockComandasDbSet.Object);
        }

        if (tieneMovimientos)
        {
            var movimientos = new List<MovimientoInventario>
            {
                MovimientoInventario.CrearIngreso(Guid.NewGuid(), 10.0m, "Test movimiento", fechaInicio.AddHours(12))
            };

            var mockMovimientosDbSet = movimientos.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(x => x.MovimientosInventario).Returns(mockMovimientosDbSet.Object);
        }
        else
        {
            var movimientosVacios = new List<MovimientoInventario>();
            var mockMovimientosDbSet = movimientosVacios.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(x => x.MovimientosInventario).Returns(mockMovimientosDbSet.Object);
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
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

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
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.UsuarioSolicitanteId))
            .Which.ErrorMessage.Should().Be("El usuario solicitante es requerido.");
    }

    [Fact]
    public async Task Validate_ConUsuarioInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        
        // Configurar contexto sin usuarios
        var usuariosVacios = new List<Usuario>();
        var mockDbSet = usuariosVacios.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Usuarios).Returns(mockDbSet.Object);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("usuario") && x.ErrorMessage.Contains("existe"));
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
            FechaInicio = default(DateTime),
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.FechaInicio))
            .Which.ErrorMessage.Should().Be("La fecha de inicio es requerida.");
    }

    [Fact]
    public async Task Validate_ConFechaInicioFutura_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(2),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.FechaInicio))
            .Which.ErrorMessage.Should().Be("La fecha de inicio no puede ser futura.");
    }

    [Fact]
    public async Task Validate_ConFechaInicioMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddYears(-10),
            FechaFin = DateTime.Today.AddYears(-10).AddDays(1),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.FechaInicio))
            .Which.ErrorMessage.Should().Be("La fecha de inicio no puede ser anterior a 5 años.");
    }

    [Fact]
    public async Task Validate_ConFechaFinAnteriorAInicio_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(-1),
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.FechaFin))
            .Which.ErrorMessage.Should().Be("La fecha de fin debe ser posterior a la fecha de inicio.");
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

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.FechaFin))
            .Which.ErrorMessage.Should().Be("El rango de fechas no puede ser mayor a un año.");
    }

    #endregion

    #region Validaciones Avanzadas

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
            FiltrosEspecificos = Enumerable.Range(1, 11).Select(i => Guid.NewGuid()).ToList()
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.FiltrosEspecificos))
            .Which.ErrorMessage.Should().Be("No se pueden especificar más de 10 filtros específicos.");
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
            FiltrosEspecificos = new List<Guid> { Guid.NewGuid(), Guid.Empty }
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "FiltrosEspecificos[1]")
            .Which.ErrorMessage.Should().Be("Los filtros específicos no pueden estar vacíos.");
    }

    [Fact]
    public async Task Validate_ConMuchosParametrosAdicionales_DeberiaRetornarError()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var parametrosAdicionales = new Dictionary<string, object>();
        for (int i = 0; i < 21; i++)
        {
            parametrosAdicionales.Add($"param{i}", $"value{i}");
        }

        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = baseCommand.FechaInicio,
            FechaFin = baseCommand.FechaFin,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            ParametrosAdicionales = parametrosAdicionales
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.ParametrosAdicionales))
            .Which.ErrorMessage.Should().Be("No se pueden especificar más de 20 parámetros adicionales.");
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
        result.Errors.Should().ContainSingle(x => x.PropertyName == "ContenidoIncluido")
            .Which.ErrorMessage.Should().Be("Debe incluir al menos un tipo de contenido en el reporte.");
    }

    [Fact]
    public async Task Validate_ConNombrePersonalizadoMuyLargo_DeberiaRetornarError()
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
            NombrePersonalizado = new string('A', 201)
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.NombrePersonalizado))
            .Which.ErrorMessage.Should().Be("El nombre personalizado no puede exceder 200 caracteres.");
    }

    #endregion

    #region Validaciones de Permisos

    [Fact]
    public async Task Validate_ConUsuarioSinPermisosReportes_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Cajero });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("permisos") && x.ErrorMessage.Contains("reportes"));
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
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("permisos") && x.ErrorMessage.Contains("tipo"));
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

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Cajero });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("permisos"));
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

    #endregion

    #region Validaciones de Email

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
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.EmailDestino))
            .Which.ErrorMessage.Should().Be("El email de destino es requerido cuando se solicita envío por email.");
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

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.EmailDestino))
            .Which.ErrorMessage.Should().Be("El formato del email de destino no es válido.");
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
            EmailDestino = "usuario@ejemplo.com"
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.EnviarPorEmail))
            .Which.ErrorMessage.Should().Be("El envío por email está limitado a reportes con rango máximo de 30 días.");
    }

    [Fact]
    public async Task Validate_ConEmailValidoYRangoCorto_DeberiaSerValido()
    {
        // Arrange
        var baseCommand = CrearCommandValido();
        var command = new GenerarReporteCommand
        {
            TipoReporte = baseCommand.TipoReporte,
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            Formato = baseCommand.Formato,
            UsuarioSolicitanteId = baseCommand.UsuarioSolicitanteId,
            EnviarPorEmail = true,
            EmailDestino = "usuario@ejemplo.com"
        };

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validaciones de Negocio

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

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.Prioridad))
            .Which.ErrorMessage.Should().Be("Los reportes financieros requieren prioridad alta.");
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

        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Administrador });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GenerarReporteCommand.NombrePersonalizado))
            .Which.ErrorMessage.Should().Be("El nombre personalizado es requerido para reportes personalizados.");
    }

    [Fact]
    public async Task Validate_ConPeriodoSinDatos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        
        // Configurar contexto sin datos para el período
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin, tieneComandas: false, tieneMovimientos: false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("datos") && x.ErrorMessage.Contains("período"));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Validate_ConReporteCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.VentasSemanales,
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            Formato = FormatoReporte.Excel,
            UsuarioSolicitanteId = Guid.NewGuid(),
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true,
            Prioridad = NivelPrioridad.Media,
            EnviarPorEmail = true,
            EmailDestino = "gerente@restaurante.com",
            FiltrosEspecificos = new List<Guid> { Guid.NewGuid() },
            ParametrosAdicionales = new Dictionary<string, object>
            {
                { "incluirImpuestos", true },
                { "agruparPorMesero", false }
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
            FechaInicio = DateTime.Today.AddDays(1), // Error: fecha futura
            FechaFin = DateTime.Today, // Error: fecha fin anterior a inicio
            Formato = FormatoReporte.PDF,
            UsuarioSolicitanteId = Guid.Empty, // Error: usuario vacío
            Prioridad = NivelPrioridad.Baja, // Error: prioridad baja para financiero
            EnviarPorEmail = true,
            EmailDestino = "email-invalido" // Error: email inválido
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuarioExistente(command.UsuarioSolicitanteId, new List<RolUsuario> { RolUsuario.Gerente });
        ConfigurarDatosExistentes(command.FechaInicio, command.FechaFin);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            await _validator.ValidateAsync(command);
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo para 100 validaciones
    }

    #endregion
} 