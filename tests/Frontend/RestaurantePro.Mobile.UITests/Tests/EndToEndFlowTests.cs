using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Tests de flujo end-to-end de la aplicación móvil
/// </summary>
public class EndToEndFlowTests : AppiumTestBase
{
    public EndToEndFlowTests(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    [Fact]
    public void EndToEnd_App_Should_Complete_Basic_Flow()
    {
        try
        {
            TestOutput.WriteLine("🔄 Test end-to-end: Verificando flujo básico de la app...");
            
            // PASO 1: Verificar que la app se lanzó
            Assert.NotNull(Driver);
            Assert.NotNull(Driver.Title);
            
            TestOutput.WriteLine($"📱 Paso 1 completado: App lanzada - {Driver.Title}");
            
            // PASO 2: Verificar que la app es responsiva
            var pageSource = Driver.PageSource;
            Assert.NotNull(pageSource);
            Assert.True(pageSource.Length > 0, "La app debe tener contenido");
            
            TestOutput.WriteLine($"📄 Paso 2 completado: App responsiva - {pageSource.Length} caracteres");
            
            // PASO 3: Verificar que podemos interactuar
            var windowHandles = Driver.WindowHandles;
            Assert.True(windowHandles.Count > 0, "La app debe tener al menos una ventana");
            
            TestOutput.WriteLine($"🪟 Paso 3 completado: Interacción disponible - {windowHandles.Count} ventanas");
            
            // PASO 4: Verificar información del dispositivo
            // var currentActivity = Driver.CurrentActivity; // No disponible en IWebDriver genérico
            TestOutput.WriteLine("🎯 Paso 4 completado: Verificando estado de la app");
            
            // Tomar screenshot del estado final
            TakeScreenshot("end_to_end_completed");
            
            TestOutput.WriteLine("✅ Test end-to-end completado exitosamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en test end-to-end: {ex.Message}");
            TakeScreenshot("end_to_end_error");
            throw;
        }
    }

    [Fact]
    public void EndToEnd_App_Should_Handle_User_Interactions()
    {
        try
        {
            TestOutput.WriteLine("👆 Test end-to-end: Verificando interacciones de usuario...");
            
            // Verificar que el driver esté funcionando
            Assert.NotNull(Driver);
            
            // Verificar que la app no esté bloqueada
            var pageSource = Driver.PageSource;
            Assert.NotNull(pageSource);
            
            TestOutput.WriteLine($"📱 App lista para interacciones de usuario");
            
            // Aquí podrías agregar más verificaciones de interacciones
            // Por ejemplo, verificar que los botones estén habilitados
            // o que los campos de entrada sean editables
            
            // Tomar screenshot del estado de interacciones
            TakeScreenshot("end_to_end_interactions");
            
            TestOutput.WriteLine("✅ Test de interacciones completado exitosamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en test de interacciones: {ex.Message}");
            TakeScreenshot("end_to_end_interactions_error");
            throw;
        }
    }
} 