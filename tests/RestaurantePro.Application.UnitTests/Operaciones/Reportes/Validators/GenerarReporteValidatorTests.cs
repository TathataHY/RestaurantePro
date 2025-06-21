using Moq;
using FluentValidation.TestHelper;
using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using System.Linq.Expressions;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Application.Common.Enums;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Validators
{
    public class GenerarReporteValidatorTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IMovimientoInventarioRepository> _movimientoInventarioRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly GenerarReporteValidator _validator;
        private readonly Guid _testUserId = Guid.NewGuid();

        public GenerarReporteValidatorTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _movimientoInventarioRepositoryMock = new Mock<IMovimientoInventarioRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();

            _validator = new GenerarReporteValidator(
                _usuarioRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _movimientoInventarioRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object);
        }

        private void SetupUser(bool userExists, List<RolUsuario>? roles = null)
        {
            Usuario? user = null;
            if (userExists)
            {
                user = Usuario.Crear("testuser", "Test User", "test@test.com", roles?.FirstOrDefault() ?? RolUsuario.Mesero);
                user.SetIdForTesting(_testUserId);

                if (roles != null)
                {
                    foreach (var rol in roles.Skip(1))
                    {
                        user.AsignarRol(rol);
                    }
                }
            }
            _usuarioRepositoryMock.Setup(r => r.ObtenerPorIdAsync(_testUserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        }

        [Fact]
        public async Task Should_Have_Error_When_Fechas_Invalidas()
        {
            // Arrange
            var command = new GenerarReporteCommand
            {
                TipoReporte = TipoReporte.VentasDiarias,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(-1),
                UsuarioSolicitanteId = _testUserId
            };

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.FechaFin)
                .WithErrorMessage("La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        [Fact]
        public async Task Should_Have_Error_When_Usuario_No_Existe()
        {
            // Arrange
            SetupUser(false);
            var command = new GenerarReporteCommand
            {
                TipoReporte = TipoReporte.VentasDiarias,
                FechaInicio = DateTime.UtcNow.AddDays(-1),
                FechaFin = DateTime.UtcNow,
                UsuarioSolicitanteId = _testUserId
            };

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.UsuarioSolicitanteId)
                .WithErrorMessage("El usuario especificado no existe.");
        }

        [Fact]
        public async Task Should_Have_Error_When_Usuario_No_Tiene_Permisos()
        {
            // Arrange
            SetupUser(true, new List<RolUsuario>()); // Usuario existe pero sin roles
            var command = new GenerarReporteCommand
            {
                TipoReporte = TipoReporte.VentasDiarias,
                FechaInicio = DateTime.UtcNow.AddDays(-1),
                FechaFin = DateTime.UtcNow,
                UsuarioSolicitanteId = _testUserId
            };

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.UsuarioSolicitanteId)
                .WithErrorMessage("El usuario no tiene permisos para generar reportes.");
        }

        [Fact]
        public async Task Should_Have_Error_When_No_Hay_Datos_Para_Reporte_Ventas()
        {
            // Arrange
            SetupUser(true, new List<RolUsuario> { RolUsuario.Administrador });
            var command = new GenerarReporteCommand
            {
                TipoReporte = TipoReporte.VentasDiarias,
                FechaInicio = DateTime.UtcNow.AddDays(-1),
                FechaFin = DateTime.UtcNow,
                UsuarioSolicitanteId = _testUserId
            };
            _comandaRepositoryMock.Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Comanda>());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor("DatosDisponibles")
                .WithErrorMessage("El período seleccionado no tiene datos suficientes para generar el reporte.");
        }

        [Fact]
        public async Task Should_Have_Error_When_No_Hay_Datos_Para_Reporte_Inventario()
        {
            // Arrange
            SetupUser(true, new List<RolUsuario> { RolUsuario.EncargadoInventario, RolUsuario.Administrador });
            var command = new GenerarReporteCommand
            {
                TipoReporte = TipoReporte.Inventario,
                FechaInicio = DateTime.UtcNow.AddDays(-1),
                FechaFin = DateTime.UtcNow,
                UsuarioSolicitanteId = _testUserId
            };
            _ordenCompraRepositoryMock.Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra>());

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor("DatosDisponibles")
                .WithErrorMessage("El período seleccionado no tiene datos suficientes para generar el reporte.");
        }


        [Theory]
        [InlineData(TipoReporte.VentasDiarias, RolUsuario.Gerente)]
        [InlineData(TipoReporte.Inventario, RolUsuario.EncargadoInventario)]
        [InlineData(TipoReporte.Financiero, RolUsuario.Cajero)]
        [InlineData(TipoReporte.VentasDiarias, RolUsuario.Administrador)]
        public async Task Should_Not_Have_Error_When_Command_Is_Valid(TipoReporte tipoReporte, RolUsuario rol)
        {
            // Arrange
            SetupUser(true, new List<RolUsuario> { rol, RolUsuario.Administrador }); //Add admin to pass general permissions
            var fechaTest = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            
            var command = new GenerarReporteCommand
            {
                TipoReporte = tipoReporte,
                FechaInicio = fechaTest.AddDays(-1),
                FechaFin = fechaTest,
                UsuarioSolicitanteId = _testUserId,
                IncluirGraficos = false, // Disable extra validation
                IncluirDetalles = false,
                IncluirResumenEjecutivo = true,
                NombrePersonalizado = tipoReporte == TipoReporte.Personalizado ? "Test" : null,
                Prioridad = tipoReporte == TipoReporte.Financiero ? NivelPrioridad.Alta : NivelPrioridad.Media
            };

            var comandas = new List<Comanda> { Comanda.Crear(meseroId: _testUserId, fechaCreacion: fechaTest.AddHours(-1), clienteId: null, mesaId: null) };
            _comandaRepositoryMock.Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comandas);

            var ordenes = new List<OrdenCompra> { OrdenCompra.Crear(Guid.NewGuid(), "Orden de prueba", fechaTest.AddHours(-1)) };
            _ordenCompraRepositoryMock.Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenes);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
} 