using OpenQA.Selenium.Appium.Android;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.PageObjects;

public class LoginPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    // Elementos de la página de login - Selectores actualizados para SimpleLoginPage
    private By EmailEntry => By.XPath("//android.widget.EditText[@content-desc='EmailEntry']");
    private By PasswordEntry => By.XPath("//android.widget.EditText[@content-desc='PasswordEntry']");
    private By LoginButton => By.XPath("//android.widget.Button[@content-desc='LoginButton']");
    private By ErrorMessage => By.XPath("//android.widget.TextView[@content-desc='ErrorMessageLabel']");
    private By LoadingIndicator => By.XPath("//android.widget.ProgressBar");
    private By RememberMeCheckbox => By.XPath("//android.widget.CheckBox[@content-desc='RememberMeCheckBox']");
    
    // Elementos adicionales para detectar la página de login
    private By PageTitle => By.XPath("//android.widget.TextView[@text='RestaurantePro']");
    private By LoginForm => By.XPath("//android.widget.FrameLayout");

    public LoginPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    public void EnterEmail(string email)
    {
        try
        {
            // Esperar un poco para que los elementos estén disponibles
            Thread.Sleep(1000);
            
            // Usar el selector específico para EmailEntry
            var emailField = _driver.FindElement(EmailEntry);
            emailField.Clear();
            emailField.SendKeys(email);
            _testOutput.WriteLine($"Email ingresado: {email}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al ingresar email: {ex.Message}");
            throw;
        }
    }

    public void EnterPassword(string password)
    {
        try
        {
            // Esperar un poco para que los elementos estén disponibles
            Thread.Sleep(1000);
            
            // Usar el selector específico para PasswordEntry
            var passwordField = _driver.FindElement(PasswordEntry);
            passwordField.Clear();
            passwordField.SendKeys(password);
            _testOutput.WriteLine("Contraseña ingresada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al ingresar contraseña: {ex.Message}");
            throw;
        }
    }

    public void ClickLoginButton()
    {
        try
        {
            var loginButton = _driver.FindElement(LoginButton);
            loginButton.Click();
            _testOutput.WriteLine("Botón de login clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al hacer click en login: {ex.Message}");
            throw;
        }
    }

    public void Login(string email, string password)
    {
        EnterEmail(email);
        EnterPassword(password);
        ClickLoginButton();
    }

    public bool IsErrorMessageDisplayed()
    {
        try
        {
            var errorElement = _driver.FindElement(ErrorMessage);
            return errorElement.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetErrorMessage()
    {
        try
        {
            var errorElement = _driver.FindElement(ErrorMessage);
            return errorElement.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener mensaje de error: {ex.Message}");
            return string.Empty;
        }
    }

    public bool IsLoadingIndicatorDisplayed()
    {
        try
        {
            var loadingElement = _driver.FindElement(LoadingIndicator);
            return loadingElement.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public void WaitForLoginToComplete()
    {
        // Esperar a que desaparezca el indicador de carga
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        wait.Until(driver => !IsLoadingIndicatorDisplayed());
    }

    public bool IsLoginPageDisplayed()
    {
        try
        {
            // Esperar un poco para que la página se cargue completamente
            Thread.Sleep(2000);
            
            // Verificar elementos básicos para confirmar que estamos en la página de login
            // Según el debug, solo tenemos 2 EditText y 1 Button, no TextView
            var loginButton = _driver.FindElement(LoginButton);
            var editTexts = _driver.FindElements(By.XPath("//android.widget.EditText"));
            
            _testOutput.WriteLine($"Login button displayed: {loginButton.Displayed}");
            _testOutput.WriteLine($"EditText elements found: {editTexts.Count}");
            
            // Si encontramos el botón de login y al menos 2 EditText, estamos en la página correcta
            return loginButton.Displayed && editTexts.Count >= 2;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error verificando página de login: {ex.Message}");
            return false;
        }
    }

    public void ToggleRememberMe()
    {
        try
        {
            var rememberMeCheckbox = _driver.FindElement(RememberMeCheckbox);
            rememberMeCheckbox.Click();
            _testOutput.WriteLine("Checkbox 'Recordarme' toggled");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al togglear 'Recordarme': {ex.Message}");
        }
    }
    
    public void DebugPageElements()
    {
        try
        {
            _testOutput.WriteLine("=== DEBUG: Elementos disponibles en la página ===");
            
            // Buscar todos los elementos de texto
            var textElements = _driver.FindElements(By.XPath("//android.widget.TextView"));
            _testOutput.WriteLine($"Elementos TextView encontrados: {textElements.Count}");
            
            foreach (var element in textElements.Take(5))
            {
                try
                {
                    _testOutput.WriteLine($"TextView: '{element.Text}' - Visible: {element.Displayed}");
                }
                catch { }
            }
            
            // Buscar todos los elementos EditText
            var editElements = _driver.FindElements(By.XPath("//android.widget.EditText"));
            _testOutput.WriteLine($"Elementos EditText encontrados: {editElements.Count}");
            
            // Buscar todos los elementos Button
            var buttonElements = _driver.FindElements(By.XPath("//android.widget.Button"));
            _testOutput.WriteLine($"Elementos Button encontrados: {buttonElements.Count}");
            
            foreach (var element in buttonElements)
            {
                try
                {
                    _testOutput.WriteLine($"Button: '{element.Text}' - Visible: {element.Displayed}");
                }
                catch { }
            }
            
            _testOutput.WriteLine("=== FIN DEBUG ===");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error en debug: {ex.Message}");
        }
    }
} 