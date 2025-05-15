# Plan de Pruebas - RestaurantePro

Este documento describe la estrategia y plan de pruebas para el sistema RestaurantePro, abarcando todos los componentes: aplicación móvil, aplicación web y API.

## Tipos de Pruebas

### 1. Pruebas Unitarias

Las pruebas unitarias se centrarán en probar componentes individuales de forma aislada, asegurando que cada unidad funcione correctamente.

#### Componentes a probar:

**API:**
- Controladores
- Servicios de aplicación
- Manejadores de comandos y consultas
- Validadores

**Aplicación Móvil:**
- ViewModels
- Servicios
- Convertidores
- Utilidades

**Aplicación Web:**
- Componentes Base
- Servicios de datos
- Funciones auxiliares
- Validadores

#### Ejemplo de prueba unitaria para ViewModel:

```csharp
[TestClass]
public class MesasViewModelTests
{
    private Mock<IMesaService> _mesaServiceMock;
    private Mock<INavigationService> _navigationServiceMock;
    private MesasViewModel _viewModel;
    
    [TestInitialize]
    public void Initialize()
    {
        _mesaServiceMock = new Mock<IMesaService>();
        _navigationServiceMock = new Mock<INavigationService>();
        
        _viewModel = new MesasViewModel(_mesaServiceMock.Object, _navigationServiceMock.Object);
    }
    
    [TestMethod]
    public async Task LoadMesasCommand_ShouldPopulateMesasCollection()
    {
        // Arrange
        var mesas = new List<Mesa>
        {
            new Mesa { Id = 1, Numero = "1", Capacidad = 4, Estado = EstadoMesa.Libre },
            new Mesa { Id = 2, Numero = "2", Capacidad = 2, Estado = EstadoMesa.Ocupada }
        };
        
        _mesaServiceMock.Setup(m => m.GetMesasAsync())
            .ReturnsAsync(mesas);
            
        // Act
        await _viewModel.LoadMesasCommand.ExecuteAsync(null);
        
        // Assert
        Assert.AreEqual(2, _viewModel.Mesas.Count);
        Assert.AreEqual("1", _viewModel.Mesas[0].Numero);
        Assert.AreEqual("2", _viewModel.Mesas[1].Numero);
        Assert.IsFalse(_viewModel.IsBusy);
    }
}
```

#### Ejemplo de prueba unitaria para controlador API:

```csharp
[TestClass]
public class MesasControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private MesasController _controller;
    
    [TestInitialize]
    public void Initialize()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MesasController(_mediatorMock.Object);
    }
    
    [TestMethod]
    public async Task GetMesas_ShouldReturnOkWithMesas()
    {
        // Arrange
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = 1, Numero = "1", Capacidad = 4, Estado = "Libre" },
            new MesaDto { Id = 2, Numero = "2", Capacidad = 2, Estado = "Ocupada" }
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMesasQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesas);
            
        // Act
        var result = await _controller.GetMesas();
        
        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        var returnedMesas = okResult.Value as List<MesaDto>;
        Assert.IsNotNull(returnedMesas);
        Assert.AreEqual(2, returnedMesas.Count);
    }
}
```

### 2. Pruebas de Integración

Las pruebas de integración verificarán que los componentes funcionen correctamente cuando interactúan entre sí.

#### Áreas a probar:

**API:**
- Flujo completo de solicitud/respuesta
- Acceso a datos con repositorios reales
- Integración con servicios externos
- Validaciones y manejo de errores

**Aplicación Móvil y Web:**
- Comunicación con API
- Persistencia y carga de datos
- Navegación entre pantallas
- Actualización de UI basada en cambios de estado

#### Ejemplo de prueba de integración para API:

```csharp
[TestClass]
public class MesasIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private ApplicationDbContext _dbContext;
    
    [TestInitialize]
    public void Initialize()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar la conexión a BD real por una en memoria
                    var descriptor = services.SingleOrDefault(d => 
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                        
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestingDb");
                    });
                });
            });
            
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Seed test data
        _dbContext.Mesas.Add(new Mesa { Id = 1, Numero = "1", Capacidad = 4, Estado = EstadoMesa.Libre });
        _dbContext.Mesas.Add(new Mesa { Id = 2, Numero = "2", Capacidad = 2, Estado = EstadoMesa.Ocupada });
        _dbContext.SaveChanges();
    }
    
    [TestMethod]
    public async Task GetMesas_ShouldReturnMesas()
    {
        // Act
        var response = await _client.GetAsync("/api/mesas");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<List<MesaDto>>();
        
        Assert.IsNotNull(content);
        Assert.AreEqual(2, content.Count);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _scope.Dispose();
        _factory.Dispose();
    }
}
```

### 3. Pruebas de UI

Las pruebas de UI verificarán la funcionalidad desde la perspectiva del usuario, probando flujos completos.

#### Enfoques:

**Aplicación Móvil:**
- Pruebas con Xamarin.UITest
- Pruebas manuales en dispositivos físicos
- Escenarios de conectividad variable

**Aplicación Web:**
- Pruebas con Selenium o Playwright
- Pruebas en diferentes navegadores
- Pruebas de accesibilidad
- Pruebas de respuesta en diferentes tamaños de pantalla

#### Ejemplo de prueba de UI para aplicación web:

```csharp
[TestClass]
public class WebUITests
{
    private IWebDriver _driver;
    
    [TestInitialize]
    public void Initialize()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Window.Maximize();
        
        // Login
        _driver.Navigate().GoToUrl("https://localhost:5001/login");
        _driver.FindElement(By.Id("email")).SendKeys("admin@restaurantepro.com");
        _driver.FindElement(By.Id("password")).SendKeys("Admin123!");
        _driver.FindElement(By.Id("loginButton")).Click();
        
        // Esperar que redireccione al dashboard
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.Contains("/dashboard"));
    }
    
    [TestMethod]
    public void Dashboard_ShouldDisplayStats()
    {
        // Arrancar desde el dashboard
        Assert.IsTrue(_driver.Url.Contains("/dashboard"));
        
        // Verificar que los elementos clave estén presentes
        var ventasDiarias = _driver.FindElement(By.Id("ventasDiarias"));
        var comandasCompletadas = _driver.FindElement(By.Id("comandasCompletadas"));
        var ventasPorHoraChart = _driver.FindElement(By.Id("ventasPorHoraChart"));
        
        Assert.IsNotNull(ventasDiarias);
        Assert.IsNotNull(comandasCompletadas);
        Assert.IsNotNull(ventasPorHoraChart);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
    }
}
```

### 4. Pruebas de Rendimiento

Estas pruebas evaluarán el rendimiento del sistema bajo diferentes cargas.

#### Escenarios:

- Cargar simultáneamente varias mesas y comandas
- Generar reportes complejos con grandes volúmenes de datos
- Simular múltiples usuarios accediendo concurrentemente
- Medir tiempos de respuesta en operaciones críticas

#### Herramientas:

- JMeter para pruebas de carga de la API
- Application Insights para monitoreo en tiempo real
- Perfil de rendimiento para la aplicación móvil

### 5. Pruebas de Seguridad

Las pruebas de seguridad buscarán vulnerabilidades y garantizarán la protección de datos.

#### Áreas a evaluar:

- Autenticación y autorización
- Protección contra inyecciones SQL
- Seguridad de comunicaciones (HTTPS)
- Almacenamiento seguro de credenciales
- Protección contra CSRF, XSS y otros ataques

## Plan de Ejecución

### Fase 1: Configuración

1. Configurar entornos de prueba (Desarrollo, QA, Pre-producción)
2. Implementar pipeline de CI/CD con ejecución automática de pruebas
3. Configurar herramientas de monitoreo y análisis de cobertura

### Fase 2: Desarrollo de Pruebas

1. Desarrollar pruebas unitarias a medida que se implementa el código
2. Implementar pruebas de integración para API y servicios principales
3. Diseñar casos de prueba de UI para flujos críticos

### Fase 3: Ejecución de Pruebas Previas al Lanzamiento

1. Ejecutar suite completa de pruebas unitarias y de integración
2. Realizar pruebas de rendimiento y seguridad
3. Ejecutar pruebas de UI en diferentes dispositivos y navegadores
4. Realizar pruebas exploratorias manuales

### Fase 4: Pruebas Continuas

1. Ejecutar pruebas unitarias y de integración en cada commit
2. Programar pruebas de regresión automatizadas semanales
3. Monitorear rendimiento y errores en entorno de producción

## Estrategia de Reportes

Se generarán los siguientes reportes:

- Cobertura de código por módulos
- Resultados de pruebas automatizadas
- Defectos encontrados y su resolución
- Métricas de rendimiento
- Resultados de pruebas de seguridad

## Criterios de Aceptación

Para que una funcionalidad se considere lista para producción, debe cumplir:

1. 80% mínimo de cobertura de pruebas unitarias
2. Todas las pruebas de integración exitosas
3. Validación de UI en dispositivos/navegadores objetivo
4. Tiempos de respuesta dentro de parámetros aceptables:
   - API: < 500ms para operaciones regulares
   - Móvil: < 2s para carga de pantallas
   - Web: < 3s para carga de dashboard
5. Sin vulnerabilidades de seguridad críticas

## Responsabilidades

- **Desarrolladores**: Pruebas unitarias
- **QA**: Pruebas de integración, UI y exploratorias
- **DevOps**: Pruebas de rendimiento y configuración CI/CD
- **Especialista en Seguridad**: Pruebas de seguridad

## Herramientas

- **Pruebas Unitarias**: MSTest, xUnit, Moq
- **Pruebas de Integración**: WebApplicationFactory, TestServer
- **Pruebas de UI**: Selenium, Playwright, Xamarin.UITest
- **Pruebas de Rendimiento**: JMeter, Application Insights
- **Pruebas de Seguridad**: OWASP ZAP, SonarQube
- **CI/CD**: Azure DevOps, GitHub Actions
- **Análisis de Código**: SonarQube, StyleCop 