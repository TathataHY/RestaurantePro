# 🚀 Tests de UI Móvil - Configuración Simple

## ✨ **¿Qué es esto?**

Tests de UI móvil usando **Appium** con **API en memoria** para tests rápidos y confiables.

## 🎯 **Características**

- ✅ **UNA SOLA** base de tests (sin confusión)
- ✅ **API en memoria** (rápida y confiable)
- ✅ **Appium** para tests de UI móvil
- ✅ **Screenshots automáticos** en caso de fallo
- ✅ **Configuración simple** y clara

## 🚀 **Cómo Usar**

### 1. **Preparar el Entorno**

```bash
# Instalar Appium Server
npm install -g appium

# Iniciar Appium Server
appium

# En otra terminal, ejecutar los tests
dotnet test --verbosity normal
```

### 2. **Configurar el Emulador**

- Asegúrate de que el emulador `RestaurantePro_Test` esté disponible
- O cambia el nombre en `appsettings.Test.json`

### 3. **Estructura de Tests**

```csharp
public class MiTest : AppiumTestBase
{
    public MiTest(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    [Fact]
    public void MiTest_Should_Work()
    {
        // Tu test aquí
        // Usa Driver para interactuar con la app
        // Usa TestOutput.WriteLine para logging
        // Screenshots automáticos en caso de fallo
    }
}
```

## 🔧 **Configuración**

### **appsettings.Test.json**
```json
{
  "AppiumConfig": {
    "ServerUrl": "http://localhost:4723",
    "PlatformName": "Android",
    "PlatformVersion": "14.0",
    "DeviceName": "emulator-5554"
  },
  "EmulatorConfig": {
    "Enabled": true,
    "AvdName": "RestaurantePro_Test"
  }
}
```

## 📱 **Métodos Helper Disponibles**

- `WaitForElement(By by, timeoutSeconds)` - Esperar elemento
- `IsElementVisible(By by, timeoutSeconds)` - Verificar visibilidad
- `TakeScreenshot(testName)` - Tomar screenshot manual
- `Driver` - Acceso al driver de Appium
- `TestOutput.WriteLine()` - Logging de test

## 🎮 **Ejecutar Tests**

```bash
# Todos los tests
dotnet test

# Test específico
dotnet test --filter "DisplayName~App_Should_Launch_Successfully"

# Con verbosidad
dotnet test --verbosity normal
```

## 🆘 **Solución de Problemas**

### **Appium no conecta**
- Verifica que Appium Server esté ejecutándose en `http://localhost:4723`
- Usa `appium` en terminal para iniciar

### **Emulador no disponible**
- Cambia `AvdName` en `appsettings.Test.json`
- O crea el emulador `RestaurantePro_Test`

### **App no instala**
- Verifica que el APK esté en la ruta correcta
- Compila la app móvil primero

## 🎉 **¡Listo!**

Ahora tienes una configuración **simple, clara y funcional** para tests de UI móvil.

**Sin confusión, sin opciones múltiples, solo una forma de hacer las cosas bien.** 🚀
