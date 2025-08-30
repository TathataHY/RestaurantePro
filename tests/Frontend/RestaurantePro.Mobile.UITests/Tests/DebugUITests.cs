using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.Tests;

public class DebugUITests : AppiumTestBaseWithRealApi
{
    private readonly LoginPageObject _loginPage;

    public DebugUITests(ITestOutputHelper testOutput) : base(testOutput)
    {
        _loginPage = new LoginPageObject(Driver, testOutput);
    }

    [Fact]
    public void Debug_WhatElementsAreActuallyOnScreen()
    {
        // Este test solo va a inspeccionar qué elementos están en la pantalla
        TestOutput.WriteLine("🔍 DEBUG: Inspeccionando elementos en la pantalla...");
        
        try
        {
            // Esperar un poco para que la aplicación se cargue
            Thread.Sleep(5000);
            
            // Ejecutar el debug de elementos
            _loginPage.DebugPageElements();
            
            // También buscar específicamente el botón de login con diferentes variaciones
            TestOutput.WriteLine("🔍 Buscando botón de login con diferentes variaciones...");
            
            var allButtons = Driver.FindElements(By.XPath("//android.widget.Button"));
            TestOutput.WriteLine($"Total de botones encontrados: {allButtons.Count}");
            
            foreach (var button in allButtons)
            {
                try
                {
                    var text = button.Text;
                    var enabled = button.Enabled;
                    var displayed = button.Displayed;
                    TestOutput.WriteLine($"Botón: Text='{text}', Enabled={enabled}, Displayed={displayed}");
                }
                catch (Exception ex)
                {
                    TestOutput.WriteLine($"Error leyendo botón: {ex.Message}");
                }
            }
            
            // Buscar elementos de texto
            var allTextViews = Driver.FindElements(By.XPath("//android.widget.TextView"));
            TestOutput.WriteLine($"Total de TextViews encontrados: {allTextViews.Count}");
            
            foreach (var textView in allTextViews.Take(10)) // Solo los primeros 10
            {
                try
                {
                    var text = textView.Text;
                    var displayed = textView.Displayed;
                    TestOutput.WriteLine($"TextView: Text='{text}', Displayed={displayed}");
                }
                catch (Exception ex)
                {
                    TestOutput.WriteLine($"Error leyendo TextView: {ex.Message}");
                }
            }
            
            // Buscar campos de entrada
            var allEditTexts = Driver.FindElements(By.XPath("//android.widget.EditText"));
            TestOutput.WriteLine($"Total de EditTexts encontrados: {allEditTexts.Count}");
            
            foreach (var editText in allEditTexts)
            {
                try
                {
                    var text = editText.Text;
                    var hint = editText.GetAttribute("content-desc");
                    var displayed = editText.Displayed;
                    TestOutput.WriteLine($"EditText: Text='{text}', Hint='{hint}', Displayed={displayed}");
                }
                catch (Exception ex)
                {
                    TestOutput.WriteLine($"Error leyendo EditText: {ex.Message}");
                }
            }
            
            // Buscar elementos con content-desc específicos
            TestOutput.WriteLine("🔍 Buscando elementos con content-desc específicos...");
            
            var emailElements = Driver.FindElements(By.XPath("//*[@content-desc='EmailEntry']"));
            TestOutput.WriteLine($"Elementos con content-desc='EmailEntry': {emailElements.Count}");
            
            var passwordElements = Driver.FindElements(By.XPath("//*[@content-desc='PasswordEntry']"));
            TestOutput.WriteLine($"Elementos con content-desc='PasswordEntry': {passwordElements.Count}");
            
            var loginButtonElements = Driver.FindElements(By.XPath("//*[@content-desc='LoginButton']"));
            TestOutput.WriteLine($"Elementos con content-desc='LoginButton': {loginButtonElements.Count}");
            
            // Buscar elementos por texto específico
            TestOutput.WriteLine("🔍 Buscando elementos por texto específico...");
            
            var loginTextElements = Driver.FindElements(By.XPath("//*[@text='Iniciar Sesión']"));
            TestOutput.WriteLine($"Elementos con texto='Iniciar Sesión': {loginTextElements.Count}");
            
            var restauranteProElements = Driver.FindElements(By.XPath("//*[@text='RestaurantePro']"));
            TestOutput.WriteLine($"Elementos con texto='RestaurantePro': {restauranteProElements.Count}");
            
            // Buscar elementos por clase específica
            TestOutput.WriteLine("🔍 Buscando elementos por clase específica...");
            
            var allInputs = Driver.FindElements(By.XPath("//*[contains(@class, 'EditText') or contains(@class, 'Entry')]"));
            TestOutput.WriteLine($"Elementos de entrada (EditText/Entry): {allInputs.Count}");
            
            var allLabels = Driver.FindElements(By.XPath("//*[contains(@class, 'TextView') or contains(@class, 'Label')]"));
            TestOutput.WriteLine($"Elementos de texto (TextView/Label): {allLabels.Count}");
            
            TestOutput.WriteLine("✅ Debug completado");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en debug: {ex.Message}");
            throw;
        }
    }
} 