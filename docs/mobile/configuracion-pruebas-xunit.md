# Configuración de Pruebas con XUnit - RestaurantePro Mobile

---

## 📋 **INFORMACIÓN DEL DOCUMENTO**
- **Framework Principal**: XUnit (moderno y recomendado para .NET 8)
- **Mocking**: Moq 4.20+
- **Coverage**: Coverlet
- **UI Testing**: Appium + .NET MAUI Testing Framework

---

## 📦 **PAQUETES NUGET REQUERIDOS**

### **Proyecto de Pruebas Unitarias**
```xml
<!-- RestaurantePro.Mobile.UnitTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- XUnit Core (moderno) -->
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    
    <!-- Mocking Framework -->
    <PackageReference Include="Moq" Version="4.20.69" />
    
    <!-- Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
    
    <!-- Assertions adicionales -->
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    
    <!-- AutoFixture para generación de datos -->
    <PackageReference Include="AutoFixture" Version="4.18.0" />
    <PackageReference Include="AutoFixture.Xunit2" Version="4.18.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Frontend\RestaurantePro.Mobile\RestaurantePro.Mobile.csproj" />
  </ItemGroup>
</Project>
```

### **Proyecto de Pruebas de Integración**
```xml
<!-- RestaurantePro.Mobile.IntegrationTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- XUnit -->
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    
    <!-- TestServer para pruebas de API -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    
    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.69" />
    
    <!-- Testcontainers para BD de pruebas -->
    <PackageReference Include="Testcontainers.SqlServer" Version="3.6.0" />
  </ItemGroup>
</Project>
```

### **Proyecto de Pruebas UI**
```xml
<!-- RestaurantePro.Mobile.UITests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- XUnit -->
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    
    <!-- Appium para UI Testing -->
    <PackageReference Include="Appium.WebDriver" Version="5.0.0-rc.1" />
    <PackageReference Include="Selenium.WebDriver" Version="4.15.0" />
    
    <!-- .NET MAUI Testing (si está disponible) -->
    <!-- <PackageReference Include="Microsoft.Maui.TestFramework" Version="8.0.0" /> -->
  </ItemGroup>
</Project>
```

---

## 🧪 **ESTRUCTURA DE PROYECTO DE PRUEBAS**

```
tests/
├── RestaurantePro.Mobile.UnitTests/
│   ├── ViewModels/
│   │   ├── AuthViewModelTests.cs
│   │   ├── MesasViewModelTests.cs
│   │   ├── ComandasViewModelTests.cs
│   │   └── PreparacionesViewModelTests.cs
│   ├── Services/
│   │   ├── ApiServiceTests.cs
│   │   ├── ComandaServiceTests.cs
│   │   ├── MesaServiceTests.cs
│   │   └── SyncOperationalServiceTests.cs
│   ├── Common/
│   │   ├── TestBase.cs
│   │   ├── MockFactory.cs
│   │   └── TestDataBuilder.cs
│   └── RestaurantePro.Mobile.UnitTests.csproj
│
├── RestaurantePro.Mobile.IntegrationTests/
│   ├── Api/
│   │   ├── AuthIntegrationTests.cs
│   │   ├── ComandasIntegrationTests.cs
│   │   └── MesasIntegrationTests.cs
│   ├── Services/
│   │   ├── SyncServiceIntegrationTests.cs
│   │   └── OfflineServiceIntegrationTests.cs
│   ├── Common/
│   │   ├── IntegrationTestBase.cs
│   │   └── TestApiFactory.cs
│   └── RestaurantePro.Mobile.IntegrationTests.csproj
│
└── RestaurantePro.Mobile.UITests/
    ├── PageObjects/
    │   ├── LoginPage.cs
    │   ├── MesasPage.cs
    │   ├── ComandasPage.cs
    │   └── PreparacionesPage.cs
    ├── Tests/
    │   ├── LoginFlowTests.cs
    │   ├── ComandaFlowTests.cs
    │   └── MesaFlowTests.cs
    ├── Common/
    │   ├── UITestBase.cs
    │   ├── AppiumDriver.cs
    │   └── TestConfiguration.cs
    └── RestaurantePro.Mobile.UITests.csproj
```

---

## ⚙️ **CONFIGURACIÓN XUNIT**

### **xunit.runner.json**
```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "diagnosticMessages": true,
  "longRunningTestSeconds": 300,
  "maxParallelThreads": 4,
  "methodDisplay": "method",
  "methodDisplayOptions": "replaceUnderscoreWithSpace",
  "preEnumerateTheories": false,
  "shadowCopy": false,
  "stopOnFail": false
}
```

### **TestBase.cs - Clase Base para XUnit**
```csharp
// Common/TestBase.cs
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly Mock<IApiService> MockApiService;
    protected readonly Mock<IAuthService> MockAuthService;
    protected readonly Mock<INavigationService> MockNavigationService;

    protected TestBase()
    {
        // Setup común para todas las pruebas
        MockApiService = new Mock<IApiService>();
        MockAuthService = new Mock<IAuthService>();
        MockNavigationService = new Mock<INavigationService>();

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Registrar mocks y servicios comunes
        services.AddSingleton(MockApiService.Object);
        services.AddSingleton(MockAuthService.Object);
        services.AddSingleton(MockNavigationService.Object);
    }

    protected T GetService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public virtual void Dispose()
    {
        (ServiceProvider as IDisposable)?.Dispose();
    }
}
```

---

## 🔧 **COMANDOS DE PRUEBAS**

### **Comandos PowerShell para Windows**
```powershell
# Ejecutar todas las pruebas
dotnet test

# Ejecutar solo pruebas unitarias
dotnet test tests/RestaurantePro.Mobile.UnitTests/

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar con filtro específico
dotnet test --filter "FullyQualifiedName~AuthViewModel"

# Ejecutar por categoría (usando traits)
dotnet test --filter "Category=Unit"

# Ejecutar en paralelo
dotnet test --parallel

# Generar reporte de cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory "./TestResults"
```

### **Filtros Comunes XUnit**
```csharp
// Usar traits para categorizar pruebas
[Fact]
[Trait("Category", "Unit")]
[Trait("Component", "Authentication")]
public async Task LoginCommand_WithValidCredentials_ShouldNavigateToDashboard()
{
    // Test implementation
}

// Usar Theory para pruebas parametrizadas
[Theory]
[InlineData("", "password", false)]
[InlineData("user@test.com", "", false)]
[InlineData("user@test.com", "password", true)]
public void LoginCommand_CanExecute_DependsOnInputs(string email, string password, bool expected)
{
    // Test implementation
}
```

---

## 📊 **CONFIGURACIÓN DE COBERTURA**

### **coverlet.runsettings**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>json,cobertura,lcov,teamcity,opencover</Format>
          <Exclude>[*.Tests]*,[*.UITests]*</Exclude>
          <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
          <ExcludeByFile>**/Migrations/**/*.*</ExcludeByFile>
          <IncludeDirectory>../src/</IncludeDirectory>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

---

## 🎯 **DIFERENCIAS CLAVE: XUNIT vs MSTest/NUnit**

### **Estructura de Clases**
```csharp
// ❌ MSTest/NUnit (obsoleto)
[TestFixture]  // NUnit
[TestClass]    // MSTest
public class MyTests
{
    [SetUp]     // NUnit
    [TestInitialize] // MSTest
    public void Setup() { }
    
    [Test]      // NUnit
    [TestMethod] // MSTest
    public void MyTest() { }
}

// ✅ XUnit (moderno)
public class MyTests : IDisposable
{
    public MyTests() 
    { 
        // Constructor = Setup
    }
    
    [Fact]
    public void MyTest() { }
    
    public void Dispose() 
    { 
        // Dispose = Teardown
    }
}
```

### **Assertions**
```csharp
// ❌ MSTest/NUnit
Assert.IsTrue(condition);
Assert.AreEqual(expected, actual);
Assert.IsNull(value);

// ✅ XUnit
Assert.True(condition);
Assert.Equal(expected, actual);
Assert.Null(value);
```

### **Pruebas Parametrizadas**
```csharp
// ❌ NUnit
[TestCase(1, 2, 3)]
[TestCase(2, 3, 5)]
public void AddTest(int a, int b, int expected) { }

// ✅ XUnit
[Theory]
[InlineData(1, 2, 3)]
[InlineData(2, 3, 5)]
public void AddTest(int a, int b, int expected) { }
```

---

## 🚀 **VENTAJAS DE XUNIT**

1. **Moderno**: Diseñado específicamente para .NET moderno
2. **Limpio**: No requiere atributos de clase ([TestFixture])
3. **Aislamiento**: Cada test tiene su propia instancia de clase
4. **Flexible**: Mejor soporte para dependency injection
5. **Performance**: Ejecución en paralelo por defecto
6. **Futuro**: Es el framework recomendado por Microsoft

---

*Esta configuración asegura que tengas un framework de pruebas moderno, eficiente y fácil de mantener para la aplicación móvil operativa.* 