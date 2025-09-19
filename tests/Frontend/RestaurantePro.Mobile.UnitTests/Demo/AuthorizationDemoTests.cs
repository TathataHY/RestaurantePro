using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Authorization;
using Xunit;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UnitTests.Demo;

/// <summary>
/// Demonstración práctica del sistema de autorización
/// Muestra cómo diferentes roles ven diferentes funcionalidades
/// </summary>
public class AuthorizationDemoTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthorizationDemoTests(ITestOutputHelper output)
    {
        _output = output;
        _mockAuthService = new Mock<IAuthService>();
    }

    [Fact]
    public async Task Demo_MeseroVsCocinero_ShouldHaveDifferentPermissions()
    {
        // Arrange
        var authService = new AuthorizationService(_mockAuthService.Object, NullLogger<AuthorizationService>.Instance);

        _output.WriteLine("🎭 DEMO: Sistema de Autorización - RestaurantePro Mobile");
        _output.WriteLine("============================================================");

        // ========== DEMO 1: MESERO ==========
        var meseroUser = new AuthUser
        {
            Id = 1,
            Email = "juan.mesero@restaurante.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Roles = new List<string> { "Mesero" }
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(meseroUser);

        _output.WriteLine("\n👨‍🍽️ USUARIO: MESERO (Juan Pérez)");
        _output.WriteLine("-----------------------------------");

        // Verificar permisos del mesero
        var meseroCanCreate = await authService.HasPermissionAsync(AppPermission.CrearComandas);
        var meseroCanModify = await authService.HasPermissionAsync(AppPermission.ModificarComandas);
        var meseroCanCook = await authService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
        var meseroCanBill = await authService.HasPermissionAsync(AppPermission.GenerarFacturas);
        var meseroCanManageTables = await authService.HasPermissionAsync(AppPermission.CambiarEstadoMesas);

        _output.WriteLine($"✅ Crear comandas: {meseroCanCreate}");
        _output.WriteLine($"✅ Modificar comandas: {meseroCanModify}");
        _output.WriteLine($"❌ Cocinar (preparaciones): {meseroCanCook}");
        _output.WriteLine($"❌ Generar facturas: {meseroCanBill}");
        _output.WriteLine($"✅ Gestionar mesas: {meseroCanManageTables}");

        // Verificar funcionalidades
        var meseroCanAccessComandas = await authService.CanAccessFeatureAsync(AppFeature.GestionComandas);
        var meseroCanAccessCocina = await authService.CanAccessFeatureAsync(AppFeature.Cocina);
        var meseroCanAccessCaja = await authService.CanAccessFeatureAsync(AppFeature.Caja);

        _output.WriteLine($"\n🎯 Funcionalidades disponibles:");
        _output.WriteLine($"   • Gestión Comandas: {meseroCanAccessComandas}");
        _output.WriteLine($"   • Cocina: {meseroCanAccessCocina}");
        _output.WriteLine($"   • Caja: {meseroCanAccessCaja}");

        // ========== DEMO 2: COCINERO ==========
        var cocineroUser = new AuthUser
        {
            Id = 2,
            Email = "carlos.cocinero@restaurante.com",
            Nombre = "Carlos",
            Apellido = "Ruiz",
            Roles = new List<string> { "Cocinero" }
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(cocineroUser);

        _output.WriteLine("\n👨‍🍳 USUARIO: COCINERO (Carlos Ruiz)");
        _output.WriteLine("-------------------------------------");

        // Verificar permisos del cocinero
        var cocineroCanCreate = await authService.HasPermissionAsync(AppPermission.CrearComandas);
        var cocineroCanCook = await authService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
        var cocineroCanComplete = await authService.HasPermissionAsync(AppPermission.CompletarPreparaciones);
        var cocineroCanCheckInventory = await authService.HasPermissionAsync(AppPermission.ConsultarDisponibilidadIngredientes);
        var cocineroCanBill = await authService.HasPermissionAsync(AppPermission.GenerarFacturas);

        _output.WriteLine($"❌ Crear comandas: {cocineroCanCreate}");
        _output.WriteLine($"✅ Actualizar preparaciones: {cocineroCanCook}");
        _output.WriteLine($"✅ Completar preparaciones: {cocineroCanComplete}");
        _output.WriteLine($"✅ Consultar inventario: {cocineroCanCheckInventory}");
        _output.WriteLine($"❌ Generar facturas: {cocineroCanBill}");

        // Verificar funcionalidades
        var cocineroCanAccessComandas = await authService.CanAccessFeatureAsync(AppFeature.GestionComandas);
        var cocineroCanAccessCocina = await authService.CanAccessFeatureAsync(AppFeature.Cocina);
        var cocineroCanAccessInventory = await authService.CanAccessFeatureAsync(AppFeature.ConsultaInventario);

        _output.WriteLine($"\n🎯 Funcionalidades disponibles:");
        _output.WriteLine($"   • Gestión Comandas: {cocineroCanAccessComandas}");
        _output.WriteLine($"   • Cocina: {cocineroCanAccessCocina}");
        _output.WriteLine($"   • Consulta Inventario: {cocineroCanAccessInventory}");

        // ========== DEMO 3: CAJERO ==========
        var cajeroUser = new AuthUser
        {
            Id = 3,
            Email = "ana.cajero@restaurante.com",
            Nombre = "Ana",
            Apellido = "López",
            Roles = new List<string> { "Cajero" }
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(cajeroUser);

        _output.WriteLine("\n💰 USUARIO: CAJERO (Ana López)");
        _output.WriteLine("--------------------------------");

        // Verificar permisos del cajero
        var cajeroCanCreate = await authService.HasPermissionAsync(AppPermission.CrearComandas);
        var cajeroCanBill = await authService.HasPermissionAsync(AppPermission.GenerarFacturas);
        var cajeroCanProcessPayments = await authService.HasPermissionAsync(AppPermission.ProcesarPagos);
        var cajeroCanManageTables = await authService.HasPermissionAsync(AppPermission.CambiarEstadoMesas);
        var cajeroCanCook = await authService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);

        _output.WriteLine($"❌ Crear comandas: {cajeroCanCreate}");
        _output.WriteLine($"✅ Generar facturas: {cajeroCanBill}");
        _output.WriteLine($"✅ Procesar pagos: {cajeroCanProcessPayments}");
        _output.WriteLine($"❌ Gestionar mesas: {cajeroCanManageTables}");
        _output.WriteLine($"❌ Cocinar: {cajeroCanCook}");

        // Verificar funcionalidades
        var cajeroCanAccessCaja = await authService.CanAccessFeatureAsync(AppFeature.Caja);
        var cajeroCanAccessClientes = await authService.CanAccessFeatureAsync(AppFeature.AtencionCliente);
        var cajeroCanAccessCocina = await authService.CanAccessFeatureAsync(AppFeature.Cocina);

        _output.WriteLine($"\n🎯 Funcionalidades disponibles:");
        _output.WriteLine($"   • Caja: {cajeroCanAccessCaja}");
        _output.WriteLine($"   • Atención Cliente: {cajeroCanAccessClientes}");
        _output.WriteLine($"   • Cocina: {cajeroCanAccessCocina}");

        // ========== DEMO 4: GERENTE (SÚPER USUARIO) ==========
        var gerenteUser = new AuthUser
        {
            Id = 4,
            Email = "luis.gerente@restaurante.com",
            Nombre = "Luis",
            Apellido = "García",
            Roles = new List<string> { "Gerente" }
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(gerenteUser);

        _output.WriteLine("\n👔 USUARIO: GERENTE (Luis García)");
        _output.WriteLine("-----------------------------------");

        // Verificar que el gerente puede hacer TODO
        var gerenteCanCreate = await authService.HasPermissionAsync(AppPermission.CrearComandas);
        var gerenteCanCook = await authService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones);
        var gerenteCanBill = await authService.HasPermissionAsync(AppPermission.GenerarFacturas);
        var gerenteCanSupervise = await authService.HasPermissionAsync(AppPermission.SupervisarOperaciones);
        var gerenteCanManageStaff = await authService.HasPermissionAsync(AppPermission.GestionarPersonalTurno);

        _output.WriteLine($"✅ Crear comandas: {gerenteCanCreate}");
        _output.WriteLine($"✅ Cocinar: {gerenteCanCook}");
        _output.WriteLine($"✅ Facturar: {gerenteCanBill}");
        _output.WriteLine($"✅ Supervisar: {gerenteCanSupervise}");
        _output.WriteLine($"✅ Gestionar personal: {gerenteCanManageStaff}");

        // Verificar funcionalidades
        var gerenteCanAccessAll = await authService.CanAccessFeatureAsync(AppFeature.Supervision);

        _output.WriteLine($"\n🎯 Funcionalidades disponibles:");
        _output.WriteLine($"   • Supervisión: {gerenteCanAccessAll}");
        _output.WriteLine($"   • ¡ACCESO COMPLETO A TODO! 🚀");

        _output.WriteLine("\n============================================================");
        _output.WriteLine("🎉 DEMO COMPLETADO - ¡El sistema funciona perfectamente!");

        // Asserts para verificar que la lógica es correcta
        Assert.True(meseroCanCreate && meseroCanModify && meseroCanManageTables);
        Assert.False(meseroCanCook || meseroCanBill);
        
        Assert.True(cocineroCanCook && cocineroCanComplete && cocineroCanCheckInventory);
        Assert.False(cocineroCanCreate || cocineroCanBill);
        
        Assert.True(cajeroCanBill && cajeroCanProcessPayments);
        Assert.False(cajeroCanCreate || cajeroCanManageTables || cajeroCanCook);
        
        Assert.True(gerenteCanCreate && gerenteCanCook && gerenteCanBill && gerenteCanSupervise);
    }

    [Fact]
    public async Task Demo_MultipleRoles_ShouldCombinePermissions()
    {
        // Arrange
        var authService = new AuthorizationService(_mockAuthService.Object, NullLogger<AuthorizationService>.Instance);

        var multiRoleUser = new AuthUser
        {
            Id = 5,
            Email = "maria.multirole@restaurante.com",
            Nombre = "María",
            Apellido = "Sánchez",
            Roles = new List<string> { "Mesero", "Cajero" } // Doble rol!
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(multiRoleUser);

        _output.WriteLine("🎭 DEMO: Usuario con Múltiples Roles");
        _output.WriteLine("==================================================");
        _output.WriteLine("\n👩‍💼 USUARIO: MESERO + CAJERO (María Sánchez)");
        _output.WriteLine("----------------------------------------------");

        // Act & Assert
        var canCreateComandas = await authService.HasPermissionAsync(AppPermission.CrearComandas); // Mesero
        var canManageTables = await authService.HasPermissionAsync(AppPermission.CambiarEstadoMesas); // Mesero
        var canGenerateBills = await authService.HasPermissionAsync(AppPermission.GenerarFacturas); // Cajero
        var canProcessPayments = await authService.HasPermissionAsync(AppPermission.ProcesarPagos); // Cajero
        var canCook = await authService.HasPermissionAsync(AppPermission.ActualizarEstadoPreparaciones); // Ninguno

        _output.WriteLine($"✅ Crear comandas (Mesero): {canCreateComandas}");
        _output.WriteLine($"✅ Gestionar mesas (Mesero): {canManageTables}");
        _output.WriteLine($"✅ Generar facturas (Cajero): {canGenerateBills}");
        _output.WriteLine($"✅ Procesar pagos (Cajero): {canProcessPayments}");
        _output.WriteLine($"❌ Cocinar (Ninguno): {canCook}");

        _output.WriteLine($"\n🎯 ¡María puede hacer trabajo de MESERO Y CAJERO!");

        // Verificar que efectivamente tiene permisos combinados
        Assert.True(canCreateComandas && canManageTables); // Permisos de mesero
        Assert.True(canGenerateBills && canProcessPayments); // Permisos de cajero
        Assert.False(canCook); // No tiene permisos de cocinero
    }
}
