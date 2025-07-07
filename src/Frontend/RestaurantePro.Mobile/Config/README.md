# Configuración de Autenticación Básica para Hosting

## Descripción

Este archivo contiene las instrucciones para configurar la autenticación básica necesaria para conectar la aplicación móvil con el backend desplegado en un servicio de hosting que requiere protección por contraseña.

## Configuración Requerida

### 1. Actualizar URL del Backend

En el archivo `ApiConfig.cs`, actualiza la URL base del backend:

```csharp
public const string BaseUrl = "https://tu-servicio-de-hosting.com/";
```

Cambia `"https://tu-servicio-de-hosting.com/"` por la URL real de tu servicio de hosting.

### 2. Configurar Credenciales de Autenticación Básica

En el archivo `ApiConfig.cs`, actualiza las credenciales del hosting:

```csharp
public static class HostingCredentials
{
    public const string Username = "usuario";        // Cambiar por tu usuario real
    public const string Password = "contraseña";     // Cambiar por tu contraseña real
}
```

### 3. Ejemplo de Configuración Completa

```csharp
public static class ApiConfig
{
    // URL del backend (cambiar según tu hosting)
    public const string BaseUrl = "https://restaurantepro.azurewebsites.net/";
    
    // Credenciales para autenticación básica del hosting
    public static class HostingCredentials
    {
        public const string Username = "admin";
        public const string Password = "miContraseña123";
        
        /// <summary>
        /// Obtiene las credenciales codificadas en Base64 para autenticación básica
        /// </summary>
        public static string GetEncodedCredentials()
        {
            var credentials = $"{Username}:{Password}";
            var bytes = System.Text.Encoding.ASCII.GetBytes(credentials);
            return Convert.ToBase64String(bytes);
        }
    }
    
    /// <summary>
    /// Timeout para las peticiones HTTP
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
}
```

## Cómo Funciona

1. **Autenticación Básica**: El `HttpClient` configurado en `MauiProgram.cs` incluye automáticamente las credenciales de autenticación básica en cada petición HTTP.

2. **Codificación Base64**: Las credenciales se codifican en Base64 como requiere el estándar HTTP Basic Authentication.

3. **Headers Automáticos**: Cada petición incluye automáticamente el header:
   ```
   Authorization: Basic <credenciales_codificadas>
   ```

## Notas Importantes

- **Seguridad**: Las credenciales están hardcodeadas en el código. Para producción, considera usar un sistema de gestión de secretos más seguro.
- **Plan Gratuito**: Esta configuración es necesaria porque los planes gratuitos de hosting suelen incluir protección por contraseña que no se puede desactivar.
- **Plan de Pago**: Cuando actualices a un plan de pago, podrás desactivar la protección por contraseña y eliminar esta configuración.

## Pruebas

Después de configurar las credenciales:

1. Compila la aplicación: `dotnet build`
2. Ejecuta la aplicación en un emulador o dispositivo
3. Intenta hacer login o acceder a cualquier funcionalidad que requiera comunicación con el backend
4. Verifica en los logs que las peticiones HTTP se completen exitosamente (código 200)

## Solución de Problemas

### Error 401 Unauthorized
- Verifica que las credenciales sean correctas
- Asegúrate de que la URL del backend sea accesible

### Error de Conexión
- Verifica que la URL del backend sea correcta
- Comprueba que el servicio de hosting esté funcionando

### Timeout
- Aumenta el valor de `RequestTimeout` si es necesario
- Verifica la conectividad de red 