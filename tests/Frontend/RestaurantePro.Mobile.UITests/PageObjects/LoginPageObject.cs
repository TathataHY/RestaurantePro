using OpenQA.Selenium.Appium.Android;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.PageObjects;

public class LoginPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    // Elementos de la página de login - Selectores flexibles para detectar la UI real
    private By EmailEntry => By.XPath("//android.widget.EditText[1]"); // Primer EditText
    private By PasswordEntry => By.XPath("//android.widget.EditText[2]"); // Segundo EditText
    private By LoginButton => By.XPath("//android.widget.Button | //android.widget.TextView[contains(@text, 'Login')] | //android.widget.TextView[contains(@text, 'Iniciar')]");
    private By ErrorMessage => By.XPath("//android.widget.TextView[contains(@text, 'Error')] | //android.widget.TextView[contains(@text, 'Incorrecto')]");
    private By LoadingIndicator => By.XPath("//android.widget.ProgressBar | //android.widget.TextView[contains(@text, 'Cargando')]");
    private By RememberMeCheckbox => By.XPath("//android.widget.CheckBox | //android.widget.TextView[contains(@text, 'Recordar')]");
    
    // Elementos adicionales para detectar la página de login
    private By PageTitle => By.XPath("//android.widget.TextView[contains(@text, 'RestaurantePro')] | //android.widget.TextView[contains(@text, 'Login')] | //android.widget.TextView[contains(@text, 'Iniciar')]");
    private By LoginForm => By.XPath("//android.widget.FrameLayout | //android.widget.LinearLayout | //android.widget.RelativeLayout");

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
            Thread.Sleep(2000);
            
            _testOutput.WriteLine($"=== DEBUG: Ingresando email: '{email}' ===");
            
            // Buscar el primer EditText disponible (probablemente el campo de email)
            var emailField = _driver.FindElement(By.XPath("//android.widget.EditText[1]"));
            
            // Verificar que el campo esté disponible
            _testOutput.WriteLine($"Campo email encontrado: Visible={emailField.Displayed}, Enabled={emailField.Enabled}");
            
            // Limpiar el campo completamente
            emailField.Clear();
            Thread.Sleep(500);
            
            // Intentar múltiples métodos para ingresar el email
            try
            {
                // Método 1: SendKeys normal
                emailField.SendKeys(email);
                _testOutput.WriteLine("✅ Email ingresado con SendKeys normal");
            }
            catch (Exception ex1)
            {
                _testOutput.WriteLine($"⚠️ SendKeys normal falló: {ex1.Message}");
                
                try
                {
                    // Método 2: Click y luego SendKeys
                    emailField.Click();
                    Thread.Sleep(500);
                    emailField.SendKeys(email);
                    _testOutput.WriteLine("✅ Email ingresado con Click + SendKeys");
                }
                catch (Exception ex2)
                {
                    _testOutput.WriteLine($"⚠️ Click + SendKeys falló: {ex2.Message}");
                    
                    try
                    {
                        // Método 3: Usar Actions
                        var actions = new OpenQA.Selenium.Interactions.Actions(_driver);
                        actions.MoveToElement(emailField).Click().SendKeys(email).Perform();
                        _testOutput.WriteLine("✅ Email ingresado con Actions");
                    }
                    catch (Exception ex3)
                    {
                        _testOutput.WriteLine($"⚠️ Actions falló: {ex3.Message}");
                        
                        // Método 4: Enviar caracter por caracter
                        emailField.Clear();
                        foreach (char c in email)
                        {
                            emailField.SendKeys(c.ToString());
                            Thread.Sleep(50);
                        }
                        _testOutput.WriteLine("✅ Email ingresado carácter por carácter");
                    }
                }
            }
            
            // Verificar que el email se ingresó correctamente
            Thread.Sleep(1000);
            var enteredText = emailField.Text;
            _testOutput.WriteLine($"Texto ingresado en el campo: '{enteredText}'");
            _testOutput.WriteLine($"Email esperado: '{email}'");
            _testOutput.WriteLine($"Coinciden: {enteredText == email}");
            
            _testOutput.WriteLine("=== FIN DEBUG EMAIL ===");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error al ingresar email: {ex.Message}");
            throw;
        }
    }

    public void EnterPassword(string password)
    {
        try
        {
            // Esperar un poco para que los elementos estén disponibles
            Thread.Sleep(2000);
            
            // Primero, contar cuántos EditText hay disponibles
            var allEditTexts = _driver.FindElements(By.XPath("//android.widget.EditText"));
            _testOutput.WriteLine($"EditTexts encontrados: {allEditTexts.Count}");
            
            if (allEditTexts.Count >= 2)
            {
                // Si hay al menos 2 EditText, usar el segundo para contraseña
                var passwordField = allEditTexts[1]; // Segundo EditText
                passwordField.Clear();
                passwordField.SendKeys(password);
                _testOutput.WriteLine("Contraseña ingresada en segundo campo");
            }
            else if (allEditTexts.Count == 1)
            {
                // Si solo hay 1 EditText, buscar un campo específico de contraseña
                var passwordField = _driver.FindElement(By.XPath("//android.widget.EditText[@password='true'] | //android.widget.EditText[contains(@text, 'Contraseña')] | //android.widget.EditText[contains(@hint, 'Contraseña')]"));
                passwordField.Clear();
                passwordField.SendKeys(password);
                _testOutput.WriteLine("Contraseña ingresada en campo específico de contraseña");
            }
            else
            {
                throw new InvalidOperationException("No se encontraron campos de entrada");
            }
        }
        catch (OpenQA.Selenium.NoSuchElementException)
        {
            _testOutput.WriteLine("No se pudo encontrar campo específico de contraseña, usando el primer EditText disponible");
            var passwordField = _driver.FindElement(By.XPath("//android.widget.EditText[1]"));
            passwordField.Clear();
            passwordField.SendKeys(password);
            _testOutput.WriteLine("Contraseña ingresada en primer campo (fallback)");
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
            // Esperar un poco para que los elementos estén disponibles
            Thread.Sleep(3000);
            
            _testOutput.WriteLine("=== DEBUG: Buscando botón de login ===");
            
            // Primero, hacer debug para ver qué elementos tenemos
            var allButtons = _driver.FindElements(By.XPath("//android.widget.Button"));
            var allTextViews = _driver.FindElements(By.XPath("//android.widget.TextView"));
            
            _testOutput.WriteLine($"Botones encontrados: {allButtons.Count}");
            _testOutput.WriteLine($"TextViews encontrados: {allTextViews.Count}");
            
            // Mostrar información de los botones
            foreach (var button in allButtons)
            {
                try
                {
                    _testOutput.WriteLine($"Botón: '{button.Text}' - Visible: {button.Displayed} - Enabled: {button.Enabled}");
                }
                catch { }
            }
            
            // Mostrar información de los TextViews
            foreach (var textView in allTextViews.Take(10))
            {
                try
                {
                    _testOutput.WriteLine($"TextView: '{textView.Text}' - Visible: {textView.Displayed}");
                }
                catch { }
            }
            
            // ESTRATEGIA 1: Intentar encontrar un botón de login
            try
            {
                var loginButton = _driver.FindElement(LoginButton);
                _testOutput.WriteLine($"Botón encontrado: '{loginButton.Text}' - Visible: {loginButton.Displayed} - Enabled: {loginButton.Enabled}");
                
                // Verificar que el botón esté habilitado
                if (!loginButton.Enabled)
                {
                    _testOutput.WriteLine("⚠️ El botón está deshabilitado, esperando...");
                    Thread.Sleep(3000);
                }
                
                // ESTRATEGIA 1A: Click normal
                loginButton.Click();
                _testOutput.WriteLine("✅ Botón de login clickeado normalmente");
                Thread.Sleep(2000);
                
                // ESTRATEGIA 1B: Click con Actions
                var actions = new OpenQA.Selenium.Interactions.Actions(_driver);
                actions.MoveToElement(loginButton).Click().Perform();
                _testOutput.WriteLine("✅ Botón clickeado con Actions");
                Thread.Sleep(2000);
                
                // ESTRATEGIA 1C: Click con Enter en el último campo
                var lastEditText = _driver.FindElement(By.XPath("//android.widget.EditText[last()]"));
                lastEditText.SendKeys(OpenQA.Selenium.Keys.Enter);
                _testOutput.WriteLine("✅ Enter presionado en último campo");
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                _testOutput.WriteLine($"⚠️ Estrategia 1 falló: {ex.Message}");
            }
            
            // ESTRATEGIA 2: Buscar TextView clickeable
            try
            {
                _testOutput.WriteLine("🔄 ESTRATEGIA 2: Buscando TextView clickeable...");
                var clickableTexts = _driver.FindElements(By.XPath("//android.widget.TextView[contains(@text, 'Login')] | //android.widget.TextView[contains(@text, 'Iniciar')] | //android.widget.TextView[contains(@text, 'Entrar')] | //android.widget.TextView[contains(@text, 'Sesión')]"));
                
                if (clickableTexts.Count > 0)
                {
                    _testOutput.WriteLine($"Encontrados {clickableTexts.Count} TextViews clickeables");
                    foreach (var text in clickableTexts)
                    {
                        try
                        {
                            _testOutput.WriteLine($"Intentando click en TextView: '{text.Text}'");
                            text.Click();
                            _testOutput.WriteLine("✅ TextView de login clickeado");
                            Thread.Sleep(2000);
                        }
                        catch (Exception ex)
                        {
                            _testOutput.WriteLine($"⚠️ Click en TextView falló: {ex.Message}");
                        }
                    }
                }
                else
                {
                    _testOutput.WriteLine("No se encontraron TextViews clickeables");
                }
            }
            catch (Exception ex)
            {
                _testOutput.WriteLine($"⚠️ Estrategia 2 falló: {ex.Message}");
            }
            
            // ESTRATEGIA 3: Click con coordenadas absolutas
            try
            {
                _testOutput.WriteLine("🔄 ESTRATEGIA 3: Click con coordenadas...");
                var loginButton = _driver.FindElement(LoginButton);
                var location = loginButton.Location;
                var size = loginButton.Size;
                var centerX = location.X + (size.Width / 2);
                var centerY = location.Y + (size.Height / 2);
                
                _testOutput.WriteLine($"Coordenadas del botón: ({centerX}, {centerY})");
                
                // Usar Actions para click en coordenadas
                var actions2 = new OpenQA.Selenium.Interactions.Actions(_driver);
                actions2.MoveByOffset(centerX, centerY).Click().Perform();
                _testOutput.WriteLine($"✅ Botón clickeado con coordenadas: ({centerX}, {centerY})");
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                _testOutput.WriteLine($"⚠️ Estrategia 3 falló: {ex.Message}");
            }
            
            // ESTRATEGIA 4: Click en cualquier botón disponible
            try
            {
                _testOutput.WriteLine("🔄 ESTRATEGIA 4: Click en cualquier botón...");
                var allButtons2 = _driver.FindElements(By.XPath("//android.widget.Button"));
                
                foreach (var button in allButtons2)
                {
                    try
                    {
                        _testOutput.WriteLine($"Intentando click en botón: '{button.Text}'");
                        button.Click();
                        _testOutput.WriteLine($"✅ Botón '{button.Text}' clickeado");
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        _testOutput.WriteLine($"⚠️ Click en botón '{button.Text}' falló: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _testOutput.WriteLine($"⚠️ Estrategia 4 falló: {ex.Message}");
            }
            
            // VERIFICAR que el click realmente funcionó
            _testOutput.WriteLine("=== VERIFICANDO SI EL CLICK FUNCIONÓ ===");
            
            // Verificar si apareció algún mensaje de error o éxito
            var errorMessages = _driver.FindElements(By.XPath("//android.widget.TextView[contains(@text, 'Error')] | //android.widget.TextView[contains(@text, 'Incorrecto')] | //android.widget.TextView[contains(@text, 'Usuario')] | //android.widget.TextView[contains(@text, 'contraseña')] | //android.widget.TextView[contains(@text, 'inválido')] | //android.widget.TextView[contains(@text, 'incorrecta')]"));
            var successMessages = _driver.FindElements(By.XPath("//android.widget.TextView[contains(@text, 'Bienvenido')] | //android.widget.TextView[contains(@text, 'Dashboard')] | //android.widget.TextView[contains(@text, 'Inicio')] | //android.widget.TextView[contains(@text, 'Principal')]"));
            
            _testOutput.WriteLine($"Mensajes de error encontrados: {errorMessages.Count}");
            _testOutput.WriteLine($"Mensajes de éxito encontrados: {successMessages.Count}");
            
            // Mostrar todos los mensajes encontrados
            foreach (var msg in errorMessages)
            {
                try
                {
                    _testOutput.WriteLine($"Mensaje de error: '{msg.Text}'");
                }
                catch { }
            }
            
            foreach (var msg in successMessages)
            {
                try
                {
                    _testOutput.WriteLine($"Mensaje de éxito: '{msg.Text}'");
                }
                catch { }
            }
            
            // Verificar si los campos se limpiaron (indicador de que el click funcionó)
            try
            {
                var emailField = _driver.FindElement(By.XPath("//android.widget.EditText[1]"));
                var passwordField = _driver.FindElement(By.XPath("//android.widget.EditText[2]"));
                
                _testOutput.WriteLine($"Campo email después del click: '{emailField.Text}'");
                _testOutput.WriteLine($"Campo contraseña después del click: '{passwordField.Text}'");
            }
            catch (Exception ex)
            {
                _testOutput.WriteLine($"⚠️ No se pudieron verificar los campos: {ex.Message}");
            }
            
            _testOutput.WriteLine("=== FIN VERIFICACIÓN ===");
            _testOutput.WriteLine("=== FIN DEBUG ===");
        }
        catch (OpenQA.Selenium.NoSuchElementException)
        {
            _testOutput.WriteLine("❌ No se encontró botón de login, buscando TextView clickeable...");
            
            var clickableTexts = _driver.FindElements(By.XPath("//android.widget.TextView[contains(@text, 'Login')] | //android.widget.TextView[contains(@text, 'Iniciar')] | //android.widget.TextView[contains(@text, 'Entrar')]"));
            
            if (clickableTexts.Count > 0)
            {
                clickableTexts[0].Click();
                _testOutput.WriteLine("✅ TextView de login clickeado");
            }
            else
            {
                // Si no hay nada clickeable, simular presionar Enter en el último campo
                _testOutput.WriteLine("⚠️ No se encontró elemento clickeable, simulando Enter...");
                var lastEditText = _driver.FindElement(By.XPath("//android.widget.EditText[last()]"));
                lastEditText.SendKeys(Keys.Enter);
                _testOutput.WriteLine("✅ Enter presionado en último campo");
            }
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error al hacer click en login: {ex.Message}");
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
            _testOutput.WriteLine("=== DEBUG: Verificando mensajes de error ===");
            
            // Esperar un poco para que aparezcan los mensajes
            Thread.Sleep(3000);
            
            // Buscar mensajes de error con múltiples selectores
            var errorSelectors = new[]
            {
                "//android.widget.TextView[contains(@text, 'Error')]",
                "//android.widget.TextView[contains(@text, 'Incorrecto')]",
                "//android.widget.TextView[contains(@text, 'Usuario')]",
                "//android.widget.TextView[contains(@text, 'contraseña')]",
                "//android.widget.TextView[contains(@text, 'inválido')]",
                "//android.widget.TextView[contains(@text, 'incorrecta')]",
                "//android.widget.TextView[contains(@text, 'HTTP')]",
                "//android.widget.TextView[contains(@text, 'Unauthorized')]",
                "//android.widget.TextView[contains(@text, '401')]",
                "//android.widget.TextView[contains(@text, 'Credenciales')]"
            };
            
            foreach (var selector in errorSelectors)
            {
                try
                {
                    var elements = _driver.FindElements(By.XPath(selector));
                    if (elements.Count > 0)
                    {
                        foreach (var element in elements)
                        {
                            if (element.Displayed)
                            {
                                _testOutput.WriteLine($"✅ Mensaje de error encontrado: '{element.Text}' con selector: {selector}");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _testOutput.WriteLine($"⚠️ Error con selector {selector}: {ex.Message}");
                }
            }
            
            // Si no encontramos mensajes de error, buscar cualquier TextView que pueda ser un mensaje
            var allTextViews = _driver.FindElements(By.XPath("//android.widget.TextView"));
            _testOutput.WriteLine($"TextViews totales en la página: {allTextViews.Count}");
            
            foreach (var textView in allTextViews)
            {
                try
                {
                    if (textView.Displayed)
                    {
                        var text = textView.Text;
                        if (!string.IsNullOrEmpty(text) && 
                            (text.Contains("Error") || text.Contains("Incorrecto") || text.Contains("HTTP") || 
                             text.Contains("401") || text.Contains("Unauthorized") || text.Contains("Credenciales")))
                        {
                            _testOutput.WriteLine($"✅ Mensaje de error encontrado en TextView: '{text}'");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _testOutput.WriteLine($"⚠️ Error verificando TextView: {ex.Message}");
                }
            }
            
            _testOutput.WriteLine("❌ No se encontraron mensajes de error");
            _testOutput.WriteLine("=== FIN DEBUG MENSAJES DE ERROR ===");
            return false;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error verificando mensajes de error: {ex.Message}");
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
            Thread.Sleep(3000);
            
            // Primero hacer debug para ver qué elementos tenemos
            DebugPageElements();
            
            // Verificar elementos básicos para confirmar que estamos en la página de login
            var editTexts = _driver.FindElements(By.XPath("//android.widget.EditText"));
            
            _testOutput.WriteLine($"EditText elements found: {editTexts.Count}");
            
            // Si tenemos al menos 1 EditText, probablemente estamos en una página de login
            // También verificar si hay algún texto relacionado con login
            var hasLoginText = _driver.FindElements(By.XPath("//android.widget.TextView[contains(@text, 'Login')] | //android.widget.TextView[contains(@text, 'Iniciar')] | //android.widget.TextView[contains(@text, 'Email')] | //android.widget.TextView[contains(@text, 'Contraseña')]")).Count > 0;
            
            _testOutput.WriteLine($"Has login-related text: {hasLoginText}");
            
            // Si encontramos al menos 1 EditText y texto relacionado con login, estamos en la página correcta
            return editTexts.Count >= 1 && hasLoginText;
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